namespace YetAnotherLeagueStatTracker.Services.Riot.Actions;

public interface IQueuedAction
{
    public IQueuedAction? Parent { get; set; }

    public void InvokeCallback();
}