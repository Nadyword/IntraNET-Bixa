using Bixa.Backend.DataAccess.Interfaces.Repositories;
using Bixa.Backend.DataAccess.Context;
using Bixa.Backend.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;
using Bixa.Backend.Models.Enums;

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

    public async Task<List<Tramite>> GetAprobados()
    {
        return await _context.Tramites
            .Include(t => t.TipoTramite)
            .Include(t => t.SolicitudVacaciones)
            .Where(t => t.Estado == EstadoTramiteEnum.Aprobado)
            .ToListAsync();
    }

    public async Task<Tramite> GetTramiteById(int tramiteId)
    {
        return await _context.Tramites.FirstOrDefaultAsync(t => t.Id == tramiteId);
    }
}