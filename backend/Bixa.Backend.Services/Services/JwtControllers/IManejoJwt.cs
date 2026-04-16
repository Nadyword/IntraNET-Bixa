using Bixa.Backend.Models.Auth;

namespace Bixa.Backend.Services.Services.JwtControllers;

public interface IManejoJwt
{
    public string GenerarToken(AuthTokenClaims authToken);
}