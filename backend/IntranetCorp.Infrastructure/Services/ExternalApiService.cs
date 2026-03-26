using Microsoft.Extensions.Caching.Memory;

namespace IntranetCorp.Infrastructure.Services;

public interface IExternalApiService
{
    Task<int> GetVacacionDaysAsync(string cedula);
    Task<decimal> GetFiniquitoAmountAsync(string cedula);
}

public class ExternalApiService : IExternalApiService
{
    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _cache;
    private readonly int _cacheTtlMinutes;

    public ExternalApiService(HttpClient httpClient, IMemoryCache cache, int cacheTtlMinutes = 60)
    {
        _httpClient = httpClient;
        _cache = cache;
        _cacheTtlMinutes = cacheTtlMinutes;
    }

    public async Task<int> GetVacacionDaysAsync(string cedula)
    {
        var cacheKey = $"vacacion_days_{cedula}";

        if (_cache.TryGetValue(cacheKey, out int cachedDays))
            return cachedDays;

        // Placeholder: reemplazar por llamada real a API externa
        var days = CalculateVacacionDays(cedula);

        _cache.Set(cacheKey, days, TimeSpan.FromMinutes(_cacheTtlMinutes));

        return await Task.FromResult(days);
    }

    public async Task<decimal> GetFiniquitoAmountAsync(string cedula)
    {
        var cacheKey = $"finiquito_amount_{cedula}";

        if (_cache.TryGetValue(cacheKey, out decimal cachedAmount))
            return cachedAmount;

        // Placeholder: reemplazar por llamada real a API externa
        var amount = CalculateFiniquitoAmount(cedula);

        _cache.Set(cacheKey, amount, TimeSpan.FromMinutes(_cacheTtlMinutes));

        return await Task.FromResult(amount);
    }

    private int CalculateVacacionDays(string cedula)
    {
        // Mock: fórmula simple (LOTTT venezolana: 15 días base + acumulo por antigüedad)
        return 15;
    }

    private decimal CalculateFiniquitoAmount(string cedula)
    {
        // Mock: cálculo simple (valor de ejemplo)
        return 12450m;
    }
}
