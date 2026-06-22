using Microsoft.EntityFrameworkCore;
using Bixa.Backend.Models.Enums;

namespace Bixa.Backend.DataAccess.Models
{
    [Keyless]
    public class PorAprobar
    {
        public int TramiteId { get; set; }
        public string? AprobadorCi { get; set; }
        public int Orden { get; set; }
        public string? Comentario { get; set; }
        public TipoTramiteEnum TipoTramiteId { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
    }
}