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
}
