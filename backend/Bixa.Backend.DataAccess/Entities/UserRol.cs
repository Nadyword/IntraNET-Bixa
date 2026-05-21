using System.ComponentModel.DataAnnotations.Schema;

namespace Bixa.Backend.DataAccess.Entities;

public class UserRol
{
    public int Id { get; set; }
    public required string? Name { get; set; }
}