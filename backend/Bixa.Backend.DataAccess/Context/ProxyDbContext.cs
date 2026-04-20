using Bixa.Backend.DataAccess.Entities.DbProxy;
using Microsoft.EntityFrameworkCore;

namespace Bixa.Backend.DataAccess.Context;

public class ProxyDbContext(DbContextOptions<ProxyDbContext> options) : DbContext(options)
{
    public virtual DbSet<SnEmple> SnEmple { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
        base.OnConfiguring(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<SnEmple>().HasNoKey();
    }
}