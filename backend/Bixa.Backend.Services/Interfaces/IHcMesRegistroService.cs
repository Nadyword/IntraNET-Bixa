using Bixa.Backend.Models.DTOs.HcDTO;
using Bixa.Backend.Models.Response;

namespace Bixa.Backend.Services.Interfaces;

public interface IHcMesRegistroService
{
    /// <summary>
    /// Obtiene los valores de Mes1/Mes2/Mes3 previamente guardados para el CI indicado (0 si aún no se ha guardado nada).
    /// </summary>
    Task<Result<HcMesRegistroDTO>> GetByCiAsync(string ci);

    /// <summary>
    /// Guarda (crea o sobrescribe) los valores de Mes1/Mes2/Mes3 para el CI indicado, validando contra
    /// la Prima trim en Bs. Factura vigente (consultada en tiempo real en la BD de Profit).
    /// </summary>
    Task<Result<HcMesRegistroDTO>> SaveAsync(string ci, HcMesRegistroSaveDTO dto);

    /// <summary>
    /// Obtiene los últimos registros editados, ordenados por fecha de actualización descendente.
    /// </summary>
    Task<Result<List<HcMesRegistroDTO>>> GetRecientesAsync(int take);

    /// <summary>
    /// Busca en TODOS los registros por nombre, apellido o CI.
    /// </summary>
    Task<Result<List<HcMesRegistroDTO>>> BuscarAsync(string query);
}
