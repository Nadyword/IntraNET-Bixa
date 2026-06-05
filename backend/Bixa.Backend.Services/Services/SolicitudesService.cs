using Bixa.Backend.DataAccess.Interfaces.Repositories;
using Bixa.Backend.Models.DTOs.SolicitudesModelDTO;
using Bixa.Backend.Services.Interfaces;

namespace Bixa.Backend.Services.Services;

public class SolicitudesService(ISolicitudesRepository solicitudesRepository) : ISolicitudesService
{
    private readonly ISolicitudesRepository _solicitudesRepository = solicitudesRepository;

    public Task<List<string>> GetAprovadoresPermisosByCi(string ci)
    {
        return _solicitudesRepository.GetAprovadoresPermisosByCi(ci);
    }

    //public Task<bool> AddSolicitudVacaciones(SolicVacacionesDTO solicitud)
    //{
    //    return _solicitudesRepository.AddSolicitudVacaciones(solicitud);
    //}
}