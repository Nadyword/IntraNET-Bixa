namespace Bixa.Backend.DataAccess.Templates.Profit;

internal static class ProfitSqlTemplates
{
    internal const string? GetByCi = """
        SELECT
            e.cod_emp AS 'CodEmp',
            e.nombres,
            e.apellidos,
            e.ci,
            e.rif,
            e.correo_e AS 'CorreoP',
            e.correo_e_2 AS 'CorreoE',
            e.fecha_nac AS 'FechaNac',
            e.direccion,
            DATEDIFF(YEAR, e.fecha_nac, GETDATE()) - CASE WHEN DATEADD(YEAR, DATEDIFF(YEAR, e.fecha_nac, GETDATE()), e.fecha_nac) > GETDATE() THEN 1 ELSE 0 END AS edad,
            CASE WHEN e.sexo = 'M' THEN 'Masculino' WHEN e.sexo = 'F' THEN 'Femenino' END AS sexo,
            DATEDIFF(YEAR, e.fecha_ing, GETDATE()) - CASE WHEN DATEADD(YEAR, DATEDIFF(YEAR, e.fecha_ing, GETDATE()), e.fecha_ing) > GETDATE() THEN 1 ELSE 0 END AS timeAtEm,
            e.fecha_ing AS 'FechaIng',
            e.telefono,
            CASE WHEN e.edo_civ = 'S' THEN 'Soltero(a)' WHEN e.edo_civ = 'C' THEN 'Casado(a)' WHEN e.edo_civ = 'D' THEN 'Divorciado(a)' WHEN e.edo_civ = 'V' THEN 'Viudo(a)' END AS EstadoCivil,
            c.des_cargo AS 'DesCargo',
            d.des_depart AS 'DesDepart',
            s.des_cont AS 'DesCont',
            u.des_ubicacion AS 'DesUbicacion',
            e.grupo_sang AS 'GrupoSang',
            CASE WHEN e.nac = 1 THEN 'Venezolano(a)' ELSE 'Extranjero(a)' END AS Nacionalidad,
            e.cta_banc1 AS 'CuentaBanc1'
        FROM dbo.snemple AS E
        INNER JOIN dbo.sncargo AS C ON e.co_cargo = c.co_cargo
        INNER JOIN dbo.sndepart AS D ON c.co_depart = d.co_depart
        INNER JOIN dbo.sncont AS S ON e.co_cont = s.co_cont
        INNER JOIN dbo.snubicacion AS U ON e.co_ubicacion = u.co_ubicacion
        WHERE e.ci IN (@ci)
        """;

    internal const string GetGrupoFamByCi = """
        SELECT
            gf.cod_emp AS codEmp,
            nombre,
            CASE WHEN gf.nac = 1 THEN 'Venezolano(a)' ELSE 'Extranjero(a)' END AS Nacionalidad,
            DATEDIFF(YEAR, gf.fecha_nac, GETDATE()) - CASE WHEN DATEADD(YEAR, DATEDIFF(YEAR, gf.fecha_nac, GETDATE()), gf.fecha_nac) > GETDATE() THEN 1 ELSE 0 END AS edad,
            CASE WHEN gf.sexo = 'M' THEN 'Masculino' WHEN gf.sexo = 'F' THEN 'Femenino' END AS sexo,
            gf.ocupacion,
            CASE CAST(gf.tip_gru_fa AS VARCHAR(80)) WHEN 'CO' THEN 'Cónyuge' WHEN 'E' THEN 'Esposo' WHEN 'HE' THEN 'Hermano' WHEN 'HI' THEN 'Hijo' WHEN 'P' THEN 'PADRE' ELSE 'Otros' END AS 'Parentesco'
        FROM sngru_fa AS gf
        INNER JOIN snemple AS e ON gf.cod_emp = e.cod_emp
        WHERE e.ci = @ci
        """;

    internal const string GetEmailByCi = """
        SELECT
            correo_e_2 AS 'CorreoE'
        FROM snemple WHERE ci = @ci
        """;

    internal const string GetInfoBasicByCi = """
        SELECT
            nombres,
            apellidos,
            ci
        FROM snemple WHERE ci = @ci
    """;

