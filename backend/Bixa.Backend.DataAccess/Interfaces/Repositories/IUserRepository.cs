using Bixa.Backend.Models.DTOs.KeyValuePairModelDTO;
using Bixa.Backend.DataAccess.Entities;

namespace Bixa.Backend.DataAccess.Interfaces.Repositories;

/// <summary>
/// Defines the contract for data access operations for User entities.
/// </summary>
public interface IUserRepository : IRepository<Users, string>
{
    Task<int> CountAdminUsersAsync(int adminRoleId);

    Task DeleteNotificationsFromUserAsync(string ci);

    /// <summary>
    /// Retrieves a filtered list of entities projected as configurable Key-Value pairs.
    /// </summary>
    /// <param name="filters">An object containing filter criteria.</param>
    /// <param name="config">An object defining the Key and Value properties for projection.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a list of KeyValuePairDTOs.</returns>
    Task<IEnumerable<KeyValuePairDTO<object, object>>> GetKeyValuePairsAsync(object filters, KeyFieldConfigurationDTO config);

    /// <summary>
    /// Retrieves a user by their Ci.
    /// </summary>
    /// <param name="ci">The Ci of the user.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the user if found, otherwise null.</returns>
    Task<Users?> GetUserByCiAsync(string ci);

    /// <summary>
    /// Retrieves a user by their refresh token and its expiration date.
    /// </summary>
    /// <param name="token">The refresh token string?.</param>
    /// <param name="date">The expiration date of the refresh token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the user if found and the token is valid, otherwise null.</returns>
    Task<Users?> GetUserByRefreshTokenAsync(string token, DateTime date);

    /// <summary>
    /// Saves the refresh token and its creation date for a specific user. Marks the user for update.
    /// </summary>
    /// <param name="token">The refresh token string to save.</param>
    /// <param name="user">The user entity to update with the new token.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task SaveRefreshTokenAsync(string token, Users user);

    /// <summary>
    /// Updates a user's password in the database. Marks the user for password update.
    /// </summary>
    /// <param name="user">The user entity containing the ID and the new password.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains true if the user was found and marked for password update, false otherwise.</returns>
    Task<bool> UpdateUserPasswordAsync(Users user);
}