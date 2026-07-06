using System.Globalization;
using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Components.WebAssembly.Server;
using Microsoft.EntityFrameworkCore;
using YetAnotherLeagueStatTracker.Data;
using YetAnotherLeagueStatTracker.Data.LeagueModels;
using YetAnotherLeagueStatTracker.Services.Models;
using YetAnotherLeagueStatTracker.Services.Models.MatchHistory;

namespace YetAnotherLeagueStatTracker.Services;

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
   
   public RiotClient(IDbContextFactory<ApplicationDbContext> factory)
   {
      ConfigurationBuilder configurationBuilder = new ConfigurationBuilder();
      IConfiguration configuration = configurationBuilder.AddUserSecrets<Program>().Build();
      ApiKey = configuration.GetValue<string>("RiotAPI")!;
      
      _scopeFactory = factory;
      
      var loggerFactory = LoggerFactory.Create(builder => 
         builder.AddConsole());
      Logger = loggerFactory.CreateLogger<RiotClient>();
      RateLimitEventHandler += OnRateLimit;

      HttpClient = new HttpClient()
      {
         DefaultRequestHeaders =
         {
            {"X-Riot-Token", ApiKey}
         }
      };
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

   private async Task<SummonerDto?> SummonerDtoByPuuid(string puuid, string regionalRouting)
   {
      var url = $"https://{regionalRouting}.{ApiUrl}/lol/summoner/v4/summoners/by-puuid/{puuid}";
      var x = await GetAsync(new Uri(url));
      if (x is not { IsSuccessStatusCode: true }) return null;
      return await JsonSerializer.DeserializeAsync<SummonerDto>(await x.Content.ReadAsStreamAsync(), _jsonSerializerOptions);
   }

   private async Task<SummonerModel?> SummonerModelByPuuid(string puuid, string platformRouting)
   {
      var accountDto = await  AccountDtoByPuuid(puuid);
      if (accountDto == null) return null;
      var summonerDto = await SummonerDtoByPuuid(puuid, platformRouting);
      if (summonerDto == null) return null;
      var summonerModel = new SummonerModel()
      {
         GameName = accountDto.GameName,
         TagLine = accountDto.TagLine,
         Puuid = accountDto.Puuid,
         Region = platformRouting.ToLowerInvariant(),
         RevisionDate = summonerDto.RevisionDate,
         SummonerLevel = summonerDto.SummonerLevel,
         ProfileIconId = summonerDto.ProfileIconId,
      };
      return summonerModel;  
   }
   
   private async Task<SummonerModel?> SummonerModelByRiotId(string gameName, string tagLine, string platformRouting, bool skip = false)
   {
      var accountDto = await AccountDtoByRiotId(gameName, tagLine);
      if (accountDto == null) return null;
      var summonerDto = await SummonerDtoByPuuid(accountDto.Puuid, platformRouting);
      if (summonerDto == null) return null;

      var summonerModel = new SummonerModel()
      {
         GameName = accountDto.GameName,
         TagLine = accountDto.TagLine,
         Puuid = accountDto.Puuid,
         Region = platformRouting,
         RevisionDate = summonerDto.RevisionDate,
         SummonerLevel = summonerDto.SummonerLevel,
         ProfileIconId = summonerDto.ProfileIconId,
      };
      return summonerModel;
   }
   
   private async Task<AccountDto?> AccountDtoByPuuid(string puuid, string regionalRouting = RegionalRouting.Europe)
   {
      var url = $"https://{regionalRouting}.{ApiUrl}/riot/account/v1/accounts/by-puuid/{puuid}";
      var x = await GetAsync(new Uri(url));
      if (x is not { IsSuccessStatusCode: true }) return null;
      return await JsonSerializer.DeserializeAsync<AccountDto>(await x.Content.ReadAsStreamAsync(), _jsonSerializerOptions);
   }

   private async Task<RankedModel[]?> UpdateSummonerRankByPuuid(string puuid)
   {
      await using var db = await _scopeFactory.CreateDbContextAsync();
      var summoner = await db.Summoners.SingleOrDefaultAsync(x => x.Puuid == puuid);
      if (summoner == null) return null;

      var resp = await GetAsync($"https://{summoner.Region}.{ApiUrl}/lol/league/v4/entries/by-puuid/{puuid}");
      if (resp is not {IsSuccessStatusCode: true}) return null;

      var entries =
         await JsonSerializer.DeserializeAsync<LeagueEntryDto[]>(await resp.Content.ReadAsStreamAsync(),
            _jsonSerializerOptions);
      if (entries == null) return null;

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
         await db.AddAsync(model);
      }

      db.Update(summoner);
      summoner.RankedModels = models;
      await db.SaveChangesAsync();
      return models.ToArray();
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
   
   public async Task<SummonerModel?> UpdateSummoner(string puuid)
   {
      await using var db = await _scopeFactory.CreateDbContextAsync();
      if (await db.Summoners.SingleOrDefaultAsync(x => x.Puuid == puuid) is not {} summoner) return null;
      var accountDto = await AccountDtoByPuuid(summoner.Puuid);
      if (accountDto == null) return null;
      // We're going to expect that they stayed on the same region unless someone searches for it later on
      var summonerDto = await SummonerDtoByPuuid(puuid, summoner.Region);
      if (summonerDto == null) return null;
      
      var summonerModel = new SummonerModel()
      {
         Id = summoner.Id,
         GameName = accountDto.GameName,
         TagLine = accountDto.TagLine,
         Puuid = summoner.Puuid,
         Region = summoner.Region,
         RevisionDate = summonerDto.RevisionDate,
         SummonerLevel = summonerDto.SummonerLevel,
         ProfileIconId = summonerDto.ProfileIconId,
      };
      db.Entry(summoner).CurrentValues.SetValues(summonerModel);
      await db.SaveChangesAsync();

      await UpdateSummonerRankByPuuid(summonerModel.Puuid);
      
      var ids = await MatchIdsByPuuid(summonerModel.Puuid, platformRouting: summonerModel.Region ,count: 20);
      if (ids == null) return summonerModel;
      foreach (var id in ids)
      {
         try
         {
            var x = await GetMatchById(id, regionalRouting: RegionalRouting.FromRegion(summonerModel.Region));
         }
         catch (Exception e)
         {
            Logger.LogError($"{e}");
         }
      }


      return summonerModel;
   }

   public async Task<SummonerModel?> SummonerByPuuid(string puuid, string platformRouting = PlatformRouting.EuW, ApplicationDbContext? context = null)
   {
      bool dispose = false;
      if (context == null)
      {
         dispose = true;
         context = await _scopeFactory.CreateDbContextAsync();
      }
      
      var account = await context.Summoners.SingleOrDefaultAsync(x => x.Puuid == puuid);
      
      // Get and save account to DB
      if (account != null)
      {
         var ranked = context.SummonerRanks.Where(x => x.Summoner == account);
         
         if (ranked.Any())
         {
            var ranks = new List<RankedModel>();
            foreach (var queue in QueueType.Queues)
            {
               var rank = await ranked.Where(x => x.QueueType == queue).OrderBy(x=>x.Time).LastOrDefaultAsync();
               Logger.LogInformation(rank?.Tier.ToString());
               if (rank != null) ranks.Add(rank);
            }

            account.RankedModels = ranks.Count > 0 ? ranks : null;
         }

         return account;
      }
      Logger.LogInformation($"{puuid} ({platformRouting}) was not found, pulling from API");
      var model = await SummonerModelByPuuid(puuid, platformRouting);
      if (model == null) return null;
      
      // Saving summoner 
      // Summoners can change names or tags, so we want to check if the Puuid exists already, then update the original
      if (await context.Summoners.SingleOrDefaultAsync(x => x.Puuid == model.Puuid) is { } summoner)
      {
         model.Id = summoner.Id;
         context.Entry(summoner).CurrentValues.SetValues(model);
      }
      else
      {
         await context.Summoners.AddAsync(model);
      }
      
      await context.SaveChangesAsync();
      await UpdateSummonerRankByPuuid(model.Puuid);
      
      if (dispose) await context.DisposeAsync();
      return model;
   }
   
   public async Task<SummonerModel?> SummonerByRiotId(string gameName, string tagLine, string platformRouting = PlatformRouting.EuW)
   {
      if (!PlatformRouting.IsValid(platformRouting.ToLowerInvariant())) return null;
      await using var db = await _scopeFactory.CreateDbContextAsync();
      // We must hit the API to get PUUIDs because Unicode characters can fuck everything up, e.g. "Αrt The Clοwn-EUW"
      // Which uses non-ascii characters
      var dto = await AccountDtoByRiotId(gameName, tagLine);
      if (dto == null) return null;
      return await SummonerByPuuid(dto.Puuid, platformRouting);
   }

   private async Task<string[]?> MatchIdsByPuuid(string puuid, long startTime = 0, long endTime = 0, int queue = 0, 
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
      return ids is { Length: > 0 } ? ids : null;
   }

   private async Task UpdateMatchParticipants(MatchModel match, string regionalRouting, ApplicationDbContext? db = null)
   {
      bool dispose = false;
      if (db == null)
      {
         dispose = true;
         db = await _scopeFactory.CreateDbContextAsync();
      } 
      
      var uri =  $"https://{regionalRouting}.{ApiUrl}/lol/match/v5/matches/{match.MatchId}";
      var result = await GetAsync(uri);
      if (result is not { IsSuccessStatusCode: true }) return;
      var matchDto = await JsonSerializer.DeserializeAsync<MatchDto>(await result.Content.ReadAsStreamAsync(), _jsonSerializerOptions);
      if (matchDto == null) return;

      var currentParticipants = db.MatchParticipants.Where(x => x.Match == match);
      foreach (var participant in matchDto.Info.Participants)
      {
         if (await currentParticipants.SingleOrDefaultAsync(x => x.Summoner.Puuid == participant.Puuid) != null) continue; 
         
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
         
         var summoner = await SummonerByPuuid(participant.Puuid, match.PlatformId.ToLowerInvariant(), db);
         
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
      }
      await db.SaveChangesAsync();

      if (dispose)
      {
         await db.DisposeAsync();
      }
   }
   
   private async Task<MatchModel?> GetMatchById(string matchId, string regionalRouting = RegionalRouting.Europe, ApplicationDbContext? db = null)
   {
      bool dispose = false;
      if (db == null)
      {
         dispose = true;
         db = await _scopeFactory.CreateDbContextAsync();
      } 

      var match = await db.Matches.SingleOrDefaultAsync(x => x.MatchId == matchId);
      if (match != null)
      {
         // If this is an old version without participant count, migrate it
         if (match.ParticipantCount == 0)
         { 
            var tUri =  $"https://{regionalRouting}.{ApiUrl}/lol/match/v5/matches/{matchId}";
            var tResult = await GetAsync(tUri);
            if (tResult is not { IsSuccessStatusCode: true }) return null;
            var tMatchDto = await JsonSerializer.DeserializeAsync<MatchDto>(await tResult.Content.ReadAsStreamAsync(), _jsonSerializerOptions);
            if (tMatchDto == null) return null;
            db.Update(match);
            match.ParticipantCount = tMatchDto.Info.Participants.Length;
            await db.SaveChangesAsync();
         }
        
         // Sometimes API rate-limiting causes us to miss a few summoners, this helps reduce that issue
         var part = db.MatchParticipants.Where(x => x.Match == match);
         if (part.Count() < match.ParticipantCount)
         {
            await UpdateMatchParticipants(match, regionalRouting, db);
         }
         return match;
      }
      
      var uri =  $"https://{regionalRouting}.{ApiUrl}/lol/match/v5/matches/{matchId}";
      var result = await GetAsync(uri);
      if (result is not { IsSuccessStatusCode: true }) return null;
      var matchDto = await JsonSerializer.DeserializeAsync<MatchDto>(await result.Content.ReadAsStreamAsync(), _jsonSerializerOptions);
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
         
         var summoner = await SummonerByPuuid(participant.Puuid, match.PlatformId.ToLowerInvariant(), db);
         
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
      }

      await db.Matches.AddAsync(match);
      await db.SaveChangesAsync();
      if (dispose) await db.DisposeAsync();
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