namespace Bixa.Backend.Models.DTOs.RequestModelDTO.Reports;


public class MonthlyRequests
{
    public int Month { get; set; }
    public string? MonthName { get; set; }
    public int TotalRequests { get; set; }
    public IEnumerable<RequestDTO> Requests { get; set; } = new List<RequestDTO>();
}