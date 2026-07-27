using Bixa.Backend.DataAccess.Interfaces.Repositories;
using Bixa.Backend.DataAccess.Entities.DbProfit;
using Bixa.Backend.DataAccess.Wrappers;
using Microsoft.AspNetCore.Authorization;
using Bixa.Backend.Services.Services;
using Microsoft.AspNetCore.Mvc;
using Bixa.Backend.Models;
using Bixa.Backend.Base;
using AutoMapper;

namespace Bixa.Backend.Controllers.UserApiProfitController;

/// <summary>
/// API Controller for user management operations following RESTful standards.
/// Now uses BaseApiController's helper methods for authorization and validation.
/// </summary>
/// <remarks>
/// Initializes a new instance of the UserApiController.
/// </remarks>
///<param name="IReadOnlyUnitOfWork">Read-only unit of work for data access operations.</param>
/// <param name="mapper">AutoMapper instance for DTO conversions.</param>
/// <param name="loggerWrapper">Logger wrapper for logging operations.</param>{
[Authorize]
[ApiController]
[Route("api/usersProfit")]
public class UserApiProfitController(
    IReadOnlyUnitOfWork IReadOnlyUnitOfWork,
    IMapper mapper,
    LoggerWrapper loggerWrapper) : BaseApiController(mapper, loggerWrapper)
{
    private readonly IReadOnlyUnitOfWork _readOnlyUnitOfWork = IReadOnlyUnitOfWork ?? throw new ArgumentNullException(nameof(IReadOnlyUnitOfWork));

    /// <summary>
    /// Retrieves a single user by their ID.
    /// GET /api/users/{id}
    /// </summary>
    /// <param name="ci">The tax ID of the user to retrieve.</param>
    /// <returns>API response containing the GrupoFa if found.</returns>
    [HttpGet("{ci}/GrupoFa")]
    [ProducesResponseType(typeof(ApiResponse<GrupoFa[]>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetGrupoFaByCi(string ci)
    {
        var normalizedCi = UtilityService.NormalizeCiFormat(ci);
        var result = await _readOnlyUnitOfWork.GrupoFa.GetFullInfoByCiAsync(normalizedCi);
        return HandleServiceResult(result);
    }

    /// <summary>
    /// Retrieves a single user by their ID.
    /// GET /api/users/{id}
    /// </summary>
    /// <param name="ci">The tax ID of the user to retrieve.</param>
    /// <returns>API response containing the SnEmple if found.</returns>
    [HttpGet("{ci}/SnEmple")]
    [ProducesResponseType(typeof(ApiResponse<SnEmple>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetUserByCi(string ci)
    {
        var normalizedCi = UtilityService.NormalizeCiFormat(ci);
        var result = await _readOnlyUnitOfWork.SnEmple.GetFullInfoByCiAsync(normalizedCi);
        return HandleServiceResult(result);
    }

    /// <summary>
    /// Historial de vacaciones por CodEmp
    /// </summary>
    /// <param name="CodEmp">Lista de CodEmp para obtener el historial de vacaciones.</param>
    /// <returns>API response containing la lista de vacaciones por CodEmp.</returns>
    [HttpGet("{CodEmp}/Vacaciones")]
    [ProducesResponseType(typeof(ApiResponse<Vacaciones[]>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetVacacionesByCodEmp(string CodEmp)
    {
        var result = await _readOnlyUnitOfWork.Vacaciones.GetHistorialVacaByCodEmpAsync(CodEmp);
        return HandleServiceResult(result);
    }

    /// <summary>
    /// Obtiene los días especiales por CodEmp
    /// </summary>
    /// <param name="CodEmp">El código del empleado para el que se recuperan los días especiales. No puede ser nulo.</param>
    /// <returns>Un resultado que contiene una lista de objetos de días especiales asociados al empleado. Si no se encuentra el
    /// empleado, el resultado indica un error de tipo NotFound.</returns>
    [HttpGet("{CodEmp}/DiasEspeciales")]
    [ProducesResponseType(typeof(ApiResponse<Vacaciones[]>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetDiasEspecialesByCodEmp(string CodEmp)
    {
        var result = await _readOnlyUnitOfWork.DiaEspeciales.GetDiaEspecialesByCodEmpAsync(CodEmp);
        return HandleServiceResult(result);
    }

    /// <summary>
    /// Monto de utilidades disponible por CI
    /// </summary>
    /// <param name="ci">La cédula de identidad del empleado para el que se recupera el monto disponible.</param>
    /// <returns>API response con el monto disponible de utilidades.</returns>
    [HttpGet("{ci}/Utilidades")]
    [ProducesResponseType(typeof(ApiResponse<decimal?>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetUtilidadesByCi(string ci)
    {
        var normalizedCi = UtilityService.NormalizeCiFormat(ci);
        var result = await _readOnlyUnitOfWork.Utilidades.GetMontoDisponibleByCiAsync(normalizedCi);
        return HandleServiceResult(result);
    }
}