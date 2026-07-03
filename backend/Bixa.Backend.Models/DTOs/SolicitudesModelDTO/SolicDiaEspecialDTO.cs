using Bixa.Backend.Models.Enums;

namespace Bixa.Backend.Models.DTOs.SolicitudesModelDTO;

public class SolicDiaEspecialDTO
{
    public int TipoTramiteId { get; } = 5;
    public required string Ci { get; set; } = null!;
    public DateTime Fecha { get; set; }
    public required string Motivo { get; set; }
    public EstadoTramiteEnum Estado { get; } = EstadoTramiteEnum.Creado;
}
