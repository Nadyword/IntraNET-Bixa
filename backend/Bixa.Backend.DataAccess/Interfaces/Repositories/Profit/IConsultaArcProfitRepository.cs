using Bixa.Backend.DataAccess.Entities.DbProfit;
using Bixa.Backend.Models.Response;

namespace Bixa.Backend.DataAccess.Interfaces.Repositories.Profit;

public interface IConsultaArcProfitRepository
{
    /// <summary>
    /// Obtiene el detalle mensual de remuneraciones e impuesto retenido (ARC/ISLR) del empleado para el año indicado.
    /// </summary>
    /// <param name="ci">La cédula de identidad del empleado.</param>
    /// <param name="anio">El año fiscal consultado.</param>
    Task<Result<List<ConsultaArc>>> GetConsultaArcAsync(string ci, int anio);
}
