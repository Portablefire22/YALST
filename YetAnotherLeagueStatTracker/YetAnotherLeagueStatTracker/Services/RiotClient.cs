using System.Globalization;
using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Components.WebAssembly.Server;
using Microsoft.EntityFrameworkCore;
using YetAnotherLeagueStatTracker.Data;
using YetAnotherLeagueStatTracker.Data.LeagueModels;
using YetAnotherLeagueStatTracker.Services.Models;

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
      
      return summonerModel;
   }
   
   
   public async Task<SummonerModel?> SummonerByRiotId(string gameName, string tagLine, string regionalRouting = PlatformRouting.EuW)
   {
      if (!PlatformRouting.IsValid(regionalRouting)) return null;
      await using var db = await _scopeFactory.CreateDbContextAsync();
      // We must hit the API to get PUUIDs because unicode characters can fuck everything up, e.g. "Αrt The Clοwn-EUW"
      // Which uses non-ascii characters
      var dto = await AccountDtoByRiotId(gameName, tagLine);
      if (dto == null) return null;
      var account = await db.Summoners.SingleOrDefaultAsync(x => x.Puuid == dto.Puuid);
      
      // Get and save account to DB
      if (account != null)
      {
         var ranked = db.SummonerRanks.Where(x => x.Summoner == account);
         
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
      Logger.LogInformation($"{gameName}#{tagLine} ({regionalRouting}) was not found, pulling from API");
      var model = await SummonerModelByRiotId(gameName, tagLine, regionalRouting);
      if (model == null) return null;
      
      // Saving summoner 
      // Summoners can change names or tags, so we want to check if the Puuid exists already, then update the original
      if (await db.Summoners.SingleOrDefaultAsync(x => x.Puuid == model.Puuid) is { } summoner)
      {
         model.Id = summoner.Id;
         db.Entry(summoner).CurrentValues.SetValues(model);
      }
      else
      {
         await db.Summoners.AddAsync(model);
      }
      
      await db.SaveChangesAsync();
      await UpdateSummonerRankByPuuid(model.Puuid);
      return model;
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