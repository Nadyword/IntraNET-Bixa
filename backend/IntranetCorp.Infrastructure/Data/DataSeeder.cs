using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Identity;
using IntranetCorp.Domain.Entities;
using IntranetCorp.Domain.Constants;
using System.Security.Claims;

namespace IntranetCorp.Infrastructure.Data;

/// <summary>
/// Seeder idempotente para crear roles iniciales, permisos y usuario administrador maestro.
/// Los permisos se almacenan como claims en AspNetRoleClaims (estándar de .NET Identity).
/// </summary>
public static class DataSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

        // 1. Crear roles iniciales si no existen
        string[] roles = ["SuperAdmin", "Líder", "Empleado"];
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        // 2. Asignar permisos a roles usando AspNetRoleClaims (estándar .NET)
        var superAdminRole = await roleManager.FindByNameAsync("SuperAdmin");
        var liderRole = await roleManager.FindByNameAsync("Líder");
        var empleadoRole = await roleManager.FindByNameAsync("Empleado");

        // SuperAdmin: todos los permisos
        if (superAdminRole != null)
        {
            var existingClaims = await roleManager.GetClaimsAsync(superAdminRole);
            foreach (var perm in SystemPermissions.All)
            {
                if (!existingClaims.Any(c => c.Type == "permission" && c.Value == perm.Name))
                {
                    await roleManager.AddClaimAsync(superAdminRole,
                        new Claim("permission", perm.Name));
                }
            }
        }

        // Líder: tramites (leer/crear/actualizar), usuarios (ver/crear), anuncios (crear/borrar)
        if (liderRole != null)
        {
            var liderPermissions = new[] {
                "tramites.leer", "tramites.crear", "tramites.actualizar",
                "usuarios.ver", "usuarios.crear",
                "anuncios.crear", "anuncios.borrar"
            };
            var existingClaims = await roleManager.GetClaimsAsync(liderRole);

            foreach (var permName in liderPermissions)
            {
                if (!existingClaims.Any(c => c.Type == "permission" && c.Value == permName))
                {
                    await roleManager.AddClaimAsync(liderRole,
                        new Claim("permission", permName));
                }
            }
        }

        // Empleado: tramites (leer/crear)
        if (empleadoRole != null)
        {
            var empleadoPermissions = new[] { "tramites.leer", "tramites.crear" };
            var existingClaims = await roleManager.GetClaimsAsync(empleadoRole);

            foreach (var permName in empleadoPermissions)
            {
                if (!existingClaims.Any(c => c.Type == "permission" && c.Value == permName))
                {
                    await roleManager.AddClaimAsync(empleadoRole,
                        new Claim("permission", permName));
                }
            }
        }

        // 3. Crear usuario maestro (superadmin)
        const string adminEmail = "samuelsc1509@gmail.com";
        const string adminPassword = "Sa753951.";

        var existingAdmin = await userManager.FindByEmailAsync(adminEmail);
        if (existingAdmin == null)
        {
            var admin = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                Nombre = "Samuel Admin",
                EmailConfirmed = true,
                IsProfileComplete = true,
                FechaIngreso = DateTime.UtcNow,
            };

            var result = await userManager.CreateAsync(admin, adminPassword);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(admin, "SuperAdmin");
            }
        }
    }
}
