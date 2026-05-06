using Bixa.Backend.DataAccess.Interfaces.Repositories;
using Bixa.Backend.Services.Services.JwtControllers;
using Bixa.Backend.DataAccess.Wrappers;
using Bixa.Backend.DataAccess.Entities;
using Bixa.Backend.Controllers.Services;
using System.IdentityModel.Tokens.Jwt;
using Bixa.Backend.Services.Interfaces;
using Bixa.Backend.Services.Services;
using Microsoft.IdentityModel.Tokens;
using Bixa.Backend.Models.Utilities;
using Bixa.Backend.Models.DTOs;
using Bixa.Backend.Models.Auth;
using Microsoft.AspNetCore.Mvc;
using Bixa.Backend.Models;
using System.Text;

namespace Bixa.Backend.Controllers.SecurityControllers;

/// <summary>
/// Handles user authentication and token operations (authentication, refresh, validation)
/// </summary>
public class AuthUser
{
    private readonly IAuthRepository _authRepository;
    private readonly AuthService _authService;
    private readonly IConfiguration _configuration;
    private readonly HandleError _handleError;
    private readonly ILogger<AuthUser> _logger;
    private readonly IManejoJwt _manejoJwt;
    private readonly ResponseService _responseService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IReadOnlyUnitOfWork _readOnlyUnitOfWork;
    private readonly ISendMailServices _sendMailServices;

