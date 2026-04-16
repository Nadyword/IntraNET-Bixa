namespace Bixa.Backend.Models.Auth;

/// <summary>
/// Represents the response object for successful user login.
/// </summary>
public class LoginDTO
{
    /// <summary>
    /// The refresh token for obtaining new access tokens.
    /// </summary>
    public required string RefreshToken { get; set; }

    /// <summary>
    /// The access token for authenticated user.
    /// </summary>
    public required string Token { get; set; }
}