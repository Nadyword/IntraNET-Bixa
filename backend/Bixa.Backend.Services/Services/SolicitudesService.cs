using AutoMapper;
using Bixa.Backend.DataAccess.Entities;
using Bixa.Backend.DataAccess.Entities.Solicitudes;
using Bixa.Backend.DataAccess.Interfaces.Repositories;
using Bixa.Backend.DataAccess.Interfaces.Repositories.Profit;
using Bixa.Backend.DataAccess.Models;
using Bixa.Backend.Models.DTOs.ReportesModelDTO;
using Bixa.Backend.Models.DTOs.SolicitudesModelDTO;
using Bixa.Backend.Models.Enums;
using Bixa.Backend.Models.Response;
using Bixa.Backend.Services.Interfaces;

namespace Bixa.Backend.Services.Services;

public class SolicitudesService(ISolicitudesRepository solicitudesRepository, IMapper mapper, IUnitOfWork unitOfWork, ITramitesService tramitesService, IAprobacionesService aprobacionesService, IGrupoFaProfitRepository grupoFaProfitRepository, ISnEmpleProfitRepository snEmpleProfitRepository) : ISolicitudesService
{
    private static readonly string[] MotivosDiaEspecialValidos =
    [
        "Cédula de identidad",
        "Libreta militar",
        "Certificado de salud",
        "Licencia de conducir",
        "Pasaporte",
        "Citaciones judiciales, policiales o civiles",
        "Inscripción escolar hijos/trabajador",
        "Carta de soltería",
        "Constancia de concubinato",
        "Constancia de residencia",
    ];

    private readonly ISnEmpleProfitRepository _snEmpleProfitRepository = snEmpleProfitRepository;
    private readonly IGrupoFaProfitRepository _grupoFaProfitRepository = grupoFaProfitRepository;
    private readonly ISolicitudesRepository _solicitudesRepository = solicitudesRepository;
    private readonly IAprobacionesService _aprobacionesService = aprobacionesService;
    private readonly ITramitesService _tramitesService = tramitesService;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;

    public async Task<List<AprobadorPermisoInfo>> GetAprovadoresPermisosByCi(string ci)
    {
        return await _solicitudesRepository.GetAprovadoresPermisosByCi(ci);
    }

