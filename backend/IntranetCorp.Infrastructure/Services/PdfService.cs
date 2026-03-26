using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace IntranetCorp.Infrastructure.Services;

public interface IPdfService
{
    byte[] GenerateTramitePdf(string titulo, string contenido, Dictionary<string, string> campos);
}

public class PdfService : IPdfService
{
    public PdfService()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public byte[] GenerateTramitePdf(string titulo, string contenido, Dictionary<string, string> campos)
    {
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(40);

                page.Header().Height(60).AlignCenter().AlignMiddle()
                    .Text(titulo).FontSize(24).Bold();

                page.Content().Column(column =>
                {
                    column.Spacing(10);

                    column.Item().Text(contenido).FontSize(12);

                    column.Spacing(20);

                    foreach (var campo in campos)
                    {
                        column.Item().Row(row =>
                        {
                            row.RelativeItem().Text(campo.Key + ":").Bold().FontSize(11);
                            row.RelativeItem().Text(campo.Value).FontSize(11);
                        });
                    }
                });

                page.Footer().AlignCenter().AlignBottom()
                    .Text($"Generado el {DateTime.Now:dd/MM/yyyy HH:mm}")
                    .FontSize(10);
            });
        }).GeneratePdf();
    }
}
