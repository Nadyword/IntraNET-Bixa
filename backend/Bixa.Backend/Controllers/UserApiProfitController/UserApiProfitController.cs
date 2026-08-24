using Bixa.Backend.DataAccess.Interfaces.Repositories;
using Bixa.Backend.DataAccess.Entities.DbProfit;
using Bixa.Backend.DataAccess.Wrappers;
using Microsoft.AspNetCore.Authorization;
using Bixa.Backend.Models.DTOs.ReportesModelDTO;
using Bixa.Backend.Services.Interfaces;
using Bixa.Backend.Services.Services;
using Microsoft.AspNetCore.Mvc;
using Bixa.Backend.Models;
using Bixa.Backend.Models.Enums;
using Bixa.Backend.Models.Response;
using Bixa.Backend.Base;
using AutoMapper;

namespace Bixa.Backend.Controllers.UserApiProfitController;

/// <summary>
/// API Controller for user management operations following RESTful standards.
/// Now uses BaseApiController's helper methods for authorization and validation.
/// </summary>
/// <remarks>
/// Initializes a new instance of the UserApiController.
/// </remarks>
///<param name="IReadOnlyUnitOfWork">Read-only unit of work for data access operations.</param>
/// <param name="mapper">AutoMapper instance for DTO conversions.</param>
/// <param name="loggerWrapper">Logger wrapper for logging operations.</param>
/// <param name="reportService">Servicio de generación de reportes PDF.</param>{
[Authorize]
[ApiController]
[Route("api/usersProfit")]
public class UserApiProfitController(
    IReadOnlyUnitOfWork IReadOnlyUnitOfWork,
    IMapper mapper,
    LoggerWrapper loggerWrapper,
    IReportService reportService) : BaseApiController(mapper, loggerWrapper)
{
    private readonly IReadOnlyUnitOfWork _readOnlyUnitOfWork = IReadOnlyUnitOfWork ?? throw new ArgumentNullException(nameof(IReadOnlyUnitOfWork));
    private readonly IReportService _reportService = reportService;

    private static readonly string[] NombresMeses =
    [
        "ENERO", "FEBRERO", "MARZO", "ABRIL", "MAYO", "JUNIO",
        "JULIO", "AGOSTO", "SEPTIEMBRE", "OCTUBRE", "NOVIEMBRE", "DICIEMBRE",
    ];

    /// <summary>
    /// Retrieves a single user by their ID.
    /// GET /api/users/{id}
    /// </summary>
    /// <param name="ci">The tax ID of the user to retrieve.</param>
    /// <returns>API response containing the GrupoFa if found.</returns>
    [HttpGet("{ci}/GrupoFa")]
    [ProducesResponseType(typeof(ApiResponse<GrupoFa[]>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetGrupoFaByCi(string ci)
    {
        var normalizedCi = UtilityService.NormalizeCiFormat(ci);
        var result = await _readOnlyUnitOfWork.GrupoFa.GetFullInfoByCiAsync(normalizedCi);
        return HandleServiceResult(result);
    }

    /// <summary>
    /// Retrieves a single user by their ID.
    /// GET /api/users/{id}
    /// </summary>
    /// <param name="ci">The tax ID of the user to retrieve.</param>
    /// <returns>API response containing the SnEmple if found.</returns>
    [HttpGet("{ci}/SnEmple")]
    [ProducesResponseType(typeof(ApiResponse<SnEmple>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetUserByCi(string ci)
    {
        var normalizedCi = UtilityService.NormalizeCiFormat(ci);
        var result = await _readOnlyUnitOfWork.SnEmple.GetFullInfoByCiAsync(normalizedCi);
        return HandleServiceResult(result);
    }

    /// <summary>
    /// Historial de vacaciones por CodEmp
    /// </summary>
    /// <param name="CodEmp">Lista de CodEmp para obtener el historial de vacaciones.</param>
    /// <returns>API response containing la lista de vacaciones por CodEmp.</returns>
    [HttpGet("{CodEmp}/Vacaciones")]
    [ProducesResponseType(typeof(ApiResponse<Vacaciones[]>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetVacacionesByCodEmp(string CodEmp)
    {
        var result = await _readOnlyUnitOfWork.Vacaciones.GetHistorialVacaByCodEmpAsync(CodEmp);
        return HandleServiceResult(result);
    }

    /// <summary>
    /// Obtiene los días especiales por CodEmp
    /// </summary>
    /// <param name="CodEmp">El código del empleado para el que se recuperan los días especiales. No puede ser nulo.</param>
    /// <returns>Un resultado que contiene una lista de objetos de días especiales asociados al empleado. Si no se encuentra el
    /// empleado, el resultado indica un error de tipo NotFound.</returns>
    [HttpGet("{CodEmp}/DiasEspeciales")]
    [ProducesResponseType(typeof(ApiResponse<Vacaciones[]>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetDiasEspecialesByCodEmp(string CodEmp)
    {
        var result = await _readOnlyUnitOfWork.DiaEspeciales.GetDiaEspecialesByCodEmpAsync(CodEmp);
        return HandleServiceResult(result);
    }

    /// <summary>
    /// Monto de utilidades disponible por CI
    /// </summary>
    /// <param name="ci">La cédula de identidad del empleado para el que se recupera el monto disponible.</param>
    /// <returns>API response con el monto disponible de utilidades.</returns>
    [HttpGet("{ci}/Utilidades")]
    [ProducesResponseType(typeof(ApiResponse<decimal?>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetUtilidadesByCi(string ci)
    {
        var normalizedCi = UtilityService.NormalizeCiFormat(ci);
        var result = await _readOnlyUnitOfWork.Utilidades.GetMontoDisponibleByCiAsync(normalizedCi);
        return HandleServiceResult(result);
    }

    /// <summary>
    /// Monto de prestaciones sociales disponible por CI
    /// </summary>
    /// <param name="ci">La cédula de identidad del empleado para el que se recupera el monto disponible.</param>
    /// <returns>API response con el monto disponible de prestaciones sociales.</returns>
    [HttpGet("{ci}/PrestacionesSociales")]
    [ProducesResponseType(typeof(ApiResponse<decimal?>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetPrestacionesSocialesByCi(string ci)
    {
        var normalizedCi = UtilityService.NormalizeCiFormat(ci);
        var result = await _readOnlyUnitOfWork.PrestacionesSociales.GetMontoDisponibleByCiAsync(normalizedCi);
        return HandleServiceResult(result);
    }

    /// <summary>
    /// Consulta de HC (Cobertura 1 - 10.000,00): titular, prima trimestral en Bs. y montos de nómina de los 3 meses.
    /// </summary>
    /// <param name="ci">La cédula de identidad del empleado.</param>
    /// <returns>API response con el registro de HC de cobertura 1.</returns>
    [HttpGet("{ci}/ConsultaHc/Cobertura1")]
    [ProducesResponseType(typeof(ApiResponse<ConsultaHc>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetConsultaHcCobertura1(string ci)
    {
        var normalizedCi = UtilityService.NormalizeCiFormat(ci);
        var result = await _readOnlyUnitOfWork.ConsultaHc.GetConsultaHcAsync(normalizedCi, 10000.00m);
        return HandleServiceResult(result);
    }

    /// <summary>
    /// Consulta de HC (Cobertura 2 - 20.000,00): familiares/beneficiarios y pago a Bixa trimestral en $.
    /// Puede devolver más de un registro (uno por cada familiar/beneficiario).
    /// </summary>
    /// <param name="ci">La cédula de identidad del empleado.</param>
    /// <returns>API response con los registros de HC de cobertura 2.</returns>
    [HttpGet("{ci}/ConsultaHc/Cobertura2")]
    [ProducesResponseType(typeof(ApiResponse<List<ConsultaHc>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetConsultaHcCobertura2(string ci)
    {
        var normalizedCi = UtilityService.NormalizeCiFormat(ci);
        var result = await _readOnlyUnitOfWork.ConsultaHc.GetConsultaHcListAsync(normalizedCi, 20000.00m);
        return HandleServiceResult(result);
    }

    /// <summary>
    /// Consulta ARC (retención de ISLR): detalle mensual de remuneraciones e impuesto retenido del año indicado (por defecto, el año en curso).
    /// </summary>
    /// <param name="ci">La cédula de identidad del empleado.</param>
    /// <param name="anio">Año a consultar. Si no se especifica, se usa el año en curso.</param>
    /// <returns>API response con el detalle mensual de ARC.</returns>
    [HttpGet("{ci}/ConsultaArc")]
    [ProducesResponseType(typeof(ApiResponse<List<ConsultaArc>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetConsultaArc(string ci, [FromQuery] int? anio)
    {
        var normalizedCi = UtilityService.NormalizeCiFormat(ci);
        var result = await _readOnlyUnitOfWork.ConsultaArc.GetConsultaArcAsync(normalizedCi, anio ?? DateTime.Now.Year);
        return HandleServiceResult(result);
    }

    /// <summary>
    /// Genera el PDF del comprobante de retención (ARC) del año indicado (por defecto, el año en curso).
    /// </summary>
    /// <param name="ci">La cédula de identidad del empleado.</param>
    /// <param name="anio">Año a consultar. Si no se especifica, se usa el año en curso.</param>
    /// <returns>Archivo PDF con el comprobante de retención.</returns>
    [HttpGet("{ci}/ConsultaArc/Reporte")]
    [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetConsultaArcReporte(string ci, [FromQuery] int? anio)
    {
        var normalizedCi = UtilityService.NormalizeCiFormat(ci);
        var year = anio ?? DateTime.Now.Year;

        var empleadoResult = await _readOnlyUnitOfWork.SnEmple.GetFullInfoByCiAsync(normalizedCi);
        if (!empleadoResult.IsSuccess) return HandleServiceResult(empleadoResult);

        var arcResult = await _readOnlyUnitOfWork.ConsultaArc.GetConsultaArcAsync(normalizedCi, year);
        if (!arcResult.IsSuccess) return HandleServiceResult(arcResult);

        var empleado = empleadoResult.Value;
        var nombreCompleto = string.Join(", ", new[] { empleado.Apellidos, empleado.Nombres }
            .Where(n => !string.IsNullOrWhiteSpace(n)));

        var model = new ArcReportModel
        {
            Anio = year,
            EmpleadoNombre = nombreCompleto,
            EmpleadoCi = $"V-{normalizedCi}",
            EmpleadoRif = !string.IsNullOrWhiteSpace(empleado.Rif) ? empleado.Rif : normalizedCi,
            Meses = arcResult.Value
                .OrderBy(a => a.Mes)
                .Select(a => new ArcMesReportModel
                {
                    Mes = a.Mes is >= 1 and <= 12 ? NombresMeses[a.Mes.Value - 1] : "—",
                    Remuneracion = a.Remuneracion ?? 0,
                    PorcentRetencion = a.PorcentRetencion,
                    ImpuestoRetenido = a.ImpuestoRetenido,
                    RemuneracionAcumulada = a.RemuneracionAcumulada ?? 0,
                    ImpuestoRetenidoAcum = a.ImpuestoRetenidoAcum,
                })
                .ToList(),
        };

        var bytes = _reportService.GenerateArcReport(model);
        return File(bytes, "application/pdf", $"ARC_{normalizedCi}_{year}.pdf");
    }

    private static readonly string[] MesesAriValidos = ["Enero", "Marzo", "Junio", "Septiembre", "Diciembre"];
    private static readonly string[] DesgravamenTiposValidos = ["Unico", "Detallado"];

    private const string ContentTypeXls = "application/vnd.ms-excel";

    /// <summary>
    /// Genera la planilla AR-I (determinación del porcentaje de retención de I.S.L.R.) rellenando
    /// una copia de la plantilla Excel del SENIAT. La identidad del contribuyente, el valor de la
    /// U.T., la carga familiar y la estimación de remuneraciones por percibir provienen de la
    /// consulta a Profit; del formulario solo se toman el mes y el desgravamen.
    /// </summary>
    /// <param name="ci">La cédula de identidad del empleado.</param>
    /// <param name="request">Los datos ingresados en el formulario ARI.</param>
    /// <returns>Archivo Excel (.xls) con la planilla AR-I.</returns>
    [HttpPost("{ci}/Ari/Reporte")]
    [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAriReporte(string ci, [FromBody] AriReportRequestDTO request)
    {
        if (!MesesAriValidos.Contains(request.Mes))
            return HandleServiceResult(Result.Fail<AriReportModel>("El mes indicado no es válido.", ErrorTypeEnum.Validation));

        if (!DesgravamenTiposValidos.Contains(request.DesgravamenTipo))
            return HandleServiceResult(Result.Fail<AriReportModel>("El tipo de desgravamen indicado no es válido.", ErrorTypeEnum.Validation));

        var normalizedCi = UtilityService.NormalizeCiFormat(ci);
        var ariResult = await _readOnlyUnitOfWork.Ari.GetAriByCiAsync(normalizedCi);
        if (!ariResult.IsSuccess) return HandleServiceResult(ariResult);

        var datos = ariResult.Value;

        if (datos.UniTribu is not > 0)
            return HandleServiceResult(Result.Fail<AriReportModel>(
                "Profit no devolvió el valor vigente de la Unidad Tributaria.", ErrorTypeEnum.Validation));

        if (datos.GranTotal is not > 0)
            return HandleServiceResult(Result.Fail<AriReportModel>(
                "Profit no devolvió recibos de nómina del año en curso, así que no se puede estimar las remuneraciones por percibir.",
                ErrorTypeEnum.Validation));

        var model = new AriReportModel
        {
            NombreEmpresa = datos.NombreEmpresa ?? string.Empty,
            NombreCompleto = datos.NombreCompleto ?? string.Empty,
            Ci = datos.Ci ?? normalizedCi,
            Rif = datos.Rif ?? string.Empty,
            AnoGravable = datos.AnoActual ?? DateTime.Now.Year,
            UniTribu = datos.UniTribu.Value,
            CargaFamiliar = datos.CargaFami ?? 0,
            GranTotal = datos.GranTotal.Value,

            Mes = request.Mes,
            DesgravamenTipo = request.DesgravamenTipo,
            InstitutosDocentes = request.InstitutosDocentes,
            PrimasSeguro = request.PrimasSeguro,
            ServiciosMedicos = request.ServiciosMedicos,
            InteresesVivienda = request.InteresesVivienda,
        };

        var bytes = _reportService.GenerateAriPlanilla(model);
        return File(bytes, ContentTypeXls, $"ARI_{normalizedCi}_{model.AnoGravable}.xls");
    }
}