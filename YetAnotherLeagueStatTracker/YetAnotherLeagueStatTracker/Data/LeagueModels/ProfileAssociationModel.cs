using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace YetAnotherLeagueStatTracker.Data.LeagueModels;

[PrimaryKey(nameof(Id))]
public class ProfileAssociationModel
{
    [Key]
    public int Id { get; set; }
    public required ApplicationUser User { get; set; }
    public required SummonerModel Summoner { get; set; }
}