using Microsoft.EntityFrameworkCore;
using Bixa.Backend.Models.Response;
using Bixa.Backend.DataAccess.Interfaces.Repositories;
using Bixa.Backend.DataAccess.Context;
using Bixa.Backend.DataAccess.Entities;

namespace Bixa.Backend.DataAccess.Repository;

/// <summary>
/// Provides data access operations for UserRol and Rol entities.
/// </summary>
public class UserRolRepository : IUserRolRepository
{
    private AppDbContext _context;

    /// <summary>
    /// Initializes a new instance of the UserRolRepository.
    /// </summary>
    /// <param name="dbContext">The application's database context.</param>
    public UserRolRepository(AppDbContext dbContext)
    {
        _context = dbContext;
    }

    /// <summary>
    /// Retrieves a paginated list of Rol entities.
    /// </summary>
    /// <returns>A paginated list of Rol entities.</returns>
    public async Task<PaginatedResult<UserRol>> GetAllRoles()
    {
        var totalCount = await _context.UserRols.CountAsync();
        var items = await _context.UserRols
                                  .Take(100)
                                  .ToListAsync();
        return new PaginatedResult<UserRol>(items, totalCount, 1, items.Count);
    }
}