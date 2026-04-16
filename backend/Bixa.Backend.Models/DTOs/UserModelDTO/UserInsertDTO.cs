namespace Bixa.Backend.Models.DTOs.UserModelDTO;

public class UserInsertDTO
{
    public required string Email { get; set; }
    public int IdUserRol { get; set; }
    public required string Name { get; set; }
    public required string Password { get; set; }
    public required string Phone { get; set; }
    public required string TaxId { get; set; }
}