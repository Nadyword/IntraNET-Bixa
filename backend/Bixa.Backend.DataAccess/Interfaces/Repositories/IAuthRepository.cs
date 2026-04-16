using Bixa.Backend.DataAccess.Entities;

namespace Bixa.Backend.DataAccess.Interfaces.Repositories;

/// <summary>
/// Defines the contract for user data access operations specific to authentication.
/// </summary>
public interface IAuthRepository
{
    /// <summary>
    /// Retrieves a user by TaxID
    /// </summary>
    /// <param name="email">User TaxID.</param>
    /// <returns>The User entity if found, otherwise null.</returns>
    Task<Users?> GetUserByTaxId(string email);

    /// <summary>
    /// Retrieves a user by refresh token and date.
    /// </summary>
    /// <param name="token">Refresh token.</param>
    /// <param name="date">Refresh token date.</param>
    /// <returns>The User entity if found, otherwise null.</returns>
    Task<Users?> GetUserByRefreshToken(string token, DateTime date);

    /// <summary>
    /// Updates a user entity (e.g., setting refresh token details).
    /// Note: This method only marks the entity for update; SaveChanges must be called via UnitOfWork.
    /// </summary>
    /// <param name="user">The user entity to update.</param>
    Task SaveRefreshToken(Users user);
}