using Bixa.Backend.Models.DTOs.ReportesModelDTO;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Bixa.Backend.Services.Templates;

public class GenericTramiteDocument(TramiteReportModel model) : IDocument
{
    private static readonly string _accentColor = "#C43625";
    private static readonly string _lightGray = "#F5F5F5";
    private static readonly string _textGray = "#666666";

    public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

    public void Compose(IDocumentContainer container)
    {
        container.Page(page =>
        {
            page.Size(PageSizes.A4);
            page.Margin(2, Unit.Centimetre);
            page.DefaultTextStyle(x => x.FontSize(10).FontFamily(Fonts.Arial));

            page.Header().Element(ComposeHeader);
            page.Content().Element(ComposeContent);
            page.Footer().Element(ComposeFooter);
        });
    }

    // ─── Header ───────────────────────────────────────────────────────────────

    private void ComposeHeader(IContainer container)
    {
        container.Column(col =>
        {
            col.Item().Row(row =>
            {
                if (model.LogoEmpresa is { Length: > 0 })
                {
                    row.ConstantItem(80).Image(model.LogoEmpresa).FitArea();
                }

                row.RelativeItem().Column(inner =>
                {
                    inner.Item()
                         .Text("IntraNET Bixa")
                         .FontSize(20).Bold().FontColor(_accentColor);

                    inner.Item()
                         .Text("Comprobante de Trámite")
                         .FontSize(13).FontColor(_textGray);
                });

                row.ConstantItem(120).AlignRight().Column(inner =>
                {
                    inner.Item().Text($"Trámite #{model.TramiteId}").Bold();
                    inner.Item().Text(DateTime.Now.ToString("dd/MM/yyyy")).FontColor(_textGray);
                });
            });

            col.Item().PaddingTop(8)
                      .LineHorizontal(2).LineColor(_accentColor);
        });
    }

    // ─── Content ──────────────────────────────────────────────────────────────

    private void ComposeContent(IContainer container)
    {
        container.Column(col =>
        {
            col.Spacing(16);

            col.Item().Element(ComposeDatosTramite);

            col.Item().Element(ComposeDatosEmpleado);

            if (model.Vacaciones is not null)
                col.Item().Element(ComposeDetalleVacaciones);

            if (model.DiaEspecial is not null)
                col.Item().Element(ComposeDetalleDiaEspecial);

            if (model.Aprobaciones.Count > 0)
                col.Item().Element(ComposeCadenaAprobacion);
        });
    }

    private void ComposeDatosTramite(IContainer container)
    {
        container.Column(col =>
        {
            col.Item().Element(SectionTitle("Datos del Trámite"));

            col.Item().Background(_lightGray).Padding(12).Table(table =>
            {
                table.ColumnsDefinition(c => { c.RelativeColumn(); c.RelativeColumn(); });

                DataRow(table, "Tipo de Trámite", model.TipoTramite);
                DataRow(table, "Estado", model.EstadoFinal);
                DataRow(table, "Fecha Solicitud", model.FechaSolicitud.ToString("dd/MM/yyyy"));
                DataRow(table, "Fecha Resolución", model.FechaResolucion.ToString("dd/MM/yyyy"));
            });
        });
    }

    private void ComposeDatosEmpleado(IContainer container)
    {
        container.Column(col =>
        {
            col.Item().Element(SectionTitle("Datos del Empleado"));

            col.Item().Background(_lightGray).Padding(12).Table(table =>
            {
                table.ColumnsDefinition(c => { c.RelativeColumn(); c.RelativeColumn(); });

                DataRow(table, "CI", model.EmpleadoCi);
                DataRow(table, "Nombre", model.EmpleadoNombre);
            });
        });
    }

    private void ComposeDetalleVacaciones(IContainer container)
    {
        var vac = model.Vacaciones!;

        container.Column(col =>
        {
            col.Item().Element(SectionTitle("Detalle de Vacaciones"));

            col.Item().Background(_lightGray).Padding(12).Table(table =>
            {
                table.ColumnsDefinition(c => { c.RelativeColumn(); c.RelativeColumn(); });

                DataRow(table, "Desde", vac.Desde.ToString("dd/MM/yyyy"));
                DataRow(table, "Hasta", vac.Hasta.ToString("dd/MM/yyyy"));
                DataRow(table, "Días Totales", vac.DiasTotales.ToString());
                DataRow(table, "Observaciones", vac.Observaciones ?? "—");
            });
        });
    }

