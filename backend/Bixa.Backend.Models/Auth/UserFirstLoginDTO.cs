namespace Bixa.Backend.Models.Auth
{
    public class UserFirstLoginDTO
    {
        public string TaxId { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
        public required string Token { get; set; }
    }
}