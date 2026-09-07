using Bixa.Backend.DataAccess.Interfaces.Repositories.Profit;
using Bixa.Backend.DataAccess.Entities.DbProfit;
using Bixa.Backend.DataAccess.Templates.Profit;
using Bixa.Backend.DataAccess.Context;
using Bixa.Backend.Models.Response;
using Microsoft.EntityFrameworkCore;
using Bixa.Backend.Models.Enums;
using Microsoft.Data.SqlClient;

namespace Bixa.Backend.DataAccess.Repository.Profit;

public class PrestacionesSocialesProfitRepository(ProfitDbContext context) : IPrestacionesSocialesProfitRepository
{
    private readonly ProfitDbContext _context = context ?? throw new ArgumentNullException(nameof(context));

    /// <summary>
    /// Obtiene las prestaciones sociales del empleado identificado por su CI: el monto disponible y
    /// la fecha del último anticipo que solicitó.
    /// </summary>
    /// <param name="ci">La cédula de identidad del empleado.</param>
    /// <returns>Un resultado con la fila de prestaciones. Si no existe el registro, el resultado indica un error de tipo NotFound.</returns>
    public async Task<Result<PrestacionesSociales>> GetPrestacionesSocialesByCiAsync(string ci)
    {
        try
        {
            var registros = await _context.PrestacionesSociales
                .FromSqlRaw(ProfitSqlTemplates.GetPrestacionesSociales, new SqlParameter("@CiEmpleado", ci))
                .ToListAsync();

            var registro = registros.FirstOrDefault();

            if (registro == null)
            {
                return Result.Fail<PrestacionesSociales>("Sin registros", ErrorTypeEnum.NotFound);
            }

            return Result.Success(registro);
        }
        catch (Exception ex)
        {
            return Result.Fail<PrestacionesSociales>(ex.Message, ErrorTypeEnum.Conflict);
        }
    }
}
