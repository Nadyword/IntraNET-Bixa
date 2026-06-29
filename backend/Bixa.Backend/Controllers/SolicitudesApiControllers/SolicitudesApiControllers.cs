using AutoMapper;
using Bixa.Backend.Base;
using Bixa.Backend.DataAccess.Models;
using Bixa.Backend.DataAccess.Wrappers;
using Bixa.Backend.Models;
using Bixa.Backend.Models.DTOs.ReportesModelDTO;
using Bixa.Backend.Models.DTOs.SolicitudesModelDTO;
using Bixa.Backend.Models.Enums;
using Bixa.Backend.Models.Response;
using Bixa.Backend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bixa.Backend.Controllers.SolicitudesApiControllers;

/// <summary>
/// API Controller for user management operations following RESTful standards.
/// Now uses BaseApiController's helper methods for authorization and validation.
/// </summary>
/// <remarks>
/// Initializes a new instance of the UserApiController.
/// </remarks>
/// <param name="solicitudesService">The solicitudes service instance for business logic.</param>
/// <param name="reportService"></param>
/// <param name="mapper">AutoMapper instance for DTO conversions.</param>
/// <param name="loggerWrapper">Logger wrapper for logging operations.</param>
[Authorize]
[ApiController]
[Route("api/solicitudes")]
public class SolicitudesApiControllers(ISolicitudesService solicitudesService,
    IReportService reportService,
    IMapper mapper,
    LoggerWrapper loggerWrapper) : BaseApiController(mapper, loggerWrapper)
{
    private readonly ISolicitudesService _solicitudesService = solicitudesService;
    private readonly IReportService _reportService = reportService;

    /// <summary>
    /// Registra una nueva solicitud de vacaciones en el sistema.
    /// </summary>
    /// <param name="solicitud">El objeto SolicVacacionesDTO que contiene la información de la solicitud.</param>
    /// <returns>Guarda una nueva solicitud de vacaciones en el sistema.</returns>
    [HttpPost("Vacaciones")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> SolicVacaciones([FromBody] SolicVacacionesDTO solicitud)
    {
        var result = await _solicitudesService.AddSolicitudVacaciones(solicitud);
        return HandleServiceResult(result);
    }

    ///// <summary>
    ///// Obtiene todos los tramites de un usuario específico, identificados por su CI.
    ///// </summary>
    ///// <param name="ci">El CI del usuario para obtener sus trámites.</param>
    ///// <returns>Obtiene todos los trámites de un usuario específico.</returns>
    [HttpGet("{ci}")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetTramitesByCi(string ci)
    {
        var result = await _solicitudesService.GetTramitesByCi(ci);
        return HandleServiceResult(result);
    }

    ///// <summary>
    ///// Obtiene todos los tramites que no estén finalizados.
    ///// </summary>
    ///// <param name="ci">El CI del usuario para obtener sus trámites.</param>
    ///// <returns>Obtiene todos los trámites de un usuario específico.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAllTramites()
    {
        var result = await _solicitudesService.GetAllTramites();
        return HandleServiceResult(result);
    }

    ///// <summary>
    ///// Obtiene todos los tramites que no estén finalizados.
    ///// </summary>
    ///// <param name="ci">El CI del usuario para obtener sus trámites.</param>
    ///// <returns>Obtiene todos los trámites de un usuario específico.</returns>
    [HttpGet("PorAprobarByCi/{ci}")]
    [ProducesResponseType(typeof(ApiResponse<PorAprobar>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetTramitesForAprobacion(string ci)
    {
        var result = await _solicitudesService.GetTramitesForAprobacion(ci);
        return HandleServiceResult(result);
    }

    ///// <summary>
    ///// Obtiene todos los tramites que no estén finalizados.
    ///// </summary>
    ///// <param name="ci">El CI del usuario para obtener sus trámites.</param>
    ///// <returns>Obtiene todos los trámites de un usuario específico.</returns>
    [HttpGet("Aprobaciones/{tramiteId}")]
    [ProducesResponseType(typeof(ApiResponse<AprobacionDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAprobacionesByTramiteId(int tramiteId)
    {
        var result = await _solicitudesService.GetAprobacionesByTramiteId(tramiteId);
        return HandleServiceResult(result);
    }

    ///// <summary>
    ///// Aprueba un trámite específico, identificado por su ID, y registra la aprobación en el sistema.
    ///// </summary>
    ///// <param name="ci">El CI del usuario que aprueba el trámite.</param>
    ///// <param name="estado">El estado de la aprobación.</param>
    ///// <returns>Un resultado indicando si la operación fue exitosa o no.</returns>
    [HttpPut("Aprobar")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> AprobarTramite(AprobarTramiteDTO aprobarTramiteDTO)
    {
        var authResult = RequireUserRol(UserRolEnum.Supervisor, UserRolEnum.Administrador);
        if (authResult != null) return authResult;

        var result = await _solicitudesService.AprobarTramite(aprobarTramiteDTO);
        return HandleServiceResult(result);
    }

    ///// <summary>
    ///// Aprueba un trámite específico, identificado por su ID, y registra la aprobación en el sistema.
    ///// </summary>
    ///// <param name="ci">El CI del usuario que aprueba el trámite.</param>
    ///// <param name="estado">El estado de la aprobación.</param>
    ///// <returns>Un resultado indicando si la operación fue exitosa o no.</returns>
    [HttpGet("Aprobados")]
    [ProducesResponseType(typeof(ApiResponse<TramiteDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAprobados()
    {
        var authResult = RequireUserRol(UserRolEnum.Administrador);
        if (authResult != null) return authResult;

        var result = await _solicitudesService.GetAprobados();
        return HandleServiceResult(result);
    }

    /// <summary>
    /// Genera un PDF de prueba usando la plantilla genérica con datos de ejemplo.
    /// </summary>
    [HttpGet("Reporte/Vacaciones/{tramiteId}")]
    [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetReporte(int tramiteId)
    {
        var authResult = RequireUserRol(UserRolEnum.Administrador, UserRolEnum.Supervisor);
        if (authResult != null) return authResult;

        var result = await _solicitudesService.GetInfoReporteVacaciones(tramiteId);
        if (!result.IsSuccess)
        {
            return HandleServiceResult(Result.Fail<TramiteReportModel>(result.Error));
        }

        TramiteReportModel modelo = result.Value;
        var bytes = _reportService.GenerateTramiteReport(modelo, "Vacaciones");

        return File(bytes, "application/pdf", "reporte_vacaciones.pdf");
    }
}