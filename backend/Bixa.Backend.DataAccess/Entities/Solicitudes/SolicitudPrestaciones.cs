using Bixa.Backend.Models.Enums;

namespace Bixa.Backend.DataAccess.Entities.Solicitudes;

public class SolicitudPrestaciones
{
    public int Id { get; set; }
    public int TramiteId { get; set; }
    public bool EsPrestamo { get; set; }
    public decimal Monto { get; set; }
    public DestinoPrestacionEnum Destino { get; set; }
    public string? Observaciones { get; set; }
    public int? Cuotas { get; set; }
    public string? ArchivoAdjunto { get; set; }

    /*-----------------------*/
    public virtual Tramite? Tramite { get; set; }
}
