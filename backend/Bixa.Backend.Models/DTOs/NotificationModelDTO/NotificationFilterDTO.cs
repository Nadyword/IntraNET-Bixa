using Microsoft.AspNetCore.Mvc;

namespace Bixa.Backend.Models.DTOs.NotificationModelDTO;

public class NotificationFilterDTO
{
    [FromQuery] public int? Id { get; set; }
    [FromQuery] public int? UserId { get; set; }
    [FromQuery] public string? NotificationType { get; set; }
    [FromQuery] public string? Priority { get; set; }
    [FromQuery] public string? Title { get; set; }
    [FromQuery] public string? Message { get; set; }
    [FromQuery] public bool? IsRead { get; set; }
    [FromQuery] public string? ReferenceType { get; set; }
    [FromQuery] public int? ReferenceId { get; set; }
    [FromQuery] public DateTime? ReadAtFrom { get; set; }
    [FromQuery] public DateTime? ReadAtTo { get; set; }
    [FromQuery] public DateTime? CreatedFrom { get; set; }
    [FromQuery] public DateTime? CreatedTo { get; set; }
}