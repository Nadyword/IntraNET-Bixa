namespace Bixa.Backend.Models.DTOs.RequestModelDTO.Reports;

public class DeviceReportFilter
{
    public int? EndMonth { get; set; } = 12;
    public int? EstablishmentID { get; set; }
    public int? StartMonth { get; set; } = 1;
    public int? Year { get; set; } = DateTime.Now.Year;
}