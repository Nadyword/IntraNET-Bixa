using Bixa.Backend.DataAccess.Interfaces.Repositories;
using Bixa.Backend.DataAccess.Context;
using Bixa.Backend.DataAccess.Entities;
using Bixa.Backend.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace Bixa.Backend.DataAccess.Repository;

public class NotificationRepository(AppDbContext context) : INotificationRepository
{
    private readonly AppDbContext _context = context;

    public async Task AddAsync(Notifications entity)
    {
        await _context.Notifications.AddAsync(entity);
        await _context.SaveChangesAsync();
    }

    public async Task<List<Notifications>> GetRecentByUserCiAsync(string userCi, int take)
    {
        return await _context.Notifications
            .Where(n => n.UserCi == userCi)
            .OrderByDescending(n => n.CreatedAt)
            .Take(take)
            .ToListAsync();
    }

    public async Task<int> GetUnreadCountAsync(string userCi)
    {
        return await _context.Notifications
            .CountAsync(n => n.UserCi == userCi && !n.IsRead);
    }

    public async Task<bool> MarkAsReadAsync(int notificationId, string userCi)
    {
        return await _context.Notifications
            .Where(n => n.Id == notificationId && n.UserCi == userCi && !n.IsRead)
            .ExecuteUpdateAsync(s => s
                .SetProperty(n => n.IsRead, true)
                .SetProperty(n => n.ReadAt, DateTime.UtcNow)) > 0;
    }

    public async Task<int> MarkAllAsReadAsync(string userCi, string? referenceType = null)
    {
        var query = _context.Notifications
            .Where(n => n.UserCi == userCi && !n.IsRead);

        if (referenceType != null)
            query = query.Where(n => n.ReferenceType == referenceType);

        return await query.ExecuteUpdateAsync(s => s
            .SetProperty(n => n.IsRead, true)
            .SetProperty(n => n.ReadAt, DateTime.UtcNow));
    }

    public async Task<bool> DeleteAsync(int notificationId, string userCi)
    {
        return await _context.Notifications
            .Where(n => n.Id == notificationId && n.UserCi == userCi)
            .ExecuteDeleteAsync() > 0;
    }

    public async Task<int> DeleteAllAsync(string userCi)
    {
        return await _context.Notifications
            .Where(n => n.UserCi == userCi)
            .ExecuteDeleteAsync();
    }

    public async Task<List<string>> GetCisByRolAsync(UserRolEnum rol)
    {
        return await _context.Users
            .Where(u => u.IdUserRol == (int)rol)
            .Select(u => u.Ci)
            .ToListAsync();
    }
}
