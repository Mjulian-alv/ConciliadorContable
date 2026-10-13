-- Fecha: 13/10/2026 - TAREA: 00083 - Linea: 2 - Migración de fechas de bancos en T-SQL puro, equivalente a MigradorFechasBancos, para correr en SSMS.
--
-- Qué hace
--   bancos.MovimientosArchivo y bancos.ConciliacionItemsExternos:
--     Fecha (texto)  ->  FechaTextoOriginal (texto, NULL)      se conserva el texto histórico
--     fecha2         ->  Fecha (date NOT NULL)                  columna operativa nueva
--   Completa sólo los fecha2 vacíos; nunca pisa un valor cargado. Si un fecha2 cargado
--   no coincide con el texto, o hay una fecha ilegible, ABORTA sin cambiar nada.
--
-- Cómo se usa
--   1) Con la app CERRADA (todos los puestos) y con un RESPALDO COMPLETO de la base:
--        BACKUP DATABASE [alvesu] TO DISK = N'...' WITH COPY_ONLY, CHECKSUM;
--   2) Ejecutar este script tal cual: @Aplicar = 0 sólo diagnostica y no modifica nada.
--      Revisar el resumen y que "incidencias" sea 0.
--   3) Cambiar @Aplicar a 1 y ejecutar de nuevo. Es una sola transacción: si algo falla,
--      se revierte todo. Una segunda ejecución reconoce que ya está migrada y no hace nada.
--   4) Desplegar la versión nueva de los ejecutables: los anteriores no funcionan con el
--      esquema migrado.
--
-- Reglas de interpretación (las mismas que el parser de la aplicación):
--   d/M/yyyy, d-M-yyyy y yyyy-MM-dd, con hora opcional (H:mm, H:mm:ss, H:mm:ss.fffffff;
--   en ISO también separador T y sufijo Z o ±hh:mm). Día primero. No se infiere mes/día.
--   Se conserva el día escrito, sin conversión de zona horaria.

USE [alvesu];   -- cambiar a [alvesu_pruebas] para ensayar sobre la copia
GO

SET NOCOUNT ON;
SET XACT_ABORT ON;
SET LANGUAGE Spanish;
SET DATEFORMAT dmy;

DECLARE @Aplicar bit = 0;   -- 0 = sólo diagnóstico (no modifica nada); 1 = migrar

PRINT 'Base: ' + DB_NAME() + '   Servidor: ' + @@SERVERNAME + '   Aplicar: ' + CAST(@Aplicar AS varchar(1));

-- ---------------------------------------------------------------------------------------
-- 1. Estado de las tablas
-- ---------------------------------------------------------------------------------------
CREATE TABLE #Tablas (Tabla sysname PRIMARY KEY, TipoFecha sysname NULL, FechaNula bit NULL, TieneFecha2 bit, TipoFecha2 sysname NULL, TieneOriginal bit);
INSERT #Tablas (Tabla) VALUES (N'MovimientosArchivo'), (N'ConciliacionItemsExternos');

UPDATE t SET
    TipoFecha     = (SELECT TYPE_NAME(c.system_type_id) FROM sys.columns c WHERE c.object_id = OBJECT_ID(N'bancos.' + t.Tabla) AND c.name = N'Fecha'),
    FechaNula     = (SELECT c.is_nullable               FROM sys.columns c WHERE c.object_id = OBJECT_ID(N'bancos.' + t.Tabla) AND c.name = N'Fecha'),
    TieneFecha2   = CASE WHEN EXISTS (SELECT 1 FROM sys.columns c WHERE c.object_id = OBJECT_ID(N'bancos.' + t.Tabla) AND c.name = N'fecha2') THEN 1 ELSE 0 END,
    TipoFecha2    = (SELECT TYPE_NAME(c.system_type_id) FROM sys.columns c WHERE c.object_id = OBJECT_ID(N'bancos.' + t.Tabla) AND c.name = N'fecha2'),
    TieneOriginal = CASE WHEN EXISTS (SELECT 1 FROM sys.columns c WHERE c.object_id = OBJECT_ID(N'bancos.' + t.Tabla) AND c.name = N'FechaTextoOriginal') THEN 1 ELSE 0 END
FROM #Tablas t;

SELECT Tabla, TipoFecha, FechaNula, TieneFecha2, TipoFecha2, TieneOriginal FROM #Tablas;

