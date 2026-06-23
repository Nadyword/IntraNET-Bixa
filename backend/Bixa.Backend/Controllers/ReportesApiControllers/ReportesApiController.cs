using AutoMapper;
using Bixa.Backend.Base;
using Bixa.Backend.DataAccess.Wrappers;
using Bixa.Backend.Models.DTOs.ReportesModelDTO;
using Bixa.Backend.Models.Enums;
using Bixa.Backend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bixa.Backend.Controllers.ReportesApiControllers;

[Authorize]
[ApiController]
[Route("api/reportes")]
public class ReportesApiController(
    IReportService reportService,
    IWebHostEnvironment env,
    IMapper mapper,
    LoggerWrapper loggerWrapper) : BaseApiController(mapper, loggerWrapper)
{
    private readonly IReportService _reportService = reportService;
    private readonly string _assetsUrl = "/report-assets";

    // ─── Generación de reportes ───────────────────────────────────────────────

    /// <summary>
    /// Genera el PDF de un trámite específico.
    /// </summary>
    [HttpGet("{tramiteId}/Generar")]
    [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GenerarReporte(int tramiteId, [FromQuery] string plantilla = "Generic")
    {
        var authResult = RequireUserRol(UserRolEnum.Administrador, UserRolEnum.Supervisor);
        if (authResult != null) return authResult;

        // TODO: obtener datos del tramite desde ISolicitudesService y construir TramiteReportModel
        var model = new TramiteReportModel { TramiteId = tramiteId };

        var bytes = _reportService.GenerateTramiteReport(model, plantilla);

        return File(bytes, "application/pdf", $"tramite_{tramiteId}.pdf");
    }

    // ─── Gestión de imágenes ──────────────────────────────────────────────────

    /// <summary>
    /// Lista todas las imágenes disponibles para usar en plantillas de reportes.
    /// </summary>
    [HttpGet("Imagenes")]
    [ProducesResponseType(typeof(IEnumerable<ReportImageDTO>), StatusCodes.Status200OK)]
    public IActionResult GetImagenes()
    {
        var authResult = RequireUserRol(UserRolEnum.Administrador);
        if (authResult != null) return authResult;

        var imagenes = _reportService.GetReportImages()
            .Select(name => new ReportImageDTO
            {
                FileName = name,
                Url      = $"{_assetsUrl}/{name}",
            });

        return Ok(imagenes);
    }

    /// <summary>
    /// Sube una imagen al almacén de assets de reportes.
    /// Formatos aceptados: PNG, JPG, SVG. Tamaño máximo: 5 MB.
    /// </summary>
    [HttpPost("Imagenes")]
    [ProducesResponseType(typeof(ReportImageDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SubirImagen(IFormFile imagen)
    {
        var authResult = RequireUserRol(UserRolEnum.Administrador);
        if (authResult != null) return authResult;

        var allowedExtensions = new[] { ".png", ".jpg", ".jpeg", ".svg" };
        var ext = Path.GetExtension(imagen.FileName).ToLowerInvariant();

        if (!allowedExtensions.Contains(ext))
            return BadRequest("Formato no permitido. Use PNG, JPG o SVG.");

        if (imagen.Length > 5 * 1024 * 1024)
            return BadRequest("El archivo supera el tamaño máximo de 5 MB.");

        await using var stream = imagen.OpenReadStream();
        var savedName = await _reportService.SaveReportImageAsync(stream, imagen.FileName);

        return Ok(new ReportImageDTO
        {
            FileName = savedName,
            Url      = $"{_assetsUrl}/{savedName}",
        });
    }

    /// <summary>
    /// Elimina una imagen del almacén de assets de reportes.
    /// </summary>
    [HttpDelete("Imagenes/{fileName}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public IActionResult EliminarImagen(string fileName)
    {
        var authResult = RequireUserRol(UserRolEnum.Administrador);
        if (authResult != null) return authResult;

        _reportService.DeleteReportImage(fileName);
        return Ok();
    }
}
