using Bixa.Backend.DataAccess.Interfaces.Repositories.Profit;
using Bixa.Backend.DataAccess.Entities.DbProfit;
using Bixa.Backend.DataAccess.Templates.Profit;
using Bixa.Backend.DataAccess.Context;
using Bixa.Backend.Models.Response;
using Microsoft.EntityFrameworkCore;
using Bixa.Backend.Models.Enums;
using Microsoft.Data.SqlClient;

namespace Bixa.Backend.DataAccess.Repository.Profit;

// Interfaz de ejemplo. Reemplazar con el repositorio real de la entidad de la BD secundaria.
// Puede extender IReadOnlyRepository<T, TKey> o definir métodos específicos según la consulta.
public class VacacionesProfitRepository(ProfitDbContext context) : IVacacionesProfitRepository
{
    private readonly ProfitDbContext _context = context ?? throw new ArgumentNullException(nameof(context));

    /// <summary>
    ///     Obtiene de forma asincrónica el historial de vacaciones para un empleado especificado por su código.
    /// </summary>
    /// <remarks>Si no existe ningún registro de vacaciones para el código de empleado proporcionado, el
    /// resultado contendrá un error indicando que el usuario no fue encontrado. Si ocurre un error durante la consulta,
    /// el resultado contendrá el mensaje de error y un tipo de conflicto.</remarks>
    /// <param name="CodEmp">El código único del empleado para el que se recupera el historial de vacaciones. No puede ser nulo.</param>
    /// <returns>Un resultado que contiene una lista de objetos de vacaciones asociados al empleado. Si no se encuentra el
    /// empleado, el resultado indica un error de tipo NotFound.</returns>
    public async Task<Result<List<Vacaciones>>> GetHistorialVacaByCodEmpAsync(string CodEmp)
    {
        try
        {
            var vacaciones = await _context.Vacaciones.FromSqlRaw(ProfitSqlTemplates.GetVacacionesByCodEmp, new SqlParameter("@codEmp", CodEmp)).ToListAsync();

            if (vacaciones.Count == 0)
            {
                return Result.Fail<List<Vacaciones>>("Sin registros", ErrorTypeEnum.NotFound);
            }

            return Result.Success(vacaciones);
        }
        catch (Exception ex)
        {
            return Result.Fail<List<Vacaciones>>(ex.Message, ErrorTypeEnum.Conflict);
        }
    }
}