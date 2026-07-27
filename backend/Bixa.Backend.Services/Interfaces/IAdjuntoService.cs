using Bixa.Backend.Models.Response;
using Microsoft.AspNetCore.Http;

namespace Bixa.Backend.Services.Interfaces;

public interface IAdjuntoService
{
    /// <summary>
    /// Valida que el archivo sea PDF, JPG o PNG y no supere 3 MB, y lo guarda en el
    /// almacén de adjuntos de solicitudes. Retorna el nombre de archivo guardado.
    /// </summary>
    Task<Result<string>> ValidateAndSaveAsync(IFormFile archivo, string ci);
}
