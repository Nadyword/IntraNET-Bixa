using Bixa.Backend.DataAccess.Interfaces.Repositories;
using Bixa.Backend.DataAccess.Entities.Solicitudes;
using Bixa.Backend.DataAccess.Templates.Profit;
using Bixa.Backend.DataAccess.Context;
using Bixa.Backend.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;

namespace Bixa.Backend.DataAccess.Repository;

public class SolicitudesRepository(AppDbContext dbContext, ProfitDbContext profitDbContext) : ISolicitudesRepository
{
    private readonly AppDbContext _context = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    private readonly ProfitDbContext _profitContext = profitDbContext ?? throw new ArgumentNullException(nameof(profitDbContext));

    public async Task<List<string>> GetAprovadoresPermisosByCi(string ci)
    {
        var result = await _profitContext.Database
            .SqlQueryRaw<string>(ProfitSqlTemplates.GetListAprovadoresByCi!.Replace("@ci", ci)).ToListAsync();

        return result;
    }

    public async Task<bool> AddSolicitudVacaciones(SolicitudVacaciones solicitud)
    {
        var result = await _context.SolicitudesVacaciones.AddAsync(solicitud);
        return result != null;
    }

    public async Task<bool> AddNewTramite(Tramite tramite)
    {
        var result = await _context.Tramites.AddAsync(tramite);
        return result != null;
    }
}