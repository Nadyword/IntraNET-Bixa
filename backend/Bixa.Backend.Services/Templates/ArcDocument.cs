using System.Globalization;
using Bixa.Backend.Models.DTOs.ReportesModelDTO;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Bixa.Backend.Services.Templates;

public class ArcDocument(ArcReportModel model) : IDocument
{
    private static readonly CultureInfo Culture = CultureInfo.GetCultureInfo("es-VE");

    private const string BorderColor = "#333333";
    private const string GrayBg = "#EEEEEE";

    private const string EmpresaNombre = "PRODUCTOS BIXA, S.A.";
    private const string EmpresaRif = "J-00220504-0";
    private const string EmpresaDireccion =
        "Av. Libertador con Av. Francisco de Miranda Edif. Centro Empresarial del Este, Conjunto Miranda, " +
        "Edif. Miranda, Torre B, Piso 10, Ofic. B-104, Urb. Chacao, Caracas, Miranda.";

    public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

    public void Compose(IDocumentContainer container)
    {
        container.Page(page =>
        {
            page.Size(PageSizes.A4);
            page.Margin(2, Unit.Centimetre);
            page.DefaultTextStyle(x => x.FontSize(9).FontFamily(Fonts.Arial));
            page.Content().Element(ComposeContent);
        });
    }

    private void ComposeContent(IContainer container)
    {
        container.Column(col =>
        {
            col.Item().Element(ComposeHeader);
            col.Item().PaddingTop(16).Element(ComposeBeneficiario);
            col.Item().PaddingTop(8).Element(ComposeAgenteRetencion);
            col.Item().PaddingTop(8).Element(ComposeDireccion);
            col.Item().PaddingTop(16).Element(ComposeTabla);
            col.Item().PaddingTop(45).Element(ComposeFirma);
        });
    }

    // ─── Encabezado ────────────────────────────────────────────────────────────

    private void ComposeHeader(IContainer container)
    {
        container.Row(row =>
        {
            row.ConstantItem(80).AlignMiddle().Column(col =>
            {
                if (model.LogoEmpresa is { Length: > 0 })
                    col.Item().Image(model.LogoEmpresa).FitWidth();
                else
                    col.Item().AlignCenter().Text("BIXA").Bold().FontSize(14);
            });

            row.RelativeItem().AlignMiddle().Column(col =>
            {
                col.Item().AlignCenter().Text("* COMPROBANTE DE RETENCIÓN (ARC) *").Bold().FontSize(11);
                col.Item().AlignCenter()
                   .Text("De Impuesto sobre la renta o de cese de actividades para personas residentes y")
                   .FontSize(8);
                col.Item().AlignCenter().Text("receptoras de sueldos y demás remuneraciones de salariales").FontSize(8);
                col.Item().PaddingTop(2).AlignCenter().Text("Período Enero - Diciembre").Bold().FontSize(9);
            });

            row.ConstantItem(90).Column(col =>
            {
                col.Item().AlignRight().Text(t =>
                {
                    t.Span("Año: ").Bold().FontSize(9);
                    t.Span(model.Anio.ToString()).FontSize(9);
                });
            });
        });
    }

    // ─── Beneficiario ─────────────────────────────────────────────────────────

    private void ComposeBeneficiario(IContainer container)
    {
        container.Column(col =>
        {
            col.Item().Text("BENEFICIARIO DE RETENCIÓN").Bold().FontSize(9);
            col.Item().PaddingTop(4).Row(row =>
            {
                row.RelativeItem(2).Text(t =>
                {
                    t.Span("Nombre y Apellido: ").Bold();
                    t.Span(model.EmpleadoNombre);
                });
                row.RelativeItem(1).Text(t =>
                {
                    t.Span("Cédula de Identidad: ").Bold();
                    t.Span(model.EmpleadoCi);
                });
                row.RelativeItem(1).Text(t =>
                {
                    t.Span("Nro Rif Empleado: ").Bold();
                    t.Span(model.EmpleadoRif);
                });
            });
        });
    }

    // ─── Agente de retención ──────────────────────────────────────────────────

