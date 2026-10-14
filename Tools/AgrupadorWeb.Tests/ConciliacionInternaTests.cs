// Fecha: 14/10/2026 - TAREA: 00097 - Linea: 5 - La conciliación interna que usa la web: auto-conciliar entre extractos distintos con desempate, manual, desconciliar, finalizar (cuenta del banco contrario) y Excel con importes numéricos en absoluto. Usa una base SQL temporal; si no hay SQL disponible se omite.
using AgrupadorConceptos.Data;
using AgrupadorConceptos.Models;
using AgrupadorConceptos.Services;
using ClosedXML.Excel;
using Conciliador.Comun;
using Dapper;
using Microsoft.Data.SqlClient;
static class ConciliacionInternaTests
{
    static void Check(bool ok, string mensaje = "Condición falsa") { if (!ok) throw new Exception(mensaje); }
    static MemoryStream Extracto(params (DateTime Fecha, string Concepto, decimal Monto)[] filas)
    {
        using var libro = new XLWorkbook(); var h = libro.AddWorksheet("Hoja1");
        h.Cell(1, 1).Value = "Fecha"; h.Cell(1, 2).Value = "Concepto"; h.Cell(1, 3).Value = "Monto";
        for (int i = 0; i < filas.Length; i++) { h.Cell(i + 2, 1).Value = filas[i].Fecha; h.Cell(i + 2, 2).Value = filas[i].Concepto; h.Cell(i + 2, 3).Value = filas[i].Monto; }
        var ms = new MemoryStream(); libro.SaveAs(ms); ms.Position = 0; return ms;
    }
    public static void Run(Action<string, Action> test)
    {
        string maestra = Environment.GetEnvironmentVariable("AGRUPADOR_TEST_SQL") ?? @"Server=(localdb)\MSSQLLocalDB;Database=master;Integrated Security=true;TrustServerCertificate=true;Connect Timeout=20";
        string nombre = "AgrWebConc_" + Guid.NewGuid().ToString("N");
        SqlConnection? admin = null;
        try { admin = new SqlConnection(maestra); admin.Open(); admin.Execute("DECLARE @s nvarchar(max)='CREATE DATABASE '+QUOTENAME(@N); EXEC(@s)", new { N = nombre }); }
        catch (Exception ex) { Console.WriteLine("OMITIDAS: conciliación interna (sin SQL de prueba: " + ex.GetType().Name + "). Definí AGRUPADOR_TEST_SQL."); admin?.Dispose(); return; }
        try
        {
            SqlDb.ConnectionString = new SqlConnectionStringBuilder(maestra) { InitialCatalog = nombre }.ConnectionString;
            // El esquema se crea una vez por proceso: como otra suite ya inicializó su base, se reinicia el indicador para crearlo en ésta.
            typeof(DatabaseHelper).GetField("_inicializada", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!.SetValue(null, false);
            DatabaseHelper.InitializeDatabase();
            using var cn = SqlDb.GetConnection(); cn.Open();
            int cuentaA = cn.QuerySingle<int>("INSERT bancos.CuentasContables(Cuenta,Descripcion) OUTPUT INSERTED.Id VALUES('1110','Banco A')");
            int cuentaB = cn.QuerySingle<int>("INSERT bancos.CuentasContables(Cuenta,Descripcion) OUTPUT INSERTED.Id VALUES('1120','Banco B')");
            int concepto = cn.QuerySingle<int>("INSERT bancos.ConceptosEstandar(Nombre) OUTPUT INSERTED.Id VALUES('TRANSFERENCIAS')");
            int Perfil(string banco, int cuenta)
            {
                int id = cn.QuerySingle<int>(@"INSERT bancos.PerfilesBanco(NombreBanco,ColumnaConcepto,ColumnaDescripcion,EsCodigo,FilaEncabezado,TipoImporte,ColumnaImporteUnico,ColumnaFecha,IdCuentaContable)
                    OUTPUT INSERTED.Id VALUES(@B,'Concepto','Concepto',0,1,1,'Monto','Fecha',@C)", new { B = banco, C = cuenta });
                cn.Execute("INSERT bancos.HomologacionConceptos(IdPerfilBanco,ValorOriginal,IdConceptoEstandar) VALUES(@P,'TRANSF',@C)", new { P = id, C = concepto });
                return id;
            }
            int perfilA = Perfil("Banco A", cuentaA), perfilB = Perfil("Banco B", cuentaB);
            PerfilBanco Leer(int id) => cn.QuerySingle<PerfilBanco>("SELECT * FROM bancos.PerfilesBanco WHERE Id=@Id", new { Id = id });
            var d1 = new DateTime(2026, 9, 1); var d2 = new DateTime(2026, 9, 2); var d3 = new DateTime(2026, 9, 3); var d5 = new DateTime(2026, 9, 5);
            var movA = ImportacionService.ImportarArchivo(Extracto((d1, "TRANSF 1", -1000m), (d2, "TRANSF 2", -2000m), (d3, "TRANSF 3", -500m), (d1, "TRANSF 4", -300m), (d1, "TRANSF 5", 300m)), "a.xlsx", Leer(perfilA));
            var movB = ImportacionService.ImportarArchivo(Extracto((d1, "TRANSF 1", 1000m), (d5, "TRANSF 2", 2000m), (d3, "TRANSF 3", 500m), (d3, "TRANSF 4", 500m)), "b.xlsx", Leer(perfilB));
            var sesion = ConciliacionInternaService.CrearSesion("Prueba", d1, new DateTime(2026, 9, 30), new[] { "TRANSFERENCIAS" });

            test("Conceptos disponibles del rango y pendientes de cada lado, sin importar el extracto", () =>
            {
                Check(ConciliacionInternaService.ObtenerConceptosDisponibles(d1, d5).SequenceEqual(new[] { "TRANSFERENCIAS" }), "Conceptos");
                Check(ConciliacionInternaService.ObtenerConceptosDisponibles(new DateTime(2025, 1, 1), new DateTime(2025, 1, 31)).Count == 0, "Rango vacío");
                Check(ConciliacionInternaService.ObtenerPendientesDebitos(sesion.Id).Count == 4 && ConciliacionInternaService.ObtenerPendientesCreditos(sesion.Id).Count == 5, "Pendientes");
            });
            (int Conciliados, List<(MovimientoProcesado A, List<MovimientoProcesado> Candidatos)> Duplicados) auto = default;
            test("Auto-conciliar: fecha+importe, sólo importe, desempate y nunca contra el mismo extracto", () =>
            {
                auto = ConciliacionInternaService.AutoConciliar(sesion.Id);
                Check(auto.Conciliados == 2, "Pares automáticos: " + auto.Conciliados);
                Check(auto.Duplicados.Count == 1 && auto.Duplicados[0].Candidatos.Count == 2 && Math.Abs(auto.Duplicados[0].A.Debitos) == 500m, "Un débito con dos candidatos");
                var pares = ConciliacionInternaService.ObtenerPares(sesion.Id);
                Check(pares.Any(p => p.TipoMatch == "FechaImporte") && pares.Any(p => p.TipoMatch == "SoloImporte"), "Tipos de match");
                Check(pares.All(p => p.IdArchivoA != p.IdArchivoB), "Mismo extracto emparejado");
                Check(ConciliacionInternaService.ObtenerPendientesDebitos(sesion.Id).Any(m => Math.Abs(m.Debitos) == 300m), "El -300 y el +300 del mismo extracto quedan pendientes");
            });
            test("Desempate manual, un movimiento no entra en dos pares y desconciliar lo libera", () =>
            {
                var origen = auto.Duplicados[0].A; var elegido = auto.Duplicados[0].Candidatos[0];
                Check(ConciliacionInternaService.ConciliarPar(sesion.Id, origen.Id, elegido.Id, TipoMatch.SeleccionManual) == null, "Concilió");
                Check(ConciliacionInternaService.ConciliarPar(sesion.Id, origen.Id, auto.Duplicados[0].Candidatos[1].Id, TipoMatch.Manual) != null, "Aceptó el débito dos veces");
                var par = ConciliacionInternaService.ObtenerPares(sesion.Id).Single(p => p.IdMovimientoA == origen.Id);
                ConciliacionInternaService.DesconciliarPar(par.Id);
                Check(ConciliacionInternaService.ObtenerPendientesDebitos(sesion.Id).Any(m => m.Id == origen.Id), "No volvió a pendientes");
                Check(ConciliacionInternaService.ConciliarPar(sesion.Id, origen.Id, elegido.Id, TipoMatch.Manual) == null, "Reconciliar a mano");
            });
            test("Finalizar exige cuenta en los dos perfiles y deja la cuenta del banco contrario", () =>
            {
                cn.Execute("UPDATE bancos.PerfilesBanco SET IdCuentaContable=NULL WHERE Id=@P", new { P = perfilB });
                var falla = ConciliacionInternaService.Finalizar(sesion.Id);
                Check(!falla.Exito && falla.ParesSinCuenta.Count > 0, "Debía fallar sin cuenta");
                Check(cn.QuerySingle<string>("SELECT Estado FROM bancos.ConciliacionInternaSesiones WHERE Id=@I", new { I = sesion.Id }) == "EnProceso", "La sesión no debía cerrarse");
                cn.Execute("UPDATE bancos.PerfilesBanco SET IdCuentaContable=@C WHERE Id=@P", new { C = cuentaB, P = perfilB });
                Check(ConciliacionInternaService.Finalizar(sesion.Id).Exito, "No finalizó");
                var par = ConciliacionInternaService.ObtenerPares(sesion.Id)[0];
                Check(cn.QuerySingle<string>("SELECT CuentaFinal FROM bancos.MovimientosArchivo WHERE Id=@I", new { I = par.IdMovimientoA }) == "1120", "El débito debe llevar la cuenta del banco del crédito");
                Check(cn.QuerySingle<string>("SELECT CuentaFinal FROM bancos.MovimientosArchivo WHERE Id=@I", new { I = par.IdMovimientoB }) == "1110", "El crédito debe llevar la cuenta del banco del débito");
                Check(ConciliacionInternaService.ConciliarPar(sesion.Id, movA[3].Id, movB[3].Id, TipoMatch.Manual) != null, "Una sesión finalizada no admite pares");
            });
            test("Excel: importes como números en valor absoluto, fechas tipadas y dos hojas", () =>
            {
                var bytes = ConciliacionInternaExcelExporter.ExportarBytes(ConciliacionInternaService.ObtenerPares(sesion.Id), ConciliacionInternaService.ObtenerPendientesDebitos(sesion.Id), ConciliacionInternaService.ObtenerPendientesCreditos(sesion.Id));
                using var libro = new XLWorkbook(new MemoryStream(bytes));
                var con = libro.Worksheet("Conciliados"); var pend = libro.Worksheet("Pendientes");
                Check(con.Cell(2, 2).DataType == XLDataType.Number && con.Cell(2, 2).GetDouble() > 0, "Importe débito numérico y positivo");
                Check(con.Cell(2, 1).DataType == XLDataType.DateTime && con.Column(2).Style.NumberFormat.Format == "#,##0.00", "Fecha tipada y formato");
                Check(pend.Cell(2, 3).DataType == XLDataType.Number && pend.Cell(2, 3).GetDouble() > 0, "Importe pendiente numérico y positivo");
            });
            test("Eliminar la sesión borra sus pares y no toca los movimientos", () =>
            {
                int movs = cn.QuerySingle<int>("SELECT COUNT(*) FROM bancos.MovimientosArchivo");
                ConciliacionInternaService.EliminarSesion(sesion.Id);
                Check(cn.QuerySingle<int>("SELECT COUNT(*) FROM bancos.ConciliacionInternaPares") == 0 && cn.QuerySingle<int>("SELECT COUNT(*) FROM bancos.MovimientosArchivo") == movs, "Pares o movimientos");
            });
        }
        finally
        {
            try { SqlConnection.ClearAllPools(); admin!.Execute("DECLARE @s nvarchar(max)='ALTER DATABASE '+QUOTENAME(@N)+' SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE '+QUOTENAME(@N); EXEC(@s)", new { N = nombre }); } catch { }
            admin?.Dispose();
        }
    }
}