    internal const string GetVacacionesByCodEmp = """
        DECLARE @sCod_Emp_d char(17) = @codEmp;

        DECLARE @TablaRecibo table (cod_emp char(17), Nombre varchar(122), desde datetime, hasta datetime, dias int);
        DECLARE @cnt INT = 0;
        DECLARE @dia INT = 15;
        DECLARE @fec datetime = (SELECT fecha_ing FROM snemple WHERE cod_emp = @sCod_Emp_d);
        DECLARE @tope INT = YEAR(GETDATE()) - YEAR(@fec);

        WHILE @cnt < @tope
        BEGIN
            INSERT INTO @TablaRecibo (cod_emp, Nombre, desde, hasta, dias)
            SELECT
                e.cod_emp,
                e.nombre_completo,
                DATEADD(yy, @cnt, @fec),
                DATEADD(yy, @cnt + 1, @fec),
                CASE WHEN @dia + @cnt > 30 THEN 30 ELSE @dia + @cnt END
            FROM snemple e
            WHERE e.cod_emp = @sCod_Emp_d;

            SET @cnt = @cnt + 1;
        END;

        INSERT INTO @TablaRecibo (cod_emp, Nombre, desde, hasta, dias)
        SELECT e.cod_emp, e.nombre_completo, v.desde, v.hasta, -1 * v.dias FROM dbo.snemple AS e
        INNER JOIN snvacaci AS v ON (e.cod_emp = v.cod_emp OR convert(nvarchar(max), v.trabajadores) LIKE '%<Trabajador>' + e.cod_emp + '%</Trabajador>%')
        WHERE e.cod_emp = @sCod_Emp_d
        GROUP BY e.cod_emp, e.nombre_completo,v.desde,v.hasta,v.dias;

        SELECT cod_emp codEmp, Nombre, desde, hasta, dias, SUM(dias) OVER (PARTITION BY cod_emp ORDER BY desde, dias DESC) AS disponibleAcumulado FROM    @TablaRecibo ORDER BY desde;
    """;

    internal const string GetDiasEspeciales = """
      SELECT
	    des_novedad_dia desNovedadDia ,
	    fecha_registro fechaRegistro,
	    e.nombre_completo AutorisadoPor,
	    a.desde,
	    a.hasta,
	    a.dias,
		a.comentario
    FROM snnovedad_dia a
    INNER JOIN snemple e ON e.cod_emp = a.cod_emp
    WHERE a.cod_emp = @codEmp AND a.co_tipoaus = '999' AND
    a.desde >= (SELECT val_f FROM snconst WHERE co_const = 'A003') AND
    a.hasta <= (SELECT val_f FROM snconst WHERE co_const = 'A004')

""";

    internal const string GetListAprovadoresByCi = """
    IF OBJECT_ID('tempdb..#EmpleadosTemp') IS NOT NULL
        DROP TABLE #EmpleadosTemp;

    CREATE TABLE #EmpleadosTemp (
        nombre varchar(100) NOT NULL,
        ci char(15) NOT NULL
    );

    DECLARE @Ci char(20) = '@ci';
    DECLARE @supervisor char(15);
    DECLARE @contador BIT = 0;
    WHILE (@contador = 0)
    BEGIN
        SET @supervisor = (SELECT supervisor FROM snemple WHERE ci = @Ci);
        INSERT INTO #EmpleadosTemp ( ci, nombre)  SELECT ci, nombre_completo FROM snemple WHERE cod_emp = @supervisor AND supervisor NOT IN ('3.666.186', '3.959.047');

        IF (@supervisor IS NULL)
        BEGIN
            SET @contador = 1;
        END
        SET @Ci = (SELECT ci FROM snemple WHERE cod_emp = @supervisor);
    END
    SELECT * FROM #EmpleadosTemp;
    DROP TABLE #EmpleadosTemp;
    """;

    internal const string GetTramitesForAprobacion = """
        SELECT
        	TramiteId, AprobadorCi, Orden, Comentario, TipoTramiteId, u.FirstName, u.LastName
        FROM Aprobaciones a
        INNER JOIN Tramites t ON a.TramiteId = t.Id
        INNER JOIN Users u ON t.UserCi = u.Ci
        WHERE AprobadorCi = '@ci' AND a.Estado = 1
    """;