    /// <summary>
    /// Initializes a new instance of the AuthUser
    /// </summary>
    /// <param name="manejoJwt">JWT management service</param>
    /// <param name="loggerWrapper">Logger wrapper instance</param>
    /// <param name="configuration">Application configuration</param>
    /// <param name="authRepository">The repository for authentication data access.</param>
    /// <param name="unitOfWork">The Unit of Work for managing database transactions.</param>
    /// <param name="readOnlyUnitOfWork">The Read-Only Unit of Work for read-only database operations.</param>
    /// <param name="sendMailServices">Service for sending emails.</param>
    public AuthUser(IManejoJwt manejoJwt,
                    LoggerWrapper loggerWrapper,
                    IConfiguration configuration,
                    IAuthRepository authRepository,
                    IUnitOfWork unitOfWork,
                    IReadOnlyUnitOfWork readOnlyUnitOfWork,
                    ISendMailServices sendMailServices)
    {
        _manejoJwt = manejoJwt ?? throw new ArgumentNullException(nameof(manejoJwt));
        _logger = loggerWrapper?.CreateLogger<AuthUser>() ?? throw new ArgumentNullException(nameof(loggerWrapper));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _authRepository = authRepository ?? throw new ArgumentNullException(nameof(authRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _readOnlyUnitOfWork = readOnlyUnitOfWork ?? throw new ArgumentNullException(nameof(readOnlyUnitOfWork));
        _sendMailServices = sendMailServices ?? throw new ArgumentNullException(nameof(sendMailServices));

        _authService = new AuthService(
            _authRepository,
            _unitOfWork,
            loggerWrapper,
            _manejoJwt,
            _readOnlyUnitOfWork
        );

        _responseService = new ResponseService();
        _handleError = new HandleError();
    }

    /// <summary>
    /// Authenticates a user and returns a JWT token
    /// </summary>
    /// <param name="credentials">User credentials</param>
    /// <returns>Authentication result</returns>
    public async Task<IActionResult> Authenticate([FromBody] UserCredentials? credentials)
    {
        if (credentials == null || string.IsNullOrWhiteSpace(credentials.Ci) || string.IsNullOrWhiteSpace(credentials.Password))
            return _responseService.CreateResponse(ApiResponse<object>.BadRequest(null, "Invalid Credentials."));

        var result = await _authService.Authenticate(credentials);

        return result.Match(
            success => _responseService.CreateResponse(ApiResponse<LoginDTO>.SuccessResponse(success)),
            error => _handleError.HandleErrorResult(error)
        );
    }

    /// <summary>
    /// Authenticates a user for the first time and returns a JWT token
    /// </summary>
    /// <param name="credentials">User credentials</param>
    /// <returns>Authentication result</returns>
    public async Task<IActionResult> ChangePasswordReturnCredentials([FromBody] UserFirstLoginDTO? credentials)
    {
        if (credentials == null || string.IsNullOrWhiteSpace(credentials.Ci))
            return _responseService.CreateResponse(ApiResponse<object>.BadRequest(null, "Invalid Credentials."));

        var result = await _authService.ChangePasswordReturnCredentials(credentials);

        return result.Match(
            success => _responseService.CreateResponse(ApiResponse<LoginDTO>.SuccessResponse(success)),
            error => _handleError.HandleErrorResult(error)
        );
    }

    /// <summary>
    /// Refreshes a JWT token using a refresh token
    /// </summary>
    /// <param name="request">Refresh token</param>
    /// <returns>Refresh token result</returns>
    public async Task<IActionResult> RefreshToken(RefreshTokenRequestDTO request)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
            return _responseService.CreateResponse(ApiResponse<object>.BadRequest(null, "Please Provide Token"));

        var result = await _authService.Refresh(request);

        return result.Match(
            success => _responseService.CreateResponse(ApiResponse<LoginDTO>.SuccessResponse(success)),
            error => _handleError.HandleErrorResult(error)
        );
    }

    /// <summary>
    /// Refreshes a JWT token using a refresh token
    /// </summary>
    /// <param name="users">User entity containing the refresh token</param>
    /// <returns>Refresh token result</returns>
    public async Task<string> RefreshToken(Users users)
    {
        return await _authService.Refresh(users);
    }

    /// <summary>
    /// Validates a JWT token.
    /// </summary>
    /// <param name="request">Token validation request containing the token string.</param>
    /// <returns>An <see cref="IActionResult"/> indicating the validation result (OK, BadRequest, Unauthorized, InternalServerError).</returns>
    public async Task<IActionResult> ValidateToken(TokenValidationRequest? request)
    {
        try
        {
            if (request == null || string.IsNullOrEmpty(request.Token))
            {
                _logger.LogWarning("Token validation failed: Request or Token is missing.");
                return new BadRequestObjectResult(new { Valid = false, Message = "Token is required." });
            }

            var jwtConfigurationSection = _configuration.GetSection("ConfiguracionJwt");

            if (jwtConfigurationSection == null)
            {
                _logger.LogError("JWT configuration section 'ConfiguracionJwt' is missing.");
                return new ObjectResult(new { Valid = false, Message = "Server configuration error." }) { StatusCode = StatusCodes.Status500InternalServerError };
            }

            string? keyString = jwtConfigurationSection["Llave"];
            string? issuer = jwtConfigurationSection["Issuer"];
            string? audience = jwtConfigurationSection["Audience"];

            if (string.IsNullOrEmpty(keyString) || string.IsNullOrEmpty(issuer) || string.IsNullOrEmpty(audience))
            {
                _logger.LogError("Missing required JWT configuration values (Llave, Issuer, or Audience).");
                return new ObjectResult(new { Valid = false, Message = "Server JWT configuration is incomplete." }) { StatusCode = StatusCodes.Status500InternalServerError };
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(keyString);

            tokenHandler.ValidateToken(request.Token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidIssuer = issuer,
                ValidAudience = audience,
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);

            return new OkObjectResult(new { Valid = true });
        }
        catch (SecurityTokenException ex)
        {
            _logger.LogError(ex, "Token validation failed. Invalid token or signature.");
            return new UnauthorizedObjectResult(new { Valid = false, Message = "Invalid or expired token." });
        }
        catch (ArgumentException ex)
        {
            _logger.LogError(ex, "Token validation failed. Invalid token format.");
            return new BadRequestObjectResult(new { Valid = false, Message = "Invalid token format." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred during token validation. Token: {Token}", request?.Token);
            return new ObjectResult(new { Valid = false, Message = "An unexpected error occurred." }) { StatusCode = StatusCodes.Status500InternalServerError };
        }
    }

    public async Task<IActionResult> RetrievePassword(string ci)
    {
        if (string.IsNullOrWhiteSpace(ci))
            return new BadRequestObjectResult(new { Valid = false, Message = "La C.I es requerida." });

        string normalizedCi = UtilityService.NormalizeCiFormat(ci);
        var user = await _unitOfWork.Users.GetUserByCiAsync(normalizedCi);

        if (user == null)
            return new BadRequestObjectResult(new { Valid = false, Message = "No se encontró ningún usuario asociado a la C.I proporcionada." });

        var email = await _readOnlyUnitOfWork.SnEmple.GetEmailByCiAsync(normalizedCi);

        if (string.IsNullOrWhiteSpace(email))
            return new BadRequestObjectResult(new { Valid = false, Message = "No se encontró ningún correo asociado a la C.I proporcionada." });

        string token = await RefreshToken(user);

        _ = Task.Run(async () => _ = _sendMailServices.SendMailRetrievePassword(email, token, normalizedCi));

        return new OkObjectResult(new { Valid = true, Message = "Se ha enviado un correo para recuperar la contraseña." });
    }
}