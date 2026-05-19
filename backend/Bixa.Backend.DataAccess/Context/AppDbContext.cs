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
    public virtual DbSet<Notifications> Notifications { get; set; }
    public virtual DbSet<UserRol> UserRols { get; set; }
    public virtual DbSet<Users> Users { get; set; }
    public virtual DbSet<SoporteChat> SoporteChats { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.ConfigureWarnings(warnings => warnings.Ignore(RelationalEventId.PendingModelChangesWarning));
        base.OnConfiguring(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

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
               .Property(u => u.RefreshToken);

        modelBuilder.Entity<Users>()
            .Property(u => u.RefreshTokenDate);

        // Relación Uno a Muchos de User a UserNotification
        modelBuilder.Entity<Users>()
            .HasMany(u => u.Notification)
            .WithOne(n => n.User)
            .HasForeignKey(n => n.UserCi)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        // Datos semilla para User (Administrador)
        var usuarios = new List<Users>
        {
            new ()
            {
                Id = 74,
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
                RefreshTokenDate = null
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
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_SoporteChat_RespondidoPor");

        #endregion SoporteChat Entity Configuration

        #region BaseEntities Relationships Configuration (Auditoría)

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            // Excluir UserNotification y UserRol ya que no heredan de BaseEntities
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