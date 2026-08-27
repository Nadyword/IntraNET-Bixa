using Bixa.Backend.DataAccess.Entities.Solicitudes;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Bixa.Backend.Models.Configurations;
using Bixa.Backend.DataAccess.Entities;
using Bixa.Backend.DataAccess.Models;
using Microsoft.EntityFrameworkCore;
using Bixa.Backend.Models.Utilities;
using Bixa.Backend.Models.Enums;

namespace Bixa.Backend.DataAccess.Context;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public virtual DbSet<SolicitudDiasEspeciales> SolicitudesDiasEspeciales { get; set; }
    public virtual DbSet<SolicitudUtilidades> SolicitudesUtilidades { get; set; }
    public virtual DbSet<SolicitudPrestaciones> SolicitudesPrestaciones { get; set; }
    public virtual DbSet<SolicitudConstanciaTrabajo> SolicitudesConstanciaTrabajo { get; set; }
    public virtual DbSet<SolicitudVacaciones> SolicitudesVacaciones { get; set; }
    public virtual DbSet<SolicitudesChats> SolicitudesChats { get; set; }
    public virtual DbSet<SoporteChat> SoporteChats { get; set; }
    public virtual DbSet<Aprobacion> Aprobaciones { get; set; }
    public virtual DbSet<TipoTramite> TipoTramites { get; set; }
    public virtual DbSet<Notifications> Notifications { get; set; }
    public virtual DbSet<PorAprobar> PorAprobar { get; set; }
    public virtual DbSet<UserRol> UserRols { get; set; }
    public virtual DbSet<Tramite> Tramites { get; set; }
    public virtual DbSet<Users> Users { get; set; }
    public virtual DbSet<FAQs> FAQs { get; set; }
    public virtual DbSet<HcMesRegistro> HcMesRegistros { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.ConfigureWarnings(warnings => warnings.Ignore(RelationalEventId.PendingModelChangesWarning));
        base.OnConfiguring(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        #region Views Configuration

        modelBuilder.Entity<SolicitudesChats>().HasNoKey().ToView(null);
        modelBuilder.Entity<PorAprobar>().HasNoKey().ToView(null);

        #endregion Views Configuration

        #region User Entity Configuration

        modelBuilder.Entity<Users>()
            .Property(u => u.Id)
            .ValueGeneratedOnAdd();

        modelBuilder.Entity<Users>()
            .HasKey(u => u.Id);

        modelBuilder.Entity<Users>()
            .Property(u => u.Ci)
            .IsUnicode();

        modelBuilder.Entity<Users>()
            .Property(u => u.PasswordHash)
            .IsRequired()
            .HasMaxLength(100);

        modelBuilder.Entity<Users>()
            .Property(u => u.FirstName)
            .IsRequired()
            .HasMaxLength(ModelLengths.Name);

        modelBuilder.Entity<Users>()
            .Property(u => u.LastName)
            .IsRequired()
            .HasMaxLength(ModelLengths.Name);

        modelBuilder.Entity<Users>()
            .Property(u => u.Ci)
            .IsRequired()
            .HasMaxLength(ModelLengths.Ci);

        modelBuilder.Entity<Users>()
            .HasIndex(u => u.Ci)
            .IsUnique();

        modelBuilder.Entity<Users>()
            .HasOne(u => u.UserRol)
            .WithMany()
            .HasForeignKey(u => u.IdUserRol)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_Users_UserRols_IdUserRol");

        modelBuilder.Entity<Users>()
            .Property(u => u.IsActive)
            .IsRequired()
            .HasDefaultValue(false);

        modelBuilder.Entity<Users>()
            .Property(u => u.UrlFirma)
            .IsRequired(false)
            .HasMaxLength(ModelLengths.FilePath);

        modelBuilder.Entity<Users>()
            .Property(u => u.RefreshToken);

        modelBuilder.Entity<Users>()
            .Property(u => u.RefreshTokenDate);

        modelBuilder.Entity<Users>()
            .HasMany(u => u.Notification)
            .WithOne(n => n.User)
            .HasForeignKey(n => n.UserCi)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        var usuarios = new List<Users>
        {
            new ()
            {
                Id = 1,
                PasswordHash = Hasher.HashPassword("Sa753951."),
                FirstName = "MAGLENY",
                LastName = "MATHEUS",
                IsActive = true,
                Ci = "10.486.165",
                IdUserRol = (int)UserRolEnum.Administrador,
                CreatedAt = new DateTime(2025, 1, 1),
                UpdatedAt = new DateTime(2025, 1, 1),
                ModifiedByCi = null,
                RefreshToken = null,
                RefreshTokenDate = null,
                UrlFirma = ""
            },
            new ()
            {
                Id = 2,
                PasswordHash = Hasher.HashPassword("Sa753951."),
                FirstName = "ROBERTO JOSE",
                LastName = "ESPINOZA MARMOL",
                IsActive = true,
                Ci = "10.508.836",
                IdUserRol = (int)UserRolEnum.Supervisor,
                CreatedAt = new DateTime(2025, 1, 1),
                UpdatedAt = new DateTime(2025, 1, 1),
                ModifiedByCi = null,
                RefreshToken = null,
                RefreshTokenDate = null,
                UrlFirma = "SinFirma.png"
            },
            new ()
            {
                Id = 3,
                PasswordHash = Hasher.HashPassword("Sa753951."),
                FirstName = "RICARDO",
                LastName = "RUEDA ALONSO",
                IsActive = true,
                Ci = "3.666.186",
                IdUserRol = (int)UserRolEnum.Administrador,
                CreatedAt = new DateTime(2025, 1, 1),
                UpdatedAt = new DateTime(2025, 1, 1),
                ModifiedByCi = null,
                RefreshToken = null,
                RefreshTokenDate = null,
                UrlFirma = "SinFirma.png"
            },
            new ()
            {
                Id = 4,
                PasswordHash = Hasher.HashPassword("Sa753951."),
                FirstName = "ELAIDER MARILYN",
                LastName = "ALVAREZ RAMIREZ",
                IsActive = true,
                Ci = "20.603.146",
                IdUserRol = (int)UserRolEnum.Empleado,
                CreatedAt = new DateTime(2025, 1, 1),
                UpdatedAt = new DateTime(2025, 1, 1),
                ModifiedByCi = null,
                RefreshToken = null,
                RefreshTokenDate = null,
                UrlFirma = "SinFirma.png"
            }
        };
        modelBuilder.Entity<Users>().HasData(usuarios);

        #endregion User Entity Configuration

        #region UserRol Entity Configuration

        modelBuilder.Entity<UserRol>().ToTable("UserRol");
        modelBuilder.Entity<UserRol>()
            .HasKey(ur => ur.Id);

        modelBuilder.Entity<UserRol>()
            .Property(ur => ur.Id)
            .ValueGeneratedOnAdd();

        modelBuilder.Entity<UserRol>()
            .Property(ur => ur.Name)
            .IsRequired()
            .HasMaxLength(ModelLengths.Name);

        modelBuilder.Entity<UserRol>().HasData(
            Enum.GetValues<UserRolEnum>()
                .Cast<UserRolEnum>()
                .Select(userRole => new UserRol
                {
                    Id = (int)userRole,
                    Name = userRole.GetDescription(),
                })
                .ToList());

        #endregion UserRol Entity Configuration

        #region Notification Entity Configuration

        modelBuilder.Entity<Notifications>(entity =>
        {
            entity.Property(n => n.Id)
                .ValueGeneratedOnAdd();

            entity.Property(n => n.UserCi)
                .IsRequired();

            entity.Property(n => n.NotificationType)
                .IsRequired()
                .HasMaxLength(ModelLengths.Description);

            entity.Property(n => n.Title)
                .IsRequired()
                .HasMaxLength(ModelLengths.ItemName);

            entity.Property(n => n.Message)
                .IsRequired()
                .HasMaxLength(ModelLengths.Description);

            entity.Property(n => n.ReadAt)
                .IsRequired(false);

            entity.Property(n => n.ReferenceType)
                .IsRequired(false)
                .HasMaxLength(ModelLengths.Description);

            entity.Property(n => n.ReferenceId)
                .IsRequired(false);

            modelBuilder.Entity<Notifications>()
                .HasOne(n => n.User)
                .WithMany(u => u.Notification)
                .HasForeignKey(n => n.UserCi)
                .HasPrincipalKey(u => u.Ci)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);
        });

        #endregion Notification Entity Configuration

        #region SoporteChat Entity Configuration

        modelBuilder.Entity<SoporteChat>()
            .HasKey(sc => sc.Id);

        modelBuilder.Entity<SoporteChat>()
            .Property(sc => sc.Id)
            .ValueGeneratedOnAdd();

        modelBuilder.Entity<SoporteChat>()
            .Property(sc => sc.UserCi)
            .IsRequired();

        modelBuilder.Entity<SoporteChat>()
            .Property(sc => sc.Message)
            .IsRequired(false)
            .HasMaxLength(ModelLengths.Observation);

        modelBuilder.Entity<SoporteChat>()
            .Property(sc => sc.IsRead)
            .IsRequired();

        modelBuilder.Entity<SoporteChat>()
            .HasOne(sc => sc.RespondidoPor)
            .WithMany()
            .HasForeignKey(sc => sc.RespondidoPorCi)
            .HasPrincipalKey(u => u.Ci)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<SoporteChat>()
            .HasIndex(sc => sc.UserCi)
            .HasDatabaseName("IX_SoporteChat_UserCi");

        modelBuilder.Entity<SoporteChat>()
            .HasIndex(sc => sc.RespondidoPorCi)
            .HasDatabaseName("IX_SoporteChat_RespondidoPorCi");

        #endregion SoporteChat Entity Configuration

        #region TipoTramite Entity Configuration

        modelBuilder.Entity<TipoTramite>().HasKey(tt => tt.Id);
        modelBuilder.Entity<TipoTramite>().Property(tt => tt.Id).ValueGeneratedOnAdd();
        modelBuilder.Entity<TipoTramite>().Property(tt => tt.Nombre).IsRequired().HasMaxLength(ModelLengths.Name);

        modelBuilder.Entity<TipoTramite>().HasData(
            Enum.GetValues<TipoTramiteEnum>()
                .Cast<TipoTramiteEnum>()
                .Select(t => new TipoTramite
                {
                    Id = (int)t,
                    Nombre = t.GetDescription(),
                })
                .ToList());

        #endregion TipoTramite Entity Configuration

        #region Tramite Entity Configuration

        modelBuilder.Entity<Tramite>().HasKey(t => t.Id);
        modelBuilder.Entity<Tramite>().Property(t => t.Id).ValueGeneratedOnAdd();
        modelBuilder.Entity<Tramite>().Property(t => t.TipoTramiteId).IsRequired();
        modelBuilder.Entity<Tramite>().Property(t => t.UserCi).IsRequired().HasMaxLength(ModelLengths.Ci);
        modelBuilder.Entity<Tramite>().Property(t => t.Estado).IsRequired().HasConversion<int>();
        modelBuilder.Entity<Tramite>().Property(t => t.MotivoRechazo).IsRequired(false).HasMaxLength(ModelLengths.Observation);

        modelBuilder.Entity<Tramite>()
            .HasOne(t => t.TipoTramite)
            .WithMany()
            .HasForeignKey(t => t.TipoTramiteId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Tramite>()
            .HasOne(t => t.User)
            .WithMany(u => u.Tramites)
            .HasForeignKey(t => t.UserCi)
            .HasPrincipalKey(u => u.Ci)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        #endregion Tramite Entity Configuration

        #region Aprobacion Entity Configuration

        modelBuilder.Entity<Aprobacion>().HasKey(a => a.Id);
        modelBuilder.Entity<Aprobacion>().Property(a => a.Id).ValueGeneratedOnAdd();
        modelBuilder.Entity<Aprobacion>().Property(a => a.TramiteId).IsRequired();
        modelBuilder.Entity<Aprobacion>().Property(a => a.AprobadorCi).IsRequired().HasMaxLength(ModelLengths.Ci);
        modelBuilder.Entity<Aprobacion>().Property(a => a.Orden).IsRequired();
        modelBuilder.Entity<Aprobacion>().Property(a => a.Estado).IsRequired().HasConversion<int>();
        modelBuilder.Entity<Aprobacion>().Property(a => a.Nombre).IsRequired(false).HasMaxLength(ModelLengths.Name);
        modelBuilder.Entity<Aprobacion>().Property(a => a.Comentario).IsRequired(false).HasMaxLength(ModelLengths.Observation);
        modelBuilder.Entity<Aprobacion>().Property(a => a.FechaRespuesta).IsRequired(false);

        modelBuilder.Entity<Aprobacion>()
            .HasOne(a => a.Tramite)
            .WithMany(t => t.Aprobaciones)
            .HasForeignKey(a => a.TramiteId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Aprobacion>()
            .HasOne(a => a.Aprobador)
            .WithMany(u => u.Aprobaciones)
            .HasForeignKey(a => a.AprobadorCi)
            .HasPrincipalKey(u => u.Ci)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        #endregion Aprobacion Entity Configuration

        #region SolicitudVacaciones Entity Configuration

        modelBuilder.Entity<SolicitudVacaciones>().HasKey(sv => sv.Id);
        modelBuilder.Entity<SolicitudVacaciones>().Property(sv => sv.Id).ValueGeneratedOnAdd();
        modelBuilder.Entity<SolicitudVacaciones>().Property(sv => sv.TramiteId).IsRequired();
        modelBuilder.Entity<SolicitudVacaciones>().Property(sv => sv.Desde).IsRequired();
        modelBuilder.Entity<SolicitudVacaciones>().Property(sv => sv.Hasta).IsRequired();
        modelBuilder.Entity<SolicitudVacaciones>().Property(sv => sv.DiasTotales).IsRequired();
        modelBuilder.Entity<SolicitudVacaciones>().Property(sv => sv.Observaciones).IsRequired(false).HasMaxLength(ModelLengths.Observation);

        modelBuilder.Entity<SolicitudVacaciones>()
            .HasOne(sv => sv.Tramite)
            .WithOne(t => t.SolicitudVacaciones)
            .HasForeignKey<SolicitudVacaciones>(sv => sv.TramiteId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        #endregion SolicitudVacaciones Entity Configuration

        #region SolicitudDiasEspeciales Entity Configuration

        modelBuilder.Entity<SolicitudDiasEspeciales>().HasKey(sde => sde.Id);
        modelBuilder.Entity<SolicitudDiasEspeciales>().Property(sde => sde.Id).ValueGeneratedOnAdd();
        modelBuilder.Entity<SolicitudDiasEspeciales>().Property(sde => sde.TramiteId).IsRequired();
        modelBuilder.Entity<SolicitudDiasEspeciales>().Property(sde => sde.Fecha).IsRequired();
        modelBuilder.Entity<SolicitudDiasEspeciales>().Property(sde => sde.Motivo).IsRequired().HasMaxLength(ModelLengths.Description);

        modelBuilder.Entity<SolicitudDiasEspeciales>()
            .HasOne(sde => sde.Tramite)
            .WithOne(t => t.SolicitudDiasEspeciales)
            .HasForeignKey<SolicitudDiasEspeciales>(sde => sde.TramiteId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        #endregion SolicitudDiasEspeciales Entity Configuration

        #region SolicitudUtilidades Entity Configuration

        modelBuilder.Entity<SolicitudUtilidades>().HasKey(su => su.Id);
        modelBuilder.Entity<SolicitudUtilidades>().Property(su => su.Id).ValueGeneratedOnAdd();
        modelBuilder.Entity<SolicitudUtilidades>().Property(su => su.TramiteId).IsRequired();
        modelBuilder.Entity<SolicitudUtilidades>().Property(su => su.Monto).IsRequired().HasColumnType("decimal(18,2)");
        modelBuilder.Entity<SolicitudUtilidades>().Property(su => su.Motivo).IsRequired().HasMaxLength(ModelLengths.Description);

        modelBuilder.Entity<SolicitudUtilidades>()
            .HasOne(su => su.Tramite)
            .WithOne(t => t.SolicitudUtilidades)
            .HasForeignKey<SolicitudUtilidades>(su => su.TramiteId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        #endregion SolicitudUtilidades Entity Configuration

        #region SolicitudPrestaciones Entity Configuration

        modelBuilder.Entity<SolicitudPrestaciones>().HasKey(sp => sp.Id);
        modelBuilder.Entity<SolicitudPrestaciones>().Property(sp => sp.Id).ValueGeneratedOnAdd();
        modelBuilder.Entity<SolicitudPrestaciones>().Property(sp => sp.TramiteId).IsRequired();
        modelBuilder.Entity<SolicitudPrestaciones>().Property(sp => sp.EsPrestamo).IsRequired();
        modelBuilder.Entity<SolicitudPrestaciones>().Property(sp => sp.Monto).IsRequired().HasColumnType("decimal(18,2)");
        modelBuilder.Entity<SolicitudPrestaciones>().Property(sp => sp.Destino).IsRequired().HasConversion<int>();
        modelBuilder.Entity<SolicitudPrestaciones>().Property(sp => sp.Observaciones).IsRequired(false).HasMaxLength(ModelLengths.Observation);
        modelBuilder.Entity<SolicitudPrestaciones>().Property(sp => sp.Cuotas).IsRequired(false);
        modelBuilder.Entity<SolicitudPrestaciones>().Property(sp => sp.ArchivoAdjunto).IsRequired(false).HasMaxLength(ModelLengths.FileName);

        modelBuilder.Entity<SolicitudPrestaciones>()
            .HasOne(sp => sp.Tramite)
            .WithOne(t => t.SolicitudPrestaciones)
            .HasForeignKey<SolicitudPrestaciones>(sp => sp.TramiteId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        #endregion SolicitudPrestaciones Entity Configuration

        #region SolicitudConstanciaTrabajo Entity Configuration

        modelBuilder.Entity<SolicitudConstanciaTrabajo>().HasKey(sct => sct.Id);
        modelBuilder.Entity<SolicitudConstanciaTrabajo>().Property(sct => sct.Id).ValueGeneratedOnAdd();
        modelBuilder.Entity<SolicitudConstanciaTrabajo>().Property(sct => sct.TramiteId).IsRequired();
        modelBuilder.Entity<SolicitudConstanciaTrabajo>().Property(sct => sct.ConSueldo).IsRequired();
        modelBuilder.Entity<SolicitudConstanciaTrabajo>().Property(sct => sct.DirigidoAEspecifico).IsRequired();
        modelBuilder.Entity<SolicitudConstanciaTrabajo>().Property(sct => sct.DirigidoA).IsRequired(false).HasMaxLength(ModelLengths.Name);

        modelBuilder.Entity<SolicitudConstanciaTrabajo>()
            .HasOne(sct => sct.Tramite)
            .WithOne(t => t.SolicitudConstanciaTrabajo)
            .HasForeignKey<SolicitudConstanciaTrabajo>(sct => sct.TramiteId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        #endregion SolicitudConstanciaTrabajo Entity Configuration

        #region FAQs Entity Configuration

        modelBuilder.Entity<FAQs>().HasKey(f => f.Id);
        modelBuilder.Entity<FAQs>().Property(f => f.Id).ValueGeneratedOnAdd();
        modelBuilder.Entity<FAQs>().Property(f => f.Question).HasColumnType("nvarchar(max)");
        modelBuilder.Entity<FAQs>().Property(f => f.Response).HasColumnType("nvarchar(max)");
        modelBuilder.Entity<FAQs>().Property(f => f.DisplayOrder).HasDefaultValue(0);
        modelBuilder.Entity<FAQs>().HasIndex(f => f.DisplayOrder);

        #endregion FAQs Entity Configuration

        #region HcMesRegistro Entity Configuration

        modelBuilder.Entity<HcMesRegistro>().HasKey(h => h.Id);
        modelBuilder.Entity<HcMesRegistro>().Property(h => h.Id).ValueGeneratedOnAdd();
        modelBuilder.Entity<HcMesRegistro>().Property(h => h.UserCi).IsRequired().HasMaxLength(ModelLengths.Ci);
        modelBuilder.Entity<HcMesRegistro>().Property(h => h.Mes1).IsRequired().HasColumnType("decimal(18,2)");
        modelBuilder.Entity<HcMesRegistro>().Property(h => h.Mes2).IsRequired().HasColumnType("decimal(18,2)");
        modelBuilder.Entity<HcMesRegistro>().Property(h => h.Mes3).IsRequired().HasColumnType("decimal(18,2)");
        modelBuilder.Entity<HcMesRegistro>().Property(h => h.PrimaTrimBs).IsRequired().HasColumnType("decimal(18,2)");

        modelBuilder.Entity<HcMesRegistro>()
            .HasIndex(h => h.UserCi)
            .IsUnique();

        modelBuilder.Entity<HcMesRegistro>()
            .HasOne(h => h.User)
            .WithMany()
            .HasForeignKey(h => h.UserCi)
            .HasPrincipalKey(u => u.Ci)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        #endregion HcMesRegistro Entity Configuration

        #region BaseEntities Relationships Configuration (Auditoría)

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(BaseEntities).IsAssignableFrom(entityType.ClrType) && entityType.ClrType != typeof(BaseEntities))
            {
                modelBuilder.Entity(entityType.ClrType)
                    .Property("CreatedAt")
                    .IsRequired();

                modelBuilder.Entity(entityType.ClrType)
                    .Property("UpdatedAt")
                    .IsRequired();

                modelBuilder.Entity(entityType.ClrType)
                    .HasOne(typeof(Users), "ModifiedUser")
                    .WithMany()
                    .HasForeignKey("ModifiedByCi")
                    .HasPrincipalKey("Ci")
                    .IsRequired(false)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName($"FK_{entityType.ClrType.Name}_ModifiedUser");
            }
        }

        #endregion BaseEntities Relationships Configuration (Auditoría)
    }
}