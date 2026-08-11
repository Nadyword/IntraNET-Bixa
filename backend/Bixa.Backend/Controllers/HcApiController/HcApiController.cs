using Bixa.Backend.DataAccess.Interfaces.Repositories;
using Bixa.Backend.DataAccess.Wrappers;
using Microsoft.AspNetCore.Authorization;
using Bixa.Backend.Services.Interfaces;
using Bixa.Backend.Services.Services;
using Bixa.Backend.Models.DTOs.HcDTO;
using Bixa.Backend.Models.Response;
using Bixa.Backend.Models.Enums;
using Microsoft.AspNetCore.Mvc;
using Bixa.Backend.Models;
using Bixa.Backend.Base;
using AutoMapper;

namespace Bixa.Backend.Controllers.HcApiController;

/// <summary>
/// API Controller para los valores declarados por mes (Mes1/Mes2/Mes3) de la prima trimestral de HC.
/// </summary>
[Authorize]
[ApiController]
[Route("api/hc")]
public class HcApiController(
    IHcMesRegistroService hcMesRegistroService,
    IUnitOfWork unitOfWork,
    IMapper mapper,
    LoggerWrapper loggerWrapper) : BaseApiController(mapper, loggerWrapper)
{
    private readonly IHcMesRegistroService _hcMesRegistroService = hcMesRegistroService;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    /// <summary>
    /// Obtiene los valores de Mes1/Mes2/Mes3 guardados para el CI indicado.
    /// El propio empleado solo puede consultar su CI; un Administrador puede consultar cualquier CI.
    /// </summary>
    [HttpGet("{ci}")]
    [ProducesResponseType(typeof(ApiResponse<HcMesRegistroDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetByCi(string ci)
    {
        var authResult = RequireOwnCiOrAdmin(ci);
        if (authResult != null) return authResult;

        var result = await _hcMesRegistroService.GetByCiAsync(ci);
        return HandleServiceResult(result);
    }

    /// <summary>
    /// Guarda (crea o sobrescribe) los valores de Mes1/Mes2/Mes3 para el CI indicado.
    /// La suma no puede superar la Prima trim en Bs. Factura vigente.
    /// </summary>
    [HttpPut("{ci}")]
    [ProducesResponseType(typeof(ApiResponse<HcMesRegistroDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Save(string ci, [FromBody] HcMesRegistroSaveDTO dto)
    {
        var authResult = RequireOwnCiOrAdmin(ci);
        if (authResult != null) return authResult;

        var result = await _hcMesRegistroService.SaveAsync(ci, dto);
        return HandleServiceResult(result, "Registro de HC guardado exitosamente.");
    }

    /// <summary>
    /// Obtiene los últimos registros de HC editados. Solo para Administrador.
    /// </summary>
    [HttpGet("recientes")]
    [ProducesResponseType(typeof(ApiResponse<List<HcMesRegistroDTO>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetRecientes([FromQuery] int take = 20)
    {
        var authResult = RequireUserRol(UserRolEnum.Administrador);
        if (authResult != null) return authResult;

        var result = await _hcMesRegistroService.GetRecientesAsync(take);
        return HandleServiceResult(result);
    }

    /// <summary>
    /// Busca en TODOS los registros de HC por nombre, apellido o CI. Solo para Administrador.
    /// </summary>
    [HttpGet("buscar")]
    [ProducesResponseType(typeof(ApiResponse<List<HcMesRegistroDTO>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Buscar([FromQuery] string q)
    {
        var authResult = RequireUserRol(UserRolEnum.Administrador);
        if (authResult != null) return authResult;

        var result = await _hcMesRegistroService.BuscarAsync(q);
        return HandleServiceResult(result);
    }

    private IActionResult? RequireOwnCiOrAdmin(string ci)
    {
        if (_unitOfWork.GetCurrentUserRol() == UserRolEnum.Administrador)
            return null;

        var normalizedRequestedCi = UtilityService.NormalizeCiFormat(ci);
        var currentUserCi = _unitOfWork.GetCurrentUserCi();

        if (!string.IsNullOrEmpty(currentUserCi) && currentUserCi == normalizedRequestedCi)
            return null;

        return HandleServiceResult(Result.Fail("Acceso denegado: solo puede consultar/editar su propio registro de HC.", ErrorTypeEnum.Unauthorized));
    }
}
