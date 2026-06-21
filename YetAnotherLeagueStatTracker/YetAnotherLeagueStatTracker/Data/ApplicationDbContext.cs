using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using YetAnotherLeagueStatTracker.Data.LeagueModels;

namespace YetAnotherLeagueStatTracker.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<ApplicationUser>(options)
{
    public virtual DbSet<SummonerModel> Summoners { get; set; } = default!;
    public virtual DbSet<RankedModel> SummonerRanks { get; set; } = default!;

    public virtual DbSet<MatchModel> Matches { get; set; } = default!;
    public virtual DbSet<MatchParticipant> MatchParticipants { get; set; } = default!;
    
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.Entity<RankedModel>().Property(e => e.Tier)
            .HasConversion(v => v.ToString(), v => (Tier)Enum.Parse(typeof(Tier), v));

        builder.Entity<SummonerModel>().Property(e => e.Id).ValueGeneratedOnAdd();
        builder.Entity<MatchModel>().Property(e => e.Id).ValueGeneratedOnAdd();
        builder.Entity<MatchParticipant>().Property(e => e.Id).ValueGeneratedOnAdd();
        builder.Entity<RankedModel>().Property(e => e.Id).ValueGeneratedOnAdd();
    }
}