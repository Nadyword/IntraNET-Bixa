using Bixa.Backend.DataAccess.Entities;
using Bixa.Backend.DataAccess.Models;

namespace Bixa.Backend.DataAccess.Interfaces.Repositories;

/// <summary>
/// Defines the contract for data access operations for SoporteChat entities.
/// </summary>
public interface ISoporteChatRepository
{
    Task<string> AddNewMessageAsync(SoporteChat soporte);

    Task<string> AddNewAnswerAsync(SoporteChat soporte);

    Task<SoporteChat[]> GetHistoriChat(string Ci);

    /// <summary>
    /// Obtener las solicitudes de chat que no han sido respondidas y las respondidas.
    /// </summary>
    /// <returns>Devuelve un array de objetos SolicitudesChats que representan las solicitudes de chat.</returns>
    Task<List<SolicitudesChats>> GetChatRequests();
}