    private void ComposeAgenteRetencion(IContainer container)
    {
        container.Column(col =>
        {
            col.Item().Text("TIPO DE AGENTE DE RETENCIÓN").Bold().FontSize(9);
            col.Item().PaddingTop(4).Row(row =>
            {
                row.RelativeItem(2).Text(t =>
                {
                    t.Span("Persona Jurídica: ").Bold();
                    t.Span(EmpresaNombre);
                });
                row.RelativeItem(1).Text(t =>
                {
                    t.Span("Nro RIF.:Empresa: ").Bold();
                    t.Span(EmpresaRif);
                });
            });
        });
    }

    private void ComposeDireccion(IContainer container)
    {
        container.Column(col =>
        {
            col.Item().Text("DIRECCIÓN DEL AGENTE DE RETENCIÓN").Bold().FontSize(9);
            col.Item().PaddingTop(4).Text(EmpresaDireccion).FontSize(8);
        });
    }

    // ─── Tabla de meses ───────────────────────────────────────────────────────

    private void ComposeTabla(IContainer container)
    {
        container.Border(1).BorderColor(BorderColor).Table(table =>
        {
            table.ColumnsDefinition(c =>
            {
                c.RelativeColumn(1.2f);
                c.RelativeColumn(1.6f);
                c.RelativeColumn(1.2f);
                c.RelativeColumn(1.2f);
                c.RelativeColumn(1.8f);
                c.RelativeColumn(1.4f);
            });

            var headerStyle = TextStyle.Default.Bold().FontSize(7);
            table.Header(h =>
            {
                h.Cell().Border(1).BorderColor(BorderColor).Background(GrayBg).Padding(4).AlignCenter()
                    .Text("MESES").Style(headerStyle);
                h.Cell().Border(1).BorderColor(BorderColor).Background(GrayBg).Padding(4).AlignCenter()
                    .Text("REMUNERACIONES PAGADAS ABONADAS EN CUENTAS").Style(headerStyle);
                h.Cell().Border(1).BorderColor(BorderColor).Background(GrayBg).Padding(4).AlignCenter()
                    .Text("PORCENTAJE DE RETENCIÓN").Style(headerStyle);
                h.Cell().Border(1).BorderColor(BorderColor).Background(GrayBg).Padding(4).AlignCenter()
                    .Text("IMPUESTO RETENIDO").Style(headerStyle);
                h.Cell().Border(1).BorderColor(BorderColor).Background(GrayBg).Padding(4).AlignCenter()
                    .Text("REMUNERACIONES PAGADAS O ABONADAS EN CUENTAS ACUMULADAS").Style(headerStyle);
                h.Cell().Border(1).BorderColor(BorderColor).Background(GrayBg).Padding(4).AlignCenter()
                    .Text("IMPUESTO RETENIDO ACUMULADO").Style(headerStyle);
            });

            foreach (var mes in model.Meses)
            {
                table.Cell().Border(1).BorderColor(BorderColor).Padding(4).AlignCenter().Text(mes.Mes).FontSize(8);
                table.Cell().Border(1).BorderColor(BorderColor).Padding(4).AlignRight()
                    .Text(mes.Remuneracion.ToString("N2", Culture)).FontSize(8);
                table.Cell().Border(1).BorderColor(BorderColor).Padding(4).AlignRight()
                    .Text(mes.PorcentRetencion?.ToString("N2", Culture) ?? string.Empty).FontSize(8);
                table.Cell().Border(1).BorderColor(BorderColor).Padding(4).AlignRight()
                    .Text(mes.ImpuestoRetenido?.ToString("N2", Culture) ?? string.Empty).FontSize(8);
                table.Cell().Border(1).BorderColor(BorderColor).Padding(4).AlignRight()
                    .Text(mes.RemuneracionAcumulada.ToString("N2", Culture)).FontSize(8);
                table.Cell().Border(1).BorderColor(BorderColor).Padding(4).AlignRight()
                    .Text(mes.ImpuestoRetenidoAcum?.ToString("N2", Culture) ?? string.Empty).FontSize(8);
            }
        });
    }

    // ─── Firma y sello ────────────────────────────────────────────────────────

    private void ComposeFirma(IContainer container)
    {
        container.Row(row =>
        {
            row.RelativeItem();
            row.ConstantItem(220).Column(col =>
            {
                col.Item().LineHorizontal(1).LineColor(BorderColor);
                col.Item().PaddingTop(2).Element(c =>
                {
                    if (model.FirmaSelloAgente is { Length: > 0 })
                        c.Height(75).AlignCenter().Image(model.FirmaSelloAgente).FitHeight();
                });
            });
            row.RelativeItem();
        });
    }
}
