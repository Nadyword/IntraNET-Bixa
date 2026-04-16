namespace Bixa.Backend.Models.DTOs.UserRolDTO;

public class UserRolDTO
{
    /// <summary>
    /// Unique identifier for the user.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// User's full name.
    /// </summary>
    public string? Name { get; set; } = string.Empty;
}