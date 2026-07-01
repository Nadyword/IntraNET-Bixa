namespace Bixa.Backend.Models.DTOs.UserModelDTO;

public class UserDTO
{
    public int Id { get; set; }
    public string PasswordHash { get; set; } = string.Empty;
    public string Ci { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime? LastLogin { get; set; }
    public string UrlFirma { get; set; } = string.Empty;
    public int IdUserRol { get; set; }
    public bool IsActive { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string? ModifiedByCi { get; set; }
}