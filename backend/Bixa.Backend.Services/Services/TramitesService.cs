using Bixa.Backend.DataAccess.Interfaces.Repositories;
using Bixa.Backend.DataAccess.Entities;
using Bixa.Backend.Services.Interfaces;

namespace Bixa.Backend.Services.Services;

public class TramitesService(ITramitesRepository tramitesRepository) : ITramitesService
{
    private readonly ITramitesRepository _tramitesRepository = tramitesRepository;

    public async Task<List<Tramite>> GetTramitesByCi(string ci)
    {
        return await _tramitesRepository.GetTramitesByCi(ci);
    }

    public async Task<List<Tramite>> GetAllTramites()
    {
        return await _tramitesRepository.GetAllTramites();
    }

    public async Task<List<Tramite>> GetAprobados()
    {
        return await _tramitesRepository.GetAprobados();
    }

    public async Task<Tramite> GetTramiteById(int tramiteId)
    {
        return await _tramitesRepository.GetTramiteById(tramiteId);
    }

    public async Task<Tramite?> GetTramiteDetalladoById(int tramiteId)
    {
        return await _tramitesRepository.GetTramiteDetalladoById(tramiteId);
    }

    public async Task<bool> ArchivarTramite(int tramiteId)
    {
        return await _tramitesRepository.ArchivarTramite(tramiteId);
    }
}