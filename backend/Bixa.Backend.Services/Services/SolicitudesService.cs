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
using Bixa.Backend.Models.Utilities;
using Bixa.Backend.Services.Interfaces;

namespace Bixa.Backend.Services.Services;

public class SolicitudesService(ISolicitudesRepository solicitudesRepository, IMapper mapper, IUnitOfWork unitOfWork, ITramitesService tramitesService, IAprobacionesService aprobacionesService, IGrupoFaProfitRepository grupoFaProfitRepository, ISnEmpleProfitRepository snEmpleProfitRepository, IFechasFeriadasProfitRepository fechasFeriadasProfitRepository, IUtilidadesProfitRepository utilidadesProfitRepository, INotificationService notificationService, IAdjuntoService adjuntoService, ISendMailServices sendMailServices, IAjusteFirmantesService ajusteFirmantesService) : ISolicitudesService
{
    private const int CuotasMaximas = 52;

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
    private readonly IFechasFeriadasProfitRepository _fechasFeriadasProfitRepository = fechasFeriadasProfitRepository;
    private readonly IUtilidadesProfitRepository _utilidadesProfitRepository = utilidadesProfitRepository;
    private readonly ISolicitudesRepository _solicitudesRepository = solicitudesRepository;
    private readonly IAprobacionesService _aprobacionesService = aprobacionesService;
    private readonly ITramitesService _tramitesService = tramitesService;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;
    private readonly INotificationService _notificationService = notificationService;
    private readonly IAdjuntoService _adjuntoService = adjuntoService;
    private readonly ISendMailServices _sendMailServices = sendMailServices;
    private readonly IAjusteFirmantesService _ajusteFirmantesService = ajusteFirmantesService;

    /// <summary>
    /// Cadena de firmantes con la que se arman las aprobaciones de una solicitud: la jerarquía de
    /// supervisores de Profit, con los ajustes manuales del empleado aplicados encima si los tiene.
    /// </summary>
    public async Task<List<AprobadorPermisoInfo>> GetAprovadoresPermisosByCi(string ci)
    {
        var firmantesProfit = await _solicitudesRepository.GetAprovadoresPermisosByCi(ci);

        return await _ajusteFirmantesService.AplicarAjusteAsync(ci, firmantesProfit);
    }

