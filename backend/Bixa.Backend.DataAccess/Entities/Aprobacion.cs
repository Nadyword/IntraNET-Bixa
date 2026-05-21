using Bixa.Backend.DataAccess.Models;
using Bixa.Backend.Models.Enums;

namespace Bixa.Backend.DataAccess.Entities;

public class Aprobacion : BaseEntities
{
    public int TramiteId { get; set; }
    public required string AprobadorCi { get; set; }
    public int Orden { get; set; }
    public EstadoAprobacionEnum Estado { get; set; }
    public string? Comentario { get; set; }
    public DateTime? FechaRespuesta { get; set; }

    /*-----------------------*/
    public virtual Tramite? Tramite { get; set; }
    public virtual Users? Aprobador { get; set; }
}
