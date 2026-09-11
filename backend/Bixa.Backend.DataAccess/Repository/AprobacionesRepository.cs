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

    /// <summary>
    /// Historial de firmas ya resueltas (aprobadas o rechazadas). Los registros nunca se borran, así que
    /// esta consulta alimenta tanto el historial del firmante como el global del administrador.
    /// </summary>
    /// <param name="aprobadorCi">CI del firmante; si es nulo o vacío devuelve el historial global.</param>
    public async Task<List<HistorialAprobacionDTO>> GetHistorialAprobaciones(string? aprobadorCi)
    {
        var query = _context.Aprobaciones
            .AsNoTracking()
            .Include(a => a.Aprobador)
            .Include(a => a.Tramite!).ThenInclude(t => t.User)
            .Include(a => a.Tramite!).ThenInclude(t => t.TipoTramite)
            .Where(a => a.Estado == EstadoAprobacionEnum.Aprobado || a.Estado == EstadoAprobacionEnum.Rechazado);

        if (!string.IsNullOrWhiteSpace(aprobadorCi))
        {
            query = query.Where(a => a.AprobadorCi == aprobadorCi);
        }

        return await query
            .OrderByDescending(a => a.FechaRespuesta ?? a.UpdatedAt)
            .Select(a => new HistorialAprobacionDTO
            {
                TramiteId = a.TramiteId,
                TipoTramiteId = (TipoTramiteEnum)a.Tramite!.TipoTramiteId,
                TipoTramiteNombre = a.Tramite.TipoTramite != null ? a.Tramite.TipoTramite.Nombre : null,
                SolicitanteCi = a.Tramite.UserCi,
                SolicitanteNombre = a.Tramite.User != null
                    ? (a.Tramite.User.FirstName + " " + a.Tramite.User.LastName).Trim()
                    : null,
                AprobadorCi = a.AprobadorCi,
                AprobadorNombre = a.Aprobador != null
                    ? (a.Aprobador.FirstName + " " + a.Aprobador.LastName).Trim()
                    : a.Nombre,
                Orden = a.Orden,
                Estado = a.Estado,
                Comentario = a.Comentario,
                FechaRespuesta = a.FechaRespuesta ?? a.UpdatedAt,
                FechaSolicitud = a.Tramite.CreatedAt,
                EstadoTramite = a.Tramite.Estado,
                MotivoRechazo = a.Tramite.MotivoRechazo,
            })
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
            return false;
        }

        List<Aprobacion> aprobaciones = await _context.Aprobaciones
            .Where(a => a.TramiteId == aprobarTramiteDTO.TramiteId)
            .ToListAsync();

        Aprobacion? registro = BuscarFirmaPendiente(aprobaciones, aprobarTramiteDTO.Ci);
        if (registro == null)
        {
            return false;
        }

        return aprobarTramiteDTO.Estado > (int)EstadoAprobacionEnum.Aprobado
            ? await FirmaRechazo(registro, aprobaciones, aprobarTramiteDTO, tramite)
            : await FirmaAprobacion(registro, aprobaciones, aprobarTramiteDTO, tramite);
    }

    /// <summary>
    /// Rechazo desde el panel de administración: no hay firma que registrar, así que solo se anulan las
    /// firmas que quedaron pendientes. Las ya resueltas se conservan intactas para el historial.
    /// </summary>
    public async Task<bool> RechazarTramite(int tramiteId, string razon)
    {
        var tramite = await _context.Tramites.FirstOrDefaultAsync(t => t.Id == tramiteId);
        if (tramite == null)
        {
            return false;
        }

        var aprobaciones = await _context.Aprobaciones.Where(a => a.TramiteId == tramiteId).ToListAsync();
        AnularPendientes(aprobaciones);

        tramite.Estado = EstadoTramiteEnum.Rechazado;
        tramite.MotivoRechazo = razon;

        _context.Aprobaciones.UpdateRange(aprobaciones);
        _context.Tramites.Update(tramite);
        await _context.SaveChangesAsync();
        return true;
    }

    /// <summary>
    /// Localiza la firma pendiente del usuario dentro de la cadena. Filtrar por Pendiente evita que un
    /// firmante vuelva a actuar sobre un trámite que ya resolvió.
    /// </summary>
    private static Aprobacion? BuscarFirmaPendiente(List<Aprobacion> aprobaciones, string? ci)
    {
        return aprobaciones
            .Where(a => a.Estado == EstadoAprobacionEnum.Pendiente && a.AprobadorCi.Trim() == ci?.Trim())
            .OrderBy(a => a.Orden)
            .FirstOrDefault();
    }

    /// <summary>
    /// Marca como Anulado las firmas que ya no se van a solicitar porque el trámite se cortó antes de
    /// llegar a ellas. No se les asigna fecha de respuesta: el firmante nunca llegó a responder.
    /// </summary>
    private static void AnularPendientes(IEnumerable<Aprobacion> aprobaciones, int exceptoId = 0)
    {
        foreach (var pendiente in aprobaciones.Where(a => a.Id != exceptoId && a.Estado == EstadoAprobacionEnum.Pendiente))
        {
            pendiente.Estado = EstadoAprobacionEnum.Anulado;
        }
    }

    private async Task<bool> FirmaRechazo(Aprobacion registro, List<Aprobacion> aprobaciones, AprobarTramiteDTO aprobarTramiteDTO, Tramite tramite)
    {
        registro.Estado = EstadoAprobacionEnum.Rechazado;
        registro.Comentario = aprobarTramiteDTO.Motivo;
        registro.FechaRespuesta = DateTime.UtcNow;

        AnularPendientes(aprobaciones, registro.Id);

        tramite.Estado = EstadoTramiteEnum.Rechazado;
        tramite.MotivoRechazo = aprobarTramiteDTO.Motivo;

        _context.Aprobaciones.UpdateRange(aprobaciones);
        _context.Tramites.Update(tramite);
        await _context.SaveChangesAsync();
        return true;
    }

    private async Task<bool> FirmaAprobacion(Aprobacion registro, List<Aprobacion> aprobaciones, AprobarTramiteDTO aprobarTramiteDTO, Tramite tramite)
    {
        registro.Estado = EstadoAprobacionEnum.Aprobado;
        registro.FechaRespuesta = DateTime.UtcNow;
        if (!string.IsNullOrWhiteSpace(aprobarTramiteDTO.Motivo))
        {
            registro.Comentario = aprobarTramiteDTO.Motivo;
        }

        // El turno avanza en el trámite; el Orden de cada firma queda como se creó la cadena.
        tramite.OrdenActual = registro.Orden + 1;
        tramite.Estado = aprobaciones.Any(a => a.Orden > registro.Orden && a.Estado == EstadoAprobacionEnum.Pendiente)
            ? EstadoTramiteEnum.Revision
            : EstadoTramiteEnum.Firmado;

        _context.Aprobaciones.Update(registro);
        _context.Tramites.Update(tramite);
        await _context.SaveChangesAsync();
        return true;
    }
}
