namespace Bixa.Backend.Models.Auth;

/// <summary>
/// Represents the JWT configuration settings, mapped from appsettings.json.
/// </summary>
public class JwtConfiguration
{
    // Define el nombre de la sección en appsettings.json
    public const string SectionName = "ConfiguracionJwt";

    public required string Audience { get; set; }

    /// <summary>
    /// Gets or sets the token expiration time in minutes.
    /// This should be aligned with the 'expires' property in the generated token.
    /// </summary>
    public int ExpirationMinutes { get; set; } = 60;

    public required string Issuer { get; set; }
    public required string Llave { get; set; }
    // Default a 60 minutos
}