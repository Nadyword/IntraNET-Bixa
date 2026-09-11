using Bixa.Backend.Models.DTOs.SolicitudesModelDTO;
using Bixa.Backend.DataAccess.Entities;
using Bixa.Backend.DataAccess.Models;

namespace Bixa.Backend.DataAccess.Interfaces;

public interface IAprobacionesRepository
{
    Task<List<Aprobacion>> GetAprobacionesByTramiteId(int tramiteId);

    Task<List<PorAprobar>> GetTramitesForAprobacion(string ci);

    Task<List<HistorialAprobacionDTO>> GetHistorialAprobaciones(string? aprobadorCi);

    Task<bool> AprobarTramite(AprobarTramiteDTO aprobarTramiteDTO);

    Task<bool> AprobarTramite(int tramiteId);

    Task<bool> RechazarTramite(int tramiteId, string razon);
}