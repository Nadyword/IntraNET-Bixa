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
    public async Task<IActionResult> GetGrupoFaById(string ci)
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
    public async Task<IActionResult> GetUserById(string ci)
    {
        var normalizedCi = UtilityService.NormalizeCiFormat(ci);
        var result = await _readOnlyUnitOfWork.SnEmple.GetFullInfoByCiAsync(normalizedCi);
        return HandleServiceResult(result);
    }
}