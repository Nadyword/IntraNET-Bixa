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
        INNER JOIN dbo.sndepart AS d ON e.co_depart = d.co_depart
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
        INNER JOIN dbo.sndepart AS d ON e.co_depart = d.co_depart
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
            @iNum_Contrpol_d int = null,
            @iNum_Contrpol_h int = null,
            @sCod_emp_d char(17) = null,
            @sCod_emp_h char(17) = null,
            @ci VARCHAR(15) = @CiEmpleado,
            @covertura DECIMAL(18,2) = @Cover,
            @Parentesco VARCHAR(40),
            @NControl INT
            SET NOCOUNT ON;
            SET @Parentesco = IIF(@covertura = 10000.00,'EMPLEADO','');

            -- TOP 1 + ORDER BY: algunos empleados tienen más de un registro con la misma ci
            -- (reingresos). Se prioriza el registro Activo para evitar "Subquery returned more than 1 value".
            SET @sCod_emp_d = (SELECT TOP 1 cod_emp FROM snemple WHERE ci = @ci ORDER BY CASE WHEN status = 'A' THEN 0 ELSE 1 END, cod_emp DESC)
            SET @sCod_emp_h = @sCod_emp_d

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

        -- 1. Inserción de Titulares (Empleados)
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
                (snpolitrab.monto_pol - ISNULL(pol_ref.monto_pol, 0)) AS monto_emp,
                ISNULL(pol_ref.monto_pol, CAST(0.00 AS DECIMAL(18,2))) AS monto_pol_ref,
                -- campo1/campo2 son varchar y algunos registros usan coma como separador decimal
                -- (ej. '737,88'); sin REPLACE+TRY_CAST el CAST directo lanza el error 8114.
                TRY_CAST(REPLACE(sncontr_pol.campo1, ',', '.') AS DECIMAL(18,2)) AS dias,
                TRY_CAST(REPLACE(sncontr_pol.campo2, ',', '.') AS DECIMAL(18,6)) AS tasa,
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
                (snpolifam.monto_poliza - ISNULL(fam_ref.monto_poliza, 0)) AS monto_emp,
                ISNULL(fam_ref.monto_poliza, CAST(0.00 AS DECIMAL(18,2))) AS monto_pol_ref,
                TRY_CAST(REPLACE(sncontr_pol.campo1, ',', '.') AS DECIMAL(18,2)) AS dias,
                TRY_CAST(REPLACE(sncontr_pol.campo2, ',', '.') AS DECIMAL(18,6)) AS tasa,

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

            SET @NControl =  (SELECT Max(r.num_contrpol) FROM #Resultados r INNER JOIN snren_contrpol c ON r.num_contrpol = c.num_contrpol
            WHERE r.status = 'Activo' AND c.cobertura = @covertura AND r.cod_emp = @sCod_emp_d AND r.parentesco_bixa <> @Parentesco)

            SELECT
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
                CAST(((r.monto_pol / 365.0) * r.dias) * r.tasa AS DECIMAL(18,2)) AS PrimaTrimBs,
                CAST((r.monto_emp / 4) AS DECIMAL(18,2)) AS PagoBixaTrim
            FROM #Resultados r INNER JOIN snren_contrpol c ON r.num_contrpol = c.num_contrpol
            WHERE r.status = 'Activo' AND c.cobertura = @covertura AND r.cod_emp = @sCod_emp_d AND r.parentesco_bixa <> @Parentesco AND r.num_contrpol = @NControl
            ORDER BY c.num_contrpol DESC

            DROP TABLE #Resultados
            DROP TABLE #NominaE071
        """;

    internal const string GetConsultaARC = """

        DECLARE
                        @sCo_Emp_d char(17)= null,
                        @sCo_Emp_h char(17)= null,
                        @ci VARCHAR(15) = @CiEmpleado,
                        @iAnhio int = @anoActual,
                        @sCo_Cont char(12)=null,
                        @sCo_Depart char(12)=null,
                        @sCampOrderBy varchar(16) = null,
                        @sDir varchar(6) = null,
                        @bHeaderRep bit = 0

        	SET NOCOUNT ON;
                        -- TOP 1 + ORDER BY: algunos empleados tienen más de un registro con la misma ci
                        -- (reingresos). Se prioriza el registro Activo para evitar "Subquery returned more than 1 value".
                        SET @sCo_Emp_d = (SELECT TOP 1 cod_emp FROM snemple WHERE ci = @ci ORDER BY CASE WHEN status = 'A' THEN 0 ELSE 1 END, cod_emp DESC)
                        SET @sCo_Emp_h = @sCo_Emp_d
                        DECLARE @CodRemuneracion char(12), @CodPorcentRetencion char(12)
                        set @CodRemuneracion = dbo.GetConcepto('O012')
                        set @CodPorcentRetencion =  dbo.GetConcepto('R004')

                        SELECT  snemple.cod_emp,
                                                       snemple.nombre_completo,
                                                       (dbo.GetPrefixId(snemple.nac)) +snemple.ci as  cedula,
                                                       snemple.fecha_nac,
                                                       isnull(dbo.GetValorCampoAdi('G14TEL', 'GENE'),'') AS telefono,
                                                       isnull(dbo.GetValorCampoAdi('G10EST', 'GENE'),'') AS estado,
                                                       isnull(dbo.GetValorCampoAdi('G12CIU', 'GENE'),'') AS ciudad,
                                                       isnull(dbo.GetValorCampoAdi('G09DCM', 'GENE'),'') AS direccion,
                                                       [dbo].[GetMontoConcepMensAcum] (@CodRemuneracion, snrecibo.cod_emp, 1 , @iAnhio)as RemuneracionAcumulada,
                                                       1 as mes,

                                                       max([dbo].[GetPorcentRetencion] (@CodPorcentRetencion,snnomi.reci_num)) as PorcentRetencion,
                                                       sum([dbo].[GetMontoConceptoRecibo] (@CodRemuneracion,snnomi.reci_num)) as Remuneracion,

                                                       [dbo].[GetImpuestoRetenido](snrecibo.cod_emp,@CodPorcentRetencion,1,@iAnhio) as ImpuestoRetenido,
                                                       [dbo].[GetMontoImpuestoRetenidoAcum](snrecibo.cod_emp, 1 ,@iAnhio )as ImpuestoRetenidoAcum
                        FROM snemple
                                       INNER JOIN snrecibo ON (snrecibo.cod_emp = snemple.cod_emp)
                                       INNER JOIN snnomi ON (snnomi.reci_num = snrecibo.reci_num)

                        WHERE
                                       (MONTH(snrecibo.fec_emis) = 1)AND
                                       ((@sCo_Emp_d IS NULL OR @sCo_Emp_d <= snemple.cod_emp )
                                                                       AND (@sCo_Emp_h IS NULL OR snemple.cod_emp <= @sCo_Emp_h ))
                                       AND (YEAR(snrecibo.fec_emis) = @iAnhio)
                                       AND (snnomi.co_conce=@CodRemuneracion)
                                       AND (@sCo_Cont is null or @sCo_Cont = snemple.co_cont)
                                       AND (@sCo_Depart is null or @sCo_Depart = snemple.co_depart)

                        GROUP BY snrecibo.cod_emp, snnomi.cod_emp,  snemple.cod_emp, snemple.nombre_completo,snemple.nac,snemple.ci,snemple.fecha_nac,direccion, telefono, estado

                        union

                        SELECT  snemple.cod_emp,
                                                       snemple.nombre_completo,
                                                       (dbo.GetPrefixId(snemple.nac)) +snemple.ci  as  cedula,
                                                       snemple.fecha_nac,
                                                       isnull(dbo.GetValorCampoAdi('G14TEL', 'GENE'),'') AS telefono,
                                                       isnull(dbo.GetValorCampoAdi('G10EST', 'GENE'),'') AS estado,
                                                       isnull(dbo.GetValorCampoAdi('G12CIU', 'GENE'),'') AS ciudad,
                                                       isnull(dbo.GetValorCampoAdi('G09DCM', 'GENE'),'') AS direccion,
                                                       [dbo].[GetMontoConcepMensAcum] (@CodRemuneracion, snrecibo.cod_emp, 2 , @iAnhio)as RemuneracionAcumulada,
                                                       2 as mes,

                                                       max([dbo].[GetPorcentRetencion] (@CodPorcentRetencion,snnomi.reci_num)) as PorcentRetencion,
                                                       sum([dbo].[GetMontoConceptoRecibo] (@CodRemuneracion,snnomi.reci_num)) as Remuneracion,

                                                       [dbo].[GetImpuestoRetenido] (snrecibo.cod_emp,@CodPorcentRetencion,2,@iAnhio) as ImpuestoRetenido,
                                                       [dbo].[GetMontoImpuestoRetenidoAcum](snrecibo.cod_emp, 2 ,@iAnhio )as ImpuestoRetenidoAcum

                        FROM snemple
                                       INNER JOIN snrecibo ON (snrecibo.cod_emp = snemple.cod_emp)
                                       INNER JOIN snnomi ON (snnomi.reci_num = snrecibo.reci_num)

                        WHERE
                                       (MONTH(snrecibo.fec_emis) = 2)AND
                                       ((@sCo_Emp_d IS NULL OR @sCo_Emp_d <= snemple.cod_emp )
                                                                       AND (@sCo_Emp_h IS NULL OR snemple.cod_emp <= @sCo_Emp_h ))
                                       AND (YEAR(snrecibo.fec_emis) = @iAnhio)
                                       AND (snnomi.co_conce=@CodRemuneracion)
                                       AND (@sCo_Cont is null or @sCo_Cont = snemple.co_cont)
                                       AND (@sCo_Depart is null or @sCo_Depart = snemple.co_depart)

                        GROUP BY snrecibo.cod_emp,snnomi.cod_emp,  snemple.cod_emp, snemple.nombre_completo,snemple.nac,snemple.ci,snemple.fecha_nac,/*snnomi.co_cont,*/ direccion, telefono, estado

                        union

                        SELECT  snemple.cod_emp,
                                                       snemple.nombre_completo,
                                                       (dbo.GetPrefixId(snemple.nac)) + snemple.ci as  cedula,
                                                       snemple.fecha_nac,
                                                       isnull(dbo.GetValorCampoAdi('G14TEL', 'GENE'),'') AS telefono,
                                                       isnull(dbo.GetValorCampoAdi('G10EST', 'GENE'),'') AS estado,
                                                       isnull(dbo.GetValorCampoAdi('G12CIU', 'GENE'),'') AS ciudad,
                                                       isnull(dbo.GetValorCampoAdi('G09DCM', 'GENE'),'') AS direccion,
                                                       [dbo].[GetMontoConcepMensAcum] (@CodRemuneracion, snrecibo.cod_emp, 3 , @iAnhio)as RemuneracionAcumulada,
                                                       3 as mes,

                                                       max([dbo].[GetPorcentRetencion] (@CodPorcentRetencion,snnomi.reci_num)) as PorcentRetencion,
                                                       sum([dbo].[GetMontoConceptoRecibo] (@CodRemuneracion,snnomi.reci_num)) as Remuneracion,

                                                       [dbo].[GetImpuestoRetenido] (snrecibo.cod_emp,@CodPorcentRetencion,3,@iAnhio) as ImpuestoRetenido,
                                                       [dbo].[GetMontoImpuestoRetenidoAcum](snrecibo.cod_emp, 3 ,@iAnhio )as ImpuestoRetenidoAcum

                        FROM snemple
                                       INNER JOIN snrecibo ON (snrecibo.cod_emp = snemple.cod_emp)
                                       INNER JOIN snnomi ON (snnomi.reci_num = snrecibo.reci_num)

                        WHERE
                                       (MONTH(snrecibo.fec_emis) = 3)AND
                                       ((@sCo_Emp_d IS NULL OR @sCo_Emp_d <= snemple.cod_emp )
                                                                       AND (@sCo_Emp_h IS NULL OR snemple.cod_emp <= @sCo_Emp_h ))
                                       AND (YEAR(snrecibo.fec_emis) = @iAnhio)
                                       AND (snnomi.co_conce=@CodRemuneracion)
                                       AND (@sCo_Cont is null or @sCo_Cont = snemple.co_cont)
                                       AND (@sCo_Depart is null or @sCo_Depart = snemple.co_depart)

                        GROUP BY snrecibo.cod_emp,snnomi.cod_emp,  snemple.cod_emp, snemple.nombre_completo,snemple.nac,snemple.ci,snemple.fecha_nac,/*snnomi.co_cont,*/ direccion, telefono, estado

                        union

                        SELECT  snemple.cod_emp,
                                                       snemple.nombre_completo,
                                                       (dbo.GetPrefixId(snemple.nac)) + snemple.ci as  cedula,
                                                       snemple.fecha_nac,
                                                       isnull(dbo.GetValorCampoAdi('G14TEL', 'GENE'),'') AS telefono,
                                                       isnull(dbo.GetValorCampoAdi('G10EST', 'GENE'),'') AS estado,
                                                       isnull(dbo.GetValorCampoAdi('G12CIU', 'GENE'),'') AS ciudad,
                                                       isnull(dbo.GetValorCampoAdi('G09DCM', 'GENE'),'') AS direccion,
                                                       [dbo].[GetMontoConcepMensAcum] (@CodRemuneracion, snrecibo.cod_emp, 4 , @iAnhio)as RemuneracionAcumulada,
                                                       4 as mes,

                                                       max([dbo].[GetPorcentRetencion] (@CodPorcentRetencion,snnomi.reci_num)) as PorcentRetencion,
                                                       sum([dbo].[GetMontoConceptoRecibo] (@CodRemuneracion,snnomi.reci_num)) as Remuneracion,

                                                       [dbo].[GetImpuestoRetenido] (snrecibo.cod_emp,@CodPorcentRetencion,4,@iAnhio) as ImpuestoRetenido,
                                                       [dbo].[GetMontoImpuestoRetenidoAcum](snrecibo.cod_emp, 4 ,@iAnhio )as ImpuestoRetenidoAcum

                        FROM snemple
                                       INNER JOIN snrecibo ON (snrecibo.cod_emp = snemple.cod_emp)
                                       INNER JOIN snnomi ON (snnomi.reci_num = snrecibo.reci_num)

                        WHERE
                                       (MONTH(snrecibo.fec_emis) = 4)AND
                                       ((@sCo_Emp_d IS NULL OR @sCo_Emp_d <= snemple.cod_emp )
                                                                       AND (@sCo_Emp_h IS NULL OR snemple.cod_emp <= @sCo_Emp_h ))
                                       AND (YEAR(snrecibo.fec_emis) = @iAnhio)
                                       AND (snnomi.co_conce=@CodRemuneracion)
                                       AND (@sCo_Cont is null or @sCo_Cont = snemple.co_cont)
                                       AND (@sCo_Depart is null or @sCo_Depart = snemple.co_depart)

                        GROUP BY snrecibo.cod_emp,snnomi.cod_emp,  snemple.cod_emp, snemple.nombre_completo,snemple.nac,snemple.ci,snemple.fecha_nac,/*snnomi.co_cont,*/ direccion, telefono, estado

                        union

                        SELECT  snemple.cod_emp,
                                                       snemple.nombre_completo,
                                                       (dbo.GetPrefixId(snemple.nac)) + snemple.ci as  cedula,
                                                       snemple.fecha_nac,
                                                       isnull(dbo.GetValorCampoAdi('G14TEL', 'GENE'),'') AS telefono,
                                                       isnull(dbo.GetValorCampoAdi('G10EST', 'GENE'),'') AS estado,
                                                       isnull(dbo.GetValorCampoAdi('G12CIU', 'GENE'),'') AS ciudad,
                                                       isnull(dbo.GetValorCampoAdi('G09DCM', 'GENE'),'') AS direccion,
                                                       [dbo].[GetMontoConcepMensAcum] (@CodRemuneracion, snrecibo.cod_emp, 5 , @iAnhio)as RemuneracionAcumulada,
                                                       5 as mes,

                                                       max([dbo].[GetPorcentRetencion] (@CodPorcentRetencion,snnomi.reci_num)) as PorcentRetencion,
                                                       sum([dbo].[GetMontoConceptoRecibo] (@CodRemuneracion,snnomi.reci_num)) as Remuneracion,

                                                       [dbo].[GetImpuestoRetenido] (snrecibo.cod_emp,@CodPorcentRetencion,5,@iAnhio) as ImpuestoRetenido,
                                                       [dbo].[GetMontoImpuestoRetenidoAcum](snrecibo.cod_emp, 5 ,@iAnhio )as ImpuestoRetenidoAcum

                        FROM snemple
                                       INNER JOIN snrecibo ON (snrecibo.cod_emp = snemple.cod_emp)
                                       INNER JOIN snnomi ON (snnomi.reci_num = snrecibo.reci_num)

                        WHERE
                                       (MONTH(snrecibo.fec_emis) = 5)AND
                                       ((@sCo_Emp_d IS NULL OR @sCo_Emp_d <= snemple.cod_emp )
                                                                       AND (@sCo_Emp_h IS NULL OR snemple.cod_emp <= @sCo_Emp_h ))
                                       AND (YEAR(snrecibo.fec_emis) = @iAnhio)
                                       AND (snnomi.co_conce=@CodRemuneracion)
                                       AND (@sCo_Cont is null or @sCo_Cont = snemple.co_cont)
                                       AND (@sCo_Depart is null or @sCo_Depart = snemple.co_depart)

                        GROUP BY snrecibo.cod_emp, snnomi.cod_emp,  snemple.cod_emp, snemple.nombre_completo,snemple.nac,snemple.ci,snemple.fecha_nac,direccion, telefono, estado

                        union

                        SELECT  snemple.cod_emp,
                                                       snemple.nombre_completo,
                                                       (dbo.GetPrefixId(snemple.nac)) + snemple.ci as  cedula,
                                                       snemple.fecha_nac,
                                                       isnull(dbo.GetValorCampoAdi('G14TEL', 'GENE'),'') AS telefono,
                                                       isnull(dbo.GetValorCampoAdi('G10EST', 'GENE'),'') AS estado,
                                                       isnull(dbo.GetValorCampoAdi('G12CIU', 'GENE'),'') AS ciudad,
                                                       isnull(dbo.GetValorCampoAdi('G09DCM', 'GENE'),'') AS direccion,
                                                       [dbo].[GetMontoConcepMensAcum] (@CodRemuneracion, snrecibo.cod_emp, 6 , @iAnhio)as RemuneracionAcumulada,
                                                       6 as mes,

                                                       max([dbo].[GetPorcentRetencion] (@CodPorcentRetencion,snnomi.reci_num)) as PorcentRetencion,
                                                       sum([dbo].[GetMontoConceptoRecibo] (@CodRemuneracion,snnomi.reci_num)) as Remuneracion,

                                                       [dbo].[GetImpuestoRetenido] (snrecibo.cod_emp,@CodPorcentRetencion,6,@iAnhio) as ImpuestoRetenido,
                                                       [dbo].[GetMontoImpuestoRetenidoAcum](snrecibo.cod_emp, 6 ,@iAnhio )as ImpuestoRetenidoAcum

                        FROM snemple
                                       INNER JOIN snrecibo ON (snrecibo.cod_emp = snemple.cod_emp)
                                       INNER JOIN snnomi ON (snnomi.reci_num = snrecibo.reci_num)

                        WHERE
                                       (MONTH(snrecibo.fec_emis) = 6)AND
                                       ((@sCo_Emp_d IS NULL OR @sCo_Emp_d <= snemple.cod_emp )
                                                                       AND (@sCo_Emp_h IS NULL OR snemple.cod_emp <= @sCo_Emp_h ))
                                       AND (YEAR(snrecibo.fec_emis) = @iAnhio)
                                       AND (snnomi.co_conce=@CodRemuneracion)
                                       AND (@sCo_Cont is null or @sCo_Cont = snemple.co_cont)
                                       AND (@sCo_Depart is null or @sCo_Depart = snemple.co_depart)

                        GROUP BY snrecibo.cod_emp, snnomi.cod_emp,  snemple.cod_emp, snemple.nombre_completo,snemple.nac,snemple.ci,snemple.fecha_nac,/*snnomi.co_cont,*/ direccion, telefono, estado

                        union

                        SELECT  snemple.cod_emp,
                                                       snemple.nombre_completo,
                                                       (dbo.GetPrefixId(snemple.nac)) + snemple.ci as  cedula,
                                                       snemple.fecha_nac,
                                                       isnull(dbo.GetValorCampoAdi('G14TEL', 'GENE'),'') AS telefono,
                                                       isnull(dbo.GetValorCampoAdi('G10EST', 'GENE'),'') AS estado,
                                                       isnull(dbo.GetValorCampoAdi('G12CIU', 'GENE'),'') AS ciudad,
                                                       isnull(dbo.GetValorCampoAdi('G09DCM', 'GENE'),'') AS direccion,
                                                       [dbo].[GetMontoConcepMensAcum] (@CodRemuneracion, snrecibo.cod_emp, 7 , @iAnhio)as RemuneracionAcumulada,
                                                       7 as mes,

                                                       max([dbo].[GetPorcentRetencion] (@CodPorcentRetencion,snnomi.reci_num)) as PorcentRetencion,
                                                       sum([dbo].[GetMontoConceptoRecibo] (@CodRemuneracion,snnomi.reci_num)) as Remuneracion,

                                                       [dbo].[GetImpuestoRetenido] (snrecibo.cod_emp,@CodPorcentRetencion,7,@iAnhio) as ImpuestoRetenido,
                                                       [dbo].[GetMontoImpuestoRetenidoAcum](snrecibo.cod_emp, 7 ,@iAnhio )as ImpuestoRetenidoAcum

                        FROM snemple
                                       INNER JOIN snrecibo ON (snrecibo.cod_emp = snemple.cod_emp)
                                       INNER JOIN snnomi ON (snnomi.reci_num = snrecibo.reci_num)

                        WHERE
                                       (MONTH(snrecibo.fec_emis) = 7)AND
                                       ((@sCo_Emp_d IS NULL OR @sCo_Emp_d <= snemple.cod_emp )
                                                                       AND (@sCo_Emp_h IS NULL OR snemple.cod_emp <= @sCo_Emp_h ))
                                       AND (YEAR(snrecibo.fec_emis) = @iAnhio)
                                       AND (snnomi.co_conce=@CodRemuneracion)
                                       AND (@sCo_Cont is null or @sCo_Cont = snemple.co_cont)
                                       AND (@sCo_Depart is null or @sCo_Depart = snemple.co_depart)

                        GROUP BY snrecibo.cod_emp, snnomi.cod_emp,  snemple.cod_emp, snemple.nombre_completo,snemple.nac,snemple.ci,snemple.fecha_nac,direccion, telefono, estado

                        union

                        SELECT  snemple.cod_emp,
                                                       snemple.nombre_completo,
                                                       (dbo.GetPrefixId(snemple.nac)) + snemple.ci as  cedula,
                                                       snemple.fecha_nac,
                                                       isnull(dbo.GetValorCampoAdi('G14TEL', 'GENE'),'') AS telefono,
                                                       isnull(dbo.GetValorCampoAdi('G10EST', 'GENE'),'') AS estado,
                                                       isnull(dbo.GetValorCampoAdi('G12CIU', 'GENE'),'') AS ciudad,
                                                       isnull(dbo.GetValorCampoAdi('G09DCM', 'GENE'),'') AS direccion,
                                                       [dbo].[GetMontoConcepMensAcum] (@CodRemuneracion, snrecibo.cod_emp, 8 , @iAnhio)as RemuneracionAcumulada,
                                                       8 as mes,

                                                       max([dbo].[GetPorcentRetencion] (@CodPorcentRetencion,snnomi.reci_num)) as PorcentRetencion,
                                                       sum([dbo].[GetMontoConceptoRecibo] (@CodRemuneracion,snnomi.reci_num)) as Remuneracion,

                                                       [dbo].[GetImpuestoRetenido] (snrecibo.cod_emp,@CodPorcentRetencion,8,@iAnhio) as ImpuestoRetenido,
                                                       [dbo].[GetMontoImpuestoRetenidoAcum](snrecibo.cod_emp, 8 ,@iAnhio )as ImpuestoRetenidoAcum

                        FROM snemple
                                       INNER JOIN snrecibo ON (snrecibo.cod_emp = snemple.cod_emp)
                                       INNER JOIN snnomi ON (snnomi.reci_num = snrecibo.reci_num)

                        WHERE
                                       (MONTH(snrecibo.fec_emis) = 8)AND
                                       ((@sCo_Emp_d IS NULL OR @sCo_Emp_d <= snemple.cod_emp )
                                                                       AND (@sCo_Emp_h IS NULL OR snemple.cod_emp <= @sCo_Emp_h ))
                                       AND (YEAR(snrecibo.fec_emis) = @iAnhio)
                                       AND (snnomi.co_conce=@CodRemuneracion)
                                       AND (@sCo_Cont is null or @sCo_Cont = snemple.co_cont)
                                       AND (@sCo_Depart is null or @sCo_Depart = snemple.co_depart)

                        GROUP BY snrecibo.cod_emp, snnomi.cod_emp,  snemple.cod_emp, snemple.nombre_completo,snemple.nac,snemple.ci,snemple.fecha_nac,/*snnomi.co_cont,*/ direccion, telefono, estado

                        union

                        SELECT  snemple.cod_emp,
                                                       snemple.nombre_completo,
                                                       (dbo.GetPrefixId(snemple.nac)) + snemple.ci as  cedula,
                                                       snemple.fecha_nac,
                                                       isnull(dbo.GetValorCampoAdi('G14TEL', 'GENE'),'') AS telefono,
                                                       isnull(dbo.GetValorCampoAdi('G10EST', 'GENE'),'') AS estado,
                                                       isnull(dbo.GetValorCampoAdi('G12CIU', 'GENE'),'') AS ciudad,
                                                       isnull(dbo.GetValorCampoAdi('G09DCM', 'GENE'),'') AS direccion,
                                                       [dbo].[GetMontoConcepMensAcum] (@CodRemuneracion, snrecibo.cod_emp, 9 , @iAnhio)as RemuneracionAcumulada,
                                                       9 as mes,

                                                       max([dbo].[GetPorcentRetencion] (@CodPorcentRetencion,snnomi.reci_num)) as PorcentRetencion,
                                                       sum([dbo].[GetMontoConceptoRecibo] (@CodRemuneracion,snnomi.reci_num)) as Remuneracion,

                                                       [dbo].[GetImpuestoRetenido] (snrecibo.cod_emp,@CodPorcentRetencion,9,@iAnhio) as ImpuestoRetenido,
                                                       [dbo].[GetMontoImpuestoRetenidoAcum](snrecibo.cod_emp, 9 ,@iAnhio )as ImpuestoRetenidoAcum

                        FROM snemple
                                       INNER JOIN snrecibo ON (snrecibo.cod_emp = snemple.cod_emp)
                                       INNER JOIN snnomi ON (snnomi.reci_num = snrecibo.reci_num)

                        WHERE
                                       (MONTH(snrecibo.fec_emis) = 9)AND
                                       ((@sCo_Emp_d IS NULL OR @sCo_Emp_d <= snemple.cod_emp )
                                                                       AND (@sCo_Emp_h IS NULL OR snemple.cod_emp <= @sCo_Emp_h ))
                                       AND (YEAR(snrecibo.fec_emis) = @iAnhio)
                                       AND (snnomi.co_conce=@CodRemuneracion)
                                       AND (@sCo_Cont is null or @sCo_Cont = snemple.co_cont)
                                       AND (@sCo_Depart is null or @sCo_Depart = snemple.co_depart)

                        GROUP BY snrecibo.cod_emp, snnomi.cod_emp,  snemple.cod_emp, snemple.nombre_completo,snemple.nac,snemple.ci,snemple.fecha_nac,direccion, telefono, estado

                        union

                        SELECT  snemple.cod_emp,
                                                       snemple.nombre_completo,
                                                       (dbo.GetPrefixId(snemple.nac)) + snemple.ci as  cedula,
                                                       snemple.fecha_nac,
                                                       isnull(dbo.GetValorCampoAdi('G14TEL', 'GENE'),'') AS telefono,
                                                       isnull(dbo.GetValorCampoAdi('G10EST', 'GENE'),'') AS estado,
                                                       isnull(dbo.GetValorCampoAdi('G12CIU', 'GENE'),'') AS ciudad,
                                                       isnull(dbo.GetValorCampoAdi('G09DCM', 'GENE'),'') AS direccion,
                                                       [dbo].[GetMontoConcepMensAcum] (@CodRemuneracion, snrecibo.cod_emp, 10 , @iAnhio)as RemuneracionAcumulada,
                                                       10 as mes,

                                                       max([dbo].[GetPorcentRetencion] (@CodPorcentRetencion,snnomi.reci_num)) as PorcentRetencion,
                                                       sum([dbo].[GetMontoConceptoRecibo] (@CodRemuneracion,snnomi.reci_num)) as Remuneracion,

                                                       [dbo].[GetImpuestoRetenido] (snrecibo.cod_emp,@CodPorcentRetencion,10,@iAnhio) as ImpuestoRetenido,
                                                       [dbo].[GetMontoImpuestoRetenidoAcum](snrecibo.cod_emp, 10 ,@iAnhio )as ImpuestoRetenidoAcum

                        FROM snemple
                                       INNER JOIN snrecibo ON (snrecibo.cod_emp = snemple.cod_emp)
                                       INNER JOIN snnomi ON (snnomi.reci_num = snrecibo.reci_num)

                        WHERE
                                       (MONTH(snrecibo.fec_emis) = 10)AND
                                       ((@sCo_Emp_d IS NULL OR @sCo_Emp_d <= snemple.cod_emp )
                                                                       AND (@sCo_Emp_h IS NULL OR snemple.cod_emp <= @sCo_Emp_h ))
                                       AND (YEAR(snrecibo.fec_emis) = @iAnhio)
                                       AND (snnomi.co_conce=@CodRemuneracion)
                                       AND (@sCo_Cont is null or @sCo_Cont = snemple.co_cont)
                                       AND (@sCo_Depart is null or @sCo_Depart = snemple.co_depart)

                        GROUP BY snrecibo.cod_emp, snnomi.cod_emp,  snemple.cod_emp, snemple.nombre_completo,snemple.nac,snemple.ci,snemple.fecha_nac,direccion, telefono, estado

                        union

                        SELECT  snemple.cod_emp,
                                                       snemple.nombre_completo,
                                                       (dbo.GetPrefixId(snemple.nac)) + snemple.ci as  cedula,
                                                       snemple.fecha_nac,
                                                       isnull(dbo.GetValorCampoAdi('G14TEL', 'GENE'),'') AS telefono,
                                                       isnull(dbo.GetValorCampoAdi('G10EST', 'GENE'),'') AS estado,
                                                       isnull(dbo.GetValorCampoAdi('G12CIU', 'GENE'),'') AS ciudad,
                                                       isnull(dbo.GetValorCampoAdi('G09DCM', 'GENE'),'') AS direccion,
                                                       [dbo].[GetMontoConcepMensAcum] (@CodRemuneracion, snrecibo.cod_emp, 11 , @iAnhio)as RemuneracionAcumulada,
                                                       11 as mes,

                                                       max([dbo].[GetPorcentRetencion] (@CodPorcentRetencion,snnomi.reci_num)) as PorcentRetencion,
                                                       sum([dbo].[GetMontoConceptoRecibo] (@CodRemuneracion,snnomi.reci_num)) as Remuneracion,

                                                       [dbo].[GetImpuestoRetenido] (snrecibo.cod_emp,@CodPorcentRetencion,11,@iAnhio) as ImpuestoRetenido,
                                                       [dbo].[GetMontoImpuestoRetenidoAcum](snrecibo.cod_emp, 11 ,@iAnhio )as ImpuestoRetenidoAcum

                        FROM snemple
                                       INNER JOIN snrecibo ON (snrecibo.cod_emp = snemple.cod_emp)
                                       INNER JOIN snnomi ON (snnomi.reci_num = snrecibo.reci_num)

                        WHERE
                                       (MONTH(snrecibo.fec_emis) = 11)AND
                                       ((@sCo_Emp_d IS NULL OR @sCo_Emp_d <= snemple.cod_emp )
                                                                       AND (@sCo_Emp_h IS NULL OR snemple.cod_emp <= @sCo_Emp_h ))
                                       AND (YEAR(snrecibo.fec_emis) = @iAnhio)

                                       AND (snnomi.co_conce=@CodRemuneracion)
                                       AND (@sCo_Cont is null or @sCo_Cont = snemple.co_cont)
                                       AND (@sCo_Depart is null or @sCo_Depart = snemple.co_depart)

                        GROUP BY snrecibo.cod_emp, snnomi.cod_emp,  snemple.cod_emp, snemple.nombre_completo,snemple.nac,snemple.ci,snemple.fecha_nac,direccion, telefono, estado

                        union

                        SELECT  snemple.cod_emp,
                                                       snemple.nombre_completo,
                                                       (dbo.GetPrefixId(snemple.nac)) + snemple.ci as  cedula,
                                                       snemple.fecha_nac,
                                                       isnull(dbo.GetValorCampoAdi('G14TEL', 'GENE'),'') AS telefono,
                                                       isnull(dbo.GetValorCampoAdi('G10EST', 'GENE'),'') AS estado,
                                                       isnull(dbo.GetValorCampoAdi('G12CIU', 'GENE'),'') AS ciudad,
                                                       isnull(dbo.GetValorCampoAdi('G09DCM', 'GENE'),'') AS direccion,
                                                       [dbo].[GetMontoConcepMensAcum] (@CodRemuneracion, snrecibo.cod_emp, 12 , @iAnhio)as RemuneracionAcumulada,
                                                       12 as mes,

                                                       max([dbo].[GetPorcentRetencion] (@CodPorcentRetencion,snnomi.reci_num)) as PorcentRetencion,
                                                       sum([dbo].[GetMontoConceptoRecibo] (@CodRemuneracion,snnomi.reci_num)) as Remuneracion,

                                                       [dbo].[GetImpuestoRetenido] (snrecibo.cod_emp,@CodPorcentRetencion,12,@iAnhio) as ImpuestoRetenido,
                                                       [dbo].[GetMontoImpuestoRetenidoAcum](snrecibo.cod_emp, 12 ,@iAnhio )as ImpuestoRetenidoAcum

                        FROM snemple
                                       INNER JOIN snrecibo ON (snrecibo.cod_emp = snemple.cod_emp)
                                       INNER JOIN snnomi ON (snnomi.reci_num = snrecibo.reci_num)

                        WHERE
                                       (MONTH(snrecibo.fec_emis) = 12)AND
                                       ((@sCo_Emp_d IS NULL OR @sCo_Emp_d <= snemple.cod_emp )
                                                                       AND (@sCo_Emp_h IS NULL OR snemple.cod_emp <= @sCo_Emp_h ))
                                       AND (YEAR(snrecibo.fec_emis) = @iAnhio)
                                       AND (snnomi.co_conce=@CodRemuneracion)
                                       AND (@sCo_Cont is null or @sCo_Cont = snemple.co_cont)
                                       AND (@sCo_Depart is null or @sCo_Depart = snemple.co_depart)

                        GROUP BY snrecibo.cod_emp, snnomi.cod_emp,  snemple.cod_emp, snemple.nombre_completo,snemple.nac,snemple.ci,snemple.fecha_nac,direccion, telefono, estado

        """;

    internal const string GetPrestacionesSociales = """
        IF OBJECT_ID('tempdb..#temprestaIntra') IS NOT NULL
            DROP TABLE #temprestaIntra;

        IF OBJECT_ID('tempdb..#temprestaIntra2') IS NOT NULL
            DROP TABLE #temprestaIntra2;
          DECLARE

        	@sCod_Emp_d char(17) = null,
        	@sCod_Emp_h char(17) = null,
        	@sdFec_Nomina_d smalldatetime = null,
        	@sdFec_Nomina_h smalldatetime = null

        	-- TOP 1 + ORDER BY: algunos empleados tienen más de un registro con la misma ci
        	-- (reingresos). Se prioriza el registro Activo para evitar "Subquery returned more than 1 value".
        	SET @sCod_Emp_d = (SELECT TOP 1 cod_emp FROM snemple WHERE ci = @CiEmpleado ORDER BY CASE WHEN status = 'A' THEN 0 ELSE 1 END, cod_emp DESC)
        	SET @sCod_Emp_h = @sCod_Emp_d

        	select  top(1) @sdFec_Nomina_d = fec_emis from sngennomi, par_emp where co_cont = cont_pres order by fec_emis asc

        	Declare @strConceptoO004 char(12)
        	set @strConceptoO004 =  dbo.GetConcepto('O004')

        	Declare @strConceptoO005 char(12)
        	set @strConceptoO005 = dbo.GetConcepto('O005')

        	Declare @strConceptoO006 char(12)
        	set @strConceptoO006 =  dbo.GetConcepto('O006')

        	Declare @strConceptoO007 char(12)
        	set @strConceptoO007 =  dbo.GetConcepto('O007')

        	declare @strConceptoO010 char(12)
        	set @strConceptoO010 = dbo.GetConcepto('O010')

        	Declare @strConceptoO004_1 char(12)
        	set @strConceptoO004_1 =  dbo.GetConcepto('O004_1')

        	Declare @strConceptoZ015 char(12)
        	set @strConceptoZ015 = dbo.GetConcepto('Z015')

        	Declare @strConceptoO006_1 char(12)
        	set @strConceptoO006_1 =  dbo.GetConcepto('O006_1')

        	Declare @strConceptoO007_1 char(12)
        	set @strConceptoO007_1 =  dbo.GetConcepto('O007_1')

        	declare @strConceptoO010_1 char(12)
        	set @strConceptoO010_1 = dbo.GetConcepto('O010_1')

        	declare @strConceptoZ002 char(12)
        	set @strConceptoZ002 = dbo.GetConcepto('Z002')

        	declare @strConceptoZ003 char(12)
        	set @strConceptoZ003 = dbo.GetConcepto('Z003')

        	SELECT snrecibo.reci_num, Convert(smalldatetime,snrecibo.fec_emis,103) as fec_emis,
        	CAST(MONTH(snrecibo.fec_emis) AS varchar(2))+'-'+CAST(YEAR(snrecibo.fec_emis) AS varchar(4)) as fecha,

        	ISNULL((SELECT TOP(1)ISNULL(snnomi.monto,0) FROM snnomi WHERE snnomi.reci_num=snrecibo.reci_num and snnomi.cod_emp=snrecibo.cod_emp and snnomi.co_conce in (dbo.GetConcepto('O005')) order by snnomi.fec_emis),0) prest_asoc,

        	ISNULL((SELECT TOP(1)ISNULL(snnomi.monto,0) FROM snnomi WHERE snnomi.reci_num=snrecibo.reci_num and snnomi.cod_emp=snrecibo.cod_emp and snnomi.co_conce in (dbo.GetConcepto('Z015')) order by snnomi.fec_emis),0) prestamo1,

        	ISNULL((SELECT sum(snnomi.monto) FROM snnomi WHERE snnomi.reci_num=snrecibo.reci_num and snnomi.cod_emp=snrecibo.cod_emp and snnomi.co_conce in (dbo.GetConcepto('O006'),dbo.GetConcepto('O006_1'))),0) monto_dias_adicionales,

        	ISNULL((SELECT TOP (1) ISNULL(sum(snnomi.monto),0) FROM snnomi WHERE month(snnomi.fec_emis)=month(snrecibo.fec_emis) and year(snnomi.fec_emis)=year(snrecibo.fec_emis) and snnomi.cod_emp=snemple.cod_emp and snnomi.co_conce in (dbo.GetConcepto('O007'),dbo.GetConcepto('O007_1'))),0)antic_prest_soc,

        	ISNULL((SELECT TOP (1)ISNULL(val_n,0) FROM snhistor WHERE snhistor.cod_emp=snemple.cod_emp and snhistor.co_var = 'Z900'and snhistor.fecha=snrecibo.fec_emis and snhistor.co_cont=parEmp.cont_pres),0) acum_prest
        into #temprestaIntra
        FROM snrecibo
        	inner join dbo.snnomi as n on snrecibo.reci_num = n.reci_num and snrecibo.cod_emp = n.cod_emp AND
        												(@strConceptoO004 = n.co_conce OR
        														@strConceptoO005 = n.co_conce OR
        														@strConceptoO006 = n.co_conce OR
        														@strConceptoO007 = n.co_conce or
        														@strConceptoZ002 = n.co_conce OR
        														@strConceptoZ003 = n.co_conce OR
        														@strConceptoO010 = n.co_conce or
        														@strConceptoO004_1 = n.co_conce OR
        														@strConceptoZ015 = n.co_conce OR
        														@strConceptoO006_1 = n.co_conce OR
        														@strConceptoO007_1 = n.co_conce or
        														@strConceptoO010_1 = n.co_conce)
        	inner join snemple on snrecibo.cod_emp = snemple.cod_emp
        	inner join sndepart on snemple.co_depart = sndepart.co_depart
        	inner join sncont on snemple.co_cont = sncont.co_cont,dbo.par_emp as parEmp

        	WHERE
        		((@sCod_Emp_d IS NULL OR dbo.snrecibo.cod_emp >= @sCod_Emp_d)
        			AND (@sCod_Emp_h IS NULL OR (@sCod_Emp_d IS NULL AND dbo.snrecibo.cod_emp IS NULL) OR dbo.snrecibo.cod_emp <= @sCod_Emp_h))
        	AND ((@sdFec_Nomina_d IS NULL OR @sdFec_Nomina_d <= dbo.snrecibo.fec_emis )
        			AND (@sdFec_Nomina_h IS NULL OR dbo.snrecibo.fec_emis <= @sdFec_Nomina_h ))
        	AND ((snemple.status = 'A' or snemple.status = 'PL'))

        	ORDER BY snrecibo.fec_emis, snemple.cod_emp

        select
        	distinct max(reci_num) as reci_Num,
        	max(fec_emis) fec_emis,
        	fecha,
        	max(prest_asoc)prest_asoc,
        	max(prestamo1) prestamo1,
        	max(monto_dias_adicionales)monto_dias_adicionales,
        	max(antic_prest_soc)antic_prest_soc
        	INTO #temprestaIntra2
         from #temprestaIntra
        group by
        fecha

        SELECT  ((SUM(prest_asoc) + SUM(prestamo1) + SUM(monto_dias_adicionales)) - SUM(antic_prest_soc)) * 0.75 AS MontoDisponible  FROM #temprestaIntra2
        drop table #temprestaIntra
        drop table #temprestaIntra2
        """;

    /// <summary>
    /// Datos base de la planilla AR-I: identidad del contribuyente, valor de la U.T., carga familiar
    /// y la estimación de remuneraciones por percibir en el año gravable (casilla A de la planilla).
    /// Parámetro: @ciEmplea.
    /// </summary>
    internal const string GetARI = """
        IF OBJECT_ID('tempdb..#temprestaIntra') IS NOT NULL
            DROP TABLE #temprestaIntra;

        IF OBJECT_ID('tempdb..#ResultadoFinal') IS NOT NULL
            DROP TABLE #ResultadoFinal;

        DECLARE
            @sCod_Emp_d char(17) = (SELECT cod_emp FROM snemple WHERE ci = @ciEmplea),
            @sCod_Emp_h char(17) = (SELECT cod_emp FROM snemple WHERE ci = @ciEmplea),
            @sdFec_Nomina_d smalldatetime = NULL,
            @sdFec_Nomina_h smalldatetime = NULL;

        SELECT TOP(1) @sdFec_Nomina_d = fec_emis
        FROM sngennomi, par_emp
        WHERE co_cont = cont_pres
        ORDER BY fec_emis ASC;

        DECLARE @strConceptoZ002 char(12) = dbo.GetConcepto('Z002');

        SELECT snrecibo.reci_num,
               CONVERT(smalldatetime, snrecibo.fec_emis, 103) AS fec_emis,
               CAST(MONTH(snrecibo.fec_emis) AS varchar(2)) + '-' + CAST(YEAR(snrecibo.fec_emis) AS varchar(4)) AS fecha,
               ISNULL((SELECT TOP(1) ISNULL(snhistor.val_n, 0)
                       FROM snhistor
                       WHERE MONTH(snhistor.fecha) = MONTH(snrecibo.fec_emis)
                         AND YEAR(snhistor.fecha) = YEAR(snrecibo.fec_emis)
                         AND snhistor.cod_emp = snemple.cod_emp
                         AND snhistor.co_var = dbo.GetCampo('A001')
                       ORDER BY snhistor.fecha DESC), 0) AS sueldo
        INTO #temprestaIntra
        FROM snrecibo
        INNER JOIN dbo.snnomi AS n ON snrecibo.reci_num = n.reci_num AND snrecibo.cod_emp = n.cod_emp AND n.co_conce IN (@strConceptoZ002)
        INNER JOIN snemple ON snrecibo.cod_emp = snemple.cod_emp
        INNER JOIN sncont ON snemple.co_cont = sncont.co_cont, dbo.par_emp AS parEmp
        WHERE ((@sCod_Emp_d IS NULL OR dbo.snrecibo.cod_emp >= @sCod_Emp_d)
                AND (@sCod_Emp_h IS NULL OR (@sCod_Emp_d IS NULL AND dbo.snrecibo.cod_emp IS NULL) OR dbo.snrecibo.cod_emp <= @sCod_Emp_h))
          AND ((@sdFec_Nomina_d IS NULL OR @sdFec_Nomina_d <= dbo.snrecibo.fec_emis)
                AND (@sdFec_Nomina_h IS NULL OR dbo.snrecibo.fec_emis <= @sdFec_Nomina_h))
          AND (snemple.status = 'A' OR snemple.status = 'PL')
          AND YEAR(snrecibo.fec_emis) = YEAR(GETDATE());

        CREATE TABLE #ResultadoFinal (
            Mes INT,
            fec_emis SMALLDATETIME,
            fecha VARCHAR(10),
            sueldo DECIMAL(18,2)
        );

        INSERT INTO #ResultadoFinal (Mes, fec_emis, fecha, sueldo)
        SELECT MONTH(MAX(fec_emis)), MAX(fec_emis), fecha, MAX(sueldo)
        FROM #temprestaIntra
        GROUP BY fecha;

        DECLARE @UltimoMes INT, @UltimaFecha SMALLDATETIME, @UltimoSueldo DECIMAL(18,2);

        SELECT TOP 1
            @UltimoMes = Mes,
            @UltimaFecha = fec_emis,
            @UltimoSueldo = sueldo
        FROM #ResultadoFinal
        ORDER BY Mes DESC;

        -- Proyecta el último sueldo conocido hasta diciembre para completar el año gravable.
        WHILE @UltimoMes < 12
        BEGIN
            SET @UltimoMes = @UltimoMes + 1;
            SET @UltimaFecha = DATEADD(MONTH, 1, @UltimaFecha);

            INSERT INTO #ResultadoFinal (Mes, fec_emis, fecha, sueldo)
            VALUES (
                @UltimoMes, @UltimaFecha,
                CAST(@UltimoMes AS VARCHAR(2)) + '-' + CAST(YEAR(@UltimaFecha) AS VARCHAR(4)),
                @UltimoSueldo
            );
        END;

        DECLARE @SumaTotalMeses DECIMAL(18,2), @CalculoUltimoSueldo DECIMAL(18,2), @GranTotal DECIMAL(18,2);

        SELECT @SumaTotalMeses = SUM(sueldo) FROM #ResultadoFinal;
        SET @CalculoUltimoSueldo = (@UltimoSueldo / 30.0) * 100.0;
        SET @GranTotal = @SumaTotalMeses + @CalculoUltimoSueldo;

        SELECT
            'PRODUCTOS BIXA, S.A.' AS NombreEmpresa,
            nombre_completo AS NombreCompleto,
            ci AS Ci,
            rif AS Rif,
            (CAST(CONVERT(VARCHAR(8), GETDATE(), 112) AS INT) - CAST(CONVERT(VARCHAR(8), fecha_exp, 112) AS INT)) / 10000 AS AnosExp,
            CAST(43.3 AS DECIMAL(18,4)) AS UniTribu,
            3 AS CargaFami,
            @GranTotal + (@UltimoSueldo / 30) * (15 + (CAST(CONVERT(VARCHAR(8), GETDATE(), 112) AS INT) - CAST(CONVERT(VARCHAR(8), fecha_exp, 112) AS INT)) / 10000) AS GranTotal,
            YEAR(GETDATE()) AS AnoActual
        FROM snemple
        WHERE ci = @ciEmplea;

        DROP TABLE #temprestaIntra;
        DROP TABLE #ResultadoFinal;
        """;
}