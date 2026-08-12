using System.Globalization;
using Bixa.Backend.Models.DTOs.ReportesModelDTO;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Bixa.Backend.Services.Templates;

/// <summary>
/// Replica la planilla AR-I del SENIAT ("Determinación del Porcentaje de Retención de I.S.L.R.").
/// No incluye las secciones L/M de constancia de entrega (firmas): este reporte es autogenerado
/// por el empleado y no requiere firma del agente de retención.
/// </summary>
public class AriDocument(AriReportModel model) : IDocument
{
    private static readonly CultureInfo Culture = CultureInfo.GetCultureInfo("es-VE");

    private const string BorderColor = "#333333";
    private const string GrayBg = "#EEEEEE";
    private const string LabelColor = "#555555";

    private const string EmpresaNombre = "PRODUCTOS BIXA, S.A.";
    private const string EmpresaRif = "J-00220504-0";

    private static readonly string[] MesesVariacion = ["Marzo", "Junio", "Septiembre", "Diciembre"];

    public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

    public void Compose(IDocumentContainer container)
    {
        container.Page(page =>
        {
            page.Size(PageSizes.A4);
            page.Margin(1.5f, Unit.Centimetre);
            page.DefaultTextStyle(x => x.FontSize(8).FontFamily(Fonts.Arial));
            page.Content().Element(ComposeContent);
        });
    }

    private void ComposeContent(IContainer container)
    {
        container.Column(col =>
        {
            col.Spacing(6);
            col.Item().Element(ComposeHeader);
            col.Item().Element(ComposeDatosContribuyente);
            col.Item().Element(ComposeEmpresaYPeriodo);
            col.Item().Element(ComposeSeccionA);
            col.Item().Element(ComposeSeccionB);
            col.Item().Element(ComposeSeccionCDE);
            col.Item().Element(ComposeSeccionF);
            col.Item().Element(ComposeSeccionG);
            col.Item().Element(ComposeSeccionH);
            col.Item().Element(ComposeSeccionI);
            col.Item().Element(ComposeSeccionJ);
            if (model.EsVariacion) col.Item().Element(ComposeSeccionK);
            col.Item().PaddingTop(4).Text("Documento generado automáticamente por el empleado a través del portal Comunik2. No requiere firma.")
                .FontSize(7).Italic().FontColor(LabelColor);
        });
    }

    // ─── Helpers ──────────────────────────────────────────────────────────────

    private static Action<IContainer> SectionTitle(string title) =>
        c => c.Background(GrayBg).Border(1).BorderColor(BorderColor)
              .Padding(4).Text(title).Bold().FontSize(8.5f);

    private static void FormField(IContainer container, string label, string value)
    {
        container.Column(col =>
        {
            col.Item().Text(label).Bold().FontSize(7).FontColor(LabelColor);
            col.Item().PaddingTop(2).BorderBottom(1).BorderColor(BorderColor).PaddingBottom(3)
               .Text(string.IsNullOrWhiteSpace(value) ? "—" : value);
        });
    }

    private static void ResultRow(IContainer container, string label, string valor, string unidad, bool destacado = false)
    {
        container.Background(destacado ? "#FDF3D8" : Colors.White).Border(1).BorderColor(BorderColor).Padding(5)
            .Row(row =>
            {
                row.RelativeItem().Text(label).FontSize(8);
                row.ConstantItem(80).AlignRight().Text(valor).Bold().FontSize(9);
                row.ConstantItem(40).AlignRight().Text(unidad).FontSize(8).FontColor(LabelColor);
            });
    }

    private static string N(decimal v) => v.ToString("N2", Culture);

    // ─── Encabezado ───────────────────────────────────────────────────────────

    private void ComposeHeader(IContainer container)
    {
        container.Border(1).BorderColor(BorderColor).Padding(8).Row(row =>
        {
            if (model.LogoEmpresa is { Length: > 0 })
            {
                row.ConstantItem(50).AlignMiddle().PaddingRight(6).Image(model.LogoEmpresa).FitWidth();
            }

            row.RelativeItem(3).Column(col =>
            {
                col.Item().Text("REPÚBLICA BOLIVARIANA DE VENEZUELA").Bold().FontSize(9);
                col.Item().Text("MINISTERIO DEL PODER POPULAR DE ECONOMÍA Y FINANZAS").FontSize(7);
                col.Item().Text("SENIAT — SERVICIO NACIONAL INTEGRADO DE ADMINISTRACIÓN ADUANERA Y TRIBUTARIA").FontSize(7);
            });
            row.RelativeItem(4).Column(col =>
            {
                col.Item().AlignCenter().Text("DETERMINACIÓN DEL PORCENTAJE DE RETENCIÓN").Bold().FontSize(9);
                col.Item().AlignCenter().Text("DE IMPUESTO SOBRE LA RENTA").Bold().FontSize(9);
                col.Item().PaddingTop(2).AlignCenter()
                   .Text("Aplicable sobre sueldos, salarios y demás remuneraciones cuando el enriquecimiento anual exceda de 1.000 U.T., a percibir por personas naturales residentes en el país.")
                   .FontSize(6.5f);
            });
            row.RelativeItem(1).AlignRight().Column(col =>
            {
                col.Item().Text("FORMULARIO").FontSize(7);
                col.Item().Text("AR - I").Bold().FontSize(12);
            });
        });
    }

