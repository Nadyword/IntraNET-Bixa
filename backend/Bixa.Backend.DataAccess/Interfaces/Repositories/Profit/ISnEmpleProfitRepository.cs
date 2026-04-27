using Bixa.Backend.DataAccess.Entities.DbProfit;
using Bixa.Backend.Models.Response;

namespace Bixa.Backend.DataAccess.Interfaces.Repositories.Profit;

// Interfaz de ejemplo. Reemplazar con el repositorio real de la entidad de la BD secundaria.
// Puede extender IReadOnlyRepository<T, TKey> o definir métodos específicos según la consulta.
public interface ISnEmpleProfitRepository : IReadOnlyRepository<SnEmple, int>
{
    /// <summary>
    /// Busca en la BD secundaria si existe un empleado con la CI dada y devuelve su correo electrónico.
    /// Retorna null si no se encuentra ningún registro.
    /// </summary>
    Task<string?> GetEmailByCiAsync(string ci);

    /// <summary>
    /// Obtiene información completa del empleado asociado a la CI dada en formato JSON.
    /// </summary>
    /// <param name="ci">La C.I del empleado.</param>
    /// <returns>El objeto SnEmple si se encuentra, de lo contrario null.</returns>
    Task<Result<SnEmple>> GetFullInfoByCiAsync(string ci);
}