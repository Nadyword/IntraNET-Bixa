using Microsoft.EntityFrameworkCore.Storage;
using Bixa.Backend.DataAccess.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using System.ComponentModel;
using System.Security.Claims;
using Bixa.Backend.Models.Enums;
using Bixa.Backend.DataAccess.Interfaces.Repositories;
using Bixa.Backend.DataAccess.Models;
using Bixa.Backend.DataAccess.Context;
using Bixa.Backend.DataAccess.Wrappers;

namespace Bixa.Backend.DataAccess.UnitOfWork;

/// <summary>
/// Implements the Unit of Work pattern, encapsulating the database context
/// and managing transactions, including automatic population of audit fields.
/// </summary>
public class UnitOfWork : IUnitOfWork, IDisposable
{
    private readonly AppDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<UnitOfWork> _logger;
    private IDbContextTransaction? _currentTransaction;
    private bool _disposed = false;

    /// <summary>
    /// Initializes a new instance of the UnitOfWork.
    /// </summary>
    /// <param name="context">The application's database context.</param>
    /// <param name="loggerWrapper">Wrapper for logger instance.</param>
    /// <param name="httpContextAccessor">The accessor for the current HTTP context.</param>
    public UnitOfWork(AppDbContext context, LoggerWrapper loggerWrapper, IHttpContextAccessor httpContextAccessor)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = loggerWrapper.CreateLogger<UnitOfWork>();
        _httpContextAccessor = httpContextAccessor;

        Users = new UserRepository(_context);
        UserRols = new UserRolRepository(_context);
        Notifications = new NotificationRepository(_context);
    }

    public INotificationRepository Notifications { get; set; }

    public IUserRolRepository UserRols { get; private set; }

    // Repository properties
    public IUserRepository Users { get; private set; }

    /// <summary>
    /// Begins a new database transaction. If one is already active, it returns the existing one.
    /// </summary>
    /// <returns>An IDbContextTransaction representing the new or existing transaction.</returns>
    public async Task<IDbContextTransaction> BeginTransactionAsync()
    {
        if (_currentTransaction != null)
        {
            _logger.LogWarning("Attempted to start a new transaction while one is already active.");
            return _currentTransaction;
        }
        _currentTransaction = await _context.Database.BeginTransactionAsync();
        return _currentTransaction;
    }

    /// <summary>
    /// Commits the specified database transaction.
    /// </summary>
    public async Task CommitTransactionAsync()
    {
        if (_currentTransaction == null)
        {
            _logger.LogError("Attempted to commit a null transaction.");
            throw new InvalidOperationException("Cannot commit a null transaction.");
        }
        await _currentTransaction.CommitAsync();
        await _currentTransaction.DisposeAsync();
        _currentTransaction = null;
    }

    /// <summary>
    /// Disposes the database context.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public BudgetaryItemEnum? GetBudgetaryItemEnumFromDescription(string? description)
    {
        if (string.IsNullOrWhiteSpace(description))
            return null;

        string Normalize(string s) =>
            new string(s
                .Where(c => !char.IsWhiteSpace(c))
                .ToArray())
                .ToUpperInvariant();

        var normalizedInput = Normalize(description);

        foreach (var field in typeof(BudgetaryItemEnum).GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static))
        {
            var attr = Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) as DescriptionAttribute;
            if (attr != null && Normalize(attr.Description) == normalizedInput)
                return (BudgetaryItemEnum)field.GetValue(null)!;
        }
        return null;
    }

    /// <summary>
    /// Retrieves the ID of the current authenticated user from the HTTP context.
    /// </summary>
    /// <returns>The integer ID of the current user, or null if not authenticated or ID is not found/invalid.</returns>
    public int? GetCurrentUserId()
    {
        var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst("Id");
        if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
        {
            return userId;
        }
        _logger.LogWarning("Current user ID could not be retrieved from token claims. HttpContext.User.Identity.IsAuthenticated: {IsAuthenticated}",
            _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated);
        return null;
    }

    /// <summary>
    /// Retrieves the role of the current authenticated user from the HTTP context.
    /// Returns a UserRolEnum value when a valid role claim exists (by name or numeric),
    /// otherwise returns null.
    /// </summary>
    public UserRolEnum? GetCurrentUserRol()
    {
        var roleClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Role)
                        ?? _httpContextAccessor.HttpContext?.User?.FindFirst("role")
                        ?? _httpContextAccessor.HttpContext?.User?.FindFirst("roles");

        if (roleClaim == null || string.IsNullOrWhiteSpace(roleClaim.Value))
        {
            _logger.LogDebug("Role claim not found for current user. IsAuthenticated: {IsAuthenticated}",
                _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated);
            return null;
        }

        var roleValue = roleClaim.Value;

        // Try parse by enum name (case-insensitive)
        if (Enum.TryParse<UserRolEnum>(roleValue, true, out var roleByName))
            return roleByName;

        // Try parse numeric value
        if (int.TryParse(roleValue, out var numeric) && Enum.IsDefined(typeof(UserRolEnum), numeric))
            return (UserRolEnum)numeric;

        _logger.LogWarning("Unable to map role claim value '{RoleValue}' to UserRolEnum.", roleValue);
        return null;
    }

    /// <summary>
    /// Rolls back the specified database transaction.
    /// </summary>
    public async Task RollbackTransactionAsync()
    {
        if (_currentTransaction == null)
        {
            _logger.LogError("Attempted to rollback a null transaction.");
            throw new InvalidOperationException("Cannot rollback a null transaction.");
        }
        await _currentTransaction.RollbackAsync();
        await _currentTransaction.DisposeAsync();
        _currentTransaction = null;
    }

    /// <summary>
    /// Saves all pending changes in the unit of work to the database,
    /// applying audit field population before the save operation.
    /// </summary>
    /// <returns>The number of state entries written to the database.</returns>
    public async Task<int> SaveChangesAsync()
    {
        try
        {
            BeforeSaveChanges();
            return await _context.SaveChangesAsync().ConfigureAwait(false);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogError(ex, "Concurrency conflict occurred while saving changes.");
            throw;
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database update error occurred while saving changes.");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred while saving changes.");
            throw;
        }
    }

    /// <summary>
    /// Disposes the database context in a controlled manner.
    /// </summary>
    /// <param name="disposing">True to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _currentTransaction?.Dispose();
                _context.Dispose();
            }
            _disposed = true;
        }
    }

    /// <summary>
    /// Automatically populates audit fields (Created, Modified, ModifiedById)
    /// for entities inheriting from BaseEntities before saving changes.
    /// This method extracts the current user's ID from the HTTP context.
    /// </summary>
    private void BeforeSaveChanges()
    {
        int? currentUserId = GetCurrentUserId();
        foreach (var entry in _context.ChangeTracker.Entries())
        {
            if (entry.Entity is BaseEntities baseEntity)
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        baseEntity.CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);
                        baseEntity.UpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);
                        baseEntity.ModifiedById = currentUserId;
                        break;

                    case EntityState.Modified:
                        baseEntity.UpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);
                        baseEntity.ModifiedById = currentUserId;
                        entry.Property("CreatedAt").IsModified = false;
                        break;
                }
            }
        }
    }
}