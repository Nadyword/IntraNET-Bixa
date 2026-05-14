using Bixa.Backend.DataAccess.Interfaces.Repositories;
using Bixa.Backend.DataAccess.Context;
using Bixa.Backend.DataAccess.Entities;
using Bixa.Backend.Models.Response;
using Microsoft.EntityFrameworkCore;
using Bixa.Backend.Models.Utilities;
using Bixa.Backend.Models.Query;
using System.Linq.Expressions;

namespace Bixa.Backend.DataAccess.Repository;

public class NotificationRepository(AppDbContext context) : INotificationRepository
{
    private readonly AppDbContext _context = context;

    /// <summary>
    /// Adds a new Notification entity to the database.
    /// </summary>
    /// <param name="entity">The Notification entity to add.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a tuple where the first item is the new entity's ID (may be default before SaveChanges) and the second is a boolean indicating success.</returns>
    public async Task<(string, bool)> AddAsync(Notifications entity)
    {
        await _context.Notifications.AddAsync(entity);
        // Nota: entity.Id solo tendrá valor si la base de datos asigna el ID antes de SaveChanges (ej. GUIDs).
        // Para IDs autoincrementales, el ID se asigna después de SaveChangesAsync().
        return (entity.UserCi, true);
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
    /// <param name="userCi">The CI of the user whose notifications should be deleted.</param>
    /// <returns>True if notifications were found and deleted, false otherwise.</returns>
    public async Task<bool> DeleteAllAsync(string userCi)
    {
        var notifications = await _context.Notifications
            .Where(n => n.User!.Ci == userCi)
            .ToListAsync();

        if (notifications.Count == 0)
            return false;

        _context.Notifications.RemoveRange(notifications);
        return true;
    }

    /// <summary>
    /// Deletes a UserNotification entity from the database by its ID.
    /// </summary>
    /// <param name="id">The ID of the UserNotification to delete.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains true if the entity was marked for deletion, false otherwise.</returns>
    public async Task<bool> DeleteAsync(string id)
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
    /// <param name="pageSize">The number of items per page for pagination (default is 10).</param>
    /// <param name="pageNumber">The page number for pagination (default is 1).</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a paginated list of entities.</returns>
    public async Task<List<Notifications>> GetAllAsync(int pageNumber, int pageSize)
    {
        int page = (pageNumber * pageSize) - pageSize;
        var result = _context.Notifications
          .Include(x => x.UserCi)
          .OrderBy(u => u.Id)
          .Skip(page)
          .Take(pageSize);

        return await result
            .OrderByDescending(u => u.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    /// <summary>
    /// Retrieves a Notification entity by its unique identifier, optionally including its related collections.
    /// </summary>
    /// <param name="ci">The ID of the entity to retrieve.</param>
    /// <param name="includeRelated">If true, includes related entities like User, Request, and SpendingPlan.</param>
    /// <returns>A list containing the Notification entity if found, otherwise an empty list.</returns>
    public async Task<IEnumerable<Notifications>> GetByCiAsync(string ci, bool includeRelated = false)
    {
        IQueryable<Notifications> query = _context.Notifications.AsQueryable();

        if (includeRelated)
        {
            query = query
                .Include(n => n.User);
        }

        var entity = await query.FirstOrDefaultAsync(n => n.User!.Ci.Equals(ci));
        return entity != null ? [entity] : new List<Notifications>();
    }

    public async Task<IEnumerable<Notifications?>> GetByCiAsync(string ci) =>
        await GetByCiAsync(ci, includeRelated: true);

    /// <summary>
    /// Retrieves all unread notifications for a specific user.
    /// </summary>
    /// <param name="userId">The ID of the user.</param>
    /// <returns>A list of unread UserNotification entities.</returns>
    public async Task<IEnumerable<Notifications>> GetUnreadNotificationsForUserAsync(string userId)
    {
        return await _context.Notifications
            .Where(n => n.User!.Ci == userId && !n.IsRead)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Marks all notifications for a specific user as read.
    /// </summary>
    /// <param name="userCi">The CI of the user whose notifications should be marked as read.</param>
    /// <returns>True if notifications were found and marked, false otherwise.</returns>
    public async Task<bool> MarkAllAsReadAsync(string userCi)
    {
        var notifications = await _context.Notifications
            .Where(n => n.User!.Ci == userCi && !n.IsRead)
            .ToListAsync();

        if (notifications.Count == 0)
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