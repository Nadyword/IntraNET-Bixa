using Bixa.Backend.Models.Enums;

namespace Bixa.Backend.Models.DTOs.SolicitudesModelDTO;

/// <summary>
/// Una firma ya resuelta: qué trámite era, de quién, quién lo firmó o rechazó y cuándo.
/// Es la unidad del historial de aprobaciones de un supervisor.
/// </summary>
public class HistorialAprobacionDTO
{
    public int TramiteId { get; set; }
    public TipoTramiteEnum TipoTramiteId { get; set; }
    public string? TipoTramiteNombre { get; set; }

    public string? SolicitanteCi { get; set; }
    public string? SolicitanteNombre { get; set; }

    public required string AprobadorCi { get; set; }
    public string? AprobadorNombre { get; set; }

    /// <summary>Posición del firmante dentro de la cadena de aprobación del trámite.</summary>
    public int Orden { get; set; }

    /// <summary>Acción del firmante: Aprobado o Rechazado.</summary>
    public EstadoAprobacionEnum Estado { get; set; }

    public string? Comentario { get; set; }
    public DateTime? FechaRespuesta { get; set; }
    public DateTime FechaSolicitud { get; set; }

    /// <summary>Estado en el que quedó el trámite completo, no solo este paso de la firma.</summary>
    public EstadoTramiteEnum EstadoTramite { get; set; }

    public string? MotivoRechazo { get; set; }
}
