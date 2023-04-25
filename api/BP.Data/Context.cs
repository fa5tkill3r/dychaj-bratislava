using BP.Data.DbModels;
using BP.Data.DbModels.Modules;
using Microsoft.EntityFrameworkCore;

namespace BP.Data;

public class Context : DbContext
{
    public Context(DbContextOptions<Context> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // modelBuilder.Entity<Sensor>().HasData(
        //     new Sensor
        //     {
        //         Id = 1,
        //         ModuleId = 1,
        //         Name = "Temperature",
        //         Description = "Temperature sensor",
        //         Unit = "°C"
        //     }
        // );
    }

    public DbSet<Reading> Reading { get; set; }
    public DbSet<Location> Location { get; set; }

    public DbSet<Sensor> Sensor { get; set; }
    public DbSet<Module> Module { get; set; }
    public DbSet<Esp> Esp { get; set; }
    public DbSet<CykloKoalicia> CykloKoalicia { get; set; }
    public DbSet<SensorCommunity> SensorCommunities { get; set; }
}