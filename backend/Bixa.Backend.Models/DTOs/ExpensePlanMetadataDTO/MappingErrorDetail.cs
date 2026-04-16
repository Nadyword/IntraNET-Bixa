namespace Bixa.Backend.Models.DTOs.ExpensePlanMetadataDTO;

public class MappingErrorDetail
{
    public string AttemptedValue { get; set; } = string.Empty;
    public string CellAddress { get; set; } = string.Empty;
    public string ErrorMessage { get; set; } = string.Empty;
    public string ExpectedType { get; set; } = string.Empty;
    public string PropertyName { get; set; } = string.Empty;
    public string RowIdentifier { get; set; } = string.Empty;
    public string SheetName { get; set; } = string.Empty;
}