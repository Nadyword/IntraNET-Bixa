using IntranetCorp.Domain.Common;
using IntranetCorp.Domain.Enums;

namespace IntranetCorp.Domain.Entities;

public class Tramite : BaseEntity
{
    public string UserId { get; set; } = null!;
    public TramiteTipo Tipo { get; set; }
    public TramiteEstado Estado { get; set; } = TramiteEstado.Pendiente;
    public string? Descripcion { get; set; }
    public string? Notas { get; set; }
    public DateTime? FechaInicio { get; set; }
    public DateTime? FechaFin { get; set; }
    public int? Duracion { get; set; } // días
    public decimal? Monto { get; set; }

    // Relaciones
    public virtual ApplicationUser? Usuario { get; set; }
    public virtual ICollection<Documento>? Documentos { get; set; }
}
