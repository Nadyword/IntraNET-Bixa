using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Bixa.Backend.Models.DTOs.ExpensePlanMetadataDTO;

public class UploadExpensePlanExcelRequest
{
    [Required]
    public IFormFile File { get; set; } = default!;
}