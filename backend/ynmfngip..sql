DECLARE @sCod_Emp_d char(17) = '00000104';

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

SELECT cod_emp, Nombre, desde, hasta, dias FROM @TablaRecibo;