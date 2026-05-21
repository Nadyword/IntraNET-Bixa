using Bixa.Backend.DataAccess.Models;
using Microsoft.EntityFrameworkCore;

namespace Bixa.Backend.DataAccess.Entities;

[PrimaryKey(nameof(Ci))]
public class Users : BaseEntities
{
    public required string? PasswordHash { get; set; }
    public required string Ci { get; set; }
    public required string? FirstName { get; set; }
    public required string? LastName { get; set; }
    public DateTime? LastLogin { get; set; }
    public int IdUserRol { get; set; }
    public bool IsActive { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenDate { get; set; }

    /*----------------------------------*/
    public virtual ICollection<Notifications>? Notification { get; set; }
    public virtual UserRol? UserRol { get; set; }
    public virtual ICollection<SoporteChat>? SoporteChats { get; set; }
    public virtual ICollection<Tramite>? Tramites { get; set; }
    public virtual ICollection<Aprobacion>? Aprobaciones { get; set; }
}