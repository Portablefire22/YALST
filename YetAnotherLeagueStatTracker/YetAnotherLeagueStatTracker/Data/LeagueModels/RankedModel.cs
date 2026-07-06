using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace YetAnotherLeagueStatTracker.Data.LeagueModels;

[PrimaryKey(nameof(Id))]
public class RankedModel
{
    [Key]
    public int Id { get; set; }
    public long Time { get; set; }
    public required string QueueType { get; set; }
    public SummonerModel Summoner { get; set; }
    public Tier Tier { get; set; }
    public string? Rank { get; set; } 
    public int LeaguePoints { get; set; }
    public int Wins { get; set; }
    public int Losses { get; set; }

    public static RankedModel? HighestRank(RankedModel? rank1, RankedModel? rank2)
    {
        if (rank1 == null && rank2 != null) return rank2;
        if (rank2 == null && rank1 != null) return rank1;
        if (rank1 == null && rank2 == null) return null;
        
        var rankTier = RankToInt(rank1.Rank);
        var rank2Tier = RankToInt(rank2.Rank);

        var rank1Total = ((int)rank1.Tier * 400) + ((rankTier - 1) * 100) + rank1.LeaguePoints;
        var rank2Total = ((int)rank2.Tier * 400) + ((rank2Tier - 1) * 100) + rank2.LeaguePoints;

        return rank1Total >= rank2Total ? rank1 : rank2;
    }

    private static int RankToInt(string? rank)
    {
        return rank switch
        {
            "I" => 4,
            "II" => 3,
            "III" => 2,
            "IV" => 1,
            _ => 0
        };
    }
}

public class QueueType
{
    public const string RankedSolo = "RANKED_SOLO_5x5";
    public const string RankedFlex = "RANKED_FLEX_SR";

    public static readonly string[] Queues = new[] { RankedSolo, RankedFlex };
}

public enum Rank
{
    I = 1,
    II,
    III,
    IV,
}

public enum Tier
{
    Iron,
    Bronze,
    Silver,
    Gold,
    Platinum,
    Emerald,
    Diamond,
    Master,
    Grandmaster,
    Challenger
}