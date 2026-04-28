using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using Bixa.Backend.Models.Enums;
using Microsoft.Extensions.Options;
using Bixa.Backend.Models.Query;
using Bixa.Backend.Models.Auth;
using System.Security.Claims;
using System.Text;

namespace Bixa.Backend.Base;

public class JwtAuthService(
    IOptions<JwtConfiguration> jwtConfig,
    IHttpContextAccessor httpContextAccessor,
    ILogger<JwtAuthService> logger) : IJwtAuthService
{
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
    private readonly JwtConfiguration _jwtConfig = jwtConfig.Value ?? throw new ArgumentNullException(nameof(jwtConfig), "JWT configuration cannot be null.");
    private readonly ILogger<JwtAuthService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public ClaimsPrincipal? DecodeToken(string? token)
    {
        if (string.IsNullOrEmpty(token))
        {
            _logger.LogWarning("DecodeToken called with null or empty token.");
            return null;
        }

        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_jwtConfig.Llave);
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _jwtConfig.Issuer,
                ValidateAudience = true,
                ValidAudience = _jwtConfig.Audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromMinutes(5)
            };

            ClaimsPrincipal principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken validatedToken);

            return principal;
        }
        catch (SecurityTokenException ex)
        {
            _logger.LogWarning(ex, "Token validation failed for security reasons: {Message}", ex.Message);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred while decoding token: {Message}", ex.Message);
            return null;
        }
    }

    public void GetModelFilter<T>(ref SearchQuery<T> filters) where T : class, new()
    {
        var principal = GetAuthenticatedPrincipal();

        if (principal == null)
        {
            _logger.LogWarning("GetModelFilter called without an authenticated principal. Filters might not be applied as expected.");
            return;
        }

        var (supervisorId, isAdmin) = GetSupervisorClaims(principal);

        filters.Filters ??= new T();

        if (!isAdmin)
            SetSupervisorId(filters.Filters, supervisorId);

        ClaimsPrincipal? GetAuthenticatedPrincipal()
        {
            var token = GetToken();
            return DecodeToken(token);
        }

        (int supervisorId, bool isAdmin) GetSupervisorClaims(ClaimsPrincipal principalLocal)
        {
            var supervisorClaim = principalLocal?.FindFirstValue("SupervisorId");
            _ = int.TryParse(supervisorClaim, out int parsedSupervisorId);

            var isAdminUser = principalLocal?.IsInRole(nameof(UserRolEnum.SuperIntendente)) == true ||
                            principalLocal?.IsInRole(nameof(UserRolEnum.Supervisor)) == true;

            return (parsedSupervisorId, !isAdminUser);
        }

        void SetSupervisorId(T model, int SupervisorId)
        {
            var property = typeof(T).GetProperty("SupervisorId");
            if (property?.PropertyType == typeof(int) || property?.PropertyType == typeof(int?))
            {
                property.SetValue(model, SupervisorId);
            }
        }
    }

    public (UserRolEnum role, int? userId) GetScopedRoleInfo()
    {
        var user = _httpContextAccessor.HttpContext?.User;

        if (user == null)
        {
            _logger.LogError("HttpContext.User is null when trying to get scoped role info.");
            throw new InvalidOperationException("No user principal available in HTTP context.");
        }

        if (user.IsInRole(nameof(UserRolEnum.SuperIntendente)))
            return (UserRolEnum.SuperIntendente, GetUserIdFromClaims(user));

        if (user.IsInRole(nameof(UserRolEnum.Supervisor)))
            return (UserRolEnum.Supervisor, GetUserIdFromClaims(user));

        if (user.IsInRole(nameof(UserRolEnum.Empleado)))
            return (UserRolEnum.Empleado, GetUserIdFromClaims(user));

        _logger.LogWarning("User has no recognized role: {Roles}", string.Join(", ", user.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value)));

        throw new InvalidOperationException("User does not have a recognized role.");
    }

    public string? GetToken()
    {
        var authorizationHeader = _httpContextAccessor.HttpContext?.Request.Headers.Authorization.FirstOrDefault();

        if (authorizationHeader?.StartsWith("Bearer ") == true)
            return authorizationHeader["Bearer ".Length..];

        return null;
    }

    public int? GetUserId()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if (user == null)
        {
            _logger.LogWarning("HttpContext.User is null when trying to get UserId.");
            return null;
        }
        var userIdClaim = user?.FindFirst("id")?.Value;

        return int.TryParse(userIdClaim, out var id) ? id : null;
    }

    private static int? GetUserIdFromClaims(ClaimsPrincipal user)
    {
        var userIdClaim = user.FindFirst("id")?.Value;
        return int.TryParse(userIdClaim, out var id) ? id : null;
    }
}