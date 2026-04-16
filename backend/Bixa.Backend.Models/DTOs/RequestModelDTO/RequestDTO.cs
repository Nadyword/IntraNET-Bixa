using Bixa.Backend.Models.DTOs.UserModelDTO;

namespace Bixa.Backend.Models.DTOs.RequestModelDTO;

public class RequestDTO
{
    public int? AccordID { get; set; }
    public string? AccordName { get; set; }
    public string? AccountingCode { get; set; }
    public string? AccountingDescription { get; set; }
    public DateTime? AccountingReceptionDate { get; set; }
    public string? AnnualBudgetDescription { get; set; }
    public decimal? AuthorizedAmount { get; set; }
    public string? BudgetaryItem { get; set; }
    public bool? BudgetIncreaseAuthorization { get; set; }
    public bool? BudgetType { get; set; }
    public int ChildRequestId { get; set; }
    public string? CostCenterCode { get; set; }
    public DateTime Created { get; set; }
    public int? DeviceID { get; set; }
    public string? DeviceName { get; set; }
    public int EstablishmentID { get; set; }
    public virtual UserDTO? ExecutiveUser { get; set; }
    public int ExecutiveUserID { get; set; }
    public string? ExecutiveUserName { get; set; }
    public string? FixedAssetObservation { get; set; }
    public int Id { get; set; }
    public bool IsSeenByExecutive { get; set; }
    public DateTime Modified { get; set; }
    public int ModifiedById { get; set; }
    public string? Observations { get; set; }
    public DateTime? RealizationDate { get; set; }
    public virtual UserDTO? RequesterUser { get; set; }
    public int? RequesterUserID { get; set; }
    public string? RequesterUserName { get; set; }
    public decimal TotalAmount { get; set; }
    public string? UnitName { get; set; }
}