namespace YetAnotherLeagueStatTracker.Services;

public class DataDragon
{
    public string Version { get; private set; }= "";
    
    private DateTime LastChecked = DateTime.UtcNow;

    private ILogger Logger { get; }
    
    public DataDragon()
    {
        var loggerFactory = LoggerFactory.Create(builder => 
            builder.AddConsole());
        Logger = loggerFactory.CreateLogger<RiotClient>();
    }

    public async Task<string> GetVersion()
    {
        if (string.IsNullOrEmpty(Version) || (DateTime.UtcNow - LastChecked).TotalMinutes >= 15)
        {
            await UpdateVersion();
        }
        return Version;
    }
    
    private async Task UpdateVersion()
    {
        var x = new HttpClient();
        try
        {
            var versions = await x.GetFromJsonAsync<string[]>("https://ddragon.leagueoflegends.com/api/versions.json");
            if (versions != null) Version = versions.FirstOrDefault() ?? "";
        }
        catch (Exception e)
        {
            Logger.LogError($"Could not get current DD version: {e}");
        }
    } 
}