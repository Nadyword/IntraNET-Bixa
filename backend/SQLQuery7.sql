SELECT 
	des_novedad_dia, 
	fecha_registro,
	e.nombre_completo AstorisadoPor,
	a.desde,
	a.hasta,
	a.dias
FROM snnovedad_dia a 
INNER JOIN snemple e ON e.cod_emp = a.cod_emp
WHERE a.cod_emp = '00000104' AND a.co_tipoaus = '999' AND a.desde >= (SELECT val_f FROM snconst WHERE co_const = 'A003') and a.hasta <= (SELECT val_f FROM snconst WHERE co_const = 'A004')
