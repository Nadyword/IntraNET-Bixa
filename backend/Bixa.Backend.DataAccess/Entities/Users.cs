using Bixa.Backend.DataAccess.Models;

namespace Bixa.Backend.DataAccess.Entities;

public class Users : BaseEntities
{
    public required string PasswordHash { get; set; }
    public required string TaxId { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public DateTime? LastLogin { get; set; }
    public int IdUserRol { get; set; }
    public bool IsActive { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenDate { get; set; }

    /*----------------------------------*/
    public virtual ICollection<Notifications>? Notification { get; set; }
    public virtual UserRol? UserRol { get; set; }
}