using System.Linq.Expressions;
using Bixa.Backend.DataAccess.Context;
using Bixa.Backend.DataAccess.Entities;
using Bixa.Backend.DataAccess.Interfaces.Repositories;
using Bixa.Backend.Models.Query;
using Bixa.Backend.Models.Response;
using Bixa.Backend.Models.Utilities;
using Microsoft.EntityFrameworkCore;

namespace Bixa.Backend.DataAccess.Repository;

public class NotificationRepository(AppDbContext context) : INotificationRepository
{
    private readonly AppDbContext _context = context;

    /// <summary>
    /// Adds a new Notification entity to the database.
    /// </summary>
    /// <param name="entity">The Notification entity to add.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a tuple where the first item is the new entity's ID (may be default before SaveChanges) and the second is a boolean indicating success.</returns>
    public async Task<(int, bool)> AddAsync(Notifications entity)
    {
        await _context.Notifications.AddAsync(entity);
        // Nota: entity.Id solo tendrá valor si la base de datos asigna el ID antes de SaveChanges (ej. GUIDs).
        // Para IDs autoincrementales, el ID se asigna después de SaveChangesAsync().
        return (entity.Id, true);
    }

    /// <summary>
    /// Adds a range of new Notification entities to the database.
    /// </summary>
    /// <param name="entities">The list of entities to add.</param>
    public async Task AddRangeAsync(IEnumerable<Notifications> entities)
    {
        await _context.Notifications.AddRangeAsync(entities);
    }

    public async Task<bool> AnyAsync(Expression<Func<Notifications, bool>> predicate) =>
            await _context.Notifications.AnyAsync(predicate);

    /// <summary>
    /// Deletes all notifications for a specific user.
    /// </summary>
    /// <param name="userId">The ID of the user whose notifications should be deleted.</param>
    /// <returns>True if notifications were found and deleted, false otherwise.</returns>
    public async Task<bool> DeleteAllAsync(int userId)
    {
        var notifications = await _context.Notifications
            .Where(n => n.UserId == userId)
            .ToListAsync();

        if (!notifications.Any())
            return false;

        _context.Notifications.RemoveRange(notifications);
        return true;
    }

    /// <summary>
    /// Deletes a UserNotification entity from the database by its ID.
    /// </summary>
    /// <param name="id">The ID of the UserNotification to delete.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains true if the entity was marked for deletion, false otherwise.</returns>
    public async Task<bool> DeleteAsync(int id)
    {
        var entity = await _context.Notifications.FirstOrDefaultAsync(n => n.Id.Equals(id));
        if (entity == null)
            return false;
        _context.Notifications.Remove(entity);
        return true;
    }

    /// <summary>
    /// Retrieves all Notification entities with optional filtering and pagination.
    /// </summary>
    /// <param name="filters">An object containing filter criteria for the entities (expected to be NotificationFilterDTO).</param>
    /// <param name="pagination">Pagination parameters (page number, page size). If null, a default pagination is used.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a paginated list of entities.</returns>
    public async Task<PaginatedResult<Notifications>> GetAllAsync(object filters, Pagination? pagination)
    {
        pagination ??= new Pagination();

        IQueryable<Notifications> query = _context.Notifications.AsQueryable();

        query = query.ApplyFilters(filters);

        var totalCount = await query.CountAsync();

        var pagedData = await query
        .OrderByDescending(sp => sp.CreatedAt)
        .Skip((pagination.PageNumber - 1) * pagination.PageSize)
        .Take(pagination.PageSize)
        .ToListAsync();

        return new PaginatedResult<Notifications>(pagedData, totalCount, pagination.PageNumber, pagination.PageSize);
    }

    /// <summary>
    /// Retrieves a Notification entity by its unique identifier, optionally including its related collections.
    /// </summary>
    /// <param name="id">The ID of the entity to retrieve.</param>
    /// <param name="includeRelated">If true, includes related entities like User, Request, and SpendingPlan.</param>
    /// <returns>A list containing the Notification entity if found, otherwise an empty list.</returns>
    public async Task<IEnumerable<Notifications>> GetByIdAsync(int id, bool includeRelated = false)
    {
        IQueryable<Notifications> query = _context.Notifications.AsQueryable();

        if (includeRelated)
        {
            query = query
                .Include(n => n.User);
        }

        var entity = await query.FirstOrDefaultAsync(n => n.Id.Equals(id));
        return entity != null ? new List<Notifications> { entity } : new List<Notifications>();
    }

    public async Task<IEnumerable<Notifications?>> GetByIdAsync(int id) =>
        await GetByIdAsync(id, includeRelated: true);

    /// <summary>
    /// Retrieves all unread notifications for a specific user.
    /// </summary>
    /// <param name="userId">The ID of the user.</param>
    /// <returns>A list of unread UserNotification entities.</returns>
    public async Task<IEnumerable<Notifications>> GetUnreadNotificationsForUserAsync(int userId)
    {
        return await _context.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Marks all notifications for a specific user as read.
    /// </summary>
    /// <param name="userId">The ID of the user whose notifications should be marked as read.</param>
    /// <returns>True if notifications were found and marked, false otherwise.</returns>
    public async Task<bool> MarkAllAsReadAsync(int userId)
    {
        var notifications = await _context.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .ToListAsync();

        if (!notifications.Any())
        {
            return false;
        }

        foreach (var notification in notifications)
        {
            notification.IsRead = true;
            _context.Notifications.Update(notification);
        }
        return true;
    }

    /// <summary>
    /// Updates an existing UserNotification entity in the database.
    /// </summary>
    /// <param name="entity">The entity to update.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains true if the entity was marked for update, false otherwise.</returns>
    public Task<bool> UpdateAsync(Notifications entity)
    {
        _context.Notifications.Update(entity);
        return Task.FromResult(true);
    }
}