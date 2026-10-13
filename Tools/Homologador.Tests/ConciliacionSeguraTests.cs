// Fecha: 13/10/2026 - TAREA: 00083 - Linea: 1 - Los casos operativos usan días tipados; las entradas inválidas se verifican en FechasBancarias.Tests.
// Fecha: 07/10/2026 - TAREA: 00076 - Linea: 1, 2 - Regresiones de fechas y pertenencia de conciliación.
using AgrupadorConceptos.Services;
using AgrupadorConceptos.Models;
using Conciliador.Comun;
using Dapper;
using ClosedXML.Excel;
using System.Globalization;

internal static class ConciliacionSeguraTests
{
    // Fecha: 09/10/2026 - TAREA: 00085 - Linea: 1 - Una columna manual aislada impedía mostrar las demás al enlazar movimientos reales.
    public static void ColumnasExtractoConPerfil()
    {
        using var form = new AgrupadorConceptos.ConciliacionExternForm();
        var movimiento = new MovimientoProcesado
        {
            Id = 1, IdArchivo = 2, Fecha = new DateTime(2026, 10, 9), ConceptoOriginal = "TRANSFERENCIA",
            DescripcionOriginal = "Movimiento de prueba", ConceptoEstandar = "Transferencias",
            Debitos = 1250.50m, Creditos = 20m, ConceptoFinal = "Transferencia recibida",
            CuentaFinal = "110101", NombreBanco = "Perfil Norte"
        };
        var esperados = new Dictionary<string, object>
        {
            ["Fecha"] = new DateTime(2026,10,9), ["ConceptoOriginal"] = "TRANSFERENCIA",
            ["DescripcionOriginal"] = "Movimiento de prueba", ["ConceptoEstandar"] = "Transferencias",
            ["Debitos"] = 1250.50m, ["Creditos"] = 20m, ["ConceptoFinal"] = "Transferencia recibida",
            ["CuentaFinal"] = "110101", ["NombreBanco"] = "Perfil Norte"
        };
        foreach (var nombre in new[] { "dgvExtractoPendiente", "dgvExtractoSinConcepto" })
        {
            var grilla = (Telerik.WinControls.UI.RadGridView)form.Controls.Find(nombre, true).Single();
            grilla.BindingContext = new BindingContext();
            // Cubrir también el rebind al refrescar o retomar la sesión.
            for (int carga = 0; carga < 2; carga++)
            {
                grilla.DataSource = null;
                grilla.DataSource = new List<MovimientoProcesado> { movimiento };
                Application.DoEvents();
                foreach (var (campo, valor) in esperados)
                {
                    if (grilla.Columns[campo] == null || !grilla.Columns[campo].IsVisible)
                        throw new Exception($"{nombre}: falta la columna visible {campo}; sólo aparecen {string.Join(", ", grilla.Columns.Select(c => c.Name))}.");
                    if (grilla.Rows.Count != 1 || !Equals(grilla.Rows[0].Cells[campo].Value, valor))
                        throw new Exception($"{nombre}: no se muestra el valor esperado de {campo}.");
                }
                if (grilla.Columns.Count(c => c.Name == "NombreBanco") != 1 || grilla.Columns["NombreBanco"].HeaderText != "Perfil")
                    throw new Exception($"{nombre}: el perfil está duplicado o perdió su encabezado.");
            }
        }
    }

    private static DateTime? ParsearEntrada(string valor) => Conciliador.Comun.Services.FechaBancariaParser.TryParse(valor,out var fecha)?fecha:null;

    public static void FechasConHora()
    {
        if (ParsearEntrada("02/09/2026 23:59:59") != new DateTime(2026, 9, 2))
            throw new Exception("La fecha argentina con hora no conserva el día.");
        if (ParsearEntrada("2026-09-02T12:30:00.123") != new DateTime(2026, 9, 2))
            throw new Exception("La fecha ISO con fracciones no conserva el día.");
        if (ParsearEntrada("no-fecha") != null)
            throw new Exception("La fecha inválida no debe interpretarse.");
    }

