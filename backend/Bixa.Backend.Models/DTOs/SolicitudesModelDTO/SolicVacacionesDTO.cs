namespace Bixa.Backend.Models.DTOs.SolicitudesModelDTO;

public class SolicVacacionesDTO
{
    //private int TipoTramiteId { get; } = 4;
    //private int TipoTramiteId { get; } = 4;

    public required string Ci { get; set; } = null!;
    public DateTime FechaSolicitud { get; set; } = DateTime.Now;
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
    public string Observaciones { get; set; } = null!;
}