using Bixa.Backend.Models.Enums;

namespace Bixa.Backend.Models.DTOs.SolicitudesModelDTO;

public class SolicUtilidadesDTO
{
    public int TipoTramiteId { get; } = 1;
    public required string Ci { get; set; } = null!;
    public decimal Monto { get; set; }
    public required string Motivo { get; set; }
    public EstadoTramiteEnum Estado { get; } = EstadoTramiteEnum.Creado;
}
