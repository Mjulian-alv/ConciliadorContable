// Fecha: 13/10/2026 - TAREA: 00083 - Linea: 1 - Los casos operativos usan días tipados; las entradas inválidas se verifican en FechasBancarias.Tests.
// Fecha: 07/10/2026 - TAREA: 00073 - Linea: 1, 2, 3 - Probar SQL, formatos Excel y el disparo manual de la ventana contra una base aislada.
using System.Diagnostics;
using System.Globalization;
using AgrupadorConceptos;
using AgrupadorConceptos.Models;
using AgrupadorConceptos.Services;
using ClosedXML.Excel;
using Conciliador.Comun;
using Dapper;
using Microsoft.Data.SqlClient;
using Telerik.WinControls.UI;

internal static class Program
{
    private static int _pruebas;
    private static readonly DateTime Desde = new(2026, 9, 1);
    private static readonly DateTime Hasta = new(2026, 9, 30);

    [STAThread]
    private static void Main()
    {
        Application.EnableVisualStyles();
        // Fecha: 09/10/2026 - TAREA: 00085 - Linea: 1 - Cargar datos en ambas grillas detecta columnas perdidas aunque el formulario se construya correctamente.
        Probar("Extractos conservan sus datos junto al perfil", ConciliacionSeguraTests.ColumnasExtractoConPerfil);
        // Fecha: 13/10/2026 - TAREA: 00083 - Linea: 1 - La instancia de pruebas se toma de HOMOLOGADOR_TEST_SQL; sin ella rige la instancia ArcaWeb00064 de siempre, que no existe en todas las máquinas.
        using var admin = new SqlConnection(Environment.GetEnvironmentVariable("HOMOLOGADOR_TEST_SQL") ?? @"Server=(localdb)\ArcaWeb00064;Database=master;Integrated Security=true;TrustServerCertificate=true");
        admin.Open();
        string nombre = "HomologadorTest_" + Guid.NewGuid().ToString("N");
        admin.Execute("DECLARE @sql nvarchar(max)='CREATE DATABASE '+QUOTENAME(@Nombre); EXEC(@sql)", new { Nombre = nombre });
        SqlDb.ConnectionString = new SqlConnectionStringBuilder(admin.ConnectionString) { InitialCatalog = nombre }.ConnectionString;
        try
        {
            // Fecha: 07/10/2026 - TAREA: 00076 - Linea: 1 - Fallo controlado antes de cambiar el parseo compartido.
            Probar("Fechas con hora para alcance seguro", ConciliacionSeguraTests.FechasConHora);
            Probar("Filtros inválidos se rechazan antes de consultar", () =>
            {
                Rechazar(() => ConsultaMovimientosService.Consultar(Hasta, Desde, new[] { "A" }));
                Rechazar(() => ConsultaMovimientosService.Consultar(Desde, Hasta, Array.Empty<string>()));
                Rechazar(() => ConsultaMovimientosService.Consultar(Desde, Hasta, new[] { " " }));
            });
            Probar("Auto-inicialización del módulo desde otro host", () =>
                Check(ConsultaMovimientosService.ObtenerConceptos().Count == 0, "Catálogo inicial"));
            Sembrar();
            var filas = new List<MovimientoConsulta>();
            Probar("Varios conceptos, ambos extremos del rango y todos los perfiles", () =>
            {
                filas = ConsultaMovimientosService.Consultar(Desde, Hasta, new[] { "A", "B", "A" });
                Check(filas.Count == 7, $"Se esperaban 7 movimientos; llegaron {filas.Count}.");
                Check(filas.First().Fecha == Desde && filas.Last().Fecha == Hasta, "Extremos inclusivos");
                Check(filas.Select(m => m.NombrePerfil).Distinct().Count() == 2, "Perfiles");
                Check(filas.All(m => m.ConceptoFinal == "A" || m.ConceptoFinal == "B"), "Conceptos");
                Check(filas.Single(m => m.ConceptoOriginal == "Con nulos").Debitos == null, "Importe nulo");
                // Fecha: 07/10/2026 - TAREA: 00074 - Linea: 1 - Cubrir negativos, positivos, cero y nulos en ambas columnas, conservando los datos originales.
                Check(filas.All(m => !m.Debitos.HasValue || m.Debitos >= 0), "Débitos absolutos");
                Check(filas.All(m => !m.Creditos.HasValue || m.Creditos >= 0), "Créditos absolutos");
                Check(filas.Single(m => m.ConceptoOriginal == "Inicio").Creditos == 1234.56m, "Crédito negativo normalizado");
                Check(filas.Single(m => m.ConceptoOriginal == "Fin").Creditos == 1234.56m, "Crédito positivo preservado");
                Check(filas.Single(m => m.ConceptoOriginal == "Guiones").Creditos == 0m, "Cero preservado");
                Check(filas.Single(m => m.ConceptoOriginal == "Con nulos").Creditos == null, "Crédito nulo preservado");
                using var cn = SqlDb.GetConnection();
                Check(cn.QuerySingle<decimal>("SELECT Debitos FROM bancos.MovimientosArchivo WHERE ConceptoOriginal = @Concepto", new { Concepto = "Inicio" }) == -1234567.89m, "Débito original preservado");
                Check(cn.QuerySingle<decimal>("SELECT Creditos FROM bancos.MovimientosArchivo WHERE ConceptoOriginal = @Concepto", new { Concepto = "Inicio" }) == -1234.56m, "Crédito original preservado");
                Check(filas.Single(m => m.ConceptoOriginal == "Día ambiguo").Fecha == new DateTime(2026, 9, 2), "Prioridad día/mes");
            });
            Probar("Concepto único y rango vacío", () =>
            {
                Check(ConsultaMovimientosService.Consultar(Hasta, Hasta, new[] { "B" }).Count == 1, "Día único con hora al cierre");
                Check(ConsultaMovimientosService.Consultar(Desde.AddYears(1), Hasta.AddYears(1), new[] { "A" }).Count == 0, "Sin resultados");
            });
            Probar("Texto de concepto con apóstrofe parametrizado", () =>
                Check(ConsultaMovimientosService.Consultar(Desde, Hasta, new[] { "O'Brien" }).Single().ConceptoFinal == "O'Brien", "Parámetros"));
            Probar("Excel sin IDs, fecha real, importes numéricos y cuenta como texto", () =>
            {
                string ruta = Path.Combine(AppContext.BaseDirectory, "consulta-verificada.xlsx");
                ConsultaMovimientosService.Exportar(filas, ruta);
                using var libro = new XLWorkbook(ruta);
                var hoja = libro.Worksheet("Movimientos");
                Check(hoja.LastColumnUsed().ColumnNumber() == 9, "Nueve columnas sin IDs");
                Check(hoja.LastRowUsed().RowNumber() == filas.Count + 2, "Misma cantidad de filas");
                Check(hoja.Cell(2, 9).GetString() == "Perfil", "Nombre del perfil");
                Check(hoja.Cell(3, 1).DataType == XLDataType.DateTime && hoja.Cell(3, 1).GetDateTime() == Desde, "Fecha real");
                Check(hoja.Cell(3, 1).Style.DateFormat.Format == "dd/mm/yyyy", "Formato de fecha");
                // Fecha: 07/10/2026 - TAREA: 00074 - Linea: 1 - Excel debe recibir los mismos valores absolutos de la consulta, manteniendo celdas numéricas.
                Check(hoja.Cell(3, 4).DataType == XLDataType.Number && hoja.Cell(3, 4).GetDouble() == 1234567.89, "Débito absoluto real");
                Check(hoja.Cell(3, 5).DataType == XLDataType.Number && hoja.Cell(3, 5).GetDouble() == 1234.56, "Crédito absoluto real");
                Check(hoja.Cell(3, 4).Style.NumberFormat.Format == "#,##0.00", "Miles y decimales");
                Check(hoja.Cell(3, 8).DataType == XLDataType.Text && hoja.Cell(3, 8).GetString() == "00123", "Cuenta con ceros iniciales");
                for (int i = 0; i < filas.Count; i++)
                {
                    Check(hoja.Cell(i + 3, 1).GetDateTime() == filas[i].Fecha, "Paridad fecha");
                    Check(hoja.Cell(i + 3, 9).GetString() == filas[i].NombrePerfil, "Paridad perfil");
                    if (filas[i].Debitos == null) Check(hoja.Cell(i + 3, 4).IsEmpty(), "Nulo preservado");
                }
            });
            // Fecha: 08/10/2026 - TAREA: 00076 - Linea: 1, 2 - Ejecutar las mutaciones después de verificar la semilla heredada.
            Probar("Bajas protegidas y grupos externos exactos", ConciliacionSeguraTests.BajasYGrupos);
            Probar("Selector por fechas reales y archivo externo argentino", ConciliacionSeguraTests.SelectorYLectura);
            Probar("Excel externo agrupa y escribe tipos reales", ConciliacionSeguraTests.ExcelAgrupado);
            Probar("Selector amplio se construye", ConciliacionSeguraTests.ConstructorSelector);
            Probar("Pendientes internos protegen ambos estados", ConciliacionSeguraTests.PendientesInternos);
            Probar("Baja espera creación concurrente y revalida", ConciliacionSeguraTests.CarreraCreacionBaja);
            if (Environment.GetEnvironmentVariable("CONCILIACION_SEGURA_ONLY") == "1")
            {
                Console.WriteLine($"{_pruebas} pruebas correctas.");
                return;
            }
            // Fecha: 08/10/2026 - TAREA: 00076 - Linea: 1, 2, 3, 4 - Separar el smoke legado que cuelga del resto de regresiones SQL/Excel.
            if (Environment.GetEnvironmentVariable("CONCILIACION_SKIP_LEGACY_UI") != "1")
            {
                Probar("Ventana consulta sólo por botón e invalida resultados al cambiar filtros", ProbarVentana);
                Probar("Quinto botón accesible y sin superposición", () =>
                {
                    using var form = new GestionHomologacionesForm();
                    var botones = form.Controls.OfType<Button>().OrderBy(b => b.TabIndex).ToArray();
                    Check(botones.Length == 5 && botones[4].Name == "btnConsultarMovimientos", "Quinto botón");
                    Check(botones[3].Right <= botones[4].Left, "Sin superposición");
                });
            }
            Console.WriteLine($"{_pruebas} pruebas correctas.");
        }
        finally
        {
            SqlConnection.ClearAllPools();
            admin.Execute("DECLARE @sql nvarchar(max)='ALTER DATABASE '+QUOTENAME(@Nombre)+' SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE '+QUOTENAME(@Nombre); EXEC(@sql)", new { Nombre = nombre });
        }
    }

