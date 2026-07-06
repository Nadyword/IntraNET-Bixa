namespace Bixa.Backend.DataAccess.Interfaces.Repositories.Profit;

public interface IFechasFeriadasProfitRepository
{
    /// <summary>
    /// Obtiene las fechas marcadas como feriado dentro de un rango de fechas.
    /// </summary>
    /// <param name="desde">Fecha de inicio del rango (inclusive).</param>
    /// <param name="hasta">Fecha de fin del rango (inclusive).</param>
    /// <returns>Lista de fechas feriadas dentro del rango.</returns>
    Task<List<DateTime>> GetFechasFeriadasAsync(DateTime desde, DateTime hasta);
}
