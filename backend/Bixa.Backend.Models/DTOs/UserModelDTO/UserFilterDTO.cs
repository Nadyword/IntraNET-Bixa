using Microsoft.AspNetCore.Mvc;

namespace Bixa.Backend.Models.DTOs.UserModelDTO;

public class UserFilterDTO
{
    [FromQuery] public DateTime? Created { get; set; }
    [FromQuery] public string? Email { get; set; }
    [FromQuery] public bool? Enabled { get; set; }
    [FromQuery] public int? Id { get; set; }
    [FromQuery] public int? IdUserRol { get; set; }
    [FromQuery] public DateTime? LastLogin { get; set; }
    [FromQuery] public DateTime? Modified { get; set; }
    [FromQuery] public int? ModifiedById { get; set; }
    [FromQuery] public string? Name { get; set; }
    [FromQuery] public string? Phone { get; set; }
    [FromQuery] public string? TaxId { get; set; }
    [FromQuery] public int? UserType { get; set; }
}