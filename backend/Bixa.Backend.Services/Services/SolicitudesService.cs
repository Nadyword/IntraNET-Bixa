using Bixa.Backend.Models.DTOs.SolicitudesModelDTO;
using Bixa.Backend.DataAccess.Interfaces.Repositories;
using Bixa.Backend.DataAccess.Entities.Solicitudes;
using Bixa.Backend.DataAccess.Entities;
using Bixa.Backend.DataAccess.Models;
using Bixa.Backend.Services.Interfaces;
using Bixa.Backend.Models.Response;
using Bixa.Backend.Models.Enums;
using AutoMapper;

namespace Bixa.Backend.Services.Services;

public class SolicitudesService(ISolicitudesRepository solicitudesRepository, IMapper mapper, IUnitOfWork unitOfWork, ITramitesService tramitesService, IAprobacionesService aprobacionesService) : ISolicitudesService
{
    private readonly ISolicitudesRepository _solicitudesRepository = solicitudesRepository;
    private readonly ITramitesService _tramitesService = tramitesService;
    private readonly IAprobacionesService _aprobacionesService = aprobacionesService;
    private readonly IMapper _mapper = mapper;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

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

    public async Task<Result<List<TramiteDTO>>> GetAprobados()
    {
        var tramites = await _tramitesService.GetAprobados();
        var tramitesDto = _mapper.Map<List<TramiteDTO>>(tramites);
        return Result.Success(tramitesDto);
    }
}