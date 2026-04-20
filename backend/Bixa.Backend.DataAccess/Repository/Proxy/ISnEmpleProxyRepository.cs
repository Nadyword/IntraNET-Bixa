using Bixa.Backend.DataAccess.Context;
using Bixa.Backend.DataAccess.Entities.DbProxy;
using Bixa.Backend.DataAccess.Interfaces.Repositories.Proxy;
using Bixa.Backend.DataAccess.Templates.Proxy;
using Bixa.Backend.Models.Query;
using Bixa.Backend.Models.Response;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Bixa.Backend.DataAccess.Repository.Proxy;

// Repositorio de ejemplo para la BD secundaria.
// Usa FromSqlRaw con las constantes de ExampleProxySqlTemplates.
public class SnEmpleProxyRepository(ProxyDbContext context) : ISnEmpleProxyRepository
{
    private readonly ProxyDbContext _context = context;

    public async Task<bool> AnyAsync(Expression<Func<SnEmple, bool>> predicate)
        => await _context.SnEmple.AnyAsync(predicate);

    public async Task<PaginatedResult<SnEmple>> GetAllAsync(object filters, Pagination? pagination)
    {
        var page = pagination ?? new Pagination();

        var items = await _context.SnEmple
            .FromSqlRaw(ProxySqlTemplates.GetCiByEmai!)
            .Skip((page.PageNumber - 1) * page.PageSize)
            .Take(page.PageSize)
            .ToListAsync();

        var totalCount = items.Count;
        return new PaginatedResult<SnEmple>(items, totalCount, page.PageNumber, page.PageSize);
    }

    public async Task<IEnumerable<SnEmple?>> GetByIdAsync(int id)
        => await _context.SnEmple
            .FromSqlRaw(ProxySqlTemplates.GetCiByEmai!, id)
            .ToListAsync();

    public async Task<string?> GetEmailByCiAsync(string? ci)
    => (await _context.SnEmple
        .FromSqlRaw(ProxySqlTemplates.GetCiByEmai!, new SqlParameter("@ci", ci))
        .FirstOrDefaultAsync())?.CorreoE;
}