    private static void Sembrar()
    {
        using var cn = SqlDb.GetConnection();
        cn.Open();
        using var tx = cn.BeginTransaction();
        var archivos = new List<int>();
        foreach (string perfil in new[] { "Perfil Norte", "Perfil Sur" })
        {
            int id = cn.QuerySingle<int>(@"INSERT bancos.PerfilesBanco(NombreBanco,ColumnaConcepto,EsCodigo,FilaEncabezado,TipoImporte)
                OUTPUT inserted.Id VALUES(@Perfil,'Concepto',0,1,2)", new { Perfil = perfil }, tx);
            archivos.Add(cn.QuerySingle<int>(@"INSERT bancos.ArchivosImportados(IdPerfilBanco,NombreArchivo,Fecha)
                OUTPUT inserted.Id VALUES(@Id,'extracto.xlsx',@Fecha)", new { Id = id, Fecha = Desde }, tx));
        }
        var casos = new[]
        {
            ("01/09/2026", "A", "Inicio"), ("2026-09-30T23:59:59", "B", "Fin"),
            ("15-09-2026", "A", "Guiones"), ("2026-09-20", "B", "ISO"),
            ("02/09/2026 14:30:00", "A", "Día ambiguo"), ("2026-09-22", "B", "Compacta"),
            ("23/09/2026", "A", "Con nulos"), ("31/08/2026", "A", "Antes"),
            ("01/10/2026", "B", "Después"), ("15/09/2026", "C", "Otro concepto"),
            ("15/09/2026", "O'Brien", "Apóstrofe"), ("15/09/2026", "", "Sin concepto")
        };
        for (int i = 0; i < casos.Length; i++)
        {
            var c = casos[i];
            // Fecha: 07/10/2026 - TAREA: 00074 - Linea: 1 - Incluir créditos de ambos signos, cero y nulo para comprobar el absoluto sin perder información.
            cn.Execute(@"INSERT bancos.MovimientosArchivo(IdArchivo,Fecha,ConceptoOriginal,DescripcionOriginal,Debitos,Creditos,ConceptoEstandar,ConceptoFinal,CuentaFinal)
                VALUES(@Archivo,@Fecha,@Original,'Descripción de prueba',@Debitos,@Creditos,'Estándar',@Final,'00123')",
                new { Archivo = archivos[i % 2], Fecha = Conciliador.Comun.Services.FechaBancariaParser.TryParse(c.Item1,out var fecha) ? fecha : throw new Exception("Semilla inválida"), Original = c.Item3,
                    Debitos = c.Item3 == "Con nulos" ? (decimal?)null : -1234567.89m,
                    Creditos = c.Item3 == "Con nulos" ? (decimal?)null : c.Item3 == "Guiones" ? 0m : i % 2 == 0 ? -1234.56m : 1234.56m,
                    Final = c.Item2 }, tx);
        }
        tx.Commit();
    }

    private static void ProbarVentana()
    {
        using var form = new ConceptosListado { Opacity = 0, ShowInTaskbar = false };
        form.Show();
        T Control<T>(string nombre) where T : System.Windows.Forms.Control => (T)form.Controls.Find(nombre, true).Single();
        var lista = Control<CheckedListBox>("clbConceptos");
        var grilla = Control<RadGridView>("dgvMovimientos");
        var exportar = Control<Button>("btnExportar");
        Esperar(() => lista.Items.Count > 0 && lista.Enabled);
        Check(grilla.Rows.Count == 0 && !exportar.Enabled, "Sin consulta inicial");
        Control<DateTimePicker>("dtpDesde").Value = Desde;
        Control<DateTimePicker>("dtpHasta").Value = Hasta;
        lista.SetItemChecked(lista.Items.IndexOf("A"), true);
        lista.SetItemChecked(lista.Items.IndexOf("B"), true);
        Application.DoEvents();
        Check(grilla.Rows.Count == 0, "Sin consulta por cambios de filtros");
        Control<Button>("btnConsultar").PerformClick();
        Esperar(() => exportar.Enabled);
        Check(grilla.Rows.Count == 7 && grilla.ChildRows.Count == 7 && grilla.Columns.Count == 9, $"Resultado completo (filas={grilla.Rows.Count}, hijas={grilla.ChildRows.Count}, columnas={grilla.Columns.Count}, estado={Control<Label>("lblEstado").Text})");
        Check(string.Format(grilla.Columns["Debitos"].FormatInfo, grilla.Columns["Debitos"].FormatString, 1234567.89m) == "1.234.567,89", "Formato argentino");
        lista.SetItemChecked(lista.Items.IndexOf("A"), false);
        Check(grilla.Rows.Count == 0 && !exportar.Enabled, "Resultado invalidado por selección");
        Control<Button>("btnConsultar").PerformClick();
        // Fecha: 07/10/2026 - TAREA: 00074 - Linea: 1 - Informar el estado ante una espera agotada facilita distinguir demora de una consulta vacía.
        Esperar(() => exportar.Enabled, () => $"Estado={Control<Label>("lblEstado").Text}; filas={grilla.Rows.Count}; exportar={exportar.Enabled}");
        Check(grilla.Rows.Count == 3, "Segunda consulta");
        Control<DateTimePicker>("dtpHasta").Value = Hasta.AddDays(-1);
        Check(grilla.Rows.Count == 0 && !exportar.Enabled, "Resultado invalidado por fecha");
        form.Close();
    }

    // Fecha: 07/10/2026 - TAREA: 00074 - Linea: 1 - Conservar el diagnóstico de la espera sin cambiar su límite ni aceptar resultados incompletos.
    private static void Esperar(Func<bool> listo, Func<string> detalle = null)
    {
        var reloj = Stopwatch.StartNew();
        while (!listo())
        {
            if (reloj.Elapsed > TimeSpan.FromSeconds(20)) throw new Exception("Tiempo agotado esperando la ventana. " + detalle?.Invoke());
            Application.DoEvents();
            Thread.Sleep(10);
        }
    }

    private static void Probar(string nombre, Action prueba)
    {
        prueba();
        _pruebas++;
        Console.WriteLine("OK: " + nombre);
    }

    private static void Check(bool condicion, string mensaje)
    {
        if (!condicion) throw new Exception(mensaje);
    }

    private static void Rechazar(Action accion)
    {
        try { accion(); }
        catch (ArgumentException) { return; }
        throw new Exception("Se aceptaron filtros inválidos.");
    }
}
