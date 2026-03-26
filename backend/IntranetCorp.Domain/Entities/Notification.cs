using IntranetCorp.Domain.Common;

namespace IntranetCorp.Domain.Entities;

public class Notification : BaseEntity
{
    public string UserId { get; set; } = null!;
    public string? Tipo { get; set; }
    public string? Mensaje { get; set; }
    public bool IsRead { get; set; } = false;
    public DateTime? ReadAt { get; set; }

    // Relaciones
    public virtual ApplicationUser? Usuario { get; set; }
}
