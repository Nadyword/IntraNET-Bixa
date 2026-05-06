using Microsoft.Extensions.Configuration;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using Bixa.Backend.Models.Auth;
using System.Security.Claims;
using System.Text;

namespace Bixa.Backend.Services.Services.JwtControllers;

public class ManejoJwt(IConfiguration _configuration) : IManejoJwt
{
    public IConfiguration configuration = _configuration;

    public string GenerarToken(AuthTokenClaims authToken)
    {
        var claims = new List<Claim>
            {
                new(ClaimTypes.Name, authToken.Name),
                new("ci", authToken.Ci),
                new("id", Convert.ToString(authToken.Id)),
                new(ClaimTypes.Role, authToken.RolName),
                new("RolId", authToken.RolId)
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