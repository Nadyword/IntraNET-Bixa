using Bixa.Backend.Models.Enums;
using Bixa.Backend.Models.Query;
using System.Security.Claims;

namespace Bixa.Backend.Base;

public interface IJwtAuthService
{
    ClaimsPrincipal? DecodeToken(string? token);

    void GetModelFilter<T>(ref SearchQuery<T> filters) where T : class, new();

    (UserRolEnum role, int? userId) GetScopedRoleInfo();

    string? GetToken();

    int? GetUserId();
}