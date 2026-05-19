namespace Bixa.Backend.Models.DTOs.SoporteChatModelDTO;

public class SoporteChatRDTO
{
    public required string UserCi { get; set; }
    public required string Message { get; set; }
    public string? RespondidoPorCi { get; set; }
}