using Bixa.Backend.Models.Enums;
using Microsoft.EntityFrameworkCore.Storage;

namespace Bixa.Backend.DataAccess.Interfaces.Repositories;

/// <summary>
/// Defines the contract for the Unit of Work pattern, encapsulating database context
/// and managing transactions, including automatic population of audit fields.
/// </summary>
public interface IUnitOfWork : IDisposable
{
    INotificationRepository Notifications { get; }
    IUserRolRepository UserRols { get; }
    IUserRepository Users { get; }

    /// <summary>
    /// Begins a new database transaction.
    /// </summary>
    /// <returns>An IDbContextTransaction representing the new transaction.</returns>
    Task<IDbContextTransaction> BeginTransactionAsync();

    /// <summary>
    /// Commits the specified database transaction.
    /// </summary>
    Task CommitTransactionAsync();

    BudgetaryItemEnum? GetBudgetaryItemEnumFromDescription(string? description);

    /// <summary>
    /// Retrieves the ID of the current authenticated user from the HTTP context.
    /// </summary>
    /// <returns>The integer ID of the current user, or null if not authenticated or ID is not found/invalid.</returns>
    int? GetCurrentUserId();

    UserRolEnum? GetCurrentUserRol();

    /// <summary>
    /// Rolls back the specified database transaction.
    /// </summary>
    Task RollbackTransactionAsync();

    /// <summary>
    /// Saves all pending changes in the unit of work to the database.
    /// This method automatically populates audit fields before saving changes.
    /// </summary>
    /// <returns>The number of state entries written to the database.</returns>
    Task<int> SaveChangesAsync();
}