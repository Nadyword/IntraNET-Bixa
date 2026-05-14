namespace Bixa.Backend.Models.DTOs.UserModelDTO;

public class UserIdDTO
{
    public DateTime Created { get; set; }
    public string? Email { get; set; }
    public bool Enabled { get; set; }
    public int Id { get; set; }
    public int IdUserRol { get; set; }
    public DateTime? LastLogin { get; set; }
    public DateTime Modified { get; set; }
    public int ModifiedByCi { get; set; }
    public string? Name { get; set; }
    public string? Phone { get; set; }
    public string? Ci { get; set; }
    public virtual UserRolDTO.UserRolDTO? UserRol { get; set; }
}