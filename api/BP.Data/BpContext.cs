using BP.Data.DbHelpers;
using BP.Data.DbModels;
using Microsoft.EntityFrameworkCore;

namespace BP.Data;

public class BpContext : DbContext
{
    public BpContext(DbContextOptions<BpContext> options) : base(options)
    {
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
    }

    public DbSet<Reading> Reading { get; set; }
    public DbSet<Location> Location { get; set; }

    public DbSet<Sensor> Sensor { get; set; }
    public DbSet<Module> Module { get; set; }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        base.ConfigureConventions(configurationBuilder);
        
        configurationBuilder
            .Properties<DateTime>()
            .HaveConversion(typeof(UtcValueConverter));
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
}