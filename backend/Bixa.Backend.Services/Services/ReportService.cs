using Bixa.Backend.Models.DTOs.ReportesModelDTO;
using Bixa.Backend.Services.Interfaces;
using Bixa.Backend.Services.Templates;
using QuestPDF.Infrastructure;
using QuestPDF.Fluent;

namespace Bixa.Backend.Services.Services;

public class ReportService(string reportAssetsPath, string firmasPath, string templatesPath) : IReportService
{
    private readonly string _assetsPath = reportAssetsPath;
    private readonly string _firmasPath = firmasPath;
    private readonly string _templatesPath = templatesPath;

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

        model.EmpleadoFirmaImagen = LoadFirmaImage(model.EmpleadoUrlFirma);
        foreach (var aprobacion in model.Aprobaciones)
        {
            aprobacion.FirmaImagen = LoadFirmaImage(UtilityService.NormalizeCiFormat(aprobacion.AprobadorCi) + ".png");
        }

        return plantilla switch
        {
            "Generic" => new GenericTramiteDocument(model).GeneratePdf(),
            "Vacaciones" => new VacacionesDocument(model).GeneratePdf(),
            "DiaEspecial" => new DiaEspecialDocument(model).GeneratePdf(),
            "Utilidades" => new UtilidadesDocument(model).GeneratePdf(),
            "Prestaciones" => new PrestacionesDocument(model).GeneratePdf(),
            _ => new GenericTramiteDocument(model).GeneratePdf(),
        };
    }

    public byte[] GenerateArcReport(ArcReportModel model)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        if (model.LogoEmpresa is not { Length: > 0 })
        {
            var logoPath = Path.Combine(_assetsPath, "Logo.webp");
            if (File.Exists(logoPath))
                model.LogoEmpresa = File.ReadAllBytes(logoPath);
        }

        if (model.FirmaSelloAgente is not { Length: > 0 })
        {
            var firmaSelloPath = Path.Combine(_assetsPath, "FirmaSelloArc.png");
            if (File.Exists(firmaSelloPath))
                model.FirmaSelloAgente = File.ReadAllBytes(firmaSelloPath);
        }

        return new ArcDocument(model).GeneratePdf();
    }

    public byte[] GenerateAriPlanilla(AriReportModel model)
    {
        var plantillaPath = Path.Combine(_templatesPath, AriPlanillaExcel.NombreArchivoPlantilla);
        return AriPlanillaExcel.Rellenar(plantillaPath, model);
    }

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

    // ─── Gestión de imágenes ──────────────────────────────────────────────────
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

    /// <summary>
    /// Carga la imagen de firma indicada por nombre de archivo. Si no se especifica,
    /// no existe en disco, o el archivo no puede leerse, retorna la firma por defecto (SinFirma.png).
    /// </summary>
    private byte[]? LoadFirmaImage(string? urlFirma)
    {
        var fileName = string.IsNullOrWhiteSpace(urlFirma) ? FirmaService.SinFirma : Path.GetFileName(urlFirma);
        var fullPath = Path.Combine(_firmasPath, fileName);

        if (!File.Exists(fullPath))
            fullPath = Path.Combine(_firmasPath, FirmaService.SinFirma);

        return File.Exists(fullPath) ? File.ReadAllBytes(fullPath) : null;
    }
}