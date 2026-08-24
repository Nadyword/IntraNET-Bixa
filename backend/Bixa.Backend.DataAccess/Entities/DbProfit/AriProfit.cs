namespace Bixa.Backend.DataAccess.Entities.DbProfit;

/// <summary>
/// Fila que devuelve <c>ProfitSqlTemplates.GetARI</c>: los datos base con los que se rellena
/// la planilla AR-I (identidad del contribuyente, U.T. vigente, carga familiar y la estimación
/// de remuneraciones por percibir en el año gravable).
/// </summary>
public class AriProfit
{
    public string? NombreEmpresa { get; set; }

    public string? NombreCompleto { get; set; }

    public string? Ci { get; set; }

    public string? Rif { get; set; }

    /// <summary>Años de servicio derivados de <c>snemple.fecha_exp</c>.</summary>
    public int? AnosExp { get; set; }

    /// <summary>Valor vigente de la Unidad Tributaria en Bs.</summary>
    public decimal? UniTribu { get; set; }

    public int? CargaFami { get; set; }

    /// <summary>Estimación en Bs de las remuneraciones por percibir en el año (casilla A de la planilla).</summary>
    public decimal? GranTotal { get; set; }

    public int? AnoActual { get; set; }
}
