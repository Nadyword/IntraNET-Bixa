using System.ComponentModel;

namespace Bixa.Backend.Models.Enums;

public enum EstadoAprobacionEnum
{
    [Description("Pendiente")]
    Pendiente = 1,

    [Description("Aprobado")]
    Aprobado = 2,

    [Description("Rechazado")]
    Rechazado = 3,

    /// <summary>
    /// La firma ya no se solicitará: el trámite se cortó antes de llegar a este paso
    /// (rechazo de un firmante anterior o del administrador). Se conserva para el historial.
    /// </summary>
    [Description("Anulado")]
    Anulado = 4,
}
