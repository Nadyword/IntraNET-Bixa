namespace Bixa.Backend.Models.DTOs.ReportesModelDTO;

/// <summary>
/// Lo que el empleado decide en el formulario de la planilla AR-I: el mes y el desgravamen.
/// Todo lo demás (identidad, U.T., carga familiar y remuneraciones estimadas) proviene de la
/// consulta <c>GetARI</c> contra Profit.
/// </summary>
public class AriReportRequestDTO
{
    /// <summary>Enero, Marzo, Junio, Septiembre o Diciembre. Enero = declaración inicial; el resto = variación.</summary>
    public string Mes { get; set; } = string.Empty;

    /// <summary>"Unico" (774 U.T.) o "Detallado".</summary>
    public string DesgravamenTipo { get; set; } = string.Empty;

    // Cuadro C, en Bs. Se ignoran cuando el desgravamen es único.
    public decimal InstitutosDocentes { get; set; }
    public decimal PrimasSeguro { get; set; }
    public decimal ServiciosMedicos { get; set; }
    public decimal InteresesVivienda { get; set; }
}

/// <summary>
/// Valores con los que se rellenan las casillas marcadas con <c>#nombre</c> en la plantilla
/// <c>Planilla ARI.xls</c>. La plantilla trae sus propias fórmulas (casillas A a K), así que aquí
/// solo viajan datos de entrada.
/// </summary>
public class AriReportModel
{
    /// <summary>Monto fijo del desgravamen único, Art. 62 de la Ley de I.S.L.R. (casilla E).</summary>
    public const decimal DesgravamenUnicoUt = 774m;

    // ─── Provenientes de la consulta GetARI ──────────────────────────────────

    /// <summary>Marcador <c>#Nombre_empresa</c>.</summary>
    public string NombreEmpresa { get; set; } = string.Empty;

    /// <summary>Marcador <c>#nombre_completo</c>.</summary>
    public string NombreCompleto { get; set; } = string.Empty;

    /// <summary>Marcador <c>#ci</c>.</summary>
    public string Ci { get; set; } = string.Empty;

    /// <summary>Marcador <c>#rif</c>.</summary>
    public string Rif { get; set; } = string.Empty;

    /// <summary>Marcador <c>#AnoActual</c>.</summary>
    public int AnoGravable { get; set; }

    /// <summary>Marcador <c>#UniTribu</c>: valor vigente de la Unidad Tributaria en Bs.</summary>
    public decimal UniTribu { get; set; }

    /// <summary>Marcador <c>#cargaFami</c>: número de cargas familiares.</summary>
    public int CargaFamiliar { get; set; }

    /// <summary>Marcador <c>#GranTotal</c>: remuneraciones estimadas por percibir en el año, en Bs.</summary>
    public decimal GranTotal { get; set; }

    /// <summary>Marcador <c>#Lugar</c>: localidad desde la que se emite la planilla.</summary>
    public string Lugar { get; set; } = string.Empty;

    /// <summary>Marcador <c>#FechaActual</c>: fecha de emisión, escrita en la planilla en formato largo.</summary>
    public DateTime FechaActual { get; set; }

    /// <summary>
    /// Nombre del archivo de la firma del contribuyente dentro de la carpeta de firmas. La imagen se
    /// estampa sobre la celda del marcador <c>#FotoFirma</c>.
    /// </summary>
    public string? FotoFirma { get; set; }

    /// <summary>
    /// Contenido de la firma indicada por <see cref="FotoFirma"/>, que carga el servicio de reportes.
    /// Queda en <c>null</c> cuando el archivo no existe: en ese caso la casilla de la firma va vacía.
    /// </summary>
    public byte[]? FirmaImagen { get; set; }

    // ─── Provenientes del formulario ─────────────────────────────────────────

    /// <summary>Mes al que corresponde la planilla; se marca con una X en el punto 5.</summary>
    public string Mes { get; set; } = string.Empty;

    /// <summary>"Unico" o "Detallado".</summary>
    public string DesgravamenTipo { get; set; } = string.Empty;

    /// <summary>Marcador <c>#DESGRAVAMEN1</c>, en Bs.</summary>
    public decimal InstitutosDocentes { get; set; }

    /// <summary>Marcador <c>#DESGRAVAMEN2</c>, en Bs.</summary>
    public decimal PrimasSeguro { get; set; }

    /// <summary>Marcador <c>#DESGRAVAMEN3</c>, en Bs.</summary>
    public decimal ServiciosMedicos { get; set; }

    /// <summary>Marcador <c>#DESGRAVAMEN4</c>, en Bs.</summary>
    public decimal InteresesVivienda { get; set; }
}
