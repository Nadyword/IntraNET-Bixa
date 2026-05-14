using Bixa.Backend.DataAccess.Interfaces.Repositories;
using Bixa.Backend.Models.DTOs.NotificationModelDTO;
using Bixa.Backend.Models.DTOs.RequestModelDTO;
using Bixa.Backend.DataAccess.Wrappers;
using Bixa.Backend.DataAccess.Entities;
using Bixa.Backend.Services.Interfaces;
using Bixa.Backend.Models.Response;
using Bixa.Backend.Models.Enums;
using Microsoft.Extensions.Logging;
using Bixa.Backend.Models.Query;
using AutoMapper;

namespace Bixa.Backend.Services.Services;

/// <summary>
/// Service for managing user notification business logic and operations.
/// </summary>
public class NotificationService(IUnitOfWork unitOfWork, IMapper mapper, LoggerWrapper loggerWrapper) : INotificationService
{
    private readonly ILogger<NotificationService> _logger = loggerWrapper.CreateLogger<NotificationService>();
    private readonly IMapper _mapper = mapper;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    /// <summary>
    /// Adds a new user notification to the system.
    /// </summary>
    public async Task<Result<string>> AddAsync(NotificationInsertDTO dto)
    {
        try
        {
            var notificationEntity = _mapper.Map<NotificationInsertDTO, Notifications>(dto);
            await _unitOfWork.Notifications.AddAsync(notificationEntity);

            var saveChangesSuccess = await _unitOfWork.SaveChangesAsync() > 0;

            if (saveChangesSuccess)
                return Result.Success(notificationEntity.UserCi);
            else
                return Result.Fail<string>("No se pudo insertar la notificación de usuario. No se guardaron cambios.", ErrorTypeEnum.General);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while adding user notification: {Message}", ex.Message);
            return Result.Fail<string>("Ocurrió un error inesperado al procesar la notificación.", ErrorTypeEnum.Database);
        }
    }

    /// <summary>
    /// Adds multiple user notifications to the system.
    /// </summary>
    public async Task<Result<int>> AddManyAsync(List<NotificationInsertDTO> notifications)
    {
        try
        {
            var notificationEntities = notifications
                .ConvertAll(dto => _mapper.Map<NotificationInsertDTO, Notifications>(dto))
;

            await _unitOfWork.Notifications.AddRangeAsync(notificationEntities);
            var saveChangesSuccess = await _unitOfWork.SaveChangesAsync() > 0;

            if (saveChangesSuccess)
                return Result.Success(notificationEntities.Count);
            else
                return Result.Fail<int>("No se pudo insertar ninguna notificación.", ErrorTypeEnum.General);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding multiple notifications: {Message}", ex.Message);
            return Result.Fail<int>("Ocurrió un error inesperado al agregar las notificaciones.", ErrorTypeEnum.Database);
        }
    }

    /// <summary>
    /// Deletes all notifications for a specific user.
    /// </summary>
    public async Task<Result<bool>> DeleteAllNotificationsAsync()
    {
        try
        {
            await _unitOfWork.BeginTransactionAsync();
            var userId = _unitOfWork.GetCurrentUserCi();
            if (userId == null)
            {
                await _unitOfWork.RollbackTransactionAsync();
                return Result.Fail<bool>("Usuario no identificado.", ErrorTypeEnum.Validation);
            }

            var success = await _unitOfWork.Notifications.DeleteAllAsync(userId);
            if (!success)
            {
                await _unitOfWork.RollbackTransactionAsync();
                return Result.Fail<bool>("No se encontraron notificaciones para eliminar.", ErrorTypeEnum.NotFound);
            }

            var saveChangesSuccess = await _unitOfWork.SaveChangesAsync() > 0;
            if (saveChangesSuccess)
            {
                await _unitOfWork.CommitTransactionAsync();
                return Result.Success(true);
            }
            else
            {
                await _unitOfWork.RollbackTransactionAsync();
                return Result.Fail<bool>("No se pudo realizar la eliminación masiva.", ErrorTypeEnum.General);
            }
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            _logger.LogError(ex, "Error deleting all notifications: {Message}", ex.Message);
            return Result.Fail<bool>("Ocurrió un error inesperado al eliminar las notificaciones.", ErrorTypeEnum.Database);
        }
    }

