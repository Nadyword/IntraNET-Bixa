using Bixa.Backend.Models.DTOs.KeyValuePairModelDTO;
using Bixa.Backend.DataAccess.Interfaces.Repositories;
using Bixa.Backend.DataAccess.Context;
using Bixa.Backend.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;
using Bixa.Backend.Models.Utilities;
using System.Linq.Expressions;

namespace Bixa.Backend.DataAccess.Repository;

/// <summary>
/// Provides data access operations for User entities.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="UserRepository"/> class.
/// </remarks>
/// <param name="dbContext">The application's database context.</param>
/// <exception cref="ArgumentNullException">Thrown if the provided database context is null.</exception>
public class UserRepository(AppDbContext dbContext) : IUserRepository
{
    private readonly AppDbContext _context = dbContext ?? throw new ArgumentNullException(nameof(dbContext));

    /// <summary>
    /// Adds a new user to the database. Marks the user for addition.
    /// </summary>
    /// <param name="entity">The user entity to add.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task<(string, bool)> AddAsync(Users entity)
    {
        await _context.Users.AddAsync(entity);
        return (entity.Ci, true);
    }

    /// <summary>
    /// Adds a range of new Users entities to the database.
    /// </summary>
    /// <param name="entities">The list of entities to add.</param>
    public async Task AddRangeAsync(IEnumerable<Users> entities)
    {
        await _context.Users.AddRangeAsync(entities);
    }

    public async Task<bool> AnyAsync(Expression<Func<Users, bool>> predicate) =>
            await _context.Users.AnyAsync(predicate);

    /// <summary>
    /// Cuenta el número total de usuarios con el rol de Administrador.
    /// </summary>
    /// <param name="adminRoleId">El ID del rol de Administrador.</param>
    /// <returns>El número total de administradores.</returns>
    public async Task<int> CountAdminUsersAsync(int adminRoleId) =>
        await _context.Users.CountAsync(u => u.IdUserRol == adminRoleId);

    /// <summary>
    /// Recupera todos los usuarios activos que poseen el rol indicado.
    /// </summary>
    /// <param name="roleId">El ID del rol a filtrar.</param>
    /// <returns>La lista de usuarios activos con ese rol.</returns>
    public async Task<List<Users>> GetActiveByRoleAsync(int roleId) =>
        await _context.Users
            .Where(u => u.IdUserRol == roleId && u.IsActive)
            .ToListAsync();

    /// <summary>
    /// Deletes a user from the database by their ID. Marks the user for deletion.
    /// </summary>
    /// <param name="ci">The CI of the user to delete.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains true if the user was found and marked for deletion, false otherwise.</returns>
    public async Task<bool> DeleteAsync(string ci)
    {
        var user = await _context.Users.Where(userAux => userAux.Ci == ci).FirstOrDefaultAsync();

        if (user == null)
            return false;

        _context.Users.Remove(user);
        return true;
    }

    /// <summary>
    /// Determina si el usuario tiene aprobaciones asociadas como aprobador.
    /// </summary>
    /// <param name="ci">La cédula del usuario.</param>
    /// <returns>True si el usuario tiene al menos una aprobación asociada.</returns>
    public async Task<bool> HasAprobacionesAsync(string ci) =>
        await _context.Aprobaciones.AnyAsync(a => a.AprobadorCi == ci);

    public async Task DeleteNotificationsFromUserAsync(string ci)
    {
        var notifications = await _context.Notifications.Where(x => x.User!.Ci == ci).ToListAsync();
        _context.Notifications.RemoveRange(notifications);

        var modifiedNotifications = await _context.Notifications
        .Where(n => n.ModifiedByCi == ci)
        .ToListAsync();

        foreach (var notification in modifiedNotifications)
        {
            notification.ModifiedByCi = null;
        }
        _context.Notifications.UpdateRange(modifiedNotifications);
    }

    /// <summary>
    /// Retrieves all users from the database with optional filtering and pagination.
    /// </summary>
    /// <param name="pageNumber">The page number to retrieve. Default is 1.</param>
    /// <param name="pageSize">The number of users to return per page. Default is 10.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a paginated list of users.</returns>
    public async Task<List<Users>> GetAllAsync(int pageNumber = 1, int pageSize = 10)
    {
        int page = (pageNumber * pageSize) - pageSize;
        return await _context.Users
          .OrderBy(u => u.Id)
          .Skip(page)
          .Take(pageSize)
          .ToListAsync();
    }

    /// <summary>
    /// Retrieves a list of users by their ID.
    /// </summary>
    /// <param name="ci">The cedula of the user(s) to retrieve.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains an enumerable collection of users.</returns>
    public async Task<IEnumerable<Users?>> GetByCiAsync(string ci) =>
        await _context.Users
        .Include(x => x.UserRol)
        .Where(userAux => userAux.Ci == ci).ToListAsync();

    public async Task<IEnumerable<KeyValuePairDTO<object, object>>> GetKeyValuePairsAsync(object filters, KeyFieldConfigurationDTO config)
    {
        var query = _context.Users.AsQueryable();

        // Aplicar filtros genéricos
        query = query.ApplyFilters(filters);

        var projectedQuery = query.ApplyKeyValueProjection(config);

        return await projectedQuery.ToListAsync();
    }

    /// <summary>
    /// Retrieves a user by their refresh token and its expiration date.
    /// </summary>
    /// <param name="token">The refresh token string?.</param>
    /// <param name="date">The expiration date of the refresh token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the user if found and the token is valid, otherwise null.</returns>
    public async Task<Users?> GetUserByRefreshTokenAsync(string? token, DateTime date) =>
        await _context.Users.Include(x => x.UserRol).Where(u => u.RefreshTokenDate <= date && u.RefreshToken == token).FirstOrDefaultAsync();

    /// <summary>
    /// Retrieves a user by their tax ID.
    /// </summary>
    /// <param name="ci">The tax ID of the user.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the user if found, otherwise null.</returns>
    public async Task<Users?> GetUserByCiAsync(string ci) =>
        await _context.Users.Include(x => x.UserRol).Where(u => u.Ci == ci).FirstOrDefaultAsync();

    /// <summary>
    /// Saves the refresh token and its creation date for a specific user. Marks the user for update.
    /// </summary>
    /// <param name="token">The refresh token string? to save.</param>
    /// <param name="user">The user entity to update with the new token.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public Task SaveRefreshTokenAsync(string? token, Users user)
    {
        user.RefreshToken = token;
        user.RefreshTokenDate = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc);
        _context.Users.Update(user);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Updates an existing user in the database. Marks the user for update.
    /// </summary>
    /// <param name="user">The user entity to update.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public Task<bool> UpdateAsync(Users user)
    {
        _context.Users.Update(user);
        return Task.FromResult(true);
    }

    /// <summary>
    /// Updates a user's password in the database. Marks the user for password update.
    /// </summary>
    /// <param name="user">The user entity containing the ID and the new password.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains true if the user was found and marked for password update, false otherwise.</returns>
    public async Task<bool> UpdateUserPasswordAsync(Users user)
    {
        var response = await _context.Users.Include(x => x.UserRol).Where(userAux => userAux.Id.Equals(user.Id)).FirstOrDefaultAsync();

        if (response != null)
        {
            response.PasswordHash = user.PasswordHash;
            _context.Users.Update(response);
            return true;
        }
        return false;
    }
}