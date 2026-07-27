namespace Bixa.Backend.DataAccess.Entities.Solicitudes;

public class SolicitudConstanciaTrabajo
{
    public int Id { get; set; }
    public int TramiteId { get; set; }
    public bool ConSueldo { get; set; }
    public bool DirigidoAEspecifico { get; set; }
    public string? DirigidoA { get; set; }

    /*-----------------------*/
    public virtual Tramite? Tramite { get; set; }
}
