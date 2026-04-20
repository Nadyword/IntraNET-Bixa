using Bixa.Backend.DataAccess.Context;
using Bixa.Backend.DataAccess.Entities;
using Bixa.Backend.DataAccess.Interfaces.Repositories;
using Bixa.Backend.Models.DTOs.KeyValuePairModelDTO;
using Bixa.Backend.Models.Query;
using Bixa.Backend.Models.Response;
using Bixa.Backend.Models.Utilities;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Bixa.Backend.DataAccess.Repository;

/// <summary>
/// Provides data access operations for User entities.
/// </summary>
public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserRepository"/> class.
    /// </summary>
    /// <param name="dbContext">The application's database context.</param>
    /// <exception cref="ArgumentNullException">Thrown if the provided database context is null.</exception>
    public UserRepository(AppDbContext dbContext)
    {
        _context = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    /// <summary>
    /// Adds a new user to the database. Marks the user for addition.
    /// </summary>
    /// <param name="entity">The user entity to add.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task<(int, bool)> AddAsync(Users entity)
    {
        await _context.Users.AddAsync(entity);
        return (entity.Id, true);
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
    /// Deletes a user from the database by their ID. Marks the user for deletion.
    /// </summary>
    /// <param name="id">The ID of the user to delete.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains true if the user was found and marked for deletion, false otherwise.</returns>
    public async Task<bool> DeleteAsync(int id)
    {
        var user = await _context.Users.Where(userAux => userAux.Id.Equals(id)).FirstOrDefaultAsync();

        if (user == null)
            return false;

        _context.Users.Remove(user);
        return true;
    }

    public async Task DeleteNotificationsFromUserAsync(int id)
    {
        var notifications = await _context.Notifications.Where(x => x.UserId == id).ToListAsync();
        _context.Notifications.RemoveRange(notifications);

        var modifiedNotifications = await _context.Notifications
        .Where(n => n.ModifiedById == id)
        .ToListAsync();

        foreach (var notification in modifiedNotifications)
        {
            notification.ModifiedById = null;
        }
        _context.Notifications.UpdateRange(modifiedNotifications);
    }

    /// <summary>
    /// Retrieves all users from the database with optional filtering and pagination.
    /// </summary>
    /// <param name="filters">An object containing filter criteria for the users.</param>
    /// <param name="pagination">Pagination parameters (page number, page size). If null, a default pagination is used.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a paginated list of users.</returns>
    public async Task<PaginatedResult<Users>> GetAllAsync(object filters, Pagination? pagination)
    {
        pagination ??= new Pagination();

        var query = _context.Users
        .Include(x => x.UserRol)
        .AsQueryable();

        query = query.ApplyFilters(filters);

        var totalCount = await query.CountAsync();

        var pagedData = await query
            .OrderByDescending(u => u.CreatedAt)
            .Skip((pagination.PageNumber - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .ToListAsync();

        return new PaginatedResult<Users>(pagedData, totalCount, pagination.PageNumber, pagination.PageSize);
    }

    /// <summary>
    /// Retrieves a list of users by their ID.
    /// </summary>
    /// <param name="id">The ID of the user(s) to retrieve.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains an enumerable collection of users.</returns>
    public async Task<IEnumerable<Users?>> GetByIdAsync(int id) =>
        await _context.Users
        .Include(x => x.UserRol)
        .Where(userAux => userAux.Id.Equals(id)).ToListAsync();

    public async Task<IEnumerable<KeyValuePairDTO<object, object>>> GetKeyValuePairsAsync(object filters, KeyFieldConfigurationDTO config)
    {
        var query = _context.Users.AsQueryable();

        // Aplicar filtros genéricos
        query = query.ApplyFilters(filters);

        var keyPropertyName = string.IsNullOrWhiteSpace(config.KeyField) ? "Id" : config.KeyField;
        var valuePropertyName = string.IsNullOrWhiteSpace(config.ValueField) ? "Name" : config.ValueField;

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
    /// <param name="taxId">The tax ID of the user.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the user if found, otherwise null.</returns>
    public async Task<Users?> GetUserByTaxIdAsync(string? taxId) =>
        await _context.Users.Include(x => x.UserRol).Where(u => u.TaxId == taxId).FirstOrDefaultAsync();

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