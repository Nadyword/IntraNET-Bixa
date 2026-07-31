using Bixa.Backend.DataAccess.Interfaces.Repositories.Profit;
using Bixa.Backend.DataAccess.Interfaces.Repositories;
using Bixa.Backend.DataAccess.Repository.Profit;
using Bixa.Backend.DataAccess.Context;

namespace Bixa.Backend.DataAccess.UnitOfWork;

/// <summary>
/// Implementación del Unit of Work de solo lectura para la BD secundaria.
/// Sin SaveChangesAsync, sin transacciones, sin auditoría.
/// </summary>
public class ReadOnlyUnitOfWork : IReadOnlyUnitOfWork
{
    private readonly ProfitDbContext _context;
    private bool _disposed = false;

    public ReadOnlyUnitOfWork(ProfitDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        SnEmple = new SnEmpleProfitRepository(_context);
        GrupoFa = new GrupoFaProfitRepository(_context);
        Vacaciones = new VacacionesProfitRepository(_context);
        DiaEspeciales = new DiaEspecialesProfitRepository(_context);
        Utilidades = new UtilidadesProfitRepository(_context);
        ConsultaHc = new ConsultaHcProfitRepository(_context);
    }

    public ISnEmpleProfitRepository SnEmple { get; }
    public IGrupoFaProfitRepository GrupoFa { get; }
    public IVacacionesProfitRepository Vacaciones { get; }
    public IDiaEspecialesProfitRepository DiaEspeciales { get; }
    public IUtilidadesProfitRepository Utilidades { get; }
    public IConsultaHcProfitRepository ConsultaHc { get; }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
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