using Microsoft.AspNetCore.Mvc;

namespace Bixa.Backend.Models.DTOs.KeyValuePairModelDTO;

public class KeyFieldConfigurationDTO
{
    [FromQuery(Name = "keyField")]
    public required string KeyField { get; set; }

    [FromQuery(Name = "valueField")]
    public required string ValueField { get; set; }
}