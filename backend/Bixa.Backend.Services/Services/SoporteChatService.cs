using Bixa.Backend.Models.DTOs.SoporteChatModelDTO;
using Bixa.Backend.DataAccess.Interfaces.Repositories;
using Bixa.Backend.DataAccess.Entities;
using Bixa.Backend.Services.Interfaces;
using Bixa.Backend.Models.Response;
using AutoMapper;
using Bixa.Backend.DataAccess.Models;

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
    ISoporteChatRepository soporteChatRepository, IMapper mapper) : ISoporteChatService
{
    private readonly ISoporteChatRepository _soporteChatRepository = soporteChatRepository;
    private readonly IMapper _mapper = mapper;

    public Task<Result<string>> AddNewAnswerAsync(SoporteChatRDTO soporte)
    {
        soporte.UserCi = UtilityService.NormalizeCiFormat(soporte.UserCi);
        if (!string.IsNullOrEmpty(soporte.RespondidoPorCi))
            soporte.RespondidoPorCi = UtilityService.NormalizeCiFormat(soporte.RespondidoPorCi);
        SoporteChat newMensajeUser = _mapper.Map<SoporteChat>(soporte);

        return _soporteChatRepository.AddNewAnswerAsync(newMensajeUser).ContinueWith(task =>
        {
            if (task.IsFaulted)
            {
                // Log the exception (not implemented here)
                return Result.Fail<string>("An error occurred while adding the answer.");
            }
            return Result.Success(task.Result);
        });
    }

    public async Task<Result<string>> AddNewMessageAsync(SoporteChatMDTO soporte)
    {
        soporte.UserCi = UtilityService.NormalizeCiFormat(soporte.UserCi);
        SoporteChat newMensajeUser = _mapper.Map<SoporteChat>(soporte);
        string resul = await _soporteChatRepository.AddNewMessageAsync(newMensajeUser);
        return Result<string>.Success(resul);
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

    public Task<Result<bool>> SetMessageStatus(string Ci)
    {
        var result = _soporteChatRepository.SetMessageStatus(UtilityService.NormalizeCiFormat(Ci));
        return Task.FromResult(Result.Success(result.Result));
    }
}