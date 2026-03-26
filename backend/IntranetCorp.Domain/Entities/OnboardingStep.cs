using IntranetCorp.Domain.Common;

namespace IntranetCorp.Domain.Entities;

public class OnboardingStep : BaseEntity
{
    public string UserId { get; set; } = null!;
    public int StepNumber { get; set; }
    public string? Titulo { get; set; }
    public string? Descripcion { get; set; }
    public bool IsCompleted { get; set; } = false;
    public DateTime? CompletedAt { get; set; }

    // Relaciones
    public virtual ApplicationUser? Usuario { get; set; }
}
