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
}