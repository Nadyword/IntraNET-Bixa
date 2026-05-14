using Bixa.Backend.Models.Response;
using Bixa.Backend.Models.Query;

namespace Bixa.Backend.Services.Interfaces;

/// <summary>
/// Generic interface for basic CRUD service operations.
/// </summary>
/// <typeparam name="TEntityDto">The DTO type representing the entity for display/response.</typeparam>
/// <typeparam name="TInsertDto">The DTO type used for creating new entities.</typeparam>
/// <typeparam name="TUpdateDto">The DTO type used for updating existing entities.</typeparam>
/// <typeparam name="TKey">The type of the entity's primary key (e.g., int, Guid).</typeparam>
public interface IService<TEntityDto, TInsertDto, TUpdateDto, TKey>
    where TEntityDto : class
    where TInsertDto : class
    where TUpdateDto : class
{
    // POST operation
    Task<Result<TKey>> AddAsync(TInsertDto dto);

    // DELETE operation
    Task<Result<bool>> DeleteAsync(TKey ci);

    // GET operations
    Task<Result<List<TEntityDto>>> GetAllAsync(int pageNumber, int pageSize);

    Task<Result<TEntityDto>> GetByCiAsync(TKey ci);

    // PUT operation
    Task<Result<bool>> UpdateAsync(TUpdateDto dto);
}