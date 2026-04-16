using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Bixa.Backend.Models.Auth;

namespace Bixa.Backend.Services.Services.JwtControllers;

public class ManejoJwt : IManejoJwt
{
    public IConfiguration configuration;

    public ManejoJwt(IConfiguration _configuration)
    {
        configuration = _configuration;
    }

    public string GenerarToken(AuthTokenClaims authToken)
    {
        var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, authToken.Name),
                new Claim(ClaimTypes.Email, authToken.Email),
                new Claim("id", Convert.ToString(authToken.Id)),
                new Claim(ClaimTypes.Role, authToken.RolName),
                new Claim("RolId", authToken.RolId)
            };

        var llave = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration.GetSection("ConfiguracionJwt:Llave").Get<string>() ?? string.Empty));
        var credentials = new SigningCredentials(llave, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: configuration.GetSection("ConfiguracionJwt:Issuer").Get<string>() ?? string.Empty,
            audience: configuration.GetSection("ConfiguracionJwt:Audience").Get<string>() ?? string.Empty,
            claims: claims,
            expires: DateTime.Now.AddMinutes(60),
            signingCredentials: credentials
            );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}