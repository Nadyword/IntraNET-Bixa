using Microsoft.AspNetCore.Http;

namespace Bixa.Backend.Models.DTOs.UserModelDTO;

public class UserEditDTO
{
    public bool? Enabled { get; set; }
    public int? IdUserRol { get; set; }
    public string? Password { get; set; }
    public required string Ci { get; set; }

    /// <summary>
    /// Foto de firma opcional (PNG 225x225) para reemplazar la actual.
    /// </summary>
    public IFormFile? Firma { get; set; }
}