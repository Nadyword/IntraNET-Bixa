namespace Bixa.Backend.Models.DTOs.ReportesModelDTO;

public class TramiteReportModel
{
    public int TramiteId { get; set; }
    public string TipoTramite { get; set; } = string.Empty;
    public string EmpleadoCi { get; set; } = string.Empty;
    public string EmpleadoNombre { get; set; } = string.Empty;
    public string? EmpleadoCargo { get; set; }
    public string? EmpleadoDepartamento { get; set; }
    public DateTime? FechaIngreso { get; set; }
    public DateTime FechaSolicitud { get; set; }
    public DateTime FechaResolucion { get; set; }
    public string EstadoFinal { get; set; } = string.Empty;
    public VacacionesReportModel? Vacaciones { get; set; }
    public DiaEspecialReportModel? DiaEspecial { get; set; }
    public UtilidadesReportModel? Utilidades { get; set; }
    public List<AprobacionReportModel> Aprobaciones { get; set; } = [];
    public byte[]? LogoEmpresa { get; set; }
    public string? EmpleadoUrlFirma { get; set; }
    public byte[]? EmpleadoFirmaImagen { get; set; }
}

public class DiaEspecialReportModel
{
    public DateTime Fecha { get; set; }
    public string Motivo { get; set; } = string.Empty;
}

public class UtilidadesReportModel
{
    public decimal Monto { get; set; }
    public string Motivo { get; set; } = string.Empty;
    // TODO: reemplazar por la consulta dinámica del monto disponible de utilidades cuando exista.
    public decimal TotalUtilidadesDisponible { get; set; }
}

public class VacacionesReportModel
{
    public DateTime Desde { get; set; }
    public DateTime Hasta { get; set; }
    public int DiasTotales { get; set; }
    public int DiasPendientes { get; set; }
    public string? Observaciones { get; set; }
}

public class AprobacionReportModel
{
    public string AprobadorCi { get; set; } = string.Empty;
    public string AprobadorNombre { get; set; } = string.Empty;
    public string Accion { get; set; } = string.Empty;
    public DateTime Fecha { get; set; }
    public string? Motivo { get; set; }
    public string? UrlFirma { get; set; }
    public byte[]? FirmaImagen { get; set; }
}
