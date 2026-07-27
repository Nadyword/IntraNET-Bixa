using Bixa.Backend.Models.Enums;

namespace Bixa.Backend.Models.DTOs.SolicitudesModelDTO;

public class SolicConstanciaTrabajoDTO
{
    public int TipoTramiteId { get; } = (int)TipoTramiteEnum.ConstanciaTrabajo;
    public required string Ci { get; set; } = null!;
    public bool ConSueldo { get; set; }
    public bool DirigidoAEspecifico { get; set; }
    public string? DirigidoA { get; set; }

    /// <summary>
    /// Esta solicitud no requiere firmantes previos, por lo que queda lista de inmediato
    /// para la aprobación final del administrador.
    /// </summary>
    public EstadoTramiteEnum Estado { get; } = EstadoTramiteEnum.Firmado;
}
