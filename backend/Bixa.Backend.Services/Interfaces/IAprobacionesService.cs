using Bixa.Backend.DataAccess.Entities;
using Bixa.Backend.DataAccess.Models;
using Bixa.Backend.Models.DTOs.SolicitudesModelDTO;
using Bixa.Backend.Models.Response;

namespace Bixa.Backend.Services.Interfaces;

public interface IAprobacionesService
{
    Task<List<Aprobacion>> GetAprobacionesByTramiteId(int tramiteId);

    Task<List<HistorialAprobacionDTO>> GetHistorialAprobaciones(string? aprobadorCi);

    Task<List<PorAprobar>> GetTramitesForAprobacion(string ci);

    Task<bool> AprobarTramite(AprobarTramiteDTO aprobarTramiteDTO);

    Task<bool> AprobarTramite(int tramiteId);

    Task<bool> RechazarTramite(int tramiteId, string razon);
}