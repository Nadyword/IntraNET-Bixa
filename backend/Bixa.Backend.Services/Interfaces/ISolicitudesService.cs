using Bixa.Backend.Models.DTOs.SolicitudesModelDTO;
using Bixa.Backend.Models.Response;
using Bixa.Backend.DataAccess.Models;
using Bixa.Backend.Models.DTOs.ReportesModelDTO;

namespace Bixa.Backend.Services.Interfaces;

public interface ISolicitudesService
{
    /// <summary>
    /// Lista de las personas que tienen que aprobar la solicitud de permiso de un usuario, identificada por su CI.
    /// </summary>
    /// <param name="ci">La cédula de identidad del usuario.</param>
    /// <returns>Una lista de aprobadores con nombre y cédula.</returns>
    Task<List<AprobadorPermisoInfo>> GetAprovadoresPermisosByCi(string ci);

    /// <summary>
    /// Agrega una nueva solicitud de vacaciones.
    /// </summary>
    /// <param name="solicitud">El DTO que contiene la información de la solicitud de vacaciones.</param>
    /// <returns>Un resultado indicando si la operación fue exitosa o no.</returns>
    Task<Result<bool>> AddSolicitudVacaciones(SolicVacacionesDTO solicitud);

    /// <summary>
    /// Obtiene todos los trámites de un usuario específico, identificados por su CI.
    /// </summary>
    /// <param name="ci">El CI del usuario para obtener sus trámites.</param>
    /// <returns>Una lista de DTOs de trámites del usuario.</returns>
    Task<Result<List<TramiteDTO>>> GetTramitesByCi(string ci);

    /// <summary>
    /// Obtiene todos los trámites que no estén finalizados.
    /// </summary>
    /// <returns>Una lista de DTOs de trámites no finalizados.</returns>
    Task<Result<List<TramiteDTO>>> GetAllTramites();

    /// <summary>
    /// Obtiene todas las aprobaciones de un trámite específico, identificado por su ID.
    /// </summary>
    /// <param name="tramiteId">El ID del trámite para obtener sus aprobaciones.</param>
    /// <returns>Una lista de DTOs de aprobaciones del trámite.</returns>
    Task<Result<List<AprobacionDTO>>> GetAprobacionesByTramiteId(int tramiteId);

    /// <summary>
    /// Obtiene todos los trámites que requieren aprobación de un usuario específico, identificado por su CI.
    /// </summary>
    /// <param name="ci">El CI del usuario para obtener sus trámites pendientes de aprobación.</param>
    /// <returns>Una lista de DTOs de trámites pendientes de aprobación.</returns>
    Task<Result<List<PorAprobar>>> GetTramitesForAprobacion(string ci);

    /// <summary>
    /// Aprueba un trámite específico, identificado por su ID.
    /// </summary>
    /// <param name="aprobarTramiteDTO">El DTO que contiene la información necesaria para aprobar el trámite, incluyendo el ID del trámite, el CI del aprobador y el estado de la aprobación.</param>
    /// <returns>Un resultado indicando si la operación fue exitosa o no.</returns>
    Task<Result<bool>> AprobarTramite(AprobarTramiteDTO aprobarTramiteDTO);

    /// <summary>
    /// Obtiene todos los trámites que han sido aprobados.
    /// </summary>
    /// <returns>Una lista de DTOs de trámites aprobados.</returns>
    Task<Result<List<TramiteDTO>>> GetAprobados();

    /// <summary>
    /// Obtiene la información del reporte de vacaciones para un trámite específico, identificado por su ID.
    /// </summary>
    /// <param name="tramiteId">El ID del trámite para obtener la información del reporte de vacaciones.</param>
    /// <returns>Un objeto TramiteReportModel con la información del reporte de vacaciones.</returns>
    Task<Result<TramiteReportModel>> GetInfoReporteVacaciones(int tramiteId);
}