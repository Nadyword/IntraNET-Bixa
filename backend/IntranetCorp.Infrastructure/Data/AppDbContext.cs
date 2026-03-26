using IntranetCorp.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace IntranetCorp.Infrastructure.Data;

public class AppDbContext : IdentityDbContext<ApplicationUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Tramite> Tramites { get; set; }
    public DbSet<Documento> Documentos { get; set; }
    public DbSet<Announcement> Announcements { get; set; }
    public DbSet<Notification> Notifications { get; set; }
    public DbSet<OnboardingStep> OnboardingSteps { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Soft delete filter global
        builder.Entity<ApplicationUser>().HasQueryFilter(u => !u.IsDeleted);
        builder.Entity<Tramite>().HasQueryFilter(t => !t.IsDeleted);
        builder.Entity<Documento>().HasQueryFilter(d => !d.IsDeleted);
        builder.Entity<Announcement>().HasQueryFilter(a => !a.IsDeleted);
        builder.Entity<Notification>().HasQueryFilter(n => !n.IsDeleted);
        builder.Entity<OnboardingStep>().HasQueryFilter(o => !o.IsDeleted);

        // Relaciones
        builder.Entity<Tramite>()
            .HasOne(t => t.Usuario)
            .WithMany(u => u.Tramites)
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Documento>()
            .HasOne(d => d.Tramite)
            .WithMany(t => t.Documentos)
            .HasForeignKey(d => d.TramiteId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Notification>()
            .HasOne(n => n.Usuario)
            .WithMany(u => u.Notifications)
            .HasForeignKey(n => n.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<OnboardingStep>()
            .HasOne(o => o.Usuario)
            .WithMany(u => u.OnboardingSteps)
            .HasForeignKey(o => o.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Announcement>()
            .HasOne(a => a.Autor)
            .WithMany()
            .HasForeignKey(a => a.AutorId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<Tramite>()
            .Property(t => t.Monto)
            .HasPrecision(18, 2);
    }
}