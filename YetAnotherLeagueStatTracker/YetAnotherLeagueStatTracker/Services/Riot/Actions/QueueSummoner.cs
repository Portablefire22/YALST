using YetAnotherLeagueStatTracker.Data.LeagueModels;

namespace YetAnotherLeagueStatTracker.Services.Riot.Actions;

public class QueueSummoner : IQueuedAction
{
    public string? Puuid { get; private set; } = null;
    public string? GameName { get; private set; } = null;
    public string? TagLine { get; private set; } = null;
    public string? Region { get; private set; }

    public Action<SummonerModel?>? Callback { get; private set; }

    public QueueSummoner(string? puuid, Action<SummonerModel?>? callback)
    {
        Puuid = puuid;
        Callback = callback;
    }

    public QueueSummoner(string? gameName, string? tagLine, string region, Action<SummonerModel?>? callback)
    {
        GameName = gameName;
        TagLine = tagLine;
        Region = region;
        Callback = callback;
    }

    public SummonerModel? SummonerModel { get; set; }
    
    public IQueuedAction? Parent { get; set; }
    
    public void InvokeCallback()
    {
        Callback?.Invoke(SummonerModel);
        Parent?.InvokeCallback();
        
    }
}