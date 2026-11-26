using HoopsApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
namespace HoopsApi.Data;

public class AppDbContext : IdentityDbContext<AppUser>
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Team>()
            .HasMany(t => t.Players)
            .WithOne()
            .HasForeignKey(p => p.TeamId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Game>()
            .HasOne<Team>()
            .WithMany()
            .HasForeignKey(g => g.HomeTeamId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Game>()
            .HasOne<Team>()
            .WithMany()
            .HasForeignKey(g => g.AwayTeamId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) {}

    //Registering Databases
    public DbSet<Team> Teams => Set<Team>();

    public DbSet<Player> Players => Set<Player>();
    public DbSet<Game> Games => Set<Game>();
}
