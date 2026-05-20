using Bixa.Backend.Models.DTOs.SoporteChatModelDTO;
using Bixa.Backend.DataAccess.Wrappers;
using Microsoft.AspNetCore.Authorization;
using Bixa.Backend.DataAccess.Entities;
using Bixa.Backend.DataAccess.Models;
using Bixa.Backend.Services.Interfaces;
using Bixa.Backend.Models.Enums;
using Microsoft.AspNetCore.Mvc;
using Bixa.Backend.Models;
using Bixa.Backend.Base;
using AutoMapper;

namespace Bixa.Backend.Controllers.SoporteControllers;

/// <summary>
/// API Controller for user management operations following RESTful standards.
/// Now uses BaseApiController's helper methods for authorization and validation.
/// </summary>
/// <remarks>
/// Initializes a new instance of the UserApiController.
/// </remarks>
/// <param name="soporteChatService">The soporte chat service instance for business logic.</param>
/// <param name="mapper">AutoMapper instance for DTO conversions.</param>
/// <param name="loggerWrapper">Logger wrapper for logging operations.</param>
[Authorize]
[ApiController]
[Route("api/soporte")]
public class SoporteCharControllers(ISoporteChatService soporteChatService,
    IMapper mapper,
    LoggerWrapper loggerWrapper) : BaseApiController(mapper, loggerWrapper)
{
    private readonly ISoporteChatService _soporteChatService = soporteChatService;

    /// <summary>
    /// Registra un nuevo mensaje de soporte en el sistema.
    /// </summary>
    /// <param name="soporte">El objeto SoporteChat que contiene la información del mensaje.</param>
    /// <returns>Guarda un nuevo mensaje de soporte en el sistema.</returns>
    [HttpPost("NewMessage")]
    [ProducesResponseType(typeof(ApiResponse<SoporteChat>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> AddNewMessage([FromBody] SoporteChatMDTO soporte)
    {
        var result = await _soporteChatService.AddNewMessageAsync(soporte);
        return HandleServiceResult(result);
    }

    /// <summary>
    /// Registra una nueva respuesta de soporte en el sistema.
    /// </summary>
    /// <param name="soporte">El objeto SoporteChat que contiene la información del mensaje.</param>
    /// <returns>Guarda una nueva respuesta de soporte en el sistema.</returns>
    [HttpPost("NewAnswer")]
    [ProducesResponseType(typeof(ApiResponse<SoporteChat>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> AddNewAnswer([FromBody] SoporteChatRDTO soporte)
    {
        var authResult = RequireUserRol(UserRolEnum.Administrador);
        if (authResult != null) return authResult;

        var result = await _soporteChatService.AddNewAnswerAsync(soporte);
        return HandleServiceResult(result);
    }

    /// <summary>
    /// Obener el historial de mensajes de soporte en el sistema.
    /// </summary>
    /// <param name="Ci">El objeto SoporteChat que contiene la información del mensaje.</param>
    /// <returns>Guarda una nueva respuesta de soporte en el sistema.</returns>
    [HttpGet("HistoriChat/{Ci}")]
    [ProducesResponseType(typeof(ApiResponse<SoporteChat>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetHistoriChat([FromRoute] string Ci)
    {
        var result = await _soporteChatService.GetHistoriChat(Ci);
        return HandleServiceResult(result);
    }

    /// <summary>
    /// Ver el historial de mensajes de soporte abiertos en el sistema y cerrados.
    /// </summary>
    /// <returns>Devuelve el historial de mensajes de soporte abiertos y cerrados en el sistema.</returns>
    [HttpGet("ChatAbiertos")]
    [ProducesResponseType(typeof(ApiResponse<SolicitudesChats>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetChatRequests()
    {
        var authResult = RequireUserRol(UserRolEnum.Administrador);
        if (authResult != null) return authResult;

        var result = await _soporteChatService.GetChatRequests();
        return HandleServiceResult(result);
    }

    /// <summary>
    /// Ver el historial de mensajes de soporte abiertos en el sistema y cerrados.
    /// </summary>
    /// <returns>Devuelve el historial de mensajes de soporte abiertos y cerrados en el sistema.</returns>
    [HttpPut("SetMessageStatus/{Ci}")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> SetMessageStatus([FromRoute] string Ci)
    {
        var authResult = RequireUserRol(UserRolEnum.Administrador);
        if (authResult != null) return authResult;

        var result = await _soporteChatService.SetMessageStatus(Ci);
        return HandleServiceResult(result);
    }
}