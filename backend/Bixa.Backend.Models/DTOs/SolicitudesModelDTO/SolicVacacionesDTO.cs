using Bixa.Backend.Models.Enums;

namespace Bixa.Backend.Models.DTOs.SolicitudesModelDTO;

public class SolicVacacionesDTO
{
    public int TipoTramiteId { get; } = 4;
    public required string Ci { get; set; } = null!;
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
    public int DiasTotales { get; set; }
    public string Observaciones { get; set; } = null!;
    public EstadoTramiteEnum Estado { get; } = EstadoTramiteEnum.Creado;
}