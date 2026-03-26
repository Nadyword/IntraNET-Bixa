using IntranetCorp.Domain.Common;

namespace IntranetCorp.Domain.Entities;

public class Announcement : BaseEntity
{
    public string? Titulo { get; set; }
    public string? Contenido { get; set; }
    public string? AutorId { get; set; }
    public DateTime PublishedAt { get; set; } = DateTime.UtcNow;

    // Relaciones
    public virtual ApplicationUser? Autor { get; set; }
}
