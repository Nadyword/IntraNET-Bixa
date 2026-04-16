using System.ComponentModel;

namespace Bixa.Backend.Models.Enums;

public enum UserRolEnum
{
    [Description("Administrator")]
    Administrator = 1,

    [Description("Requester")]
    Requester = 2,

    [Description("Executive")]
    Executive = 3,

    [Description("Director")]
    Director = 4,

    [Description("Viewer")]
    Viewer = 5,
}