IF EXISTS (SELECT 1 FROM #Tablas WHERE TipoFecha IS NULL)
    THROW 50001, 'No se encontró la columna Fecha en alguna de las tablas: esta no es la base esperada.', 1;

-- Ya migrada: Fecha date NOT NULL y FechaTextoOriginal presente, en las dos tablas.
IF NOT EXISTS (SELECT 1 FROM #Tablas WHERE NOT (TipoFecha = N'date' AND FechaNula = 0 AND TieneOriginal = 1 AND TieneFecha2 = 0))
BEGIN
    PRINT 'Las dos tablas ya están migradas. No hay nada que hacer.';
    DROP TABLE #Tablas;
    RETURN;
END

-- Estado parcial o desconocido: se detiene con diagnóstico claro.
IF EXISTS (SELECT 1 FROM #Tablas WHERE TipoFecha NOT IN (N'nvarchar', N'varchar', N'nchar', N'char'))
    THROW 50002, 'Estado parcial: una tabla ya tiene Fecha tipada y la otra no. Resolver a mano antes de seguir.', 1;
IF EXISTS (SELECT 1 FROM #Tablas WHERE TieneOriginal = 1)
    THROW 50003, 'Estado parcial: existe FechaTextoOriginal con Fecha todavía textual. Resolver a mano antes de seguir.', 1;
IF EXISTS (SELECT 1 FROM #Tablas WHERE TieneFecha2 = 1 AND TipoFecha2 NOT IN (N'date', N'smalldatetime'))
    THROW 50004, 'fecha2 tiene un tipo no admitido (se esperaba smalldatetime o date).', 1;

-- Dependencias que impedirían cambiar el tipo (índices, restricciones, triggers, vistas, claves foráneas).
CREATE TABLE #Dependencias (Tabla sysname, Detalle nvarchar(400));
INSERT #Dependencias
SELECT t.Tabla, N'Índice: ' + i.name
FROM #Tablas t JOIN sys.indexes i ON i.object_id = OBJECT_ID(N'bancos.' + t.Tabla)
JOIN sys.index_columns ic ON ic.object_id = i.object_id AND ic.index_id = i.index_id
JOIN sys.columns c ON c.object_id = ic.object_id AND c.column_id = ic.column_id AND c.name IN (N'Fecha', N'fecha2')
UNION SELECT t.Tabla, N'Restricción: ' + o.name FROM #Tablas t JOIN sys.objects o ON o.parent_object_id = OBJECT_ID(N'bancos.' + t.Tabla) AND o.type = 'C'
UNION SELECT t.Tabla, N'Valor por defecto sobre fecha: ' + d.name FROM #Tablas t JOIN sys.default_constraints d ON d.parent_object_id = OBJECT_ID(N'bancos.' + t.Tabla)
    JOIN sys.columns c ON c.object_id = d.parent_object_id AND c.column_id = d.parent_column_id AND c.name IN (N'Fecha', N'fecha2')
UNION SELECT t.Tabla, N'Trigger: ' + tr.name FROM #Tablas t JOIN sys.triggers tr ON tr.parent_id = OBJECT_ID(N'bancos.' + t.Tabla) AND tr.is_disabled = 0
UNION SELECT t.Tabla, N'Objeto que la referencia: ' + OBJECT_SCHEMA_NAME(d.referencing_id) + N'.' + OBJECT_NAME(d.referencing_id)
    FROM #Tablas t JOIN sys.sql_expression_dependencies d ON d.referenced_id = OBJECT_ID(N'bancos.' + t.Tabla)
UNION SELECT t.Tabla, N'Clave foránea sobre fecha: ' + OBJECT_NAME(f.constraint_object_id)
    FROM #Tablas t JOIN sys.foreign_key_columns f ON (f.parent_object_id = OBJECT_ID(N'bancos.' + t.Tabla) OR f.referenced_object_id = OBJECT_ID(N'bancos.' + t.Tabla))
    JOIN sys.columns c ON ((c.object_id = f.parent_object_id AND c.column_id = f.parent_column_id) OR (c.object_id = f.referenced_object_id AND c.column_id = f.referenced_column_id)) AND c.name IN (N'Fecha', N'fecha2');

-- ---------------------------------------------------------------------------------------
-- 2. Lectura e interpretación de las fechas (sólo lectura)
-- ---------------------------------------------------------------------------------------
CREATE TABLE #F (
    Tabla sysname NOT NULL, Id int NOT NULL, Texto nvarchar(max) NULL, Inter datetime2(0) NULL,
    Interpretada date NULL, Motivo nvarchar(200) NULL,
    PRIMARY KEY (Tabla, Id));

DECLARE @sql nvarchar(max), @tabla sysname, @tiene2 bit;
DECLARE c CURSOR LOCAL FAST_FORWARD FOR SELECT Tabla, TieneFecha2 FROM #Tablas;
OPEN c; FETCH NEXT FROM c INTO @tabla, @tiene2;
WHILE @@FETCH_STATUS = 0
BEGIN
    SET @sql = N'INSERT #F (Tabla, Id, Texto, Inter) SELECT N''' + @tabla + N''', Id, Fecha, '
             + CASE WHEN @tiene2 = 1 THEN N'fecha2' ELSE N'NULL' END + N' FROM bancos.' + QUOTENAME(@tabla) + N';';
    EXEC (@sql);
    FETCH NEXT FROM c INTO @tabla, @tiene2;
END
CLOSE c; DEALLOCATE c;

-- Partición del texto: parte de fecha y parte de hora (separador: primer espacio o 'T').
;WITH P AS (
    SELECT f.Tabla, f.Id,
           t = LTRIM(RTRIM(f.Texto)),
           sep = NULLIF(PATINDEX(N'%[ T]%', LTRIM(RTRIM(f.Texto))), 0)
    FROM #F f
), Q AS (
    SELECT P.Tabla, P.Id, P.t, P.sep,
           d = CASE WHEN P.sep IS NULL THEN P.t ELSE LEFT(P.t, P.sep - 1) END,
           h = CASE WHEN P.sep IS NULL THEN NULL ELSE SUBSTRING(P.t, P.sep + 1, 100) END,
           sepc = CASE WHEN P.sep IS NULL THEN NULL ELSE SUBSTRING(P.t, P.sep, 1) END
    FROM P
), R AS (
    SELECT Q.*,
           forma = CASE
                WHEN TRANSLATE(Q.d, N'0123456789', N'9999999999') IN (N'9/9/9999', N'9/99/9999', N'99/9/9999', N'99/99/9999') THEN 'S'
                WHEN TRANSLATE(Q.d, N'0123456789', N'9999999999') IN (N'9-9-9999', N'9-99-9999', N'99-9-9999', N'99-99-9999') THEN 'G'
                WHEN TRANSLATE(Q.d, N'0123456789', N'9999999999') = N'9999-99-99' THEN 'I'
                END
    FROM Q
), S AS (
    SELECT R.*,
           delim = CASE R.forma WHEN 'S' THEN N'/' WHEN 'G' THEN N'-' END,
           p1 = CASE WHEN R.forma IN ('S','G') THEN CHARINDEX(CASE R.forma WHEN 'S' THEN N'/' ELSE N'-' END, R.d) END
    FROM R
), T AS (
    SELECT S.*, p2 = CASE WHEN S.p1 IS NOT NULL THEN CHARINDEX(S.delim, S.d, S.p1 + 1) END FROM S
), U AS (
    SELECT T.*,
           dd = CASE T.forma WHEN 'I' THEN SUBSTRING(T.d, 9, 2) ELSE LEFT(T.d, T.p1 - 1) END,
           mm = CASE T.forma WHEN 'I' THEN SUBSTRING(T.d, 6, 2) ELSE SUBSTRING(T.d, T.p1 + 1, T.p2 - T.p1 - 1) END,
           yy = CASE T.forma WHEN 'I' THEN LEFT(T.d, 4) ELSE SUBSTRING(T.d, T.p2 + 1, 4) END,
           -- hora sin sufijo de zona (Z o ±hh:mm, que sólo admite el formato ISO)
           hs = CASE WHEN T.h IS NULL THEN NULL
                     WHEN T.h LIKE N'%Z' THEN LEFT(T.h, LEN(T.h) - 1)
                     WHEN PATINDEX(N'%[+-][0-9][0-9]:[0-9][0-9]', T.h) > 0 THEN LEFT(T.h, LEN(T.h) - 6)
                     ELSE T.h END,
           zona = CASE WHEN T.h LIKE N'%Z' OR PATINDEX(N'%[+-][0-9][0-9]:[0-9][0-9]', T.h) > 0 THEN 1 ELSE 0 END
    FROM T
)
UPDATE f SET
    Interpretada = CASE WHEN U.forma IS NULL THEN NULL
                        WHEN U.sepc = N'T' AND U.forma <> 'I' THEN NULL                       -- 'T' sólo en ISO
                        WHEN U.zona = 1 AND U.forma <> 'I' THEN NULL                           -- zona sólo en ISO
                        WHEN U.h IS NOT NULL AND TRY_CONVERT(time(7), U.hs) IS NULL THEN NULL  -- hora ilegible
                        WHEN U.h IS NOT NULL AND U.hs NOT LIKE N'[0-9]:[0-9][0-9]%' AND U.hs NOT LIKE N'[0-9][0-9]:[0-9][0-9]%' THEN NULL
                        ELSE TRY_CONVERT(date, U.yy + N'-' + RIGHT(N'0' + U.mm, 2) + N'-' + RIGHT(N'0' + U.dd, 2), 23)
                   END
FROM #F f JOIN U ON U.Tabla = f.Tabla AND U.Id = f.Id;

UPDATE #F SET Motivo = N'Fecha textual vacía o inválida' WHERE Interpretada IS NULL;
-- fecha2 ya cargada debe coincidir con el día interpretado y no tener hora.
UPDATE #F SET Motivo = N'fecha2 difiere del día original o contiene hora'
WHERE Interpretada IS NOT NULL AND Inter IS NOT NULL
  AND (CAST(Inter AS date) <> Interpretada OR CAST(Inter AS time(0)) <> CAST('00:00:00' AS time(0)));

-- ---------------------------------------------------------------------------------------
-- 3. Resumen
-- ---------------------------------------------------------------------------------------
SELECT Tabla, COUNT(*) AS filas,
       SUM(CASE WHEN Inter IS NOT NULL THEN 1 ELSE 0 END) AS fecha2_cargadas,
       SUM(CASE WHEN Inter IS NULL THEN 1 ELSE 0 END)     AS fecha2_a_completar,
       SUM(CASE WHEN Motivo IS NOT NULL THEN 1 ELSE 0 END) AS incidencias,
       MIN(Interpretada) AS fecha_minima, MAX(Interpretada) AS fecha_maxima
FROM #F GROUP BY Tabla;

SELECT TOP (200) Tabla, Id, Texto, Inter AS fecha2, Motivo AS incidencia FROM #F WHERE Motivo IS NOT NULL ORDER BY Tabla, Id;
SELECT Tabla, Detalle AS dependencia_que_bloquea FROM #Dependencias;

DECLARE @incidencias int = (SELECT COUNT(*) FROM #F WHERE Motivo IS NOT NULL) + (SELECT COUNT(*) FROM #Dependencias);

IF @Aplicar = 0
BEGIN
    PRINT CASE WHEN @incidencias = 0 THEN 'DIAGNÓSTICO LIMPIO: se puede aplicar (cambiar @Aplicar a 1).'
               ELSE 'HAY INCIDENCIAS (' + CAST(@incidencias AS varchar(12)) + '): no se puede aplicar hasta resolverlas.' END;
    DROP TABLE #F, #Tablas, #Dependencias;
    RETURN;
END

IF @incidencias <> 0
    THROW 50010, 'Migración cancelada: hay incidencias o dependencias (ver resultados). No se modificó nada.', 1;

-- ---------------------------------------------------------------------------------------
-- 4. Aplicación (una transacción; todo o nada)
-- ---------------------------------------------------------------------------------------
BEGIN TRANSACTION;

DECLARE @lock int;
EXEC @lock = sys.sp_getapplock @Resource = N'bancos.Conciliacion.Mutacion', @LockMode = N'Exclusive', @LockOwner = N'Transaction', @LockTimeout = 15000;
IF @lock < 0 THROW 50011, 'Hay otra operación de bancos en curso.', 1;

-- Bloqueo exclusivo y relectura: lo diagnosticado arriba no puede haber cambiado.
SELECT COUNT_BIG(*) FROM bancos.MovimientosArchivo WITH (TABLOCKX, HOLDLOCK);
SELECT COUNT_BIG(*) FROM bancos.ConciliacionItemsExternos WITH (TABLOCKX, HOLDLOCK);
IF (SELECT COUNT(*) FROM #F WHERE Tabla = N'MovimientosArchivo')        <> (SELECT COUNT(*) FROM bancos.MovimientosArchivo)
OR (SELECT COUNT(*) FROM #F WHERE Tabla = N'ConciliacionItemsExternos') <> (SELECT COUNT(*) FROM bancos.ConciliacionItemsExternos)
    THROW 50012, 'Las tablas cambiaron desde el diagnóstico (se importó algo). Cerrar la aplicación y repetir. No se modificó nada.', 1;

DECLARE c2 CURSOR LOCAL FAST_FORWARD FOR SELECT Tabla, TieneFecha2 FROM #Tablas;
OPEN c2; FETCH NEXT FROM c2 INTO @tabla, @tiene2;
WHILE @@FETCH_STATUS = 0
BEGIN
    DECLARE @obj nvarchar(300) = N'bancos.' + QUOTENAME(@tabla);

    -- 4.1 Columna intermedia a date (se crea si esta tabla no la tenía)
    SET @sql = CASE WHEN @tiene2 = 1 THEN N'ALTER TABLE ' + @obj + N' ALTER COLUMN fecha2 date NULL;'
                    ELSE N'ALTER TABLE ' + @obj + N' ADD fecha2 date NULL;' END;
    EXEC (@sql);

    -- 4.2 Completar sólo los vacíos con la fecha interpretada
    SET @sql = N'UPDATE m SET fecha2 = f.Interpretada FROM ' + @obj + N' m JOIN #F f ON f.Tabla = N''' + @tabla + N''' AND f.Id = m.Id WHERE m.fecha2 IS NULL;';
    EXEC (@sql);

    -- 4.3 Verificar antes de renombrar: ninguna diferencia ni vacío
    DECLARE @dif int;
    SET @sql = N'SELECT @n = COUNT(*) FROM ' + @obj + N' m FULL JOIN (SELECT * FROM #F WHERE Tabla = N''' + @tabla + N''') f ON f.Id = m.Id WHERE m.Id IS NULL OR f.Id IS NULL OR m.fecha2 IS NULL OR m.fecha2 <> f.Interpretada;';
    EXEC sys.sp_executesql @sql, N'@n int OUTPUT', @n = @dif OUTPUT;
    IF @dif <> 0 THROW 50013, 'La columna intermedia difiere de la conversión validada. Se revierte todo.', 1;

    -- 4.4 Cambio de nombres: el texto queda como FechaTextoOriginal y fecha2 pasa a ser Fecha
    SET @sql = @obj + N'.Fecha';  EXEC sys.sp_rename @sql, N'FechaTextoOriginal', 'COLUMN';
    SET @sql = N'ALTER TABLE ' + @obj + N' ALTER COLUMN FechaTextoOriginal nvarchar(max) NULL;';  EXEC (@sql);
    SET @sql = @obj + N'.fecha2'; EXEC sys.sp_rename @sql, N'Fecha', 'COLUMN';
    SET @sql = N'ALTER TABLE ' + @obj + N' ALTER COLUMN Fecha date NOT NULL;';  EXEC (@sql);

    FETCH NEXT FROM c2 INTO @tabla, @tiene2;
END
CLOSE c2; DEALLOCATE c2;

-- 4.5 Verificación final: tipos, cantidades, texto original idéntico y fecha igual a la interpretada
IF (SELECT COUNT(*) FROM sys.columns WHERE object_id IN (OBJECT_ID(N'bancos.MovimientosArchivo'), OBJECT_ID(N'bancos.ConciliacionItemsExternos'))
        AND name = N'Fecha' AND TYPE_NAME(system_type_id) = N'date' AND is_nullable = 0) <> 2
    THROW 50014, 'Los tipos finales no son los esperados. Se revierte todo.', 1;

-- El filtro por tabla va en una subconsulta: dentro del ON de un FULL JOIN dejaría las filas de la otra tabla como "sin pareja".
IF EXISTS (SELECT 1 FROM bancos.MovimientosArchivo m FULL JOIN (SELECT * FROM #F WHERE Tabla = N'MovimientosArchivo') f ON f.Id = m.Id
           WHERE m.Id IS NULL OR f.Id IS NULL OR m.Fecha <> f.Interpretada OR ISNULL(m.FechaTextoOriginal, N'') <> ISNULL(f.Texto, N''))
OR EXISTS (SELECT 1 FROM bancos.ConciliacionItemsExternos m FULL JOIN (SELECT * FROM #F WHERE Tabla = N'ConciliacionItemsExternos') f ON f.Id = m.Id
           WHERE m.Id IS NULL OR f.Id IS NULL OR m.Fecha <> f.Interpretada OR ISNULL(m.FechaTextoOriginal, N'') <> ISNULL(f.Texto, N''))
    THROW 50015, 'La verificación final falló (fechas o textos distintos). Se revierte todo.', 1;

COMMIT TRANSACTION;

PRINT 'MIGRACIÓN CONFIRMADA.';
SELECT Tabla, COUNT(*) AS filas, MIN(Interpretada) AS fecha_minima, MAX(Interpretada) AS fecha_maxima FROM #F GROUP BY Tabla;
DROP TABLE #F, #Tablas, #Dependencias;
