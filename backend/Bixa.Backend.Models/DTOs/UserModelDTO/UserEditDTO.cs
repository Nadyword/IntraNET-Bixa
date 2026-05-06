namespace Bixa.Backend.Models.DTOs.UserModelDTO;

public class UserEditDTO
{
    public string? Email { get; set; }
    public bool? Enabled { get; set; }
    public int Id { get; set; }
    public int? IdUserRol { get; set; }
    public string? Name { get; set; }
    public string? Phone { get; set; }
    public string? Ci { get; set; }
}