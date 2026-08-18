using Bixa.Backend.Models.DTOs.SoporteChatModelDTO;
using Bixa.Backend.DataAccess.Interfaces.Repositories;
using Bixa.Backend.DataAccess.Interfaces.Repositories.Profit;
using Bixa.Backend.Models.DTOs.FAQsDTO;
using Bixa.Backend.DataAccess.Entities;
using Bixa.Backend.DataAccess.Models;
using Bixa.Backend.Services.Interfaces;
using Bixa.Backend.Models.Response;
using Bixa.Backend.Models.Enums;
using AutoMapper;

namespace Bixa.Backend.Services.Services;

/// <summary>
/// Service for managing user-related business logic and operations.
/// Implements generic CRUD operations and specific user functionalities.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="SoporteChatService"/> class.
/// </remarks>
/// <param name="soporteChatRepository">The SoporteChat repository instance for data access.</param>
/// /// <param name="mapper">AutoMapper instance for DTO conversions.</param>
public class SoporteChatService(
    ISoporteChatRepository soporteChatRepository,
    IMapper mapper,
    INotificationService notificationService,
    IUserRepository userRepository,
    ISnEmpleProfitRepository snEmpleProfitRepository,
    ISendMailServices sendMailServices) : ISoporteChatService
{
    private readonly ISoporteChatRepository _soporteChatRepository = soporteChatRepository;
    private readonly IMapper _mapper = mapper;
    private readonly INotificationService _notificationService = notificationService;
    private readonly IUserRepository _userRepository = userRepository;
    private readonly ISnEmpleProfitRepository _snEmpleProfitRepository = snEmpleProfitRepository;
    private readonly ISendMailServices _sendMailServices = sendMailServices;

    public async Task<Result<string>> AddNewAnswerAsync(SoporteChatRDTO soporte)
    {
        soporte.UserCi = UtilityService.NormalizeCiFormat(soporte.UserCi);
        if (!string.IsNullOrEmpty(soporte.RespondidoPorCi))
            soporte.RespondidoPorCi = UtilityService.NormalizeCiFormat(soporte.RespondidoPorCi);
        var employeeCi = soporte.UserCi;
        SoporteChat newMensajeUser = _mapper.Map<SoporteChat>(soporte);

        var result = await _soporteChatRepository.AddNewAnswerAsync(newMensajeUser).ContinueWith(task =>
        {
            if (task.IsFaulted)
            {
                // Log the exception (not implemented here)
                return Result.Fail<string>("An error occurred while adding the answer.");
            }
            return Result.Success(task.Result);
        });

        if (result.IsSuccess)
        {
            await _notificationService.NotifyAsync(employeeCi, "ChatReply", "Nueva respuesta en soporte", "Soporte respondió tu mensaje en el chat.", "Chat");
        }

        return result;
    }

    public async Task<Result<string>> AddNewMessageAsync(SoporteChatMDTO soporte)
    {
        soporte.UserCi = UtilityService.NormalizeCiFormat(soporte.UserCi);

        // Se determina antes de insertar: si ya tenía mensajes hoy, ya se notificó por correo.
        var yaNotificadoHoy = await _soporteChatRepository.HasMessageTodayAsync(soporte.UserCi);

        SoporteChat newMensajeUser = _mapper.Map<SoporteChat>(soporte);
        string resul = await _soporteChatRepository.AddNewMessageAsync(newMensajeUser);

        var empleado = await _userRepository.GetUserByCiAsync(soporte.UserCi);
        var nombreEmpleado = empleado != null ? $"{empleado.FirstName} {empleado.LastName}".Trim() : null;
        var mensaje = string.IsNullOrWhiteSpace(nombreEmpleado)
            ? $"El usuario {soporte.UserCi} envió un mensaje en el chat de soporte."
            : $"El usuario {nombreEmpleado} (CI: {soporte.UserCi}) envió un mensaje en el chat de soporte.";

        await _notificationService.NotifyRoleAsync(UserRolEnum.Administrador, "ChatMessage", "Nuevo mensaje de soporte", mensaje, "Chat");
        await _notificationService.NotifyRoleAsync(UserRolEnum.Supervisor, "ChatMessage", "Nuevo mensaje de soporte", mensaje, "Chat");

        if (!yaNotificadoHoy)
        {
            await NotificarAdministradoresPorCorreoAsync(soporte.UserCi, nombreEmpleado);
        }

        return Result<string>.Success(resul);
    }

    /// <summary>
    /// Envía un correo a todos los administradores activos avisando de un nuevo mensaje de chat.
    /// Correo es un efecto secundario: nunca debe tumbar la operación principal.
    /// </summary>
    private async Task NotificarAdministradoresPorCorreoAsync(string empleadoCi, string? empleadoNombre)
    {
        try
        {
            var nombreParaCorreo = string.IsNullOrWhiteSpace(empleadoNombre) ? empleadoCi : empleadoNombre;
            var administradores = await _userRepository.GetActiveByRoleAsync((int)UserRolEnum.Administrador);
            foreach (var admin in administradores)
            {
                var correo = await _snEmpleProfitRepository.GetEmailByCiAsync(admin.Ci);
                if (!string.IsNullOrWhiteSpace(correo))
                {
                    await _sendMailServices.SendMailNuevoMensajeChat(correo, nombreParaCorreo, empleadoCi);
                }
            }
        }
        catch
        {
            // Correo es un efecto secundario: nunca debe tumbar la operación principal.
        }
    }

    public async Task<Result<List<SolicitudesChats>>> GetChatRequests()
    {
        var result = await _soporteChatRepository.GetChatRequests();
        if (result == null || result.Count == 0)
        {
            return Result.Fail<List<SolicitudesChats>>("No chat requests found.");
        }
        return Result.Success(result);
    }

    public async Task<Result<SoporteChat[]>> GetHistoriChat(string Ci)
    {
        var result = await _soporteChatRepository.GetHistoriChat(UtilityService.NormalizeCiFormat(Ci));
        if (result == null || result.Length == 0)
        {
            return Result.Fail<SoporteChat[]>("No chat history found.");
        }

        return Result.Success(result);
    }

    public async Task<Result<bool>> SetMessageStatus(string Ci)
    {
        var result = await _soporteChatRepository.SetMessageStatus(UtilityService.NormalizeCiFormat(Ci));
        // Se limpian las notificaciones de chat de quien está autenticado (no del "Ci" del hilo):
        // cuando un admin abre el hilo de un empleado, "Ci" es el CI del empleado, pero quien está
        // leyendo (y a quien hay que quitarle el aviso de "nuevo mensaje") es el admin autenticado.
        await _notificationService.MarkAllChatNotificationsAsReadAsync();
        return Result.Success(result);
    }

    public async Task<Result<bool>> CreateFAQ(FAQsDTO fAQs)
    {
        var result = await _soporteChatRepository.CreateFAQ(_mapper.Map<FAQs>(fAQs));
        if (!result)
        {
            return Result.Fail<bool>("Failed to create FAQ.");
        }
        return Result.Success(true);
    }

    public async Task<Result<bool>> UpdateFAQ(FAQsDTO fAQs)
    {
        var result = _soporteChatRepository.UpdateFAQ(_mapper.Map<FAQs>(fAQs));
        if (!result.Result)
        {
            return Result.Fail<bool>("Failed to update FAQ.");
        }
        return Result.Success(true);
    }

    public async Task<Result<bool>> DeleteFAQ(int id)
    {
        var result = await _soporteChatRepository.DeleteFAQ(id);
        if (!result)
        {
            return Result.Fail<bool>("Failed to delete FAQ.");
        }
        return Result.Success(true);
    }

    public async Task<Result<FAQs[]>> GetAllFAQs()
    {
        var result = await _soporteChatRepository.GetAllFAQs();
        if (result == null || result.Length == 0)
        {
            return Result.Fail<FAQs[]>("No FAQs found.");
        }
        return Result.Success(result);
    }
}