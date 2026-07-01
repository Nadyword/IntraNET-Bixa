using Bixa.Backend.DataAccess.Context;
using Bixa.Backend.DataAccess.Entities;
using Bixa.Backend.DataAccess.Interfaces;
using Bixa.Backend.DataAccess.Models;
using Bixa.Backend.DataAccess.Templates.Profit;
using Bixa.Backend.Models.DTOs.SolicitudesModelDTO;
using Bixa.Backend.Models.Enums;
using Bixa.Backend.Models.Response;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;

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

    public async Task<bool> AprobarTramite(int tramiteId)
    {
        return await _context.Tramites
            .Where(t => t.Id == tramiteId)
            .ExecuteUpdateAsync(t => t.SetProperty(t => t.Estado, EstadoTramiteEnum.Aprobado)) > 0;
    }

    public async Task<bool> AprobarTramite(AprobarTramiteDTO aprobarTramiteDTO)
    {
        var tramite = await _context.Tramites.FirstOrDefaultAsync(t => t.Id == aprobarTramiteDTO.TramiteId);
        if (tramite == null)
        {
            return Result.Fail("Trámite no encontrado").IsSuccess;
        }

        if (aprobarTramiteDTO.Estado > 2)
        {
            return await FirmaRechazo(tramite, aprobarTramiteDTO);
        }

        List<Aprobacion> aprobacion = await _context.Aprobaciones.Where(a => a.TramiteId == aprobarTramiteDTO.TramiteId).ToListAsync();
        Aprobacion? registro = aprobacion.FirstOrDefault(a => a.AprobadorCi.Trim() == aprobarTramiteDTO!.Ci!.Trim());

        if (registro == null)
        {
            return Result.Fail("No se encontró la aprobación para el usuario actual").IsSuccess;
        }

        return await FirmaAprobacion(registro, aprobacion, aprobarTramiteDTO, tramite);
    }

    public async Task<bool> RechazarTramite(int tramiteId, string razon)
    {
        var tramite = await _context.Tramites.FirstOrDefaultAsync(t => t.Id == tramiteId);
        if (tramite == null)
        {
            return false;
        }
        var aprobacion = await _context.Aprobaciones.Where(a => a.TramiteId == tramiteId).ExecuteDeleteAsync();
        tramite.Estado = EstadoTramiteEnum.Rechazado;
        tramite.MotivoRechazo = razon;
        _context.Update(tramite);
        await _context.SaveChangesAsync();
        return true;
    }

    private async Task<bool> FirmaRechazo(Tramite tramite, AprobarTramiteDTO aprobarTramiteDTO)
    {
        tramite.Estado = EstadoTramiteEnum.Rechazado;
        tramite.MotivoRechazo = aprobarTramiteDTO.Motivo;
        await _context.Aprobaciones.Where(a => a.TramiteId == aprobarTramiteDTO.TramiteId).ExecuteDeleteAsync();
        _context.Update(tramite);
        await _context.SaveChangesAsync();
        return true;
    }

    private async Task<bool> FirmaAprobacion(Aprobacion registro, List<Aprobacion> aprobacion, AprobarTramiteDTO aprobarTramiteDTO, Tramite tramite)
    {
        List<int> EstadoTramite = [];
        if (registro != null)
        {
            registro.Estado = (EstadoAprobacionEnum)aprobarTramiteDTO.Estado;
            foreach (var item in aprobacion)
            {
                item.Orden--;
                EstadoTramite.Add(item.Orden);
            }
        }
        else
        {
            return Result.Fail("No se encontró la aprobación para el usuario actual").IsSuccess;
        }

        if (aprobacion.Any(a => a.Orden >= 1))
        {
            tramite.Estado = EstadoTramiteEnum.Revision;
        }
        else
        {
            tramite.Estado = EstadoTramiteEnum.Firmado;
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