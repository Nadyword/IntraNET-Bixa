using Bixa.Backend.DataAccess.Interfaces.Repositories;
using Bixa.Backend.DataAccess.Entities.Solicitudes;
using Bixa.Backend.DataAccess.Templates.Profit;
using Bixa.Backend.DataAccess.Context;
using Bixa.Backend.DataAccess.Entities;
using Bixa.Backend.DataAccess.Models;
using Microsoft.EntityFrameworkCore;

namespace Bixa.Backend.DataAccess.Repository;

public class SolicitudesRepository(AppDbContext dbContext, ProfitDbContext profitDbContext) : ISolicitudesRepository
{
    private readonly AppDbContext _context = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    private readonly ProfitDbContext _profitContext = profitDbContext ?? throw new ArgumentNullException(nameof(profitDbContext));

    public async Task<List<AprobadorPermisoInfo>> GetAprovadoresPermisosByCi(string ci)
    {
        var result = await _profitContext.AprobadorPermisoInfo
            .FromSqlRaw(ProfitSqlTemplates.GetListAprovadoresByCi!.Replace("@ci", ci))
            .ToListAsync();

        return result;
    }

    public async Task<bool> AddSolicitudVacaciones(SolicitudVacaciones solicitud)
    {
        var result = await _context.SolicitudesVacaciones.AddAsync(solicitud);
        await _context.SaveChangesAsync();
        return result != null;
    }

    public async Task<int> AddNewTramite(Tramite tramite)
    {
        var result = await _context.Tramites.AddAsync(tramite);
        await _context.SaveChangesAsync();
        return (result?.Entity.Id) ?? 0;
    }

    public async Task<int> AddAprobaciones(Aprobacion aprobaciones)
    {
        var result = await _context.Aprobaciones.AddAsync(aprobaciones);
        await _context.SaveChangesAsync();
        return (result?.Entity.Id) ?? 0;
    }
}