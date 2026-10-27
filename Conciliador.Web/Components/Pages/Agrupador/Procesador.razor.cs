// Fecha: 14/10/2026 - TAREA: 00097 - Linea: 2 - Lógica del Procesador web: importar (con avance), cargar un archivo leído, homologar una fila o un grupo, editar en el lugar, exportar y dar de baja.
using AgrupadorConceptos.Data;
using AgrupadorConceptos.Models;
using AgrupadorConceptos.Services;
using Conciliador.Web.Security;
using Conciliador.Web.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
namespace Conciliador.Web.Components.Pages.Agrupador;
public partial class Procesador
{
    enum Dialogo { Ninguno, Homologar, ConfirmarRehomologar, Pendientes, PendientesHomologar, Consolidado, BorrarArchivo, EliminarSeleccion, Bloqueo, MarcarConciliados, Desblindar, Historial } // Fecha: 26/10/2026 - TAREA: 00135 - Linea: 2, 3 - Diálogos de blindaje, clave maestra e historial
    static readonly System.Globalization.CultureInfo ar = new("es-AR");
    [Inject] AccessGuard Access { get; set; } = default!;
    [Inject] IJSRuntime JS { get; set; } = default!;
    [Inject] AgrupadorWorkspace Work { get; set; } = default!;
    [SupplyParameterFromQuery(Name = "perfil")] public int? PerfilInicial { get; set; }

    List<PerfilBanco> perfiles = new();
    List<ArchivoImportado> archivos = new();
    int perfilId, archivoSel;
    ArchivoSubido? subido;
    List<MovimientoProcesado> vista = new();
    HashSet<int> seleccion = new();
    MovimientoProcesado? actual;
    string filtro = "", orden = "fecha";
    bool descendente, soloPendientes;
    int homologados;
    decimal sumaDebitos, sumaCreditos;
    Dialogo dialogo;
    string? aviso, error, mensajeBloqueo, valorGrupo, tituloConsolidado;
    bool ocupado, importando, yaHomologado;
    int paso, guardados, total;
    IReadOnlyList<IncidenciaFechaImportacion>? incidencias;
    List<LineaConsolidado> lineasConsolidado = new();
    List<LineaAsiento> lineasAsiento = new(); // Fecha: 23/10/2026 - TAREA: 00126 - Linea: 1 - Hoja Asiento del Excel del consolidado
    int pendientesConsolidado;

    List<MovimientoProcesado>? Movs => Work.Movimientos;
    PerfilBanco? Perfil => perfiles.Find(p => p.Id == perfilId);
    ArchivoImportado? archivoSeleccionado => archivos.Find(a => a.Id == archivoSel);
    // Fecha: 26/10/2026 - TAREA: 00135 - Linea: 3 - El blindaje se mira sobre el archivo cuyos movimientos están en la grilla, no sobre el elegido en la lista
    ArchivoImportado? ArchivoEnTrabajo => Work.ArchivoId == 0 ? null : archivos.Find(a => a.Id == Work.ArchivoId);
    bool Blindado => ArchivoEnTrabajo?.Blindado == true;
    ModificacionesArchivo modBaja = new();
    string tituloBloqueo = "No se puede borrar"; string? notaBloqueo;
    const string NotaBaja = "Los motivos posibles: movimientos tomados por pares de conciliación, o una sesión externa o interna en proceso o finalizada que alcanza el archivo. Un error de base de datos se informa aparte, con su mensaje.";
    static bool EsPendiente(MovimientoProcesado m) => m.ConceptoEstandar == ConceptosBancarios.PendienteHomologar;

    protected override async Task OnInitializedAsync()
    {
        try { perfiles = await Task.Run(() => PerfilBancoStorage.ObtenerTodos()); }
        catch (Exception) { error = "No se pudieron leer los perfiles de banco. Probá de nuevo; si sigue, avisá a sistemas."; return; }
        int deseado = PerfilInicial ?? Work.PerfilId;
        perfilId = perfiles.Any(p => p.Id == deseado) ? deseado : 0;
        if (perfilId != Work.PerfilId) Work.LimpiarArchivo();
        Work.PerfilId = perfilId;
        await CargarArchivos(Work.ArchivoId == 0 ? null : Work.ArchivoId);
        Recalcular();
    }

