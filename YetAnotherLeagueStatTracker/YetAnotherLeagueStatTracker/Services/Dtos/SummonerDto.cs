namespace YetAnotherLeagueStatTracker.Services.Models;

public class SummonerDto
{
    public int ProfileIconId { get; set; }
    public long RevisionDate { get; set; }
    public string Puuid { get; set; }

    public SummonerDto(int profileIconId, long revisionDate, string puuid, long summonerLevel)
    {
        ProfileIconId = profileIconId;
        RevisionDate = revisionDate;
        Puuid = puuid;
        SummonerLevel = summonerLevel;
    }

    public long SummonerLevel { get; set; }
}