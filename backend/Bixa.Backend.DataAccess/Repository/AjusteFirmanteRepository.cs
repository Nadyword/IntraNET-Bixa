using Bixa.Backend.DataAccess.Interfaces.Repositories;
using Bixa.Backend.DataAccess.Context;
using Bixa.Backend.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;

namespace Bixa.Backend.DataAccess.Repository;

/// <summary>
/// Provides data access operations for AjusteFirmante entities.
/// </summary>
/// <param name="dbContext">The application's database context.</param>
public class AjusteFirmanteRepository(AppDbContext dbContext) : IAjusteFirmanteRepository
{
    private readonly AppDbContext _context = dbContext ?? throw new ArgumentNullException(nameof(dbContext));

    public Task<List<AjusteFirmante>> GetByUserCiAsync(string ci) =>
        _context.AjustesFirmantes
            .Where(a => a.UserCi == ci)
            .OrderBy(a => a.Orden)
            .ToListAsync();

    public async Task ReplaceAsync(string ci, IEnumerable<AjusteFirmante> ajustes)
    {
        var actuales = await _context.AjustesFirmantes.Where(a => a.UserCi == ci).ToListAsync();
        _context.AjustesFirmantes.RemoveRange(actuales);

        await _context.AjustesFirmantes.AddRangeAsync(ajustes);
    }

    public Task<List<string>> GetCisConAjusteAsync() =>
        _context.AjustesFirmantes
            .Select(a => a.UserCi)
            .Distinct()
            .ToListAsync();
}
