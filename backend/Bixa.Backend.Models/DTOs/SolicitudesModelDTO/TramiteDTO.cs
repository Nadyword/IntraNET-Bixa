using Bixa.Backend.Models.Enums;

namespace Bixa.Backend.Models.DTOs.SolicitudesModelDTO;

public class TramiteDTO
{
    public int Id { get; set; }
    public int TipoTramiteId { get; set; }
    public string TipoTramiteNombre { get; set; } = string.Empty;
    public required string UserCi { get; set; }
    public DateTime FechaSolicitud { get; set; }
    public DateTime UpdatedAt { get; set; }
    public EstadoTramiteEnum Estado { get; set; }
    public string? MotivoRechazo { get; set; }
    public VacacionesDetalleDTO? Vacaciones { get; set; }
}

public class VacacionesDetalleDTO
{
    public DateTime Desde { get; set; }
    public DateTime Hasta { get; set; }
    public int DiasTotales { get; set; }
    public string? Observaciones { get; set; }
}