    internal const string GetFechasFeriadas = """
        SELECT fecha FROM snren_cf WHERE (feriado = 1 OR descanso = 1) AND (fecha >= @Desde AND fecha <= @Hasta)
    """;

    internal const string GetUtilidades = "SELECT val_n AS MontoDisponible FROM snem_va a INNER JOIN snemple b ON a.cod_emp = b.cod_emp WHERE a.co_var = 'A001' AND b.ci = @ci";

    internal const string GetEquipoSupervidor = """
    WITH JerarquiaEmpleados AS (
        SELECT
            e.nombre_completo AS Nombres,
            e.ci,
            e.cod_emp AS CodEmp,
            d.des_depart AS DesDepart,
            c.des_cargo AS DesCargo,
            e.supervisor
        FROM snemple e
        INNER JOIN dbo.sncargo AS c ON e.co_cargo = c.co_cargo
        INNER JOIN dbo.sndepart AS d ON c.co_depart = d.co_depart
        WHERE e.supervisor IN (
            SELECT cod_emp
            FROM snemple
            WHERE Ci = @ci AND e.status = 'A'
        )

        UNION ALL

        SELECT
            e.nombre_completo AS Nombres,
            e.ci,
            e.cod_emp AS CodEmp,
            d.des_depart AS DesDepart,
            c.des_cargo AS DesCargo,
            e.supervisor
        FROM snemple e
        INNER JOIN dbo.sncargo AS c ON e.co_cargo = c.co_cargo
        INNER JOIN dbo.sndepart AS d ON c.co_depart = d.co_depart
        INNER JOIN JerarquiaEmpleados AS r ON e.supervisor = r.CodEmp
        WHERE e.status = 'A'
    )

    SELECT DISTINCT
        Nombres,
        ci,
        CodEmp,
        DesDepart,
        DesCargo
    FROM JerarquiaEmpleados;
    """;

