namespace Bixa.Backend.DataAccess.Entities.Solicitudes;

public class SolicitudDiasEspeciales
{
    public int Id { get; set; }
    public int TramiteId { get; set; }
    public DateTime Fecha { get; set; }
    public required string Motivo { get; set; }

    /*-----------------------*/
    public virtual Tramite? Tramite { get; set; }
}