namespace Bixa.Backend.Models.DTOs.UserModelDTO;

public class UserInsertDTO
{
    public int IdUserRol { get; set; }
    public required string? FirstName { get; set; }
    public required string? LastName { get; set; }
    public required string Ci { get; set; }
}