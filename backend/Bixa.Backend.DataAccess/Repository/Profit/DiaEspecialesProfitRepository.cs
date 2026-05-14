using Bixa.Backend.DataAccess.Interfaces.Repositories.Profit;
using Bixa.Backend.DataAccess.Entities.DbProfit;
using Bixa.Backend.DataAccess.Templates.Profit;
using Bixa.Backend.DataAccess.Context;
using Bixa.Backend.Models.Response;
using Microsoft.EntityFrameworkCore;
using Bixa.Backend.Models.Enums;
using Microsoft.Data.SqlClient;

namespace Bixa.Backend.DataAccess.Repository.Profit;

public class DiaEspecialesProfitRepository(ProfitDbContext context) : IDiaEspecialesProfitRepository
{
    private readonly ProfitDbContext _context = context;

    /// <summary>
    /// Obtiene los días especiales de una empresa específica utilizando un procedimiento almacenado en la base de datos.
    /// </summary>
    /// <param name="codEmp">El código de la empresa para la que se recuperan los días especiales. No puede ser nulo.</param>
    /// <returns>Un resultado que contiene una lista de objetos de días especiales asociados a la empresa. Si no se encuentra la empresa, el resultado indica un error de tipo NotFound.</returns>
    public async Task<Result<List<DiaEspaciales>>> GetDiaEspecialesByCodEmpAsync(string codEmp)
    {
        try
        {
            var diasEspeciales = await _context.DiaEspaciales.FromSqlRaw(ProfitSqlTemplates.GetDiasEspeciales, new SqlParameter("@codEmp", codEmp)).ToListAsync();

            if (diasEspeciales.Count == 0)
            {
                return Result.Fail<List<DiaEspaciales>>("Sin registros", ErrorTypeEnum.NotFound);
            }

            return Result.Success(diasEspeciales);
        }
        catch (Exception ex)
        {
            return Result.Fail<List<DiaEspaciales>>(ex.Message, ErrorTypeEnum.Conflict);
        }
    }
}