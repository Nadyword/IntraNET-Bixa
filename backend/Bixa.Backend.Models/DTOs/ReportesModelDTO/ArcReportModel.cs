namespace Bixa.Backend.Models.DTOs.ReportesModelDTO;

public class ArcReportModel
{
    public int Anio { get; set; }
    public string EmpleadoNombre { get; set; } = string.Empty;
    public string EmpleadoCi { get; set; } = string.Empty;
    public string EmpleadoRif { get; set; } = string.Empty;
    public byte[]? LogoEmpresa { get; set; }
    public byte[]? FirmaSelloAgente { get; set; }
    public List<ArcMesReportModel> Meses { get; set; } = [];
}

public class ArcMesReportModel
{
    public string Mes { get; set; } = string.Empty;
    public decimal Remuneracion { get; set; }
    public decimal? PorcentRetencion { get; set; }
    public decimal? ImpuestoRetenido { get; set; }
    public decimal RemuneracionAcumulada { get; set; }
    public decimal? ImpuestoRetenidoAcum { get; set; }
}
