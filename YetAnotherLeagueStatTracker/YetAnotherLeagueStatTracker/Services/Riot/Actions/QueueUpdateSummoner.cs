using YetAnotherLeagueStatTracker.Data.LeagueModels;

namespace YetAnotherLeagueStatTracker.Services.Riot.Actions;

public class QueueUpdateSummoner : QueueSummoner
{
    public QueueUpdateSummoner(string? puuid, Action<SummonerModel?>? callback) : base(puuid, callback)
    {
    }

    public QueueUpdateSummoner(string? gameName, string? tagLine, string region, Action<SummonerModel?>? callback) : base(gameName, tagLine, region, callback)
    {
    }
}