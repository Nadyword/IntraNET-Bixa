using Bixa.Backend.Models.DTOs.NotificationModelDTO;
using Bixa.Backend.Models.Enums;
using Bixa.Backend.Models.Response;

namespace Bixa.Backend.Services.Interfaces;

public interface INotificationService
{
    /// <summary>
    /// Crea una notificación para un usuario específico. Falla en silencio (solo loguea) para no
    /// interrumpir la operación de negocio que la dispara.
    /// </summary>
    Task NotifyAsync(string userCi, string notificationType, string title, string message, string? referenceType = null, int? referenceId = null);

    /// <summary>
    /// Crea la misma notificación para todos los usuarios con el rol indicado.
    /// </summary>
    Task NotifyRoleAsync(UserRolEnum rol, string notificationType, string title, string message, string? referenceType = null, int? referenceId = null);

    /// <summary>
    /// Resumen de notificaciones para el usuario autenticado actual: no leídas persistidas
    /// más los contadores en vivo aplicables a su rol.
    /// </summary>
    Task<Result<NotificationSummaryDTO>> GetSummaryAsync();

    Task<Result<bool>> MarkAsReadAsync(int notificationId);

    Task<Result<int>> MarkAllAsReadAsync();

    /// <summary>
    /// Marca como leídas las notificaciones de tipo "Chat" del usuario autenticado actual.
    /// Usado cuando abre un hilo de soporte (como remitente o como agente).
    /// </summary>
    Task<int> MarkAllChatNotificationsAsReadAsync();

    Task<Result<bool>> DeleteAsync(int notificationId);

    Task<Result<int>> DeleteAllAsync();
}
