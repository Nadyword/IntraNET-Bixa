namespace Bixa.Backend.Models.Auth;

public class AuthTokenClaims
{
    public required string Ci { get; set; }
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string RolId { get; set; }
    public required string RolName { get; set; }
}