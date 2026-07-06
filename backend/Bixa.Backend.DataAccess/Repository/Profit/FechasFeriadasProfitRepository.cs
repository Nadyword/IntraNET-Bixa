using Bixa.Backend.DataAccess.Interfaces.Repositories.Profit;
using Bixa.Backend.DataAccess.Templates.Profit;
using Bixa.Backend.DataAccess.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;

namespace Bixa.Backend.DataAccess.Repository.Profit;

public class FechasFeriadasProfitRepository(ProfitDbContext context) : IFechasFeriadasProfitRepository
{
    private readonly ProfitDbContext _context = context ?? throw new ArgumentNullException(nameof(context));

    public async Task<List<DateTime>> GetFechasFeriadasAsync(DateTime desde, DateTime hasta)
    {
        var feriados = await _context.FechasFeriadas
            .FromSqlRaw(ProfitSqlTemplates.GetFechasFeriadas, new SqlParameter("@Desde", desde.Date), new SqlParameter("@Hasta", hasta.Date))
            .ToListAsync();

        return feriados.Select(f => f.Fecha.Date).ToList();
    }
}
