using Bixa.Backend.Models.DTOs.ReportesModelDTO;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Bixa.Backend.Services.Templates;

public class UtilidadesDocument(TramiteReportModel model) : IDocument
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

    private static void SignatureBox(IContainer container, string role, string name, string date, byte[]? firma)
    {
        container.Column(col =>
        {
            if (firma is { Length: > 0 })
                col.Item().Height(50).AlignCenter().Image(firma).FitHeight();
            else
                col.Item().Height(50);
            col.Item().LineHorizontal(1).LineColor(BorderColor);
            col.Item().PaddingTop(8).AlignCenter().Text(role).Bold().FontSize(9);
            if (!string.IsNullOrEmpty(name))
                col.Item().AlignCenter().Text(name).FontSize(8).FontColor(LabelColor);
            col.Item().AlignCenter().Text($"Fecha: {date}").FontSize(8);
        });
    }

    private void ComposeContent(IContainer container)
    {
        container.Column(col =>
        {
            col.Item().Element(ComposeHeader);

            col.Item().PaddingTop(10).Row(row =>
            {
                row.RelativeItem();
                row.AutoItem().Text("FECHA DE LA SOLICITUD:").Bold().FontSize(9).FontColor(LabelColor);
                row.ConstantItem(8);
                row.ConstantItem(150).BorderBottom(1).BorderColor(BorderColor).PaddingBottom(3)
                   .Text(model.FechaSolicitud.ToString("dd/MM/yyyy"));
            });

            col.Item().PaddingTop(18).Element(ComposeDatosSolicitante);
            col.Item().PaddingTop(20).Element(ComposeDetalleSolicitud);
            col.Item().PaddingTop(25).Element(ComposeFirmas);
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
                col.Item().AlignCenter().Text("SOLICITUD DE ANTICIPO DE UTILIDADES").Bold().FontSize(12);
            });

            // Metadatos
            row.RelativeItem(1).Padding(10).Column(col =>
            {
                col.Item().BorderBottom(1).BorderColor(GrayBg).PaddingBottom(4)
                   .Text(t => { t.Span("Código: ").Bold().FontSize(8); t.Span("5-SAU-TH-001").FontSize(8); });
                col.Item().PaddingTop(4).BorderBottom(1).BorderColor(GrayBg).PaddingBottom(4)
                   .Text(t => { t.Span("Revisión: ").Bold().FontSize(8); t.Span("01").FontSize(8); });
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
            col.Item().Element(SectionTitle("DATOS DEL TRABAJADOR"));
            col.Spacing(15);

            col.Item().Row(row =>
            {
                row.RelativeItem(0.85f).PaddingRight(15).Element(c => FormField(c, "NOMBRE DEL TRABAJADOR", model.EmpleadoNombre));
                row.RelativeItem(0.15f).Element(c => FormField(c, "CÉDULA", model.EmpleadoCi));
            });

            col.Item().Row(row =>
            {
                row.RelativeItem(0.85f).PaddingRight(15).Column(inner =>
                {
                    inner.Spacing(10);
                    inner.Item().Element(c => FormField(c, "DEPARTAMENTO", model.EmpleadoDepartamento ?? "—"));
                    inner.Item().Element(c => FormField(c, "CARGO", model.EmpleadoCargo ?? "—"));
                });
                row.RelativeItem(0.15f).Element(c => FormField(c, "FECHA DE INGRESO", model.FechaIngreso?.ToString("dd/MM/yyyy") ?? "—"));
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
                t.Span("CANTIDAD SOLICITADA: ").Bold();
                t.Span(model.Utilidades?.Monto.ToString("N2") ?? "—");
            });

            col.Item().Text(t =>
            {
                t.Span("MOTIVO DE LA SOLICITUD: ").Bold();
                t.Span(model.Utilidades?.Motivo ?? "—");
            });

            col.Item().Text(t =>
            {
                t.Span("TOTAL UTILIDADES A LA FECHA: ").Bold();
                t.Span(model.Utilidades?.TotalUtilidadesDisponible.ToString("N2") ?? "—");
            });
        });
    }

    // ─── Firmas ───────────────────────────────────────────────────────────────

    private void ComposeFirmas(IContainer container)
    {
        string supNombre = model.Aprobaciones.Count > 0 ? model.Aprobaciones[0].AprobadorNombre : "";
        string supFecha = model.Aprobaciones.Count > 0 ? model.Aprobaciones[0].Fecha.ToString("dd/MM/yyyy") : "___/___/___";
        byte[]? supFirma = model.Aprobaciones.Count > 0 ? model.Aprobaciones[0].FirmaImagen : null;
        string rrhNombre = model.Aprobaciones.Count > 1 ? model.Aprobaciones[^1].AprobadorNombre : "";
        string rrhFecha = model.Aprobaciones.Count > 1 ? model.Aprobaciones[^1].Fecha.ToString("dd/MM/yyyy") : "___/___/___";
        byte[]? rrhFirma = model.Aprobaciones.Count > 1 ? model.Aprobaciones[^1].FirmaImagen : null;

        container.Column(col =>
        {
            col.Item().Element(SectionTitle("FIRMAS"));
            col.Item().PaddingTop(20).Row(row =>
            {
                row.RelativeItem().PaddingRight(20)
                   .Element(c => SignatureBox(c, "TRABAJADOR", model.EmpleadoNombre, model.FechaSolicitud.ToString("dd/MM/yyyy"), model.EmpleadoFirmaImagen));
                row.RelativeItem().PaddingRight(20)
                   .Element(c => SignatureBox(c, "SUPERVISOR", supNombre, supFecha, supFirma));
                row.RelativeItem()
                   .Element(c => SignatureBox(c, "RRHH", rrhNombre, rrhFecha, rrhFirma));
            });
        });
    }
}
