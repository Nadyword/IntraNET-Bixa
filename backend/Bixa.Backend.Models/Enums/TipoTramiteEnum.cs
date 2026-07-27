using System.ComponentModel;

namespace Bixa.Backend.Models.Enums;

public enum TipoTramiteEnum
{
    [Description("Anticipo de Utilidades")]
    Utilidades = 1,

    [Description("Prestaciones Sociales")]
    Sociales = 2,

    [Description("Préstamo Prestaciones")]
    Prestaciones = 3,

    [Description("Vacaciones")]
    Vacaciones = 4,

    [Description("Dia Especial")]
    DiaEspecial = 5,

    [Description("Constancia de Trabajo")]
    ConstanciaTrabajo = 6,
}
