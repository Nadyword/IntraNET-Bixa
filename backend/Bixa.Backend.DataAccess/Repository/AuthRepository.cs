using Bixa.Backend.DataAccess.Interfaces.Repositories;
using Bixa.Backend.DataAccess.Wrappers;
using Bixa.Backend.DataAccess.Context;
using Bixa.Backend.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Bixa.Backend.DataAccess.Repository;

/// <summary>
/// Repository for user data access queries (select operations).
/// This repository is now focused solely on retrieving and marking entities.
/// Persistence (SaveChanges) is handled by the Unit of Work.
/// </summary>
public class AuthRepository : IAuthRepository
{
    private readonly AppDbContext _context;
    private readonly ILogger<AuthRepository> _logger;

    /// <summary>
    /// Initializes a new instance of the AuthRepository.
    /// </summary>
    /// <param name="context">Database context.</param>
    /// <param name="loggerWrapper">Logger instance wrapper.</param>
    public AuthRepository(AppDbContext context, LoggerWrapper loggerWrapper)
    {
        _context = context;
        _logger = loggerWrapper.CreateLogger<AuthRepository>();
    }

    /// <summary>
    /// Retrieves a user by TaxId.
    /// </summary>
    /// <param name="taxId">User taxId.</param>
    /// <returns>User entity or null if not found. Exceptions are logged and rethrown.</returns>
    public async Task<Users?> GetUserByTaxId(string taxId)
    {
        try
        {
            var user = await _context.Users
                .Include(x => x.UserRol)
                .Where(u => u.TaxId == taxId)
                .FirstOrDefaultAsync()
                .ConfigureAwait(false);

            return user;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user by taxId from repository.");
            throw;
        }
    }

    /// <summary>
    /// Retrieves a user by refresh token and date.
    /// </summary>
    /// <param name="token">Refresh token.</param>
    /// <param name="date">Refresh token date.</param>
    /// <returns>User entity or null if not found. Exceptions are logged and rethrown.</returns>
    public async Task<Users?> GetUserByRefreshToken(string token, DateTime date)
    {
        try
        {
            var user = await _context.Users
                .Include(x => x.UserRol)
                .Where(u => u.RefreshTokenDate <= date && u.RefreshToken == token)
                .FirstOrDefaultAsync()
                .ConfigureAwait(false);

            return user;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user by refresh token from repository.");
            throw;
        }
    }

    /// <summary>
    /// Marks the user entity for an update (e.g., setting refresh token details).
    /// SaveChanges() must be called on the Unit of Work to persist these changes.
    /// </summary>
    /// <param name="user">User entity to update.</param>
    public Task SaveRefreshToken(Users user) =>
        Task.CompletedTask;
}