using Bixa.Backend.Models.Enums;

namespace Bixa.Backend.Models.DTOs.SolicitudesModelDTO;

public class TramiteDTO
{
    public int Id { get; set; }
    public int TipoTramiteId { get; set; }
    public string TipoTramiteNombre { get; set; } = string.Empty;
    public required string UserCi { get; set; }
    public string? UserNombre { get; set; }
    public DateTime FechaSolicitud { get; set; }
    public DateTime UpdatedAt { get; set; }
    public EstadoTramiteEnum Estado { get; set; }
    public string? MotivoRechazo { get; set; }
    public VacacionesDetalleDTO? Vacaciones { get; set; }
    public DiaEspecialDetalleDTO? DiaEspecial { get; set; }
    public UtilidadesDetalleDTO? Utilidades { get; set; }
    public PrestacionesDetalleDTO? Prestaciones { get; set; }
    public ConstanciaTrabajoDetalleDTO? ConstanciaTrabajo { get; set; }
}

public class VacacionesDetalleDTO
{
    public DateTime Desde { get; set; }
    public DateTime Hasta { get; set; }
    public int DiasTotales { get; set; }
    public string? Observaciones { get; set; }
}

public class DiaEspecialDetalleDTO
{
    public DateTime Fecha { get; set; }
    public required string Motivo { get; set; }
}

public class UtilidadesDetalleDTO
{
    public decimal Monto { get; set; }
    public required string Motivo { get; set; }
}

public class PrestacionesDetalleDTO
{
    public bool EsPrestamo { get; set; }
    public decimal Monto { get; set; }
    public required string Destino { get; set; }
    public string? Observaciones { get; set; }
    public int? Cuotas { get; set; }
    public decimal? MontoCuota { get; set; }
    public string? ArchivoAdjuntoUrl { get; set; }
}

public class ConstanciaTrabajoDetalleDTO
{
    public bool ConSueldo { get; set; }
    public bool DirigidoAEspecifico { get; set; }
    public string? DirigidoA { get; set; }
}