using Bixa.Backend.Models.DTOs.UserModelDTO;
using Bixa.Backend.DataAccess.Entities.DbProfit;
using Bixa.Backend.Models.Response;

namespace Bixa.Backend.DataAccess.Interfaces.Repositories.Profit;

// Interfaz de ejemplo. Reemplazar con el repositorio real de la entidad de la BD secundaria.
// Puede extender IReadOnlyRepository<T, TKey> o definir métodos específicos según la consulta.
public interface ISnEmpleProfitRepository : IReadOnlyRepository<SnEmple, string>
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

    /// <summary>
    ///   Obtiene de forma asíncrona una lista de empleados asociados a los usuarios especificados por grupo de CI.
    /// </summary>
    /// <param name="users">La lista de usuarios para los que se recuperarán los empleados asociados. No puede ser nula.</param>
    /// <returns>Un resultado que contiene una lista de objetos SnEmple asociados a los usuarios proporcionados. Si no se
    /// encuentran empleados, la lista estará vacía.</returns>
    Task<Result<List<SnEmple>>> GetAllByCiAsync(List<UserDTO> users);

    /// <summary>
    /// Obtiene de forma asíncrona a todo el personal a cargo (directo e indirecto) del supervisor con la CI dada.
    /// </summary>
    /// <param name="ci">La C.I. del supervisor.</param>
    /// <returns>La lista de empleados que reportan, directa o indirectamente, al supervisor.</returns>
    Task<List<EquipoSupervisor>> GetEquipoSupervisorAsync(string ci);
}