    // Fecha: 08/10/2026 - TAREA: 00076 - Linea: 1, 2 - Probar persistencia y rechazo atómico sobre SQL Server real desechable.
    public static void BajasYGrupos()
    {
        using var cn = SqlDb.GetConnection();
        cn.Open();
        var archivo = cn.QuerySingle<int>("SELECT TOP 1 Id FROM bancos.ArchivosImportados ORDER BY Id");
        var ids = cn.Query<int>("SELECT TOP 3 Id FROM bancos.MovimientosArchivo WHERE IdArchivo = @Id ORDER BY Id", new { Id = archivo }).ToArray();
        var libre = ids[0];
        var protegido = ids[1];
        var sesion = ConciliacionExternService.CrearSesion("Prueba de protección", new List<int> { archivo },
            new[] { "A" }, new[] { new ConciliacionItemExterno { Fecha = new DateTime(2026, 9, 2), Importe = 100m, Detalle = "Grupo" } });
        Rechazar(() => EliminacionMovimientosService.EliminarSeleccion(archivo, new[] { libre, protegido }));
        Igual(cn.QuerySingle<int>("SELECT COUNT(*) FROM bancos.MovimientosArchivo WHERE Id IN @Ids", new { Ids = new[] { libre, protegido } }), 2, "Lote mixto no atómico");
        Rechazar(() => EliminacionMovimientosService.EliminarArchivo(archivo));
        ConciliacionExternService.EliminarSesion(sesion.Id);
        EliminacionMovimientosService.EliminarSeleccion(archivo, new[] { libre });
        Igual(cn.QuerySingle<int>("SELECT COUNT(*) FROM bancos.MovimientosArchivo WHERE Id = @Id", new { Id = libre }), 0, "Movimiento libre no eliminado");

        var nuevo = cn.QuerySingle<int>("SELECT TOP 1 Id FROM bancos.MovimientosArchivo WHERE IdArchivo = @Id ORDER BY Id", new { Id = archivo });
        var segundo = cn.QuerySingle<int>("SELECT TOP 1 Id FROM bancos.MovimientosArchivo WHERE IdArchivo = @Id AND Id <> @Otro ORDER BY Id", new { Id = archivo, Otro = nuevo });
        cn.Execute("UPDATE bancos.MovimientosArchivo SET Debitos = @Importe, Creditos = 0 WHERE Id = @Id", new { Importe = 60m, Id = nuevo });
        cn.Execute("UPDATE bancos.MovimientosArchivo SET Debitos = @Importe, Creditos = 0 WHERE Id = @Id", new { Importe = 40m, Id = segundo });
        var grupo = ConciliacionExternService.CrearSesion("Prueba grupo", new List<int> { archivo },
            new[] { "A" }, new[] { new ConciliacionItemExterno { Fecha = new DateTime(2026, 9, 2), Importe = 100m, Detalle = "Grupo" } });
        int externo = ConciliacionExternService.ObtenerTodosItemsExternos(grupo.Id).Single().Id;
        Rechazar(() => ConciliacionExternService.ConciliarSeleccion(grupo.Id, externo, new[] { nuevo }, TipoMatch.Manual));
        Igual(cn.QuerySingle<int>("SELECT COUNT(*) FROM bancos.ConciliacionPares WHERE IdSesion = @Id", new { Id = grupo.Id }), 0, "Diferencia aceptada");
        ConciliacionExternService.ConciliarSeleccion(grupo.Id, externo, new[] { nuevo, segundo }, TipoMatch.Manual);
        var pares = ConciliacionExternService.ObtenerPares(grupo.Id);
        Igual(pares.Count, 2, "Grupo incompleto");
        ConciliacionExternService.DesconciliarPar(pares[0].Id);
        Igual(cn.QuerySingle<int>("SELECT COUNT(*) FROM bancos.ConciliacionPares WHERE IdSesion = @Id", new { Id = grupo.Id }), 0, "Desconciliación parcial");
        ConciliacionExternService.MarcarFinalizada(grupo.Id);
        Rechazar(() => ConciliacionExternService.ConciliarSeleccion(grupo.Id, externo, new[] { nuevo, segundo }, TipoMatch.Manual));
        ConciliacionExternService.EliminarSesion(grupo.Id);
    }

