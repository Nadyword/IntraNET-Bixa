using Microsoft.AspNetCore.Identity;

namespace IntranetCorp.Domain.Entities;

public class ApplicationUser : IdentityUser
{
    public string? Nombre { get; set; }
    public string? Cedula { get; set; }
    public string? Cargo { get; set; }
    public string? Departamento { get; set; }
    public DateTime FechaIngreso { get; set; }
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Onboarding
    public bool IsProfileComplete { get; set; } = false;
    public string? OnboardingToken { get; set; }
    public DateTime? OnboardingTokenExpiry { get; set; }

    // Recuperación de contraseña
    public string? PasswordResetToken { get; set; }
    public DateTime? PasswordResetTokenExpiry { get; set; }

    // Datos personales adicionales
    public string? Telefono { get; set; }
    public DateTime? FechaNacimiento { get; set; }

    // Relaciones
    public virtual ICollection<Tramite>? Tramites { get; set; }
    public virtual ICollection<Notification>? Notifications { get; set; }
    public virtual ICollection<OnboardingStep>? OnboardingSteps { get; set; }
}
