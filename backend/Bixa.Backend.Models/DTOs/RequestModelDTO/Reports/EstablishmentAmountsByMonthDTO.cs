namespace Bixa.Backend.Models.DTOs.RequestModelDTO.Reports;

public class EstablishmentAmountsByMonthDTO
{
    public int EstablishmentId { get; set; }
    public string? EstablishmentName { get; set; }
    public List<MonthlyAmounts> MonthlyAmounts { get; set; } = new List<MonthlyAmounts>();
}