    /// <summary>
    /// Deletes a user notification by its unique identifier.
    /// </summary>
    public async Task<Result<bool>> DeleteAsync()
    {
        try
        {
            await _unitOfWork.BeginTransactionAsync();
            var existingNotification = (await _unitOfWork.Notifications.GetByCiAsync(_unitOfWork.GetCurrentUserCi()!)).FirstOrDefault();
            if (existingNotification == null)
            {
                await _unitOfWork.RollbackTransactionAsync();
                return Result.Fail<bool>("Notificación de usuario no encontrada.", ErrorTypeEnum.NotFound);
            }

            var deletedSuccessfullyMarked = await _unitOfWork.Notifications.DeleteAsync(_unitOfWork.GetCurrentUserCi()!);

            var saveChangesSuccess = await _unitOfWork.SaveChangesAsync() > 0;

            if (deletedSuccessfullyMarked && saveChangesSuccess)
            {
                await _unitOfWork.CommitTransactionAsync();
                return Result.Success(true);
            }
            else
            {
                await _unitOfWork.RollbackTransactionAsync();
                return Result.Fail<bool>("No se pudo eliminar la notificación.", ErrorTypeEnum.General);
            }
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            _logger.LogError(ex, "Error occurred while deleting user notification with CI {Ci}: {Message}", _unitOfWork.GetCurrentUserCi(), ex.Message);
            return Result.Fail<bool>("Ocurrió un error inesperado al intentar eliminar la notificación.", ErrorTypeEnum.Database);
        }
    }

    public Task<Result<bool>> DeleteAsync(string ci)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Retrieves a paginated list of user notifications based on specified filters.
    /// </summary>
    public async Task<Result<List<NotificationDTO>>> GetAllAsync(int pageNumber, int pageSize)
    {
        try
        {
            var notifications = await _unitOfWork.Notifications.GetAllAsync(pageNumber, pageSize);

            return Result.Success(_mapper.Map<List<NotificationDTO>>(notifications));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving notifications: {Message}", ex.Message);
            return Result.Fail<List<NotificationDTO>>("Ocurrió un error inesperado al obtener las notificaciones.");
        }
    }