    // ─── Datos del contribuyente ─────────────────────────────────────────────

    private void ComposeDatosContribuyente(IContainer container)
    {
        container.Column(col =>
        {
            col.Item().Element(SectionTitle("DATOS DEL CONTRIBUYENTE"));
            col.Item().PaddingTop(4).Row(row =>
            {
                row.RelativeItem(3).PaddingRight(10).Element(c => FormField(c, "1. APELLIDOS Y NOMBRES", model.EmpleadoNombre));
                row.RelativeItem(1).PaddingRight(10).Element(c => FormField(c, "2. CÉDULA DE IDENTIDAD", model.EmpleadoCi));
                row.RelativeItem(1).Element(c => FormField(c, "3. N° DE RIF DEL CONTRIBUYENTE", model.EmpleadoRif));
            });
        });
    }

    // ─── Empresa / periodo ───────────────────────────────────────────────────

    private void ComposeEmpresaYPeriodo(IContainer container)
    {
        container.Row(row =>
        {
            row.RelativeItem(2).PaddingRight(10).Element(c => FormField(c, "4. NOMBRE DE LA EMPRESA U ORGANISMO DONDE TRABAJA", EmpresaNombre));

            row.RelativeItem(2).PaddingRight(10).Column(col =>
            {
                col.Item().Text("5. SI ES VARIACIÓN MARQUE X EN EL MES QUE CORRESPONDA").Bold().FontSize(7).FontColor(LabelColor);
                col.Item().PaddingTop(2).Row(r =>
                {
                    foreach (var mes in MesesVariacion)
                    {
                        var marcado = model.EsVariacion && string.Equals(model.Mes, mes, StringComparison.OrdinalIgnoreCase);
                        r.RelativeItem().AlignCenter().Border(1).BorderColor(BorderColor)
                            .Background(marcado ? "#FDF3D8" : Colors.White).Padding(3)
                            .Column(inner =>
                            {
                                inner.Item().AlignCenter().Text(mes).FontSize(6.5f);
                                inner.Item().AlignCenter().Text(marcado ? "X" : "").Bold().FontSize(9);
                            });
                    }
                });
                if (!model.EsVariacion)
                    col.Item().PaddingTop(2).Text($"Declaración inicial — mes: {model.Mes}").FontSize(7).Italic();
            });

            row.RelativeItem(1).Element(c => FormField(c, "6. AÑO GRAVABLE", model.Anio.ToString()));
        });
    }

    // ─── A. Estimación de remuneraciones ────────────────────────────────────

    private void ComposeSeccionA(IContainer container)
    {
        container.Column(col =>
        {
            col.Item().Element(SectionTitle("A. ESTIMACIÓN DE LAS REMUNERACIONES POR PERCIBIR EN EL AÑO GRAVABLE"));
            col.Item().PaddingTop(4).Border(1).BorderColor(BorderColor).Table(table =>
            {
                table.ColumnsDefinition(c =>
                {
                    c.RelativeColumn(3);
                    c.RelativeColumn(1);
                });

                void Fila(string label, decimal valor)
                {
                    table.Cell().Border(1).BorderColor(BorderColor).Padding(5).Text(label).FontSize(7.5f);
                    table.Cell().Border(1).BorderColor(BorderColor).Padding(5).AlignRight().Text($"Bs. {N(valor)}").FontSize(8);
                }

                Fila($"Sueldo mensual ({N(model.SueldoMensual)}) + otros ingresos mensuales ({N(model.OtrosIngresosMensuales)}) × 12 meses", model.SubTotal1);
                Fila("Utilidades / aguinaldos estimados a percibir en el año", model.SubTotal2);
                Fila($"Bono vacacional: {model.DiasBonoVacacional} días × sueldo diario (Bs. {N(model.SueldoDiario)})", model.SubTotal3);
            });
            col.Item().PaddingTop(4).Element(c => ResultRow(c, "TOTAL QUE ESTIMA PERCIBIR EN EL AÑO GRAVABLE", $"Bs. {N(model.TotalA)}", "(A)", true));
        });
    }