    internal const string GetConsultaHC = """
    IF OBJECT_ID('tempdb..#Resultados') IS NOT NULL
        DROP TABLE #Resultados;

    IF OBJECT_ID('tempdb..#NominaE071') IS NOT NULL
        DROP TABLE #NominaE071;

    DECLARE
        @sTipo_Poliza char(6) = null,
        @iNum_Contrpol_d int = null,                        w
        @iNum_Contrpol_h int = null,
        @sCod_emp_d char(17) = null,
        @sCod_emp_h char(17) = null,
        @ci VARCHAR(15) = @CiEmpleado,
        @covertura DECIMAL(18,2) = @Cover,
        @tipo_orden INT = 0
        SET NOCOUNT ON;
        SET @tipo_orden = IIF(@covertura = 10000.00,1,2);
        SET @sCod_emp_d = (SELECT cod_emp FROM snemple WHERE ci = @ci )
        SET @sCod_emp_h = (SELECT cod_emp FROM snemple WHERE ci = @ci)

        SELECT
            cod_emp,
            YEAR(fec_emis) AS anio,
            MONTH(fec_emis) AS mes,
            SUM(monto) AS monto
        INTO #NominaE071
        FROM snnomi
        WHERE co_conce = 'E071'
        GROUP BY cod_emp, YEAR(fec_emis), MONTH(fec_emis)

        CREATE CLUSTERED INDEX IX_NominaE071 ON #NominaE071(cod_emp, anio, mes)

    SELECT
            snemple.cod_emp,
            snemple.ci,
            sncontr_pol.num_contrpol,
            sncontr_pol.tipo_poliza,
            1 AS tipo_orden,
            snemple.nombre_completo,
            CAST(CASE snemple.status
                WHEN 'A' THEN 'Activo'
                WHEN 'I' THEN 'Inactivo'
                WHEN 'L' THEN 'Liquidado'
                WHEN 'O' THEN 'Otro'
                WHEN 'PL' THEN 'Por Liquidar'
                WHEN 'EL' THEN 'Parcialmente Liquidado' ELSE ''
            END AS VARCHAR(30)) AS status,
            CAST('EMPLEADO' AS VARCHAR(15)) AS parentesco_bixa,
            CAST('TITULAR' AS VARCHAR(15)) AS parentesco,
            snemple.fecha_nac,
            snpolitrab.monto_pol,
            (snpolitrab.monto_pol - pol_ref.monto_pol    ) AS monto_emp,
            ISNULL(pol_ref.monto_pol, CAST(0.00 AS DECIMAL(18,2))) AS monto_pol_ref,
            CAST(sncontr_pol.campo1 AS DECIMAL(18,2)) AS dias,
            CAST(sncontr_pol.campo2 AS DECIMAL(18,6)) AS tasa,
            ISNULL(NM1.monto, CAST(0.00 AS DECIMAL(18,2))) AS mes1_E071,
            ISNULL(NM2.monto, CAST(0.00 AS DECIMAL(18,2))) AS mes2_E071,
            ISNULL(NM3.monto, CAST(0.00 AS DECIMAL(18,2))) AS mes3_E071

        INTO #Resultados
        FROM sncontr_pol
        INNER JOIN snpolitrab ON (snpolitrab.num_contrpol = sncontr_pol.num_contrpol)
        INNER JOIN snemple ON (snpolitrab.cod_emp = snemple.cod_emp)
        INNER JOIN snren_contrpol ON (snren_contrpol.num_contrpol = sncontr_pol.num_contrpol)

        LEFT JOIN snpolitrab AS pol_ref
               ON CAST(pol_ref.num_contrpol AS VARCHAR(30)) = LTRIM(RTRIM(sncontr_pol.campo3))
              AND pol_ref.cod_emp = snemple.cod_emp
        LEFT JOIN #NominaE071 NM1
               ON NM1.cod_emp = snemple.cod_emp
              AND NM1.anio = YEAR(snren_contrpol.desde)
              AND NM1.mes = MONTH(snren_contrpol.desde)
        LEFT JOIN #NominaE071 NM2
               ON NM2.cod_emp = snemple.cod_emp
              AND NM2.anio = YEAR(DATEADD(MONTH, 1, snren_contrpol.desde))
              AND NM2.mes = MONTH(DATEADD(MONTH, 1, snren_contrpol.desde))

        LEFT JOIN #NominaE071 NM3
               ON NM3.cod_emp = snemple.cod_emp
              AND NM3.anio = YEAR(DATEADD(MONTH, 2, snren_contrpol.desde))
              AND NM3.mes = MONTH(DATEADD(MONTH, 2, snren_contrpol.desde))

        WHERE
        ((@iNum_Contrpol_d IS NULL OR sncontr_pol.num_contrpol >= @iNum_Contrpol_d)
            AND (@iNum_Contrpol_h IS NULL OR sncontr_pol.num_contrpol <= @iNum_Contrpol_h ))
            AND ((@sCod_emp_d IS NULL OR snemple.cod_emp >= @sCod_emp_d)
            AND (@sCod_emp_h IS NULL OR snemple.cod_emp <= @sCod_emp_h ))
            AND (@sTipo_Poliza is null or @sTipo_Poliza = sncontr_pol.tipo_poliza)

        -- 2. Inserción de Familiares
        INSERT INTO #Resultados
        SELECT
            snemple.cod_emp,
            snemple.ci,
            sncontr_pol.num_contrpol,
            sncontr_pol.tipo_poliza,
            2 AS tipo_orden,
            sngru_fa.nombre AS nombre_completo,
        CAST(CASE snemple.status
            WHEN 'A' THEN 'Activo'
            WHEN 'I' THEN 'Inactivo'
            WHEN 'L' THEN 'Liquidado'
            WHEN 'O' THEN 'Otro'
            WHEN 'PL' THEN 'Por Liquidar'
            WHEN 'EL' THEN 'Parcialmente Liquidado' ELSE ''
        END AS VARCHAR(30)) AS status,
        CAST('FAMILIAR' AS VARCHAR(15)) AS parentesco_bixa,
            CAST(case when sngru_fa.log1=1 then 'TITULAR' else 'BENEFICIARIO' end AS VARCHAR(15)) AS parentesco,
        sngru_fa.fecha_nac,

            snpolifam.monto_poliza AS monto_pol,
            (snpolifam.monto_poliza - fam_ref.monto_poliza   ) AS monto_emp,
            ISNULL(fam_ref.monto_poliza, CAST(0.00 AS DECIMAL(18,2))) AS monto_pol_ref,
            CAST(sncontr_pol.campo1 AS DECIMAL(18,2)) AS dias,
            CAST(sncontr_pol.campo2 AS DECIMAL(18,6)) AS tasa,

            CAST(0.00 AS DECIMAL(18,2)) AS mes1_E071,
            CAST(0.00 AS DECIMAL(18,2)) AS mes2_E071,
            CAST(0.00 AS DECIMAL(18,2)) AS mes3_E071

        FROM sncontr_pol
        INNER JOIN snpolifam ON (snpolifam.num_contrpol = sncontr_pol.num_contrpol)
        INNER JOIN snemple ON (snpolifam.cod_emp = snemple.cod_emp)
        INNER JOIN sngru_fa ON (sngru_fa.co_gru_fa = snpolifam.co_gru_fa and sngru_fa.cod_emp = snemple.cod_emp)
        INNER JOIN snren_contrpol ON (snren_contrpol.num_contrpol = sncontr_pol.num_contrpol)

        LEFT JOIN snpolifam AS fam_ref
               ON CAST(fam_ref.num_contrpol AS VARCHAR(30)) = LTRIM(RTRIM(sncontr_pol.campo3))
              AND fam_ref.cod_emp = snemple.cod_emp
              AND fam_ref.co_gru_fa = sngru_fa.co_gru_fa

    WHERE
    ((@iNum_Contrpol_d IS NULL OR sncontr_pol.num_contrpol >= @iNum_Contrpol_d)
    AND (@iNum_Contrpol_h IS NULL OR sncontr_pol.num_contrpol <= @iNum_Contrpol_h ))
    AND ((@sCod_emp_d IS NULL OR snemple.cod_emp >= @sCod_emp_d)
    AND (@sCod_emp_h IS NULL OR snemple.cod_emp <= @sCod_emp_h ))

        -- Días de la cobertura 1 (titular): la cobertura 2 (familiar) no siempre trae su propio
        -- campo1/campo2, así que "dias" para el pago a Bixa se toma del titular del mismo empleado.
        DECLARE @diasTitular DECIMAL(18,2) = (
            SELECT TOP 1 r.dias
            FROM #Resultados r
            INNER JOIN snren_contrpol c ON r.num_contrpol = c.num_contrpol
            WHERE r.status = 'Activo' AND c.cobertura = 10000.00 AND r.tipo_orden = 1 AND r.cod_emp = @sCod_emp_d
            ORDER BY c.num_contrpol DESC
        );

        SELECT TOP 1
            c.cobertura AS Cobertura,
            r.cod_emp AS CodEmp,
            r.ci AS Ci,
            r.num_contrpol AS NumContrpol,
            r.tipo_poliza AS TipoPoliza,
            r.tipo_orden AS TipoOrden,
            r.nombre_completo AS NombreCompleto,
            r.status AS Status,
            r.parentesco_bixa AS ParentescoBixa,
            r.parentesco AS Parentesco,
            r.fecha_nac AS FechaNac,
            r.monto_pol AS MontoPol,
            r.monto_emp AS MontoEmp,
            r.monto_pol_ref AS MontoPolRef,
            ISNULL(r.dias, @diasTitular) AS Dias,
            r.tasa AS Tasa,
            r.mes1_E071 AS Mes1E071,
            r.mes2_E071 AS Mes2E071,
            r.mes3_E071 AS Mes3E071,
            CASE WHEN r.tipo_orden = 1 THEN CAST(((r.monto_pol / 365.0) * r.dias) * r.tasa AS DECIMAL(18,2)) END AS PrimaTrimBs,
            CASE WHEN r.tipo_orden = 2 THEN CAST((r.monto_emp / 365.0) * ISNULL(r.dias, @diasTitular) AS DECIMAL(18,2)) END AS PagoBixaTrim
        FROM #Resultados r INNER JOIN snren_contrpol c ON r.num_contrpol = c.num_contrpol
        WHERE r.status = 'Activo' AND c.cobertura = @covertura AND r.tipo_orden = @tipo_orden AND r.cod_emp = @sCod_emp_d
        ORDER BY c.num_contrpol DESC

        DROP TABLE #Resultados
        DROP TABLE #NominaE071
    """;
}