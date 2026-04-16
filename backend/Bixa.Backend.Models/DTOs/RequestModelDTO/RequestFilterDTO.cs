using Microsoft.AspNetCore.Mvc;

namespace Bixa.Backend.Models.DTOs.RequestModelDTO;

public class RequestFilterDTO
{
    [FromQuery] public int? AccordID { get; set; }
    [FromQuery] public string? AccountingCode { get; set; }
    [FromQuery] public string? AccountingDescription { get; set; }
    [FromQuery] public DateTime? AccountingReceptionDate { get; set; }
    [FromQuery] public decimal? AuthorizedAmount { get; set; }
    [FromQuery] public bool? BudgetIncreaseAuthorization { get; set; }
    [FromQuery] public string? CostCenterCode { get; set; }
    [FromQuery] public int? DeviceID { get; set; }
    [FromQuery] public int? EstablishmentID { get; set; }
    [FromQuery] public int? ExecutiveUserID { get; set; }
    [FromQuery] public string? FixedAssetObservation { get; set; }
    [FromQuery] public int? Id { get; set; }
    [FromQuery] public DateTime? RealizationDate { get; set; }
    [FromQuery] public int? RequesterUserID { get; set; }
    [FromQuery] public string? UnitName { get; set; }
}