    // ─── B. Conversión a U.T. ────────────────────────────────────────────────

    private void ComposeSeccionB(IContainer container)
    {
        container.Column(col =>
        {
            col.Item().Element(SectionTitle("B. CONVERSIÓN DE LAS REMUNERACIONES ESTIMADAS EN (A.) A UNIDADES TRIBUTARIAS (U.T.)"));
            col.Item().PaddingTop(4).Row(row =>
            {
                row.RelativeItem().Text(t =>
                {
                    t.Span("Total remuneraciones (A): ").FontSize(8);
                    t.Span($"Bs. {N(model.TotalA)}").Bold().FontSize(8);
                    t.Span("   ÷   Valor U.T.: ").FontSize(8);
                    t.Span($"Bs. {N(model.ValorUT)}").Bold().FontSize(8);
                });
            });
            col.Item().PaddingTop(4).Element(c => ResultRow(c, "TOTAL REMUNERACIONES ESTIMADAS EN UNIDADES TRIBUTARIAS", N(model.TotalB_UT), "(B) U.T.", true));
        });
    }

    // ─── C/D/E. Desgravámenes ────────────────────────────────────────────────

    private void ComposeSeccionCDE(IContainer container)
    {
        container.Column(col =>
        {
            if (model.DesgravamenTipo == "Unico")
            {
                col.Item().Element(SectionTitle("E. DESGRAVAMEN ÚNICO (ART. 62 DE LA LEY)"));
                col.Item().PaddingTop(4).Element(c => ResultRow(c, "MONTO FIJO DEL DESGRAVAMEN ÚNICO", N(model.DesgravamenAplicado_UT), "(E) U.T.", true));
            }
            else
            {
                col.Item().Element(SectionTitle("C/D. DESGRAVÁMENES ESTIMADOS EN EL AÑO GRAVABLE (EN U.T.)"));
                col.Item().PaddingTop(4).Border(1).BorderColor(BorderColor).Table(table =>
                {
                    table.ColumnsDefinition(c =>
                    {
                        c.RelativeColumn(3);
                        c.RelativeColumn(1);
                    });

                    void Fila(string label, decimal valor)
                    {
                        table.Cell().Border(1).BorderColor(BorderColor).Padding(5).Text(label).FontSize(7.5f);
                        table.Cell().Border(1).BorderColor(BorderColor).Padding(5).AlignRight().Text($"{N(valor)} U.T.").FontSize(8);
                    }

                    Fila("1. Institutos docentes por la educación del contribuyente y descendientes no mayores de 25 años", model.InstitutosDocentes);
                    Fila("2. Primas de seguro de hospitalización, cirugía y maternidad", model.PrimasSeguro);
                    Fila("3. Servicios médicos odontológicos y de hospitalización (incluye carga familiar)", model.ServiciosMedicos);
                    Fila("4. Intereses por adquisición de vivienda principal o alquiler de la misma", model.InteresesVivienda);
                });
                col.Item().PaddingTop(4).Element(c => ResultRow(c, "TOTAL DESGRAVÁMENES ESTIMADOS", N(model.DesgravamenAplicado_UT), "(D) U.T.", true));
            }
        });
    }

    // ─── F. Renta gravable ───────────────────────────────────────────────────

    private void ComposeSeccionF(IContainer container)
    {
        container.Column(col =>
        {
            col.Item().Element(SectionTitle("F. DETERMINACIÓN DE LA RENTA GRAVABLE"));
            col.Item().PaddingTop(4).Text(t =>
            {
                t.Span("Remuneraciones (B): ").FontSize(8);
                t.Span($"{N(model.TotalB_UT)} U.T.").Bold().FontSize(8);
                t.Span("   −   Desgravámenes (D o E): ").FontSize(8);
                t.Span($"{N(model.DesgravamenAplicado_UT)} U.T.").Bold().FontSize(8);
            });
            col.Item().PaddingTop(4).Element(c => ResultRow(c, "RENTA GRAVABLE", N(model.RentaGravable_UT), "(F) U.T.", true));
        });
    }

    // ─── G. Impuesto estimado ────────────────────────────────────────────────

    private void ComposeSeccionG(IContainer container)
    {
        container.Column(col =>
        {
            col.Item().Element(SectionTitle("G. CÁLCULO DEL IMPUESTO ESTIMADO PARA EL AÑO GRAVABLE (TARIFA N° 1)"));
            col.Item().PaddingTop(4).Text(t =>
            {
                t.Span($"Renta gravable (F) {N(model.RentaGravable_UT)} U.T. × {model.TramoPorcentaje:P0} − sustraendo {N(model.TramoSustraendo_UT)} U.T.").FontSize(8);
            });
            col.Item().PaddingTop(4).Element(c => ResultRow(c, "TOTAL IMPUESTO DEL AÑO GRAVABLE", N(model.ImpuestoG_UT), "(G) U.T.", true));
        });
    }

