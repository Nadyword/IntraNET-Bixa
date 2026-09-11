using Bixa.Backend.DataAccess.Models;
using Bixa.Backend.DataAccess.Entities.Solicitudes;
using Bixa.Backend.Models.Enums;

namespace Bixa.Backend.DataAccess.Entities;

public class Tramite : BaseEntities
{
    public int TipoTramiteId { get; set; }
    public required string UserCi { get; set; }
    public EstadoTramiteEnum Estado { get; set; }
    public string? MotivoRechazo { get; set; }

    /// <summary>
    /// Orden de la firma que le toca al trámite. Avanza con cada aprobación y sustituye al antiguo
    /// decremento de <see cref="Aprobacion.Orden"/>, que destruía el orden original de la cadena.
    /// </summary>
    public int OrdenActual { get; set; } = 1;

    /*-----------------------*/
    public virtual TipoTramite? TipoTramite { get; set; }
    public virtual Users? User { get; set; }
    public virtual ICollection<Aprobacion> Aprobaciones { get; set; } = [];
    public virtual SolicitudVacaciones? SolicitudVacaciones { get; set; }
    public virtual SolicitudDiasEspeciales? SolicitudDiasEspeciales { get; set; }
    public virtual SolicitudUtilidades? SolicitudUtilidades { get; set; }
    public virtual SolicitudPrestaciones? SolicitudPrestaciones { get; set; }
    public virtual SolicitudConstanciaTrabajo? SolicitudConstanciaTrabajo { get; set; }
}