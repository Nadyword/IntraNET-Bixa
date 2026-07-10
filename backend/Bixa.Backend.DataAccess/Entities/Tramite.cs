using Bixa.Backend.DataAccess.Models;
using Bixa.Backend.DataAccess.Entities.Solicitudes;
using Bixa.Backend.Models.Enums;

namespace Bixa.Backend.DataAccess.Entities;

public class Tramite : BaseEntities
{
    public int TipoTramiteId { get; set; }
    public required string UserCi { get; set; }
    public DateTime FechaSolicitud { get; } = DateTime.Now;
    public EstadoTramiteEnum Estado { get; set; }
    public string? MotivoRechazo { get; set; }

    /*-----------------------*/
    public virtual TipoTramite? TipoTramite { get; set; }
    public virtual Users? User { get; set; }
    public virtual ICollection<Aprobacion> Aprobaciones { get; set; } = [];
    public virtual SolicitudVacaciones? SolicitudVacaciones { get; set; }

    public virtual SolicitudDiasEspeciales? SolicitudDiasEspeciales { get; set; }

    public virtual SolicitudUtilidades? SolicitudUtilidades { get; set; }
}