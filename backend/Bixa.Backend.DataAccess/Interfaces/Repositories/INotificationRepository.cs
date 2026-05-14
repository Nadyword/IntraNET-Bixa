using Bixa.Backend.DataAccess.Entities;

namespace Bixa.Backend.DataAccess.Interfaces.Repositories;

public interface INotificationRepository : IRepository<Notifications, string>
{
    Task<bool> DeleteAllAsync(string userId);

    Task<IEnumerable<Notifications>> GetUnreadNotificationsForUserAsync(string userCi);

    Task<bool> MarkAllAsReadAsync(string userCi);
}