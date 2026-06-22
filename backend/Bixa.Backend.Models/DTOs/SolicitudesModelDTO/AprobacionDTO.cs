using Bixa.Backend.Models.Enums;

namespace Bixa.Backend.Models.DTOs.SolicitudesModelDTO;

public class AprobacionDTO
{
    public int Orden { get; set; }
    public string? Nombre { get; set; }
    public int TramiteId { get; set; }
    public string? Comentario { get; set; }
    public required string AprobadorCi { get; set; }
    public DateTime? FechaRespuesta { get; set; }
    public EstadoAprobacionEnum Estado { get; set; }
}