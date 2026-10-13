// Fecha: 13/10/2026 - TAREA: 00083 - Linea: 2 - Identificadores limitados a las dos tablas; ninguna conversión depende del idioma de SQL.
using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;
namespace MigradorFechasBancos.Data;
internal sealed record Columna(string Nombre, string Tipo, bool Nula);
internal sealed class FilaFecha { public int Id { get; set; } public object Fecha { get; set; } public string Texto { get; set; } public DateTime? Intermedia { get; set; } }
internal static class MigracionFechaStorage
{
    internal static readonly string[] Tablas = { "bancos.MovimientosArchivo", "bancos.ConciliacionItemsExternos" };
    static bool EsMovimiento(string tabla) => tabla switch { "bancos.MovimientosArchivo" => true, "bancos.ConciliacionItemsExternos" => false, _ => throw new ArgumentException("Tabla no admitida") };
    public static List<Columna> Columnas(SqlConnection cn, SqlTransaction tx, string tabla) => cn.Query<Columna>(
        "SELECT name Nombre, TYPE_NAME(system_type_id) Tipo, is_nullable Nula FROM sys.columns WHERE object_id=OBJECT_ID(@Tabla)", new { Tabla = tabla }, tx).ToList();
    public static List<string> Dependencias(SqlConnection cn, SqlTransaction tx, string tabla) => cn.Query<string>(@"
        SELECT DISTINCT 'Índice: '+i.name FROM sys.indexes i JOIN sys.index_columns ic ON ic.object_id=i.object_id AND ic.index_id=i.index_id
          JOIN sys.columns c ON c.object_id=ic.object_id AND c.column_id=ic.column_id WHERE i.object_id=OBJECT_ID(@Tabla) AND c.name IN ('Fecha','fecha2')
        UNION SELECT 'Restricción: '+o.name FROM sys.objects o WHERE o.parent_object_id=OBJECT_ID(@Tabla) AND o.type IN ('C','D')
          AND (o.type='C' OR EXISTS(SELECT 1 FROM sys.default_constraints d JOIN sys.columns c ON c.object_id=d.parent_object_id AND c.column_id=d.parent_column_id WHERE d.object_id=o.object_id AND c.name IN ('Fecha','fecha2')))
        UNION SELECT 'Dependencia SQL: '+OBJECT_SCHEMA_NAME(d.referencing_id)+'.'+OBJECT_NAME(d.referencing_id) FROM sys.sql_expression_dependencies d
          WHERE d.referenced_id=OBJECT_ID(@Tabla) AND (d.referenced_minor_id=0 OR d.referenced_minor_id IN (SELECT column_id FROM sys.columns WHERE object_id=OBJECT_ID(@Tabla) AND name IN ('Fecha','fecha2')))
        UNION SELECT 'Trigger: '+name FROM sys.triggers WHERE parent_id=OBJECT_ID(@Tabla) AND is_disabled=0
        UNION SELECT 'Clave foránea: '+OBJECT_NAME(f.constraint_object_id) FROM sys.foreign_key_columns f
          WHERE (f.parent_object_id=OBJECT_ID(@Tabla) AND f.parent_column_id IN (SELECT column_id FROM sys.columns WHERE object_id=OBJECT_ID(@Tabla) AND name IN ('Fecha','fecha2')))
             OR (f.referenced_object_id=OBJECT_ID(@Tabla) AND f.referenced_column_id IN (SELECT column_id FROM sys.columns WHERE object_id=OBJECT_ID(@Tabla) AND name IN ('Fecha','fecha2')))", new { Tabla = tabla }, tx).ToList();
    public static List<FilaFecha> Filas(SqlConnection cn, SqlTransaction tx, string tabla, bool final, bool intermedia, bool original)
    {
        bool m = EsMovimiento(tabla);
        string sql = final
            ? (original ? (m ? "SELECT Id, Fecha, FechaTextoOriginal Texto FROM bancos.MovimientosArchivo" : "SELECT Id, Fecha, FechaTextoOriginal Texto FROM bancos.ConciliacionItemsExternos")
                        : (m ? "SELECT Id, Fecha FROM bancos.MovimientosArchivo" : "SELECT Id, Fecha FROM bancos.ConciliacionItemsExternos"))
            : (intermedia ? (m ? "SELECT Id, Fecha Texto, fecha2 Intermedia FROM bancos.MovimientosArchivo" : "SELECT Id, Fecha Texto, fecha2 Intermedia FROM bancos.ConciliacionItemsExternos")
                          : (m ? "SELECT Id, Fecha Texto FROM bancos.MovimientosArchivo" : "SELECT Id, Fecha Texto FROM bancos.ConciliacionItemsExternos"));
        return cn.Query<FilaFecha>(sql, transaction: tx).ToList();
    }
    public static void Bloquear(SqlConnection cn, SqlTransaction tx)
    {
        int r = cn.ExecuteScalar<int>(@"DECLARE @r int; EXEC @r=sys.sp_getapplock @Resource=N'bancos.Conciliacion.Mutacion', @LockMode=N'Exclusive', @LockOwner=N'Transaction', @LockTimeout=15000; SELECT @r", transaction: tx);
        if (r < 0) throw new InvalidOperationException("Hay otra operación de bancos en curso.");
        cn.Execute("SELECT COUNT_BIG(*) FROM bancos.MovimientosArchivo WITH(TABLOCKX,HOLDLOCK); SELECT COUNT_BIG(*) FROM bancos.ConciliacionItemsExternos WITH(TABLOCKX,HOLDLOCK)", transaction: tx);
    }
    // Fecha: 13/10/2026 - TAREA: 00083 - Linea: 2 - Comprobar también las columnas que la migración no debe tocar.
    public static string Huella(SqlConnection cn, SqlTransaction tx, string tabla)
    {
        EsMovimiento(tabla);
        return cn.QuerySingle<string>(@"DECLARE @columnas nvarchar(max),@sql nvarchar(max),@json nvarchar(max);
            SELECT @columnas=STRING_AGG(CAST(QUOTENAME(name) AS nvarchar(max)),',') WITHIN GROUP (ORDER BY column_id)
            FROM sys.columns WHERE object_id=OBJECT_ID(@Tabla) AND name NOT IN ('Fecha','fecha2','FechaTextoOriginal');
            SET @sql=N'SELECT @v=(SELECT '+@columnas+' FROM '+QUOTENAME(PARSENAME(@Tabla,2))+'.'+QUOTENAME(PARSENAME(@Tabla,1))+' ORDER BY Id FOR JSON PATH, INCLUDE_NULL_VALUES)';
            EXEC sys.sp_executesql @sql,N'@v nvarchar(max) OUTPUT',@v=@json OUTPUT;
            SELECT CONVERT(varchar(64),HASHBYTES('SHA2_256',@json),2)", new {Tabla=tabla},tx);
    }
    public static string Relaciones(SqlConnection cn, SqlTransaction tx) => cn.QuerySingle<string>(@"
        DECLARE @a nvarchar(max)='',@b nvarchar(max)='',@c nvarchar(max)='',@d nvarchar(max)='',@e nvarchar(max)='';
        IF OBJECT_ID('bancos.ArchivosImportados') IS NOT NULL EXEC sys.sp_executesql N'SELECT @v=(SELECT * FROM bancos.ArchivosImportados WITH(HOLDLOCK) ORDER BY Id FOR JSON PATH, INCLUDE_NULL_VALUES)',N'@v nvarchar(max) OUTPUT',@v=@a OUTPUT;
        IF OBJECT_ID('bancos.ConciliacionSesiones') IS NOT NULL EXEC sys.sp_executesql N'SELECT @v=(SELECT * FROM bancos.ConciliacionSesiones WITH(HOLDLOCK) ORDER BY Id FOR JSON PATH, INCLUDE_NULL_VALUES)',N'@v nvarchar(max) OUTPUT',@v=@b OUTPUT;
        IF OBJECT_ID('bancos.ConciliacionPares') IS NOT NULL EXEC sys.sp_executesql N'SELECT @v=(SELECT * FROM bancos.ConciliacionPares WITH(HOLDLOCK) ORDER BY Id FOR JSON PATH, INCLUDE_NULL_VALUES)',N'@v nvarchar(max) OUTPUT',@v=@c OUTPUT;
        IF OBJECT_ID('bancos.ConciliacionInternaSesiones') IS NOT NULL EXEC sys.sp_executesql N'SELECT @v=(SELECT * FROM bancos.ConciliacionInternaSesiones WITH(HOLDLOCK) ORDER BY Id FOR JSON PATH, INCLUDE_NULL_VALUES)',N'@v nvarchar(max) OUTPUT',@v=@d OUTPUT;
        IF OBJECT_ID('bancos.ConciliacionInternaPares') IS NOT NULL EXEC sys.sp_executesql N'SELECT @v=(SELECT * FROM bancos.ConciliacionInternaPares WITH(HOLDLOCK) ORDER BY Id FOR JSON PATH, INCLUDE_NULL_VALUES)',N'@v nvarchar(max) OUTPUT',@v=@e OUTPUT;
        SELECT @a+'|'+@b+'|'+@c+'|'+@d+'|'+@e", transaction: tx);
    public static void Convertir(SqlConnection cn, SqlTransaction tx, string tabla, bool intermedia, IReadOnlyDictionary<int, DateTime> fechas)
    {
        bool m = EsMovimiento(tabla);
        cn.Execute(intermedia
            ? (m ? "ALTER TABLE bancos.MovimientosArchivo ALTER COLUMN fecha2 date NULL" : "ALTER TABLE bancos.ConciliacionItemsExternos ALTER COLUMN fecha2 date NULL")
            : (m ? "ALTER TABLE bancos.MovimientosArchivo ADD fecha2 date NULL" : "ALTER TABLE bancos.ConciliacionItemsExternos ADD fecha2 date NULL"), transaction: tx);
        cn.Execute("CREATE TABLE #Fechas(Id int PRIMARY KEY, Fecha date NOT NULL)", transaction: tx);
        var datos = new DataTable(); datos.Columns.Add("Id", typeof(int)); datos.Columns.Add("Fecha", typeof(DateTime));
        foreach (var f in fechas) datos.Rows.Add(f.Key, f.Value);
        using (var bulk = new SqlBulkCopy(cn, SqlBulkCopyOptions.Default, tx) { DestinationTableName = "#Fechas" }) { bulk.ColumnMappings.Add("Id", "Id"); bulk.ColumnMappings.Add("Fecha", "Fecha"); bulk.WriteToServer(datos); }
        cn.Execute(m ? "UPDATE m SET fecha2=f.Fecha FROM bancos.MovimientosArchivo m JOIN #Fechas f ON f.Id=m.Id WHERE m.fecha2 IS NULL" : "UPDATE m SET fecha2=f.Fecha FROM bancos.ConciliacionItemsExternos m JOIN #Fechas f ON f.Id=m.Id WHERE m.fecha2 IS NULL", transaction: tx);
        int diferencias=cn.ExecuteScalar<int>(m ? "SELECT COUNT(*) FROM bancos.MovimientosArchivo m FULL JOIN #Fechas f ON f.Id=m.Id WHERE m.Id IS NULL OR f.Id IS NULL OR m.fecha2 IS NULL OR m.fecha2<>f.Fecha" : "SELECT COUNT(*) FROM bancos.ConciliacionItemsExternos m FULL JOIN #Fechas f ON f.Id=m.Id WHERE m.Id IS NULL OR f.Id IS NULL OR m.fecha2 IS NULL OR m.fecha2<>f.Fecha", transaction: tx);
        if (diferencias != 0) throw new InvalidOperationException("La columna intermedia difiere de la conversión validada.");
        cn.Execute("DROP TABLE #Fechas", transaction: tx);
        cn.Execute("EXEC sys.sp_rename @Objeto, @Nombre, 'COLUMN'", new { Objeto=tabla+".Fecha", Nombre="FechaTextoOriginal" }, tx);
        // El histórico admite nulos para movimientos nuevos y conserva la amplitud del texto original.
        cn.Execute(m ? "ALTER TABLE bancos.MovimientosArchivo ALTER COLUMN FechaTextoOriginal nvarchar(max) NULL" : "ALTER TABLE bancos.ConciliacionItemsExternos ALTER COLUMN FechaTextoOriginal nvarchar(max) NULL", transaction: tx);
        cn.Execute("EXEC sys.sp_rename @Objeto, @Nombre, 'COLUMN'", new { Objeto=tabla+".fecha2", Nombre="Fecha" }, tx);
        cn.Execute(m ? "ALTER TABLE bancos.MovimientosArchivo ALTER COLUMN Fecha date NOT NULL" : "ALTER TABLE bancos.ConciliacionItemsExternos ALTER COLUMN Fecha date NOT NULL", transaction: tx);
    }
}
