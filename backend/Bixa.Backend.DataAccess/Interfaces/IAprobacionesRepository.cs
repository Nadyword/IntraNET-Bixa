using Bixa.Backend.Models.DTOs.SolicitudesModelDTO;
using Bixa.Backend.DataAccess.Entities;
using Bixa.Backend.DataAccess.Models;

namespace Bixa.Backend.DataAccess.Interfaces;

public interface IAprobacionesRepository
{
    Task<List<Aprobacion>> GetAprobacionesByTramiteId(int tramiteId);

    Task<List<PorAprobar>> GetTramitesForAprobacion(string ci);

    Task<bool> AprobarTramite(AprobarTramiteDTO aprobarTramiteDTO);
}