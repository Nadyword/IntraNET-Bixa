using Bixa.Backend.DataAccess.Models;

namespace Bixa.Backend.DataAccess.Entities;

public class SoporteChat : BaseEntities
{
    public required string UserCi { get; set; }
    public required string? Message { get; set; }
    public required bool IsRead { get; set; }
    public string? RespondidoPorCi { get; set; }

    /*-- Relaciones --*/
    public virtual Users? RespondidoPor { get; set; }
}