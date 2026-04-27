using Bixa.Backend.DataAccess.Interfaces.Repositories.Profit;
using Bixa.Backend.DataAccess.Entities.DbProfit;
using Bixa.Backend.DataAccess.Templates.Profit;
using Bixa.Backend.DataAccess.Context;
using Bixa.Backend.Models.Response;
using Microsoft.EntityFrameworkCore;
using Bixa.Backend.Models.Enums;
using Bixa.Backend.Models.Query;
using System.Linq.Expressions;
using Microsoft.Data.SqlClient;

namespace Bixa.Backend.DataAccess.Repository.Profit;

// Repositorio de ejemplo para la BD secundaria.
// Usa FromSqlRaw con las constantes de ExampleProfitSqlTemplates.
public class SnEmpleProfitRepository(ProfitDbContext context) : ISnEmpleProfitRepository
{
    private readonly ProfitDbContext _context = context;

    public async Task<bool> AnyAsync(Expression<Func<SnEmple, bool>> predicate)
        => await _context.SnEmple.AnyAsync(predicate);

    public async Task<PaginatedResult<SnEmple>> GetAllAsync(object filters, Pagination? pagination)
    {
        var page = pagination ?? new Pagination();

        var items = await _context.SnEmple
            .FromSqlRaw(ProfitSqlTemplates.GetByCi!)
            .Skip((page.PageNumber - 1) * page.PageSize)
            .Take(page.PageSize)
            .ToListAsync();

        var totalCount = items.Count;
        return new PaginatedResult<SnEmple>(items, totalCount, page.PageNumber, page.PageSize);
    }

    public async Task<IEnumerable<SnEmple?>> GetByIdAsync(int id)
        => await _context.SnEmple
            .FromSqlRaw(ProfitSqlTemplates.GetByCi!, id)
            .ToListAsync();

    public async Task<string?> GetEmailByCiAsync(string ci)
        => await _context.SnEmple
            .FromSqlRaw(ProfitSqlTemplates.GetEmailByCi!, new SqlParameter("@ci", ci))
            .Select(e => e.CorreoE)
            .FirstOrDefaultAsync();

    public async Task<Result<SnEmple>> GetFullInfoByCiAsync(string ci)
    {
        var emple = await _context.SnEmple
            .FromSqlRaw(ProfitSqlTemplates.GetByCi!, new SqlParameter("@ci", ci))
            .FirstOrDefaultAsync();

        if (emple == null)
            return Result.Fail<SnEmple>("Usuario no encontrado", ErrorTypeEnum.NotFound);

        return Result.Success(emple);
    }
}