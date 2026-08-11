using Bixa.Backend.DataAccess.Entities.DbProfit;
using Bixa.Backend.Models.Response;

namespace Bixa.Backend.DataAccess.Interfaces.Repositories.Profit;

public interface IConsultaHcProfitRepository
{
    /// <summary>
    /// Obtiene el registro de HC (cobertura 1 = 10000.00 o cobertura 2 = 20000.00) para el empleado indicado.
    /// </summary>
    /// <param name="ci">La cédula de identidad del empleado.</param>
    /// <param name="cover">El valor de cobertura consultado (10000.00 o 20000.00).</param>
    Task<Result<ConsultaHc>> GetConsultaHcAsync(string ci, decimal cover);

    /// <summary>
    /// Obtiene TODOS los registros de HC (cobertura 1 = 10000.00 o cobertura 2 = 20000.00) para el empleado indicado.
    /// En cobertura 2 puede haber más de un registro (uno por cada familiar/beneficiario).
    /// </summary>
    /// <param name="ci">La cédula de identidad del empleado.</param>
    /// <param name="cover">El valor de cobertura consultado (10000.00 o 20000.00).</param>
    Task<Result<List<ConsultaHc>>> GetConsultaHcListAsync(string ci, decimal cover);
}
