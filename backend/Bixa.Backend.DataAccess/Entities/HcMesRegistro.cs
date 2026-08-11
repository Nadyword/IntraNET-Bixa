using Bixa.Backend.DataAccess.Models;

namespace Bixa.Backend.DataAccess.Entities;

public class HcMesRegistro : BaseEntities
{
    public required string UserCi { get; set; }
    public required decimal Mes1 { get; set; }
    public required decimal Mes2 { get; set; }
    public required decimal Mes3 { get; set; }
    public required decimal PrimaTrimBs { get; set; }

    /*-- Relaciones --*/
    public virtual Users? User { get; set; }
}
