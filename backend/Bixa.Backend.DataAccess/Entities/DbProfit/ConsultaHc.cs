namespace Bixa.Backend.DataAccess.Entities.DbProfit;

public class ConsultaHc
{
    public decimal? Cobertura { get; set; }
    public string? CodEmp { get; set; }
    public string? Ci { get; set; }
    public int? NumContrpol { get; set; }
    public string? TipoPoliza { get; set; }
    public int? TipoOrden { get; set; }
    public string? NombreCompleto { get; set; }
    public string? Status { get; set; }
    public string? ParentescoBixa { get; set; }
    public string? Parentesco { get; set; }
    public DateTime? FechaNac { get; set; }
    public decimal? MontoPol { get; set; }
    public decimal? MontoEmp { get; set; }
    public decimal? MontoPolRef { get; set; }
    public decimal? Dias { get; set; }
    public decimal? Tasa { get; set; }
    public decimal? Mes1E071 { get; set; }
    public decimal? Mes2E071 { get; set; }
    public decimal? Mes3E071 { get; set; }

    /// <summary>Cobertura 1: ((MontoPol / 365) * Dias) * Tasa.</summary>
    public decimal? PrimaTrimBs { get; set; }

    /// <summary>Cobertura 2: ((MontoPol - MontoPolRef) / 365) * Dias.</summary>
    public decimal? PagoBixaTrim { get; set; }
}
