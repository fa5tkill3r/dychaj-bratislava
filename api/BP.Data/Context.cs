using BP.Data.DbModels;
using Microsoft.EntityFrameworkCore;

namespace BP.Data;

public class Context : DbContext
{
    public Context(DbContextOptions<Context> options) : base(options)
    {
    }

    public DbSet<Reading> Reading { get; set; }
    public DbSet<Sensor> Sensor { get; set; }
    public DbSet<Module> Module { get; set; }
}