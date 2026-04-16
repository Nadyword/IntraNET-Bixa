namespace Bixa.Backend.Models.DTOs.RequestModelDTO.Reports;

public class DeviceRequestsByMonthDTO
{
    public int DeviceId { get; set; }
    public string? DeviceName { get; set; }
    public List<MonthlyRequests> MonthlyRequests { get; set; } = new List<MonthlyRequests>();
}


public class AccordRequestsByMonthDTO
{
    public int AccordId { get; set; }
    public string? AccordName { get; set; }
    public List<MonthlyRequests> MonthlyRequests { get; set; } = new List<MonthlyRequests>();
}