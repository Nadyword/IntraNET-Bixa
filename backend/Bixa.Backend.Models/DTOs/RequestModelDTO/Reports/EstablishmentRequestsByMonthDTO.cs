namespace Bixa.Backend.Models.DTOs.RequestModelDTO.Reports;

public class EstablishmentRequestsByMonthDTO
{
    public int EstablishmentId { get; set; }
    public string? EstablishmentName { get; set; }
    public List<MonthlyRequests> MonthlyRequests { get; set; } = new List<MonthlyRequests>();
}

public class EstablishmentDeviceRequestsByMonthDTO
{
    public int EstablishmentId { get; set; }
    public string? EstablishmentName { get; set; }
    public IEnumerable<DeviceRequestsByMonthDTO>? DeviceRequestsByMonthDTOs { get; set; }
}


public class EstablishmentAccordRequestsByMonthDTO
{
    public int EstablishmentId { get; set; }
    public string? EstablishmentName { get; set; }
    public IEnumerable<AccordRequestsByMonthDTO>? AccordRequestsByMonthDTOs { get; set; }
}