using Bixa.Backend.DataAccess.Entities.DbProfit;
using Microsoft.EntityFrameworkCore;

namespace Bixa.Backend.DataAccess.Context;

public class ProfitDbContext(DbContextOptions<ProfitDbContext> options) : DbContext(options)
{
    public virtual DbSet<SnEmple> SnEmple { get; set; }
    public virtual DbSet<GrupoFa> GrupoFa { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
        base.OnConfiguring(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<SnEmple>().HasNoKey();
        modelBuilder.Entity<GrupoFa>().HasNoKey();
    }
}