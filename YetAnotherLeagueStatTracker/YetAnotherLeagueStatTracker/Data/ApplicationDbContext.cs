using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using YetAnotherLeagueStatTracker.Data.LeagueModels;

namespace YetAnotherLeagueStatTracker.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<ApplicationUser>(options)
{
    public virtual DbSet<SummonerModel> Summoners { get; set; } = default!;
}