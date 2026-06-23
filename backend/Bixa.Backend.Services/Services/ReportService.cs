using Bixa.Backend.Models.DTOs.ReportesModelDTO;
using Bixa.Backend.Services.Interfaces;
using Bixa.Backend.Services.Templates;
using QuestPDF.Infrastructure;
using QuestPDF.Fluent;

namespace Bixa.Backend.Services.Services;

public class ReportService(string reportAssetsPath) : IReportService
{
    private readonly string _assetsPath = reportAssetsPath;

    // ─── Generación ───────────────────────────────────────────────────────────

    public byte[] GenerateTramiteReport(TramiteReportModel model, string plantilla = "Generic")
    {
        QuestPDF.Settings.License = LicenseType.Community;

        if (model.LogoEmpresa is not { Length: > 0 })
        {
            var logoPath = Path.Combine(_assetsPath, "Logo.webp");
            if (File.Exists(logoPath))
                model.LogoEmpresa = File.ReadAllBytes(logoPath);
        }

        return plantilla switch
        {
            "Generic"    => new GenericTramiteDocument(model).GeneratePdf(),
            "Vacaciones" => new VacacionesDocument(model).GeneratePdf(),
            _            => new GenericTramiteDocument(model).GeneratePdf(),
        };
    }

    // ─── Gestión de imágenes ──────────────────────────────────────────────────

    public async Task<string> SaveReportImageAsync(Stream imageStream, string fileName)
    {
        Directory.CreateDirectory(_assetsPath);

        var safeName = Path.GetFileNameWithoutExtension(fileName)
                            .Replace(" ", "_")
                        + Path.GetExtension(fileName).ToLowerInvariant();

        var fullPath = Path.Combine(_assetsPath, safeName);

        await using var file = File.Create(fullPath);
        await imageStream.CopyToAsync(file);

        return safeName;
    }

    public IEnumerable<string> GetReportImages()
    {
        if (!Directory.Exists(_assetsPath))
            return [];

        return Directory
            .GetFiles(_assetsPath, "*.*")
            .Where(f => new[] { ".png", ".jpg", ".jpeg", ".svg" }
                .Contains(Path.GetExtension(f).ToLowerInvariant()))
            .Select(Path.GetFileName)
            .Where(n => n is not null)
            .Cast<string>();
    }

    public void DeleteReportImage(string fileName)
    {
        var fullPath = Path.Combine(_assetsPath, Path.GetFileName(fileName));

        if (File.Exists(fullPath))
            File.Delete(fullPath);
    }
}