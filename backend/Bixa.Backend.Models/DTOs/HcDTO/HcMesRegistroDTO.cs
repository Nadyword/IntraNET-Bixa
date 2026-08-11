namespace Bixa.Backend.Models.DTOs.HcDTO;

public class HcMesRegistroDTO
{
    public string Ci { get; set; } = null!;
    public string? NombreCompleto { get; set; }
    public decimal Mes1 { get; set; }
    public decimal Mes2 { get; set; }
    public decimal Mes3 { get; set; }
    public decimal PrimaTrimBs { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string? ModifiedByCi { get; set; }
}
