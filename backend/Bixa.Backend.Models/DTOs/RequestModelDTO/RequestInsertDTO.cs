namespace Bixa.Backend.Models.DTOs.RequestModelDTO;

public class RequestInsertDTO
{
    public int? AccordID { get; set; }
    public string? AccountingCode { get; set; }
    public string? AccountingDescription { get; set; }
    public DateTime? AccountingReceptionDate { get; set; }
    public decimal? AuthorizedAmount { get; set; }
    public bool? BudgetIncreaseAuthorization { get; set; }
    public string? CostCenterCode { get; set; }
    public int? DeviceID { get; set; }
    public int? EstablishmentID { get; set; }
    public int? ExecutiveUserID { get; set; }
    public string? FixedAssetObservation { get; set; }
    public bool IsSeenByExecutive { get; set; }
    public string? Observations { get; set; }
    public DateTime? RealizationDate { get; set; }
    public int? RequesterUserID { get; set; }
    public decimal TotalAmount { get; set; }
    public string? UnitName { get; set; }
}