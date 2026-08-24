using Bixa.Backend.Models.DTOs.UserModelDTO;
using Bixa.Backend.DataAccess.Entities.DbProfit;
using Bixa.Backend.DataAccess.Wrappers;
using Microsoft.AspNetCore.Authorization;
using Bixa.Backend.Services.Interfaces;
using Bixa.Backend.Models.Enums;
using Bixa.Backend.Models.Response;
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
[Authorize]
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
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(ApiResponse<int>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateUser([FromForm] UserInsertDTO user)
    {
        var authResult = RequireUserRol(UserRolEnum.Administrador);
        if (authResult != null) return authResult;

        var result = await _userService.AddAsync(user);
        return HandleServiceResult(result, "Usuario creado exitosamente");
    }

    /// <summary>
    /// Desactiva un usuario del sistema (baja lógica). El usuario deja de aparecer en los listados
    /// y no podrá iniciar sesión, pero sus datos se conservan y puede reactivarse creándolo nuevamente.
    /// DELETE /api/users/{ci}
    /// </summary>
    /// <param name="ci">Cedula of the user to deactivate.</param>
    /// <returns>API response indicating the operation result.</returns>
    [HttpDelete("{ci}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DeleteUser(string ci)
    {
        var authResult = RequireUserRol(UserRolEnum.Administrador);
        if (authResult != null) return authResult;

        var result = await _userService.DeleteAsync(ci);
        return HandleServiceResult(result, "Usuario desactivado exitosamente.");
    }

    /// <summary>
    /// Retrieves a single user by their CI.
    /// GET /api/users/{ci}
    /// </summary>
    /// <param name="ci">The CI of the user to retrieve.</param>
    /// <returns>API response containing the UserDTO if found.</returns>
    [HttpGet("{ci}")]
    [ProducesResponseType(typeof(ApiResponse<UserDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetUserByCi(string ci)
    {
        var authResult = RequireUserRol(UserRolEnum.Administrador, UserRolEnum.Empleado);
        if (authResult != null) return authResult;

        var result = await _userService.GetUserByCiAsync(ci);
        return HandleServiceResult(result);
    }

    /// <summary>
    /// Retrieves a paginated list of users based on specified filters.
    /// GET /api/users?pageNumber=1&amp;pageSize=10&amp;filters.Name=John
    /// </summary>
    ///<param name="pageNumber">The page number for pagination (default is 1).</param>
    ///<param name="pageSize">The number of items per page for pagination (default is 10).</param>
    /// <returns>API response containing paginated UserDTO results.</returns>
    [HttpGet("{pageNumber:int}/{pageSize:int}")]
    [ProducesResponseType(typeof(ApiResponse<List<SnEmpleDTO>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetUsers(int pageNumber = 1, int pageSize = 10)
    {
        var authResult = RequireUserRol(UserRolEnum.Administrador, UserRolEnum.Supervisor);
        if (authResult != null) return authResult;

        var listUser = await _userService.GetAllAsync(pageNumber, pageSize);

        return HandleServiceResult(await _userService.GetAllByCiAsync(listUser.Value));
    }

    /// <summary>
    /// Retrieves todo el personal a cargo (directo e indirecto) del supervisor autenticado.
    /// GET /api/users/mi-equipo
    /// </summary>
    /// <returns>API response containing the list of employees under the supervisor's charge.</returns>
    [HttpGet("mi-equipo")]
    [ProducesResponseType(typeof(ApiResponse<List<EquipoSupervisor>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetMiEquipo()
    {
        var authResult = RequireUserRol(UserRolEnum.Supervisor, UserRolEnum.Administrador);
        if (authResult != null) return authResult;

        var ci = User.FindFirst("ci")?.Value;
        if (string.IsNullOrEmpty(ci))
            return HandleServiceResult(Result.Fail<List<EquipoSupervisor>>("No se pudo identificar al usuario autenticado.", ErrorTypeEnum.Unauthorized));

        var result = await _userService.GetEquipoSupervisorAsync(ci);
        return HandleServiceResult(result);
    }

    /// <summary>
    /// Updates an existing user's information.
    /// PUT /api/users/{id}
    /// </summary>
    /// <param name="userEdited">User data transfer object with updated information.</param>
    /// <returns>API response indicating the operation result.</returns>
    [HttpPut]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpdateUser([FromForm] UserEditDTO userEdited)
    {
        var authResult = RequireUserRol(UserRolEnum.Administrador);
        if (authResult != null) return authResult;

        userEdited.Password = null; // Ensure password is not updated in this endpoint

        var resultUser = await _userService.UpdateAsync(userEdited);

        return HandleServiceResult(resultUser, $"Usuario con CI {userEdited.Ci} actualizado");
    }

    /// <summary>
    /// Updates an existing user's information.
    /// PUT /api/users/{id}
    /// </summary>
    /// <param name="userEdited">User data transfer object with updated information.</param>
    /// <param name="PassNew">The new password for the user.</param>
    /// <returns>API response indicating the operation result.</returns>
    [HttpPut("ChangePassword/{PassNew}")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpdateUserPassword([FromBody] UserEditDTO userEdited, string PassNew)
    {
        var authResult = RequireUserRol(UserRolEnum.Administrador);
        if (authResult != null) return authResult;

        var resultUser = await _userService.UpdateUserPassword(userEdited, PassNew);

        return HandleServiceResult(resultUser, $"Usuario con CI {userEdited.Ci} actualizado");
    }

    ///<summary>
    /// POST reenvío de correo de bienvenida
    ///</summary>
    ///<param name="Ci">The CI of the user to resend the welcome email to.</param>
    ///<returns>API response indicating the operation result.</returns>
    [HttpPost("ResendWelcomeEmail/{Ci}")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ResendWelcomeEmail(string Ci)
    {
        var authResult = RequireUserRol(UserRolEnum.Administrador);
        if (authResult != null) return authResult;

        var resultUser = await _userService.ResendWelcomeEmail(Ci);

        return HandleServiceResult(resultUser, $"Si existe un correo asociado, se ha reenviado el correo de bienvenida al usuario con CI {Ci}");
    }

    /// <summary>
    /// Envía una solicitud de corrección de datos personales a todos los administradores activos.
    /// POST /api/users/SolicitarCorreccion
    /// </summary>
    /// <param name="dto">El comentario con el detalle de la corrección solicitada.</param>
    /// <returns>API response indicating the operation result.</returns>
    [HttpPost("SolicitarCorreccion")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> SolicitarCorreccion([FromBody] SolicitarCorreccionDTO dto)
    {
        var authResult = RequireUserRol(UserRolEnum.Administrador, UserRolEnum.Supervisor, UserRolEnum.Empleado);
        if (authResult != null) return authResult;

        var ci = User.FindFirst("ci")?.Value;
        if (string.IsNullOrEmpty(ci))
            return HandleServiceResult(Result.Fail<bool>("No se pudo identificar al usuario autenticado.", ErrorTypeEnum.Unauthorized));

        var result = await _userService.SolicitarCorreccion(ci, dto.Comentario);
        return HandleServiceResult(result, "Solicitud de corrección enviada a los administradores.");
    }
}