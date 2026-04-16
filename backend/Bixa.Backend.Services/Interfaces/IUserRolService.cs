using Bixa.Backend.Models.DTOs.UserRolDTO;
using Bixa.Backend.Models.Response;

namespace Bixa.Backend.Services.Interfaces;

/// <summary>
/// Defines the contract for user role-related business logic.
/// </summary>
public interface IUserRolService
{
    /// <summary>
    /// Retrieves all user roles from the repository with pagination and mapping.
    /// </summary>
    /// <returns>A <see cref="Result{T}"/> containing a paginated list of <see cref="UserRolDTO"/>.</returns>
    Task<Result<PaginatedResult<UserRolDTO>>> GetAllUserRolsAsync();
}