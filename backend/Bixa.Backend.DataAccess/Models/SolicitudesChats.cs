using Microsoft.EntityFrameworkCore;

namespace Bixa.Backend.DataAccess.Models;

[Keyless]
public class SolicitudesChats
{
    public required string UserCi { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public int Respondido { get; set; }
}