    async Task CambioPerfil()
    {
        aviso = error = null; incidencias = null; seleccion.Clear(); actual = null;
        Work.PerfilId = perfilId; Work.LimpiarArchivo();
        await CargarArchivos(null);
        Recalcular();
    }

    async Task CargarArchivos(int? seleccionar)
    {
        archivos = perfilId == 0 ? new() : await Task.Run(() => ArchivoImportadoStorage.ObtenerPorPerfil(perfilId));
        archivoSel = seleccionar.HasValue && archivos.Any(a => a.Id == seleccionar.Value) ? seleccionar.Value : archivos.FirstOrDefault()?.Id ?? 0;
    }

    async Task CargarDatos()
    {
        aviso = error = null; incidencias = null;
        var perfil = Perfil; if (perfil == null || archivoSel == 0) return;
        ocupado = true;
        try
        {
            int idArchivo = archivoSel;
            // Vuelve a pasar la homologación sobre los pendientes, por si se agregaron reglas desde la última carga.
            Work.Movimientos = await Task.Run(() => SesionMovimientosService.RehomologarPendientes(idArchivo, perfil));
            Work.ArchivoId = idArchivo; seleccion.Clear(); actual = null;
            Recalcular();
        }
        catch (Exception ex) { error = "No se pudo cargar el archivo: " + ex.Message; }
        finally { ocupado = false; }
    }

    async Task Elegir(InputFileChangeEventArgs e)
    {
        error = null; subido = null; incidencias = null;
        try { subido = await ArchivosWeb.LeerAsync(e.File); }
        catch (Exception ex) { error = ex.Message; }
    }

    async Task Importar()
    {
        var perfil = Perfil;
        if (perfil == null) { error = "Elegí un perfil de banco."; return; }
        if (subido == null) { error = "Elegí un archivo."; return; }
        aviso = error = null; incidencias = null;
        try { await Access.Require("AgrProcesador"); }
        catch (UnauthorizedAccessException ex) { error = ex.Message; return; }
        importando = true; ocupado = true; paso = 0; guardados = total = 0;
        var archivo = subido;
        try
        {
            var movimientos = await Task.Run(() => ImportacionService.ImportarArchivo(archivo.Abrir(), archivo.Nombre, perfil, p =>
            {
                paso = p.Paso; guardados = p.Guardados; total = p.Total;
                // Cada 500 filas (o la última) para no saturar la conexión del circuito.
                if (p.Paso != ProgresoImportacion.PasoGuardando || p.Guardados % 500 == 0 || p.Guardados == p.Total) _ = InvokeAsync(StateHasChanged);
            }));
            paso = 3; StateHasChanged();
            int idArchivo = movimientos.Count > 0 ? movimientos[0].IdArchivo : 0;
            await CargarArchivos(idArchivo);
            Work.Movimientos = movimientos; Work.ArchivoId = idArchivo; seleccion.Clear(); actual = null; subido = null;
            Recalcular();
            aviso = $"Archivo leído: {movimientos.Count.ToString("N0", ar)} movimiento(s) de «{archivo.Nombre}».";
        }
        catch (FechasImportacionException ex) { incidencias = ex.Incidencias; }
        catch (InvalidOperationException ex) { error = "No se pudo importar el archivo: " + ex.Message; }
        catch (Exception ex) { error = "Ocurrió un error al procesar el archivo: " + ex.Message; }
        finally { importando = false; ocupado = false; }
    }

    string Clase(int i) => paso > i ? "done" : paso == i ? "now" : "";
    string Marca(int i) => paso > i ? "✔" : paso == i ? "●" : "○";
    int Porcentaje() => paso switch { 0 => 10, 1 => 30, 2 => total > 0 ? 65 + (int)(20.0 * guardados / total) : 65, _ => 95 };

