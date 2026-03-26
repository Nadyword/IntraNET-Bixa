using IntranetCorp.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace IntranetCorp.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PermissionsController(RoleManager<IdentityRole> roleManager) : ControllerBase
{
    private readonly RoleManager<IdentityRole> _roleManager = roleManager;

    /// <summary>
    /// Obtiene todos los permisos del sistema, agrupados por categoría.
    /// Los permisos se definen en SystemPermissions (constantes), no en BD.
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    public IActionResult GetAllPermissions()
    {
        var grouped = SystemPermissions.All
            .GroupBy(p => p.Category)
            .Select(g => new
            {
                category = g.Key,
                permissions = g.Select(p => new { name = p.Name, description = p.Description }).ToList()
            })
            .OrderBy(g => g.category)
            .ToList();

        return Ok(grouped);
    }

    /// <summary>
    /// Obtiene todos los roles con sus permisos asignados (desde AspNetRoleClaims).
    /// </summary>
    [HttpGet("roles")]
    [AllowAnonymous]
    public async Task<IActionResult> GetRolesWithPermissions()
    {
        var roles = await _roleManager.Roles.ToListAsync();
        var result = new List<object>();

        foreach (var role in roles.Where(r => r.Name != null))
        {
            var permissions = await _roleManager.GetClaimsAsync(role);
            result.Add(new
            {
                id = role.Id,
                name = role.Name,
                permissions = permissions
                    .Where(c => c.Type == "permission")
                    .Select(c => new { name = c.Value })
                    .ToList()
            });
        }

        return Ok(result);
    }

    /// <summary>
    /// Obtiene los permisos asignados a un rol específico.
    /// </summary>
    [HttpGet("roles/{roleId}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetRolePermissions(string roleId)
    {
        var role = await _roleManager.FindByIdAsync(roleId);
        if (role == null)
            return NotFound(new { message = "Rol no encontrado" });

        var claims = await _roleManager.GetClaimsAsync(role);
        var permissions = claims
            .Where(c => c.Type == "permission")
            .Select(c => c.Value)
            .ToList();

        return Ok(new
        {
            roleId,
            roleName = role.Name,
            permissions = permissions
        });
    }

    /// <summary>
    /// Actualiza los permisos asignados a un rol.
    /// Solo SuperAdmin puede hacer esto.
    /// </summary>
    [HttpPut("roles/{roleId}")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> UpdateRolePermissions(string roleId, [FromBody] UpdateRolePermissionsRequest request)
    {
        var role = await _roleManager.FindByIdAsync(roleId);
        if (role == null)
            return NotFound(new { message = "Rol no encontrado" });

        // Obtener permisos actuales
        var currentClaims = await _roleManager.GetClaimsAsync(role);
        var currentPermissions = currentClaims
            .Where(c => c.Type == "permission")
            .Select(c => c.Value)
            .ToList();

        // Eliminar permisos que no estén en la nueva lista
        var currentClaimsList = currentClaims.ToList();
        foreach (var permToRemove in currentPermissions.Except(request.PermissionNames))
        {
            var claim = currentClaimsList.First(c => c.Type == "permission" && c.Value == permToRemove);
            await _roleManager.RemoveClaimAsync(role, claim);
        }

        // Agregar nuevos permisos
        foreach (var permToAdd in request.PermissionNames.Except(currentPermissions))
        {
            await _roleManager.AddClaimAsync(role, new Claim("permission", permToAdd));
        }

        return Ok(new { message = $"Permisos del rol '{role.Name}' actualizados correctamente" });
    }
}

public class UpdateRolePermissionsRequest
{
    public List<string> PermissionNames { get; set; } = [];
}
