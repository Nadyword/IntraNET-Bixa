using Bixa.Backend.DataAccess.Entities;
using Bixa.Backend.DataAccess.Interfaces;
using Bixa.Backend.DataAccess.Models;
using Bixa.Backend.Models.DTOs.SolicitudesModelDTO;
using Bixa.Backend.Services.Interfaces;

namespace Bixa.Backend.Services.Services;

public class AprobacionesService(IAprobacionesRepository aprobacionesRepository) : IAprobacionesService
{
    private readonly IAprobacionesRepository _aprobacionesRepository = aprobacionesRepository ?? throw new ArgumentNullException(nameof(aprobacionesRepository));

    public async Task<List<Aprobacion>> GetAprobacionesByTramiteId(int tramiteId)
    {
        return await _aprobacionesRepository.GetAprobacionesByTramiteId(tramiteId);
    }

    public async Task<List<PorAprobar>> GetTramitesForAprobacion(string ci)
    {
        return await _aprobacionesRepository.GetTramitesForAprobacion(ci);
    }

    public async Task<bool> AprobarTramite(AprobarTramiteDTO aprobarTramiteDTO)
    {
        return await _aprobacionesRepository.AprobarTramite(aprobarTramiteDTO);
    }

    public async Task<bool> AprobarTramite(int tramiteId)
    {
        return await _aprobacionesRepository.AprobarTramite(tramiteId);
    }

    public async Task<bool> RechazarTramite(int tramiteId, string razon)
    {
        return await _aprobacionesRepository.RechazarTramite(tramiteId, razon);
    }
}