    // ─── H. Rebajas ──────────────────────────────────────────────────────────

    private void ComposeSeccionH(IContainer container)
    {
        container.Column(col =>
        {
            col.Item().Element(SectionTitle("H. REBAJAS AL IMPUESTO DETERMINADO EN (G.)"));
            col.Item().PaddingTop(4).Border(1).BorderColor(BorderColor).Table(table =>
            {
                table.ColumnsDefinition(c =>
                {
                    c.RelativeColumn(3);
                    c.RelativeColumn(1);
                });

                void Fila(string label, decimal valor)
                {
                    table.Cell().Border(1).BorderColor(BorderColor).Padding(5).Text(label).FontSize(7.5f);
                    table.Cell().Border(1).BorderColor(BorderColor).Padding(5).AlignRight().Text($"{N(valor)} U.T.").FontSize(8);
                }

                Fila("1. Rebaja personal", 10m);
                Fila($"2. Carga familiar (ver instructivo): {model.CargaFamiliar} × 10 U.T.", model.RebajaCargaFamiliar_UT);
                Fila($"3. Impuestos retenidos de más en años anteriores (Bs. {N(model.ImpuestosRetenidosAnterioresBs)} ÷ valor U.T.)", model.RebajaImpuestosAnteriores_UT);
            });
            col.Item().PaddingTop(4).Element(c => ResultRow(c, "TOTAL REBAJAS (1 + 2 + 3)", N(model.TotalH_UT), "(H) U.T.", true));
            if (model.CargaFamiliar == 0)
                col.Item().PaddingTop(2).Text("⚠ Carga familiar no ingresada aún — pendiente por actualizar.").FontSize(7).Italic().FontColor(LabelColor);
        });
    }

    // ─── I. Impuesto a retener ───────────────────────────────────────────────

    private void ComposeSeccionI(IContainer container)
    {
        container.Column(col =>
        {
            col.Item().Element(SectionTitle("I. IMPUESTO (ESTIMADO) A RETENER EN EL AÑO GRAVABLE  —  (G) − (H)"));
            col.Item().PaddingTop(4).Element(c => ResultRow(c, "IMPUESTO ESTIMADO A RETENER", N(model.ImpuestoI_UT), "(I) U.T.", true));
        });
    }

    // ─── J. Porcentaje de retención inicial ─────────────────────────────────

    private void ComposeSeccionJ(IContainer container)
    {
        container.Column(col =>
        {
            col.Item().Element(SectionTitle("J. PORCENTAJE DE RETENCIÓN INICIAL"));
            col.Item().PaddingTop(4).Text(t =>
            {
                t.Span("(J) % = Total casilla (I) ÷ Total casilla (B) × 100 = ").FontSize(8);
                t.Span($"{N(model.ImpuestoI_UT)} ÷ {N(model.TotalB_UT)} × 100").FontSize(8);
            });
            col.Item().PaddingTop(4).Element(c => ResultRow(c, "PORCENTAJE DE RETENCIÓN INICIAL", $"{N(model.PorcentajeJ)} %", "(J)", true));
        });
    }

    // ─── K. Porcentaje por variación ─────────────────────────────────────────

    private void ComposeSeccionK(IContainer container)
    {
        container.Column(col =>
        {
            col.Item().Element(SectionTitle("K. PORCENTAJE POR VARIACIÓN EN LOS DATOS APLICABLE POR EL RESTO DEL AÑO GRAVABLE"));
            col.Item().PaddingTop(4).Row(row =>
            {
                row.RelativeItem().PaddingRight(10).Element(c => FormField(c, "1. TOTAL DEL IMPUESTO RETENIDO HASTA LA FECHA", $"Bs. {N(model.ImpuestoRetenidoHastaFechaBs ?? 0)}"));
                row.RelativeItem().Element(c => FormField(c, "2. TOTAL REMUNERACIONES PERCIBIDAS HASTA LA FECHA", $"Bs. {N(model.RemuneracionesPercibidasHastaFechaBs ?? 0)}"));
            });
            col.Item().PaddingTop(4).Text(t =>
            {
                t.Span("(K) % = [Total (I) × Valor U.T. − Total (1)] ÷ [Total (A) − Total (2)] × 100").FontSize(8);
            });
            col.Item().PaddingTop(4).Element(c => ResultRow(c, "PORCENTAJE POR VARIACIÓN", $"{N(model.PorcentajeK ?? 0)} %", "(K)", true));
        });
    }
}
