namespace YetAnotherLeagueStatTracker.Services.Riot.Actions;

public class QueueMatch : IQueuedAction
{
    public QueueMatch(string matchId, string regionalRouting)
    {
        MatchId = matchId;
        RegionalRouting = regionalRouting;
    }

    public string MatchId { get; private set; }
    public string RegionalRouting { get; private set; }
    public IQueuedAction? Parent { get; set; }

    public void InvokeCallback()
    {
        Parent?.InvokeCallback();
    }
}