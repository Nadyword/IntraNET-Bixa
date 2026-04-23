using Bixa.Backend.Controllers.SecurityControllers;
using Bixa.Backend.DataAccess.Context;
using Bixa.Backend.DataAccess.Interfaces.Repositories;
using Bixa.Backend.DataAccess.Wrappers;
using Bixa.Backend.Models;
using Bixa.Backend.Models.Auth;
using Bixa.Backend.Models.DTOs;
using Bixa.Backend.Services.Interfaces;
using Bixa.Backend.Services.Services.JwtControllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Runtime.Intrinsics.X86;

namespace Bixa.Backend.Controllers.LoginController;

[ApiController]
[Route("api/[controller]")]
public class LoginController(IManejoJwt manejoJwt,
                       AppDbContext dbContext,
                       IConfiguration configuration,
                       LoggerWrapper loggerWrapper,
                       IAuthRepository authRepository,
                       IUnitOfWork unitOfWork,
                       IReadOnlyUnitOfWork _IReadOnlyUnitOfWork,
                       ISendMailServices sendMailServices) : ControllerBase
{
    private readonly IAuthRepository _authRepository = authRepository ?? throw new ArgumentNullException(nameof(authRepository));
    private readonly IConfiguration _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    private readonly AppDbContext _context = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    private readonly LoggerWrapper _loggerWrapper = loggerWrapper ?? throw new ArgumentNullException(nameof(loggerWrapper));
    private readonly IManejoJwt _manejoJwt = manejoJwt ?? throw new ArgumentNullException(nameof(manejoJwt));
    private readonly IUnitOfWork _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    private readonly IReadOnlyUnitOfWork _readOnlyUnitOfWork = _IReadOnlyUnitOfWork ?? throw new ArgumentNullException(nameof(_IReadOnlyUnitOfWork));
    private readonly ISendMailServices _sendMailServices = sendMailServices ?? throw new ArgumentNullException(nameof(sendMailServices));

    /// <summary>
    /// Authenticates a user and returns a JWT token along with a refresh token
    /// </summary>
    /// <param name="credentials">User credentials</param>
    /// <returns>Token the authentication</returns>
    [AllowAnonymous]
    [HttpPost("AuthenticateLogin")]
    [ProducesResponseType(typeof(ApiResponse<LoginDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Authenticate([FromBody] UserCredentials credentials)
    {
        var authUser = new AuthUser(_manejoJwt, _context, _loggerWrapper, _configuration, _authRepository, _unitOfWork);
        return await authUser.Authenticate(credentials);
    }

    /// <summary>
    /// Set the user's final password upon their first login.
    /// </summary>
    /// <param name="credentials"> Email address and new password of the new user</param>
    /// <returns>Confirmation of the key change and the new authentication token.</returns>
    [AllowAnonymous]
    [HttpPost("FirstLogin")]
    [ProducesResponseType(typeof(ApiResponse<LoginDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> FirstLogin([FromBody] UserFirstLoginDTO? credentials)
    {
        // Pass all required dependencies to AuthUser constructor
        var authUser = new AuthUser(_manejoJwt, _context, _loggerWrapper, _configuration, _authRepository, _unitOfWork);
        return await authUser.ChangePasswordReturnCredentials(credentials);
    }

    /// <summary>
    /// Refreshes a JWT token using a refresh token
    /// </summary>
    /// <param name="request">Refresh token</param>
    /// <returns>Refresh token result</returns>
    [AllowAnonymous]
    [HttpPost("RefreshToken")]
    [ProducesResponseType(typeof(ApiResponse<LoginDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDTO request)
    {
        var authUser = new AuthUser(_manejoJwt, _context, _loggerWrapper, _configuration, _authRepository, _unitOfWork);
        return await authUser.RefreshToken(request);
    }

    /// <summary>
    /// Validates a JWT token
    /// </summary>
    /// <param name="request">Token validation request</param>
    /// <returns>Validation result</returns>
    [AllowAnonymous]
    [HttpPost("ValidateToken")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public IActionResult ValidateToken([FromBody] TokenValidationRequest? request)
    {
        var authUser = new AuthUser(_manejoJwt, _context, _loggerWrapper, _configuration, _authRepository, _unitOfWork);
        return authUser.ValidateToken(request);
    }

    /// <summary>
    /// Recover key
    /// </summary>
    /// <param name="request">Token validation request</param>
    /// <returns>Validation result</returns>
    [AllowAnonymous]
    [HttpPost("RetrievePassword")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RetrievePassword([FromBody] CiCheckRequestDTO request)
    {
        if (string.IsNullOrWhiteSpace(request.Ci))
            return BadRequest(ApiResponse<object>.BadRequest(null, "La C.I es requerida."));

        var user = await _unitOfWork.Users.GetUserByTaxIdAsync(request.Ci);

        if (user == null)
            return NotFound(ApiResponse<object>.NotFoundResponse("No se encontró ningún usuario asociado a la C.I proporcionada."));

        var email = await _readOnlyUnitOfWork.SnEmple.GetEmailByCiAsync(request.Ci);

        if (email == null || string.IsNullOrWhiteSpace(email))
            return NotFound(ApiResponse<object>.NotFoundResponse("No se encontró ningún correo asociado a la C.I proporcionada."));

        var authUser = new AuthUser(_manejoJwt, _context, _loggerWrapper, _configuration, _authRepository, _unitOfWork);

        string token = await authUser.RefreshToken(user);

        await _sendMailServices.SendMailRetrievePassword(email, token);

        return Ok(ApiResponse<string>.SuccessResponse(email, "Correo electrónico obtenido correctamente."));
    }
}