using Bixa.Backend.DataAccess.Interfaces.Repositories.Profit;
using Bixa.Backend.DataAccess.Entities.DbProfit;
using Bixa.Backend.DataAccess.Templates.Profit;
using Bixa.Backend.DataAccess.Context;
using Bixa.Backend.Models.Response;
using Microsoft.EntityFrameworkCore;
using Bixa.Backend.Models.Enums;
using Bixa.Backend.Models.Query;
using Microsoft.Data.SqlClient;
using System.Linq.Expressions;

namespace Bixa.Backend.DataAccess.Repository.Profit;

// Repositorio de ejemplo para la BD secundaria.
// Usa FromSqlRaw con las constantes de ExampleProfitSqlTemplates.
public class GrupoFaProfitRepository(ProfitDbContext context) : IGrupoFaProfitRepository
{
    private readonly ProfitDbContext _context = context;

    public async Task<bool> AnyAsync(Expression<Func<GrupoFa, bool>> predicate)
        => await _context.GrupoFa.AnyAsync(predicate);

    public async Task<List<GrupoFa>> GetAllAsync(int pageNumber, int pageSize)
    {
        int page = (pageNumber * pageSize) - pageSize;
        return await _context.GrupoFa
          .OrderBy(u => u.CodEmp)
          .Skip(page)
          .Take(pageSize)
          .ToListAsync();
    }

    public async Task<IEnumerable<GrupoFa?>> GetByIdAsync(int id)
        => await _context.GrupoFa
            .FromSqlRaw(ProfitSqlTemplates.GetByCi!, id)
            .ToListAsync();

    public async Task<Result<GrupoFa[]>> GetFullInfoByCiAsync(string? ci)
    {
        var emple = await _context.GrupoFa
            .FromSqlRaw(ProfitSqlTemplates.GetGrupoFamByCi!, new SqlParameter("@ci", ci))
            .ToArrayAsync();

        if (emple == null)
            return Result.Fail<GrupoFa[]>("Usuario no encontrado", ErrorTypeEnum.NotFound);

        return Result.Success(emple);
    }
}