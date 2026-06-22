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
        INSERT INTO #EmpleadosTemp ( ci, nombre)  SELECT ci, nombre_completo FROM snemple WHERE cod_emp = @supervisor;

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
        WHERE AprobadorCi = '@ci'
    """;
}