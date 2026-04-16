namespace Bixa.Backend.Models.DTOs.NotificationModelDTO;

public class NotificationDTO
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public required string NotificationType { get; set; }
    public required string Priority { get; set; }
    public required string Title { get; set; }
    public required string Message { get; set; }
    public bool IsRead { get; set; }
    public string? ReferenceType { get; set; }
    public int? ReferenceId { get; set; }
    public DateTime? ReadAt { get; set; }
}