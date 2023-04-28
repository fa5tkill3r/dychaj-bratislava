using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace BP.Data.TestModels;

public partial class TestContext : DbContext
{
    public TestContext()
    {
    }

    public TestContext(DbContextOptions<TestContext> options)
        : base(options)
    {
    }

    public virtual DbSet<TableName> TableNames { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TableName>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("table_name");

            entity.HasIndex(e => e.Id, "table_name_id_uindex").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Meno)
                .HasColumnType("text")
                .HasColumnName("meno");
            entity.Property(e => e.Priezvisko)
                .HasColumnType("text")
                .HasColumnName("priezvisko");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
