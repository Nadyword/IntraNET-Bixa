using System.ComponentModel;

namespace Bixa.Backend.Models.Enums;

public enum EstadoTramiteEnum
{
    [Description("Creado")]
    Creado = 1,

    [Description("En Revisión")]
    Revision = 2,

    [Description("Firmado")]
    Firmado = 3,

    [Description("Aprobado")]
    Aprobado = 4,

    [Description("Archivado")]
    Archivado = 5,

    [Description("Finalizado")]
    Finalizado = 6,

    [Description("Rechazado")]
    Rechazado = 7,
}