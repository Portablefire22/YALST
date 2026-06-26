using System.Text.Json;

namespace YetAnotherLeagueStatTracker.Services;

public class DataDragon
{
    public string Version { get; private set; }= "";
    
    private DateTime LastChecked = DateTime.UtcNow;

    private Dictionary<int, GameQueue> Queues = [];
    
    private ILogger Logger { get; }
    
    
    private static Dictionary<string, string> _queueTranslation = new Dictionary<string, string>()
    {
        {"5v5 RANKED FLEX GAMES", "Ranked Flex" },
        {"5V5 RANKED SOLO GAMES", "Ranked Solo"}
    };
    
    
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

        var queus = await x.GetFromJsonAsync<GameQueue[]>("https://static.developer.riotgames.com/docs/lol/queues.json", new JsonSerializerOptions()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        });
        if (queus == null) return;
        Queues.Clear();
        
        // For some reason they never updated the docs for this
        Queues.Add(1750, new GameQueue()
        {
            QueueId = 1750,
            Map = "Rings of Wrath",
            Description = "Arena",
            Notes = "16 player lobby"
        });
        foreach (var queue in queus)
        {
            Queues.TryAdd(queue.QueueId, queue);
        }
    }

    public GameQueue? GetQueue(int queueId)
    {
        Queues.TryGetValue(queueId, out GameQueue? queue); 
        return queue;
    }

    public string? GetQueueDescription(int queueId)
    {
        var queue = GetQueue(queueId);
        if (queue == null) return null;
        return _queueTranslation.TryGetValue(queue.Description.ToUpperInvariant(), out string? description) ? description : queue.Description;
    }
}

public class GameQueue
{
    public int QueueId { get; set; }
    public string Map { get; set; }
    public string Description { get; set; }
    public string? Notes { get; set; }
}