using System.ComponentModel;

namespace Bixa.Backend.Models.Enums;

public enum EstadoTramiteEnum
{
    [Description("Creado")]
    Creado = 1,

    [Description("En Revisión")]
    Revision = 2,

    [Description("Aprobado")]
    Aprobado = 3,

    [Description("Archivado")]
    Archivado = 4,

    [Description("Finalizado")]
    Finalizado = 5,

    [Description("Rechazado")]
    Rechazado = 6,
}
