using Microsoft.EntityFrameworkCore;

namespace YetAnotherLeagueStatTracker.Data.LeagueModels;

[PrimaryKey(nameof(Id))]
public class MatchModel
{
    public int Id { get; set; }
    
    public string DataVersion { get; set; }
    public string GameVersion { get; set; }
    public string MatchId { get; set; }
    
    public string EndOfGameResult { get; set; }
    public long GameCreation { get; set; }
    public long GameDuration { get; set; }
    public long GameEndTimestamp { get; set; }
    public long GameId { get; set; }
    public string GameMode { get; set; }
    public string GameName { get; set; }
    public long GameStartTimestamp { get; set; }
    public string GameType { get; set; }
    public int MapId { get; set; }
    public string PlatformId { get; set; }
    public int QueueId { get; set; }
    public string? TournamentCode { get; set; }
}


[PrimaryKey(nameof(Id))]
public class MatchTeam
{
    public int Id { get; set; }
    public int TeamId { get; set; }
    public bool Won { get; set; }
    
}