using Bixa.Backend.DataAccess.Interfaces.Repositories.Profit;

namespace Bixa.Backend.DataAccess.Interfaces.Repositories;

/// <summary>
/// Contrato de acceso a la segunda BD (solo lectura).
/// No expone SaveChangesAsync, transacciones ni campos de auditoría.
/// </summary>
public interface IReadOnlyUnitOfWork : IDisposable
{
    ISnEmpleProfitRepository SnEmple { get; }

    IGrupoFaProfitRepository GrupoFa { get; }

    IVacacionesProfitRepository Vacaciones { get; }

    IDiaEspecialesProfitRepository DiaEspeciales { get; }

    IUtilidadesProfitRepository Utilidades { get; }
}