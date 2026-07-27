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
            .Include(t => t.SolicitudDiasEspeciales)
            .Include(t => t.SolicitudUtilidades)
            .Include(t => t.SolicitudPrestaciones)
            .Include(t => t.SolicitudConstanciaTrabajo)
            .Where(t => t.UserCi == ci)
            .ToListAsync();
    }

    public async Task<List<Tramite>> GetAllTramites()
    {
        return await _context.Tramites
            .Include(t => t.TipoTramite)
            .Include(t => t.SolicitudVacaciones)
            .Include(t => t.SolicitudDiasEspeciales)
            .Include(t => t.SolicitudUtilidades)
            .Include(t => t.SolicitudPrestaciones)
            .Include(t => t.SolicitudConstanciaTrabajo)
            .ToListAsync();
    }

    public async Task<List<Tramite>> GetAprobados()
    {
        return await _context.Tramites
            .Include(t => t.TipoTramite)
            .Include(t => t.SolicitudVacaciones)
            .Include(t => t.SolicitudDiasEspeciales)
            .Include(t => t.SolicitudUtilidades)
            .Include(t => t.SolicitudPrestaciones)
            .Include(t => t.SolicitudConstanciaTrabajo)
            .Include(t => t.User)
            .Where(t => t.Estado >= EstadoTramiteEnum.Firmado)
            .ToListAsync();
    }

    public async Task<Tramite> GetTramiteById(int tramiteId)
    {
        return await _context.Tramites.FirstOrDefaultAsync(t => t.Id == tramiteId) ?? throw new KeyNotFoundException($"Tramite with Id {tramiteId} not found.");
    }

    public async Task<Tramite?> GetTramiteDetalladoById(int tramiteId)
    {
        return await _context.Tramites
            .Include(t => t.TipoTramite)
            .Include(t => t.SolicitudVacaciones)
            .Include(t => t.SolicitudDiasEspeciales)
            .Include(t => t.SolicitudUtilidades)
            .Include(t => t.SolicitudPrestaciones)
            .Include(t => t.SolicitudConstanciaTrabajo)
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.Id == tramiteId);
    }

    public async Task<bool> ArchivarTramite(int tramiteId)
    {
        Tramite tramite = _context.Tramites.FirstOrDefault(t => t.Id == tramiteId) ?? throw new KeyNotFoundException($"Tramite with Id {tramiteId} not found.");

        tramite.Estado = EstadoTramiteEnum.Tramitando;
        _context.Tramites.Update(tramite);

        return await _context.SaveChangesAsync() > 0;
    }
}