    // ---- grilla: filtro, orden, selección y totales ----
    void Recalcular()
    {
        var movs = Movs;
        if (movs == null) { vista = new(); homologados = 0; sumaDebitos = sumaCreditos = 0; return; }
        homologados = movs.Count(m => !EsPendiente(m));
        sumaDebitos = movs.Sum(m => Math.Abs(m.Debitos)); sumaCreditos = movs.Sum(m => Math.Abs(m.Creditos));
        IEnumerable<MovimientoProcesado> q = movs;
        if (soloPendientes) q = q.Where(EsPendiente);
        var f = filtro?.Trim();
        if (!string.IsNullOrEmpty(f))
            q = q.Where(m => Contiene(m.ConceptoOriginal, f) || Contiene(m.DescripcionOriginal, f) || Contiene(m.ConceptoEstandar, f) || Contiene(m.ConceptoFinal, f) || Contiene(m.CuentaFinal, f));
        Func<MovimientoProcesado, object?> clave = orden switch
        {
            "concepto" => m => m.ConceptoOriginal, "descripcion" => m => m.DescripcionOriginal, "estandar" => m => m.ConceptoEstandar,
            "debitos" => m => Math.Abs(m.Debitos), "creditos" => m => Math.Abs(m.Creditos), _ => m => m.Fecha
        };
        vista = (descendente ? q.OrderByDescending(clave) : q.OrderBy(clave)).ToList();
    }
    static bool Contiene(string? texto, string f) => texto != null && texto.Contains(f, StringComparison.OrdinalIgnoreCase);
    void Ordenar(string columna) { descendente = orden == columna && !descendente; orden = columna; Recalcular(); }
    string Flecha(string columna) => orden != columna ? "" : descendente ? " ▼" : " ▲";
    string ClaseFila(MovimientoProcesado m) => actual?.Id == m.Id ? "sel" : EsPendiente(m) ? "pend" : "";
    void Marcar(int id, bool marcar) { if (marcar) seleccion.Add(id); else seleccion.Remove(id); }
    void MarcarVisibles(bool marcar) { foreach (var m in vista) { if (marcar) seleccion.Add(m.Id); else seleccion.Remove(m.Id); } }
    // Sólo cuenta lo que se está viendo: como en el escritorio, la baja es sobre lo seleccionado y visible.
    List<int> SeleccionVisible() => vista.Where(m => seleccion.Contains(m.Id)).Select(m => m.Id).ToList();

    // Fecha: 23/10/2026 - TAREA: 00125 - Linea: 4 - Qué se marca: la selección visible si la hay; si no, todo lo que deja pasar el filtro. Lo ya conciliado no cuenta.
    bool HaySeleccionVisible() => vista.Any(m => seleccion.Contains(m.Id));
    List<int> IdsParaConciliar() => (HaySeleccionVisible() ? vista.Where(m => seleccion.Contains(m.Id)) : vista)
        .Where(m => !m.Conciliado).Select(m => m.Id).ToList();

