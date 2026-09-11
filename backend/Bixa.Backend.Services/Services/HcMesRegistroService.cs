using Bixa.Backend.DataAccess.Interfaces.Repositories;
using Bixa.Backend.DataAccess.Wrappers;
using Bixa.Backend.Models.DTOs.HcDTO;
using Bixa.Backend.Models.Enums;
using Bixa.Backend.Models.Response;
using Bixa.Backend.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace Bixa.Backend.Services.Services;

public class HcMesRegistroService(
    IHcMesRegistroRepository hcMesRegistroRepository,
    IUnitOfWork unitOfWork,
    IReadOnlyUnitOfWork readOnlyUnitOfWork,
    ISendMailServices sendMailServices,
    LoggerWrapper loggerWrapper) : IHcMesRegistroService
{
    private readonly IHcMesRegistroRepository _hcMesRegistroRepository = hcMesRegistroRepository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IReadOnlyUnitOfWork _readOnlyUnitOfWork = readOnlyUnitOfWork;
    private readonly ISendMailServices _sendMailServices = sendMailServices;
    private readonly ILogger<HcMesRegistroService> _logger = loggerWrapper.CreateLogger<HcMesRegistroService>();

    private const decimal CoberturaUno = 10000.00m;

    public async Task<Result<HcMesRegistroDTO>> GetByCiAsync(string ci)
    {
        var normalizedCi = UtilityService.NormalizeCiFormat(ci);
        var registro = await _hcMesRegistroRepository.GetByCiAsync(normalizedCi);

        return Result.Success(new HcMesRegistroDTO
        {
            Ci = normalizedCi,
            Mes1 = registro?.Mes1 ?? 0,
            Mes2 = registro?.Mes2 ?? 0,
            Mes3 = registro?.Mes3 ?? 0,
            PrimaTrimBs = registro?.PrimaTrimBs ?? 0,
            UpdatedAt = registro?.UpdatedAt ?? default,
            ModifiedByCi = registro?.ModifiedByCi,
        });
    }

    public async Task<Result<HcMesRegistroDTO>> SaveAsync(string ci, HcMesRegistroSaveDTO dto)
    {
        var normalizedCi = UtilityService.NormalizeCiFormat(ci);

        var primaResult = await _readOnlyUnitOfWork.ConsultaHc.GetConsultaHcAsync(normalizedCi, CoberturaUno);
        if (!primaResult.IsSuccess)
            return Result.Fail<HcMesRegistroDTO>("No se encontró información de HC (cobertura 1) para este empleado.", ErrorTypeEnum.NotFound);

        var primaTrimBs = primaResult.Value.PrimaTrimBs ?? 0;
        var suma = dto.Mes1 + dto.Mes2 + dto.Mes3;

        if (decimal.Round(suma, 2) != decimal.Round(primaTrimBs, 2))
        {
            return Result.Fail<HcMesRegistroDTO>(
                $"La suma de Mes 1 + Mes 2 + Mes 3 ({suma:N2}) debe ser igual a la Prima trim en Bs. Factura ({primaTrimBs:N2}).",
                ErrorTypeEnum.Validation);
        }

        try
        {
            var esAutoservicio = UtilityService.NormalizeCiFormat(_unitOfWork.GetCurrentUserCi() ?? string.Empty) == normalizedCi;

            var registro = await _hcMesRegistroRepository.UpsertAsync(normalizedCi, dto.Mes1, dto.Mes2, dto.Mes3, primaTrimBs);
            await _unitOfWork.SaveChangesAsync();

            if (esAutoservicio)
            {
                await NotificarAdministradoresAsync(primaResult.Value.NombreCompleto ?? normalizedCi, normalizedCi);
            }

            return Result.Success(new HcMesRegistroDTO
            {
                Ci = normalizedCi,
                NombreCompleto = primaResult.Value.NombreCompleto,
                Mes1 = registro.Mes1,
                Mes2 = registro.Mes2,
                Mes3 = registro.Mes3,
                PrimaTrimBs = registro.PrimaTrimBs,
                UpdatedAt = registro.UpdatedAt,
                ModifiedByCi = registro.ModifiedByCi,
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al guardar el registro de HC. Ci:{Ci}", normalizedCi);
            return Result.Fail<HcMesRegistroDTO>("Ocurrió un error al guardar el registro de HC.", ErrorTypeEnum.Database);
        }
    }

    /// <summary>
    /// Notifica por correo a todos los administradores activos que un empleado actualizó su registro de HC.
    /// El correo es un efecto secundario: cualquier error aquí nunca debe tumbar la operación principal de guardado.
    /// </summary>
    private async Task NotificarAdministradoresAsync(string empleadoNombre, string empleadoCi)
    {
        try
        {
            var administradores = await _unitOfWork.Users.GetActiveByRoleAsync((int)UserRolEnum.Administrador);
            foreach (var admin in administradores)
            {
                var correo = await _readOnlyUnitOfWork.SnEmple.GetEmailByCiAsync(admin.Ci);
                if (!string.IsNullOrWhiteSpace(correo))
                {
                    await _sendMailServices.SendMailHcActualizado(correo, empleadoNombre, empleadoCi);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "No se pudieron notificar los administradores del cambio de HC. Ci:{Ci}", empleadoCi);
        }
    }

    public async Task<Result<List<HcMesRegistroDTO>>> GetRecientesAsync(int take)
    {
        var registros = await _hcMesRegistroRepository.GetRecientesAsync(take);
        return Result.Success(registros.Select(MapToDTO).ToList());
    }

    public async Task<Result<List<HcMesRegistroDTO>>> BuscarAsync(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return Result.Success(new List<HcMesRegistroDTO>());

        var registros = await _hcMesRegistroRepository.BuscarAsync(query);
        return Result.Success(registros.Select(MapToDTO).ToList());
    }

    private static HcMesRegistroDTO MapToDTO(DataAccess.Entities.HcMesRegistro registro) => new()
    {
        Ci = registro.UserCi,
        NombreCompleto = registro.User != null ? $"{registro.User.FirstName} {registro.User.LastName}".Trim() : null,
        Mes1 = registro.Mes1,
        Mes2 = registro.Mes2,
        Mes3 = registro.Mes3,
        PrimaTrimBs = registro.PrimaTrimBs,
        UpdatedAt = registro.UpdatedAt,
        ModifiedByCi = registro.ModifiedByCi,
    };
}
