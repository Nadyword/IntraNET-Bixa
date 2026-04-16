using Bixa.Backend.Models.Response;
using Bixa.Backend.Models.Query;
using Bixa.Backend.Models.DTOs.NotificationModelDTO;
using Bixa.Backend.Models.DTOs.RequestModelDTO;

namespace Bixa.Backend.Services.Interfaces;

public interface INotificationService : IService<NotificationDTO, NotificationInsertDTO, NotificationEditDTO, NotificationFilterDTO, int>
{
    Task<Result<int>> AddManyAsync(List<NotificationInsertDTO> notifications);

    //string BuildAdvanceStateChangeNotification(Request Request, StateEnum newState);

    /// <summary>
    /// Deletes all notifications for a specific user.
    /// </summary>
    Task<Result<bool>> DeleteAllNotificationsAsync();

    /// <summary>
    /// Retrieves a paginated list of notifications for a specific user.
    /// </summary>
    Task<Result<PaginatedResult<NotificationDTO>>> GetNotificationsByUserIdAsync(int userId, SearchQuery<NotificationFilterDTO> filters);

    //Task<List<int>> GetUsersToNotifyRequestAsync(Request request, StateEnum newState);

    /// <summary>
    /// Marks all notifications for a specific user as read.
    /// </summary>
    Task<Result<bool>> MarkAllAsReadAsync();

    /// <summary>
    /// Marks a specific user notification as read.
    /// </summary>
    Task<Result<bool>> MarkAsReadAsync(int notificationId);

    Task<bool> SendNotificationsToMultipleUsersAsync(
    List<int> userIds,
    RequestDTO requestDto,
    Func<RequestDTO, string> notificationDescriptionBuilder);
}