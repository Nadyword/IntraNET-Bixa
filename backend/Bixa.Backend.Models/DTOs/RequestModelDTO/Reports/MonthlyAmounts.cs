namespace Bixa.Backend.Models.DTOs.RequestModelDTO.Reports;

public class MonthlyAmounts
{
    public int Month { get; set; }
    public string? MonthName { get; set; }
    public decimal TotalAmount { get; set; }
}