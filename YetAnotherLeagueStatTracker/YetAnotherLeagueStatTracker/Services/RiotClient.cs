using System.Globalization;
using System.Net;
using System.Text.Json;
using YetAnotherLeagueStatTracker.Services.Models;

namespace YetAnotherLeagueStatTracker.Services;

public class RiotClient
{
   private string ApiKey { get; set; }
   private ILogger Logger { get; set; }

   private bool IsLimited { get; set; } = false;
   
   private EventHandler<RateLimitArgs>? RateLimitEventHandler { get; set; }
   
   private HttpClient HttpClient { get; set; }

   private static string ApiUrl = "api.riotgames.com";

   private JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase};
   
   public RiotClient(string apiKey)
   {
      ApiKey = apiKey;
      
      var factory = LoggerFactory.Create(builder => 
         builder.AddConsole());
      Logger = factory.CreateLogger<RiotClient>();
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
   
   public async Task<AccountDto?> AccountByRiotId(string gameName, string tagLine, string regionalRouting = RegionalRouting.Europe)
   {
      var url = $"https://{regionalRouting}.{ApiUrl}/riot/account/v1/accounts/by-riot-id/{gameName}/{tagLine}";
      var x = await GetAsync(new Uri(url));
      if (x == null) return null;
      return await JsonSerializer.DeserializeAsync<AccountDto>(await x.Content.ReadAsStreamAsync(), _jsonSerializerOptions);
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