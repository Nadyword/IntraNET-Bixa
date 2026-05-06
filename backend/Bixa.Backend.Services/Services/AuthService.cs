using Bixa.Backend.DataAccess.Interfaces.Repositories;
using Bixa.Backend.Services.Services.JwtControllers;
using Bixa.Backend.DataAccess.Wrappers;
using Bixa.Backend.DataAccess.Entities;
using Bixa.Backend.Models.Response;
using Bixa.Backend.Models.Utilities;
using Microsoft.Extensions.Logging;
using Bixa.Backend.Models.Enums;
using Bixa.Backend.Models.DTOs;
using Bixa.Backend.Models.Auth;

namespace Bixa.Backend.Services.Services;

/// <summary>
/// Service layer for authentication operations.
/// This service now leverages the Unit of Work pattern for transaction management
/// and interacts with repositories through Interfaces.
/// </summary>
/// <remarks>
/// Initializes a new instance of the AuthService.
/// </remarks>
/// <param name="authRepository">The repository for authentication data access.</param>
/// <param name="unitOfWork">The Unit of Work for managing database transactions.</param>
/// <param name="loggerWrapper">Logger instance wrapper.</param>
/// <param name="manejoJwt">JWT management service.</param>
public class AuthService(
    IAuthRepository authRepository,
    IUnitOfWork unitOfWork,
    LoggerWrapper loggerWrapper,
    IManejoJwt manejoJwt,
    IReadOnlyUnitOfWork readOnlyUnitOfWork)
{
    private readonly IAuthRepository _authRepository = authRepository ?? throw new ArgumentNullException(nameof(authRepository));
    private readonly IManejoJwt _manejoJwt = manejoJwt ?? throw new ArgumentNullException(nameof(manejoJwt));
    private readonly IUnitOfWork _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    private readonly IReadOnlyUnitOfWork _readOnlyUnitOfWork = readOnlyUnitOfWork ?? throw new ArgumentNullException(nameof(readOnlyUnitOfWork));
    public ILogger<AuthService> Logger { get; } = loggerWrapper?.CreateLogger<AuthService>() ?? throw new ArgumentNullException(nameof(loggerWrapper));

    /// <summary>
    /// Authenticates a user and returns a login DTO.
    /// </summary>
    /// <param name="credentials">User credentials.</param>
    /// <returns>Login DTO or error result.</returns>
    public async Task<Result<LoginDTO>> Authenticate(UserCredentials? credentials)
    {
        if (credentials == null || string.IsNullOrWhiteSpace(credentials.Ci) || string.IsNullOrWhiteSpace(credentials.Password))
        {
            Logger.LogWarning("Intento de autenticación con credenciales inválidas o faltantes.");
            return Result.Fail<LoginDTO>("Credenciales inválidas", ErrorTypeEnum.BadRequest);
        }

        try
        {
            var normalizedCi = UtilityService.NormalizeCiFormat(credentials.Ci);
            var user = await _authRepository.GetUserByCi(normalizedCi);

            if (user == null)
            {
                Logger.LogWarning("Intento de autenticación con credenciales inválidas o faltantes para el CI {Ci}.", credentials.Ci);
                return Result.Fail<LoginDTO>("Credenciales inválidas", ErrorTypeEnum.Unauthorized);
            }

            if (!Hasher.VerifyPassword(credentials.Password, user!.PasswordHash!))
            {
                Logger.LogWarning("Autenticación fallida para el usuario {UserId}: Contraseña inválida.", user.Id);
                return Result.Fail<LoginDTO>("Credenciales inválidas", ErrorTypeEnum.Unauthorized);
            }

            //If its first login, send auth for changing password and signature if necessary
            if (user.LastLogin == null)
            {
                var (firstToken, Refresh) = await GenerateTokenForFirstLogin(user);
                return Result.Success(new LoginDTO { Token = firstToken, RefreshToken = Refresh });
            }

            var roleValidationResult = ValidateUserRole(user);
            if (!roleValidationResult.IsSuccess)
            {
                return roleValidationResult;
            }

            var (token, refreshToken) = await GenerateAndSaveRefreshToken(user).ConfigureAwait(false);

            return Result.Success(new LoginDTO { Token = token, RefreshToken = refreshToken });
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error al autenticar usuario con CI {Ci}.", credentials?.Ci);
            return Result.Fail<LoginDTO>("Error al autenticar el usuario", ErrorTypeEnum.General);
        }
    }

    /// <summary>
    /// Authenticates a user for the first time and returns a login DTO.
    /// </summary>
    /// <param name="credentials">User credentials.</param>
    /// <returns>Login DTO or error result.</returns>
    public async Task<Result<LoginDTO>> ChangePasswordReturnCredentials(UserFirstLoginDTO? credentials)
    {
        if (credentials == null || string.IsNullOrWhiteSpace(credentials.Ci))
        {
            Logger.LogWarning("Intento de autenticación con credenciales inválidas o faltantes.");
            return Result.Fail<LoginDTO>("Credenciales inválidas", ErrorTypeEnum.BadRequest);
        }

        try
        {
            var normalizedCi = UtilityService.NormalizeCiFormat(credentials.Ci);
            var user = await _authRepository.GetUserByCi(normalizedCi);

            if (user == null)
            {
                Logger.LogWarning("Intento de autenticación con credenciales inválidas o faltantes para el CI {Ci}.", credentials.Ci);
                return Result.Fail<LoginDTO>("Credenciales inválidas", ErrorTypeEnum.Unauthorized);
            }

            var roleValidationResult = ValidateUserRole(user);
            if (!roleValidationResult.IsSuccess)
            {
                return roleValidationResult;
            }

            ArgumentException argumentException = new("El rol de usuario no puede ser nulo o vacío para la generación del token.", nameof(user.UserRol));
            var (token, refreshToken, listErros) = await ChangePasswordAndReturnToken(user, credentials.NewPassword, argumentException).ConfigureAwait(false);

            if (listErros.Count > 0)
            {
                return Result.Fail<LoginDTO>(string.Join("\n", listErros), ErrorTypeEnum.General);
            }

            return Result.Success(new LoginDTO { Token = token, RefreshToken = refreshToken });
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error al autenticar usuario con CI {Ci}.", credentials?.Ci);
            return Result.Fail<LoginDTO>("Error al autenticar el usuario", ErrorTypeEnum.General);
        }
    }

    /// <summary>
    /// Refreshes a JWT token using a refresh token.
    /// </summary>
    /// <param name="refreshToken">Refresh token.</param>
    /// <returns>Refreshed token or error result.</returns>
    // Removed unused _lock field.
    public async Task<Result<LoginDTO>> Refresh(RefreshTokenRequestDTO refreshToken)
    {
        if (string.IsNullOrWhiteSpace(refreshToken.RefreshToken))
        {
            Logger.LogWarning("Intento de refresco de token con token nulo o vacío.");
            return Result.Fail<LoginDTO>("Token de refresco inválido", ErrorTypeEnum.BadRequest);
        }

        try
        {
            var user = await _authRepository.GetUserByRefreshToken(refreshToken.RefreshToken, DateTime.UtcNow);

            if (user == null)
            {
                Logger.LogWarning("Fallo en el refresco del token {RefreshToken}: Usuario no encontrado o token expirado/inválido.", refreshToken);
                return Result.Fail<LoginDTO>("Token de refresco inválido o expirado.", ErrorTypeEnum.Unauthorized);
            }

            var (token, newRefreshToken) = await GenerateAndSaveRefreshToken(user).ConfigureAwait(false);

            var result = new LoginDTO
            {
                Token = token,
                RefreshToken = newRefreshToken,
            };

            return Result.Success(result);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error al refrescar token con token {RefreshToken}.", refreshToken);
            return Result.Fail<LoginDTO>("Error al refrescar el token", ErrorTypeEnum.General);
        }
    }

    /// <summary>
    /// Refreshes a JWT token using a refresh token.
    /// </summary>
    /// <param name="user">User associated with the refresh token.</param>
    /// <returns>Refreshed token or error result.</returns>
    // Removed unused _lock field.
    public async Task<string> Refresh(Users user)
    {
        if (string.IsNullOrWhiteSpace(user.RefreshToken))
        {
            return "Intento de refresco de token con token nulo o vacío.";
        }

        try
        {
            if (user == null)
            {
                return $"Fallo en el refresco del token {user!.RefreshToken}: Usuario no encontrado o token expirado/inválido.";
            }

            var (token, newRefreshToken) = await GenerateAndSaveRefreshToken(user).ConfigureAwait(false);

            var result = new LoginDTO
            {
                Token = token,
                RefreshToken = newRefreshToken,
            };

            return token;
        }
        catch (Exception ex)
        {
            return $"Error al refrescar token con token {user.RefreshToken}. Error: {ex.Message}";
        }
    }

    #region Private Methods

    /// <summary>
    /// Validates that a user has the required establishment associations based on their role.
    /// </summary>
    /// <param name="user">The user to validate.</param>
    /// <returns>Success result if valid, failure result with appropriate error if invalid.</returns>
    private static Result<LoginDTO> ValidateUserRole(Users user)
    {
        _ = (UserRolEnum)user.IdUserRol;
        return Result.Success<LoginDTO>(null!);
    }

    /// <summary>
    /// Updates user authentication tokens and timestamps.
    /// </summary>
    /// <param name="user">The user to update.</param>
    /// <param name="refreshToken">The new refresh token.</param>
    private static void UpdateUserAuthenticationData(Users user, string refreshToken)
    {
        user.RefreshToken = refreshToken;
        user.RefreshTokenDate = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc);
        user.LastLogin = DateTime.UtcNow;
    }

    /// <summary>
    /// Builds authentication token claims from a user entity.
    /// </summary>
    /// <param name="user">The user entity.</param>
    /// <returns>AuthTokenClaims object.</returns>
    /// <exception cref="ArgumentException">Thrown if user role is invalid.</exception>
    private AuthTokenClaims BuildAuthTokenClaims(Users user, ArgumentException argumentException)
    {
        string userRolName = user.UserRol?.Name ?? "";
        string userRolIdEnum = user.IdUserRol.ToString();

        if (string.IsNullOrEmpty(userRolName))
        {
            Logger.LogError("El rol de usuario (UserRol.Name) es nulo o vacío para el usuario {UserId}. Esto es necesario para la generación del token.", user.Id);
            throw argumentException;
        }

        return new AuthTokenClaims
        {
            Id = user.Id,
            Name = user.FirstName ?? "",
            Ci = user.Ci ?? "",
            RolName = userRolName,
            RolId = userRolIdEnum
        };
    }

    /// <summary>
    /// Generates new access and refresh tokens, and saves the new refresh token to the user.
    /// This method performs the transactional update. ONLY FOR FIRST LOGIN
    /// </summary>
    /// <param name="user">The user entity to update.</param>
    /// <returns>A tuple containing the new access token and new refresh token.</returns>
    /// <exception cref="ArgumentNullException">Thrown if the user parameter is null.</exception>
    /// <exception cref="ArgumentException">Thrown if the user's role is null or empty.</exception>
    private async Task<(string token, string refreshToken, List<string> errores)> ChangePasswordAndReturnToken(Users user, string newPassword, ArgumentException argumentException)

    {
        try
        {
            if (user == null)
            {
                Logger.LogError("ChangePasswordAndReturnToken llamado con un usuario nulo.");
                throw new ArgumentNullException(nameof(user));
            }

            string userRolName = user.UserRol?.Name ?? "";

            if (string.IsNullOrEmpty(userRolName))
            {
                Logger.LogError("El rol de usuario (UserRol.Name) es nulo o vacío para el usuario {UserId}. Esto es necesario para la generación del token.", user.Id);
                throw new ArgumentException("El rol de usuario no puede ser nulo o vacío para la generación del token.");
            }

            if (string.IsNullOrEmpty(newPassword))
            {
                Logger.LogError("La contraseña nueva es nulo o vacío para el usuario {UserId}.", user.Id);
                throw new ArgumentException("La contraseña nueva no puede ser nulo o vacío.");
            }

            var errorList = ValidationUtils.IsValidPassword(newPassword);
            if (errorList.Count != 0)
            {
                return (string.Empty, string.Empty, errorList);
            }

            var authToken = BuildAuthTokenClaims(user, argumentException);
            var token = _manejoJwt.GenerarToken(authToken);
            var newRefreshToken = Convert.ToBase64String(Guid.NewGuid().ToByteArray());

            UpdateUserAuthenticationData(user, newRefreshToken);

            // Password and signature updates
            user.PasswordHash = Hasher.HashPassword(newPassword);

            var savedChanges = await _unitOfWork.SaveChangesAsync().ConfigureAwait(false);

            if (savedChanges == 0)
                Logger.LogWarning("No se guardaron cambios al intentar actualizar el token de refresco para el usuario {UserId}", user.Id);

            return (token, newRefreshToken, errorList);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error al generar o guardar el token de refresco para el usuario {UserId} en el servicio.", user?.Id);
            throw;
        }
    }

    /// <summary>
    /// Generates new access and refresh tokens, and saves the new refresh token to the user.
    /// This method performs the transactional update.
    /// </summary>
    /// <param name="user">The user entity to update.</param>
    /// <returns>A tuple containing the new access token and new refresh token.</returns>
    /// <exception cref="ArgumentNullException">Thrown if the user parameter is null.</exception>
    /// <exception cref="ArgumentException">Thrown if the user's role is null or empty.</exception>
    private async Task<(string token, string refreshToken)> GenerateAndSaveRefreshToken(Users user)
    {
        try
        {
            if (user == null)
            {
                Logger.LogError("GenerateAndSaveRefreshToken llamado con un usuario nulo.");
                throw new ArgumentNullException(nameof(user));
            }

            var authToken = BuildAuthTokenClaims(user, new("El rol de usuario no puede ser nulo o vacío para la generación del token.", nameof(user.UserRol)));
            var token = _manejoJwt.GenerarToken(authToken);
            var newRefreshToken = Convert.ToBase64String(Guid.NewGuid().ToByteArray());

            UpdateUserAuthenticationData(user, newRefreshToken);

            var savedChanges = await _unitOfWork.SaveChangesAsync().ConfigureAwait(false);

            if (savedChanges == 0)
                Logger.LogWarning("No se guardaron cambios al intentar actualizar el token de refresco para el usuario {UserId}", user.Id);

            return (token, newRefreshToken);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error al generar o guardar el token de refresco para el usuario {UserId} en el servicio.", user?.Id);
            throw;
        }
    }

    private async Task<(string, string)> GenerateTokenForFirstLogin(Users user)
    {
        try
        {
            if (user == null)
            {
                Logger.LogError("GenerateTokenForFirstLogin llamado con un usuario nulo.");
                throw new ArgumentNullException(nameof(user));
            }
            string userRolName = user.UserRol?.Name ?? "";
            if (string.IsNullOrEmpty(userRolName))
            {
                Logger.LogError("El rol de usuario (UserRol.Name) es nulo o vacío para el usuario {UserId}. Esto es necesario para la generación del token.", user.Id);
                ArgumentException argumentException = new("El rol de usuario no puede ser nulo o vacío para la generación del token.", nameof(user.UserRol));
                throw argumentException;
            }

            var firstLoginToken = Convert.ToBase64String(Guid.NewGuid().ToByteArray());

            user.RefreshToken = firstLoginToken;
            user.RefreshTokenDate = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc);

            var savedChanges = await _unitOfWork.SaveChangesAsync().ConfigureAwait(false);

            if (savedChanges == 0)
                Logger.LogWarning("No se guardaron cambios al intentar actualizar el token de refresco para el usuario {UserId}", user.Id);

            return (firstLoginToken, "FIRSTLOGIN");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error al generar o guardar el token de refresco para el usuario {UserId} en el servicio.", user?.Id);
            throw;
        }
    }

    #endregion Private Methods
}