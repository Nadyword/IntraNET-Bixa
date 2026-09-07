namespace Bixa.Backend.Models.DTOs.FirmantesDTO;

/// <summary>Un firmante dentro de una cadena de aprobación, en la posición en que le toca firmar.</summary>
public class FirmanteDTO
{
    public string Ci { get; set; } = string.Empty;

    public string Nombre { get; set; } = string.Empty;

    /// <summary>Posición en la que firma; 1 es el primero.</summary>
    public int Orden { get; set; }

    /// <summary>
    /// <c>true</c> cuando el firmante viene de la jerarquía de supervisores de Profit y <c>false</c>
    /// cuando lo agregó un ajuste manual. Sirve para distinguirlos en pantalla.
    /// </summary>
    public bool DesdeProfit { get; set; }
}

/// <summary>
/// Cadena de firmantes de un empleado: la que arroja Profit y la que se usará realmente después de
/// aplicar sus ajustes.
/// </summary>
public class AjusteFirmantesDTO
{
    public string Ci { get; set; } = string.Empty;

    public string? NombreCompleto { get; set; }

    /// <summary><c>true</c> si el empleado tiene diferencias guardadas respecto de Profit.</summary>
    public bool TieneAjuste { get; set; }

    /// <summary>Cadena de supervisores tal como la devuelve Profit, sin ajustes.</summary>
    public List<FirmanteDTO> Original { get; set; } = [];

    /// <summary>Cadena que firmará realmente: la de Profit con los ajustes aplicados.</summary>
    public List<FirmanteDTO> Efectiva { get; set; } = [];
}

/// <summary>
/// Cadena de firmantes tal como quedó en pantalla. El backend la compara contra la de Profit y
/// guarda únicamente las diferencias.
/// </summary>
public class GuardarAjusteFirmantesDTO
{
    /// <summary>CIs de los firmantes en el orden en que deben firmar.</summary>
    public List<string> Firmantes { get; set; } = [];
}

/// <summary>Usuario de la intranet que puede añadirse como firmante.</summary>
public class CandidatoFirmanteDTO
{
    public string Ci { get; set; } = string.Empty;

    public string Nombre { get; set; } = string.Empty;
}
