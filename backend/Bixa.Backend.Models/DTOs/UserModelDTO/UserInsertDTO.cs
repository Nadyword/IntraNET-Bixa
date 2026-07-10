using Microsoft.AspNetCore.Http;

namespace Bixa.Backend.Models.DTOs.UserModelDTO;

public class UserInsertDTO
{
    public int IdUserRol { get; set; }
    public required string? FirstName { get; set; }
    public required string? LastName { get; set; }
    public required string? PasswordHash { get; set; }
    public required string Ci { get; set; }

    /// <summary>
    /// Foto de firma opcional (PNG con proporción 2:1). Si no se envía, se usa SinFirma.png.
    /// </summary>
    public IFormFile? Firma { get; set; }
}