using Bixa.Backend.DataAccess.Entities.Solicitudes;
using Bixa.Backend.DataAccess.Entities;
using Bixa.Backend.DataAccess.Models;

namespace Bixa.Backend.DataAccess.Interfaces.Repositories;

public interface ISolicitudesRepository
{
    /// <summary>
    /// Lista de las personas que tienen que aprobar la solicitud de permiso de un usuario, identificada por su CI.
    /// </summary>
    /// <param name="ci">La cédula de identidad del usuario.</param>
    /// <returns>Una lista de aprobadores con nombre y cédula.</returns>
    Task<List<AprobadorPermisoInfo>> GetAprovadoresPermisosByCi(string ci);

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
    /// <returns>Id trámite</returns>
    Task<int> AddNewTramite(Tramite tramite);

    /// <summary>
    /// Agrega una aprobación para un trámite a la base de datos.
    /// </summary>
    /// <param name="aprobaciones">La aprobación a agregar.</param>
    /// <returns>El número de aprobaciones agregadas.</returns>
    Task<int> AddAprobaciones(Aprobacion aprobaciones);

    /// <summary>
    ///  Obtiene una solicitud de vacaciones por el ID del trámite.
    /// </summary>
    /// <param name="tramiteId"></param>
    /// <returns></returns>
    Task<SolicitudVacaciones> GetSolicitudVacacionesByTramiteId(int tramiteId);

    /// <summary>
    /// Agrega una nueva solicitud de día especial a la base de datos.
    /// </summary>
    /// <param name="solicitud"></param>
    /// <returns></returns>
    Task<bool> AddSolicitudDiasEspeciales(SolicitudDiasEspeciales solicitud);

    /// <summary>
    ///  Obtiene una solicitud de día especial por el ID del trámite.
    /// </summary>
    /// <param name="tramiteId"></param>
    /// <returns></returns>
    Task<SolicitudDiasEspeciales> GetSolicitudDiasEspecialesByTramiteId(int tramiteId);

    /// <summary>
    /// Agrega una nueva solicitud de anticipo de utilidades a la base de datos.
    /// </summary>
    /// <param name="solicitud"></param>
    /// <returns></returns>
    Task<bool> AddSolicitudUtilidades(SolicitudUtilidades solicitud);

    /// <summary>
    ///  Obtiene una solicitud de anticipo de utilidades por el ID del trámite.
    /// </summary>
    /// <param name="tramiteId"></param>
    /// <returns></returns>
    Task<SolicitudUtilidades> GetSolicitudUtilidadesByTramiteId(int tramiteId);

    /// <summary>
    /// Obtiene un usuario por su cédula de identidad (CI).
    /// </summary>
    /// <param name="ci"></param>
    /// <returns></returns>
    Task<Users> GetUserByCi(string ci);
}