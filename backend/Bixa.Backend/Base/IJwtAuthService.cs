using Bixa.Backend.Models.Enums;
using System.Security.Claims;

namespace Bixa.Backend.Base;

public interface IJwtAuthService
{
    ClaimsPrincipal? DecodeToken(string? token);

    (UserRolEnum role, int? userId) GetScopedRoleInfo();

    string? GetToken();

    int? GetUserId();
}