    // ---- edición en el lugar ----
    async Task CambioConceptoFinal(MovimientoProcesado m, ChangeEventArgs e)
    {
        string anterior = m.ConceptoFinal; string nuevo = e.Value?.ToString() ?? "";
        // Fecha: 23/10/2026 - TAREA: 00125 - Linea: 4 - Un movimiento conciliado no se edita (como en el escritorio).
        if (m.Conciliado) { error = "Este movimiento ya está marcado como conciliado y no se puede editar."; StateHasChanged(); return; }
        // Fecha: 26/10/2026 - TAREA: 00135 - Linea: 2 - El historial guarda quién editó
        try { string usuario = await Access.RequireUser("AgrProcesador"); using var aud = AuditoriaContexto.Usar(usuario: usuario); m.ConceptoFinal = nuevo; await Task.Run(() => MovimientoStorage.ActualizarConceptoFinal(m.Id, nuevo)); }
        catch (Exception ex) { m.ConceptoFinal = anterior; error = ex is UnauthorizedAccessException ? ex.Message : "No se pudo guardar el concepto final: " + ex.Message; }
    }
    async Task CambioCuentaFinal(MovimientoProcesado m, ChangeEventArgs e)
    {
        string anterior = m.CuentaFinal; string nuevo = e.Value?.ToString() ?? "";
        // Fecha: 23/10/2026 - TAREA: 00125 - Linea: 4 - Un movimiento conciliado no se edita (como en el escritorio).
        if (m.Conciliado) { error = "Este movimiento ya está marcado como conciliado y no se puede editar."; StateHasChanged(); return; }
        try
        {
            string usuario = await Access.RequireUser("AgrProcesador"); using var aud = AuditoriaContexto.Usar(usuario: usuario);
            // Una cuenta conciliada en una sesión interna en proceso se fija sola al cerrarla: editarla a mano se perdería.
            string? sesion = await Task.Run(() => ConciliacionInternaService.ObtenerSesionEnProcesoDelMovimiento(m.Id));
            if (sesion != null)
            {
                error = $"Este movimiento está conciliado en la sesión interna «{sesion}», todavía en proceso. La cuenta final se va a fijar sola cuando esa conciliación se cierre, así que no se puede editar a mano ahora.";
                m.CuentaFinal = anterior; StateHasChanged(); return;
            }
            m.CuentaFinal = nuevo; await Task.Run(() => MovimientoStorage.ActualizarCuentaFinal(m.Id, nuevo));
        }
        catch (Exception ex) { m.CuentaFinal = anterior; error = ex is UnauthorizedAccessException ? ex.Message : "No se pudo guardar la cuenta final: " + ex.Message; }
    }

    // ---- homologar una fila ----
    void AbrirHomologar()
    {
        aviso = error = null;
        if (Perfil == null || Movs == null) { error = "Primero cargue la sesión de un archivo."; return; }
        if (actual == null) { error = "Seleccioná un movimiento de la grilla que quieras homologar."; return; }
        yaHomologado = !EsPendiente(actual);
        dialogo = yaHomologado ? Dialogo.ConfirmarRehomologar : Dialogo.Homologar;
    }
    void TrasConfirmarRehomologar(bool confirma) => dialogo = confirma ? Dialogo.Homologar : Dialogo.Ninguno;
    async Task TrasHomologar(ResultadoHomologar r)
    {
        dialogo = Dialogo.Ninguno;
        if (!r.Exitosa || Perfil == null || Movs == null) return;
        ocupado = true;
        try
        {
            var perfil = Perfil; var movs = Movs; bool rehomologa = yaHomologado;
            // Fecha: 26/10/2026 - TAREA: 00135 - Linea: 2 - El historial guarda quién homologó
            string usuario = await Access.RequireUser("AgrProcesador"); using var aud = AuditoriaContexto.Usar(usuario: usuario);
            await Task.Run(() =>
            {
                // Sólo en la re-homologación (que el usuario confirmó) se re-resuelven los de la misma regla; en el alta sobre un pendiente alcanza con el barrido de pendientes.
                if (rehomologa) SesionMovimientosService.ReaplicarHomologacion(SesionMovimientosService.MovimientosDeLaMismaRegla(movs, perfil, r.Valor), perfil);
                SesionMovimientosService.RehomologarEnMemoria(movs, perfil);
            });
            aviso = $"Homologación guardada: «{r.Valor}» → «{r.Concepto}».";
            Recalcular();
        }
        catch (Exception ex) { error = "No se pudo aplicar la homologación: " + ex.Message; }
        finally { ocupado = false; }
    }

