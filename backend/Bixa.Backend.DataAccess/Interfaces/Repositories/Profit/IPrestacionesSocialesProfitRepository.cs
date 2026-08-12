using Bixa.Backend.Models.Response;

namespace Bixa.Backend.DataAccess.Interfaces.Repositories.Profit;

public interface IPrestacionesSocialesProfitRepository
{
    Task<Result<decimal?>> GetMontoDisponibleByCiAsync(string ci);
}
