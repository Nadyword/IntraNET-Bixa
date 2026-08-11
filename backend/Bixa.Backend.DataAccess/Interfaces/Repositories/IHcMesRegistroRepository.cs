using Bixa.Backend.DataAccess.Entities;

namespace Bixa.Backend.DataAccess.Interfaces.Repositories;

/// <summary>
/// Defines the contract for data access operations for HcMesRegistro entities
/// (valores declarados por mes para la prima trimestral de HC).
/// </summary>
public interface IHcMesRegistroRepository
{
    /// <summary>
    /// Obtiene el registro actual de HC (mes1/mes2/mes3) para el CI indicado, si existe.
    /// </summary>
    Task<HcMesRegistro?> GetByCiAsync(string ci);

    /// <summary>
    /// Crea o actualiza (sobrescribe) el registro actual de HC para el CI indicado.
    /// No llama a SaveChangesAsync; el llamador es responsable de persistir los cambios.
    /// </summary>
    Task<HcMesRegistro> UpsertAsync(string ci, decimal mes1, decimal mes2, decimal mes3, decimal primaTrimBs);

    /// <summary>
    /// Obtiene los últimos registros editados (por UpdatedAt descendente), incluyendo los datos del usuario.
    /// </summary>
    Task<List<HcMesRegistro>> GetRecientesAsync(int take);

    /// <summary>
    /// Busca en TODOS los registros por nombre, apellido o CI (sin límite de resultados).
    /// </summary>
    Task<List<HcMesRegistro>> BuscarAsync(string query);
}
