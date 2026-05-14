namespace Bixa.Backend.DataAccess.Entities.DbProfit;

public class Vacaciones
{
    public string? CodEmp { get; set; }
    public string? Nombre { get; set; }
    public DateTime? Desde { get; set; }
    public DateTime? Hasta { get; set; }
    public int? Dias { get; set; }
    public int? DisponibleAcumulado { get; set; }
}