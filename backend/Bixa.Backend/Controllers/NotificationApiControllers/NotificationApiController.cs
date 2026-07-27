using Bixa.Backend.DataAccess.Wrappers;
using Microsoft.AspNetCore.Authorization;
using Bixa.Backend.Services.Interfaces;
using Bixa.Backend.Models.Response;
using Microsoft.AspNetCore.Mvc;
using Bixa.Backend.Models;
using Bixa.Backend.Base;
using AutoMapper;

namespace Bixa.Backend.Controllers.NotificationApiControllers;

/// <summary>
/// Expone el resumen y la gestión de notificaciones del usuario autenticado.
/// </summary>
[ApiController]
[Route("api/notificationsApi")]
[Authorize]
public class NotificationApiController(
    IMapper mapper,
    LoggerWrapper loggerWrapper,
    INotificationService notificationService
        ) : BaseApiController(mapper, loggerWrapper)
{
    private readonly INotificationService _notificationService = notificationService;

    /// <summary>
    /// Resumen de notificaciones del usuario autenticado: no leídas persistidas más los
    /// contadores en vivo de acciones pendientes aplicables a su rol.
    /// </summary>
    [HttpGet("Summary")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetSummary()
    {
        var result = await _notificationService.GetSummaryAsync();
        return HandleServiceResult(result);
    }

    /// <summary>
    /// Marca una notificación específica como leída.
    /// </summary>
    [HttpPut("{notificationId:int}/MarkAsRead")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> MarkAsRead(int notificationId)
    {
        var result = await _notificationService.MarkAsReadAsync(notificationId);
        return HandleServiceResult(result, "Notificación marcada como leída.");
    }

    /// <summary>
    /// Marca todas las notificaciones del usuario autenticado como leídas.
    /// </summary>
    [HttpPut("MarkAllAsRead")]
    [ProducesResponseType(typeof(ApiResponse<int>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> MarkAllAsRead()
    {
        var result = await _notificationService.MarkAllAsReadAsync();
        return HandleServiceResult(result, "Notificaciones marcadas como leídas.");
    }

    /// <summary>
    /// Elimina una notificación específica del usuario autenticado.
    /// </summary>
    [HttpDelete("{notificationId:int}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Delete(int notificationId)
    {
        var result = await _notificationService.DeleteAsync(notificationId);
        return HandleServiceResult(result);
    }

    /// <summary>
    /// Elimina todas las notificaciones del usuario autenticado.
    /// </summary>
    [HttpDelete("all")]
    [ProducesResponseType(typeof(ApiResponse<int>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteAll()
    {
        var result = await _notificationService.DeleteAllAsync();
        return HandleServiceResult(result);
    }
}
