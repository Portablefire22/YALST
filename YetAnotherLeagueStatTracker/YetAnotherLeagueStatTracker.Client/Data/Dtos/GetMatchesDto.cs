namespace YetAnotherLeagueStatTracker.Client.Data.Dtos;

public class GetMatchesDto
{
    public string[] Puuids { get; set; }
    public int count { get; set; } = 10;
}