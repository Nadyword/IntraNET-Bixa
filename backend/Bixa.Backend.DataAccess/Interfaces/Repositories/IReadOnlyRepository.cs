using Bixa.Backend.Models.Response;
using Bixa.Backend.Models.Query;
using System.Linq.Expressions;

namespace Bixa.Backend.DataAccess.Interfaces.Repositories;

/// <summary>
/// Interfaz genérica para repositorios de solo lectura (segunda BD).
/// No expone AddAsync, UpdateAsync ni DeleteAsync.
/// </summary>
public interface IReadOnlyRepository<TEntity, TKey> where TEntity : class
{
    Task<IEnumerable<TEntity?>> GetByIdAsync(TKey id);

    Task<PaginatedResult<TEntity>> GetAllAsync(object filters, Pagination? pagination);

    Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate);
}