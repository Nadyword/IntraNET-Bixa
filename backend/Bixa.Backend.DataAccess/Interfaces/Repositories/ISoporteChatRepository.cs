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

    /// <summary>
    ///    Cambiar el estado de los mensajes de soporte a respondidos, marcando el campo RespondidoPorCi con el CI del agente que respondió.
    /// </summary>
    /// <param name="Ci">El CI del agente que respondió los mensajes.</param>
    /// <returns>Devuelve un valor booleano que indica si la operación fue exitosa.</returns>
    Task<bool> SetMessageStatus(string Ci);
}