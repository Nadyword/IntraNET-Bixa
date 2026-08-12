namespace Bixa.Backend.Models.DTOs.ReportesModelDTO;

/// <summary>
/// Datos que el empleado ingresa en el formulario de la planilla AR-I.
/// Los datos de identidad (nombre, cédula, RIF) se obtienen del empleado autenticado, no de este DTO.
/// </summary>
public class AriReportRequestDTO
{
    public int Anio { get; set; }

    /// <summary>Enero, Marzo, Junio, Septiembre o Diciembre. Enero = declaración inicial; el resto = variación.</summary>
    public string Mes { get; set; } = string.Empty;

    public decimal SueldoMensual { get; set; }
    public decimal OtrosIngresosMensuales { get; set; }
    public decimal Utilidades { get; set; }
    public int DiasBonoVacacional { get; set; }

    /// <summary>Valor vigente de la Unidad Tributaria en Bs.</summary>
    public decimal ValorUT { get; set; }

    /// <summary>"Unico" (774 U.T.) o "Detallado".</summary>
    public string DesgravamenTipo { get; set; } = string.Empty;
    public decimal InstitutosDocentes { get; set; }
    public decimal PrimasSeguro { get; set; }
    public decimal ServiciosMedicos { get; set; }
    public decimal InteresesVivienda { get; set; }

    public int CargaFamiliar { get; set; }
    public decimal ImpuestosRetenidosAnterioresBs { get; set; }

    /// <summary>Solo aplica cuando Mes distinto de Enero (hay variación).</summary>
    public decimal? ImpuestoRetenidoHastaFechaBs { get; set; }
    public decimal? RemuneracionesPercibidasHastaFechaBs { get; set; }
}

/// <summary>
/// Modelo con todos los valores ya calculados, listo para ser renderizado por AriDocument.
/// </summary>
public class AriReportModel
{
    private const decimal DesgravamenUnicoUt = 774m;
    private const decimal RebajaPersonalUt = 10m;
    private const decimal RebajaPorCargaUt = 10m;

    // Tarifa N° 1 (Art. 50 Ley de I.S.L.R.) — tramos en U.T., límite superior inclusive, null = sin tope.
    private static readonly (decimal? LimiteSuperior, decimal Porcentaje, decimal Sustraendo)[] Tarifa1 =
    [
        (1000m, 0.06m, 0m),
        (1500m, 0.09m, 30m),
        (2000m, 0.12m, 75m),
        (2500m, 0.16m, 155m),
        (3000m, 0.20m, 255m),
        (4000m, 0.24m, 375m),
        (6000m, 0.29m, 575m),
        (null, 0.34m, 875m),
    ];

    public byte[]? LogoEmpresa { get; set; }

    public string EmpleadoNombre { get; set; } = string.Empty;
    public string EmpleadoCi { get; set; } = string.Empty;
    public string EmpleadoRif { get; set; } = string.Empty;

    public int Anio { get; set; }
    public string Mes { get; set; } = string.Empty;
    public bool EsVariacion { get; set; }

    // A. Estimación de remuneraciones
    public decimal SueldoMensual { get; set; }
    public decimal OtrosIngresosMensuales { get; set; }
    public decimal SueldoDiario { get; set; }
    public int DiasBonoVacacional { get; set; }
    public decimal Utilidades { get; set; }
    public decimal SubTotal1 { get; set; }
    public decimal SubTotal2 { get; set; }
    public decimal SubTotal3 { get; set; }
    public decimal TotalA { get; set; }

    // B. Conversión a U.T.
    public decimal ValorUT { get; set; }
    public decimal TotalB_UT { get; set; }

    // C/D/E. Desgravámenes
    public string DesgravamenTipo { get; set; } = string.Empty;
    public decimal InstitutosDocentes { get; set; }
    public decimal PrimasSeguro { get; set; }
    public decimal ServiciosMedicos { get; set; }
    public decimal InteresesVivienda { get; set; }
    public decimal TotalDesgravamenDetallado_UT { get; set; }
    public decimal DesgravamenAplicado_UT { get; set; }

    // F. Renta gravable
    public decimal RentaGravable_UT { get; set; }

    // G. Impuesto estimado
    public decimal TramoPorcentaje { get; set; }
    public decimal TramoSustraendo_UT { get; set; }
    public decimal ImpuestoG_UT { get; set; }

    // H. Rebajas
    public int CargaFamiliar { get; set; }
    public decimal RebajaCargaFamiliar_UT { get; set; }
    public decimal ImpuestosRetenidosAnterioresBs { get; set; }
    public decimal RebajaImpuestosAnteriores_UT { get; set; }
    public decimal TotalH_UT { get; set; }

    // I. Impuesto a retener
    public decimal ImpuestoI_UT { get; set; }

