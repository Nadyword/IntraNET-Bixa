using Bixa.Backend.DataAccess.Wrappers;
using Bixa.Backend.Controllers.Services;
using Bixa.Backend.Models.Response;
using Bixa.Backend.Models.Enums;
using Bixa.Backend.Models.Utilities;
using Microsoft.AspNetCore.Mvc;
using Bixa.Backend.Models;
using AutoMapper;

namespace Bixa.Backend.Base;

/// <summary>
/// Abstract base controller for all API controllers,
/// providing common dependencies and response handling logic,
/// and now including helper methods for claim-based validation.
/// </summary>
/// <remarks>
/// Initializes a new instance of the BaseApiController.
/// </remarks>
/// <param name="mapper">AutoMapper instance for DTO conversions.</param>
/// <param name="loggerWrapper">The logger wrapper for creating type-specific loggers.</param>
[ApiController]
[Route("api/[controller]/[action]")]
public abstract class BaseApiController(IMapper mapper, LoggerWrapper loggerWrapper) : ControllerBase
{
    protected readonly HandleError _handleError = new();
    protected readonly ILogger<BaseApiController> _logger = loggerWrapper.CreateLogger<BaseApiController>();
    protected readonly IMapper _mapper = mapper;
    protected readonly ResponseService _responseService = new();

    /// <summary>
    /// Handles the result of a service operation, returning an appropriate API response.
    /// </summary>
    /// <typeparam name="TSuccess">The type of the success value.</typeparam>
    /// <param name="result">The Result object from a service operation.</param>
    /// <param name="successMessage">Optional success message to include in the response.</param>
    /// <returns>An IActionResult representing the API response.</returns>
    protected IActionResult HandleServiceResult<TSuccess>(Result<TSuccess> result, string successMessage = "") =>
     result.Match(
            success => _responseService.CreateResponse(ApiResponse<TSuccess>.SuccessResponse(success, successMessage)),
            error => _handleError.HandleErrorResult(error)
        );

    /// <summary>
    /// Handles the result of a void service operation, returning an appropriate API response.
    /// Use for operations that don't return a specific value on success (e.g., Delete).
    /// </summary>
    /// <param name="result">The Result object from a void service operation.</param>
    /// <param name="successMessage">Optional success message to include in the response.</param>
    /// <returns>An IActionResult representing the API response.</returns>
    protected IActionResult HandleServiceResult(Result result, string successMessage = "") =>
         result.Match(
            () => _responseService.CreateResponse(ApiResponse<object>.SuccessResponse(new object(), successMessage)),
            error => _handleError.HandleErrorResult(error)
        );

    /// <summary>
    /// Verifies if the authenticated user has a specific combination of UserRol and/or SigningRole.
    /// This method allows for flexible inline checks without relying solely on authorization policies.
    /// </summary>
    /// <param name="userRol">Optional: The UserRolEnum to be checked.</param>
    /// <returns>An IActionResult (Forbidden) if the user does not meet the combination; otherwise, null.</returns>
    protected IActionResult? RequireCombination(UserRolEnum? userRol = null)
    {
        bool hasRole = userRol == null || User.IsInRole(userRol.Value.ToString());

        if (hasRole)
        {
            return null;
        }

        var missingCriteria = new List<string>();
        var missingCriteriaEs = new List<string>();

        if (!hasRole && userRol.HasValue)
        {
            missingCriteria.Add($"role '{userRol.Value}'");
            missingCriteriaEs.Add($"rol '{userRol.Value}'");
        }

        _logger.LogWarning("Access forbidden: user does not meet criteria: {Criteria}", string.Join(", ", missingCriteria));
        return HandleServiceResult(Result.Fail($"Acceso denegado: el usuario no cumple con los criterios requeridos: {string.Join(", ", missingCriteriaEs)}.", ErrorTypeEnum.Unauthorized));
    }

    /// <summary>
    /// Checks if the authenticated user has the specified UserRol.
    /// </summary>
    /// <param name="requiredRoles">The UserRolEnum required.</param>
    /// <returns>An IActionResult (Forbidden) if the user does not have the role, otherwise null.</returns>
    protected IActionResult? RequireUserRol(params UserRolEnum[] requiredRoles)
    {
        if (requiredRoles == null || requiredRoles.Length == 0)
            return null;

        foreach (var role in requiredRoles)
        {
            if (User.IsInRole(role.ToString()))
                return null;
        }

        var rolesList = string.Join(", ", requiredRoles);
        _logger.LogWarning("Access denied: user lacks required roles {Roles}", rolesList);
        return HandleServiceResult(Result.Fail($"Acceso denegado: el usuario no posee ninguno de los roles requeridos: {rolesList}.", ErrorTypeEnum.Unauthorized));
    }

    /// <summary>
    /// Performs a validation check and returns a BadRequest response if the condition is false.
    /// This method is designed to be called at the beginning of an action method
    /// to handle common validation scenarios like ID mismatches.
    /// </summary>
    /// <param name="condition">The boolean condition to check. If false, validation fails.</param>
    /// <param name="errorMessage">The error message to include in the response if validation fails.</param>
    /// <param name="errorType">The type of error (defaults to ErrorTypeEnum.Validation).</param>
    /// <returns>An <see cref="IActionResult"/> representing a BadRequest if validation fails, otherwise <c>null</c>.</returns>
    protected IActionResult? ValidateRequest(bool condition, string errorMessage, ErrorTypeEnum errorType = ErrorTypeEnum.Validation)
    {
        if (!condition)
        {
            _logger.LogWarning("Validation failed: {ErrorMessage}", errorMessage);
            return HandleServiceResult(Result.Fail(errorMessage, errorType));
        }
        return null;
    }
}