    // ---- pendientes agrupados (homologación masiva) ----
    List<PendienteAgrupado> GruposPendientes() => (Movs ?? new()).Where(EsPendiente)
        .GroupBy(m => new { m.ConceptoOriginal, m.DescripcionOriginal })
        .Select(g => new PendienteAgrupado(g.Key.ConceptoOriginal, g.Key.DescripcionOriginal, g.Count(), g.Sum(x => Math.Abs(x.Debitos)), g.Sum(x => Math.Abs(x.Creditos))))
        .OrderByDescending(g => g.Cantidad).ToList();
    void HomologarGrupo(PendienteAgrupado g)
    {
        var perfil = Perfil; if (perfil == null) return;
        string valor = perfil.EsCodigo ? g.ConceptoOriginal : g.DescripcionOriginal;
        valorGrupo = string.IsNullOrEmpty(valor) ? g.ConceptoOriginal : valor;
        dialogo = Dialogo.PendientesHomologar;
    }
    async Task TrasHomologarGrupo(ResultadoHomologar r)
    {
        dialogo = Dialogo.Pendientes;
        if (!r.Exitosa || Perfil == null || Movs == null) return;
        var perfil = Perfil; var movs = Movs;
        try { string usuario = await Access.RequireUser("AgrProcesador"); using var aud = AuditoriaContexto.Usar(usuario: usuario); await Task.Run(() => SesionMovimientosService.RehomologarEnMemoria(movs, perfil)); Recalcular(); aviso = $"Homologación guardada: «{r.Valor}» → «{r.Concepto}»."; }
        catch (Exception ex) { error = "No se pudo aplicar la homologación: " + ex.Message; dialogo = Dialogo.Ninguno; }
    }

    // ---- exportaciones ----
    void AbrirConsolidado()
    {
        aviso = error = null;
        if (Movs == null || Movs.Count == 0) { error = "No hay datos para exportar."; return; }
        lineasConsolidado = ConsolidadoExporter.Calcular(Movs);
        lineasAsiento = ConsolidadoExporter.CalcularAsiento(Movs);
        pendientesConsolidado = ConsolidadoExporter.ContarPendientes(Movs);
        tituloConsolidado = $"Consolidado bancario - {DateTime.Now:dd/MM/yyyy HH:mm} - {archivoSeleccionado?.DisplayName}";
        dialogo = Dialogo.Consolidado;
    }
    async Task ExportarConsolidado()
    {
        try
        {
            await Access.Require("AgrProcesador");
            var bytes = await Task.Run(() => ConsolidadoExporter.ExportarBytes(lineasConsolidado, lineasAsiento, tituloConsolidado ?? "Consolidado bancario"));
            await Descargar($"Consolidado_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx", bytes);
        }
        catch (Exception ex) { error = ex is UnauthorizedAccessException ? ex.Message : "No se pudo exportar: " + ex.Message; dialogo = Dialogo.Ninguno; }
    }
    async Task ExportarDetalle()
    {
        aviso = error = null;
        if (Movs == null || vista.Count == 0) { error = "No hay datos para exportar."; return; }
        try
        {
            await Access.Require("AgrProcesador");
            // Si la grilla está filtrada, que quede dicho en el archivo: quien lo recibe tiene que saber que no está viendo el extracto completo.
            string filtrado = vista.Count < Movs.Count ? $" (filtrado: {vista.Count} de {Movs.Count} movimientos)" : "";
            string titulo = $"Detalle del extracto - {DateTime.Now:dd/MM/yyyy HH:mm} - {archivoSeleccionado?.DisplayName}{filtrado}";
            var encabezados = new List<string> { "Fecha", "Concepto original", "Descripción original", "Concepto estándar", "Débitos", "Créditos", "Concepto final", "Cuenta final" };
            var filas = vista.Select(m => new object[] { m.Fecha, m.ConceptoOriginal, m.DescripcionOriginal, m.ConceptoEstandar, Math.Abs(m.Debitos), Math.Abs(m.Creditos), m.ConceptoFinal, m.CuentaFinal }).ToList();
            var bytes = await Task.Run(() => TablaExcelExporter.ExportarBytes(encabezados, filas, titulo, "Detalle"));
            await Descargar($"DetalleExtracto_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx", bytes);
        }
        catch (Exception ex) { error = ex is UnauthorizedAccessException ? ex.Message : "No se pudo exportar: " + ex.Message; }
    }
    async Task Descargar(string nombre, byte[] bytes)
    {
        using var stream = new MemoryStream(bytes); using var referencia = new DotNetStreamReference(stream);
        await JS.InvokeVoidAsync("downloadFile", nombre, referencia);
    }

