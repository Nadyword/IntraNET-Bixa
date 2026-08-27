using Bixa.Backend.DataAccess.Models;

namespace Bixa.Backend.DataAccess.Entities;

public class FAQs : BaseEntities
{
    public required string Question { get; set; } = null!;
    public required string Response { get; set; } = null!;

    /// <summary>
    /// Posición en la que se muestra la pregunta. Menor valor = se muestra primero.
    /// </summary>
    public int DisplayOrder { get; set; }
}