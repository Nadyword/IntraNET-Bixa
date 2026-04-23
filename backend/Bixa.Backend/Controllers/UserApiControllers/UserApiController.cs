using Bixa.Backend.Models.DTOs.UserModelDTO;
using Bixa.Backend.DataAccess.Wrappers;
using Bixa.Backend.Services.Interfaces;
using Bixa.Backend.Models.Response;
using Bixa.Backend.Models.Enums;
using Bixa.Backend.Models.Query;
using Microsoft.AspNetCore.Mvc;
using Bixa.Backend.Models;
using Bixa.Backend.Base;
using AutoMapper;

namespace Bixa.Backend.Controllers.UserApiControllers;

/// <summary>
/// API Controller for user management operations following RESTful standards.
/// Now uses BaseApiController's helper methods for authorization and validation.
/// </summary>
/// <remarks>
/// Initializes a new instance of the UserApiController.
/// </remarks>
/// <param name="mapper">AutoMapper instance for DTO conversions.</param>
/// <param name="loggerWrapper">Logger wrapper for logging operations.</param>
/// <param name="userService">The user service instance for business logic.</param>
[ApiController]
[Route("api/users")]
public class UserApiController(
    IMapper mapper,
    LoggerWrapper loggerWrapper,
    IUserService userService) : BaseApiController(mapper, loggerWrapper)
{
    private readonly IUserService _userService = userService;

    /// <summary>
    /// Creates a new user in the system.
    /// POST /api/users
    /// </summary>
    /// <param name="user">User data transfer object for creation.</param>
    /// <returns>API response with the Id of the created user.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<int>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateUser([FromBody] UserInsertDTO user)
    {
        // Solo los superintendentes pueden crear usuarios.
        var authResult = RequireUserRol(UserRolEnum.SuperIntendente);
        if (authResult != null) return authResult;

        var result = await _userService.AddAsync(user);
        return HandleServiceResult(result, "Usuario creado exitosamente");
    }

    /// <summary>
    /// Deletes a user from the system.
    /// DELETE /api/users/{id}
    /// </summary>
    /// <param name="id">Id of the user to delete.</param>
    /// <returns>API response indicating the operation result.</returns>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DeleteUser(int id)
    {
        var authResult = RequireUserRol(UserRolEnum.SuperIntendente);
        if (authResult != null) return authResult;

        var result = await _userService.DeleteAsync(id);
        return HandleServiceResult(result, "Eliminación completada");
    }

    /// <summary>
    /// Retrieves a single user by their ID.
    /// GET /api/users/{id}
    /// </summary>
    /// <param name="id">The ID of the user to retrieve.</param>
    /// <returns>API response containing the UserDTO if found.</returns>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<UserDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetUserById(int id)
    {
        var authResult = RequireUserRol(UserRolEnum.SuperIntendente, UserRolEnum.Empleado);
        if (authResult != null) return authResult;

        var result = await _userService.GetUserByIdAsync(id);
        return HandleServiceResult(result);
    }

    /// <summary>
    /// Retrieves a paginated list of users based on specified filters.
    /// GET /api/users?pageNumber=1&amp;pageSize=10&amp;filters.Name=John
    /// </summary>
    /// <param name="filters">Filtering, sorting, and pagination parameters.</param>
    /// <returns>API response containing paginated UserDTO results.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResult<UserDTO>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetUsers([FromQuery] SearchQuery<UserFilterDTO> filters)
    {
        var authResult = RequireUserRol(UserRolEnum.SuperIntendente, UserRolEnum.Empleado);
        if (authResult != null) return authResult;

        var result = await _userService.GetAllAsync(filters);
        return HandleServiceResult(result);
    }

    /// <summary>
    /// Updates an existing user's information.
    /// PUT /api/users/{id}
    /// </summary>
    /// <param name="id">The ID of the user to update. Must match userEdited.Id.</param>
    /// <param name="userEdited">User data transfer object with updated information.</param>
    /// <returns>API response indicating the operation result.</returns>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpdateUser(int id, [FromBody] UserEditDTO userEdited)
    {
        var authResult = RequireUserRol(UserRolEnum.SuperIntendente);
        if (authResult != null) return authResult;

        var validationError = ValidateRequest(
            id == userEdited.Id,
            $"Route ID ({id}) does not match User ID in body ({userEdited.Id})",
            ErrorTypeEnum.Validation);

        if (validationError != null)
            return validationError;

        var result = await _userService.UpdateAsync(userEdited);
        return HandleServiceResult(result, $"Usuario con Id {userEdited.Id} actualizado");
    }

    /// <summary>
    /// Updates a user's password. This is a specific action, so we can use a dedicated endpoint.
    /// PUT /api/users/{id}/password
    /// </summary>
    /// <param name="id">The ID of the user whose password is to be updated.</param>
    /// <param name="passwordChange">Password change data transfer object.</param>
    /// <returns>API response indicating the operation result.</returns>
    [HttpPut("{id:int}/password")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpdateUserPassword(int id, [FromBody] UserChangePasswordDTO passwordChange)
    {
        var authResult = RequireUserRol(UserRolEnum.SuperIntendente);
        if (authResult != null) return authResult;

        var validationError = ValidateRequest(
            id == passwordChange.Id,
            $"Route ID ({id}) does not match User ID in body ({passwordChange.Id})",
            ErrorTypeEnum.Validation);

        if (validationError != null)
            return validationError;

        var result = await _userService.UpdateUserPassword(passwordChange);
        return HandleServiceResult(result, "Tu contraseña ha sido cambiada exitosamente");
    }
}