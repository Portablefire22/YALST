namespace YetAnotherLeagueStatTracker.Services;

public class RateLimitArgs
{
    public RateLimitArgs(int waitTime)
    {
        WaitTime = waitTime;
    }

    public int WaitTime { get; private set; }
}