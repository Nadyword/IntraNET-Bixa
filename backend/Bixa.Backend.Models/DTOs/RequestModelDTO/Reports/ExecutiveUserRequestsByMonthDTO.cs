namespace Bixa.Backend.Models.DTOs.RequestModelDTO.Reports;

public class ExecutiveUserRequestsByMonthDTO
{
    public int? UserId { get; set; }
    public string? UserName { get; set; }
    public List<MonthlyRequests> MonthlyRequests { get; set; } = new List<MonthlyRequests>();
}