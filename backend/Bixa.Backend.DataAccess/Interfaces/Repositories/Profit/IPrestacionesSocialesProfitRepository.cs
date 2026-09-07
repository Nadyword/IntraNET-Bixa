using Bixa.Backend.DataAccess.Entities.DbProfit;
using Bixa.Backend.Models.Response;

namespace Bixa.Backend.DataAccess.Interfaces.Repositories.Profit;

public interface IPrestacionesSocialesProfitRepository
{
    Task<Result<PrestacionesSociales>> GetPrestacionesSocialesByCiAsync(string ci);
}
