using Bixa.Backend.Models.Enums;
using Bixa.Backend.Models.Response;
using Bixa.Backend.Services.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Bixa.Backend.Services.Services;

public class AdjuntoService(string adjuntosPath) : IAdjuntoService
{
    private const long TamanoMaximoBytes = 3 * 1024 * 1024;
    private static readonly string[] ExtensionesPermitidas = [".pdf", ".jpg", ".jpeg", ".png"];

    private readonly string _adjuntosPath = adjuntosPath;

    public async Task<Result<string>> ValidateAndSaveAsync(IFormFile archivo, string ci)
    {
        if (archivo.Length == 0)
            return Result.Fail<string>("El archivo adjunto está vacío.", ErrorTypeEnum.Validation);

        if (archivo.Length > TamanoMaximoBytes)
            return Result.Fail<string>("El archivo adjunto no puede superar los 3 MB.", ErrorTypeEnum.Validation);

        var extension = Path.GetExtension(archivo.FileName);
        if (!ExtensionesPermitidas.Contains(extension, StringComparer.OrdinalIgnoreCase))
            return Result.Fail<string>("El archivo adjunto debe ser PDF, JPG o PNG.", ErrorTypeEnum.Validation);

        Directory.CreateDirectory(_adjuntosPath);
        var safeCi = new string([.. ci.Where(c => char.IsLetterOrDigit(c) || c is '.' or '-')]);
        var fileName = $"{safeCi}_{Guid.NewGuid():N}{extension.ToLowerInvariant()}";
        var fullPath = Path.Combine(_adjuntosPath, fileName);

        await using (var stream = new FileStream(fullPath, FileMode.Create))
        {
            await archivo.CopyToAsync(stream);
        }

        return Result.Success(fileName);
    }
}
