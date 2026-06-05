using Bixa.Backend.DataAccess.Entities;
using Bixa.Backend.DataAccess.Entities.Solicitudes;
using Microsoft.EntityFrameworkCore;

namespace Bixa.Backend.DataAccess.Interfaces.Repositories;

public interface ISolicitudesRepository
{
    /// <summary>
    /// Lista de las personas que tienen que aprobar la solicitud de permiso de un usuario, identificada por su CI.
    /// </summary>
    /// <param name="ci">La cédula de identidad del usuario.</param>
    /// <returns>Una lista de nombres de los aprobadores.</returns>
    Task<List<string>> GetAprovadoresPermisosByCi(string ci);

    /// <summary>
    /// Agrega una nueva solicitud de vacaciones a la base de datos.
    /// </summary>
    /// <param name="solicitud"></param>
    /// <returns></returns>
    Task<bool> AddSolicitudVacaciones(SolicitudVacaciones solicitud);

    /// <summary>
    /// Agrega un nuevo trámite a la base de datos.
    /// </summary>
    /// <param name="tramite"></param>
    /// <returns></returns>
    Task<bool> AddNewTramite(Tramite tramite);
}