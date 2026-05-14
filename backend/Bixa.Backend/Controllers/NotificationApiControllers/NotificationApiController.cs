using Bixa.Backend.Models.DTOs.NotificationModelDTO;
using Bixa.Backend.DataAccess.Wrappers;
using Microsoft.AspNetCore.Authorization;
using Bixa.Backend.Services.Interfaces;
using Bixa.Backend.Models.Response;
using Bixa.Backend.Models.Query;
using Microsoft.AspNetCore.Mvc;
using Bixa.Backend.Models;
using Bixa.Backend.Base;
using AutoMapper;

namespace Bixa.Backend.Controllers.NotificationApiControllers;

/// <summary>
/// Initializes a new instance of the <see cref="NotificationApiController"/> class.
/// </summary>
/// <param name="mapper">AutoMapper instance for DTO conversions.</param>
/// <param name="loggerWrapper">Logger wrapper instance for logging.</param>
/// <param name="notificationService">The user notification service instance for business logic.</param>
[ApiController]
[Route("api/notificationsApi")]
public class NotificationApiController(
    IMapper mapper,
    LoggerWrapper loggerWrapper,
    INotificationService notificationService
        ) : BaseApiController(mapper, loggerWrapper)
{
    private readonly INotificationService _notificationService = notificationService;

    /// <summary>
    /// Deletes a specific user notification for the current user.
    /// </summary>
    /// <param name="notificationId">The ID of the notification to delete.</param>
    /// <returns>An <see cref="IActionResult"/> indicating the operation result (NoContent on success).</returns>
    [HttpDelete("{notificationId:int}")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Delete(string notificationId)
    {
        var result = await _notificationService.DeleteAsync(notificationId);
        return HandleServiceResult(result);
    }

    /// <summary>
    /// Deletes all notifications for the currently authenticated user.
    /// </summary>
    /// <returns>An <see cref="IActionResult"/> indicating the operation result (NoContent on success).</returns>
    [HttpDelete("all")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteAllNotifications()
    {
        var result = await _notificationService.DeleteAllNotificationsAsync();
        return HandleServiceResult(result);
    }

    /// <summary>
    /// Marks all unread notifications for the current user as read.
    /// </summary>
    /// <returns>An <see cref="IActionResult"/> indicating the success or failure.</returns>
    [HttpPut("mark-all-as-read")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<int>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> MarkAllAsRead()
    {
        var result = await _notificationService.MarkAllAsReadAsync();
        return HandleServiceResult(result, $"Se marcaron exitosamente {result.Value} notificaciones como leídas.");
    }

    /// <summary>
    /// Marks a specific user notification as read for the current user.
    /// </summary>
    /// <param name="notificationId">The ID of the notification to mark as read.</param>
    /// <returns>An <see cref="IActionResult"/> indicating the success or failure.</returns>
    [HttpPut("{notificationId:int}/mark-as-read")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> MarkAsRead(string notificationId)
    {
        var result = await _notificationService.MarkAsReadAsync(notificationId);
        return HandleServiceResult(result, "Notificación marcada como leída exitosamente.");
    }
}