using Bixa.Backend.Models.DTOs.NotificationModelDTO;
using Bixa.Backend.Models.DTOs.RequestModelDTO;
using Bixa.Backend.Models.Response;

namespace Bixa.Backend.Services.Interfaces;

public interface INotificationService : IService<NotificationDTO, NotificationInsertDTO, NotificationEditDTO, string>
{
    Task<Result<int>> AddManyAsync(List<NotificationInsertDTO> notifications);

    //string BuildAdvanceStateChangeNotification(Request Request, StateEnum newState);

    /// <summary>
    /// Deletes all notifications for a specific user.
    /// </summary>
    Task<Result<bool>> DeleteAllNotificationsAsync();

    /// <summary>
    /// Marks all notifications for a specific user as read.
    /// </summary>
    Task<Result<bool>> MarkAllAsReadAsync();

    /// <summary>
    /// Marks a specific user notification as read.
    /// </summary>
    Task<Result<bool>> MarkAsReadAsync(string notificationId);

    Task<bool> SendNotificationsToMultipleUsersAsync(
    List<int> userIds,
    RequestDTO requestDto,
    Func<RequestDTO, string> notificationDescriptionBuilder);
}