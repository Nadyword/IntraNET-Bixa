using Bixa.Backend.DataAccess.Entities.DbProfit;
using Bixa.Backend.Models.Response;

namespace Bixa.Backend.DataAccess.Interfaces.Repositories.Profit;

// Interfaz de ejemplo. Reemplazar con el repositorio real de la entidad de la BD secundaria.
// Puede extender IReadOnlyRepository<T, TKey> o definir métodos específicos según la consulta.
public interface IGrupoFaProfitRepository : IReadOnlyRepository<GrupoFa, int>
{
    /// <summary>
    /// Obtiene información completa del empleado asociado a la CI dada en formato JSON.
    /// </summary>
    /// <param name="ci">La C.I del empleado.</param>
    /// <returns>El arreglo de objetos GrupoFa si se encuentra, de lo contrario null.</returns>
    Task<Result<GrupoFa[]>> GetFullInfoByCiAsync(string? ci);
}