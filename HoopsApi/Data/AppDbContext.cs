using HoopsApi.Models;
using Microsoft.EntityFrameworkCore;
namespace HoopsApi.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) {}

    //Registering Databases
    public DbSet<Team> Teams => Set<Team>();

    public DbSet<Player> Players => Set<Player>();
    public DbSet<Game> Games => Set<Game>();
}
