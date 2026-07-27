namespace Bixa.Backend.Models.DTOs.NotificationModelDTO;

public class NotificationSummaryDTO
{
    public int UnreadCount { get; set; }
    public int PendingApprovals { get; set; }
    public int PendingAdminApproval { get; set; }
    public int PendingArchive { get; set; }
    public List<NotificationDTO> Recent { get; set; } = [];
}
