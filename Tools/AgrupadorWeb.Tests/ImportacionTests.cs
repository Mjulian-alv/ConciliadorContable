// Fecha: 14/10/2026 - TAREA: 00097 - Linea: 2 - La importación desde un flujo (el archivo que llega del navegador) hace lo mismo que por ruta: reparte los importes, homologa y rechaza el archivo completo ante una fecha inválida. Usa una base SQL temporal; si no hay SQL disponible se omite.
using AgrupadorConceptos.Data;
using AgrupadorConceptos.Models;
using AgrupadorConceptos.Services;
using ClosedXML.Excel;
using Conciliador.Comun;
using Dapper;
using Microsoft.Data.SqlClient;
static class ImportacionTests
{
    static void Check(bool ok, string mensaje = "Condición falsa") { if (!ok) throw new Exception(mensaje); }
    // Dos filas de título, encabezado en la 3 (como los extractos reales, que traen cabecera antes de los datos).
    static MemoryStream Extracto(params (object Fecha, string Concepto, decimal? Debe, decimal? Haber, decimal? Monto)[] filas)
    {
        using var libro = new XLWorkbook(); var h = libro.AddWorksheet("Hoja1");
        h.Cell(1, 1).Value = "Banco de prueba"; h.Cell(2, 1).Value = "Cuenta 123";
        string[] cab = { "Fecha", "Concepto", "Debe", "Haber", "Monto" };
        for (int c = 0; c < cab.Length; c++) h.Cell(3, c + 1).Value = cab[c];
        for (int i = 0; i < filas.Length; i++)
        {
            int r = 4 + i; var f = filas[i];
            if (f.Fecha is DateTime d) h.Cell(r, 1).Value = d; else h.Cell(r, 1).Value = f.Fecha?.ToString() ?? "";
            h.Cell(r, 2).Value = f.Concepto;
            if (f.Debe.HasValue) h.Cell(r, 3).Value = f.Debe.Value; if (f.Haber.HasValue) h.Cell(r, 4).Value = f.Haber.Value; if (f.Monto.HasValue) h.Cell(r, 5).Value = f.Monto.Value;
        }
        var ms = new MemoryStream(); libro.SaveAs(ms); ms.Position = 0; return ms;
    }
    public static void Run(Action<string, Action> test)
    {
        string maestra = Environment.GetEnvironmentVariable("AGRUPADOR_TEST_SQL") ?? @"Server=(localdb)\MSSQLLocalDB;Database=master;Integrated Security=true;TrustServerCertificate=true;Connect Timeout=20";
        string nombre = "AgrWebTest_" + Guid.NewGuid().ToString("N");
        SqlConnection? admin = null;
        try { admin = new SqlConnection(maestra); admin.Open(); admin.Execute("DECLARE @s nvarchar(max)='CREATE DATABASE '+QUOTENAME(@N); EXEC(@s)", new { N = nombre }); }
        catch (Exception ex) { Console.WriteLine("OMITIDAS: importación desde flujo (sin SQL de prueba: " + ex.GetType().Name + "). Definí AGRUPADOR_TEST_SQL."); admin?.Dispose(); return; }
        try
        {
            SqlDb.ConnectionString = new SqlConnectionStringBuilder(maestra) { InitialCatalog = nombre }.ConnectionString;
            DatabaseHelper.InitializeDatabase();
            using var cn = SqlDb.GetConnection(); cn.Open();
            int Perfil(string banco, int tipo) => cn.QuerySingle<int>(@"INSERT bancos.PerfilesBanco(NombreBanco,ColumnaConcepto,ColumnaDescripcion,EsCodigo,FilaEncabezado,TipoImporte,ColumnaImporteUnico,ColumnaDebe,ColumnaHaber,ColumnaFecha)
                OUTPUT INSERTED.Id VALUES(@B,'Concepto','Concepto',0,3,@T,@U,@D,@H,'Fecha')", new { B = banco, T = tipo, U = tipo == 1 ? "Monto" : null, D = tipo == 2 ? "Debe" : null, H = tipo == 2 ? "Haber" : null });
            PerfilBanco Leer(int id) => cn.QuerySingle<PerfilBanco>("SELECT * FROM bancos.PerfilesBanco WHERE Id=@Id", new { Id = id });
            int unico = Perfil("Banco único", 1), debeHaber = Perfil("Banco debe/haber", 2);
            int concepto = cn.QuerySingle<int>("INSERT bancos.ConceptosEstandar(Nombre) OUTPUT INSERTED.Id VALUES('TRANSFERENCIAS RECIBIDAS')");
            cn.Execute("INSERT bancos.HomologacionConceptos(IdPerfilBanco,ValorOriginal,IdConceptoEstandar) VALUES(@P,'TRANSF',@C)", new { P = unico, C = concepto });

            test("Importación por flujo: el importe único negativo va a débito y el positivo a crédito, en absoluto", () =>
            {
                var movs = ImportacionService.ImportarArchivo(Extracto((new DateTime(2026, 9, 1), "TRANSF RECIBIDA", null, null, 58049.41m), (new DateTime(2026, 9, 2), "GRAVAMEN LEY 25413", null, null, -3412.18m),
                    (new DateTime(2026, 9, 3), "SIN REGLA", null, null, 100m)), "extracto-unico.xlsx", Leer(unico));
                Check(movs.Count == 3, "Cantidad");
                Check(movs[0].Creditos == 58049.41m && movs[0].Debitos == 0, "Crédito");
                Check(movs[1].Debitos == 3412.18m && movs[1].Creditos == 0, "Débito en absoluto");
                Check(movs[0].ConceptoEstandar == "TRANSFERENCIAS RECIBIDAS" && movs[2].ConceptoEstandar == ConceptosBancarios.PendienteHomologar, "Homologación por regla y pendiente");
                Check(movs.All(m => m.IdArchivo > 0 && m.Id > 0), "Ids asignados");
                Check(cn.QuerySingle<string>("SELECT NombreArchivo FROM bancos.ArchivosImportados WHERE Id=@I", new { I = movs[0].IdArchivo }) == "extracto-unico.xlsx", "Nombre guardado en la cabecera");
                Check(cn.QuerySingle<DateTime>("SELECT MIN(Fecha) FROM bancos.MovimientosArchivo WHERE IdArchivo=@I", new { I = movs[0].IdArchivo }) == new DateTime(2026, 9, 1), "Fecha tipada");
            });
            test("Importación por flujo: Debe/Haber según las columnas del perfil", () =>
            {
                var movs = ImportacionService.ImportarArchivo(Extracto((new DateTime(2026, 9, 5), "PAGO PROVEEDOR", 1500m, null, null), (new DateTime(2026, 9, 6), "COBRANZA", null, 2300.50m, null)), "debe-haber.xlsx", Leer(debeHaber));
                Check(movs.Count == 2 && Math.Abs(movs[0].Debitos) == 1500m && movs[0].Creditos == 0 && movs[1].Creditos == 2300.50m, "Reparto Debe/Haber");
            });
            test("Importación por flujo: una fecha inválida rechaza el archivo completo y no deja nada guardado", () =>
            {
                int antes = cn.QuerySingle<int>("SELECT COUNT(*) FROM bancos.ArchivosImportados"), movAntes = cn.QuerySingle<int>("SELECT COUNT(*) FROM bancos.MovimientosArchivo");
                try
                {
                    ImportacionService.ImportarArchivo(Extracto((new DateTime(2026, 9, 1), "OK", null, null, 10m), ("31/02/2026", "FECHA INEXISTENTE", null, null, 20m), ("", "SIN FECHA", null, null, 30m)), "malo.xlsx", Leer(unico));
                    throw new Exception("Aceptó un archivo con fechas inválidas");
                }
                catch (FechasImportacionException ex)
                {
                    Check(ex.Incidencias.Count == 2, "Incidencias: " + ex.Incidencias.Count);
                    Check(ex.Incidencias.Select(i => i.Fila).SequenceEqual(new[] { 5, 6 }), "Filas del Excel: " + string.Join(",", ex.Incidencias.Select(i => i.Fila)));
                    Check(ex.Incidencias.All(i => i.Archivo == "malo.xlsx"), "Nombre del archivo en las incidencias");
                }
                Check(cn.QuerySingle<int>("SELECT COUNT(*) FROM bancos.ArchivosImportados") == antes && cn.QuerySingle<int>("SELECT COUNT(*) FROM bancos.MovimientosArchivo") == movAntes, "Quedó una cabecera o filas a medias");
            });
            test("Perfil: encabezados de la fila elegida desde un flujo, y fila inexistente", () =>
            {
                var h = ImportacionService.LeerEncabezados(Extracto(), ".xlsx", 3);
                Check(h.SequenceEqual(new[] { "Fecha", "Concepto", "Debe", "Haber", "Monto" }), "Encabezados: " + string.Join("|", h));
                try { ImportacionService.LeerEncabezados(Extracto(), ".xlsx", 50); throw new Exception("Aceptó una fila inexistente"); } catch (InvalidOperationException) { }
            });
        }
        finally
        {
            try { SqlConnection.ClearAllPools(); admin!.Execute("DECLARE @s nvarchar(max)='ALTER DATABASE '+QUOTENAME(@N)+' SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE '+QUOTENAME(@N); EXEC(@s)", new { N = nombre }); } catch { }
            admin?.Dispose();
        }
    }
}
