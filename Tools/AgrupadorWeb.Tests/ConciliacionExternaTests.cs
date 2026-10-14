// Fecha: 14/10/2026 - TAREA: 00097 - Linea: 4 - La conciliación externa que usa la web: lectura del archivo externo desde un flujo (Excel y CSV, con fechas inválidas), auto-conciliar, conciliar un registro con varios movimientos de suma exacta, desconciliar el grupo, finalizar y Excel con importes en absoluto. Usa una base SQL temporal; si no hay SQL disponible se omite.
using System.Text;
using AgrupadorConceptos.Data;
using AgrupadorConceptos.Models;
using AgrupadorConceptos.Services;
using ClosedXML.Excel;
using Conciliador.Comun;
using Dapper;
using Microsoft.Data.SqlClient;
static class ConciliacionExternaTests
{
    static void Check(bool ok, string mensaje = "Condición falsa") { if (!ok) throw new Exception(mensaje); }
    static MemoryStream Extracto(params (DateTime Fecha, string Concepto, decimal Monto)[] filas)
    {
        using var libro = new XLWorkbook(); var h = libro.AddWorksheet("Hoja1");
        h.Cell(1, 1).Value = "Fecha"; h.Cell(1, 2).Value = "Concepto"; h.Cell(1, 3).Value = "Monto";
        for (int i = 0; i < filas.Length; i++) { h.Cell(i + 2, 1).Value = filas[i].Fecha; h.Cell(i + 2, 2).Value = filas[i].Concepto; h.Cell(i + 2, 3).Value = filas[i].Monto; }
        var ms = new MemoryStream(); libro.SaveAs(ms); ms.Position = 0; return ms;
    }
    static MemoryStream ExternoExcel(params (object Fecha, decimal Importe, string Concepto)[] filas)
    {
        using var libro = new XLWorkbook(); var h = libro.AddWorksheet("Hoja1");
        h.Cell(1, 1).Value = "Fecha"; h.Cell(1, 2).Value = "Importe"; h.Cell(1, 3).Value = "Concepto";
        for (int i = 0; i < filas.Length; i++)
        {
            if (filas[i].Fecha is DateTime d) h.Cell(i + 2, 1).Value = d; else h.Cell(i + 2, 1).Value = filas[i].Fecha?.ToString() ?? "";
            h.Cell(i + 2, 2).Value = filas[i].Importe; h.Cell(i + 2, 3).Value = filas[i].Concepto;
        }
        var ms = new MemoryStream(); libro.SaveAs(ms); ms.Position = 0; return ms;
    }
    public static void Run(Action<string, Action> test)
    {
        // El lector del archivo externo no necesita base: se prueba siempre.
        test("Archivo externo desde un flujo: Excel con importes en absoluto y CSV con coma argentina", () =>
        {
            var d1 = new DateTime(2026, 9, 1);
            var x = ArchivoExternoReader.Leer(ExternoExcel((d1, -1500.5m, "Pago A"), (new DateTime(2026, 9, 2), 20m, "Cobro B")), ".xlsx", "erp.xlsx");
            Check(x.Count == 2 && x[0].Importe == 1500.5m && x[0].Fecha == d1 && x[1].Detalle == "Cobro B", "Excel");
            var csv = new MemoryStream(Encoding.UTF8.GetBytes("Fecha;Importe;Concepto\n01/09/2026;1.234,50;Pago C\n02/09/2026;-10,25;Pago D\n"));
            var y = ArchivoExternoReader.Leer(csv, ".csv", "erp.csv");
            Check(y.Count == 2 && y[0].Importe == 1234.50m && y[1].Importe == 10.25m && y[1].Fecha == new DateTime(2026, 9, 2), "CSV");
        });
        test("Archivo externo desde un flujo: una fecha inválida rechaza todo y nombra el archivo y la fila", () =>
        {
            try { ArchivoExternoReader.Leer(ExternoExcel((new DateTime(2026, 9, 1), 10m, "OK"), ("31/02/2026", 20m, "MAL")), ".xlsx", "erp-malo.xlsx"); throw new Exception("Aceptó una fecha inválida"); }
            catch (FechasImportacionException ex) { Check(ex.Incidencias.Count == 1 && ex.Incidencias[0].Archivo == "erp-malo.xlsx" && ex.Incidencias[0].Fila == 3, "Incidencia"); }
            try { ArchivoExternoReader.Leer(new MemoryStream(Encoding.UTF8.GetBytes("A;B\n1;2\n")), ".csv", "x.csv"); throw new Exception("Aceptó un CSV sin columnas"); } catch (InvalidOperationException) { }
        });

        string maestra = Environment.GetEnvironmentVariable("AGRUPADOR_TEST_SQL") ?? @"Server=(localdb)\MSSQLLocalDB;Database=master;Integrated Security=true;TrustServerCertificate=true;Connect Timeout=20";
        string nombre = "AgrWebExt_" + Guid.NewGuid().ToString("N");
        SqlConnection? admin = null;
        try { admin = new SqlConnection(maestra); admin.Open(); admin.Execute("DECLARE @s nvarchar(max)='CREATE DATABASE '+QUOTENAME(@N); EXEC(@s)", new { N = nombre }); }
        catch (Exception ex) { Console.WriteLine("OMITIDAS: conciliación externa con base (sin SQL de prueba: " + ex.GetType().Name + "). Definí AGRUPADOR_TEST_SQL."); admin?.Dispose(); return; }
        try
        {
            SqlDb.ConnectionString = new SqlConnectionStringBuilder(maestra) { InitialCatalog = nombre }.ConnectionString;
            // El esquema se crea una vez por proceso: como otra suite ya inicializó su base, se reinicia el indicador para crearlo en ésta.
            typeof(DatabaseHelper).GetField("_inicializada", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!.SetValue(null, false);
            DatabaseHelper.InitializeDatabase();
            using var cn = SqlDb.GetConnection(); cn.Open();
            int concepto = cn.QuerySingle<int>("INSERT bancos.ConceptosEstandar(Nombre) OUTPUT INSERTED.Id VALUES('COBRANZAS')");
            int perfil = cn.QuerySingle<int>(@"INSERT bancos.PerfilesBanco(NombreBanco,ColumnaConcepto,ColumnaDescripcion,EsCodigo,FilaEncabezado,TipoImporte,ColumnaImporteUnico,ColumnaFecha)
                OUTPUT INSERTED.Id VALUES('Banco X','Concepto','Concepto',0,1,1,'Monto','Fecha')");
            cn.Execute("INSERT bancos.HomologacionConceptos(IdPerfilBanco,ValorOriginal,IdConceptoEstandar) VALUES(@P,'COBRO',@C)", new { P = perfil, C = concepto });
            var p = cn.QuerySingle<PerfilBanco>("SELECT * FROM bancos.PerfilesBanco WHERE Id=@Id", new { Id = perfil });
            var d1 = new DateTime(2026, 9, 1); var d2 = new DateTime(2026, 9, 2); var d3 = new DateTime(2026, 9, 3);
            var movs = ImportacionService.ImportarArchivo(Extracto((d1, "COBRO 1", 1000m), (d2, "COBRO 2", 600m), (d2, "COBRO 3", 400m), (d3, "COBRO 4", -250m), (d3, "COBRO 5", 77m)), "x.xlsx", p);
            int idArchivo = movs[0].IdArchivo;
            var externos = new List<ConciliacionItemExterno>
            {
                new() { Fecha = d1, Importe = 1000m, Detalle = "Cobranza 1" },   // fecha+importe
                new() { Fecha = d3, Importe = 250m, Detalle = "Pago débito" },   // débito del extracto, sólo por importe
                new() { Fecha = d2, Importe = 1000m, Detalle = "Cobranza 2+3" }, // suma de dos movimientos
                new() { Fecha = d3, Importe = 5m, Detalle = "Sin match" },
            };
            var sesion = ConciliacionExternService.CrearSesion("Prueba externa", new List<int> { idArchivo }, new[] { "COBRANZAS" }, externos);

            test("Sesión: guarda los registros externos y los movimientos pendientes con el concepto elegido", () =>
            {
                Check(ConciliacionExternService.ObtenerTodosItemsExternos(sesion.Id).Count == 4 && ConciliacionExternService.ObtenerItemsPendientes(sesion.Id).Count == 4, "Items");
                Check(ConciliacionExternService.ObtenerMovimientosPendientes(sesion.Id).Count == 5 && ConciliacionExternService.ObtenerMovimientosSinConcepto(sesion.Id).Count == 5, "Movimientos");
                Check(SelectorExtractosService.Obtener(d1, d3, false).Any(c => c.IdArchivo == idArchivo && c.Cantidad == 5), "El extracto aparece en el rango");
                Check(!SelectorExtractosService.Obtener(new DateTime(2025, 1, 1), new DateTime(2025, 1, 31), false).Any(), "Fuera de rango no aparece");
            });
            test("Auto-conciliar: fecha+importe, también contra un débito del extracto en valor absoluto", () =>
            {
                var (conciliados, duplicados) = ConciliacionExternService.AutoConciliar(sesion.Id);
                Check(conciliados == 2 && duplicados.Count == 0, "Conciliados: " + conciliados);
                var pares = ConciliacionExternService.ObtenerPares(sesion.Id);
                Check(pares.Count == 2 && pares.All(x => x.TipoMatch == "FechaImporte"), "Tipos");
                Check(pares.Any(x => x.DetalleExterno == "Pago débito" && x.ImporteExtracto != 0), "El débito del extracto se concilió");
            });
            var pendiente = () => ConciliacionExternService.ObtenerItemsPendientes(sesion.Id);
            test("Conciliar selección: la suma debe ser exacta y el grupo queda atómico", () =>
            {
                Check(pendiente().Select(x => x.Detalle).OrderBy(x => x).SequenceEqual(new[] { "Cobranza 2+3", "Sin match" }), "Quedan dos registros pendientes");
                var libres = ConciliacionExternService.ObtenerMovimientosSinConcepto(sesion.Id);
                Check(libres.Count == 3, "Tres movimientos libres: " + libres.Count);
                var ext = pendiente().Single(x => x.Detalle == "Cobranza 2+3");
                var m600 = libres.Single(m => m.Creditos == 600m); var m400 = libres.Single(m => m.Creditos == 400m); var m77 = libres.Single(m => m.Creditos == 77m);
                try { ConciliacionExternService.ConciliarSeleccion(sesion.Id, ext.Id, new[] { m600.Id, m77.Id }, TipoMatch.Manual); throw new Exception("Aceptó una suma que no coincide"); }
                catch (InvalidOperationException) { }
                Check(ConciliacionExternService.ObtenerPares(sesion.Id).Count == 2, "Una suma inexacta no debe dejar pares");
                ConciliacionExternService.ConciliarSeleccion(sesion.Id, ext.Id, new[] { m600.Id, m400.Id }, TipoMatch.Manual);
                var grupo = ConciliacionExternService.ObtenerPares(sesion.Id).Where(x => x.DetalleExterno == "Cobranza 2+3").ToList();
                Check(grupo.Count == 2 && grupo.Sum(g => g.ImporteExtracto) == 1000m, "El grupo 1:N debe sumar el importe externo");
            });
            test("Desconciliar un par del grupo deshace el grupo entero y el registro vuelve a pendiente", () =>
            {
                var par = ConciliacionExternService.ObtenerPares(sesion.Id).First(x => x.DetalleExterno == "Cobranza 2+3");
                ConciliacionExternService.DesconciliarPar(par.Id);
                Check(!ConciliacionExternService.ObtenerPares(sesion.Id).Any(x => x.DetalleExterno == "Cobranza 2+3"), "Quedó un par suelto del grupo");
                Check(pendiente().Any(x => x.Detalle == "Cobranza 2+3"), "El registro no volvió a pendiente");
            });
            test("Excel: importes numéricos en valor absoluto, un externo por grupo, fechas tipadas", () =>
            {
                var bytes = ConciliacionExcelExporter.ExportarBytes(ConciliacionExternService.ObtenerPares(sesion.Id), ConciliacionExternService.ObtenerItemsPendientes(sesion.Id), ConciliacionExternService.ObtenerMovimientosPendientes(sesion.Id));
                using var libro = new XLWorkbook(new MemoryStream(bytes));
                var con = libro.Worksheet("Conciliados"); var pend = libro.Worksheet("Pendientes");
                Check(con.LastRowUsed()!.RowNumber() >= 2 && con.Cell(2, 5).DataType == XLDataType.Number && con.Column(5).Cells().Skip(1).All(c => c.IsEmpty() || c.GetDouble() >= 0), "Importes del extracto positivos");
                Check(con.Cell(2, 1).DataType == XLDataType.DateTime && pend.Column(3).Cells().Skip(1).All(c => c.IsEmpty() || c.GetDouble() >= 0), "Fechas tipadas y pendientes positivos");
            });
            test("Finalizar deja la sesión de sólo lectura; eliminar la borra sin tocar el extracto", () =>
            {
                ConciliacionExternService.MarcarFinalizada(sesion.Id);
                var ext = pendiente()[0];
                try { ConciliacionExternService.ConciliarSeleccion(sesion.Id, ext.Id, new[] { movs[4].Id }, TipoMatch.Manual); throw new Exception("Aceptó un par en una sesión finalizada"); } catch (InvalidOperationException) { }
                int movsTotal = cn.QuerySingle<int>("SELECT COUNT(*) FROM bancos.MovimientosArchivo");
                ConciliacionExternService.EliminarSesion(sesion.Id);
                Check(cn.QuerySingle<int>("SELECT COUNT(*) FROM bancos.ConciliacionSesiones") == 0 && cn.QuerySingle<int>("SELECT COUNT(*) FROM bancos.MovimientosArchivo") == movsTotal, "Sesión o movimientos");
            });
        }
        finally
        {
            try { SqlConnection.ClearAllPools(); admin!.Execute("DECLARE @s nvarchar(max)='ALTER DATABASE '+QUOTENAME(@N)+' SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE '+QUOTENAME(@N); EXEC(@s)", new { N = nombre }); } catch { }
            admin?.Dispose();
        }
    }
}
