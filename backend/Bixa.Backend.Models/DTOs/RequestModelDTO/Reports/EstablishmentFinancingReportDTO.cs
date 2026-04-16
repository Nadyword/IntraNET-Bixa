// DTO base para la métrica mensual (reemplaza MonthlyRequests/MonthlyAmounts)
public class MonthlyFinancingMetrics
{
    public int Month { get; set; }
    public string? MonthName { get; set; }

    // Métricas por Presupuesto (BudgetType = false/null)
    public decimal BudgetAmount { get; set; }
    public int BudgetCount { get; set; }

    // Métricas por Convenio (BudgetType = true)
    public decimal AgreementAmount { get; set; }
    public int AgreementCount { get; set; }

    // Totales (para facilitar la visualización)
    public decimal TotalAmount => BudgetAmount + AgreementAmount;
    public int TotalCount => BudgetCount + AgreementCount;
}

// DTO principal para el reporte (agrupado por Establecimiento)
public class EstablishmentFinancingReportDTO
{
    public int EstablishmentId { get; set; }
    public string? EstablishmentName { get; set; }
    public List<MonthlyFinancingMetrics> MonthlyMetrics { get; set; } = new List<MonthlyFinancingMetrics>();
}