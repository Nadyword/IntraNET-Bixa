namespace Bixa.Backend.Models.DTOs.SolicitudesModelDTO;

public class AprobarTramiteDTO
{
    public int TramiteId { get; set; }
    public string? Ci { get; set; }
    public int Estado { get; set; }
    public string? Motivo { get; set; }
}