    // Fecha: 08/10/2026 - TAREA: 00076 - Linea: 3, 4 - El archivo importado en otra fecha se elige por sus movimientos reales.
    public static void SelectorYLectura()
    {
        var sugerencia = SelectorExtractosService.SugerirRango(new[]
        {
            new ConciliacionItemExterno { Fecha = new DateTime(2026, 9, 1) },
            new ConciliacionItemExterno { Fecha = new DateTime(2026, 9, 30) },
        });
        if (sugerencia.Desde != new DateTime(2026, 9, 1) || sugerencia.Hasta != new DateTime(2026, 9, 30) || sugerencia.Invalidas != 0)
            throw new Exception("Sugerencia externa incorrecta.");
        var visibles = SelectorExtractosService.Obtener(new DateTime(2026, 9, 30), new DateTime(2026, 9, 30), false);
        if (visibles.Count == 0 || visibles.Any(a => !a.CoincideRango)) throw new Exception("Rango por movimientos incorrecto.");
        var todos = SelectorExtractosService.Obtener(new DateTime(2026, 9, 30), new DateTime(2026, 9, 30), true);
        if (todos.Count < visibles.Count) throw new Exception("Mostrar todos perdió archivos.");
        string ruta = Path.Combine(Path.GetTempPath(), "externo-00076-" + Guid.NewGuid().ToString("N") + ".csv");
        try
        {
            File.WriteAllText(ruta, "Fecha;Importe;Concepto\n02/09/2026;1.234,56;Prueba\n");
            var anterior = CultureInfo.CurrentCulture;
            try
            {
                CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("en-US");
                var leidos = ArchivoExternoReader.Leer(ruta);
                if (leidos.Single().Importe != 1234.56m) throw new Exception("Importe CSV interpretado según cultura de Windows.");
            }
            finally { CultureInfo.CurrentCulture = anterior; }
        }
        finally { File.Delete(ruta); }
    }

    public static void ExcelAgrupado()
    {
        string ruta = Path.Combine(Path.GetTempPath(), "conciliacion-00076-" + Guid.NewGuid().ToString("N") + ".xlsx");
        try
        {
            var pares = new[]
            {
                new ConciliacionPar { IdSesion = 1, IdItemExterno = 1, FechaExterno = new DateTime(2026, 9, 2), ImporteExterno = 100m,
                    DetalleExterno = "Grupo", FechaExtracto = new DateTime(2026, 9, 2), ImporteExtracto = 60m },
                new ConciliacionPar { IdSesion = 1, IdItemExterno = 1, FechaExterno = new DateTime(2026, 9, 2), ImporteExterno = 100m,
                    DetalleExterno = "Grupo", FechaExtracto = new DateTime(2026,9,3), ImporteExtracto = 40m }
            };
            ConciliacionExcelExporter.Exportar(pares, Array.Empty<ConciliacionItemExterno>(), Array.Empty<MovimientoProcesado>(), ruta);
            using var libro = new XLWorkbook(ruta);
            var hoja = libro.Worksheet("Conciliados");
            if (hoja.Cell(2, 2).GetValue<decimal>() != 100m || !hoja.Cell(3, 2).IsEmpty())
                throw new Exception("Importe externo duplicado en grupo.");
            if (hoja.Cell(2, 1).DataType != XLDataType.DateTime || hoja.Cell(2, 5).DataType != XLDataType.Number ||
                hoja.Cell(3, 4).GetDateTime() != new DateTime(2026,9,3) || hoja.Cell(3, 4).Style.DateFormat.Format != "dd/mm/yyyy")
                throw new Exception("Tipos o formato de fecha perdidos en Excel.");
        }
        finally { File.Delete(ruta); }
    }

