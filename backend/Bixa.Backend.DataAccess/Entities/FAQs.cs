using Bixa.Backend.DataAccess.Models;

namespace Bixa.Backend.DataAccess.Entities;

public class FAQs : BaseEntities
{
    public required string Question { get; set; } = null!;
    public required string Response { get; set; } = null!;
}