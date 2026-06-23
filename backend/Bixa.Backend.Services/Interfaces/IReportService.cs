using Bixa.Backend.Models.DTOs.ReportesModelDTO;

namespace Bixa.Backend.Services.Interfaces;

public interface IReportService
{
    /// <summary>
    /// Genera el PDF de un trámite usando la plantilla indicada.
    /// Si no se indica plantilla, usa la genérica.
    /// </summary>
    byte[] GenerateTramiteReport(TramiteReportModel model, string plantilla = "Generic");

    /// <summary>
    /// Guarda una imagen en el almacén de assets de reportes y retorna su nombre de archivo.
    /// </summary>
    Task<string> SaveReportImageAsync(Stream imageStream, string fileName);

    /// <summary>
    /// Retorna la lista de nombres de imagen disponibles en el almacén de reportes.
    /// </summary>
    IEnumerable<string> GetReportImages();

    /// <summary>
    /// Elimina una imagen del almacén de reportes.
    /// </summary>
    void DeleteReportImage(string fileName);
}
