using Bixa.Backend.DataAccess.Interfaces.Repositories;
using Bixa.Backend.DataAccess.Context;
using Bixa.Backend.DataAccess.Entities;
using Bixa.Backend.DataAccess.Models;
using Microsoft.EntityFrameworkCore;

namespace Bixa.Backend.DataAccess.Repository;

/// <summary>
/// Provides data access operations for User entities.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="SoporteChatRepository"/> class.
/// </remarks>
/// <param name="dbContext">The application's database context.</param>
/// <exception cref="ArgumentNullException">Thrown if the provided database context is null.</exception>
public class SoporteChatRepository(AppDbContext dbContext) : ISoporteChatRepository
{
    private readonly AppDbContext _context = dbContext ?? throw new ArgumentNullException(nameof(dbContext));

    public async Task<string> AddNewAnswerAsync(SoporteChat soporte)
    {
        try
        {
            // soporte.UserCi     = CI del empleado (hilo al que se responde)
            // soporte.RespondidoPorCi = CI del agente (quien responde)
            var employeeCi = soporte.UserCi;
            var agentCi = soporte.RespondidoPorCi!;

            // Marcar mensajes anteriores del empleado como respondidos
            var existingSoporte = _context.SoporteChats
                .Where(sc => string.IsNullOrEmpty(sc.RespondidoPorCi) && sc.UserCi == employeeCi)
                .ToList();

            foreach (var item in existingSoporte)
                item.RespondidoPorCi = agentCi;

            // La respuesta se guarda con UserCi = agente y RespondidoPorCi = empleado,
            // para que el frontend pueda diferenciarlos por UserCi.
            soporte.UserCi = agentCi;
            soporte.RespondidoPorCi = employeeCi;

            await _context.SoporteChats.AddAsync(soporte);
            await _context.SaveChangesAsync();
            return "Respuesta agregada correctamente";
        }
        catch (Exception ex)
        {
            throw new NotImplementedException(ex.Message);
        }
    }

    public async Task<string> AddNewMessageAsync(SoporteChat soporte)
    {
        try
        {
            await _context.SoporteChats.AddAsync(soporte);
            await _context.SaveChangesAsync();

            return "Mensaje agregado correctamente";
        }
        catch (Exception ex)
        {
            throw new NotImplementedException(ex.Message);
        }
    }

    public async Task<bool> HasMessageTodayAsync(string ci)
    {
        var hoyUtc = DateTime.UtcNow.Date;
        return await _context.SoporteChats.AnyAsync(sc => sc.UserCi == ci && sc.CreatedAt >= hoyUtc);
    }

    public async Task<List<SolicitudesChats>> GetChatRequests()
    {
        var adminCis = await _context.Users
            .Where(u => u.IdUserRol == 1)
            .Select(u => u.Ci)
            .ToHashSetAsync();

        return await (from sc in _context.SoporteChats
                      join u in _context.Users on sc.UserCi equals u.Ci
                      where !adminCis.Contains(sc.UserCi)
                      group sc by new { sc.UserCi, u.FirstName, u.LastName } into grouped
                      select new SolicitudesChats
                      {
                          UserCi = grouped.Key.UserCi,
                          FirstName = grouped.Key.FirstName,
                          LastName = grouped.Key.LastName,
                          Respondido = grouped.Any(m => m.RespondidoPorCi == null) ? 0 : 1
                      })
                     .OrderBy(x => x.Respondido)
                     .ToListAsync();
    }

    public async Task<SoporteChat[]> GetHistoriChat(string Ci) =>
        await _context.SoporteChats
            .Where(sc => sc.UserCi == Ci || sc.RespondidoPorCi == Ci)
            .OrderBy(sc => sc.CreatedAt)
            .ToArrayAsync();

    public async Task<bool> SetMessageStatus(string Ci)
    {
        await _context.SoporteChats
            .Where(sc => sc.UserCi == Ci || sc.RespondidoPorCi == Ci)
            .ExecuteUpdateAsync(s => s.SetProperty(sc => sc.IsRead, true));
        return true;
    }

    public async Task<bool> CreateFAQ(FAQs fAQs)
    {
        // Sin orden explícito la pregunta se agrega al final de la lista.
        if (fAQs.DisplayOrder <= 0)
        {
            var ultimoOrden = await _context.FAQs.MaxAsync(f => (int?)f.DisplayOrder) ?? 0;
            fAQs.DisplayOrder = ultimoOrden + 1;
        }

        await _context.FAQs.AddAsync(fAQs);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UpdateFAQ(FAQs fAQs)
    {
        var existingFAQ = await _context.FAQs.FindAsync(fAQs.Id);
        if (existingFAQ == null) return false;
        existingFAQ.Question = fAQs.Question;
        existingFAQ.Response = fAQs.Response;
        // Un orden no especificado (0 o menos) conserva la posición actual.
        if (fAQs.DisplayOrder > 0) existingFAQ.DisplayOrder = fAQs.DisplayOrder;
        existingFAQ.UpdatedAt = DateTime.UtcNow;
        _context.FAQs.Update(existingFAQ);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteFAQ(int id)
    {
        var existingFAQ = await _context.FAQs.FindAsync(id);
        if (existingFAQ == null) return false;
        _context.FAQs.Remove(existingFAQ);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<FAQs[]> GetAllFAQs() =>
        await _context.FAQs
            .OrderBy(f => f.DisplayOrder)
            .ThenBy(f => f.Id)
            .ToArrayAsync();
}