using Bixa.Backend.DataAccess.Models;
using Bixa.Backend.Models.Enums;

namespace Bixa.Backend.DataAccess.Entities;

public class Tramite : BaseEntities
{
    public int TipoTramiteId { get; set; }
    public required string UserCi { get; set; }
    public EstadoTramiteEnum Estado { get; set; }
    public string? Observacion { get; set; }
    public string? MotivoRechazo { get; set; }

    /*-----------------------*/
    public virtual TipoTramite? TipoTramite { get; set; }
    public virtual Users? User { get; set; }
    public virtual ICollection<Aprobacion> Aprobaciones { get; set; } = [];
}