    // ---- bajas ----
    // Fecha: 26/10/2026 - TAREA: 00135 - Linea: 1 - Antes de pedir la confirmación se cuenta lo modificado en el archivo
    async Task PedirBorrarArchivo()
    {
        aviso = error = null;
        var archivo = archivoSeleccionado; if (archivo == null) return;
        try { modBaja = await Task.Run(() => EliminacionMovimientosStorage.ContarModificaciones(archivo.Id)); dialogo = Dialogo.BorrarArchivo; }
        catch (Exception ex) { error = "No se pudo revisar el archivo antes de darlo de baja: " + ex.Message; }
    }
    async Task TrasBorrarArchivo(bool confirma)
    {
        if (!confirma || archivoSeleccionado == null) { dialogo = Dialogo.Ninguno; return; }
        var archivo = archivoSeleccionado; ocupado = true;
        try
        {
            string usuario = await Access.RequireUser("AgrProcesador"); using var aud = AuditoriaContexto.Usar(usuario: usuario);
            await Task.Run(() => ArchivoImportadoStorage.Eliminar(archivo.Id));
            if (Work.ArchivoId == archivo.Id) { Work.LimpiarArchivo(); actual = null; seleccion.Clear(); }
            await CargarArchivos(null); Recalcular();
            // Fecha: 26/10/2026 - TAREA: 00135 - Linea: 1 - El archivo queda inactivo y sus movimientos archivados dentro de él
            aviso = $"Archivo dado de baja: «{archivo.NombreArchivo}». Sus movimientos quedaron archivados y el historial se conserva.";
            dialogo = Dialogo.Ninguno;
        }
        // La conciliación se informa por InvalidOperationException con su motivo; un error de base de datos no es «está conciliado».
        catch (InvalidOperationException ex) { tituloBloqueo = "No se puede dar de baja"; notaBloqueo = NotaBaja; mensajeBloqueo = ex.Message; dialogo = Dialogo.Bloqueo; }
        catch (UnauthorizedAccessException ex) { error = ex.Message; dialogo = Dialogo.Ninguno; }
        catch (Exception ex) { error = "No se pudo borrar el archivo por un error de la base de datos: " + ex.Message; dialogo = Dialogo.Ninguno; }
        finally { ocupado = false; }
    }
    void PedirEliminarSeleccion() { aviso = error = null; if (SeleccionVisible().Count > 0) dialogo = Dialogo.EliminarSeleccion; }
    async Task TrasEliminarSeleccion(bool confirma)
    {
        if (!confirma || archivoSeleccionado == null) { dialogo = Dialogo.Ninguno; return; }
        var ids = SeleccionVisible(); int idArchivo = Work.ArchivoId; ocupado = true;
        try
        {
            string usuario = await Access.RequireUser("AgrProcesador"); using var aud = AuditoriaContexto.Usar(usuario: usuario);
            await Task.Run(() => EliminacionMovimientosService.EliminarSeleccion(idArchivo, ids));
            Work.Movimientos = await Task.Run(() => MovimientoStorage.ObtenerPorArchivo(idArchivo));
            seleccion.Clear(); actual = null; Recalcular();
            aviso = $"{ids.Count.ToString("N0", ar)} movimiento(s) eliminado(s).";
            dialogo = Dialogo.Ninguno;
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException) { tituloBloqueo = "No se pueden eliminar"; notaBloqueo = NotaBaja; mensajeBloqueo = ex.Message; dialogo = Dialogo.Bloqueo; }
        catch (UnauthorizedAccessException ex) { error = ex.Message; dialogo = Dialogo.Ninguno; }
        catch (Exception ex) { error = "No se pudo eliminar: " + ex.Message; dialogo = Dialogo.Ninguno; }
        finally { ocupado = false; }
    }

