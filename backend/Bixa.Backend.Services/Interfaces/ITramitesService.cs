using Bixa.Backend.DataAccess.Entities;

namespace Bixa.Backend.Services.Interfaces;

public interface ITramitesService
{
    Task<List<Tramite>> GetTramitesByCi(string ci);

    Task<List<Tramite>> GetAllTramites();

    Task<List<Tramite>> GetAprobados();

    Task<Tramite> GetTramiteById(int tramiteId);
}