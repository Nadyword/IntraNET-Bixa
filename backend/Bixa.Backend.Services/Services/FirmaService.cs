using Bixa.Backend.Models.Enums;
using Bixa.Backend.Models.Response;
using Bixa.Backend.Services.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Bixa.Backend.Services.Services;

public class FirmaService(string firmasPath) : IFirmaService
{
    public const string SinFirma = "SinFirma.png";
    private const int RequiredSize = 225;

    private static readonly byte[] PngSignature = [137, 80, 78, 71, 13, 10, 26, 10];

    private readonly string _firmasPath = firmasPath;

    public async Task<Result<string>> ValidateAndSaveAsync(IFormFile firma, string ci)
    {
        if (firma.Length == 0)
            return Result.Fail<string>("El archivo de firma está vacío.", ErrorTypeEnum.Validation);

        if (!string.Equals(Path.GetExtension(firma.FileName), ".png", StringComparison.OrdinalIgnoreCase))
            return Result.Fail<string>("La firma debe ser un archivo PNG.", ErrorTypeEnum.Validation);

        using var memoryStream = new MemoryStream();
        await firma.CopyToAsync(memoryStream);
        var bytes = memoryStream.ToArray();

        if (!TryReadPngDimensions(bytes, out var width, out var height))
            return Result.Fail<string>("El archivo no es un PNG válido.", ErrorTypeEnum.Validation);

        if (width != RequiredSize || height != RequiredSize)
            return Result.Fail<string>($"La imagen debe medir exactamente {RequiredSize}x{RequiredSize} píxeles (recibido {width}x{height}).", ErrorTypeEnum.Validation);

        Directory.CreateDirectory(_firmasPath);
        var fileName = BuildFileName(ci);
        var fullPath = Path.Combine(_firmasPath, fileName);
        await File.WriteAllBytesAsync(fullPath, bytes);

        return Result.Success(fileName);
    }

    public void DeleteFirma(string? urlFirma)
    {
        if (string.IsNullOrWhiteSpace(urlFirma)) return;
        if (string.Equals(urlFirma, SinFirma, StringComparison.OrdinalIgnoreCase)) return;

        var fullPath = Path.Combine(_firmasPath, Path.GetFileName(urlFirma));
        if (File.Exists(fullPath))
            File.Delete(fullPath);
    }

    private static string BuildFileName(string ci)
    {
        var safe = new string([.. ci.Where(c => char.IsLetterOrDigit(c) || c is '.' or '-')]);
        return $"{safe}.png";
    }

    /// <summary>
    /// Lee ancho/alto desde el chunk IHDR sin depender de una librería de imágenes.
    /// </summary>
    private static bool TryReadPngDimensions(byte[] bytes, out int width, out int height)
    {
        width = height = 0;

        if (bytes.Length < 24) return false;
        if (!bytes.AsSpan(0, 8).SequenceEqual(PngSignature)) return false;
        if (bytes[12] != (byte)'I' || bytes[13] != (byte)'H' || bytes[14] != (byte)'D' || bytes[15] != (byte)'R')
            return false;

        width = (bytes[16] << 24) | (bytes[17] << 16) | (bytes[18] << 8) | bytes[19];
        height = (bytes[20] << 24) | (bytes[21] << 16) | (bytes[22] << 8) | bytes[23];
        return true;
    }
}
