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
}