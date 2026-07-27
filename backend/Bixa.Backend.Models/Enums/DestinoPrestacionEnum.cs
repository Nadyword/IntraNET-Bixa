using System.ComponentModel;

namespace Bixa.Backend.Models.Enums;

public enum DestinoPrestacionEnum
{
    [Description("Construcción, Adquisición o Mejora de Vivienda")]
    Vivienda = 1,

    [Description("Liberación de Hipoteca")]
    LiberacionHipoteca = 2,

    [Description("Pensiones Escolares")]
    PensionesEscolares = 3,

    [Description("Gastos por Atención Médica y Hospitalaria")]
    GastosMedicos = 4,
}
