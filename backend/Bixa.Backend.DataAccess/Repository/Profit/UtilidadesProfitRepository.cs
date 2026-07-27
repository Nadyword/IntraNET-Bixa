using Bixa.Backend.DataAccess.Interfaces.Repositories.Profit;
using Bixa.Backend.DataAccess.Entities.DbProfit;
using Bixa.Backend.DataAccess.Templates.Profit;
using Bixa.Backend.DataAccess.Context;
using Bixa.Backend.Models.Response;
using Microsoft.EntityFrameworkCore;
using Bixa.Backend.Models.Enums;
using Microsoft.Data.SqlClient;

namespace Bixa.Backend.DataAccess.Repository.Profit;

public class UtilidadesProfitRepository(ProfitDbContext context) : IUtilidadesProfitRepository
{
    private readonly ProfitDbContext _context = context ?? throw new ArgumentNullException(nameof(context));

    /// <summary>
    /// Obtiene el monto de utilidades disponible para el empleado identificado por su CI.
    /// </summary>
    /// <param name="ci">La cédula de identidad del empleado.</param>
    /// <returns>Un resultado con el monto disponible. Si no existe el registro, el resultado indica un error de tipo NotFound.</returns>
    public async Task<Result<decimal?>> GetMontoDisponibleByCiAsync(string ci)
    {
        try
        {
            var registro = await _context.Utilidades
                .FromSqlRaw(ProfitSqlTemplates.GetUtilidades, new SqlParameter("@ci", ci))
                .FirstOrDefaultAsync();

            if (registro == null)
            {
                return Result.Fail<decimal?>("Sin registros", ErrorTypeEnum.NotFound);
            }

            return Result.Success(registro.MontoDisponible);
        }
        catch (Exception ex)
        {
            return Result.Fail<decimal?>(ex.Message, ErrorTypeEnum.Conflict);
        }
    }
}