    // Fecha: 23/10/2026 - TAREA: 00125 - Linea: 4 - Marcado manual: el storage omite lo que está en cualquier par de conciliación y devuelve el motivo.
    void PedirMarcarConciliados() { aviso = error = null; if (IdsParaConciliar().Count > 0) dialogo = Dialogo.MarcarConciliados; }
    async Task TrasMarcarConciliados(bool confirma)
    {
        if (!confirma || archivoSeleccionado == null) { dialogo = Dialogo.Ninguno; return; }
        var ids = IdsParaConciliar(); int idArchivo = Work.ArchivoId; ocupado = true;
        try
        {
            string usuario = await Access.RequireUser("AgrProcesador"); using var aud = AuditoriaContexto.Usar(usuario: usuario);
            var r = await Task.Run(() => ConciliacionManualService.MarcarConciliados(ids));
            Work.Movimientos = await Task.Run(() => MovimientoStorage.ObtenerPorArchivo(idArchivo));
            seleccion.Clear(); actual = null; Recalcular();
            aviso = $"Se marcaron {r.Marcados.ToString("N0", ar)} movimiento(s) como conciliados.";
            if (r.Omitidos.Count > 0)
                aviso += $" No se marcaron {r.Omitidos.Count.ToString("N0", ar)}: " + string.Join("; ", r.Omitidos.GroupBy(o => o.Motivo).Select(g => $"{g.Count()} {g.Key}")) + ".";
            dialogo = Dialogo.Ninguno;
        }
        // Fecha: 26/10/2026 - TAREA: 00135 - Linea: 3 - Conciliar con el archivo sin blindar se rechaza con su propio título
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException) { tituloBloqueo = "No se puede conciliar"; notaBloqueo = "Todos los archivos que intervienen en una conciliación tienen que estar blindados."; mensajeBloqueo = ex.Message; dialogo = Dialogo.Bloqueo; }
        catch (UnauthorizedAccessException ex) { error = ex.Message; dialogo = Dialogo.Ninguno; }
        catch (Exception ex) { error = "No se pudo marcar como conciliados: " + ex.Message; dialogo = Dialogo.Ninguno; }
        finally { ocupado = false; }
    }

    // ---- blindaje ----
    // Fecha: 26/10/2026 - TAREA: 00135 - Linea: 3 - Blindar, desblindar y clave maestra del archivo en trabajo
    async Task Blindar()
    {
        aviso = error = null;
        var archivo = ArchivoEnTrabajo; if (archivo == null) return;
        ocupado = true;
        try
        {
            string usuario = await Access.RequireUser("AgrProcesador");
            // Sin clave maestra nadie podría desblindar después. Se define en Administración de usuarios (escritorio): el servicio lo avisa.
            bool cambio = await Task.Run(() => BlindajeService.Blindar(archivo.Id, usuario));
            await CargarArchivos(archivo.Id);
            aviso = cambio ? $"Archivo blindado: «{archivo.NombreArchivo}». Ya se puede conciliar; la homologación quedó cerrada." : "El archivo ya estaba blindado.";
        }
        catch (Exception ex) when (ex is InvalidOperationException or UnauthorizedAccessException) { error = ex.Message; }
        catch (Exception ex) { error = "No se pudo blindar el archivo: " + ex.Message; }
        finally { ocupado = false; }
    }
    async Task TrasDesblindar(bool desblindado)
    {
        dialogo = Dialogo.Ninguno;
        if (!desblindado) return;
        await CargarArchivos(Work.ArchivoId);
        aviso = "Archivo desblindado. Ya se puede homologar y editar; para conciliar hay que blindarlo de nuevo.";
    }

}
