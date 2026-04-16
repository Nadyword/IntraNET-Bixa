using Bixa.Backend.Models.Query;
using Bixa.Backend.Models.Response;

namespace Bixa.Backend.Services.Interfaces;

/// <summary>
/// Generic interface for basic CRUD service operations.
/// </summary>
/// <typeparam name="TEntityDto">The DTO type representing the entity for display/response.</typeparam>
/// <typeparam name="TInsertDto">The DTO type used for creating new entities.</typeparam>
/// <typeparam name="TUpdateDto">The DTO type used for updating existing entities.</typeparam>
/// <typeparam name="TFilterDto">The DTO type used for filtering entities in queries.</typeparam>
/// <typeparam name="TKey">The type of the entity's primary key (e.g., int, Guid).</typeparam>
public interface IService<TEntityDto, TInsertDto, TUpdateDto, TFilterDto, TKey>
    where TEntityDto : class
    where TInsertDto : class
    where TUpdateDto : class
    where TFilterDto : class, new()
{
    // POST operation
    Task<Result<TKey>> AddAsync(TInsertDto dto);

    // DELETE operation
    Task<Result<bool>> DeleteAsync(TKey id);

    // GET operations
    Task<Result<PaginatedResult<TEntityDto>>> GetAllAsync(SearchQuery<TFilterDto> filters);

    Task<Result<TEntityDto>> GetByIdAsync(TKey id);

    // PUT operation
    Task<Result<bool>> UpdateAsync(TUpdateDto dto);
}