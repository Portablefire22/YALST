namespace YetAnotherLeagueStatTracker.Services.Models;

public class LeagueEntryDto
{
    public string QueueType { get; set; }
    public string Tier { get; set; }
    public string Rank { get; set; }
    public string Puuid { get; set; }
    public int LeaguePoints { get; set; }
    public int Wins { get; set; }
    public int Losses { get; set; }
    public bool Veteran { get; set; }
    public bool Inactive { get; set; }
    public bool FreshBlood { get; set; }
    public bool HotStreak { get; set; }
    public MiniSeriesDto? MiniSeries { get; set; }
}

public class MiniSeriesDto
{
    public int Losses { get; set; }
    public required string Progress { get; set; }
    public int Target { get; set; }
    public int Wins { get; set; }
}