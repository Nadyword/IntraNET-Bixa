namespace Bixa.Backend.DataAccess.Entities.DbProfit.Solicitudes;

public class SolicitudVacaciones
{
    public int Id { get; set; }
    public int TramiteId { get; set; }
    public DateTime Desde { get; set; }
    public DateTime Hasta { get; set; }
    public int DiasTotales { get; set; }
    public string? Observaciones { get; set; }

    /*-----------------------*/
    public virtual Tramite? Tramite { get; set; }
}
