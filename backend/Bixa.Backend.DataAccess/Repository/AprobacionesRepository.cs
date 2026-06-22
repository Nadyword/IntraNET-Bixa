using Bixa.Backend.DataAccess.Templates.Profit;
using Bixa.Backend.DataAccess.Interfaces;
using Bixa.Backend.DataAccess.Context;
using Bixa.Backend.DataAccess.Entities;
using Bixa.Backend.DataAccess.Models;
using Microsoft.EntityFrameworkCore;
using Bixa.Backend.Models.Enums;

namespace Bixa.Backend.DataAccess.Repository;

public class AprobacionesRepository(AppDbContext dbContext) : IAprobacionesRepository
{
    private readonly AppDbContext _context = dbContext ?? throw new ArgumentNullException(nameof(dbContext));

    public async Task<List<Aprobacion>> GetAprobacionesByTramiteId(int tramiteId)
    {
        return await _context.Aprobaciones
            .Include(a => a.Aprobador)
            .Where(a => a.TramiteId == tramiteId)
            .OrderBy(a => a.Orden)
            .ToListAsync();
    }

    public async Task<List<PorAprobar>> GetTramitesForAprobacion(string ci)
    {
        return await _context.PorAprobar
            .FromSqlRaw(ProfitSqlTemplates.GetTramitesForAprobacion.Replace("@ci", ci))
            .ToListAsync();
    }

    public async Task<bool> AprobarTramite(int tramiteId, string ci, int estado)
    {
        var aprobacion = await _context.Aprobaciones.Where(a => a.TramiteId == tramiteId).ToListAsync();
        var registro = aprobacion.FirstOrDefault(a => a.Orden == 1 && a.AprobadorCi == ci);
        if (registro != null)
        {
            foreach (var item in aprobacion)
            {
                item.Orden--;
            }
        }

        registro?.Estado = (EstadoAprobacionEnum)estado;
        if (registro != null)
        {
            _context.Aprobaciones.Update(registro);
        }
        _context.Aprobaciones.UpdateRange(aprobacion);
        await _context.SaveChangesAsync();
        return true;
    }
}