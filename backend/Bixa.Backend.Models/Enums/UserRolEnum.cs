using System.ComponentModel;

namespace Bixa.Backend.Models.Enums;

public enum UserRolEnum
{
    [Description("SuperIntendente")]
    SuperIntendente = 1,

    [Description("Gerente")]
    Gerente = 2,

    [Description("Supervisor")]
    Supervisor = 3,

    [Description("Empleado")]
    Empleado = 4,
}