using Bixa.Backend.DataAccess.Entities;

namespace Bixa.Backend.DataAccess.Models;

/// <summary>
/// Base class for database entities, providing common audit fields like Id, Created, Modified, and ModifiedByCi.
/// </summary>
public abstract class BaseEntities
{
    /// <summary>
    /// Unique identifier for the entity
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
    /// The CI of the user who last modified this entity (Foreign Key to User.CI).
    /// </summary>
    public string? ModifiedByCi { get; set; }

    /// <summary>
    /// Navigation property to the Users entity representing the user who last modified this entity.
    /// </summary>
    public virtual Users? ModifiedUser { get; set; }
}