using Bixa.Backend.DataAccess.Entities;
using Bixa.Backend.Models.Enums;

namespace Bixa.Backend.DataAccess.Interfaces.Repositories;

public interface INotificationRepository
{
    Task AddAsync(Notifications entity);

    Task<List<Notifications>> GetRecentByUserCiAsync(string userCi, int take);

    Task<int> GetUnreadCountAsync(string userCi);

    Task<bool> MarkAsReadAsync(int notificationId, string userCi);

    Task<int> MarkAllAsReadAsync(string userCi, string? referenceType = null);

    Task<bool> DeleteAsync(int notificationId, string userCi);

    Task<int> DeleteAllAsync(string userCi);

    Task<List<string>> GetCisByRolAsync(UserRolEnum rol);
}
