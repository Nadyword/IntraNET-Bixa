using Bixa.Backend.DataAccess.Interfaces.Repositories;
using Bixa.Backend.DataAccess.Context;
using Bixa.Backend.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;

namespace Bixa.Backend.DataAccess.Repository;

/// <summary>
/// Provides data access operations for HcMesRegistro entities.
/// </summary>
/// <param name="dbContext">The application's database context.</param>
public class HcMesRegistroRepository(AppDbContext dbContext) : IHcMesRegistroRepository
{
    private readonly AppDbContext _context = dbContext ?? throw new ArgumentNullException(nameof(dbContext));

    public Task<HcMesRegistro?> GetByCiAsync(string ci) =>
        _context.HcMesRegistros.FirstOrDefaultAsync(h => h.UserCi == ci);

    public async Task<HcMesRegistro> UpsertAsync(string ci, decimal mes1, decimal mes2, decimal mes3, decimal primaTrimBs)
    {
        var existing = await _context.HcMesRegistros.FirstOrDefaultAsync(h => h.UserCi == ci);

        if (existing == null)
        {
            existing = new HcMesRegistro
            {
                UserCi = ci,
                Mes1 = mes1,
                Mes2 = mes2,
                Mes3 = mes3,
                PrimaTrimBs = primaTrimBs,
            };
            await _context.HcMesRegistros.AddAsync(existing);
        }
        else
        {
            existing.Mes1 = mes1;
            existing.Mes2 = mes2;
            existing.Mes3 = mes3;
            existing.PrimaTrimBs = primaTrimBs;
        }

        return existing;
    }

    public Task<List<HcMesRegistro>> GetRecientesAsync(int take) =>
        _context.HcMesRegistros
            .Include(h => h.User)
            .OrderByDescending(h => h.UpdatedAt)
            .Take(take)
            .ToListAsync();

    public Task<List<HcMesRegistro>> BuscarAsync(string query)
    {
        var q = query.Trim().ToLower();

        return _context.HcMesRegistros
            .Include(h => h.User)
            .Where(h =>
                h.UserCi.ToLower().Contains(q) ||
                (h.User != null && h.User.FirstName != null && h.User.FirstName.ToLower().Contains(q)) ||
                (h.User != null && h.User.LastName != null && h.User.LastName.ToLower().Contains(q)))
            .OrderByDescending(h => h.UpdatedAt)
            .ToListAsync();
    }
}
