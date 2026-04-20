using Bixa.Backend.DataAccess.Entities.DbProxy;

namespace Bixa.Backend.DataAccess.Interfaces.Repositories.Proxy;

// Interfaz de ejemplo. Reemplazar con el repositorio real de la entidad de la BD secundaria.
// Puede extender IReadOnlyRepository<T, TKey> o definir métodos específicos según la consulta.
public interface ISnEmpleProxyRepository : IReadOnlyRepository<SnEmple, int>
{
    /// <summary>
    /// Busca en la BD secundaria si existe un empleado con la CI dada y devuelve su correo electrónico.
    /// Retorna null si no se encuentra ningún registro.
    /// </summary>
    Task<string?> GetEmailByCiAsync(string? ci);
}