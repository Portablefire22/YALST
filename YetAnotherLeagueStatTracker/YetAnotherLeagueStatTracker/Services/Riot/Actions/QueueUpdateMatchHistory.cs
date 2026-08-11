using YetAnotherLeagueStatTracker.Data.LeagueModels;

namespace YetAnotherLeagueStatTracker.Services.Riot.Actions;

public class QueueUpdateMatchHistory : IQueuedAction
{
   public QueueUpdateMatchHistory(string puuid)
   {
      Puuid = puuid;
   }

   public QueueUpdateMatchHistory(string puuid, IQueuedAction parent)
   {
      Puuid = puuid;
      Parent = parent;
   }

   public string Puuid { get; private set; }
   public Action<SummonerModel?>? Callback { get; set; }
   public IQueuedAction? Parent { get; set; }

   public void InvokeCallback()
   {
      Parent?.InvokeCallback();
   }
}