using Bixa.Backend.DataAccess.Interfaces.Repositories.Profit;
using Bixa.Backend.DataAccess.Entities.DbProfit;
using Bixa.Backend.DataAccess.Templates.Profit;
using Bixa.Backend.DataAccess.Context;
using Bixa.Backend.Models.Response;
using Microsoft.EntityFrameworkCore;
using Bixa.Backend.Models.Enums;
using Microsoft.Data.SqlClient;

namespace Bixa.Backend.DataAccess.Repository.Profit;

public class AriProfitRepository(ProfitDbContext context) : IAriProfitRepository
{
    private readonly ProfitDbContext _context = context ?? throw new ArgumentNullException(nameof(context));

    /// <summary>
    /// Obtiene los datos base de la planilla AR-I del empleado identificado por su CI.
    /// </summary>
    /// <param name="ci">La cédula de identidad del empleado.</param>
    /// <returns>Un resultado con la fila de datos. Si el empleado no existe, el resultado indica un error de tipo NotFound.</returns>
    public async Task<Result<AriProfit>> GetAriByCiAsync(string ci)
    {
        try
        {
            var registros = await _context.Ari
                .FromSqlRaw(ProfitSqlTemplates.GetARI, new SqlParameter("@ciEmplea", ci))
                .ToListAsync();

            var registro = registros.FirstOrDefault();

            if (registro == null)
            {
                return Result.Fail<AriProfit>("Sin registros", ErrorTypeEnum.NotFound);
            }

            return Result.Success(registro);
        }
        catch (Exception ex)
        {
            return Result.Fail<AriProfit>(ex.Message, ErrorTypeEnum.Conflict);
        }
    }
}
