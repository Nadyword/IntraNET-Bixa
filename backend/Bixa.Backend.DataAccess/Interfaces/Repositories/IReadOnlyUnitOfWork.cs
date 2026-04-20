using Bixa.Backend.DataAccess.Interfaces.Repositories.Proxy;

namespace Bixa.Backend.DataAccess.Interfaces.Repositories;

/// <summary>
/// Contrato de acceso a la segunda BD (solo lectura).
/// No expone SaveChangesAsync, transacciones ni campos de auditoría.
/// </summary>
public interface IReadOnlyUnitOfWork : IDisposable
{
    // Agregar una propiedad por cada repositorio de la BD secundaria.
    ISnEmpleProxyRepository SnEmple { get; }

    /// <summary>
    /// Search for a user by Tax ID
    /// </summary>
    /// <param name="taxId">The Tax ID of the user</param>
    /// <returns>Email of the user</returns>

    Task<string?> GetUserByTaxidAsync(string? taxId);
}