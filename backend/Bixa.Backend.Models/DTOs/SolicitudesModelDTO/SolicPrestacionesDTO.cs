using Bixa.Backend.Models.Enums;
using Microsoft.AspNetCore.Http;

namespace Bixa.Backend.Models.DTOs.SolicitudesModelDTO;

public class SolicPrestacionesDTO
{
    public required string Ci { get; set; } = null!;

    /// <summary>
    /// 2 = Prestaciones Sociales (anticipo), 3 = Préstamo Prestaciones. Ver <see cref="TipoTramiteEnum"/>.
    /// </summary>
    public int TipoTramiteId { get; set; }
    public decimal Monto { get; set; }
    public DestinoPrestacionEnum Destino { get; set; }
    public string? Observaciones { get; set; }
    public EstadoTramiteEnum Estado { get; } = EstadoTramiteEnum.Creado;

    /// <summary>
    /// Cantidad de cuotas (máx. 52). Solo aplica cuando TipoTramiteId = Préstamo Prestaciones.
    /// </summary>
    public int? Cuotas { get; set; }

    /// <summary>
    /// Archivo de soporte obligatorio (PDF, JPG o PNG, máx. 3 MB).
    /// </summary>
    public IFormFile? Archivo { get; set; }
}
