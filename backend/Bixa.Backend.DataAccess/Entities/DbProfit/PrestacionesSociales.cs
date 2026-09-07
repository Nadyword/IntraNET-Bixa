namespace Bixa.Backend.DataAccess.Entities.DbProfit;

/// <summary>
/// Fila que devuelve <c>ProfitSqlTemplates.GetPrestacionesSociales</c>.
/// </summary>
public class PrestacionesSociales
{
    /// <summary>Monto en Bs. del que el empleado puede disponer (75 % del acumulado neto de anticipos).</summary>
    public decimal? MontoDisponible { get; set; }

    /// <summary>
    /// Fecha del último anticipo de prestaciones cobrado. Es <c>null</c> cuando el empleado nunca
    /// ha solicitado uno.
    /// </summary>
    public DateTime? UltimaSolicitud { get; set; }
}
