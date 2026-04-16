-- Created by GitHub Copilot in SSMS - review carefully before executing
CREATE VIEW dbo.VW_SSMisDatos AS
SELECT 
    e.cod_emp,
    e.nombres, 
    e.apellidos, 
    e.ci, 
    e.rif, 
    e.correo_e, 
    e.fecha_nac,
    e.direccion,
    DATEDIFF(YEAR, e.fecha_nac, GETDATE()) 
      - CASE WHEN DATEADD(YEAR, DATEDIFF(YEAR, e.fecha_nac, GETDATE()), e.fecha_nac) > GETDATE() THEN 1 ELSE 0 END AS edad,
    CASE 
        WHEN e.sexo = 'M' THEN 'Masculino' 
        WHEN e.sexo = 'F' THEN 'Femenino' 
    END AS sexo,
    DATEDIFF(YEAR, e.fecha_ing, GETDATE()) 
      - CASE WHEN DATEADD(YEAR, DATEDIFF(YEAR, e.fecha_ing, GETDATE()), e.fecha_ing) > GETDATE() THEN 1 ELSE 0 END AS timeAtEm,
    e.fecha_ing,
    e.telefono,
    CASE 
        WHEN e.edo_civ = 'S' THEN 'Soltero(a)' 
        WHEN e.edo_civ = 'C' THEN 'Casado(a)' 
        WHEN e.edo_civ = 'D' THEN 'Divorciado(a)' 
        WHEN e.edo_civ = 'V' THEN 'Viudo(a)' 
    END AS estado_civil,
    c.des_cargo,
    d.des_depart,
    s.des_cont,
    u.des_ubicacion,
    e.grupo_sang
FROM dbo.snemple AS e
INNER JOIN dbo.sncargo AS c ON e.co_cargo = c.co_cargo
INNER JOIN dbo.sndepart AS d ON c.co_depart = d.co_depart
INNER JOIN dbo.sncont AS s ON e.co_cont = s.co_cont 
INNER JOIN dbo.snubicacion AS u ON e.co_ubicacion = u.co_ubicacion;