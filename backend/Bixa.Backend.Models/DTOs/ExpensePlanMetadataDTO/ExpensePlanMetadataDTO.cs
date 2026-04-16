namespace Bixa.Backend.Models.DTOs.ExpensePlanMetadataDTO;

public class FurnitureMachineryEquipmentDTO
{
    public decimal Apr { get; set; }
    public decimal Aug { get; set; }
    public string BudgetaryItem { get; set; } = string.Empty;
    public decimal Dec { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Feb { get; set; }
    public decimal Jan { get; set; }
    public decimal Jul { get; set; }
    public decimal Jun { get; set; }
    public decimal Mar { get; set; }
    public decimal May { get; set; }
    public decimal Nov { get; set; }
    public decimal Oct { get; set; }
    public decimal Sep { get; set; }
}

public class GoodForUseOrServiceDTO
{
    public decimal Apr { get; set; }
    public decimal Aug { get; set; }
    public string? BudgetaryItem { get; set; } = string.Empty;
    public decimal Dec { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Feb { get; set; }
    public decimal Jan { get; set; }
    public decimal Jul { get; set; }
    public decimal Jun { get; set; }
    public decimal Mar { get; set; }
    public decimal May { get; set; }
    public decimal Nov { get; set; }
    public decimal Oct { get; set; }
    public string? Quantity { get; set; } = string.Empty;
    public decimal Sep { get; set; }
}

public class HumanResourceDTO
{
    public decimal Apr { get; set; }
    public decimal Aug { get; set; }
    public string BudgetaryItem { get; set; } = string.Empty;
    public DateTime ContracStart { get; set; }
    public int ContractedHours { get; set; }
    public decimal Dec { get; set; }
    public decimal Feb { get; set; }
    public decimal Jan { get; set; }
    public decimal Jul { get; set; }
    public decimal Jun { get; set; }
    public decimal Mar { get; set; }
    public decimal May { get; set; }
    public decimal MonthlyRemuneration { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Nov { get; set; }
    public decimal Oct { get; set; }
    public string Profession { get; set; } = string.Empty;
    public string Rut { get; set; } = string.Empty;
    public decimal Sep { get; set; }
}

public class OtherElementDTO
{
    public decimal Apr { get; set; }
    public decimal Aug { get; set; }
    public string BudgetaryItem { get; set; } = string.Empty;
    public decimal Dec { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Feb { get; set; }
    public decimal Jan { get; set; }
    public decimal Jul { get; set; }
    public decimal Jun { get; set; }
    public decimal Mar { get; set; }
    public decimal May { get; set; }
    public decimal Nov { get; set; }
    public decimal Oct { get; set; }
    public decimal Sep { get; set; }
}

public class ExpensePlanResponseDTO
{
    public ICollection<FurnitureMachineryEquipmentDTO>? FurnitureMachineryAndEquipment { get; set; }
    public ICollection<GoodForUseOrServiceDTO>? GoodsForUseOrConsumptionAndServices { get; set; }
    public ICollection<HumanResourceDTO>? HumanResources { get; set; }
    public ICollection<MappingErrorDetail> MappingErrors { get; set; } = new List<MappingErrorDetail>();
    public ICollection<OtherElementDTO>? Others { get; set; }
}