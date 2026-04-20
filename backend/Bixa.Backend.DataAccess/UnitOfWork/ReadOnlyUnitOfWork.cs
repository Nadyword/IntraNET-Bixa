using Bixa.Backend.DataAccess.Context;
using Bixa.Backend.DataAccess.Interfaces.Repositories;
using Bixa.Backend.DataAccess.Interfaces.Repositories.Proxy;
using Bixa.Backend.DataAccess.Repository.Proxy;

namespace Bixa.Backend.DataAccess.UnitOfWork;

/// <summary>
/// Implementación del Unit of Work de solo lectura para la BD secundaria.
/// Sin SaveChangesAsync, sin transacciones, sin auditoría.
/// </summary>
public class ReadOnlyUnitOfWork : IReadOnlyUnitOfWork
{
    private readonly ProxyDbContext _context;
    private bool _disposed = false;

    public ReadOnlyUnitOfWork(ProxyDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        SnEmple = new SnEmpleProxyRepository(_context);
    }

    public ISnEmpleProxyRepository SnEmple { get; private set; }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public async Task<string?> GetUserByTaxidAsync(string? taxId)
    {
        var user = await _context.SnEmple.FindAsync(taxId);
        return user?.CorreoE ?? string.Empty;
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
                _context.Dispose();
            _disposed = true;
        }
    }
}