// Fecha: 26/10/2026 - TAREA: 00135 - Linea: 1, 2, 3 - Concepto final protegido, historial de movimientos, blindaje del archivo, clave maestra y baja con archivo inactivo. Usa una base SQL temporal; si no hay SQL disponible se omite.
using System.IO.Compression;
using System.Text.Json;
using AgrupadorConceptos.Data;
using AgrupadorConceptos.Models;
using AgrupadorConceptos.Services;
using ClosedXML.Excel;
using Conciliador.Comun;
using Dapper;
using Microsoft.Data.SqlClient;

static class BlindajeTests
{
    const string Clave = "clave-maestra-1";

    static void Check(bool ok, string mensaje = "Condición falsa") { if (!ok) throw new Exception(mensaje); }

    static T Rechaza<T>(Action accion, string que) where T : Exception
    {
        try { accion(); }
        catch (T ex) { return ex; }
        throw new Exception("No rechazó: " + que);
    }

    /// <summary>Define la clave maestra si falta y blinda los archivos: lo que hacen las pruebas de conciliación antes de conciliar.</summary>
    public static void Blindar(params int[] idsArchivo)
    {
        using (BlindajeService.AutorizarAdministracionClave())
            if (!BlindajeService.HayClaveMaestra()) BlindajeService.DefinirClaveMaestra(Clave, "prueba");
        foreach (int id in idsArchivo) BlindajeService.Blindar(id, "prueba");
    }

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
        string nombre = "AgrWebBlin_" + Guid.NewGuid().ToString("N");
        SqlConnection? admin = null;
        try { admin = new SqlConnection(maestra); admin.Open(); admin.Execute("DECLARE @s nvarchar(max)='CREATE DATABASE '+QUOTENAME(@N); EXEC(@s)", new { N = nombre }); }
        catch (Exception ex) { Console.WriteLine("OMITIDAS: blindaje con base (sin SQL de prueba: " + ex.GetType().Name + "). Definí AGRUPADOR_TEST_SQL."); admin?.Dispose(); return; }
        try
        {
            SqlDb.ConnectionString = new SqlConnectionStringBuilder(maestra) { InitialCatalog = nombre }.ConnectionString;
            using var autorizada = BlindajeService.AutorizarAdministracionClave(); // las pruebas actúan como quien administra la clave; la prueba de permisos la quita a propósito
            typeof(DatabaseHelper).GetField("_inicializada", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!.SetValue(null, false);
            DatabaseHelper.InitializeDatabase();
            using var cn = SqlDb.GetConnection(); cn.Open();

            int concepto = cn.QuerySingle<int>("INSERT bancos.ConceptosEstandar(Nombre) OUTPUT INSERTED.Id VALUES('COBRANZAS')");
            int perfilId = cn.QuerySingle<int>(@"INSERT bancos.PerfilesBanco(NombreBanco,ColumnaConcepto,ColumnaDescripcion,EsCodigo,FilaEncabezado,TipoImporte,ColumnaImporteUnico,ColumnaFecha)
                OUTPUT INSERTED.Id VALUES('Banco X','Concepto','Concepto',0,1,1,'Monto','Fecha')");
            cn.Execute("INSERT bancos.HomologacionConceptos(IdPerfilBanco,ValorOriginal,IdConceptoEstandar) VALUES(@P,'COBRO',@C)", new { P = perfilId, C = concepto });
            var perfil = cn.QuerySingle<PerfilBanco>("SELECT * FROM bancos.PerfilesBanco WHERE Id=@Id", new { Id = perfilId });
            var d1 = new DateTime(2026, 10, 1); var d2 = new DateTime(2026, 10, 2);
            List<MovimientoProcesado> Importar(string nombreArchivo) =>
                ImportacionService.ImportarArchivo(Extracto((d1, "COBRO 1", 1000m), (d2, "COBRO 2", 600m), (d2, "OTRO 3", 400m)), nombreArchivo, perfil);
            List<Dictionary<string, object?>> Historial(int idMov) =>
                cn.Query("SELECT Campo, ValorAnterior, ValorNuevo, Origen, Usuario FROM bancos.MovimientosArchivoHistorial WHERE IdMovimiento=@Id ORDER BY Id", new { Id = idMov })
                  .Select(r => ((IDictionary<string, object?>)r).ToDictionary(k => k.Key, k => k.Value)).ToList();
            string Final(int idMov) => cn.QuerySingle<string>("SELECT ConceptoFinal FROM bancos.MovimientosArchivo WHERE Id=@Id", new { Id = idMov });
            string Estandar(int idMov) => cn.QuerySingle<string>("SELECT ConceptoEstandar FROM bancos.MovimientosArchivo WHERE Id=@Id", new { Id = idMov });

            // ---------- Ítem 1 y 2: concepto final protegido e historial ----------
            var movs = Importar("a.xlsx");
            int idArchivo = movs[0].IdArchivo;
            var editado = movs.Single(m => m.ConceptoOriginal == "COBRO 1");
            var siguiente = movs.Single(m => m.ConceptoOriginal == "COBRO 2");
            var pendiente = movs.Single(m => m.ConceptoOriginal == "OTRO 3");

            test("Una copia en memoria vieja no pisa un concepto final editado a mano y la copia se corrige con lo que hay en la base", () =>
            {
                Check(Final(editado.Id) == "COBRANZAS" && Estandar(editado.Id) == "COBRANZAS", "Importación: el final sigue al estándar");
                var vieja = MovimientoStorage.ObtenerPorArchivo(idArchivo); // copia tomada antes de la edición
                MovimientoStorage.ActualizarConceptoFinal(editado.Id, "COBRANZAS ESPECIAL");
                // Un proceso automático con la copia vieja: cambia el estándar de los dos movimientos homologados.
                var aAplicar = vieja.Where(m => m.Id == editado.Id || m.Id == siguiente.Id).ToList();
                foreach (var m in aAplicar) m.ConceptoEstandar = "COBROS NUEVOS";
                using (AuditoriaContexto.Usar(AuditoriaContexto.Rehomologacion, "mjulian"))
                    Check(MovimientoStorage.ActualizarConceptos(aAplicar) == 2, "Escribe los dos movimientos");
                Check(Final(editado.Id) == "COBRANZAS ESPECIAL", "La edición manual no se toca: " + Final(editado.Id));
                Check(Estandar(editado.Id) == "COBROS NUEVOS", "El estándar sí cambia");
                Check(Final(siguiente.Id) == "COBROS NUEVOS", "Un final que seguía al estándar lo sigue: " + Final(siguiente.Id));
                Check(aAplicar.Single(m => m.Id == editado.Id).ConceptoFinal == "COBRANZAS ESPECIAL", "La copia en memoria queda igual a la base");
            });
            test("El final vacío o pendiente también sigue al estándar, y la cuenta sólo se completa si estaba vacía", () =>
            {
                cn.Execute("UPDATE bancos.MovimientosArchivo SET CuentaFinal='9999' WHERE Id=@Id", new { Id = pendiente.Id });
                var copia = MovimientoStorage.ObtenerPorArchivo(idArchivo).Single(m => m.Id == pendiente.Id);
                copia.ConceptoEstandar = "OTROS"; copia.CuentaFinal = "1111";
                MovimientoStorage.ActualizarConceptos(new[] { copia });
                Check(Final(pendiente.Id) == "OTROS", "El final vacío toma el estándar: " + Final(pendiente.Id));
                Check(cn.QuerySingle<string>("SELECT CuentaFinal FROM bancos.MovimientosArchivo WHERE Id=@Id", new { Id = pendiente.Id }) == "9999", "La cuenta con contenido no se pisa");
            });
            test("Historial: una fila por campo que cambió, con su origen y su usuario", () =>
            {
                var h = Historial(editado.Id);
                var manual = h.Single(x => (string)x["Campo"]! == "ConceptoFinal" && (string)x["Origen"]! == "Manual");
                Check((string)manual["ValorAnterior"]! == "COBRANZAS" && (string)manual["ValorNuevo"]! == "COBRANZAS ESPECIAL", "Valores de la edición manual");
                var estandar = h.Single(x => (string)x["Campo"]! == "ConceptoEstandar");
                Check((string)estandar["Origen"]! == "Re-homologación" && (string)estandar["Usuario"]! == "mjulian"
                    && (string)estandar["ValorAnterior"]! == "COBRANZAS" && (string)estandar["ValorNuevo"]! == "COBROS NUEVOS", "El estándar lo cambió la re-homologación de mjulian");
                Check(h.Count == 2, "El final no cambió en la re-homologación: sólo dos filas (" + h.Count + ")");
                var sigue = Historial(siguiente.Id);
                Check(sigue.Count == 2 && sigue.All(x => (string)x["Origen"]! == "Re-homologación"), "El que seguía cambió estándar y final por el mismo proceso");
            });
            test("Historial: sin ámbito de auditoría el origen queda como Desconocido, y lo que no cambió no genera filas", () =>
            {
                int antes = cn.QuerySingle<int>("SELECT COUNT(*) FROM bancos.MovimientosArchivoHistorial");
                cn.Execute("UPDATE bancos.MovimientosArchivo SET CuentaFinal=CuentaFinal, ConceptoFinal=ConceptoFinal WHERE Id=@Id", new { Id = editado.Id });
                Check(cn.QuerySingle<int>("SELECT COUNT(*) FROM bancos.MovimientosArchivoHistorial") == antes, "Un UPDATE sin cambios no registra nada");
                cn.Execute("UPDATE bancos.MovimientosArchivo SET ConceptoFinal=N'X' WHERE Id=@Id", new { Id = editado.Id });
                var ultima = Historial(editado.Id).Last();
                Check((string)ultima["Origen"]! == "Desconocido" && (string)ultima["ValorNuevo"]! == "X", "Origen desconocido");
                cn.Execute("UPDATE bancos.MovimientosArchivo SET ConceptoFinal=N'COBRANZAS ESPECIAL' WHERE Id=@Id", new { Id = editado.Id });
            });

            // ---------- Ítem 3: blindaje ----------
            test("Sin clave maestra no se puede blindar", () =>
            {
                var ex = Rechaza<InvalidOperationException>(() => BlindajeService.Blindar(idArchivo, "u"), "blindar sin clave");
                Check(ex.Message.Contains("clave maestra"), ex.Message);
                Check(!BlindajeService.HayClaveMaestra() && !BlindajeService.EstaBlindado(idArchivo), "Nada cambió");
            });
            test("Clave maestra: sin permiso para administrarla no se define ni se cambia (falla cerrado)", () =>
            {
                // El hilo se lanza con el flujo de contexto suprimido: no hereda la autorización de la prueba.
                Exception? error = null;
                var hilo = new Thread(() => { try { BlindajeService.DefinirClaveMaestra("otra-clave-larga", "intruso"); } catch (Exception ex) { error = ex; } });
                using (System.Threading.ExecutionContext.SuppressFlow()) hilo.Start();
                hilo.Join();
                Check(error is UnauthorizedAccessException, "Sin permiso: " + (error?.GetType().Name ?? "no falló"));
                Check(!BlindajeService.HayClaveMaestra(), "No quedó ninguna clave");
            });
            test("Clave maestra: se guarda sólo el hash, con largo mínimo, y no se puede definir dos veces", () =>
            {
                Rechaza<ArgumentException>(() => BlindajeService.DefinirClaveMaestra("corta", "u"), "clave corta");
                BlindajeService.DefinirClaveMaestra(Clave, "admin");
                var fila = cn.QuerySingle<(byte[] Hash, byte[] Sal, int It, string Por)>("SELECT Hash, Sal, Iteraciones, CambiadoPor FROM bancos.ClaveMaestra");
                Check(fila.Hash.Length == 32 && fila.Sal.Length == 32 && fila.It >= 100_000 && fila.Por == "admin", "Hash y sal");
                Check(!System.Text.Encoding.UTF8.GetBytes(Clave).SequenceEqual(fila.Hash), "No se guarda la clave");
                Rechaza<InvalidOperationException>(() => BlindajeService.DefinirClaveMaestra("otra-clave-larga", "u"), "segunda definición");
            });
            test("Blindar es idempotente, queda registrado y no se aplica a un archivo inexistente", () =>
            {
                Check(BlindajeService.Blindar(idArchivo, "mjulian") && BlindajeService.EstaBlindado(idArchivo), "Blindó");
                Check(!BlindajeService.Blindar(idArchivo, "mjulian"), "La segunda vez no hace nada");
                Check(cn.QuerySingle<int>("SELECT COUNT(*) FROM bancos.BlindajeLog WHERE IdArchivo=@Id AND Accion='Blindar'", new { Id = idArchivo }) == 1, "Un solo registro");
                var a = cn.QuerySingle<(bool Blindado, string Por)>("SELECT Blindado, BlindadoPor FROM bancos.ArchivosImportados WHERE Id=@Id", new { Id = idArchivo });
                Check(a.Blindado && a.Por == "mjulian", "Quién blindó");
                Rechaza<InvalidOperationException>(() => BlindajeService.Blindar(99999, "u"), "archivo inexistente");
            });
            test("Archivo blindado: no se edita concepto ni cuenta, la homologación automática lo saltea y no se agregan movimientos", () =>
            {
                Rechaza<ArchivoBlindadoException>(() => MovimientoStorage.ActualizarConceptoFinal(editado.Id, "OTRO"), "editar concepto final");
                Rechaza<ArchivoBlindadoException>(() => MovimientoStorage.ActualizarCuentaFinal(editado.Id, "123"), "editar cuenta final");
                var copia = MovimientoStorage.ObtenerPorArchivo(idArchivo).Single(m => m.Id == siguiente.Id);
                copia.ConceptoEstandar = "NO DEBE ENTRAR";
                Check(MovimientoStorage.ActualizarConceptos(new[] { copia }) == 0, "No escribió nada");
                Check(Estandar(siguiente.Id) == "COBROS NUEVOS" && copia.ConceptoEstandar == "COBROS NUEVOS", "La base y la copia quedaron como estaban");
                Rechaza<ArchivoBlindadoException>(() => MovimientoStorage.InsertarLote(new List<MovimientoProcesado>
                    { new() { IdArchivo = idArchivo, Fecha = d1, ConceptoOriginal = "X", DescripcionOriginal = "X", ConceptoEstandar = "X", ConceptoFinal = "X" } }), "insertar en blindado");
            });
            test("Archivo blindado: no se eliminan movimientos ni el archivo", () =>
            {
                Rechaza<ArchivoBlindadoException>(() => EliminacionMovimientosService.EliminarSeleccion(idArchivo, new[] { pendiente.Id }), "eliminar movimientos");
                Rechaza<ArchivoBlindadoException>(() => EliminacionMovimientosService.EliminarArchivo(idArchivo), "eliminar archivo");
                Check(cn.QuerySingle<int>("SELECT COUNT(*) FROM bancos.MovimientosArchivo WHERE IdArchivo=@Id", new { Id = idArchivo }) == 3, "Siguen los tres movimientos");
            });
            test("Archivo blindado: se puede conciliar a mano, y queda en el historial como conciliación manual", () =>
            {
                var (marcados, omitidos) = MovimientoStorage.MarcarConciliados(new[] { siguiente.Id });
                Check(marcados == 1 && omitidos.Count == 0, "Marcó uno");
                var h = Historial(siguiente.Id).Where(x => (string)x["Origen"]! == "Conciliación manual").ToList();
                Check(h.Any(x => (string)x["Campo"]! == "Conciliado" && (string)x["ValorNuevo"]! == "1")
                      && h.Any(x => (string)x["Campo"]! == "ConciliadoFecha" && x["ValorAnterior"] == null), "Conciliado y fecha");
            });

            // ---------- Conciliar ya no exige blindaje ----------
            // Fecha: 05/11/2026 - TAREA: 00162 - Linea: 1 - Las pruebas de la 00135 afirmaban que conciliar exigía el archivo blindado; ahora afirman lo contrario
            var movsB = Importar("b.xlsx");
            int idArchivoB = movsB[0].IdArchivo;
            test("Conciliar a mano un archivo sin blindar se puede, y queda en el historial como conciliación manual", () =>
            {
                Check(!BlindajeService.EstaBlindado(idArchivoB), "b.xlsx no está blindado");
                var (marcados, omitidos) = MovimientoStorage.MarcarConciliados(new[] { movsB[2].Id, siguiente.Id });
                Check(marcados == 1 && omitidos.Count == 1 && omitidos[0].Id == siguiente.Id, $"Marcó el libre y omitió el ya conciliado: {marcados}/{omitidos.Count}");
                Check(cn.QuerySingle<int>("SELECT COUNT(*) FROM bancos.MovimientosArchivo WHERE IdArchivo=@Id AND Conciliado=1", new { Id = idArchivoB }) == 1, "Quedó conciliado un movimiento de un archivo sin blindar");
                Check(Historial(movsB[2].Id).Any(x => (string)x["Campo"]! == "Conciliado" && (string)x["Origen"]! == "Conciliación manual"), "Queda en el historial");
            });
            // Fecha: 05/11/2026 - TAREA: 00162 - Linea: 2 - Revisión 1, hallazgos 1 y 2: lo conciliado no se edita (validado en el storage) y el marcado manual no cuenta lo que ya no existe
            test("Un movimiento conciliado a mano no se edita aunque la pantalla tenga una copia vieja", () =>
            {
                Rechaza<MovimientoConciliadoException>(() => MovimientoStorage.ActualizarConceptoFinal(movsB[2].Id, "NO"), "editar el concepto final de un conciliado");
                Rechaza<MovimientoConciliadoException>(() => MovimientoStorage.ActualizarCuentaFinal(movsB[2].Id, "123"), "editar la cuenta final de un conciliado");
                Check(Final(movsB[2].Id) != "NO", "No se escribió nada");
            });
            test("Marcar a mano un movimiento que ya no existe lo informa como omitido y no como marcado", () =>
            {
                var (marcados, omitidos) = MovimientoStorage.MarcarConciliados(new[] { 987654321 });
                Check(marcados == 0 && omitidos.Count == 1 && omitidos[0].Motivo.Contains("ya no existe"), $"{marcados}/{omitidos.Count}");
            });
            test("Sesión externa: se crea con un archivo blindado y otro sin blindar; en proceso, no deja desblindar", () =>
            {
                var items = new List<ConciliacionItemExterno> { new() { Fecha = d1, Importe = 1000m, Detalle = "Cobranza" } };
                var sesion = ConciliacionExternService.CrearSesion("S", new List<int> { idArchivo, idArchivoB }, new[] { "COBROS NUEVOS" }, items);
                Check(cn.QuerySingle<int>("SELECT COUNT(*) FROM bancos.ConciliacionSesiones") == 1, "Se creó con un archivo sin blindar");
                var ex = Rechaza<InvalidOperationException>(() => BlindajeService.Desblindar(idArchivo, Clave, "probar", "u"), "desblindar con sesión en proceso");
                Check(ex.Message.Contains("sesión de conciliación") && BlindajeService.EstaBlindado(idArchivo), ex.Message);
                ConciliacionExternService.EliminarSesion(sesion.Id);
            });
            test("Conciliar con un archivo dado de baja se sigue rechazando, con su propio mensaje", () =>
            {
                var movsH = Importar("h.xlsx"); int idH = movsH[0].IdArchivo;
                EliminacionMovimientosService.EliminarArchivo(idH);
                var items = new List<ConciliacionItemExterno> { new() { Fecha = d1, Importe = 1000m, Detalle = "Cobranza" } };
                var ex = Rechaza<ArchivoInactivoException>(() => ConciliacionExternService.CrearSesion("Baja", new List<int> { idH }, new[] { "COBRANZAS" }, items), "sesión sobre un archivo dado de baja");
                Check(ex.Message.Contains("dado de baja") && ex.Message.Contains("h.xlsx"), ex.Message);
            });
            test("Sesión interna: un par entre un archivo blindado y otro sin blindar se concilia; en proceso, no deja desblindar", () =>
            {
                Blindar(idArchivoB);
                var movsC = Importar("c.xlsx"); // sin blindar
                var sesion = ConciliacionInternaService.CrearSesion("I", d1, new DateTime(2026, 10, 31), new[] { "COBROS NUEVOS", "COBRANZAS" });
                // Fecha: 26/10/2026 - TAREA: 00135 - Linea: 3 - Revisión 1, hallazgo 3: aun sin pares, la sesión en proceso alcanza los movimientos de su rango y conceptos
                Rechaza<InvalidOperationException>(() => BlindajeService.Desblindar(idArchivoB, Clave, "sin pares todavía", "u"), "desblindar con una sesión interna que alcanza el archivo");
                Check(BlindajeService.EstaBlindado(idArchivoB), "Sigue blindado");
                // Fecha: 05/11/2026 - TAREA: 00162 - Linea: 1 - Un archivo blindado con otro sin blindar ya se concilia; también dos sin blindar
                Check(ConciliacionInternaMutationStorage.ConciliarPar(sesion.Id, movsB[0].Id, movsC[0].Id, TipoMatch.Manual) == null, "Blindado con sin blindar se concilia");
                Check(ConciliacionInternaMutationStorage.ConciliarPar(sesion.Id, movsC[1].Id, movsC[2].Id, TipoMatch.Manual) == null, "Entre archivos sin blindar también");
                Check(ConciliacionInternaMutationStorage.ObtenerPares(sesion.Id).Count == 2, "Quedaron los dos pares");
                var ex = Rechaza<InvalidOperationException>(() => BlindajeService.Desblindar(idArchivoB, Clave, "probar", "u"), "desblindar con sesión interna en proceso");
                Check(ex.Message.Contains("sesión de conciliación"), ex.Message);
                ConciliacionInternaService.EliminarSesion(sesion.Id);
                BlindajeService.Desblindar(idArchivoB, Clave, "prueba terminada", "u");
                BlindajeService.Desblindar(idArchivo, Clave, "prueba terminada", "u");
                Blindar(idArchivo);
            });

            // ---------- Un movimiento apareado no se toca, aunque la conciliación no esté cerrada (00162, línea 2) ----------
            // Fecha: 05/11/2026 - TAREA: 00162 - Linea: 2 - Los datos van en un perfil propio: las reglas del perfil se prueban sin alterar el resto
            int perfilYId = cn.QuerySingle<int>(@"INSERT bancos.PerfilesBanco(NombreBanco,ColumnaConcepto,ColumnaDescripcion,EsCodigo,FilaEncabezado,TipoImporte,ColumnaImporteUnico,ColumnaFecha)
                OUTPUT INSERTED.Id VALUES('Banco Y','Concepto','Concepto',0,1,1,'Monto','Fecha')");
            cn.Execute("INSERT bancos.HomologacionConceptos(IdPerfilBanco,ValorOriginal,IdConceptoEstandar) VALUES(@P,'COBRO',@C)", new { P = perfilYId, C = concepto });
            var perfilY = cn.QuerySingle<PerfilBanco>("SELECT * FROM bancos.PerfilesBanco WHERE Id=@Id", new { Id = perfilYId });
            var movsG = ImportacionService.ImportarArchivo(Extracto((d1, "COBRO 1", 1000m), (d2, "COBRO 2", 600m), (d2, "OTRO 3", 400m)), "g.xlsx", perfilY); // sin blindar
            int idG = movsG[0].IdArchivo;
            var g0 = movsG.Single(m => m.ConceptoOriginal == "COBRO 1"); var g1 = movsG.Single(m => m.ConceptoOriginal == "COBRO 2"); var g2 = movsG.Single(m => m.ConceptoOriginal == "OTRO 3");
            var sesionInternaG = ConciliacionInternaService.CrearSesion("Apareados", d1, new DateTime(2026, 10, 31), new[] { "COBRANZAS", ConceptosBancarios.PendienteHomologar });
            int sesionExternaGId = 0, sesionCarreraId = 0, idK = 0;

            test("Apareado en una sesión interna en proceso: no se edita concepto ni cuenta; uno libre del mismo archivo sí", () =>
            {
                Check(ConciliacionInternaMutationStorage.ConciliarPar(sesionInternaG.Id, g0.Id, g1.Id, TipoMatch.Manual) == null, "Se apareó");
                var ex = Rechaza<MovimientoApareadoException>(() => MovimientoStorage.ActualizarConceptoFinal(g0.Id, "NO"), "editar el concepto final de un apareado");
                Check(ex.Message.Contains("conciliación interna") && ex.Message.Contains("Apareados") && ex.Message.Contains("no se puede modificar"), ex.Message);
                Rechaza<MovimientoApareadoException>(() => MovimientoStorage.ActualizarCuentaFinal(g1.Id, "123"), "editar la cuenta final de un apareado");
                Check(Final(g0.Id) == "COBRANZAS", "No se escribió nada");
                MovimientoStorage.ActualizarConceptoFinal(g2.Id, "LIBRE EDITADO");
                Check(Final(g2.Id) == "LIBRE EDITADO", "Un movimiento libre del mismo archivo sí se edita");
            });
            test("La grilla marca los apareados al cargar el archivo, con dónde y en qué estado", () =>
            {
                var carga = MovimientoStorage.ObtenerPorArchivo(idG);
                var a = carga.Single(m => m.Id == g0.Id); var libre = carga.Single(m => m.Id == g2.Id);
                Check(a.Apareado && a.ApareadoEn!.Contains("Apareados") && a.ApareadoEn.Contains("en proceso") && a.MensajeApareado!.Contains("no se puede modificar"), "Marcado: " + a.ApareadoEn);
                Check(!libre.Apareado && libre.ApareadoEn == null && libre.MensajeApareado == null, "El libre no se marca");
            });
            test("Una regla automática saltea al apareado y escribe al libre", () =>
            {
                var copia = MovimientoStorage.ObtenerPorArchivo(idG).Where(m => m.Id == g0.Id || m.Id == g2.Id).ToList();
                foreach (var m in copia) m.ConceptoEstandar = "REGLA NUEVA";
                Check(MovimientoStorage.ActualizarConceptos(copia) == 1, "Sólo escribió el libre");
                Check(Estandar(g0.Id) == "COBRANZAS" && Estandar(g2.Id) == "REGLA NUEVA", "El apareado conserva su estándar: " + Estandar(g0.Id));
                Check(copia.Single(m => m.Id == g0.Id).ConceptoEstandar == "COBRANZAS", "La copia en memoria se corrige con la base");
            });
            test("Eliminar rechaza y marcar como conciliado omite al apareado", () =>
            {
                var ex = Rechaza<InvalidOperationException>(() => EliminacionMovimientosService.EliminarSeleccion(idG, new[] { g0.Id }), "eliminar un apareado");
                Check(ex.Message.Contains("pares de conciliación"), ex.Message);
                var (marcados, omitidos) = MovimientoStorage.MarcarConciliados(new[] { g0.Id });
                Check(marcados == 0 && omitidos.Count == 1 && omitidos[0].Motivo.Contains("conciliación interna"), $"{marcados}/{omitidos.Count}");
            });
            test("Con la sesión interna finalizada el movimiento sigue apareado y no se edita", () =>
            {
                ConciliacionInternaMutationStorage.Finalizar(sesionInternaG.Id, new[] { (g0.Id, "1.1.01.001", g1.Id, "1.1.01.002") });
                var par = ParesMovimientoStorage.ObtenerApareados(new[] { g0.Id })[g0.Id];
                Check(par.Estado == "Finalizada" && par.Via == "interna", "Finalizada: " + par.Estado);
                var ex = Rechaza<MovimientoApareadoException>(() => MovimientoStorage.ActualizarConceptoFinal(g1.Id, "NO"), "editar un apareado de una sesión finalizada");
                Check(ex.Par.Detalle.Contains("finalizada"), ex.Par.Detalle);
            });
            test("Apareado en una sesión externa: tampoco se edita", () =>
            {
                var items = new List<ConciliacionItemExterno> { new() { Fecha = d2, Importe = 400m, Detalle = "Cobranza externa" } };
                var sesion = ConciliacionExternService.CrearSesion("Externa apareados", new List<int> { idG }, new[] { ConceptosBancarios.PendienteHomologar }, items);
                sesionExternaGId = sesion.Id;
                int idItem = cn.QuerySingle<int>("SELECT Id FROM bancos.ConciliacionItemsExternos WHERE IdSesion=@S", new { S = sesion.Id });
                ConciliacionExternService.ConciliarPar(sesion.Id, idItem, g2.Id, TipoMatch.Manual);
                var ex = Rechaza<MovimientoApareadoException>(() => MovimientoStorage.ActualizarConceptoFinal(g2.Id, "NO"), "editar un apareado en una sesión externa");
                Check(ex.Message.Contains("conciliación externa") && ex.Message.Contains("Externa apareados"), ex.Message);
            });
            // Fecha: 05/11/2026 - TAREA: 00162 - Linea: 2 - Revisión 1, hallazgos 4 y 6: la prueba ahora ejecuta la baja, y comprueba que lo apareado después de calcular el impacto también se cuenta
            test("Reglas del perfil: la baja y reapuntar saltean los apareados y los cuentan aparte, también si se aparean después de calcular el impacto", () =>
            {
                var regla = HomologacionStorage.ObtenerListado(perfilYId).Single(r => r.ValorOriginal == "COBRO");
                var impacto0 = HomologacionAdminService.CalcularImpactoBaja(regla, perfilY);
                Check(impacto0.EnMovimientosApareados == 2 && impacto0.Total == 0, $"Dos apareados y ninguno a tocar: {impacto0.EnMovimientosApareados}/{impacto0.Total}");
                int reapuntado = HomologacionAdminService.Reapuntar(regla, perfilY, "REAPUNTADO Y", out int porBlindaje, out int porPares);
                Check(reapuntado > 0 && porBlindaje == 0 && porPares == 2, $"Saltó dos apareados: {porBlindaje}/{porPares}");
                Check(Estandar(g0.Id) == "COBRANZAS" && Estandar(g1.Id) == "COBRANZAS", "Los apareados no se arrastran");

                // Un archivo nuevo con dos movimientos libres de la misma regla.
                var movsK = ImportacionService.ImportarArchivo(Extracto((d1, "COBRO 1", 100m), (d2, "COBRO 2", 200m)), "k.xlsx", perfilY);
                idK = movsK[0].IdArchivo;
                var regla2 = HomologacionStorage.ObtenerListado(perfilYId).Single(r => r.ValorOriginal == "COBRO");
                var impacto = HomologacionAdminService.CalcularImpactoBaja(regla2, perfilY);
                Check(impacto.EnMovimientosApareados == 2 && impacto.Afectados.Count == 2, $"Dos apareados y dos libres: {impacto.EnMovimientosApareados}/{impacto.Afectados.Count}");
                // Otra sesión aparea los dos libres mientras el diálogo de la baja está abierto.
                var carrera = ConciliacionInternaService.CrearSesion("Carrera", d1, new DateTime(2026, 10, 31), new[] { "REAPUNTADO Y" });
                sesionCarreraId = carrera.Id;
                Check(ConciliacionInternaMutationStorage.ConciliarPar(carrera.Id, movsK[0].Id, movsK[1].Id, TipoMatch.Manual) == null, "Se aparearon");
                HomologacionAdminService.AplicarBaja(regla2, impacto, null);
                Check(impacto.EnMovimientosApareados == 4, "La baja suma los que la base salteó: " + impacto.EnMovimientosApareados);
                Check(Estandar(movsK[0].Id) == "REAPUNTADO Y" && Estandar(movsK[1].Id) == "REAPUNTADO Y", "No se despegaron");
                Check(HomologacionStorage.ObtenerListado(perfilYId).All(r => r.ValorOriginal != "COBRO"), "La regla se dio de baja");
            });
            test("Homologación masiva y reaplicación: los apareados se saltean y se cuentan", () =>
            {
                var copia = MovimientoStorage.ObtenerPorArchivo(idG).Where(m => m.Id == g0.Id || m.Id == g2.Id).ToList();
                var cambiados = SesionMovimientosService.ReaplicarHomologacion(copia, perfilY, out int noModificados);
                Check(cambiados.Count > 0 && noModificados == cambiados.Count, $"Los dos están apareados: todo lo que había que escribir se salteó y se contó ({noModificados}/{cambiados.Count})");
                Check(Estandar(g0.Id) == "COBRANZAS", "El apareado no cambia");
            });
            test("El aviso de movimientos no modificados junta blindados y apareados", () =>
            {
                Check(HomologacionAdminService.AvisoNoModificados(12, 5) == "12 movimiento(s) de archivos blindados y 5 apareados en una conciliación no se modificaron.", "Ambos");
                Check(HomologacionAdminService.AvisoNoModificados(3, 0) == "3 movimiento(s) de archivos blindados no se modificaron.", "Sólo blindados");
                Check(HomologacionAdminService.AvisoNoModificados(0, 4) == "4 movimiento(s) apareados en una conciliación no se modificaron.", "Sólo apareados");
                Check(HomologacionAdminService.AvisoNoModificados(0, 0) == null, "Ninguno");
            });
            test("Apareados: limpieza de los datos de prueba", () =>
            {
                cn.Execute(@"DELETE FROM bancos.ConciliacionInternaPares WHERE IdSesion=@I; DELETE FROM bancos.ConciliacionInternaSesiones WHERE Id=@I;
                    DELETE FROM bancos.ConciliacionPares WHERE IdSesion=@E; DELETE FROM bancos.ConciliacionItemsExternos WHERE IdSesion=@E; DELETE FROM bancos.ConciliacionSesiones WHERE Id=@E;
                    DELETE FROM bancos.ConciliacionInternaPares WHERE IdSesion=@C; DELETE FROM bancos.ConciliacionInternaSesiones WHERE Id=@C;
                    DELETE FROM bancos.MovimientosArchivo WHERE IdArchivo IN (@G, @K); DELETE FROM bancos.ArchivosImportados WHERE Id IN (@G, @K);
                    DELETE FROM bancos.HomologacionConceptos WHERE IdPerfilBanco=@Y; DELETE FROM bancos.PerfilesBanco WHERE Id=@Y;",
                    new { I = sesionInternaG.Id, E = sesionExternaGId, C = sesionCarreraId, G = idG, K = idK, Y = perfilYId });
                Check(cn.QuerySingle<int>("SELECT COUNT(*) FROM bancos.ConciliacionInternaSesiones") == 0 && cn.QuerySingle<int>("SELECT COUNT(*) FROM bancos.ConciliacionSesiones") == 0, "No quedaron sesiones");
            });

            // ---------- Desblindar y clave maestra ----------
            test("Desblindar exige motivo y clave; la clave incorrecta cuenta intentos y se bloquea al quinto", () =>
            {
                Rechaza<ArgumentException>(() => BlindajeService.Desblindar(idArchivo, Clave, "  ", "u"), "sin motivo");
                var ex = Rechaza<ClaveMaestraIncorrectaException>(() => BlindajeService.Desblindar(idArchivo, "mala", "motivo", "u"), "clave incorrecta");
                Check(ex.IntentosRestantes == 4 && BlindajeService.EstaBlindado(idArchivo), "Un intento menos y sigue blindado");
                for (int i = 0; i < 3; i++) Rechaza<ClaveMaestraIncorrectaException>(() => BlindajeService.Desblindar(idArchivo, "mala", "motivo", "u"), "clave incorrecta");
                var ultimo = Rechaza<ClaveMaestraBloqueadaException>(() => BlindajeService.Desblindar(idArchivo, "mala", "motivo", "u"), "quinto intento");
                Check(ultimo.Segundos > 0, "Bloqueada");
                Rechaza<ClaveMaestraBloqueadaException>(() => BlindajeService.Desblindar(idArchivo, Clave, "motivo", "u"), "la clave correcta tampoco entra mientras dura el bloqueo");
                Check(BlindajeService.EstaBlindado(idArchivo), "Sigue blindado");
                Check(cn.QuerySingle<int>("SELECT COUNT(*) FROM bancos.BlindajeLog WHERE IdArchivo=@Id AND Accion='DesblindoFallido'", new { Id = idArchivo }) == 4, "Quedan registrados los intentos fallidos");
                cn.Execute("UPDATE bancos.ClaveMaestra SET BloqueadoHasta = DATEADD(MINUTE,-1,SYSDATETIME())");
            });
            test("Desblindar con la clave correcta lo deja registrado con motivo y reinicia los intentos; vuelve a permitir editar", () =>
            {
                BlindajeService.Desblindar(idArchivo, Clave, "Corrección del concepto", "mjulian");
                Check(!BlindajeService.EstaBlindado(idArchivo), "Desblindado");
                var log = cn.QuerySingle<(string Usuario, string Motivo)>("SELECT TOP 1 Usuario, Motivo FROM bancos.BlindajeLog WHERE IdArchivo=@Id AND Accion='Desblindar' ORDER BY Id DESC", new { Id = idArchivo });
                Check(log.Usuario == "mjulian" && log.Motivo == "Corrección del concepto", "Quién y por qué");
                Check(cn.QuerySingle<int>("SELECT IntentosFallidos FROM bancos.ClaveMaestra") == 0, "Intentos reiniciados");
                MovimientoStorage.ActualizarConceptoFinal(pendiente.Id, "CORREGIDO");
                Check(Final(pendiente.Id) == "CORREGIDO", "Ya se puede editar");
            });
            test("Cambiar la clave maestra pide la actual; la vieja deja de servir", () =>
            {
                Rechaza<ClaveMaestraIncorrectaException>(() => BlindajeService.CambiarClaveMaestra("equivocada", "clave-nueva-2", "admin"), "clave actual incorrecta");
                BlindajeService.CambiarClaveMaestra(Clave, "clave-nueva-2", "admin");
                Blindar(idArchivo);
                Rechaza<ClaveMaestraIncorrectaException>(() => BlindajeService.Desblindar(idArchivo, Clave, "motivo", "u"), "la clave vieja");
                BlindajeService.Desblindar(idArchivo, "clave-nueva-2", "motivo", "u");
                Check(!BlindajeService.EstaBlindado(idArchivo), "Con la nueva sí");
            });

            // ---------- Reglas del perfil: los archivos blindados no se tocan ----------
            test("Reglas del perfil: reapuntar y dar de baja una regla saltean los archivos blindados y lo informan", () =>
            {
                var movsE = Importar("e.xlsx"); var movsF = Importar("f.xlsx");
                int idF = movsF[0].IdArchivo;
                Blindar(idF);
                var regla = HomologacionStorage.ObtenerListado(perfilId).Single(r => r.ValorOriginal == "COBRO");
                int reapuntado = HomologacionAdminService.Reapuntar(regla, perfil, "REAPUNTADO", out int saltados);
                Check(reapuntado > 0 && saltados == 2, "Dos movimientos de COBRO en el archivo blindado: " + saltados);
                Check(movsE.Where(m => m.ConceptoOriginal.StartsWith("COBRO")).All(m => Estandar(m.Id) == "REAPUNTADO" && Final(m.Id) == "REAPUNTADO"), "El archivo sin blindar sí se arrastra");
                Check(movsF.Where(m => m.ConceptoOriginal.StartsWith("COBRO")).All(m => Estandar(m.Id) == "COBRANZAS" && Final(m.Id) == "COBRANZAS"), "El blindado queda como estaba");
                var regla2 = HomologacionStorage.ObtenerListado(perfilId).Single(r => r.ValorOriginal == "COBRO");
                var impacto = HomologacionAdminService.CalcularImpactoBaja(regla2, perfil);
                Check(impacto.EnArchivosBlindados == 2 && impacto.Afectados.All(i => i.Movimiento.IdArchivo != idF), "El impacto no incluye al blindado y lo cuenta aparte");
                HomologacionAdminService.AplicarBaja(regla2, impacto, null);
                Check(movsE.Where(m => m.ConceptoOriginal.StartsWith("COBRO")).All(m => Estandar(m.Id) == ConceptosBancarios.PendienteHomologar), "El archivo sin blindar vuelve a pendiente");
                Check(movsF.Where(m => m.ConceptoOriginal.StartsWith("COBRO")).All(m => Estandar(m.Id) == "COBRANZAS"), "El blindado sigue igual");
            });

            // ---------- Baja de archivo: aviso, archivo inactivo y movimientos archivados ----------
            test("Baja con modificaciones: cuenta lo editado, deja el archivo inactivo y archiva los movimientos en JSON comprimido", () =>
            {
                var movsD = Importar("d.xlsx"); int idD = movsD[0].IdArchivo;
                var sinCambios = EliminacionMovimientosStorage.ContarModificaciones(idD);
                Check(!sinCambios.HayModificaciones, "Recién importado no tiene modificaciones");
                MovimientoStorage.ActualizarConceptoFinal(movsD[0].Id, "EDITADO A MANO");
                Blindar(idD);
                MovimientoStorage.MarcarConciliados(new[] { movsD[1].Id });
                BlindajeService.Desblindar(idD, "clave-nueva-2", "preparar la baja", "u");
                var mod = EliminacionMovimientosStorage.ContarModificaciones(idD);
                Check(mod.Total == 2 && mod.ConceptosFinalesEditados == 1 && mod.Conciliados == 1, $"Conteo: {mod.Total}/{mod.ConceptosFinalesEditados}/{mod.Conciliados}");
                var idsOriginales = movsD.Select(m => m.Id).OrderBy(x => x).ToList();
                var finales = cn.Query<(int Id, string Final)>("SELECT Id, ConceptoFinal FROM bancos.MovimientosArchivo WHERE IdArchivo=@Id", new { Id = idD }).ToDictionary(x => x.Id, x => x.Final);

                using (AuditoriaContexto.Usar(usuario: "mjulian")) EliminacionMovimientosService.EliminarArchivo(idD);

                var a = cn.QuerySingle<(bool Activo, int? Cant, string? Por, DateTime? Fecha, byte[]? Json)>("SELECT Activo, CantMovimientos, InactivoPor, InactivoFecha, MovimientosJson FROM bancos.ArchivosImportados WHERE Id=@Id", new { Id = idD });
                Check(!a.Activo && a.Cant == 3 && a.Por == "mjulian" && a.Fecha != null && a.Json is { Length: > 0 }, "El archivo queda inactivo con sus datos");
                Check(cn.QuerySingle<int>("SELECT COUNT(*) FROM bancos.MovimientosArchivo WHERE IdArchivo=@Id", new { Id = idD }) == 0, "Los movimientos salieron de la tabla viva");
                using var gz = new GZipStream(new MemoryStream(a.Json!), CompressionMode.Decompress);
                using var doc = JsonDocument.Parse(gz);
                var filas = doc.RootElement.EnumerateArray().ToList();
                Check(filas.Count == 3 && filas.Select(f => f.GetProperty("Id").GetInt32()).OrderBy(x => x).SequenceEqual(idsOriginales), "El JSON conserva los Id originales");
                Check(filas.All(f => f.GetProperty("ConceptoFinal").GetString() == finales[f.GetProperty("Id").GetInt32()]), "Y el concepto final de cada uno");
                Check(filas.Count(f => f.GetProperty("Conciliado").GetBoolean()) == 1, "Y lo conciliado");
                Check(Historial(movsD[0].Id).Any(x => (string)x["Origen"]! == "Manual"), "El historial de los movimientos se conserva");
                Check(!ArchivoImportadoStorage.ObtenerTodos().Any(x => x.Id == idD) && !ArchivoImportadoStorage.ObtenerPorPerfil(perfilId).Any(x => x.Id == idD), "No aparece en los listados");
                Rechaza<InvalidOperationException>(() => EliminacionMovimientosService.EliminarArchivo(idD), "dar de baja dos veces");
                Rechaza<InvalidOperationException>(() => BlindajeService.Blindar(idD, "u"), "blindar un archivo inactivo");
            });
        }
        finally
        {
            try { SqlConnection.ClearAllPools(); admin!.Execute("DECLARE @s nvarchar(max)='ALTER DATABASE '+QUOTENAME(@N)+' SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE '+QUOTENAME(@N); EXEC(@s)", new { N = nombre }); } catch { }
            admin?.Dispose();
        }
    }
}
