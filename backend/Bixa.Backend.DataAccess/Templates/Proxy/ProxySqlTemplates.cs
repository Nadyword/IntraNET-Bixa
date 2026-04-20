namespace Bixa.Backend.DataAccess.Templates.Proxy;

internal static class ProxySqlTemplates
{
    internal const string? GetCiByEmai = """
        SELECT
                   e.cod_emp AS 'CodEmp',
                   e.nombres,
                   e.apellidos,
                   e.ci,
                   e.rif,
                   e.correo_e AS 'CorreoE',
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
                   e.grupo_sang AS 'GrupoSang'
               FROM dbo.snemple AS E
               INNER JOIN dbo.sncargo AS C ON e.co_cargo = c.co_cargo
               INNER JOIN dbo.sndepart AS D ON c.co_depart = d.co_depart
               INNER JOIN dbo.sncont AS S ON e.co_cont = s.co_cont
               INNER JOIN dbo.snubicacion AS U ON e.co_ubicacion = u.co_ubicacion
               WHERE e.ci = @ci
        """;
}