    /// <summary>
    /// Guarda el trámite junto con su cadena de firmas pendientes. Si el empleado no tiene firmantes
    /// (sin supervisor en Profit ni ajustes manuales) no hay a quién pedir la firma, así que el trámite
    /// nace Aprobado y pasa directo al administrador para su tramitación y archivo.
    /// Debe llamarse dentro de la transacción del llamador, que es quien hace el rollback si falla.
    /// </summary>
    private async Task<Result<int>> RegistrarTramiteConAprobacionesAsync(Tramite tramite)
    {
        List<AprobadorPermisoInfo> aprobadores = await GetAprovadoresPermisosByCi(tramite.UserCi);

        if (aprobadores.Count == 0)
        {
            tramite.Estado = EstadoTramiteEnum.Aprobado;
        }

        int tramiteId = await _solicitudesRepository.AddNewTramite(tramite);
        if (tramiteId < 1)
        {
            return Result.Fail<int>("Error al guardar el trámite");
        }

        for (int i = 0; i < aprobadores.Count; i++)
        {
            AprobadorPermisoInfo aprobador = aprobadores[i];
            int result = await _solicitudesRepository.AddAprobaciones(new Aprobacion
            {
                TramiteId = tramiteId,
                Nombre = aprobador.Nombre,
                AprobadorCi = aprobador.Ci,
                Orden = i + 1,
                Estado = EstadoAprobacionEnum.Pendiente
            });

            if (result < 1)
            {
                return Result.Fail<int>($"Error al guardar la aprobación para el aprobador {aprobador.Nombre} ({aprobador.Ci})");
            }
        }

        return Result.Success(tramiteId);
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
            var registro = await RegistrarTramiteConAprobacionesAsync(tramite);
            if (!registro.IsSuccess)
            {
                await _unitOfWork.RollbackTransactionAsync();
                return Result.Fail<bool>(registro.Error);
            }

            int tramiteId = registro.Value;

            SolicitudVacaciones SVacacion = _mapper.Map<SolicitudVacaciones>(solicitud);
            SVacacion.TramiteId = tramiteId;

            bool solicitudResult = await _solicitudesRepository.AddSolicitudVacaciones(SVacacion);
            if (!solicitudResult)
            {
                await _unitOfWork.RollbackTransactionAsync();
                return Result.Fail<bool>("Error al guardar la solicitud de vacaciones");
            }

            await _unitOfWork.CommitTransactionAsync();
            await NotificarTramiteCreadoAsync(tramiteId);
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
            var registro = await RegistrarTramiteConAprobacionesAsync(tramite);
            if (!registro.IsSuccess)
            {
                await _unitOfWork.RollbackTransactionAsync();
                return Result.Fail<bool>(registro.Error);
            }

            int tramiteId = registro.Value;

            SolicitudDiasEspeciales sDiaEspecial = _mapper.Map<SolicitudDiasEspeciales>(solicitud);
            sDiaEspecial.TramiteId = tramiteId;

            bool solicitudResult = await _solicitudesRepository.AddSolicitudDiasEspeciales(sDiaEspecial);
            if (!solicitudResult)
            {
                await _unitOfWork.RollbackTransactionAsync();
                return Result.Fail<bool>("Error al guardar la solicitud de día especial");
            }

            await _unitOfWork.CommitTransactionAsync();
            await NotificarTramiteCreadoAsync(tramiteId);
            return Result.Success(true);
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            return Result.Fail<bool>($"Error en el proceso: {ex.Message}");
        }
    }

    public async Task<Result<bool>> AddSolicitudUtilidades(SolicUtilidadesDTO solicitud)
    {
        if (solicitud.Monto <= 0)
        {
            return Result.Fail<bool>("Monto inválido: ingresa un monto mayor a cero.");
        }

        solicitud.Ci = UtilityService.NormalizeCiFormat(solicitud.Ci);

        var montoDisponibleResult = await _utilidadesProfitRepository.GetMontoDisponibleByCiAsync(solicitud.Ci);
        var montoDisponible = montoDisponibleResult.IsSuccess ? (montoDisponibleResult.Value ?? 0) : 0;

        if (solicitud.Monto > montoDisponible)
        {
            return Result.Fail<bool>($"Monto inválido: el monto solicitado no puede superar {montoDisponible:N2}.");
        }

        await _unitOfWork.BeginTransactionAsync();
        var tramite = _mapper.Map<Tramite>(solicitud);

        try
        {
            var registro = await RegistrarTramiteConAprobacionesAsync(tramite);
            if (!registro.IsSuccess)
            {
                await _unitOfWork.RollbackTransactionAsync();
                return Result.Fail<bool>(registro.Error);
            }

            int tramiteId = registro.Value;

            SolicitudUtilidades sUtilidades = _mapper.Map<SolicitudUtilidades>(solicitud);
            sUtilidades.TramiteId = tramiteId;

            bool solicitudResult = await _solicitudesRepository.AddSolicitudUtilidades(sUtilidades);
            if (!solicitudResult)
            {
                await _unitOfWork.RollbackTransactionAsync();
                return Result.Fail<bool>("Error al guardar la solicitud de anticipo de utilidades");
            }

            await _unitOfWork.CommitTransactionAsync();
            await NotificarTramiteCreadoAsync(tramiteId);
            return Result.Success(true);
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            return Result.Fail<bool>($"Error en el proceso: {ex.Message}");
        }
    }

    public async Task<Result<bool>> AddSolicitudPrestaciones(SolicPrestacionesDTO solicitud)
    {
        if (solicitud.TipoTramiteId != (int)TipoTramiteEnum.Sociales && solicitud.TipoTramiteId != (int)TipoTramiteEnum.Prestaciones)
        {
            return Result.Fail<bool>("Tipo de solicitud inválido: selecciona préstamo o anticipo de prestaciones sociales.");
        }

        if (solicitud.Monto <= 0)
        {
            return Result.Fail<bool>("Monto inválido: ingresa un monto mayor a cero.");
        }

        if (!Enum.IsDefined(solicitud.Destino))
        {
            return Result.Fail<bool>("Destino inválido: selecciona una de las opciones permitidas.");
        }

        bool esPrestamo = solicitud.TipoTramiteId == (int)TipoTramiteEnum.Prestaciones;

        if (esPrestamo && (solicitud.Cuotas is null || solicitud.Cuotas <= 0 || solicitud.Cuotas > CuotasMaximas))
        {
            return Result.Fail<bool>($"Cantidad de cuotas inválida: debe estar entre 1 y {CuotasMaximas}.");
        }

        if (solicitud.Archivo == null)
        {
            return Result.Fail<bool>("Debes adjuntar un archivo de soporte para la solicitud.");
        }

        var archivoResult = await _adjuntoService.ValidateAndSaveAsync(solicitud.Archivo, solicitud.Ci);
        if (!archivoResult.IsSuccess)
        {
            return Result.Fail<bool>(archivoResult.Error);
        }
        string? archivoAdjuntoGuardado = archivoResult.Value;

        solicitud.Ci = UtilityService.NormalizeCiFormat(solicitud.Ci);
        await _unitOfWork.BeginTransactionAsync();
        var tramite = _mapper.Map<Tramite>(solicitud);

        try
        {
            var registro = await RegistrarTramiteConAprobacionesAsync(tramite);
            if (!registro.IsSuccess)
            {
                await _unitOfWork.RollbackTransactionAsync();
                return Result.Fail<bool>(registro.Error);
            }

            int tramiteId = registro.Value;

            SolicitudPrestaciones sPrestaciones = _mapper.Map<SolicitudPrestaciones>(solicitud);
            sPrestaciones.TramiteId = tramiteId;
            sPrestaciones.Cuotas = esPrestamo ? solicitud.Cuotas : null;
            sPrestaciones.ArchivoAdjunto = archivoAdjuntoGuardado;

            bool solicitudResult = await _solicitudesRepository.AddSolicitudPrestaciones(sPrestaciones);
            if (!solicitudResult)
            {
                await _unitOfWork.RollbackTransactionAsync();
                return Result.Fail<bool>("Error al guardar la solicitud de prestaciones sociales");
            }

            await _unitOfWork.CommitTransactionAsync();
            await NotificarTramiteCreadoAsync(tramiteId);
            return Result.Success(true);
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            return Result.Fail<bool>($"Error en el proceso: {ex.Message}");
        }
    }

    public async Task<Result<bool>> AddSolicitudConstanciaTrabajo(SolicConstanciaTrabajoDTO solicitud)
    {
        if (solicitud.DirigidoAEspecifico && string.IsNullOrWhiteSpace(solicitud.DirigidoA))
        {
            return Result.Fail<bool>("Debes especificar a quién va dirigida la constancia.");
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

            SolicitudConstanciaTrabajo sConstanciaTrabajo = _mapper.Map<SolicitudConstanciaTrabajo>(solicitud);
            sConstanciaTrabajo.TramiteId = tramiteId;

            bool solicitudResult = await _solicitudesRepository.AddSolicitudConstanciaTrabajo(sConstanciaTrabajo);
            if (!solicitudResult)
            {
                await _unitOfWork.RollbackTransactionAsync();
                return Result.Fail<bool>("Error al guardar la solicitud de constancia de trabajo");
            }

            await _unitOfWork.CommitTransactionAsync();
            await NotificarAdministradoresAsync();
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

    public async Task<Result<List<HistorialAprobacionDTO>>> GetHistorialAprobacionesByCi(string ci)
    {
        ci = UtilityService.NormalizeCiFormat(ci);
        var historial = await _aprobacionesService.GetHistorialAprobaciones(ci);
        return Result.Success(historial);
    }

    public async Task<Result<List<HistorialAprobacionDTO>>> GetHistorialAprobaciones()
    {
        var historial = await _aprobacionesService.GetHistorialAprobaciones(null);
        return Result.Success(historial);
    }

    public async Task<Result<bool>> AprobarTramite(AprobarTramiteDTO aprobarTramiteDTO)
    {
        if (aprobarTramiteDTO.Estado == (int)EstadoAprobacionEnum.Rechazado)
        {
            var tramite = await _tramitesService.GetTramiteById(aprobarTramiteDTO.TramiteId);
            if (tramite.TipoTramiteId == (int)TipoTramiteEnum.ConstanciaTrabajo)
            {
                return Result.Fail<bool>("No se puede rechazar una solicitud de Constancia de Trabajo.");
            }
        }

        var result = await _aprobacionesService.AprobarTramite(aprobarTramiteDTO);
        if (!result)
        {
            return Result.Fail<bool>("No se encontró una firma pendiente tuya para este trámite. Es posible que ya lo hayas respondido.");
        }

        await NotifyCambioEstadoAsync(aprobarTramiteDTO.TramiteId);
        await NotificarSiguientePasoAsync(aprobarTramiteDTO.TramiteId);
        return Result.Success(result);
    }

    public async Task<Result<bool>> AprobarTramite(int tramiteId)
    {
        var result = await _aprobacionesService.AprobarTramite(tramiteId);
        if (!result)
        {
            return Result.Fail<bool>($"Error al aprobar el trámite con ID {tramiteId}");
        }
        await NotifyCambioEstadoAsync(tramiteId);
        return Result.Success<bool>(true);
    }

    public async Task<Result<List<TramiteDTO>>> GetAprobados()
    {
        var tramites = await _tramitesService.GetAprobados();
        var tramitesDto = _mapper.Map<List<TramiteDTO>>(tramites);
        return Result.Success(tramitesDto);
    }

    public async Task<Result<TramiteDTO>> GetTramiteDetalle(int tramiteId)
    {
        var tramite = await _tramitesService.GetTramiteDetalladoById(tramiteId);
        if (tramite == null)
        {
            return Result.Fail<TramiteDTO>($"No se encontró el trámite con ID {tramiteId}");
        }

        var tramiteDto = _mapper.Map<TramiteDTO>(tramite);
        return Result.Success(tramiteDto);
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
        result.FechaSolicitud = tramite.CreatedAt;
        result.FechaResolucion = DateTime.Now;
        result.EmpleadoUrlFirma = user?.UrlFirma;
        result.Vacaciones = new()
        {
            Desde = solicitudVacaciones.Desde,
            Hasta = solicitudVacaciones.Hasta,
            DiasTotales = solicitudVacaciones.DiasTotales,
            Observaciones = solicitudVacaciones.Observaciones
        };

        // Solo las firmas efectivamente otorgadas: las rechazadas y anuladas se conservan para el
        // historial, pero no representan una firma sobre el documento.
        foreach (var aprobacion in aprobaciones.Where(a => a.Estado == EstadoAprobacionEnum.Aprobado))
        {
            result.Aprobaciones.Add(new AprobacionReportModel
            {
                AprobadorCi = aprobacion.AprobadorCi,
                AprobadorNombre = aprobacion.Nombre ?? "Sin datos",
                Accion = aprobacion.Estado.ToString(),
                Fecha = aprobacion.FechaRespuesta ?? aprobacion.UpdatedAt,
                UrlFirma = aprobacion.Aprobador?.UrlFirma
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
        result.FechaSolicitud = tramite.CreatedAt;
        result.FechaResolucion = DateTime.Now;
        result.EmpleadoUrlFirma = user?.UrlFirma;
        result.DiaEspecial = new()
        {
            Fecha = solicitudDiaEspecial.Fecha,
            Motivo = solicitudDiaEspecial.Motivo
        };

        // Solo las firmas efectivamente otorgadas: las rechazadas y anuladas se conservan para el
        // historial, pero no representan una firma sobre el documento.
        foreach (var aprobacion in aprobaciones.Where(a => a.Estado == EstadoAprobacionEnum.Aprobado))
        {
            result.Aprobaciones.Add(new AprobacionReportModel
            {
                AprobadorCi = aprobacion.AprobadorCi,
                AprobadorNombre = aprobacion.Nombre ?? "Sin datos",
                Accion = aprobacion.Estado.ToString(),
                Fecha = aprobacion.FechaRespuesta ?? aprobacion.UpdatedAt,
                UrlFirma = aprobacion.Aprobador?.UrlFirma
            });
        }

        return Result.Success(result);
    }

    public async Task<Result<TramiteReportModel>> GetInfoReporteUtilidades(int tramiteId)
    {
        TramiteReportModel result = new();
        var tramite = await _tramitesService.GetTramiteById(tramiteId);
        var aprobaciones = await _aprobacionesService.GetAprobacionesByTramiteId(tramiteId);
        var solicitudUtilidades = await _solicitudesRepository.GetSolicitudUtilidadesByTramiteId(tramiteId);
        var user = await _solicitudesRepository.GetUserByCi(tramite.UserCi);
        var empleadoInfo = await _snEmpleProfitRepository.GetFullInfoByCiAsync(tramite.UserCi);
        var montoDisponibleResult = await _utilidadesProfitRepository.GetMontoDisponibleByCiAsync(tramite.UserCi);
        if (tramite == null)
        {
            return Result.Fail<TramiteReportModel>($"No se encontró el trámite con ID {tramiteId}");
        }

        result.TramiteId = tramite.Id;
        result.TipoTramite = "Solicitud de anticipo de utilidades";
        result.EmpleadoCi = tramite.UserCi;
        result.EmpleadoNombre = user?.FirstName + " " + user?.LastName;
        result.EmpleadoCargo = empleadoInfo.IsSuccess ? empleadoInfo.Value.DesCargo : null;
        result.EmpleadoDepartamento = empleadoInfo.IsSuccess ? empleadoInfo.Value.DesDepart : null;
        result.FechaIngreso = empleadoInfo.IsSuccess ? empleadoInfo.Value.FechaIng : null;
        result.FechaSolicitud = tramite.CreatedAt;
        result.FechaResolucion = DateTime.Now;
        result.EmpleadoUrlFirma = user?.UrlFirma;
        result.Utilidades = new()
        {
            Monto = solicitudUtilidades.Monto,
            Motivo = solicitudUtilidades.Motivo,
            TotalUtilidadesDisponible = montoDisponibleResult.IsSuccess ? (montoDisponibleResult.Value ?? 0) : 0
        };

        // Solo las firmas efectivamente otorgadas: las rechazadas y anuladas se conservan para el
        // historial, pero no representan una firma sobre el documento.
        foreach (var aprobacion in aprobaciones.Where(a => a.Estado == EstadoAprobacionEnum.Aprobado))
        {
            result.Aprobaciones.Add(new AprobacionReportModel
            {
                AprobadorCi = aprobacion.AprobadorCi,
                AprobadorNombre = aprobacion.Nombre ?? "Sin datos",
                Accion = aprobacion.Estado.ToString(),
                Fecha = aprobacion.FechaRespuesta ?? aprobacion.UpdatedAt,
                UrlFirma = aprobacion.Aprobador?.UrlFirma
            });
        }

        return Result.Success(result);
    }

    public async Task<Result<TramiteReportModel>> GetInfoReportePrestaciones(int tramiteId)
    {
        TramiteReportModel result = new();
        var tramite = await _tramitesService.GetTramiteById(tramiteId);
        var aprobaciones = await _aprobacionesService.GetAprobacionesByTramiteId(tramiteId);
        var solicitudPrestaciones = await _solicitudesRepository.GetSolicitudPrestacionesByTramiteId(tramiteId);
        var user = await _solicitudesRepository.GetUserByCi(tramite.UserCi);
        var empleadoInfo = await _snEmpleProfitRepository.GetFullInfoByCiAsync(tramite.UserCi);
        if (tramite == null)
        {
            return Result.Fail<TramiteReportModel>($"No se encontró el trámite con ID {tramiteId}");
        }

        result.TramiteId = tramite.Id;
        result.TipoTramite = solicitudPrestaciones.EsPrestamo
            ? "Solicitud de préstamo sobre prestaciones sociales"
            : "Solicitud de anticipo de prestaciones sociales";
        result.EmpleadoCi = tramite.UserCi;
        result.EmpleadoNombre = user?.FirstName + " " + user?.LastName;
        result.EmpleadoCargo = empleadoInfo.IsSuccess ? empleadoInfo.Value.DesCargo : null;
        result.EmpleadoDepartamento = empleadoInfo.IsSuccess ? empleadoInfo.Value.DesDepart : null;
        result.FechaIngreso = empleadoInfo.IsSuccess ? empleadoInfo.Value.FechaIng : null;
        result.FechaSolicitud = tramite.CreatedAt;
        result.FechaResolucion = DateTime.Now;
        result.EmpleadoUrlFirma = user?.UrlFirma;
        result.Prestaciones = new()
        {
            EsPrestamo = solicitudPrestaciones.EsPrestamo,
            Monto = solicitudPrestaciones.Monto,
            Destino = solicitudPrestaciones.Destino.GetDescription(),
            Observaciones = solicitudPrestaciones.Observaciones,
            Cuotas = solicitudPrestaciones.Cuotas,
            MontoCuota = solicitudPrestaciones.Cuotas is > 0
                ? solicitudPrestaciones.Monto / solicitudPrestaciones.Cuotas.Value
                : null
        };

        // Solo las firmas efectivamente otorgadas: las rechazadas y anuladas se conservan para el
        // historial, pero no representan una firma sobre el documento.
        foreach (var aprobacion in aprobaciones.Where(a => a.Estado == EstadoAprobacionEnum.Aprobado))
        {
            result.Aprobaciones.Add(new AprobacionReportModel
            {
                AprobadorCi = aprobacion.AprobadorCi,
                AprobadorNombre = aprobacion.Nombre ?? "Sin datos",
                Accion = aprobacion.Estado.ToString(),
                Fecha = aprobacion.FechaRespuesta ?? aprobacion.UpdatedAt,
                UrlFirma = aprobacion.Aprobador?.UrlFirma
            });
        }

        return Result.Success(result);
    }

    public async Task<Result<bool>> ArchivarTramite(int tramiteId)
    {
        var result = await _tramitesService.ArchivarTramite(tramiteId);
        if (result)
        {
            await NotifyCambioEstadoAsync(tramiteId);
        }
        return Result.Success(result);
    }

    public async Task<Result<bool>> RechazarTramite(int tramiteId, string razon)
    {
        var tramite = await _tramitesService.GetTramiteById(tramiteId);
        if (tramite.TipoTramiteId == (int)TipoTramiteEnum.ConstanciaTrabajo)
        {
            return Result.Fail<bool>("No se puede rechazar una solicitud de Constancia de Trabajo.");
        }

        if (tramite.Estado is EstadoTramiteEnum.Tramitando or EstadoTramiteEnum.Rechazado)
        {
            return Result.Fail<bool>("El trámite ya fue finalizado y no se puede rechazar.");
        }

        var result = await _aprobacionesService.RechazarTramite(tramiteId, razon);
        if (result)
        {
            await NotifyCambioEstadoAsync(tramiteId);
        }
        return Result.Success(result);
    }

    public async Task<Result<int>> GetDiasHabiles(DateTime desde, DateTime hasta)
    {
        if (hasta.Date < desde.Date)
        {
            return Result.Fail<int>("La fecha de fin debe ser mayor o igual a la fecha de inicio.");
        }

        var feriados = (await _fechasFeriadasProfitRepository.GetFechasFeriadasAsync(desde, hasta)).ToHashSet();

        int dias = 0;
        for (var fecha = desde.Date; fecha <= hasta.Date; fecha = fecha.AddDays(1))
        {
            bool esFinDeSemana = fecha.DayOfWeek == DayOfWeek.Saturday || fecha.DayOfWeek == DayOfWeek.Sunday;
            if (!esFinDeSemana && !feriados.Contains(fecha))
            {
                dias++;
            }
        }

        return Result.Success(dias);
    }

    private async Task NotifyCambioEstadoAsync(int tramiteId)
    {
        try
        {
            var tramite = await _tramitesService.GetTramiteById(tramiteId);

            var (title, message) = tramite.Estado switch
            {
                EstadoTramiteEnum.Revision => ("Trámite en revisión", $"Tu trámite #{tramiteId} fue firmado por un aprobador y avanzó a revisión."),
                EstadoTramiteEnum.Firmado => ("Trámite firmado", $"Tu trámite #{tramiteId} fue firmado por todos los aprobadores y espera aprobación final."),
                EstadoTramiteEnum.Aprobado => ("Trámite aprobado", $"Tu trámite #{tramiteId} fue aprobado."),
                EstadoTramiteEnum.Rechazado => ("Trámite rechazado", $"Tu trámite #{tramiteId} fue rechazado." + (string.IsNullOrWhiteSpace(tramite.MotivoRechazo) ? "" : $" Motivo: {tramite.MotivoRechazo}")),
                EstadoTramiteEnum.Tramitando => ("Trámite finalizado", $"Tu trámite #{tramiteId} fue archivado."),
                _ => (null, null),
            };

            if (title != null)
                await _notificationService.NotifyAsync(tramite.UserCi, "TramiteEstado", title, message!, "Tramite", tramiteId);
        }
        catch
        {
            // Notificación es un efecto secundario: nunca debe tumbar la operación principal.
        }
    }

    /// <summary>
    /// Avisa por correo al firmante que tiene el turno del trámite. El turno se resuelve sobre las
    /// aprobaciones ya guardadas, que son la cadena del empleado con sus ajustes aplicados (firmantes
    /// agregados, excluidos o reubicados): mirar la jerarquía de Profit o dar por hecho que el turno
    /// es el primer paso le mandaría el correo a quien no firma.
    /// </summary>
    private async Task NotificarFirmanteDeTurnoAsync(int tramiteId)
    {
        try
        {
            var tramite = await _tramitesService.GetTramiteById(tramiteId);
            var aprobaciones = await _aprobacionesService.GetAprobacionesByTramiteId(tramiteId);

            var turno = FirmanteDeTurno(aprobaciones, tramite.OrdenActual);
            if (turno != null)
                await EnviarCorreoAprobadorAsync(turno.AprobadorCi);
        }
        catch
        {
            // Correo es un efecto secundario: nunca debe tumbar la operación principal.
        }
    }

    /// <summary>
    /// Aviso al crear un trámite: si nació Aprobado (empleado sin firmantes) se le informa al empleado y
    /// se avisa a los administradores para que lo tramiten; si no, se le escribe al primer firmante.
    /// </summary>
    private async Task NotificarTramiteCreadoAsync(int tramiteId)
    {
        try
        {
            var tramite = await _tramitesService.GetTramiteById(tramiteId);

            if (tramite.Estado == EstadoTramiteEnum.Aprobado)
            {
                await NotifyCambioEstadoAsync(tramiteId);
                await NotificarAdministradoresAsync();
            }
            else
            {
                await NotificarFirmanteDeTurnoAsync(tramiteId);
            }
        }
        catch
        {
            // Correo es un efecto secundario: nunca debe tumbar la operación principal.
        }
    }

    /// <summary>
    /// Firma a la que le toca: la pendiente de menor orden desde el turno del trámite. Recién creado
    /// el trámite el turno es 1, y cada firma lo adelanta al orden siguiente.
    /// </summary>
    private static Aprobacion? FirmanteDeTurno(List<Aprobacion> aprobaciones, int ordenActual) =>
        aprobaciones
            .Where(a => a.Estado == EstadoAprobacionEnum.Pendiente && a.Orden >= ordenActual)
            .OrderBy(a => a.Orden)
            .FirstOrDefault();

    private async Task NotificarSiguientePasoAsync(int tramiteId)
    {
        try
        {
            var tramite = await _tramitesService.GetTramiteById(tramiteId);

            if (tramite.Estado == EstadoTramiteEnum.Firmado)
            {
                await NotificarAdministradoresAsync();
            }
            else if (tramite.Estado == EstadoTramiteEnum.Revision)
            {
                await NotificarFirmanteDeTurnoAsync(tramiteId);
            }
        }
        catch
        {
            // Correo es un efecto secundario: nunca debe tumbar la operación principal.
        }
    }

    private async Task EnviarCorreoAprobadorAsync(string ci)
    {
        try
        {
            var correo = await _snEmpleProfitRepository.GetEmailByCiAsync(ci);
            if (!string.IsNullOrWhiteSpace(correo))
            {
                await _sendMailServices.SendMailSolicitudPendienteAprobacion(correo);
            }
        }
        catch
        {
            // Correo es un efecto secundario: nunca debe tumbar la operación principal.
        }
    }

    private async Task NotificarAdministradoresAsync()
    {
        try
        {
            var administradores = await _unitOfWork.Users.GetActiveByRoleAsync((int)UserRolEnum.Administrador);
            foreach (var admin in administradores)
            {
                var correo = await _snEmpleProfitRepository.GetEmailByCiAsync(admin.Ci);
                if (!string.IsNullOrWhiteSpace(correo))
                {
                    await _sendMailServices.SendMailSolicitudFirmadaCompleta(correo);
                }
            }
        }
        catch
        {
            // Correo es un efecto secundario: nunca debe tumbar la operación principal.
        }
    }
}