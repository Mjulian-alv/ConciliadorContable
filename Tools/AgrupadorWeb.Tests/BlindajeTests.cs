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

            // ---------- Conciliar exige blindaje ----------
            var movsB = Importar("b.xlsx");
            int idArchivoB = movsB[0].IdArchivo;
            test("Conciliar a mano un archivo sin blindar se rechaza entero, nombrando el archivo", () =>
            {
                var ex = Rechaza<ArchivoSinBlindarException>(() => MovimientoStorage.MarcarConciliados(new[] { movsB[0].Id, siguiente.Id }), "conciliar sin blindar");
                Check(ex.Message.StartsWith("Para conciliar, blindá primero el archivo:") && ex.Message.Contains("b.xlsx") && ex.Message.Contains("Banco X"), ex.Message);
                Check(cn.QuerySingle<int>("SELECT COUNT(*) FROM bancos.MovimientosArchivo WHERE IdArchivo=@Id AND Conciliado=1", new { Id = idArchivoB }) == 0, "No se marcó nada");
            });
            test("Sesión externa: con un archivo sin blindar no se crea; blindado sí, y bloquea desblindar mientras esté en proceso", () =>
            {
                var items = new List<ConciliacionItemExterno> { new() { Fecha = d1, Importe = 1000m, Detalle = "Cobranza" } };
                Rechaza<ArchivoSinBlindarException>(() => ConciliacionExternService.CrearSesion("S", new List<int> { idArchivo, idArchivoB }, new[] { "COBROS NUEVOS" }, items), "sesión con archivo sin blindar");
                Check(cn.QuerySingle<int>("SELECT COUNT(*) FROM bancos.ConciliacionSesiones") == 0, "No quedó sesión");
                var sesion = ConciliacionExternService.CrearSesion("S", new List<int> { idArchivo }, new[] { "COBROS NUEVOS" }, items);
                var ex = Rechaza<InvalidOperationException>(() => BlindajeService.Desblindar(idArchivo, Clave, "probar", "u"), "desblindar con sesión en proceso");
                Check(ex.Message.Contains("sesión de conciliación") && BlindajeService.EstaBlindado(idArchivo), ex.Message);
                ConciliacionExternService.EliminarSesion(sesion.Id);
            });
            test("Sesión interna: un par con un archivo sin blindar se rechaza con el mensaje; entre archivos blindados se concilia", () =>
            {
                Blindar(idArchivoB);
                var movsC = Importar("c.xlsx"); // sin blindar
                var sesion = ConciliacionInternaService.CrearSesion("I", d1, new DateTime(2026, 10, 31), new[] { "COBROS NUEVOS", "COBRANZAS" });
                string? msg = ConciliacionInternaMutationStorage.ConciliarPar(sesion.Id, movsB[0].Id, movsC[0].Id, TipoMatch.Manual);
                Check(msg != null && msg.StartsWith("Para conciliar, blindá primero el archivo:") && msg.Contains("c.xlsx"), "Mensaje: " + msg);
                Check(ConciliacionInternaMutationStorage.ObtenerPares(sesion.Id).Count == 0, "No hay par");
                Check(ConciliacionInternaMutationStorage.ConciliarPar(sesion.Id, movsB[0].Id, movsB[1].Id, TipoMatch.Manual) == null, "Entre blindados se concilia");
                var ex = Rechaza<InvalidOperationException>(() => BlindajeService.Desblindar(idArchivoB, Clave, "probar", "u"), "desblindar con sesión interna en proceso");
                Check(ex.Message.Contains("sesión de conciliación"), ex.Message);
                ConciliacionInternaService.EliminarSesion(sesion.Id);
                BlindajeService.Desblindar(idArchivoB, Clave, "prueba terminada", "u");
                BlindajeService.Desblindar(idArchivo, Clave, "prueba terminada", "u");
                Blindar(idArchivo);
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
