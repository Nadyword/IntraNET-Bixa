using Bixa.Backend.DataAccess.Interfaces.Repositories;
using Bixa.Backend.Services.Services.JwtControllers;
using Bixa.Backend.DataAccess.Wrappers;
using Microsoft.AspNetCore.Authorization;
using Bixa.Backend.DataAccess.Context;
using Bixa.Backend.SecurityControllers;
using Bixa.Backend.Models.DTOs;
using Bixa.Backend.Models.Auth;
using Microsoft.AspNetCore.Mvc;
using Bixa.Backend.Models;

namespace Bixa.Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LoginController(IManejoJwt manejoJwt,
                       AppDbContext dbContext,
                       IConfiguration configuration,
                       LoggerWrapper loggerWrapper,
                       IAuthRepository authRepository,
                       IUnitOfWork unitOfWork) : ControllerBase
{
    private readonly IAuthRepository _authRepository = authRepository ?? throw new ArgumentNullException(nameof(authRepository));
    private readonly IConfiguration _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    private readonly AppDbContext _context = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    private readonly LoggerWrapper _loggerWrapper = loggerWrapper ?? throw new ArgumentNullException(nameof(loggerWrapper));
    private readonly IManejoJwt _manejoJwt = manejoJwt ?? throw new ArgumentNullException(nameof(manejoJwt));
    private readonly IUnitOfWork _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));

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
    [HttpPost("RecoverKey")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public void RecoverKey([FromBody] UserCredentials request)
    {
    }
}