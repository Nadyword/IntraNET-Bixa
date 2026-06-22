using Bixa.Backend.DataAccess.Context;
using Bixa.Backend.DataAccess.Interfaces.Repositories;
using Bixa.Backend.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;

namespace Bixa.Backend.DataAccess.Repository;

public class TramitesRepository(AppDbContext dbContext) : ITramitesRepository
{
    private readonly AppDbContext _context = dbContext ?? throw new ArgumentNullException(nameof(dbContext));

    public async Task<List<Tramite>> GetTramitesByCi(string ci)
    {
        return await _context.Tramites
            .Include(t => t.TipoTramite)
            .Include(t => t.SolicitudVacaciones)
            .Where(t => t.UserCi == ci)
            .ToListAsync();
    }

    public async Task<List<Tramite>> GetAllTramites()
    {
        return await _context.Tramites
            .Include(t => t.TipoTramite)
            .Include(t => t.SolicitudVacaciones)
            .ToListAsync();
    }
}