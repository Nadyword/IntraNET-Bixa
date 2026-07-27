namespace Bixa.Backend.Models.DTOs.NotificationModelDTO;

public class NotificationInsertDTO
{
    public required string UserCi { get; set; }
    public required string NotificationType { get; set; }
    public required string Title { get; set; }
    public required string Message { get; set; }
    public string? ReferenceType { get; set; }
    public int? ReferenceId { get; set; }
}
