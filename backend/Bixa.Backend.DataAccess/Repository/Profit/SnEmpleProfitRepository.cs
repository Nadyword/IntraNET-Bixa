using Bixa.Backend.DataAccess.Interfaces.Repositories.Profit;
using Bixa.Backend.DataAccess.Entities.DbProfit;
using Bixa.Backend.DataAccess.Templates.Profit;
using Bixa.Backend.DataAccess.Context;
using Bixa.Backend.Models.Response;
using Microsoft.EntityFrameworkCore;
using Bixa.Backend.Models.Enums;
using System.Linq.Expressions;
using Microsoft.Data.SqlClient;
using Bixa.Backend.Models.DTOs.UserModelDTO;

namespace Bixa.Backend.DataAccess.Repository.Profit;

// Repositorio de ejemplo para la BD secundaria.
// Usa FromSqlRaw con las constantes de ExampleProfitSqlTemplates.
public class SnEmpleProfitRepository(ProfitDbContext context) : ISnEmpleProfitRepository
{
    private readonly ProfitDbContext _context = context;

    public async Task<bool> AnyAsync(Expression<Func<SnEmple, bool>> predicate)
        => await _context.SnEmple.AnyAsync(predicate);

    public async Task<List<SnEmple>> GetAllAsync(int pageNumber, int pageSize)
    {
        int page = (pageNumber * pageSize) - pageSize;
        var result = _context.SnEmple
          .OrderBy(u => u.CodEmp)
          .Skip(page)
          .Take(pageSize);

        return await result
            .OrderByDescending(u => u.CodEmp)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<IEnumerable<SnEmple?>> GetByCiAsync(string ci)
        => await _context.SnEmple
            .FromSqlRaw(ProfitSqlTemplates.GetByCi!, new SqlParameter("@ci", ci))
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

    public async Task<Result<List<SnEmple>>> GetAllByCiAsync(List<UserDTO> users)
    {
        var ciList = string.Join(",", users.Select(u => $"'{u.Ci}'"));
        var emple = await _context.SnEmple
            .FromSqlRaw(ProfitSqlTemplates.GetByCi!.Replace("@ci", ciList))
            .ToListAsync();
        if (emple == null || emple.Count == 0)
            return Result.Fail<List<SnEmple>>("Usuario no encontrado", ErrorTypeEnum.NotFound);
        return Result.Success(emple);
    }
}