    private void ComposeDetalleDiaEspecial(IContainer container)
    {
        var dia = model.DiaEspecial!;

        container.Column(col =>
        {
            col.Item().Element(SectionTitle("Detalle de Día Especial"));

            col.Item().Background(_lightGray).Padding(12).Table(table =>
            {
                table.ColumnsDefinition(c => { c.RelativeColumn(); c.RelativeColumn(); });

                DataRow(table, "Fecha", dia.Fecha.ToString("dd/MM/yyyy"));
                DataRow(table, "Motivo", dia.Motivo);
            });
        });
    }

    private void ComposeCadenaAprobacion(IContainer container)
    {
        container.Column(col =>
        {
            col.Item().Element(SectionTitle("Cadena de Aprobación"));

            col.Item().Table(table =>
            {
                table.ColumnsDefinition(c =>
                {
                    c.RelativeColumn(2);
                    c.RelativeColumn(2);
                    c.RelativeColumn(1);
                    c.RelativeColumn(1);
                    c.RelativeColumn(2);
                });

                // Encabezado de tabla
                var headerStyle = TextStyle.Default.Bold().FontColor(Colors.White);
                table.Header(h =>
                {
                    h.Cell().Background(_accentColor).Padding(6).Text("Aprobador").Style(headerStyle);
                    h.Cell().Background(_accentColor).Padding(6).Text("Nombre").Style(headerStyle);
                    h.Cell().Background(_accentColor).Padding(6).Text("Acción").Style(headerStyle);
                    h.Cell().Background(_accentColor).Padding(6).Text("Fecha").Style(headerStyle);
                    h.Cell().Background(_accentColor).Padding(6).Text("Motivo").Style(headerStyle);
                });

                foreach (var (aprobacion, index) in model.Aprobaciones.Select((a, i) => (a, i)))
                {
                    var bg = index % 2 != 0 ? _lightGray : "#FFFFFF";

                    table.Cell().Background(bg).Padding(6).Text(aprobacion.AprobadorCi);
                    table.Cell().Background(bg).Padding(6).Text(aprobacion.AprobadorNombre);
                    table.Cell().Background(bg).Padding(6).Text(aprobacion.Accion)
                         .FontColor(aprobacion.Accion == "Rechazó" ? "#C43625" : "#1E8449");
                    table.Cell().Background(bg).Padding(6).Text(aprobacion.Fecha.ToString("dd/MM/yyyy"));
                    table.Cell().Background(bg).Padding(6).Text(aprobacion.Motivo ?? "—").FontColor(_textGray);
                }
            });
        });
    }

    // ─── Footer ───────────────────────────────────────────────────────────────

    private void ComposeFooter(IContainer container)
    {
        container.Column(col =>
        {
            col.Item().LineHorizontal(1).LineColor(_lightGray);

            col.Item().PaddingTop(4).Row(row =>
            {
                row.RelativeItem()
                   .Text($"Generado por IntraNET Bixa · {DateTime.Now:dd/MM/yyyy HH:mm}")
                   .FontSize(8).FontColor(_textGray);

                row.ConstantItem(60).AlignRight()
                   .Text(x =>
                   {
                       x.Span("Página ").FontSize(8).FontColor(_textGray);
                       x.CurrentPageNumber().FontSize(8).FontColor(_textGray);
                       x.Span(" de ").FontSize(8).FontColor(_textGray);
                       x.TotalPages().FontSize(8).FontColor(_textGray);
                   });
            });
        });
    }

    // ─── Helpers ──────────────────────────────────────────────────────────────

    private static Action<IContainer> SectionTitle(string title) =>
        c => c.PaddingBottom(6)
              .Text(title)
              .FontSize(12).Bold().FontColor("#333333");

    private static void DataRow(TableDescriptor table, string label, string value)
    {
        table.Cell().Padding(4).Text(label).Bold().FontColor(_textGray);
        table.Cell().Padding(4).Text(value);
    }
}