    // Fecha: 09/10/2026 - TAREA: 00084 - Linea: 1 - El Designer conserva el nombre del campo con prefijo; el smoke debe buscar esos nombres actuales.
    public static void ConstructorSelector()
    {
        using var form = new AgrupadorConceptos.ConciliacionExternForm();
        if (form.Controls.Find("_grillaArchivos", true).Length != 1 ||
            form.Controls.Find("_desdeArchivos", true).Length != 1 ||
            form.Controls.Find("_mostrarTodos", true).Length != 1)
            throw new Exception("Faltan controles del selector amplio.");
        // Fecha: 08/10/2026 - TAREA: 00076 - Linea: 4 - Guardar una captura de la composición del panel sin abrir una base de usuario.
        var panel = form.Controls.Find("pnlConfigNueva", true).Single();
        form.Opacity = 0;
        form.ShowInTaskbar = false;
        form.Show();
        ((Button)form.Controls.Find("btnNuevaSesion", true).Single()).PerformClick();
        panel.Visible = true;
        Application.DoEvents();
        var grilla = (Telerik.WinControls.UI.RadGridView)form.Controls.Find("_grillaArchivos", true).Single();
        var mostrarTodos = (CheckBox)form.Controls.Find("_mostrarTodos", true).Single();
        mostrarTodos.Checked = true;
        Application.DoEvents();
        if (grilla.Rows.Count == 0) throw new Exception("El selector no cargó archivos de prueba.");
        grilla.Rows[0].Cells["Seleccionado"].Value = true;
        Application.DoEvents();
        var desde = (DateTimePicker)form.Controls.Find("_desdeArchivos", true).Single();
        var hasta = (DateTimePicker)form.Controls.Find("_hastaArchivos", true).Single();
        desde.Value = new DateTime(2027, 1, 1);
        hasta.Value = new DateTime(2027, 1, 1);
        mostrarTodos.Checked = false;
        Application.DoEvents();
        var estado = (Label)form.Controls.Find("_estadoArchivos", true).Single();
        if (!estado.Text.Contains("1 seleccionados, 1 fuera del rango"))
            throw new Exception("La selección oculta se perdió: " + estado.Text);
        mostrarTodos.Checked = true;
        Application.DoEvents();
        if (grilla.Rows.Count == 0 || !grilla.Rows.Cast<Telerik.WinControls.UI.GridViewRowInfo>()
                .Any(r => r.Cells["Seleccionado"].Value is true))
            throw new Exception("Mostrar todos no recuperó la casilla seleccionada.");
        using var imagen = new System.Drawing.Bitmap(form.Width, form.Height);
        form.DrawToBitmap(imagen, new System.Drawing.Rectangle(0, 0, imagen.Width, imagen.Height));
        imagen.Save(Path.GetFullPath(".superpowers/sdd/2026-10-07-conciliacion-segura/selector-prueba.png"));
        form.Close();
    }

