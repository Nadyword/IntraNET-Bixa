using Bixa.Backend.Models.DTOs.ReportesModelDTO;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Bixa.Backend.Services.Templates;

public class DiaEspecialDocument(TramiteReportModel model) : IDocument
{
    private const string BorderColor = "#333333";
    private const string GrayBg = "#EEEEEE";
    private const string LabelColor = "#555555";

    public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

    public void Compose(IDocumentContainer container)
    {
        container.Page(page =>
        {
            page.Size(PageSizes.A4);
            page.Margin(2, Unit.Centimetre);
            page.DefaultTextStyle(x => x.FontSize(10).FontFamily(Fonts.Arial));
            page.Content().Element(ComposeContent);
        });
    }

    // ─── Estructura principal ─────────────────────────────────────────────────

    private static Action<IContainer> SectionTitle(string title) =>
        c => c.Background(GrayBg).Border(1).BorderColor(BorderColor)
              .Padding(8).AlignCenter()
              .Text(title).Bold().FontSize(10);

    // ─── Helpers ──────────────────────────────────────────────────────────────

    private static void FormField(IContainer container, string label, string value)
    {
        container.Column(col =>
        {
            col.Item().Text(label).Bold().FontSize(8).FontColor(LabelColor);
            col.Item().PaddingTop(4).BorderBottom(1).BorderColor(BorderColor).PaddingBottom(5)
               .Text(value);
        });
    }

    private void ComposeContent(IContainer container)
    {
        container.Column(col =>
        {
            col.Item().Element(ComposeHeader);
            col.Item().PaddingTop(18).Element(ComposeDatosSolicitante);
            col.Item().PaddingTop(20).Element(ComposeDetalleSolicitud);
        });
    }

    // ─── Encabezado (3 columnas con borde) ───────────────────────────────────

    private void ComposeHeader(IContainer container)
    {
        container.Border(2).BorderColor(BorderColor).Row(row =>
        {
            // Logo
            row.RelativeItem(1).BorderRight(1).BorderColor(BorderColor).Padding(10).Column(col =>
            {
                if (model.LogoEmpresa is { Length: > 0 })
                    col.Item().PaddingHorizontal(20).AlignCenter().Image(model.LogoEmpresa).FitWidth();
                else
                    col.Item().AlignCenter().Text("BIXA").Bold().FontSize(14);
            });

            // Título
            row.RelativeItem(2).BorderRight(1).BorderColor(BorderColor).Padding(10).Column(col =>
            {
                col.Item().AlignCenter().Text("REGISTRO").Bold().FontSize(12);
                col.Item().Height(10);
                col.Item().AlignCenter().Text("SOLICITUD DE DIA ESPECIAL").Bold().FontSize(12);
            });

            // Metadatos
            row.RelativeItem(1).Padding(10).Column(col =>
            {
                col.Item().BorderBottom(1).BorderColor(GrayBg).PaddingBottom(4)
                   .Text(t => { t.Span("Código: ").Bold().FontSize(8); t.Span("5-SDE-TH-001").FontSize(8); });
                col.Item().PaddingTop(4).BorderBottom(1).BorderColor(GrayBg).PaddingBottom(4)
                   .Text(t => { t.Span("Revisión: ").Bold().FontSize(8); t.Span("00").FontSize(8); });
                col.Item().PaddingTop(4)
                   .Text(t => { t.Span("Página: ").Bold().FontSize(8); t.Span("1/1").FontSize(8); });
            });
        });
    }

    // ─── Datos del Solicitante ────────────────────────────────────────────────

    private void ComposeDatosSolicitante(IContainer container)
    {
        container.Column(col =>
        {
            col.Item().Element(SectionTitle("DATOS DEL SOLICITANTE"));
            col.Spacing(15);

            col.Item().Row(row =>
            {
                row.RelativeItem(2).PaddingRight(15).Element(c => FormField(c, "NOMBRE Y APELLIDOS", model.EmpleadoNombre));
                row.RelativeItem(1).Element(c => FormField(c, "CI", model.EmpleadoCi));
            });

            col.Item().Row(row =>
            {
                row.RelativeItem(1).PaddingRight(15).Element(c => FormField(c, "CARGO", model.EmpleadoCargo ?? "—"));
                row.RelativeItem(1).Element(c => FormField(c, "DEPARTAMENTO", model.EmpleadoDepartamento ?? "—"));
            });
        });
    }

    // ─── Detalle de la solicitud ──────────────────────────────────────────────

    private void ComposeDetalleSolicitud(IContainer container)
    {
        container.Border(1).BorderColor(BorderColor).Padding(15).Column(col =>
        {
            col.Spacing(10);

            col.Item().Text(t =>
            {
                t.Span("FECHA SOLICITADA: ").Bold();
                t.Span(model.DiaEspecial?.Fecha.ToString("dd/MM/yyyy") ?? "—");
            });

            col.Item().Text(t =>
            {
                t.Span("TOTAL DIAS SOLICITADOS: ").Bold();
                t.Span("1");
            });

            col.Item().Text(t =>
            {
                t.Span("DILIGENCIA A REALIZAR: ").Bold();
                t.Span(model.DiaEspecial?.Motivo ?? "—");
            });
        });
    }
}
