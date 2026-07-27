using Bixa.Backend.Models.Response;

namespace Bixa.Backend.DataAccess.Interfaces.Repositories.Profit;

public interface IUtilidadesProfitRepository
{
    Task<Result<decimal?>> GetMontoDisponibleByCiAsync(string ci);
}
