using Bixa.Backend.DataAccess.Entities;

namespace Bixa.Backend.DataAccess.Interfaces.Repositories;

public interface ITramitesRepository
{
    /// <summary>
    /// Obtiene todos los trámites de un usuario específico, identificados por su CI.
    /// </summary>
    /// <param name="ci">La cédula de identidad del usuario.</param>
    /// <returns>Una lista de trámites del usuario.</returns>
    Task<List<Tramite>> GetTramitesByCi(string ci);

    /// <summary>
    /// Obtiene todos los trámites que no estén finalizados.
    /// </summary>
    /// <returns>Una lista de trámites no finalizados.</returns>
    Task<List<Tramite>> GetAllTramites();

    /// <summary>
    /// Obtiene todos los trámites que han sido aprobados.
    /// </summary>
    /// <returns>Una lista de trámites aprobados.</returns>
    Task<List<Tramite>> GetAprobados();

    /// <summary>
    /// Obtiene un trámite específico por su ID.
    /// </summary>
    /// <param name="tramiteId">El ID del trámite.</param>
    /// <returns>El trámite correspondiente al ID proporcionado.</returns>
    Task<Tramite> GetTramiteById(int tramiteId);
}