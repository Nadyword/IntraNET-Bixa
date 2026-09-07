using Bixa.Backend.DataAccess.Models;
using Bixa.Backend.Models.DTOs.FirmantesDTO;
using Bixa.Backend.Models.Response;

namespace Bixa.Backend.Services.Interfaces;

/// <summary>
/// Ajustes manuales sobre la cadena de firmantes que Profit deriva de la jerarquía de supervisores.
/// </summary>
public interface IAjusteFirmantesService
{
    /// <summary>
    /// Cadena de firmantes del empleado: la original de Profit y la efectiva con sus ajustes aplicados.
    /// </summary>
    Task<Result<AjusteFirmantesDTO>> GetAsync(string ci);

    /// <summary>
    /// Aplica los ajustes guardados del empleado sobre la cadena de firmantes recibida. Si no tiene
    /// ajustes devuelve la misma cadena, de modo que puede llamarse siempre sin condicionales.
    /// </summary>
    Task<List<AprobadorPermisoInfo>> AplicarAjusteAsync(string ci, List<AprobadorPermisoInfo> firmantesProfit);

    /// <summary>
    /// Guarda la cadena indicada: compara contra la de Profit y persiste solo las diferencias.
    /// Una cadena idéntica a la de Profit borra el ajuste.
    /// </summary>
    Task<Result<AjusteFirmantesDTO>> GuardarAsync(string ci, GuardarAjusteFirmantesDTO dto);

    /// <summary>Elimina el ajuste del empleado: vuelve a firmarse según la jerarquía de Profit.</summary>
    Task<Result<AjusteFirmantesDTO>> RestablecerAsync(string ci);

    /// <summary>Usuarios activos de la intranet que pueden añadirse como firmantes.</summary>
    Task<Result<List<CandidatoFirmanteDTO>>> GetCandidatosAsync(string? query, int take);

    /// <summary>CIs de los empleados que tienen un ajuste guardado.</summary>
    Task<Result<List<string>>> GetCisConAjusteAsync();
}
