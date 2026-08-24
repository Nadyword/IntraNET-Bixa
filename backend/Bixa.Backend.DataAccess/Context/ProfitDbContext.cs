using Bixa.Backend.DataAccess.Entities.DbProfit;
using Bixa.Backend.DataAccess.Models;
using Microsoft.EntityFrameworkCore;

namespace Bixa.Backend.DataAccess.Context;

public class ProfitDbContext(DbContextOptions<ProfitDbContext> options) : DbContext(options)
{
    public virtual DbSet<SnEmple> SnEmple { get; set; }
    public virtual DbSet<GrupoFa> GrupoFa { get; set; }
    public virtual DbSet<Vacaciones> Vacaciones { get; set; }

    public virtual DbSet<DiaEspaciales> DiaEspaciales { get; set; }
    public virtual DbSet<AprobadorPermisoInfo> AprobadorPermisoInfo { get; set; }
    public virtual DbSet<FechaFeriada> FechasFeriadas { get; set; }
    public virtual DbSet<Utilidades> Utilidades { get; set; }
    public virtual DbSet<PrestacionesSociales> PrestacionesSociales { get; set; }
    public virtual DbSet<EquipoSupervisor> EquipoSupervisor { get; set; }
    public virtual DbSet<ConsultaHc> ConsultaHc { get; set; }
    public virtual DbSet<ConsultaArc> ConsultaArc { get; set; }
    public virtual DbSet<AriProfit> Ari { get; set; }

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
        modelBuilder.Entity<Vacaciones>().HasNoKey();
        modelBuilder.Entity<DiaEspaciales>().HasNoKey();
        modelBuilder.Entity<AprobadorPermisoInfo>().HasNoKey();
        modelBuilder.Entity<FechaFeriada>().HasNoKey();
        modelBuilder.Entity<Utilidades>().HasNoKey();
        modelBuilder.Entity<PrestacionesSociales>().HasNoKey();
        modelBuilder.Entity<EquipoSupervisor>().HasNoKey();
        modelBuilder.Entity<ConsultaHc>().HasNoKey();
        modelBuilder.Entity<ConsultaArc>().HasNoKey();
        modelBuilder.Entity<AriProfit>().HasNoKey();
    }
}