namespace Bixa.Backend.Models.DTOs.UserModelDTO;

public class UserChangePasswordDTO
{
    /// <summary>
    /// Unique identifier for the user.
    /// </summary>
    public required string Ci { get; set; }

    /// <summary>
    /// User's password.
    /// </summary>
    public string? Password { get; set; }
}