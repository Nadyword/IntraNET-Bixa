using Bixa.Backend.Models.Response;

namespace Bixa.Backend.Services.Interfaces;

public interface IEntityRelationshipValidatorService
{
    /// <summary>
    /// Checks if a given entity (by its ID) is referenced by any other entities in the database.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity to check (e.g., User, Product).</typeparam>
    /// <param name="entityId">The ID of the entity to check.</param>
    /// <param name="excludedTables">Optional list of table names (entity names) to exclude from the check (e.g., if cascade delete is intended for them).</param>
    /// <returns>A Result indicating success if no linked records are found, or failure with details of linked tables.</returns>
    Task<Result> HasLinkedRecords<TEntity>(int entityId, params string[] excludedTables) where TEntity : class;
}