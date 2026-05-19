using Bixa.Backend.DataAccess.Entities;

namespace Bixa.Backend.DataAccess.Interfaces.Repositories;

/// <summary>
/// Defines the contract for data access operations for SoporteChat entities.
/// </summary>
public interface ISoporteChatRepository
{
    Task<string> AddNewMessageAsync(SoporteChat soporte);

    Task<string> AddNewAnswerAsync(SoporteChat soporte);

    Task<SoporteChat[]> GetHistoriChat(string Ci);
}