using Bixa.Backend.Models.DTOs.SolicitudesModelDTO;
using Bixa.Backend.DataAccess.Wrappers;
using Microsoft.AspNetCore.Authorization;
using Bixa.Backend.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Bixa.Backend.Models;
using Bixa.Backend.Base;
using AutoMapper;

namespace Bixa.Backend.Controllers.SolicitudesApiControllers;

/// <summary>
/// API Controller for user management operations following RESTful standards.
/// Now uses BaseApiController's helper methods for authorization and validation.
/// </summary>
/// <remarks>
/// Initializes a new instance of the UserApiController.
/// </remarks>
/// <param name="solicitudesService">The solicitudes service instance for business logic.</param>
/// <param name="mapper">AutoMapper instance for DTO conversions.</param>
/// <param name="loggerWrapper">Logger wrapper for logging operations.</param>
[Authorize]
[ApiController]
[Route("api/solicitudes")]
public class SolicitudesApiControllers(ISolicitudesService solicitudesService,
    IMapper mapper,
    LoggerWrapper loggerWrapper) : BaseApiController(mapper, loggerWrapper)
{
    private readonly ISolicitudesService _solicitudesService = solicitudesService;

    ///// <summary>
    ///// Registra una nueva solicitud de vacaciones en el sistema.
    ///// </summary>
    ///// <param name="solicitud">El objeto SolicVacacionesDTO que contiene la información de la solicitud.</param>
    ///// <returns>Guarda una nueva solicitud de vacaciones en el sistema.</returns>
    //[HttpPost("Vacaciones")]
    //[ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    //[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    //[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    //[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    //public async Task<IActionResult> SolicVacaciones([FromBody] SolicVacacionesDTO solicitud)
    //{
    //    var result = await _solicitudesService.AddNewSolicitudAsync(solicitud);
    //    return HandleServiceResult(result);
    //}
}