    // J. Porcentaje de retención inicial
    public decimal PorcentajeJ { get; set; }

    // K. Porcentaje por variación (solo si EsVariacion)
    public decimal? ImpuestoRetenidoHastaFechaBs { get; set; }
    public decimal? RemuneracionesPercibidasHastaFechaBs { get; set; }
    public decimal? PorcentajeK { get; set; }

    public static AriReportModel Calcular(AriReportRequestDTO req, string empleadoNombre, string empleadoCi, string empleadoRif)
    {
        var sueldoDiario = req.SueldoMensual / 30m;
        var subTotal1 = (req.SueldoMensual + req.OtrosIngresosMensuales) * 12m;
        var subTotal2 = req.Utilidades;
        var subTotal3 = req.DiasBonoVacacional * sueldoDiario;
        var totalA = subTotal1 + subTotal2 + subTotal3;

        var totalB = req.ValorUT > 0 ? totalA / req.ValorUT : 0m;

        var totalDesgravamenDetallado = req.InstitutosDocentes + req.PrimasSeguro + req.ServiciosMedicos + req.InteresesVivienda;
        var desgravamenAplicado = req.DesgravamenTipo == "Unico" ? DesgravamenUnicoUt : totalDesgravamenDetallado;

        var rentaGravable = Math.Max(0m, totalB - desgravamenAplicado);

        var (limite, pct, sustraendo) = Array.Find(Tarifa1, t => t.LimiteSuperior is null || rentaGravable <= t.LimiteSuperior);
        _ = limite;
        var impuestoG = Math.Max(0m, (rentaGravable * pct) - sustraendo);

        var rebajaCarga = req.CargaFamiliar * RebajaPorCargaUt;
        var rebajaImpuestosAnteriores = req.ValorUT > 0 ? req.ImpuestosRetenidosAnterioresBs / req.ValorUT : 0m;
        var totalH = RebajaPersonalUt + rebajaCarga + rebajaImpuestosAnteriores;

        var impuestoI = Math.Max(0m, impuestoG - totalH);
        var porcentajeJ = totalB > 0 ? (impuestoI / totalB) * 100m : 0m;

        var esVariacion = !string.Equals(req.Mes, "Enero", StringComparison.OrdinalIgnoreCase);
        decimal? porcentajeK = null;
        if (esVariacion && req.ImpuestoRetenidoHastaFechaBs is decimal retenido && req.RemuneracionesPercibidasHastaFechaBs is decimal percibidas)
        {
            var numerador = (impuestoI * req.ValorUT) - retenido;
            var denominador = totalA - percibidas;
            porcentajeK = denominador != 0 ? (numerador / denominador) * 100m : 0m;
        }

        return new AriReportModel
        {
            EmpleadoNombre = empleadoNombre,
            EmpleadoCi = empleadoCi,
            EmpleadoRif = empleadoRif,
            Anio = req.Anio,
            Mes = req.Mes,
            EsVariacion = esVariacion,

            SueldoMensual = req.SueldoMensual,
            OtrosIngresosMensuales = req.OtrosIngresosMensuales,
            SueldoDiario = sueldoDiario,
            DiasBonoVacacional = req.DiasBonoVacacional,
            Utilidades = req.Utilidades,
            SubTotal1 = subTotal1,
            SubTotal2 = subTotal2,
            SubTotal3 = subTotal3,
            TotalA = totalA,

            ValorUT = req.ValorUT,
            TotalB_UT = totalB,

            DesgravamenTipo = req.DesgravamenTipo,
            InstitutosDocentes = req.InstitutosDocentes,
            PrimasSeguro = req.PrimasSeguro,
            ServiciosMedicos = req.ServiciosMedicos,
            InteresesVivienda = req.InteresesVivienda,
            TotalDesgravamenDetallado_UT = totalDesgravamenDetallado,
            DesgravamenAplicado_UT = desgravamenAplicado,

            RentaGravable_UT = rentaGravable,

            TramoPorcentaje = pct,
            TramoSustraendo_UT = sustraendo,
            ImpuestoG_UT = impuestoG,

            CargaFamiliar = req.CargaFamiliar,
            RebajaCargaFamiliar_UT = rebajaCarga,
            ImpuestosRetenidosAnterioresBs = req.ImpuestosRetenidosAnterioresBs,
            RebajaImpuestosAnteriores_UT = rebajaImpuestosAnteriores,
            TotalH_UT = totalH,

            ImpuestoI_UT = impuestoI,
            PorcentajeJ = porcentajeJ,

            ImpuestoRetenidoHastaFechaBs = req.ImpuestoRetenidoHastaFechaBs,
            RemuneracionesPercibidasHastaFechaBs = req.RemuneracionesPercibidasHastaFechaBs,
            PorcentajeK = porcentajeK,
        };
    }
}
