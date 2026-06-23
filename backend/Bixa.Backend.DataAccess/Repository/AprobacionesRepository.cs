using Bixa.Backend.DataAccess.Context;
using Bixa.Backend.DataAccess.Entities;
using Bixa.Backend.DataAccess.Interfaces;
using Bixa.Backend.DataAccess.Models;
using Bixa.Backend.DataAccess.Templates.Profit;
using Bixa.Backend.Models.DTOs.SolicitudesModelDTO;
using Bixa.Backend.Models.Enums;
using Microsoft.EntityFrameworkCore;

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

    public async Task<bool> AprobarTramite(AprobarTramiteDTO aprobarTramiteDTO)
    {
        var tramite = await _context.Tramites.FirstOrDefaultAsync(t => t.Id == aprobarTramiteDTO.TramiteId);
        if (aprobarTramiteDTO.Estado > 2)
        {
            tramite.Estado = EstadoTramiteEnum.Rechazado;
            tramite.MotivoRechazo = aprobarTramiteDTO.Motivo;
            await _context.Aprobaciones.Where(a => a.TramiteId == aprobarTramiteDTO.TramiteId).ExecuteDeleteAsync();
            return true;
        }

        var aprobacion = await _context.Aprobaciones.Where(a => a.TramiteId == aprobarTramiteDTO.TramiteId).ToListAsync();
        List<int> EstadoTramite = [];
        var registro = aprobacion.FirstOrDefault(a => a.AprobadorCi.Trim() == aprobarTramiteDTO.Ci.Trim());
        if (registro != null)
        {
            registro.Estado = (EstadoAprobacionEnum)aprobarTramiteDTO.Estado;
            foreach (var item in aprobacion)
            {
                item.Orden--;
                EstadoTramite.Add(item.Orden);
            }
        }

        if (aprobacion.Any(a => a.Orden >= 1))
        {
            tramite?.Estado = EstadoTramiteEnum.Revision;
        }
        else
        {
            tramite?.Estado = EstadoTramiteEnum.Aprobado;
        }

        _ = _context.Update(tramite);

        if (registro != null)
        {
            _context.Aprobaciones.Update(registro);
        }
        _context.Aprobaciones.UpdateRange(aprobacion);
        await _context.SaveChangesAsync();
        return true;
    }
}