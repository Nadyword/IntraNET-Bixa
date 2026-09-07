using Bixa.Backend.DataAccess.Wrappers;
using Microsoft.AspNetCore.Authorization;
using Bixa.Backend.Services.Interfaces;
using Bixa.Backend.Models.DTOs.FirmantesDTO;
using Bixa.Backend.Models.Enums;
using Microsoft.AspNetCore.Mvc;
using Bixa.Backend.Models;
using Bixa.Backend.Base;
using AutoMapper;

namespace Bixa.Backend.Controllers.FirmantesApiControllers;

/// <summary>
/// API Controller para los ajustes manuales de la cadena de firmantes de un empleado.
/// </summary>
/// <remarks>
/// Sin ajuste, quienes firman las solicitudes son los supervisores que Profit deriva de
/// <c>snemple.supervisor</c>. Estos endpoints permiten apartarse de esa cadena guardando únicamente
/// las diferencias, que se aplican cada vez que se arma la lista de aprobaciones de una solicitud.
/// </remarks>
[Authorize]
[ApiController]
[Route("api/firmantes")]
public class FirmantesApiController(
    IAjusteFirmantesService ajusteFirmantesService,
    IMapper mapper,
    LoggerWrapper loggerWrapper) : BaseApiController(mapper, loggerWrapper)
{
    private readonly IAjusteFirmantesService _ajusteFirmantesService = ajusteFirmantesService;

    /// <summary>
    /// Usuarios activos de la intranet que pueden añadirse como firmantes.
    /// GET /api/firmantes/candidatos?q=perez
    /// </summary>
    [HttpGet("candidatos")]
    [ProducesResponseType(typeof(ApiResponse<List<CandidatoFirmanteDTO>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetCandidatos([FromQuery] string? q, [FromQuery] int take = 20)
    {
        var authResult = RequireUserRol(UserRolEnum.Administrador);
        if (authResult != null) return authResult;

        var result = await _ajusteFirmantesService.GetCandidatosAsync(q, take);
        return HandleServiceResult(result);
    }

    /// <summary>
    /// CIs de los empleados que tienen un ajuste guardado, para señalarlos en el listado.
    /// GET /api/firmantes/ajustados
    /// </summary>
    [HttpGet("ajustados")]
    [ProducesResponseType(typeof(ApiResponse<List<string>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAjustados()
    {
        var authResult = RequireUserRol(UserRolEnum.Administrador);
        if (authResult != null) return authResult;

        var result = await _ajusteFirmantesService.GetCisConAjusteAsync();
        return HandleServiceResult(result);
    }

    /// <summary>
    /// Cadena de firmantes del empleado: la original de Profit y la efectiva con sus ajustes.
    /// GET /api/firmantes/{ci}
    /// </summary>
    [HttpGet("{ci}")]
    [ProducesResponseType(typeof(ApiResponse<AjusteFirmantesDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetByCi(string ci)
    {
        var authResult = RequireUserRol(UserRolEnum.Administrador);
        if (authResult != null) return authResult;

        var result = await _ajusteFirmantesService.GetAsync(ci);
        return HandleServiceResult(result);
    }

    /// <summary>
    /// Guarda la cadena de firmantes del empleado. Se persisten solo las diferencias contra Profit,
    /// de modo que enviar la cadena original equivale a no tener ajuste.
    /// PUT /api/firmantes/{ci}
    /// </summary>
    [HttpPut("{ci}")]
    [ProducesResponseType(typeof(ApiResponse<AjusteFirmantesDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Save(string ci, [FromBody] GuardarAjusteFirmantesDTO dto)
    {
        var authResult = RequireUserRol(UserRolEnum.Administrador);
        if (authResult != null) return authResult;

        var result = await _ajusteFirmantesService.GuardarAsync(ci, dto);
        return HandleServiceResult(result, "Firmantes guardados exitosamente.");
    }

    /// <summary>
    /// Elimina el ajuste: el empleado vuelve a firmar según la jerarquía de Profit.
    /// DELETE /api/firmantes/{ci}
    /// </summary>
    [HttpDelete("{ci}")]
    [ProducesResponseType(typeof(ApiResponse<AjusteFirmantesDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Restablecer(string ci)
    {
        var authResult = RequireUserRol(UserRolEnum.Administrador);
        if (authResult != null) return authResult;

        var result = await _ajusteFirmantesService.RestablecerAsync(ci);
        return HandleServiceResult(result, "Se restableció la cadena de firmantes de Profit.");
    }
}
