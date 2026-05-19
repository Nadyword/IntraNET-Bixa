using Bixa.Backend.DataAccess.Entities;
using Bixa.Backend.Models.DTOs.SoporteChatModelDTO;
using Bixa.Backend.Models.Response;

namespace Bixa.Backend.Services.Interfaces;

public interface ISoporteChatService
{
    Task<Result<string>> AddNewMessageAsync(SoporteChatMDTO soporte);

    Task<Result<string>> AddNewAnswerAsync(SoporteChatRDTO soporte);

    Task<Result<SoporteChat[]>> GetHistoriChat(string Ci);
}