using System.Globalization;
using System.Net;
using System.Text.Json;
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

   private async Task<AccountDto?> AccountDtoByRiotId(string gameName, string tagLine, string regionalRouting)
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
      var accountDto = await AccountDtoByRiotId(gameName, tagLine, RegionalRouting.Europe);
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
   
   public async Task<SummonerModel?> SummonerByRiotId(string gameName, string tagLine, string regionalRouting = PlatformRouting.EuW)
   {
      await using var db = await _scopeFactory.CreateDbContextAsync();
      var account = await db.Summoners.SingleOrDefaultAsync();
      // Get and save account to DB
      if (account != null) return account;
      Logger.LogInformation($"{gameName}#{tagLine} ({regionalRouting}) was not found, pulling from API");
      var model = await SummonerModelByRiotId(gameName, tagLine, regionalRouting);
      if (model == null) return null;
      
      // Saving summoner 

      if (await db.Summoners.SingleOrDefaultAsync() is { } summoner)
      {
         db.Entry(summoner).CurrentValues.SetValues(model);
      }
      else
      {
         await db.Summoners.AddAsync(model);
      }
      
      await db.SaveChangesAsync();
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