using System.Text.Json;
using YetAnotherLeagueStatTracker.Services.Models;

namespace YetAnotherLeagueStatTracker.Services;

public class DataDragon
{
    public string Version { get; private set; }= "";
    
    private DateTime LastChecked = DateTime.UtcNow;

    private Dictionary<int, GameQueue> Queues = [];
    
    private ILogger Logger { get; }


    public static Dictionary<string, SummonerSpellDto> _summonerSpells = new Dictionary<string, SummonerSpellDto>();
    
    private static Dictionary<string, string> _queueTranslation = new Dictionary<string, string>()
    {
        {"5V5 RANKED FLEX GAMES", "Ranked Flex" },
        {"5V5 RANKED SOLO GAMES", "Ranked Solo"},
        {"5V5 DRAFT PICK GAMES", "Normal Draft"}
    };

    private static Dictionary<int, string> _cherryTeams = new Dictionary<int, string>()
    {
        {1, "Poros" },
        {2, "Minions"},
        {3, "Scuttles"},
        {4, "Krugs"},
        {5, "Raptor"},
        {6, "Sentinel"}
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

        var spells = await x.GetFromJsonAsync<SummonerSpellsDto>(
            $"https://ddragon.leagueoflegends.com/cdn/{Version}/data/en_US/summoner.json",
            new JsonSerializerOptions()
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            });
        if (spells == null) return;
        
        _summonerSpells.Clear();
        foreach (var spell in spells.Data)
        {
            _summonerSpells.TryAdd(spell.Value.Key, spell.Value);
        }

    }

    public string? GetSubTeamName(int subteamId)
    {
        _cherryTeams.TryGetValue(subteamId, out var teamName);
        return teamName;
    }
    
    public GameQueue? GetQueue(int queueId)
    {
        Queues.TryGetValue(queueId, out var queue); 
        return queue;
    }

    public string? GetQueueDescription(int queueId)
    {
        var queue = GetQueue(queueId);
        if (queue == null) return null;
        return _queueTranslation.TryGetValue(queue.Description.ToUpperInvariant(), out string? description) ? description : queue.Description;
    }

    public string? GetSummonerSpellFromId(string id)
    {
        _summonerSpells.TryGetValue(id, out var spell);
        return spell?.GetImageUrl(Version);
    }
}

public class GameQueue
{
    public int QueueId { get; set; }
    public string Map { get; set; }
    public string Description { get; set; }
    public string? Notes { get; set; }
}