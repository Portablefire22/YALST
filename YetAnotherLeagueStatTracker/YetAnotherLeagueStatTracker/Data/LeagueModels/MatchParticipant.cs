using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace YetAnotherLeagueStatTracker.Data.LeagueModels;

[PrimaryKey(nameof(Id))]
public class MatchParticipant
{
    // Massively cutdown from the API since we don't use 99% of it
    
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    public required MatchModel Match { get; set; }
    public required SummonerModel Summoner { get; set; }
    public int TeamId { get; set; }
    
    public int Assists { get; set; }
    
    public int ChampionLevel { get; set; }
    public required string ChampionName { get; set; }
    public int ChampionId { get; set; }
    public int ChampionTransform { get; set; }
    public int DamageDealtToBuildings { get; set; }
    public int DamageDealtToObjectives { get; set; }
    public int DamageSelfMitigated { get; set; }
    public int Deaths { get; set; }
    public bool FirstBlood { get; set; }
    public bool FirstTowerKill { get; set; }
    public int GoldEarned { get; set; }
    public required string TeamPosition { get; set; }
    public int Item0 { get; set; }
    public int Item1 { get; set; }
    public int Item2 { get; set; }
    public int Item3 { get; set; }
    public int Item4 { get; set; }
    public int Item5 { get; set; }
    public int Item6 { get; set; }
    
    public int Kills { get; set; }
    public int LargestMultiKill { get; set; }
    public int MagicDamageDealtToChampions { get; set; }
    public int PhysicalDamageDealtToChampions { get; set; }
    public int TrueDamageDealtToChampions { get; set; }
    
    public int Placement { get; set; }
    public int SubteamPlacement { get; set; }
    public int PlayerAugment1 { get; set; }
    public int PlayerAugment2 { get; set; }
    public int PlayerAugment3 { get; set; }
    public int PlayerAugment4 { get; set; }
    public int PlayerSubteamId { get; set; }
    
    public int Summoner1Id { get; set; }
    public int Summoner2Id { get; set; }
    public int TotalDamageTaken { get; set; }
    public int VisionScore { get; set; }
    public bool Win { get; set; }
    
    public int MainRune { get; set; }
    public int SubRune { get; set; }
}