    public async Task<Result<bool>> AddSolicitudVacaciones(SolicVacacionesDTO solicitud)
    {
        if (!(solicitud.FechaFin >= solicitud.FechaInicio) || !(solicitud.FechaInicio >= DateTime.Now.Date))
        {
            return Result.Fail<bool>("Fechas inválidas: La fecha de fin debe ser mayor que la fecha de inicio y la fecha de inicio no puede ser en el pasado.");
        }

        solicitud.Ci = UtilityService.NormalizeCiFormat(solicitud.Ci);
        await _unitOfWork.BeginTransactionAsync();
        var tramite = _mapper.Map<Tramite>(solicitud);

        try
        {
            int tramiteId = await _solicitudesRepository.AddNewTramite(tramite);

            if (tramiteId < 1)
            {
                await _unitOfWork.RollbackTransactionAsync();
                return Result.Fail<bool>("Error al guardar el trámite");
            }

            List<AprobadorPermisoInfo> aprobadores = await GetAprovadoresPermisosByCi(solicitud.Ci);

            List<Aprobacion> aprobaciones = [];

            foreach (AprobadorPermisoInfo aprobador in aprobadores)
            {
                int result = await _solicitudesRepository.AddAprobaciones(new Aprobacion
                {
                    TramiteId = tramiteId,
                    Nombre = aprobador.Nombre,
                    AprobadorCi = aprobador.Ci,
                    Orden = aprobadores.IndexOf(aprobador) + 1,
                    Estado = EstadoAprobacionEnum.Pendiente
                });

                if (result < 1)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return Result.Fail<bool>($"Error al guardar la aprobación para el aprobador {aprobador.Nombre} ({aprobador.Ci})");
                }
            }

            SolicitudVacaciones SVacacion = _mapper.Map<SolicitudVacaciones>(solicitud);
            SVacacion.TramiteId = tramiteId;

            bool solicitudResult = await _solicitudesRepository.AddSolicitudVacaciones(SVacacion);
            if (!solicitudResult)
            {
                await _unitOfWork.RollbackTransactionAsync();
                return Result.Fail<bool>("Error al guardar la solicitud de vacaciones");
            }

            await _unitOfWork.CommitTransactionAsync();
            return Result.Success(true);
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            return Result.Fail<bool>($"Error en el proceso: {ex.Message}");
        }
    }

    public async Task<Result<bool>> AddSolicitudDiasEspeciales(SolicDiaEspecialDTO solicitud)
    {
        if (solicitud.Fecha.Date < DateTime.Now.Date)
        {
            return Result.Fail<bool>("Fecha inválida: la fecha no puede ser en el pasado.");
        }

        if (!MotivosDiaEspecialValidos.Contains(solicitud.Motivo))
        {
            return Result.Fail<bool>("Motivo inválido: selecciona una de las opciones permitidas.");
        }

        solicitud.Ci = UtilityService.NormalizeCiFormat(solicitud.Ci);
        await _unitOfWork.BeginTransactionAsync();
        var tramite = _mapper.Map<Tramite>(solicitud);

        try
        {
            int tramiteId = await _solicitudesRepository.AddNewTramite(tramite);

            if (tramiteId < 1)
            {
                await _unitOfWork.RollbackTransactionAsync();
                return Result.Fail<bool>("Error al guardar el trámite");
            }

            List<AprobadorPermisoInfo> aprobadores = await GetAprovadoresPermisosByCi(solicitud.Ci);

            foreach (AprobadorPermisoInfo aprobador in aprobadores)
            {
                int result = await _solicitudesRepository.AddAprobaciones(new Aprobacion
                {
                    TramiteId = tramiteId,
                    Nombre = aprobador.Nombre,
                    AprobadorCi = aprobador.Ci,
                    Orden = aprobadores.IndexOf(aprobador) + 1,
                    Estado = EstadoAprobacionEnum.Pendiente
                });

                if (result < 1)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return Result.Fail<bool>($"Error al guardar la aprobación para el aprobador {aprobador.Nombre} ({aprobador.Ci})");
                }
            }

            SolicitudDiasEspeciales sDiaEspecial = _mapper.Map<SolicitudDiasEspeciales>(solicitud);
            sDiaEspecial.TramiteId = tramiteId;

            bool solicitudResult = await _solicitudesRepository.AddSolicitudDiasEspeciales(sDiaEspecial);
            if (!solicitudResult)
            {
                await _unitOfWork.RollbackTransactionAsync();
                return Result.Fail<bool>("Error al guardar la solicitud de día especial");
            }

            await _unitOfWork.CommitTransactionAsync();
            return Result.Success(true);
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            return Result.Fail<bool>($"Error en el proceso: {ex.Message}");
        }
    }

    public async Task<Result<List<TramiteDTO>>> GetTramitesByCi(string ci)
    {
        ci = UtilityService.NormalizeCiFormat(ci);
        var tramites = await _tramitesService.GetTramitesByCi(ci);
        var tramitesDto = _mapper.Map<List<TramiteDTO>>(tramites);
        return Result.Success(tramitesDto);
    }

    public async Task<Result<List<TramiteDTO>>> GetAllTramites()
    {
        var tramites = await _tramitesService.GetAllTramites();
        var tramitesDto = _mapper.Map<List<TramiteDTO>>(tramites);
        return Result.Success(tramitesDto);
    }

    public async Task<Result<List<AprobacionDTO>>> GetAprobacionesByTramiteId(int tramiteId)
    {
        var aprobaciones = await _aprobacionesService.GetAprobacionesByTramiteId(tramiteId);
        var aprobacionesDto = _mapper.Map<List<AprobacionDTO>>(aprobaciones);
        return Result.Success(aprobacionesDto);
    }

    public async Task<Result<List<PorAprobar>>> GetTramitesForAprobacion(string ci)
    {
        ci = UtilityService.NormalizeCiFormat(ci);
        var tramites = await _aprobacionesService.GetTramitesForAprobacion(ci);
        return Result.Success(tramites);
    }

    public async Task<Result<bool>> AprobarTramite(AprobarTramiteDTO aprobarTramiteDTO)
    {
        var result = await _aprobacionesService.AprobarTramite(aprobarTramiteDTO);
        return Result.Success(result);
    }

    public async Task<Result<bool>> AprobarTramite(int tramiteId)
    {
        var result = await _aprobacionesService.AprobarTramite(tramiteId);
        if (!result)
        {
            return Result.Fail<bool>($"Error al aprobar el trámite con ID {tramiteId}");
        }
        return Result.Success<bool>(true);
    }

    public async Task<Result<List<TramiteDTO>>> GetAprobados()
    {
        var tramites = await _tramitesService.GetAprobados();
        var tramitesDto = _mapper.Map<List<TramiteDTO>>(tramites);
        return Result.Success(tramitesDto);
    }

    public async Task<Result<TramiteReportModel>> GetInfoReporteVacaciones(int tramiteId)
    {
        TramiteReportModel result = new();
        var tramite = await _tramitesService.GetTramiteById(tramiteId);
        var aprobaciones = await _aprobacionesService.GetAprobacionesByTramiteId(tramiteId);
        var solicitudVacaciones = await _solicitudesRepository.GetSolicitudVacacionesByTramiteId(tramiteId);
        var user = await _solicitudesRepository.GetUserByCi(tramite.UserCi);
        var deparmento = await _grupoFaProfitRepository.GetFullInfoByCiAsync(tramite.UserCi);
        if (tramite == null)
        {
            return Result.Fail<TramiteReportModel>($"No se encontró el trámite con ID {tramiteId}");
        }

        result.TramiteId = tramite.Id;
        result.TipoTramite = "Solicitud de vacaciones";
        result.EmpleadoCi = tramite.UserCi;
        result.EmpleadoNombre = user?.FirstName + " " + user?.LastName;
        result.EmpleadoCargo = deparmento.Value[0].Ocupacion;
        result.FechaIngreso = DateTime.Now;
        result.FechaSolicitud = tramite.FechaSolicitud;
        result.FechaResolucion = DateTime.Now;
        result.Vacaciones = new()
        {
            Desde = solicitudVacaciones.Desde,
            Hasta = solicitudVacaciones.Hasta,
            DiasTotales = solicitudVacaciones.DiasTotales,
            Observaciones = solicitudVacaciones.Observaciones
        };

        foreach (var aprobacion in aprobaciones)
        {
            result.Aprobaciones.Add(new AprobacionReportModel
            {
                AprobadorCi = aprobacion.AprobadorCi,
                AprobadorNombre = aprobacion.Nombre ?? "Sin datos",
                Accion = aprobacion.Estado.ToString(),
                Fecha = aprobacion.UpdatedAt
            });
        }

        return Result.Success(result);
    }

    public async Task<Result<TramiteReportModel>> GetInfoReporteDiaEspecial(int tramiteId)
    {
        TramiteReportModel result = new();
        var tramite = await _tramitesService.GetTramiteById(tramiteId);
        var aprobaciones = await _aprobacionesService.GetAprobacionesByTramiteId(tramiteId);
        var solicitudDiaEspecial = await _solicitudesRepository.GetSolicitudDiasEspecialesByTramiteId(tramiteId);
        var user = await _solicitudesRepository.GetUserByCi(tramite.UserCi);
        var empleadoInfo = await _snEmpleProfitRepository.GetFullInfoByCiAsync(tramite.UserCi);
        if (tramite == null)
        {
            return Result.Fail<TramiteReportModel>($"No se encontró el trámite con ID {tramiteId}");
        }

        result.TramiteId = tramite.Id;
        result.TipoTramite = "Solicitud de día especial";
        result.EmpleadoCi = tramite.UserCi;
        result.EmpleadoNombre = user?.FirstName + " " + user?.LastName;
        result.EmpleadoCargo = empleadoInfo.IsSuccess ? empleadoInfo.Value.DesCargo : null;
        result.EmpleadoDepartamento = empleadoInfo.IsSuccess ? empleadoInfo.Value.DesDepart : null;
        result.FechaSolicitud = tramite.FechaSolicitud;
        result.FechaResolucion = DateTime.Now;
        result.DiaEspecial = new()
        {
            Fecha = solicitudDiaEspecial.Fecha,
            Motivo = solicitudDiaEspecial.Motivo
        };

        foreach (var aprobacion in aprobaciones)
        {
            result.Aprobaciones.Add(new AprobacionReportModel
            {
                AprobadorCi = aprobacion.AprobadorCi,
                AprobadorNombre = aprobacion.Nombre ?? "Sin datos",
                Accion = aprobacion.Estado.ToString(),
                Fecha = aprobacion.UpdatedAt
            });
        }

        return Result.Success(result);
    }

    public async Task<Result<bool>> ArchivarTramite(int tramiteId)
    {
        var result = await _tramitesService.ArchivarTramite(tramiteId);
        return Result.Success(result);
    }

    public async Task<Result<bool>> RechazarTramite(int tramiteId, string razon)
    {
        var result = await _aprobacionesService.RechazarTramite(tramiteId, razon);
        return Result.Success(result);
    }
}