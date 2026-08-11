using Bixa.Backend.DataAccess.Interfaces.Repositories.Profit;
using Bixa.Backend.DataAccess.Entities.DbProfit;
using Bixa.Backend.DataAccess.Templates.Profit;
using Bixa.Backend.DataAccess.Context;
using Bixa.Backend.Models.Response;
using Microsoft.EntityFrameworkCore;
using Bixa.Backend.Models.Enums;
using Microsoft.Data.SqlClient;

namespace Bixa.Backend.DataAccess.Repository.Profit;

public class ConsultaArcProfitRepository(ProfitDbContext context) : IConsultaArcProfitRepository
{
    private readonly ProfitDbContext _context = context ?? throw new ArgumentNullException(nameof(context));

    public async Task<Result<List<ConsultaArc>>> GetConsultaArcAsync(string ci, int anio)
    {
        try
        {
            var registros = await _context.ConsultaArc
                .FromSqlRaw(
                    ProfitSqlTemplates.GetConsultaARC,
                    new SqlParameter("@CiEmpleado", ci),
                    new SqlParameter("@anoActual", anio))
                .ToListAsync();

            if (registros.Count == 0)
            {
                return Result.Fail<List<ConsultaArc>>("Sin registros", ErrorTypeEnum.NotFound);
            }

            return Result.Success(registros.OrderBy(r => r.Mes).ToList());
        }
        catch (Exception ex)
        {
            return Result.Fail<List<ConsultaArc>>(ex.Message, ErrorTypeEnum.Conflict);
        }
    }
}
