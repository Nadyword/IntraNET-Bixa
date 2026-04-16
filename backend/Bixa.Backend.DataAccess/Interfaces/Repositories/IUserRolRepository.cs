using Bixa.Backend.DataAccess.Entities;
using Bixa.Backend.Models.Response;

namespace Bixa.Backend.DataAccess.Interfaces.Repositories;

public interface IUserRolRepository
{
    /// <summary>
    /// Retrieves a paginated list of Rol entities.
    /// </summary>
    /// <returns>A paginated list of Rol entities.</returns>
    Task<PaginatedResult<UserRol>> GetAllRoles();
}