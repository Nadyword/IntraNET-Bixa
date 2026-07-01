using Bixa.Backend.Models.Response;
using Microsoft.AspNetCore.Http;

namespace Bixa.Backend.Services.Interfaces;

public interface IFirmaService
{
    /// <summary>
    /// Valida que el archivo sea un PNG de 225x225 píxeles y lo guarda en el
    /// almacén de firmas. Retorna el nombre de archivo guardado (para UrlFirma).
    /// </summary>
    Task<Result<string>> ValidateAndSaveAsync(IFormFile firma, string ci);

    /// <summary>
    /// Elimina el archivo de firma indicado. Nunca elimina la firma por defecto.
    /// </summary>
    void DeleteFirma(string? urlFirma);
}
