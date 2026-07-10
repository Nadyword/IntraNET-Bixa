namespace Bixa.Backend.DataAccess.Entities.Solicitudes;

public class SolicitudUtilidades
{
    public int Id { get; set; }
    public int TramiteId { get; set; }
    public decimal Monto { get; set; }
    public required string Motivo { get; set; }

    /*-----------------------*/
    public virtual Tramite? Tramite { get; set; }
}
