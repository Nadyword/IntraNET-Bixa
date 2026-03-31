using Microsoft.AspNetCore.Authorization;
using IntranetCorp.Infrastructure.Services;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Identity;
using IntranetCorp.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace IntranetCorp.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    IConfiguration configuration,
    IEmailService emailService,
    RoleManager<IdentityRole> roleManager) : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly SignInManager<ApplicationUser> _signInManager = signInManager;
    private readonly IConfiguration _configuration = configuration;
    private readonly IEmailService _emailService = emailService;
    private readonly RoleManager<IdentityRole> _roleManager = roleManager;


    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email!);
        if (user == null || user.IsDeleted)
            return Unauthorized(new { message = "Credenciales inválidas" });

        // Validar que el perfil esté completo
        if (!user.IsProfileComplete)
            return Unauthorized(new { message = "Debes completar tu registro antes de iniciar sesión. Revisa tu correo." });

        var result = await _signInManager.PasswordSignInAsync(user, request.Password!, false, false);
        if (!result.Succeeded)
            return Unauthorized(new { message = "Credenciales inválidas" });

        var token = GenerateJwtToken(user);
        return Ok(new { token, user = new { user.Id, user.Email, user.Nombre, user.Cargo } });
    }

    [HttpPost("create-employee")]
    [Authorize(Roles = "SuperAdmin,Líder")]
    public async Task<IActionResult> CreateEmployee([FromBody] CreateEmployeeRequest request)
    {
        // Validar que el email no exista
        var existingUser = await _userManager.FindByEmailAsync(request.Email!);
        if (existingUser != null)
            return BadRequest(new { message = "El email ya está registrado" });

        // Crear usuario sin contraseña (usar un GUID temporal)
        var tempPassword = Guid.NewGuid().ToString("N");
        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            Nombre = request.Nombre,
            Cedula = request.Cedula,
            Cargo = request.Cargo,
            FechaIngreso = DateTime.UtcNow,
            IsProfileComplete = false,
            OnboardingToken = Guid.NewGuid().ToString("N"),
            OnboardingTokenExpiry = DateTime.UtcNow.AddHours(8),
            EmailConfirmed = false
        };

        var result = await _userManager.CreateAsync(user, tempPassword);
        if (!result.Succeeded)
            return BadRequest(new { errors = result.Errors.Select(e => e.Description) });

        // Asignar rol
        await _userManager.AddToRoleAsync(user, "Empleado");

        // Enviar email con link de activación
        var activationLink = $"http://localhost:5173/activate?token={user.OnboardingToken}";
        var emailBody = $@"
            <h2>¡Bienvenido a BIXA!</h2>
            <p>Hola {user.Nombre},</p>
            <p>Tu cuenta ha sido creada. Para completar tu registro y establecer tu contraseña, haz clic en el siguiente enlace:</p>
            <a href='{activationLink}' style='background-color: #D91A1A; color: white; padding: 10px 20px; text-decoration: none; border-radius: 4px;'>
                Completar Registro
            </a>
            <p>Este enlace es válido por 8 horas.</p>
            <p>Si no creaste esta cuenta, ignora este correo.</p>
        ";

        await _emailService.SendEmailAsync(user.Email!, "Completa tu registro en BIXA", emailBody);

        return CreatedAtAction(nameof(Login), new { message = "Empleado creado. Se ha enviado un email con las instrucciones de activación." });
    }

    [HttpGet("validate-token")]
    [AllowAnonymous]
    public async Task<IActionResult> ValidateToken([FromQuery] string? token)
    {
        if (string.IsNullOrEmpty(token))
            return BadRequest(new { valid = false, message = "Token no proporcionado" });

        var user = await _userManager.Users.Where(u => u.OnboardingToken == token).FirstOrDefaultAsync();
        if (user == null)
            return Ok(new { valid = false, message = "Token inválido" });

        if (user.OnboardingTokenExpiry == null || user.OnboardingTokenExpiry < DateTime.UtcNow)
            return Ok(new { valid = false, message = "El token ha expirado. Contacta a tu administrador para obtener un nuevo enlace." });

        return Ok(new
        {
            valid = true,
            email = user.Email,
            nombre = user.Nombre,
            cedula = user.Cedula,
            cargo = user.Cargo
        });
    }

    [HttpPost("activate-account")]
    [AllowAnonymous]
    public async Task<IActionResult> ActivateAccount([FromBody] ActivateAccountRequest request)
    {
        if (string.IsNullOrEmpty(request.Token))
            return BadRequest(new { message = "Token no proporcionado" });

        if (request.Password != request.ConfirmPassword)
            return BadRequest(new { message = "Las contraseñas no coinciden" });

        var user = await _userManager.Users.Where(u => u.OnboardingToken == request.Token).FirstOrDefaultAsync();
        if (user == null)
            return BadRequest(new { message = "Token inválido" });

        if (user.OnboardingTokenExpiry == null || user.OnboardingTokenExpiry < DateTime.UtcNow)
            return BadRequest(new { message = "El token ha expirado" });

        // Establecer contraseña definitiva (el usuario aún no tiene contraseña)
        var addPasswordResult = await _userManager.AddPasswordAsync(user, request.Password!);
        if (!addPasswordResult.Succeeded)
            return BadRequest(new { errors = addPasswordResult.Errors.Select(e => e.Description) });

        // Actualizar datos personales
        user.Telefono = request.Telefono;
        user.FechaNacimiento = request.FechaNacimiento;
        user.Cedula = request.Cedula;
        user.Cargo = request.Cargo;
        user.IsProfileComplete = true;
        user.OnboardingToken = null;
        user.OnboardingTokenExpiry = null;
        user.EmailConfirmed = true;

        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
            return BadRequest(new { errors = updateResult.Errors.Select(e => e.Description) });

        // Generar token JWT para auto-login
        var token = GenerateJwtToken(user);
        return Ok(new { token, user = new { user.Id, user.Email, user.Nombre, user.Cargo } });
    }

    [HttpPost("forgot-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        if (string.IsNullOrEmpty(request.Email))
            return BadRequest(new { message = "Correo no proporcionado" });

        var user = await _userManager.FindByEmailAsync(request.Email);

        // Siempre devolver OK para no revelar si el correo existe
        if (user == null || user.IsDeleted || !user.IsProfileComplete)
            return Ok(new { message = "Si el correo existe, recibirás las instrucciones." });

        // Generar token de recuperación (válido 1 hora)
        user.PasswordResetToken = Guid.NewGuid().ToString("N");
        user.PasswordResetTokenExpiry = DateTime.UtcNow.AddHours(1);
        await _userManager.UpdateAsync(user);

        var resetLink = $"http://localhost:5173/reset-password?token={user.PasswordResetToken}";
        var emailBody = $@"
            <h2>Recuperación de contraseña - BIXA</h2>
            <p>Hola {user.Nombre},</p>
            <p>Recibimos una solicitud para restablecer la contraseña de tu cuenta.</p>
            <p>Haz clic en el siguiente enlace para crear una nueva contraseña:</p>
            <a href='{resetLink}' style='background-color: #D91A1A; color: white; padding: 10px 20px; text-decoration: none; border-radius: 4px; display: inline-block; margin: 16px 0;'>
                Restablecer Contraseña
            </a>
            <p>Este enlace es válido por 1 hora.</p>
            <p>Si no solicitaste este cambio, ignora este correo. Tu contraseña no será modificada.</p>
        ";

        await _emailService.SendEmailAsync(user.Email!, "Restablece tu contraseña en BIXA", emailBody);

        return Ok(new { message = "Si el correo existe, recibirás las instrucciones." });
    }

    [HttpGet("validate-reset-token")]
    [AllowAnonymous]
    public async Task<IActionResult> ValidateResetToken([FromQuery] string? token)
    {
        if (string.IsNullOrEmpty(token))
            return BadRequest(new { valid = false, message = "Token no proporcionado" });

        var user = await _userManager.Users.Where(u => u.PasswordResetToken == token).FirstOrDefaultAsync();
        if (user == null)
            return Ok(new { valid = false, message = "Token inválido" });

        if (user.PasswordResetTokenExpiry == null || user.PasswordResetTokenExpiry < DateTime.UtcNow)
            return Ok(new { valid = false, message = "El enlace ha expirado. Solicita uno nuevo." });

        return Ok(new { valid = true, email = user.Email, nombre = user.Nombre });
    }

    [HttpPost("reset-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        if (string.IsNullOrEmpty(request.Token))
            return BadRequest(new { message = "Token no proporcionado" });

        if (request.Password != request.ConfirmPassword)
            return BadRequest(new { message = "Las contraseñas no coinciden" });

        var user = await _userManager.Users.Where(u => u.PasswordResetToken == request.Token).FirstOrDefaultAsync();
        if (user == null)
            return BadRequest(new { message = "Token inválido" });

        if (user.PasswordResetTokenExpiry == null || user.PasswordResetTokenExpiry < DateTime.UtcNow)
            return BadRequest(new { message = "El enlace ha expirado. Solicita uno nuevo." });

        // Generar token de Identity para resetear contraseña
        var identityResetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
        var resetResult = await _userManager.ResetPasswordAsync(user, identityResetToken, request.Password!);
        if (!resetResult.Succeeded)
            return BadRequest(new { errors = resetResult.Errors.Select(e => e.Description) });

        // Limpiar token de recuperación
        user.PasswordResetToken = null;
        user.PasswordResetTokenExpiry = null;
        await _userManager.UpdateAsync(user);

        return Ok(new { message = "Contraseña actualizada correctamente. Ya puedes iniciar sesión." });
    }

    private async Task<string> GenerateJwtToken(ApplicationUser user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? ""));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var roles = await _userManager.GetRolesAsync(user);
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Email, user.Email ?? ""),
            new(ClaimTypes.Name, user.Nombre ?? ""),
        };

        // Agregar rol(es)
        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        // Agregar permisos del usuario según sus roles (desde AspNetRoleClaims)
        foreach (var roleName in roles)
        {
            var role = await _roleManager.FindByNameAsync(roleName);
            if (role != null)
            {
                var roleClaims = await _roleManager.GetClaimsAsync(role);
                foreach (var roleClaim in roleClaims.Where(c => c.Type == "permission"))
                {
                    claims.Add(roleClaim);
                }
            }
        }

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: null,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(15),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

public class LoginRequest
{
    public string? Email { get; set; }
    public string? Password { get; set; }
}

public class CreateEmployeeRequest
{
    public string? Email { get; set; }
    public string? Nombre { get; set; }
    public string? Cedula { get; set; }
    public string? Cargo { get; set; }
}

public class ValidateTokenRequest
{
    public string? Token { get; set; }
}

public class ActivateAccountRequest
{
    public string? Token { get; set; }
    public string? Password { get; set; }
    public string? ConfirmPassword { get; set; }
    public string? Telefono { get; set; }
    public DateTime? FechaNacimiento { get; set; }
    public string? Cedula { get; set; }
    public string? Cargo { get; set; }
}

public class ForgotPasswordRequest
{
    public string? Email { get; set; }
}

public class ResetPasswordRequest
{
    public string? Token { get; set; }
    public string? Password { get; set; }
    public string? ConfirmPassword { get; set; }
}
