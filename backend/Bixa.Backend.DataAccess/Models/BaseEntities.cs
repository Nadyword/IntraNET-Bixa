using Bixa.Backend.DataAccess.Entities;

namespace Bixa.Backend.DataAccess.Models;

/// <summary>
/// Base class for database entities, providing common audit fields like Id, Created, Modified, and ModifiedById.
/// </summary>
public abstract class BaseEntities
{
    /// <summary>
    /// Unique identifier for the entity (Primary Key).
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Date and time when the entity was created (UTC).
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Date and time when the entity was last modified (UTC).
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow; // Default to UTC now

    /// <summary>
    /// The ID of the user who last modified this entity (Foreign Key to User.Id).
    /// </summary>
    public int? ModifiedById { get; set; }

    /// <summary>
    /// Navigation property to the Users entity representing the user who last modified this entity.
    /// </summary>
    public virtual Users? ModifiedUser { get; set; }
}