    /// <summary>
    /// Retrieves a single user notification by its unique identifier.
    /// </summary>
    public async Task<Result<NotificationDTO>> GetByCiAsync(string ci)
    {
        try
        {
            var notificationEntity = (await _unitOfWork.Notifications.GetByCiAsync(ci)).FirstOrDefault();
            if (notificationEntity == null)
                return Result.Fail<NotificationDTO>("Notificación no encontrada.", ErrorTypeEnum.NotFound);

            var notificationDto = _mapper.Map<NotificationDTO>(notificationEntity);
            return Result.Success(notificationDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving notification CI {ci}: {Message}", ci, ex.Message);
            return Result.Fail<NotificationDTO>("Ocurrió un error inesperado al recuperar la notificación.");
        }
    }

    /// <summary>
    /// Retrieves a paginated list of notifications for a specific user.
    /// </summary>
    public async Task<Result<List<NotificationDTO>>> GetNotificationsByUserIdAsync(int pageNumber, int pageSize)
    {
        return await GetAllAsync(pageNumber, pageSize);
    }

    /// <summary>
    /// Marks all notifications for a specific user as read.
    /// </summary>
    public async Task<Result<bool>> MarkAllAsReadAsync()
    {
        await _unitOfWork.BeginTransactionAsync();
        try
        {
            var userCi = _unitOfWork.GetCurrentUserCi();
            if (userCi == null)
            {
                await _unitOfWork.RollbackTransactionAsync();
                return Result.Fail<bool>("Usuario no identificado.", ErrorTypeEnum.Validation);
            }

            var success = await _unitOfWork.Notifications.MarkAllAsReadAsync(userCi);
            if (!success)
            {
                await _unitOfWork.RollbackTransactionAsync();
                return Result.Fail<bool>("No se encontraron notificaciones sin leer para el usuario.", ErrorTypeEnum.NotFound);
            }

            var saveChangesSuccess = await _unitOfWork.SaveChangesAsync() > 0;
            if (saveChangesSuccess)
            {
                await _unitOfWork.CommitTransactionAsync();
                return Result.Success(true);
            }
            else
            {
                await _unitOfWork.RollbackTransactionAsync();
                return Result.Fail<bool>("No se realizaron cambios al marcar todas las notificaciones.", ErrorTypeEnum.General);
            }
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            _logger.LogError(ex, "Error marking all notifications as read: {Message}", ex.Message);
            return Result.Fail<bool>("Ocurrió un error inesperado al procesar la solicitud.", ErrorTypeEnum.Database);
        }
    }

    /// <summary>
    /// Marks a specific user notification as read.
    /// </summary>
    public async Task<Result<bool>> MarkAsReadAsync(string notificationId)
    {
        await _unitOfWork.BeginTransactionAsync();
        try
        {
            var success = await _unitOfWork.Notifications.MarkAllAsReadAsync(notificationId);
            if (!success)
            {
                await _unitOfWork.RollbackTransactionAsync();
                return Result.Fail<bool>("Notificación no encontrada o ya marcada como leída.", ErrorTypeEnum.NotFound);
            }

            var saveChangesSuccess = await _unitOfWork.SaveChangesAsync() > 0;
            if (saveChangesSuccess)
            {
                await _unitOfWork.CommitTransactionAsync();
                return Result.Success(true);
            }
            else
            {
                await _unitOfWork.RollbackTransactionAsync();
                return Result.Fail<bool>("No se pudo actualizar el estado de la notificación.", ErrorTypeEnum.General);
            }
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            _logger.LogError(ex, "Error marking notification {Id} as read: {Message}", notificationId, ex.Message);
            return Result.Fail<bool>("Ocurrió un error inesperado al procesar la solicitud.", ErrorTypeEnum.Database);
        }
    }

    public async Task<bool> SendNotificationsToMultipleUsersAsync(
            List<int> userIds,
            RequestDTO requestDto,
            Func<RequestDTO, string> notificationDescriptionBuilder)
    {
        if (userIds.Count == 0)
        {
            return false;
        }

        try
        {
            var notificationDescription = notificationDescriptionBuilder(requestDto);

            var notifications = userIds
                .Distinct()
                .Select(userId => new NotificationInsertDTO
                {
                    UserId = userId,
                    Title = "General",
                    Priority = "Normal",
                    NotificationType = "RequestUpdate",
                    Message = notificationDescription,
                    IsRead = false
                })
                .ToList();

            var notificationResult = await AddManyAsync(notifications);
            return notificationResult.IsSuccess;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending multiple notifications for request {Id}: {Message}", requestDto.Id, ex.Message);
            throw;
        }
    }

    /// <summary>
    /// Updates an existing user notification's information.
    /// </summary>
    public async Task<Result<bool>> UpdateAsync(NotificationEditDTO dto)
    {
        try
        {
            var entity = (await _unitOfWork.Notifications.GetByCiAsync(dto.UserCi)).FirstOrDefault();

            if (entity == null)
                return Result.Fail<bool>("Notificación no encontrada.", ErrorTypeEnum.NotFound);

            _mapper.Map(dto, entity);

            await _unitOfWork.Notifications.UpdateAsync(entity);

            var saveChangesSuccess = await _unitOfWork.SaveChangesAsync() > 0;

            if (saveChangesSuccess)
                return Result.Success(true);
            else
                return Result.Fail<bool>("No se guardaron cambios al intentar actualizar la notificación.", ErrorTypeEnum.General);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating notification ID {Id}: {Message}", dto.Id, ex.Message);
            return Result.Fail<bool>("Ocurrió un error inesperado al actualizar la notificación.", ErrorTypeEnum.Database);
        }
    }
}