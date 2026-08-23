using System.Collections.Concurrent;
using System.Net;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using YetAnotherLeagueStatTracker.Data;
using YetAnotherLeagueStatTracker.Data.LeagueModels;
using YetAnotherLeagueStatTracker.Services.Dtos;
using YetAnotherLeagueStatTracker.Services.Dtos.MatchHistory;
using YetAnotherLeagueStatTracker.Services.Events;
using YetAnotherLeagueStatTracker.Services.Riot.Actions;
using YetAnotherLeagueStatTracker.Services.Routing;

namespace YetAnotherLeagueStatTracker.Services.Riot;

public class RiotClient : IRiotClient
{
   private string ApiKey { get; set; }
   private ILogger Logger { get; set; }

   private bool IsLimited { get; set; } = false;

   private readonly IDbContextFactory<ApplicationDbContext> _scopeFactory;
   
   private EventHandler<RateLimitArgs>? RateLimitEventHandler { get; set; }

   private HttpClient HttpClient { get; set; }

   private const string ApiUrl = "api.riotgames.com";
   
   private JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase};
   
   private ConcurrentQueue<IQueuedAction> QueuedActions { get; } = [];
   
   private Task QueueProcessing { get; set; }

   // TODO Implement incremental loading on match history
   
   public RiotClient(IDbContextFactory<ApplicationDbContext> factory)
   {
      ConfigurationBuilder configurationBuilder = new ConfigurationBuilder();
      IConfiguration configuration = configurationBuilder.AddUserSecrets<Program>().AddEnvironmentVariables().Build();
      ApiKey = configuration.GetValue<string>("RiotAPI")!;
      
      _scopeFactory = factory;
      
      var loggerFactory = LoggerFactory.Create(builder => 
         builder.AddConsole());
      Logger = loggerFactory.CreateLogger<RiotClient>();
      RateLimitEventHandler += OnRateLimit;

      var censored = String.Concat(ApiKey.TakeLast(8));
      Logger.LogInformation($"Loaded API key ending with: {censored}");
      
      HttpClient = new HttpClient()
      {
         DefaultRequestHeaders =
         {
            {"X-Riot-Token", ApiKey}
         }
      };
      QueueProcessing = new Task(async () =>
      {
         await ProcessQueue();
      });
      QueueProcessing.Start();
   }

   private async Task ProcessQueue()
   {
      while (true)
      {
         try
         {
            if (IsLimited) continue;
            if (!QueuedActions.TryDequeue(out var result))
            {
               
               continue;
            }
            Logger.LogInformation($"Processing: {result}");
            await ProcessAction(result);
            // Likely that it failed if we're limited at the end of it
            if (IsLimited)
            {
               QueuedActions.Enqueue(result);
            } 
         }
         catch (Exception e)
         {
            Logger.LogError($"{e}");
         }
         finally
         {
            await Task.Delay(10);
         }
      }
   }

   private async Task ProcessAction(IQueuedAction action)
   {
      switch (action)
      {
         case QueueSummoner queueSummoner:
            var tryDb = queueSummoner is not QueueUpdateSummoner;
            SummonerModel? summonerModel = null;
            if (queueSummoner.Puuid != null)
            {
               summonerModel = await SummonerModelByPuuid(queueSummoner.Puuid, null, tryDb);
            }
            else
            {
               if (queueSummoner is { TagLine: not null, GameName: not null, Region: not null})
                  summonerModel = await SummonerModelByRiotId(queueSummoner.GameName, queueSummoner.TagLine,
                     queueSummoner.Region);
            }
            queueSummoner.SummonerModel = summonerModel;
            break;
         case QueueUpdateMatchHistory queueUpdateMatchHistory:
            await UpdateMatchHistory(queueUpdateMatchHistory);
            break;
         case QueueMatch queueMatch:
            _ = await GetMatchById(queueMatch.MatchId, regionalRouting: queueMatch.RegionalRouting);
            break;
      }
      action.InvokeCallback();
   }

   private async Task UpdateMatchHistory(QueueUpdateMatchHistory queueUpdateMatchHistory)
   {
      var summoner = await SummonerModelByPuuid(queueUpdateMatchHistory.Puuid);
      if (summoner == null) return;

      var matchIds = await MatchIdsByPuuid(summoner.Puuid, platformRouting: summoner.Region, count: 100);
      await using (var db = await _scopeFactory.CreateDbContextAsync())
      {
         var existingIds = db.MatchParticipants.Where(x => x.Summoner == summoner)
            .Select(x => x.Match.MatchId);
         foreach (var id in matchIds)
         {
            if (!await existingIds.ContainsAsync(id))
            {
               QueueAction(new QueueMatch(id, RegionalRouting.FromRegion(summoner.Region)));
            }
         }
      }

      if (queueUpdateMatchHistory.Parent != null) QueuedActions.Enqueue(queueUpdateMatchHistory.Parent);
   }

   public bool InQueue(string puuid)
   {
      return QueuedActions.FirstOrDefault(x => x is QueueUpdateSummoner up && up.Puuid == puuid) != null;
   }
   
   public bool InQueue(string gameName, string tagLine)
   {
      return QueuedActions.FirstOrDefault(x => x is not QueueUpdateSummoner up || up.GameName?.ToLowerInvariant() != gameName.ToLowerInvariant()
                                                                               || up.TagLine?.ToLowerInvariant() != tagLine.ToLowerInvariant()) != null;
   }
   
   public void QueueAction(IQueuedAction action) {
      if (action is QueueUpdateSummoner { Puuid: not null } queueUpdateSummoner)
      {
         QueueAction(new QueueUpdateMatchHistory(queueUpdateSummoner.Puuid,queueUpdateSummoner));
         return;
      }
      QueuedActions.Enqueue(action);
   }
   
   private async Task<HttpResponseMessage?> GetAsync(Uri? requestUri)
   {
      if (IsLimited) return null;
      Logger.LogDebug($"Sent HTTP-GET {Uri.UriSchemeHttps}");
      var resp = await HttpClient.GetAsync(requestUri);
      if (resp.StatusCode == HttpStatusCode.TooManyRequests && resp.Headers.TryGetValues("Retry-After", out var values))
      {
         var enumerable = values as string[] ?? values.ToArray();
         if (int.TryParse(enumerable.First(), out int result))
         {
            RateLimitEventHandler?.Invoke(this, new RateLimitArgs(result));
         }
      }

      return resp;
   }

   private async Task<HttpResponseMessage?> GetAsync(string request) => await GetAsync(new Uri(request));

   private async Task<AccountDto?> AccountDtoByRiotId(string gameName, string tagLine, string regionalRouting = RegionalRouting.Europe)
   {
      var url = $"https://{regionalRouting}.{ApiUrl}/riot/account/v1/accounts/by-riot-id/{gameName}/{tagLine}";
      var x = await GetAsync(new Uri(url));
      if (x is not { IsSuccessStatusCode: true }) return null;
      return await JsonSerializer.DeserializeAsync<AccountDto>(await x.Content.ReadAsStreamAsync(), _jsonSerializerOptions);
   }
   
   /// <summary>
   /// Pulls an AccountDto from Riot's Api using the given PUUID
   /// </summary>
   /// <param name="puuid">PUUID to search</param>
   /// <param name="regionalRouting">Regionalrouting to use, use the closest region.</param>
   /// <returns>AccountDo of associated PUUID if found, null if not</returns>
   private async Task<AccountDto?> AccountDtoByPuuid(string puuid, string regionalRouting = RegionalRouting.Europe)
   {
      var url = $"https://{regionalRouting}.{ApiUrl}/riot/account/v1/accounts/by-puuid/{puuid}";
      var x = await GetAsync(new Uri(url));
      if (x is not { IsSuccessStatusCode: true }) return null;
      return await JsonSerializer.DeserializeAsync<AccountDto>(await x.Content.ReadAsStreamAsync(), _jsonSerializerOptions);
   }
   
   /// <summary>
   /// Pulls a SummonerDto from Riot's Api using the given account PUUID
   /// </summary>
   /// <param name="puuid">PUUID to search</param>
   /// <param name="platformRouting">Platform to search SummonerDto on, e.g. EUW1,NA1</param>
   /// <returns>SummonerDto of associated PUUID if found, null if not</returns>
   private async Task<SummonerDto?> SummonerDtoByPuuid(string puuid, string platformRouting)
   {
      var url = $"https://{platformRouting}.{ApiUrl}/lol/summoner/v4/summoners/by-puuid/{puuid}";
      var x = await GetAsync(new Uri(url));
      if (x is not { IsSuccessStatusCode: true }) return null;
      return await JsonSerializer.DeserializeAsync<SummonerDto>(await x.Content.ReadAsStreamAsync(), _jsonSerializerOptions);
   }

   /// <summary>
   /// Retrieves SummonerModel from the DB matching the PUUID, pulling from the Riot API if the summoner was not found
   /// in the DB. 
   /// </summary>
   /// <param name="puuid">PUUID of the summoner to find</param>
   /// <param name="platformRouting">PlatformRouting of account, e.g EUW1, NA1. If region is null, attempts to get
   /// region from Summoner's Region field</param>
   /// <param name="tryDb">Check DB before pulling API, false to update Summoner in DB</param>
   /// <returns>SummonerModel if found, null if not</returns>
   public async Task<SummonerModel?> SummonerModelByPuuid(string puuid, string? platformRouting = null, bool tryDb = true)
   {
      await using var db = await _scopeFactory.CreateDbContextAsync();
      // PUUIDs are globally unique, so we don't check for region
      var dbSummoner = await db.Summoners.SingleOrDefaultAsync(x => x.Puuid == puuid);
      if (tryDb && dbSummoner != null)
      {
         if (string.IsNullOrEmpty(dbSummoner.InternalName) || string.IsNullOrEmpty(dbSummoner.InternalTag))
         {
            db.Update(dbSummoner);
            dbSummoner.InternalName = dbSummoner.GameName.ToLowerInvariant();
            dbSummoner.InternalTag = dbSummoner.TagLine.ToLowerInvariant();
            await db.SaveChangesAsync();
         }
         return dbSummoner;
      }

      platformRouting ??= dbSummoner?.Region;
      if (platformRouting == null) return null;
      
      var accountDto = await  AccountDtoByPuuid(puuid);
      if (accountDto == null) return null;
      var summonerDto = await SummonerDtoByPuuid(puuid, platformRouting);
      if (summonerDto == null) return null;
      var apiSummoner = new SummonerModel()
      {
         GameName = accountDto.GameName,
         TagLine = accountDto.TagLine,
         InternalName = accountDto.GameName.ToLowerInvariant(),
         InternalTag = accountDto.TagLine.ToLowerInvariant(),
         Puuid = accountDto.Puuid,
         Region = platformRouting.ToLowerInvariant(),
         RevisionDate = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
         SummonerLevel = summonerDto.SummonerLevel,
         ProfileIconId = summonerDto.ProfileIconId,
      };
      if (dbSummoner != null)
      {
         apiSummoner.Id = dbSummoner.Id;
         db.Entry(dbSummoner).CurrentValues.SetValues(apiSummoner);
      }
      else
      {
         await db.Summoners.AddAsync(apiSummoner);
      }
      await db.SaveChangesAsync();
      return apiSummoner;
   }
  
   /// <summary>
   /// Retrieves Summoner from the given Gamename, Tagline, and Region, pulling from API if not found in local DB.
   /// </summary>
   /// <param name="gameName">Gamename to search</param>
   /// <param name="tagLine">Tagline to search</param>
   /// <param name="platformRouting">Region to search</param>
   /// <returns>SummonerModel matching parameters, or null if not found</returns>
   public async Task<SummonerModel?> SummonerModelByRiotId(string gameName, string tagLine, string platformRouting)
   {
      if (!PlatformRouting.IsValid(platformRouting.ToLowerInvariant())) return null;
      await using var db = await _scopeFactory.CreateDbContextAsync();

      var summoner = await db.Summoners.Include(x => x.RankedModels).SingleOrDefaultAsync(x => 
         x.InternalName == gameName.ToLowerInvariant() &&
         x.InternalTag == tagLine.ToLowerInvariant() 
         && x.Region == platformRouting.ToLowerInvariant());
      if (summoner != null) return summoner;

      var dto = await AccountDtoByRiotId(gameName, tagLine);
      if (dto == null) return null;
      return await SummonerModelByPuuid(dto.Puuid, platformRouting);
   }

   /// <summary>
   /// Updates a given summoner's rank by pulling the API. Auto-populates the RankModels field with the new values
   /// </summary>
   /// <param name="summoner">Summoner to update</param>
   /// <returns>Summoner's RankedModel for all applicable Queues</returns>
   private async Task<RankedModel[]> UpdateSummonerRank(SummonerModel summoner)
   {
      var resp = await GetAsync($"https://{summoner.Region}.{ApiUrl}/lol/league/v4/entries/by-puuid/{summoner.Puuid}");
      if (resp is not {IsSuccessStatusCode: true}) return [];

      var entries =
         await JsonSerializer.DeserializeAsync<LeagueEntryDto[]>(await resp.Content.ReadAsStreamAsync(),
            _jsonSerializerOptions);
      if (entries == null) return [];

      await using var db = await _scopeFactory.CreateDbContextAsync();
      var models = new List<RankedModel>();
      foreach (var entry in entries)
      {
         var model = new RankedModel()
         {
            Summoner = summoner,
            QueueType = entry.QueueType,
            Tier = Enum.Parse<Tier>(string.Concat(entry.Tier[0].ToString().ToUpper(), entry.Tier.ToLower().AsSpan(1))),
            Rank = entry.Rank,
            LeaguePoints = entry.LeaguePoints,
            Losses = entry.Losses,
            Wins = entry.Wins,
            Time = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
         };
         models.Add(model);
         db.Summoners.Attach(summoner);
      }
      await db.AddRangeAsync(models);
      await db.SaveChangesAsync();

      summoner.RankedModels = models;
      return [.. models];
   }
   
   private async Task<RankedModel[]?> UpdateSummonerRankByPuuid(string puuid)
   {
      await using var db = await _scopeFactory.CreateDbContextAsync();
      var summoner = await db.Summoners.SingleOrDefaultAsync(x => x.Puuid == puuid);
      return summoner == null? null : await UpdateSummonerRank(summoner);
   }

   public async Task<Dictionary<string, RankedModel[]>?> SummonerRankHistoryByPuuid(string puuid)
   {
      await using var db = await _scopeFactory.CreateDbContextAsync();
      var ranks = db.SummonerRanks.Where(x => x.Summoner.Puuid == puuid);
      if (!ranks.Any()) return null;

      var rankedByQueues = new Dictionary<string, RankedModel[]>();
      var tmp = new List<RankedModel>();
      foreach (var queue in QueueType.Queues)
      {
         tmp.AddRange(ranks.Where(x => x.QueueType == queue));
         rankedByQueues.Add(queue, tmp.ToArray());
         tmp.Clear();
      }
      return rankedByQueues.Count == 0 ? null : rankedByQueues;
   }

   /// <summary>
   /// Populates the given SummonerModel with its associated ranks
   /// </summary>
   /// <param name="summoner">Summoner to populate</param>
   private async Task PopulateSummonerRank(SummonerModel summoner)
   {
      await using var db = await _scopeFactory.CreateDbContextAsync();
      var ranked = await db.SummonerRanks.Where(x => x.Summoner == summoner).ToArrayAsync();
      if (ranked.Length == 0) ranked = await UpdateSummonerRank(summoner);
      
      if (ranked.Length != 0)
      {
         var ranks = new List<RankedModel>();
         foreach (var queue in QueueType.Queues)
         {
            var rank = ranked.Where(x => x.QueueType == queue).OrderBy(x=>x.Time).LastOrDefault();
            Logger.LogInformation(rank?.Tier.ToString());
            if (rank != null) ranks.Add(rank);
         }

         summoner.RankedModels = ranks;
      }
   }
   
   /// <summary>
   /// Updates a given Summoner from their PUUID, updating Rank and populating Match history.
   /// </summary>
   /// <param name="puuid">PUUID of summoner to update</param>
   /// <returns>Newly updated Summoner</returns>
   public async Task<SummonerModel?> UpdateSummonerByPuuid(string puuid)
   {
      var summonerModel = await SummonerModelByPuuid(puuid, tryDb: false);
      if (summonerModel == null) return null;
      
      await UpdateSummonerRank(summonerModel);
      
      var ids = await MatchIdsByPuuid(summonerModel.Puuid, platformRouting: summonerModel.Region, count: 20);
      if (ids == null) return summonerModel;
      foreach (var id in ids)
      {
         try
         {
            _ = await GetMatchById(id, regionalRouting: RegionalRouting.FromRegion(summonerModel.Region));
         }
         catch (Exception e)
         {
            Logger.LogError($"{e}");
         }
      }
      return summonerModel;
   }

   /// <summary>
   /// Checks if an account matching Gamename, Tagline, and Region exists in the DB or Api
   /// </summary>
   /// <param name="gameName">Gamename of summoner to search</param>
   /// <param name="tagLine">Tagline of summoner to search</param>
   /// <param name="platformRouting">Region summoner resides in</param>
   /// <returns>True if account exists, false if not</returns>
   public async Task<bool> AccountExists(string gameName, string tagLine, string platformRouting = PlatformRouting.EuW)
   {
      await using var db = await _scopeFactory.CreateDbContextAsync();

      var summoner = await db.Summoners.SingleOrDefaultAsync(x =>
         x.InternalName == gameName.ToLowerInvariant() && x.TagLine == tagLine && x.Region == platformRouting);
      if (summoner != null) return true;
      
      var account = await AccountDtoByRiotId(gameName, tagLine);
      return account != null;
   }
  
   /// <summary>
   /// Pulls MatchIds played by the associated PUUID from the API.
   /// </summary>
   /// <param name="puuid">Summoner PUUID to search</param>
   /// <param name="startTime">Epoch timestamp in seconds</param>
   /// <param name="endTime">Epoch timestamp in seconds</param>
   /// <param name="queue">Filters to only match ids of a specific queue id</param>
   /// <param name="type">Filter to only match ids of a specific type</param>
   /// <param name="start">Starting index</param>
   /// <param name="count">Number of IDs to return</param>
   /// <param name="platformRouting">Region to query</param>
   /// <returns>List of Match IDs found</returns>
   private async Task<string[]> MatchIdsByPuuid(string puuid, long startTime = 0, long endTime = 0, int queue = 0, 
      string? type = null, int start = 0, int count = 5, string platformRouting = PlatformRouting.EuW)
   {
      var url = $"https://{RegionalRouting.FromRegion(platformRouting)}.{ApiUrl}/lol/match/v5/matches/by-puuid/{puuid}/ids?start={start}&count={count}";

      if (queue > 0)
      {
         url += $"&queue={queue}";
      }

      if (!string.IsNullOrEmpty(type))
      {
         url += $"&type={type}";
      }

      if (startTime > 0)
      {
         url += $"&startTime={startTime}";
      }
      if (endTime> 0)
      {
         url += $"&endTime={endTime}";
      }
      var result = await GetAsync(url);
      if (result is not { IsSuccessStatusCode: true }) return null;

      string[]? ids = null;
      try
      {
         ids = await JsonSerializer.DeserializeAsync<string[]>(await result.Content.ReadAsStreamAsync());
      }
      catch
      {
         Logger.LogError($"Failed to deserialise match IDs");
      }
      return ids;
   }

   public async Task<MatchModel[]> GetMatches(IEnumerable<string> puuids, int count = 10)
   {
      await using var db = await _scopeFactory.CreateDbContextAsync();
      var matchModels = db.MatchParticipants.Where(x => puuids.Contains(x.Summoner.Puuid)).Select(x => x.Match);
      return await matchModels.OrderByDescending(x => x.GameCreation).Take(count).ToArrayAsync();
   }

   public async Task<MatchParticipant[]> GetMatchParticipants(string matchId)
   {
      await using var db = await _scopeFactory.CreateDbContextAsync();
      return await db.MatchParticipants.Where(x => x.Match.MatchId == matchId).ToArrayAsync();
   }
   

   private async Task<MatchDto?> GetMatchDto(string matchId, string regionalRouting)
   {
      var uri =  $"https://{regionalRouting}.{ApiUrl}/lol/match/v5/matches/{matchId}";
      var result = await GetAsync(uri);
      if (result is not { IsSuccessStatusCode: true }) return null;
      var matchDto = await JsonSerializer.DeserializeAsync<MatchDto>(await result.Content.ReadAsStreamAsync(), _jsonSerializerOptions);
      return matchDto;
   }
   
   public async Task UpdateMatchParticipants(MatchModel match, string regionalRouting)
   {
      await using var db = await _scopeFactory.CreateDbContextAsync();
      
      var matchDto = await GetMatchDto(match.MatchId, regionalRouting);
      if (matchDto == null) return;
      
      var currentParticipants = db.MatchParticipants.Where(x => x.Match == match);
      foreach (var participant in matchDto.Info.Participants)
      {
         if (await currentParticipants.SingleOrDefaultAsync(x => x.Summoner.Puuid == participant.Puuid) != null) continue; 
         
          int mainRune = 0, subRune = 0;

         foreach (var perk in participant.Perks.Styles)
         {
            switch (perk.Description)
            {
               case "primaryStyle":
                  mainRune = perk.Selections[0].Perk;
                  break;
               case "subStyle":
                  subRune = perk.Selections[0].Perk;
                  break;
            }
         }
         
         var summoner = await SummonerModelByPuuid(participant.Puuid, match.PlatformId.ToLowerInvariant());
         if (summoner == null) continue;
         var model = new MatchParticipant()
         {
            Assists =  participant.Assists,
            Kills =  participant.Kills,
            Deaths =  participant.Deaths,
            ChampionName =  participant.ChampionName,
            Match = match,
            Summoner = summoner,
            TeamPosition = participant.TeamPosition,
            ChampionId =  participant.ChampionId,
            ChampionLevel = participant.ChampLevel,
            ChampionTransform =  participant.ChampionTransform,
            DamageDealtToBuildings =  participant.DamageDealtToBuildings,
            DamageDealtToObjectives =  participant.DamageDealtToObjectives,
            DamageSelfMitigated =   participant.DamageSelfMitigated,
            FirstBlood = participant.FirstBloodKill,
            FirstTowerKill =  participant.FirstTowerKill,
            GoldEarned =   participant.GoldEarned,
            Item0 =    participant.Item0,
            Item1 =   participant.Item1,
            Item2 =   participant.Item2,
            Item3 =   participant.Item3,
            Item4 =   participant.Item4,
            Item5 =   participant.Item5,
            Item6 =   participant.Item6,
            LargestMultiKill =    participant.LargestMultiKill,
            MagicDamageDealtToChampions =    participant.MagicDamageDealtToChampions,
            MainRune = mainRune,
            SubRune = subRune,
            PhysicalDamageDealtToChampions =     participant.PhysicalDamageDealtToChampions,
            Placement =  participant.Placement,
            PlayerAugment1 =  participant.PlayerAugment1,
            PlayerAugment2 = participant.PlayerAugment2,
            PlayerAugment3 = participant.PlayerAugment3,
            PlayerAugment4 = participant.PlayerAugment4,
            PlayerSubteamId =   participant.PlayerSubteamId,
            SubteamPlacement =   participant.SubteamPlacement,
            Summoner1Id =    participant.Summoner1Id,
            Summoner2Id =    participant.Summoner2Id,
            TeamId =  participant.TeamId,
            TotalDamageTaken =   participant.TotalDamageTaken,
            TrueDamageDealtToChampions =    participant.TrueDamageDealtToChampions,
            VisionScore =       participant.VisionScore,
            Win =  participant.Win,
         };
         await db.MatchParticipants.AddAsync(model);
         db.Matches.Attach(match);
         db.Summoners.Attach(summoner);
         await db.SaveChangesAsync();
      }
      await db.SaveChangesAsync();
   }

   public async Task<MatchModel?> GetMatchById(string matchId, string regionalRouting = RegionalRouting.Europe)
   {
      await using var db = await _scopeFactory.CreateDbContextAsync();

      var match = await db.Matches.SingleOrDefaultAsync(x => x.MatchId == matchId);
      if (match != null)
      {
         // If this is an old version without participant count, migrate it
         if (match.ParticipantCount == 0)
         { 
            var tMatchDto = await GetMatchDto(matchId, regionalRouting);
            if (tMatchDto == null) return null;
            db.Update(match);
            match.ParticipantCount = tMatchDto.Info.Participants.Length;
            await db.SaveChangesAsync();
         }
        
         // Sometimes API rate-limiting causes us to miss a few summoners, this helps reduce that issue
         var part = db.MatchParticipants.Where(x => x.Match == match);
         if (part.Count() != match.ParticipantCount)
         {
            await UpdateMatchParticipants(match, regionalRouting);
            await db.SaveChangesAsync();
         }

         return match;
      }
     
      var matchDto =  await GetMatchDto(matchId, regionalRouting);
      if (matchDto == null) return null;

     
      match = new MatchModel()
      {
         DataVersion = matchDto.Metadata.DataVersion,
         MatchId =  matchDto.Metadata.MatchId,
         EndOfGameResult = matchDto.Info.EndOfGameResult,
         GameMode =  matchDto.Info.GameMode,
         GameName =  matchDto.Info.GameName,
         GameType =   matchDto.Info.GameType,
         ParticipantCount = matchDto.Info.Participants.Length,
         GameVersion =   matchDto.Info.GameVersion,
         PlatformId =   matchDto.Info.PlatformId,
         GameCreation =  matchDto.Info.GameCreation,
         GameDuration =   matchDto.Info.GameDuration,
         GameEndTimestamp =   matchDto.Info.GameEndTimestamp,
         GameId =   matchDto.Info.GameId,
         GameStartTimestamp =    matchDto.Info.GameStartTimestamp,
         MapId =    matchDto.Info.MapId,
         QueueId =     matchDto.Info.QueueId,
         TournamentCode =   matchDto.Info.TournamentCode,
      };
      
      foreach (var participant in matchDto.Info.Participants)
      {
         var tmp = await db.MatchParticipants.SingleOrDefaultAsync(x =>
            x.Match.Id == match.Id && x.Summoner.Puuid == participant.Puuid);
         if (tmp != null) continue;
         int mainRune = 0, subRune = 0;

         foreach (var perk in participant.Perks.Styles)
         {
            if (perk.Description == "primaryStyle")
            {
               mainRune = perk.Selections[0].Perk;
            }
            else if (perk.Description == "subStyle")
            {
               subRune = perk.Selections[0].Perk;
            }
         }
         
         var summoner = await SummonerModelByPuuid(participant.Puuid, match.PlatformId.ToLowerInvariant());
         if (summoner == null) continue;
         
         var model = new MatchParticipant()
         {
            Assists =  participant.Assists,
            Kills =  participant.Kills,
            Deaths =  participant.Deaths,
            ChampionName =  participant.ChampionName,
            Match = match,
            Summoner = summoner!,
            TeamPosition = participant.TeamPosition,
            ChampionId =  participant.ChampionId,
            ChampionLevel = participant.ChampLevel,
            ChampionTransform =  participant.ChampionTransform,
            DamageDealtToBuildings =  participant.DamageDealtToBuildings,
            DamageDealtToObjectives =  participant.DamageDealtToObjectives,
            DamageSelfMitigated =   participant.DamageSelfMitigated,
            FirstBlood = participant.FirstBloodKill,
            FirstTowerKill =  participant.FirstTowerKill,
            GoldEarned =   participant.GoldEarned,
            Item0 =    participant.Item0,
            Item1 =   participant.Item1,
            Item2 =   participant.Item2,
            Item3 =   participant.Item3,
            Item4 =   participant.Item4,
            Item5 =   participant.Item5,
            Item6 =   participant.Item6,
            LargestMultiKill =    participant.LargestMultiKill,
            MagicDamageDealtToChampions =    participant.MagicDamageDealtToChampions,
            MainRune = mainRune,
            SubRune = subRune,
            PhysicalDamageDealtToChampions =     participant.PhysicalDamageDealtToChampions,
            Placement =  participant.Placement,
            PlayerAugment1 =  participant.PlayerAugment1,
            PlayerAugment2 = participant.PlayerAugment2,
            PlayerAugment3 = participant.PlayerAugment3,
            PlayerAugment4 = participant.PlayerAugment4,
            PlayerSubteamId =   participant.PlayerSubteamId,
            SubteamPlacement =   participant.SubteamPlacement,
            Summoner1Id =    participant.Summoner1Id,
            Summoner2Id =    participant.Summoner2Id,
            TeamId =  participant.TeamId,
            TotalDamageTaken =   participant.TotalDamageTaken,
            TrueDamageDealtToChampions =    participant.TrueDamageDealtToChampions,
            VisionScore =       participant.VisionScore,
            Win =  participant.Win,
         };
         await db.MatchParticipants.AddAsync(model);
         db.Summoners.Attach(summoner!);
      }

      await db.Matches.AddAsync(match);
      await db.SaveChangesAsync();
      return match;
   }
   
   private async void OnRateLimit(object? sender, RateLimitArgs args)
   {
      if (IsLimited)
         return;
      Logger.LogInformation($"Rate limited for {args.WaitTime} seconds");
      IsLimited = true;
      Task.Run(async void () =>
      {
         try
         {
            await Task.Delay((int)args.WaitTime * 1000);
            IsLimited = false;
            Logger.LogInformation("Rate limited ended");
         }
         catch (Exception e)
         {
            // ignored
         }
      });
   }
   
}