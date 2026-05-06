namespace Bixa.Backend.Models.DTOs.UserModelDTO;

public class UserDTO
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Ci { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime? LastLogin { get; set; }
    public int IdUserRol { get; set; }
    public bool IsActive { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenDate { get; set; }
    public DateTime Created { get; set; }
    public DateTime Modified { get; set; }
    public int? ModifiedById { get; set; }
    public virtual UserRolDTO.UserRolDTO? UserRol { get; set; }
}