    public static void PendientesInternos()
    {
        using var cn = SqlDb.GetConnection();
        cn.Open();
        var candidato = cn.QuerySingle<(int Id, int IdArchivo)>(@"SELECT TOP 1 Id, IdArchivo FROM bancos.MovimientosArchivo
            WHERE ConceptoFinal = 'A' AND Fecha = @Fecha", new {Fecha=new DateTime(2026,9,2)});
        var sesion = ConciliacionInternaService.CrearSesion("Interna protegida", new DateTime(2026, 9, 1),
            new DateTime(2026, 9, 30), new[] { "A" });
        Rechazar(() => EliminacionMovimientosService.EliminarSeleccion(candidato.IdArchivo, new[] { candidato.Id }));
        var cierre = ConciliacionInternaService.Finalizar(sesion.Id);
        if (!cierre.Exito) throw new Exception("No se pudo cerrar una sesión interna sin pares.");
        Rechazar(() => EliminacionMovimientosService.EliminarSeleccion(candidato.IdArchivo, new[] { candidato.Id }));
        ConciliacionInternaService.EliminarSesion(sesion.Id);
        Rechazar(() => EliminacionMovimientosService.EliminarSeleccion(candidato.IdArchivo, new[] { -1 }));
        Igual(cn.QuerySingle<int>("SELECT COUNT(*) FROM bancos.MovimientosArchivo WHERE Id = @Id", new { candidato.Id }), 1,
            "Movimiento interno protegido desapareció");
    }

    // Fecha: 08/10/2026 - TAREA: 00076 - Linea: 1 - Dos conexiones coordinadas prueban la relectura tras esperar el bloqueo.
    public static void CarreraCreacionBaja()
    {
        using var cn1 = SqlDb.GetConnection();
        cn1.Open();
        int archivo = cn1.QuerySingle<int>("SELECT TOP 1 Id FROM bancos.ArchivosImportados ORDER BY Id");
        int movimiento = cn1.QuerySingle<int>("SELECT TOP 1 Id FROM bancos.MovimientosArchivo WHERE IdArchivo = @Id", new { Id = archivo });
        using var tx1 = cn1.BeginTransaction();
        int bloqueo = cn1.ExecuteScalar<int>(@"DECLARE @resultado int; EXEC @resultado = sys.sp_getapplock
            @Resource=N'bancos.Conciliacion.Mutacion', @LockMode=N'Exclusive', @LockOwner=N'Transaction', @LockTimeout=15000;
            SELECT @resultado", transaction: tx1);
        if (bloqueo < 0) throw new Exception("No se pudo preparar la carrera.");
        using var iniciada = new ManualResetEventSlim(false);
        var baja = Task.Run(() =>
        {
            iniciada.Set();
            try { EliminacionMovimientosService.EliminarSeleccion(archivo, new[] { movimiento }); return "eliminado"; }
            catch (InvalidOperationException) { return "bloqueado"; }
        });
        if (!iniciada.Wait(TimeSpan.FromSeconds(5))) throw new Exception("La baja no comenzó.");
        if (baja.Wait(TimeSpan.FromMilliseconds(200))) throw new Exception("La baja no esperó el bloqueo común.");
        int sesion = cn1.QuerySingle<int>(@"INSERT INTO bancos.ConciliacionSesiones
            (Nombre, FechaCreacion, IdArchivoImportado, ArchivosJson, ConceptosJson, Estado)
            OUTPUT INSERTED.Id VALUES ('Carrera', @Fecha, @Archivo, NULL, '[]', 'EnProceso')",
            new { Fecha = DateTime.Now, Archivo = archivo }, tx1);
        tx1.Commit();
        if (!baja.Wait(TimeSpan.FromSeconds(20)) || baja.Result != "bloqueado")
            throw new Exception("La baja no revalidó la sesión creada mientras esperaba.");
        Igual(cn1.QuerySingle<int>("SELECT COUNT(*) FROM bancos.MovimientosArchivo WHERE Id = @Id", new { Id = movimiento }), 1,
            "Carrera dejó movimiento borrado en sesión");
        ConciliacionExternService.EliminarSesion(sesion);
    }

    private static void Rechazar(Action accion)
    {
        try { accion(); }
        catch (InvalidOperationException) { return; }
        catch (ArgumentException) { return; }
        throw new Exception("Se aceptó una operación que debía bloquearse.");
    }

    private static void Igual(int actual, int esperado, string mensaje)
    {
        if (actual != esperado) throw new Exception($"{mensaje}: {actual} en vez de {esperado}.");
    }
}
