using Bixa.Backend.DataAccess.Interfaces.Repositories.Profit;
using Bixa.Backend.DataAccess.Entities.DbProfit;
using Bixa.Backend.DataAccess.Templates.Profit;
using Bixa.Backend.DataAccess.Context;
using Bixa.Backend.Models.Response;
using Microsoft.EntityFrameworkCore;
using Bixa.Backend.Models.Enums;
using Microsoft.Data.SqlClient;

namespace Bixa.Backend.DataAccess.Repository.Profit;

public class ConsultaHcProfitRepository(ProfitDbContext context) : IConsultaHcProfitRepository
{
    private readonly ProfitDbContext _context = context ?? throw new ArgumentNullException(nameof(context));

    /// <summary>
    /// Obtiene el registro de HC (titular para cobertura 1, familiar para cobertura 2) para el empleado indicado.
    /// </summary>
    /// <param name="ci">La cédula de identidad del empleado.</param>
    /// <param name="cover">El valor de cobertura consultado (10000.00 o 20000.00).</param>
    /// <returns>Un resultado con el registro de HC. Si no existe, el resultado indica un error de tipo NotFound.</returns>
    public async Task<Result<ConsultaHc>> GetConsultaHcAsync(string ci, decimal cover)
    {
        try
        {
            var registros = await _context.ConsultaHc
                .FromSqlRaw(
                    ProfitSqlTemplates.GetConsultaHC,
                    new SqlParameter("@CiEmpleado", ci),
                    new SqlParameter("@Cover", cover))
                .ToListAsync();

            var registro = registros.FirstOrDefault();

            if (registro == null)
            {
                return Result.Fail<ConsultaHc>("Sin registros", ErrorTypeEnum.NotFound);
            }

            return Result.Success(registro);
        }
        catch (Exception ex)
        {
            return Result.Fail<ConsultaHc>(ex.Message, ErrorTypeEnum.Conflict);
        }
    }
}
