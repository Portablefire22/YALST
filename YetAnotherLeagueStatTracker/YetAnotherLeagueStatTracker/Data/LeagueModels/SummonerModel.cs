using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace YetAnotherLeagueStatTracker.Data.LeagueModels;

[PrimaryKey(nameof(Id))]
public class SummonerModel
{
    [Key]
    public int Id { get; set; }
    public string Puuid { get; set; }
    public long SummonerLevel { get; set; }
    public string GameName { get; set; }
    
    public ICollection<RankedModel>? RankedModels { get; set; }
    
    public string TagLine { get; set; }
    public string Region { get; set; }
    public int ProfileIconId { get; set; }
    public long RevisionDate { get; set; }
}