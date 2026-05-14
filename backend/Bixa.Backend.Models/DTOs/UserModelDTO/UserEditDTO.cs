namespace Bixa.Backend.Models.DTOs.UserModelDTO;

public class UserEditDTO
{
    public bool? Enabled { get; set; }
    public int? IdUserRol { get; set; }
    public string? Password { get; set; }
    public required string Ci { get; set; }
}