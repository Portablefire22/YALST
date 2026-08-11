using System.Text.Json;
using YetAnotherLeagueStatTracker.Services.Dtos;
using YetAnotherLeagueStatTracker.Services.Riot;

namespace YetAnotherLeagueStatTracker.Services;

public class DataDragon
{
    public string Version { get; private set; }= "16.13.1";
    
    private DateTime _lastChecked = DateTime.UnixEpoch;

    private Dictionary<int, GameQueue> Queues = [];
    
    private ILogger Logger { get; }


    public static Dictionary<string, SummonerSpellDto> _summonerSpells = new Dictionary<string, SummonerSpellDto>();

    private static Dictionary<int, AugmentDto> _augments = [];
    
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
        if (string.IsNullOrEmpty(Version) || (DateTime.UtcNow - _lastChecked).TotalMinutes >= 15)
        {
            _lastChecked = DateTime.UtcNow;
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

            // Sometimes CommunityDragon can be a bit slow to update, so we just select the newest working
            var index = 0;
            Version ??= "";
            while (true)
            {
                if (index > versions?.Length || versions == null) break;
                var r = await x.GetAsync($"https://cdn.communitydragon.org/{versions[index]}/profile-icon/501");
                if (r.IsSuccessStatusCode)
                {
                    Version = versions[index];
                    break;
                }
                index++;
            }
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

        var augs = await x.GetFromJsonAsync<AugmentsDto>(
            $"https://raw.communitydragon.org/latest/cdragon/arena/en_us.json", new JsonSerializerOptions()
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            });
        if (augs == null) return;
        
        _augments.Clear();
        foreach (var aug in augs.Augments)
        {
            _augments.TryAdd(aug.Id, aug);
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

    public AugmentDto? GetAugment(int augmentId)
    {
        _augments.TryGetValue(augmentId, out var augment);
        return augment;
    }
}

public class GameQueue
{
    public int QueueId { get; set; }
    public string Map { get; set; }
    public string Description { get; set; }
    public string? Notes { get; set; }
}