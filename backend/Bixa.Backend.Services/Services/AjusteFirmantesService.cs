using Bixa.Backend.DataAccess.Entities;
using Bixa.Backend.DataAccess.Interfaces.Repositories;
using Bixa.Backend.DataAccess.Models;
using Bixa.Backend.Models.DTOs.FirmantesDTO;
using Bixa.Backend.Models.Enums;
using Bixa.Backend.Models.Response;
using Bixa.Backend.Services.Interfaces;

namespace Bixa.Backend.Services.Services;

/// <summary>
/// Mantiene los ajustes manuales de la cadena de firmantes y los aplica al armar las aprobaciones
/// de una solicitud.
/// </summary>
/// <remarks>
/// De la cadena solo se guardan las diferencias contra la jerarquía de supervisores de Profit
/// (ver <see cref="AjusteFirmante"/>), así que hay dos operaciones simétricas: <c>Aplicar</c>, que
/// reconstruye la cadena efectiva, y <c>CalcularDiferencias</c>, que obtiene el ajuste a partir de
/// la cadena que el administrador dejó en pantalla.
/// </remarks>
public class AjusteFirmantesService(
    IAjusteFirmanteRepository ajusteFirmanteRepository,
    ISolicitudesRepository solicitudesRepository,
    IUserRepository userRepository,
    IUnitOfWork unitOfWork) : IAjusteFirmantesService
{
    private readonly IAjusteFirmanteRepository _ajusteFirmanteRepository = ajusteFirmanteRepository;
    private readonly ISolicitudesRepository _solicitudesRepository = solicitudesRepository;
    private readonly IUserRepository _userRepository = userRepository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    /// <summary>Tope de resultados del buscador de firmantes por agregar.</summary>
    private const int MaximoCandidatos = 50;

    public async Task<Result<AjusteFirmantesDTO>> GetAsync(string ci)
    {
        var normalizedCi = UtilityService.NormalizeCiFormat(ci);

        var firmantesProfit = await ObtenerFirmantesProfitAsync(normalizedCi);
        var ajustes = await _ajusteFirmanteRepository.GetByUserCiAsync(normalizedCi);
        var usuario = await _userRepository.GetUserByCiAsync(normalizedCi);

        return Result.Success(Describir(normalizedCi, NombreDe(usuario), firmantesProfit, ajustes));
    }

    public async Task<List<AprobadorPermisoInfo>> AplicarAjusteAsync(string ci, List<AprobadorPermisoInfo> firmantesProfit)
    {
        var normalizedCi = UtilityService.NormalizeCiFormat(ci);
        var ajustes = await _ajusteFirmanteRepository.GetByUserCiAsync(normalizedCi);

        return Aplicar(Normalizar(firmantesProfit), ajustes);
    }

    public async Task<Result<AjusteFirmantesDTO>> GuardarAsync(string ci, GuardarAjusteFirmantesDTO dto)
    {
        var normalizedCi = UtilityService.NormalizeCiFormat(ci);

        var solicitados = (dto.Firmantes ?? [])
            .Select(UtilityService.NormalizeCiFormat)
            .Where(c => !string.IsNullOrWhiteSpace(c))
            .ToList();

        // Una cadena vacía dejaría las solicitudes sin aprobaciones, es decir, sin forma de aprobarse.
        if (solicitados.Count == 0)
            return Result.Fail<AjusteFirmantesDTO>("La cadena debe tener al menos un firmante.", ErrorTypeEnum.Validation);

        if (solicitados.Distinct(StringComparer.OrdinalIgnoreCase).Count() != solicitados.Count)
            return Result.Fail<AjusteFirmantesDTO>("La cadena de firmantes tiene cédulas repetidas.", ErrorTypeEnum.Validation);

        if (solicitados.Contains(normalizedCi, StringComparer.OrdinalIgnoreCase))
            return Result.Fail<AjusteFirmantesDTO>("El empleado no puede firmar su propia solicitud.", ErrorTypeEnum.Validation);

        var firmantesProfit = await ObtenerFirmantesProfitAsync(normalizedCi);
        var cisProfit = firmantesProfit.Select(f => f.Ci).ToHashSet(StringComparer.OrdinalIgnoreCase);

        // Los firmantes que no vienen de Profit terminan en Aprobacion.AprobadorCi, que es clave
        // foránea contra Users: sin usuario en la intranet la solicitud fallaría al guardarse.
        var agregados = solicitados.Where(c => !cisProfit.Contains(c)).ToList();
        var usuariosAgregados = new Dictionary<string, Users>(StringComparer.OrdinalIgnoreCase);

        foreach (var agregado in agregados)
        {
            var usuario = await _userRepository.GetUserByCiAsync(agregado);

            if (usuario is null || !usuario.IsActive)
            {
                return Result.Fail<AjusteFirmantesDTO>(
                    $"El firmante con cédula {agregado} no es un usuario activo de la intranet.", ErrorTypeEnum.Validation);
            }

            usuariosAgregados[agregado] = usuario;
        }

        var diferencias = CalcularDiferencias(normalizedCi, firmantesProfit, solicitados, usuariosAgregados);

        await _ajusteFirmanteRepository.ReplaceAsync(normalizedCi, diferencias);
        await _unitOfWork.SaveChangesAsync();

        var solicitante = await _userRepository.GetUserByCiAsync(normalizedCi);
        return Result.Success(Describir(normalizedCi, NombreDe(solicitante), firmantesProfit, diferencias));
    }

    public async Task<Result<AjusteFirmantesDTO>> RestablecerAsync(string ci)
    {
        var normalizedCi = UtilityService.NormalizeCiFormat(ci);

        await _ajusteFirmanteRepository.ReplaceAsync(normalizedCi, []);
        await _unitOfWork.SaveChangesAsync();

        return await GetAsync(normalizedCi);
    }

    public async Task<Result<List<CandidatoFirmanteDTO>>> GetCandidatosAsync(string? query, int take)
    {
        var usuarios = await _userRepository.BuscarActivosAsync(query, Math.Clamp(take, 1, MaximoCandidatos));

        return Result.Success(usuarios
            .Select(u => new CandidatoFirmanteDTO { Ci = u.Ci, Nombre = NombreDe(u) ?? u.Ci })
            .ToList());
    }

    public async Task<Result<List<string>>> GetCisConAjusteAsync() =>
        Result.Success(await _ajusteFirmanteRepository.GetCisConAjusteAsync());

    /// <summary>
    /// Reconstruye la cadena efectiva: saca a los excluidos, levanta a los reubicados de su posición
    /// original y vuelve a insertarlos junto con los agregados, en las posiciones guardadas.
    /// </summary>
    private static List<AprobadorPermisoInfo> Aplicar(List<AprobadorPermisoInfo> firmantesProfit, List<AjusteFirmante> ajustes)
    {
        if (ajustes.Count == 0) return firmantesProfit;

        var porCi = ajustes
            .GroupBy(a => a.FirmanteCi, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

        var resultado = firmantesProfit
            .Where(f => !porCi.TryGetValue(f.Ci, out var ajuste) || ajuste.Accion == AccionAjusteFirmanteEnum.Agregado)
            .ToList();

        var enProfit = firmantesProfit.Select(f => f.Ci).ToHashSet(StringComparer.OrdinalIgnoreCase);

        // Se insertan por orden ascendente para que cada posición ya tenga delante lo que le precede.
        var porInsertar = ajustes
            .Where(a => a.Accion == AccionAjusteFirmanteEnum.Agregado ||
                       (a.Accion == AccionAjusteFirmanteEnum.Reubicado && enProfit.Contains(a.FirmanteCi)))
            .OrderBy(a => a.Orden ?? int.MaxValue)
            .ToList();

        foreach (var ajuste in porInsertar)
        {
            // Un agregado deja de tener sentido si la jerarquía terminó incluyéndolo por su cuenta.
            if (resultado.Any(f => string.Equals(f.Ci, ajuste.FirmanteCi, StringComparison.OrdinalIgnoreCase)))
                continue;

            var nombreEnProfit = firmantesProfit
                .FirstOrDefault(f => string.Equals(f.Ci, ajuste.FirmanteCi, StringComparison.OrdinalIgnoreCase))?.Nombre;

            var posicion = Math.Clamp((ajuste.Orden ?? resultado.Count + 1) - 1, 0, resultado.Count);

            resultado.Insert(posicion, new AprobadorPermisoInfo
            {
                Ci = ajuste.FirmanteCi,
                Nombre = nombreEnProfit ?? ajuste.NombreFirmante ?? ajuste.FirmanteCi,
            });
        }

        return resultado;
    }

    /// <summary>
    /// Obtiene el ajuste que lleva de la cadena de Profit a la cadena pedida: quién sobra, quién
    /// falta y quién se movió. De los que se movieron se guarda el mínimo posible, dejando quietos a
    /// los que ya conservan entre sí el orden de Profit.
    /// </summary>
    private static List<AjusteFirmante> CalcularDiferencias(
        string userCi,
        List<AprobadorPermisoInfo> firmantesProfit,
        List<string> solicitados,
        Dictionary<string, Users> usuariosAgregados)
    {
        var enProfit = firmantesProfit.Select(f => f.Ci).ToList();
        var diferencias = new List<AjusteFirmante>();

        int PosicionFinal(string firmanteCi) =>
            solicitados.FindIndex(s => string.Equals(s, firmanteCi, StringComparison.OrdinalIgnoreCase));

        AjusteFirmante Crear(string firmanteCi, AccionAjusteFirmanteEnum accion, int? orden) => new()
        {
            UserCi = userCi,
            FirmanteCi = firmanteCi,
            Accion = accion,
            Orden = orden,
            NombreFirmante = firmantesProfit
                .FirstOrDefault(f => string.Equals(f.Ci, firmanteCi, StringComparison.OrdinalIgnoreCase))?.Nombre
                ?? (usuariosAgregados.TryGetValue(firmanteCi, out var usuario) ? NombreDe(usuario) : null),
        };

        diferencias.AddRange(enProfit
            .Where(c => !solicitados.Contains(c, StringComparer.OrdinalIgnoreCase))
            .Select(c => Crear(c, AccionAjusteFirmanteEnum.Excluido, null)));

        diferencias.AddRange(solicitados
            .Where(c => !enProfit.Contains(c, StringComparer.OrdinalIgnoreCase))
            .Select(c => Crear(c, AccionAjusteFirmanteEnum.Agregado, PosicionFinal(c) + 1)));

        // De los que vienen de Profit y siguen firmando, solo son reubicados los que rompen el orden
        // relativo original; los demás vuelven solos a su sitio al aplicar el ajuste.
        var sobrevivientes = enProfit.Where(c => solicitados.Contains(c, StringComparer.OrdinalIgnoreCase)).ToList();
        var posiciones = sobrevivientes.Select(PosicionFinal).ToList();
        var quietos = IndicesDeLaSubsecuenciaCrecienteMasLarga(posiciones);

        diferencias.AddRange(sobrevivientes
            .Where((_, i) => !quietos.Contains(i))
            .Select(c => Crear(c, AccionAjusteFirmanteEnum.Reubicado, PosicionFinal(c) + 1)));

        return diferencias;
    }

    /// <summary>
    /// Índices de la subsecuencia creciente más larga: el mayor grupo de firmantes que conserva el
    /// orden de Profit entre sí, y que por lo tanto no hace falta guardar como reubicado.
    /// </summary>
    private static HashSet<int> IndicesDeLaSubsecuenciaCrecienteMasLarga(List<int> valores)
    {
        if (valores.Count == 0) return [];

        var longitud = new int[valores.Count];
        var anterior = new int[valores.Count];

        for (var i = 0; i < valores.Count; i++)
        {
            longitud[i] = 1;
            anterior[i] = -1;

            for (var j = 0; j < i; j++)
            {
                if (valores[j] < valores[i] && longitud[j] + 1 > longitud[i])
                {
                    longitud[i] = longitud[j] + 1;
                    anterior[i] = j;
                }
            }
        }

        var fin = 0;
        for (var i = 1; i < valores.Count; i++)
            if (longitud[i] > longitud[fin]) fin = i;

        var indices = new HashSet<int>();
        for (var i = fin; i >= 0; i = anterior[i])
        {
            indices.Add(i);
            if (anterior[i] < 0) break;
        }

        return indices;
    }

    private async Task<List<AprobadorPermisoInfo>> ObtenerFirmantesProfitAsync(string ci) =>
        Normalizar(await _solicitudesRepository.GetAprovadoresPermisosByCi(ci));

    /// <summary>Profit devuelve las cédulas en columnas <c>char</c>, con el relleno a la derecha.</summary>
    private static List<AprobadorPermisoInfo> Normalizar(List<AprobadorPermisoInfo> firmantes) =>
        [.. firmantes.Select(f => new AprobadorPermisoInfo
        {
            Ci = UtilityService.NormalizeCiFormat(f.Ci),
            Nombre = f.Nombre?.Trim() ?? string.Empty,
        })];

    private static AjusteFirmantesDTO Describir(
        string ci,
        string? nombreCompleto,
        List<AprobadorPermisoInfo> firmantesProfit,
        List<AjusteFirmante> ajustes)
    {
        var efectiva = Aplicar(firmantesProfit, ajustes);
        var cisProfit = firmantesProfit.Select(f => f.Ci).ToHashSet(StringComparer.OrdinalIgnoreCase);

        return new AjusteFirmantesDTO
        {
            Ci = ci,
            NombreCompleto = nombreCompleto,
            TieneAjuste = ajustes.Count > 0,
            Original = ComoDto(firmantesProfit, cisProfit),
            Efectiva = ComoDto(efectiva, cisProfit),
        };
    }

    private static List<FirmanteDTO> ComoDto(List<AprobadorPermisoInfo> firmantes, HashSet<string> cisProfit) =>
        [.. firmantes.Select((f, i) => new FirmanteDTO
        {
            Ci = f.Ci,
            Nombre = f.Nombre,
            Orden = i + 1,
            DesdeProfit = cisProfit.Contains(f.Ci),
        })];

    private static string? NombreDe(Users? usuario) =>
        usuario is null ? null : $"{usuario.FirstName} {usuario.LastName}".Trim();
}
