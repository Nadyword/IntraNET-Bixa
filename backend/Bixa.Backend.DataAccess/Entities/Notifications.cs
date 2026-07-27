using Bixa.Backend.DataAccess.Models;

namespace Bixa.Backend.DataAccess.Entities;

public class Notifications : BaseEntities
{
    public required string UserCi { get; set; }
    public required string? NotificationType { get; set; }
    public required string? Title { get; set; }
    public required string? Message { get; set; }
    public bool IsRead { get; set; }
    public DateTime? ReadAt { get; set; }
    public string? ReferenceType { get; set; }
    public int? ReferenceId { get; set; }
    public virtual Users? User { get; set; }
}