using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace YetAnotherLeagueStatTracker.Data.LeagueModels;

[PrimaryKey(nameof(Id))]
public class MatchModel
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; }
    
    public required string DataVersion { get; set; }
    public required string GameVersion { get; set; }
    public required string MatchId { get; set; }
    
    public int ParticipantCount { get; set; }
    
    public required string EndOfGameResult { get; set; }
    public long GameCreation { get; set; }
    public long GameDuration { get; set; }
    public long GameEndTimestamp { get; set; }
    public long GameId { get; set; }
    public required string GameMode { get; set; }
    public required string GameName { get; set; }
    public long GameStartTimestamp { get; set; }
    public required string GameType { get; set; }
    public int MapId { get; set; }
    public required string PlatformId { get; set; }
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