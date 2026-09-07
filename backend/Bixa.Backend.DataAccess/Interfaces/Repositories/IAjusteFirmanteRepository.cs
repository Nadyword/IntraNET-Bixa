using Bixa.Backend.DataAccess.Entities;

namespace Bixa.Backend.DataAccess.Interfaces.Repositories;

/// <summary>
/// Acceso a las diferencias guardadas entre la cadena de firmantes de Profit y la que debe
/// aplicarse a las solicitudes de un empleado.
/// </summary>
public interface IAjusteFirmanteRepository
{
    /// <summary>Diferencias guardadas para el empleado indicado; lista vacía si no tiene ajuste.</summary>
    Task<List<AjusteFirmante>> GetByUserCiAsync(string ci);

    /// <summary>
    /// Sustituye por completo las diferencias del empleado por las indicadas. Guardar una lista
    /// vacía equivale a restablecer la cadena de Profit.
    /// No llama a SaveChangesAsync; el llamador es responsable de persistir los cambios.
    /// </summary>
    Task ReplaceAsync(string ci, IEnumerable<AjusteFirmante> ajustes);

    /// <summary>CIs de los empleados que tienen al menos una diferencia guardada.</summary>
    Task<List<string>> GetCisConAjusteAsync();
}
