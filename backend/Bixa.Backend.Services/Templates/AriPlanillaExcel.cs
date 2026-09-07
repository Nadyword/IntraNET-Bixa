using System.Buffers.Binary;
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

    /// <summary>Marcador sobre cuya celda se estampa la firma del contribuyente.</summary>
    private const string MarcadorFirma = "#FotoFirma";

    /// <summary>Fracciones en las que Excel divide el ancho de una columna al anclar un dibujo.</summary>
    private const int UnidadesColumna = 1024;

    /// <summary>Fracciones en las que Excel divide el alto de una fila al anclar un dibujo.</summary>
    private const int UnidadesFila = 256;

    /// <summary>La planilla es un formulario venezolano: la fecha va en castellano ("04 de septiembre de 2026").</summary>
    private static readonly CultureInfo Cultura = CultureInfo.GetCultureInfo("es-VE");

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

        // Antes de sustituir los marcadores: la firma se ubica por el texto de su propia celda.
        InsertarFirma(libro, hoja, model.FirmaImagen);

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
            ["#Lugar"] = model.Lugar,
            ["#FechaActual"] = model.FechaActual.ToString("dd 'de' MMMM 'de' yyyy", Cultura),
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
    /// Estampa la firma del contribuyente sobre la celda marcada con <c>#FotoFirma</c>, ajustada al
    /// recuadro de la planilla sin deformarla. El marcador se borra siempre, de modo que cuando el
    /// empleado no tiene una firma cargada la casilla simplemente queda vacía.
    /// </summary>
    private static void InsertarFirma(IWorkbook libro, ISheet hoja, byte[]? imagen)
    {
        var celda = BuscarMarcador(hoja, MarcadorFirma);
        if (celda is null) return;

        celda.SetCellValue(string.Empty);

        if (imagen is not { Length: > 0 }) return;

        var ancla = Anclar(hoja, Recuadro(hoja, celda.RowIndex, celda.ColumnIndex), DimensionesPng(imagen));
        ancla.AnchorType = AnchorType.DontMoveAndResize;

        var indice = libro.AddPicture(imagen, PictureType.PNG);
        hoja.CreateDrawingPatriarch().CreatePicture(ancla, indice);
    }

    private static ICell? BuscarMarcador(ISheet hoja, string marcador)
    {
        for (var f = hoja.FirstRowNum; f <= hoja.LastRowNum; f++)
        {
            var fila = hoja.GetRow(f);
            if (fila is null) continue;

            foreach (var celda in fila.Cells)
            {
                if (celda.CellType == CellType.String &&
                    string.Equals(celda.StringCellValue.Trim(), marcador, StringComparison.OrdinalIgnoreCase))
                {
                    return celda;
                }
            }
        }

        return null;
    }

    /// <summary>
    /// Recuadro que ocupará la imagen: la región combinada a la que pertenece la celda del marcador
    /// (la casilla de la firma en la plantilla suele serlo) o la celda sola si no está combinada.
    /// </summary>
    private static CellRangeAddress Recuadro(ISheet hoja, int fila, int columna)
    {
        for (var i = 0; i < hoja.NumMergedRegions; i++)
        {
            var region = hoja.GetMergedRegion(i);
            if (region.IsInRange(fila, columna)) return region;
        }

        return new CellRangeAddress(fila, fila, columna, columna);
    }

    /// <summary>
    /// Calcula el anclaje que centra la imagen dentro del recuadro respetando su proporción. Sin las
    /// dimensiones de la imagen no hay proporción que respetar y se estira al recuadro completo.
    /// </summary>
    private static HSSFClientAnchor Anclar(ISheet hoja, CellRangeAddress recuadro, (int Ancho, int Alto)? imagen)
    {
        var anchoRecuadro = Sumar(recuadro.FirstColumn, recuadro.LastColumn, c => AnchoColumna(hoja, c));
        var altoRecuadro = Sumar(recuadro.FirstRow, recuadro.LastRow, f => AltoFila(hoja, f));

        var escala = imagen is null
            ? 0f
            : Math.Min(anchoRecuadro / imagen.Value.Ancho, altoRecuadro / imagen.Value.Alto);

        var ancho = imagen is null ? anchoRecuadro : imagen.Value.Ancho * escala;
        var alto = imagen is null ? altoRecuadro : imagen.Value.Alto * escala;

        var izquierda = (anchoRecuadro - ancho) / 2f;
        var arriba = (altoRecuadro - alto) / 2f;

        var (col1, dx1) = Ubicar(izquierda, recuadro.FirstColumn, recuadro.LastColumn, c => AnchoColumna(hoja, c), UnidadesColumna);
        var (col2, dx2) = Ubicar(izquierda + ancho, recuadro.FirstColumn, recuadro.LastColumn, c => AnchoColumna(hoja, c), UnidadesColumna);
        var (fila1, dy1) = Ubicar(arriba, recuadro.FirstRow, recuadro.LastRow, f => AltoFila(hoja, f), UnidadesFila);
        var (fila2, dy2) = Ubicar(arriba + alto, recuadro.FirstRow, recuadro.LastRow, f => AltoFila(hoja, f), UnidadesFila);

        return new HSSFClientAnchor(dx1, dy1, dx2, dy2, col1, fila1, col2, fila2);
    }

    /// <summary>
    /// Traduce un desplazamiento en píxeles dentro del recuadro a la celda donde cae y la fracción
    /// de esa celda, que es como Excel expresa las esquinas de un dibujo.
    /// </summary>
    private static (int Indice, int Fraccion) Ubicar(float desplazamiento, int primera, int ultima, Func<int, float> medida, int unidades)
    {
        for (var i = primera; i < ultima; i++)
        {
            var tamano = medida(i);
            if (desplazamiento < tamano)
                return (i, Fraccion(desplazamiento, tamano, unidades));

            desplazamiento -= tamano;
        }

        return (ultima, Fraccion(desplazamiento, medida(ultima), unidades));
    }

    private static int Fraccion(float desplazamiento, float tamano, int unidades) =>
        tamano <= 0 ? 0 : (int)Math.Clamp(desplazamiento / tamano * unidades, 0, unidades - 1);

    private static float Sumar(int primera, int ultima, Func<int, float> medida)
    {
        var total = 0f;
        for (var i = primera; i <= ultima; i++) total += medida(i);
        return total;
    }

    private static float AnchoColumna(ISheet hoja, int columna) => (float)hoja.GetColumnWidthInPixels(columna);

    /// <summary>Alto de la fila en píxeles: NPOI lo expresa en puntos y la pantalla trabaja a 96 ppp.</summary>
    private static float AltoFila(ISheet hoja, int fila) =>
        (hoja.GetRow(fila)?.HeightInPoints ?? hoja.DefaultRowHeightInPoints) * 96f / 72f;

    /// <summary>
    /// Ancho y alto declarados en la cabecera IHDR del PNG: 8 bytes de firma, 4 de longitud, el
    /// nombre del bloque y los dos enteros big-endian. Retorna <c>null</c> si no es un PNG.
    /// </summary>
    private static (int Ancho, int Alto)? DimensionesPng(byte[] imagen)
    {
        if (imagen.Length < 24 || !imagen.AsSpan(0, 4).SequenceEqual(FirmaPng)) return null;

        var ancho = BinaryPrimitives.ReadInt32BigEndian(imagen.AsSpan(16, 4));
        var alto = BinaryPrimitives.ReadInt32BigEndian(imagen.AsSpan(20, 4));

        return ancho > 0 && alto > 0 ? (ancho, alto) : null;
    }

    private static ReadOnlySpan<byte> FirmaPng => [0x89, (byte)'P', (byte)'N', (byte)'G'];

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
