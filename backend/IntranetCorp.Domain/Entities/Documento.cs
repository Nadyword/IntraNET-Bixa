using IntranetCorp.Domain.Common;

namespace IntranetCorp.Domain.Entities;

public class Documento : BaseEntity
{
    public Guid TramiteId { get; set; }
    public string? FileName { get; set; }
    public string? FilePath { get; set; }
    public string? ContentType { get; set; }
    public long Size { get; set; }

    // Relaciones
    public virtual Tramite? Tramite { get; set; }
}
