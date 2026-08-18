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

    /// <summary>
    /// Indica si el usuario ya tiene algún mensaje registrado en el día calendario actual (UTC).
    /// </summary>
    /// <param name="ci">El CI del usuario que envía el mensaje.</param>
    /// <returns>True si ya existe al menos un mensaje de ese usuario hoy.</returns>
    Task<bool> HasMessageTodayAsync(string ci);

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

    /// <summary>
    /// Crear una nueva entrada de FAQ en la base de datos utilizando el objeto FAQs proporcionado.
    /// </summary>
    /// <param name="fAQs"></param>
    /// <returns></returns>
    Task<bool> CreateFAQ(FAQs fAQs);

    /// <summary>
    ///  Actualizar una entrada de FAQ existente en la base de datos utilizando el objeto FAQs proporcionado, identificando la entrada a actualizar por su ID o algún otro identificador único.
    /// </summary>
    /// <param name="fAQs"></param>
    /// <returns></returns>
    Task<bool> UpdateFAQ(FAQs fAQs);

    /// <summary>
    ///  Eliminar una entrada de FAQ de la base de datos utilizando el ID proporcionado para identificar la entrada a eliminar.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    Task<bool> DeleteFAQ(int id);

    /// <summary>
    /// Recuperar todas las entradas de FAQ de la base de datos y devolverlas como un array de objetos FAQs.
    /// </summary>
    /// <returns></returns>
    Task<FAQs[]> GetAllFAQs();
}