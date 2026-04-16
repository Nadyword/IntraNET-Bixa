using Bixa.Backend.Configuration;
using Bixa.Backend.Models.Enums;
using Microsoft.AspNetCore.Authorization;

namespace Bixa.Backend.Controllers.Services;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class AuthorizeByRoleAttribute : AuthorizeAttribute
{
    public AuthorizeByRoleAttribute(UserRolEnum role)
    {
        Policy = role.GetDescriptionPolicy();
    }
}