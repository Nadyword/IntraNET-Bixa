using Bixa.Backend.Models.Response;
using System.Linq.Expressions;

namespace Bixa.Backend.DataAccess.Interfaces.Repositories;

/// <summary>
/// Generic interface for basic repository operations.
/// </summary>
/// <typeparam name="TEntity">The type of the entity (e.g., User, Product).</typeparam>
/// <typeparam name="TKey">The type of the entity's primary key (e.g., int, Guid).</typeparam>
public interface IRepository<TEntity, TKey> where TEntity : class
{
    /// <summary>
    /// Adds a new entity to the database.
    /// </summary>
    /// <param name="entity">The entity to add.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a tuple where the first item is the new entity's ID and the second is a boolean indicating success.</returns>
    Task<(TKey, bool)> AddAsync(TEntity entity);

    /// <summary>
    /// Adds a range of entities to the database asynchronously.
    /// </summary>
    /// <param name="entities">The list of entities to add.</param>
    Task AddRangeAsync(IEnumerable<TEntity> entities);

    /// <summary>
    /// Checks if any entity satisfies the condition specified by the predicate.
    /// </summary>
    /// <param name="predicate">A function to test each element for a condition.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains true if any entity exists, false otherwise.</returns>
    Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate);

    /// <summary>
    /// Deletes an entity from the database by its ID.
    /// </summary>
    /// <param name="id">The ID of the entity to delete.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains true if the entity was deleted, false otherwise.</returns>
    Task<bool> DeleteAsync(TKey id);

    /// <summary>
    /// Retrieves all entities with optional filtering and pagination.
    /// </summary>
    /// <param name="pageNumber">The page number for pagination (default is 1).</param>
    /// <param name="pageSize">The number of items per page for pagination (default is 10).</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a paginated list of entities.</returns>
    Task<List<TEntity>> GetAllAsync(int pageNumber = 1, int pageSize = 10);

    /// <summary>
    /// Retrieves an entity by its unique identifier.
    /// </summary>
    /// <param name="ci">The cedula of the entity to retrieve.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the entity if found, otherwise null.</returns>
    Task<IEnumerable<TEntity?>> GetByCiAsync(TKey ci);

    /// <summary>
    /// Updates an existing entity in the database.
    /// </summary>
    /// <param name="entity">The entity to update.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains true if the update was successful, false otherwise.</returns>
    Task<bool> UpdateAsync(TEntity entity);
}