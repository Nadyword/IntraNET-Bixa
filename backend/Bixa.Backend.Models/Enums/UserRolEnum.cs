using System.ComponentModel;

namespace Bixa.Backend.Models.Enums;

public enum UserRolEnum
{
    [Description("SuperIntendente")]
    SuperIntendente = 1,

    [Description("Supervisor")]
    Supervisor = 2,

    [Description("Empleado")]
    Empleado = 3,
}