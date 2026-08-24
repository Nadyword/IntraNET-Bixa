using System.Globalization;
using Bixa.Backend.Models.DTOs.ReportesModelDTO;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.SS.Util;

namespace Bixa.Backend.Services.Templates;

/// <summary>
/// Rellena una copia de la plantilla <c>Planilla ARI.xls</c> (formulario AR-I del SENIAT).
/// </summary>
/// <remarks>
/// La plantilla es un libro Excel 97-2003 (BIFF8) que ya trae las fórmulas de las casillas A a K.
/// Aquí solo se sustituyen las celdas marcadas con <c>#nombre</c> por su valor correspondiente, se
/// marca el mes en el punto 5 y se recalcula el libro para que los valores queden cacheados.
/// </remarks>
public static class AriPlanillaExcel
{
    /// <summary>Nombre del archivo de plantilla dentro de la carpeta de plantillas.</summary>
    public const string NombreArchivoPlantilla = "Planilla ARI.xls";

    /// <summary>Celda combinada del punto 5 con los cinco meses; no tiene marcador <c>#nombre</c>.</summary>
    private const string CeldaMeses = "I11";

    /// <summary>
    /// Casilla E.1. La plantilla la dejó vacía aunque su etiqueta (P38) dice "774 U.T.", y la
    /// fórmula de la casilla F es <c>IF(O35&gt;774, O35, O38)</c>: sin este valor, elegir el
    /// desgravamen único daría un desgravamen de cero.
    /// </summary>
    private const string CeldaDesgravamenUnico = "O38";

    public static byte[] Rellenar(string plantillaPath, AriReportModel model)
    {
        if (!File.Exists(plantillaPath))
            throw new FileNotFoundException($"No se encontró la plantilla de la planilla AR-I en '{plantillaPath}'.", plantillaPath);

        // Se copia a memoria para no mantener abierto el archivo original en ningún momento.
        using var plantilla = new MemoryStream(File.ReadAllBytes(plantillaPath));
        var libro = new HSSFWorkbook(plantilla);
        var hoja = libro.GetSheetAt(0);

        SustituirMarcadores(hoja, ConstruirMarcadores(model));
        MarcarMes(hoja, model.Mes);
        EscribirNumero(hoja, CeldaDesgravamenUnico, AriReportModel.DesgravamenUnicoUt);

        Recalcular(libro, hoja);

        using var salida = new MemoryStream();
        libro.Write(salida, leaveOpen: true);
        return salida.ToArray();
    }

    /// <summary>
    /// Arma el mapa de marcador a valor. Un valor <see cref="decimal"/> se escribe como número para
    /// que las fórmulas de la plantilla lo tomen; un <see cref="string"/> se escribe como texto.
    /// </summary>
    private static Dictionary<string, object> ConstruirMarcadores(AriReportModel model)
    {
        // Con el desgravamen único el cuadro C va en cero: la plantilla resuelve en (F) cuál de los
        // dos desgravámenes aplica, quedándose con el mayor entre el detallado y las 774 U.T.
        var esUnico = string.Equals(model.DesgravamenTipo, "Unico", StringComparison.OrdinalIgnoreCase);

        return new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
        {
            ["#Nombre_empresa"] = model.NombreEmpresa,
            ["#nombre_completo"] = model.NombreCompleto,
            ["#ci"] = ComoNumeroSiAplica(model.Ci),
            ["#rif"] = model.Rif,
            ["#AnoActual"] = model.AnoGravable.ToString(CultureInfo.InvariantCulture),
            ["#GranTotal"] = model.GranTotal,
            ["#UniTribu"] = model.UniTribu,
            ["#cargaFami"] = (decimal)model.CargaFamiliar,
            ["#DESGRAVAMEN1"] = esUnico ? 0m : model.InstitutosDocentes,
            ["#DESGRAVAMEN2"] = esUnico ? 0m : model.PrimasSeguro,
            ["#DESGRAVAMEN3"] = esUnico ? 0m : model.ServiciosMedicos,
            ["#DESGRAVAMEN4"] = esUnico ? 0m : model.InteresesVivienda,
        };
    }

    /// <summary>
    /// La celda de la cédula viene con formato numérico en la plantilla, así que se escribe como
    /// número cuando la cédula es solo dígitos y como texto en cualquier otro caso.
    /// </summary>
    private static object ComoNumeroSiAplica(string ci) =>
        decimal.TryParse(ci, NumberStyles.None, CultureInfo.InvariantCulture, out var numero) ? numero : ci;

    private static void SustituirMarcadores(ISheet hoja, Dictionary<string, object> marcadores)
    {
        for (var f = hoja.FirstRowNum; f <= hoja.LastRowNum; f++)
        {
            var fila = hoja.GetRow(f);
            if (fila is null) continue;

            foreach (var celda in fila.Cells)
            {
                // Solo las celdas de texto son marcadores: las de fórmula que muestran un #nombre
                // lo traen cacheado de la celda a la que apuntan y se resuelven al recalcular.
                if (celda.CellType != CellType.String) continue;

                if (marcadores.TryGetValue(celda.StringCellValue.Trim(), out var valor))
                    Escribir(celda, valor);
            }
        }
    }

    /// <summary>
    /// Marca con una X el mes al que corresponde la planilla dentro de la lista de meses del punto 5,
    /// que en la plantilla es una sola celda combinada con los cinco nombres.
    /// </summary>
    private static void MarcarMes(ISheet hoja, string mes)
    {
        if (string.IsNullOrWhiteSpace(mes)) return;

        var celda = Celda(hoja, CeldaMeses);
        if (celda.CellType != CellType.String) return;

        var texto = celda.StringCellValue;
        var indice = texto.IndexOf(mes, StringComparison.OrdinalIgnoreCase);
        if (indice < 0) return;

        celda.SetCellValue(texto.Insert(indice + mes.Length, " (X)"));
    }

    /// <summary>
    /// Recalcula las fórmulas para dejar los valores cacheados en el archivo y, además, marca la hoja
    /// para que Excel vuelva a calcular al abrirla, por si alguna función no la resuelve NPOI.
    /// </summary>
    private static void Recalcular(IWorkbook libro, ISheet hoja)
    {
        hoja.ForceFormulaRecalculation = true;

        try
        {
            HSSFFormulaEvaluator.EvaluateAllFormulaCells(libro);
        }
        catch (Exception)
        {
            // Si NPOI no resuelve alguna fórmula, ForceFormulaRecalculation deja que Excel la calcule al abrir.
        }
    }

    private static void EscribirNumero(ISheet hoja, string referencia, decimal valor) =>
        Celda(hoja, referencia).SetCellValue((double)valor);

    private static void Escribir(ICell celda, object valor)
    {
        if (valor is decimal numero)
            celda.SetCellValue((double)numero);
        else
            celda.SetCellValue(valor as string ?? string.Empty);
    }

    private static ICell Celda(ISheet hoja, string referencia)
    {
        var posicion = new CellReference(referencia);
        var fila = hoja.GetRow(posicion.Row) ?? hoja.CreateRow(posicion.Row);
        return fila.GetCell(posicion.Col, MissingCellPolicy.CREATE_NULL_AS_BLANK);
    }
}
