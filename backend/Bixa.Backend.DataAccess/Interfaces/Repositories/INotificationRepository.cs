using Bixa.Backend.DataAccess.Entities;

namespace Bixa.Backend.DataAccess.Interfaces.Repositories;

public interface INotificationRepository : IRepository<Notifications, int>
{
    Task<bool> DeleteAllAsync(int userId);

    Task<IEnumerable<Notifications>> GetUnreadNotificationsForUserAsync(int userId);

    Task<bool> MarkAllAsReadAsync(int userId);
}