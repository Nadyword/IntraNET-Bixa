using Bixa.Backend.DataAccess.Interfaces;
using Bixa.Backend.DataAccess.Interfaces.Repositories;
using Bixa.Backend.DataAccess.Wrappers;
using Bixa.Backend.DataAccess.Entities;
using Bixa.Backend.Models.DTOs.NotificationModelDTO;
using Bixa.Backend.Services.Interfaces;
using Bixa.Backend.Models.Response;
using Bixa.Backend.Models.Enums;
using Microsoft.Extensions.Logging;
using AutoMapper;

namespace Bixa.Backend.Services.Services;

/// <summary>
/// Servicio de notificaciones de usuario: eventos discretos persistidos (cambios de estado de
/// trámite, mensajes de chat) más contadores en vivo de acciones pendientes por rol.
/// </summary>
public class NotificationService(
    IUnitOfWork unitOfWork,
    IAprobacionesRepository aprobacionesRepository,
    ITramitesRepository tramitesRepository,
    IMapper mapper,
    LoggerWrapper loggerWrapper) : INotificationService
{
    private readonly ILogger<NotificationService> _logger = loggerWrapper.CreateLogger<NotificationService>();
    private readonly IMapper _mapper = mapper;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IAprobacionesRepository _aprobacionesRepository = aprobacionesRepository;
    private readonly ITramitesRepository _tramitesRepository = tramitesRepository;

    private const int RecentTake = 20;

    public async Task NotifyAsync(string userCi, string notificationType, string title, string message, string? referenceType = null, int? referenceId = null)
    {
        try
        {
            await _unitOfWork.Notifications.AddAsync(new Notifications
            {
                UserCi = userCi,
                NotificationType = notificationType,
                Title = title,
                Message = message,
                IsRead = false,
                ReferenceType = referenceType,
                ReferenceId = referenceId,
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear notificación para {UserCi} ({Type}): {Message}", userCi, notificationType, ex.Message);
        }
    }

    public async Task NotifyRoleAsync(UserRolEnum rol, string notificationType, string title, string message, string? referenceType = null, int? referenceId = null)
    {
        try
        {
            var cis = await _unitOfWork.Notifications.GetCisByRolAsync(rol);
            foreach (var ci in cis)
            {
                await NotifyAsync(ci, notificationType, title, message, referenceType, referenceId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al notificar al rol {Rol} ({Type}): {Message}", rol, notificationType, ex.Message);
        }
    }

    public async Task<Result<NotificationSummaryDTO>> GetSummaryAsync()
    {
        try
        {
            var userCi = _unitOfWork.GetCurrentUserCi();
            if (userCi == null)
                return Result.Fail<NotificationSummaryDTO>("Usuario no identificado.", ErrorTypeEnum.Validation);

            var rol = _unitOfWork.GetCurrentUserRol();

            var summary = new NotificationSummaryDTO
            {
                UnreadCount = await _unitOfWork.Notifications.GetUnreadCountAsync(userCi),
                Recent = _mapper.Map<List<NotificationDTO>>(await _unitOfWork.Notifications.GetRecentByUserCiAsync(userCi, RecentTake)),
            };

            if (rol is UserRolEnum.Supervisor or UserRolEnum.Administrador)
            {
                summary.PendingApprovals = (await _aprobacionesRepository.GetTramitesForAprobacion(userCi)).Count;
            }

            if (rol == UserRolEnum.Administrador)
            {
                var aprobados = await _tramitesRepository.GetAprobados();
                summary.PendingAdminApproval = aprobados.Count(t => t.Estado == EstadoTramiteEnum.Firmado);
                summary.PendingArchive = aprobados.Count(t => t.Estado == EstadoTramiteEnum.Aprobado);
            }

            return Result.Success(summary);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener el resumen de notificaciones: {Message}", ex.Message);
            return Result.Fail<NotificationSummaryDTO>("Ocurrió un error al obtener las notificaciones.", ErrorTypeEnum.Database);
        }
    }

    public async Task<Result<bool>> MarkAsReadAsync(int notificationId)
    {
        try
        {
            var userCi = _unitOfWork.GetCurrentUserCi();
            if (userCi == null)
                return Result.Fail<bool>("Usuario no identificado.", ErrorTypeEnum.Validation);

            var success = await _unitOfWork.Notifications.MarkAsReadAsync(notificationId, userCi);
            if (!success)
                return Result.Fail<bool>("Notificación no encontrada o ya leída.", ErrorTypeEnum.NotFound);

            return Result.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al marcar la notificación {Id} como leída: {Message}", notificationId, ex.Message);
            return Result.Fail<bool>("Ocurrió un error al procesar la solicitud.", ErrorTypeEnum.Database);
        }
    }

    public async Task<Result<int>> MarkAllAsReadAsync()
    {
        try
        {
            var userCi = _unitOfWork.GetCurrentUserCi();
            if (userCi == null)
                return Result.Fail<int>("Usuario no identificado.", ErrorTypeEnum.Validation);

            var count = await _unitOfWork.Notifications.MarkAllAsReadAsync(userCi);
            return Result.Success(count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al marcar todas las notificaciones como leídas: {Message}", ex.Message);
            return Result.Fail<int>("Ocurrió un error al procesar la solicitud.", ErrorTypeEnum.Database);
        }
    }

    public async Task<int> MarkAllChatNotificationsAsReadAsync()
    {
        var userCi = _unitOfWork.GetCurrentUserCi();
        if (userCi == null) return 0;

        try
        {
            return await _unitOfWork.Notifications.MarkAllAsReadAsync(userCi, "Chat");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al marcar notificaciones de chat como leídas para {UserCi}: {Message}", userCi, ex.Message);
            return 0;
        }
    }

    public async Task<Result<bool>> DeleteAsync(int notificationId)
    {
        try
        {
            var userCi = _unitOfWork.GetCurrentUserCi();
            if (userCi == null)
                return Result.Fail<bool>("Usuario no identificado.", ErrorTypeEnum.Validation);

            var success = await _unitOfWork.Notifications.DeleteAsync(notificationId, userCi);
            if (!success)
                return Result.Fail<bool>("Notificación no encontrada.", ErrorTypeEnum.NotFound);

            return Result.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar la notificación {Id}: {Message}", notificationId, ex.Message);
            return Result.Fail<bool>("Ocurrió un error al eliminar la notificación.", ErrorTypeEnum.Database);
        }
    }

    public async Task<Result<int>> DeleteAllAsync()
    {
        try
        {
            var userCi = _unitOfWork.GetCurrentUserCi();
            if (userCi == null)
                return Result.Fail<int>("Usuario no identificado.", ErrorTypeEnum.Validation);

            var count = await _unitOfWork.Notifications.DeleteAllAsync(userCi);
            return Result.Success(count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar todas las notificaciones: {Message}", ex.Message);
            return Result.Fail<int>("Ocurrió un error al eliminar las notificaciones.", ErrorTypeEnum.Database);
        }
    }
}
