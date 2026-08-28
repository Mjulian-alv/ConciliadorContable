# Gestión de homologaciones: baja con impacto, alta/edición y filtro por perfil (diseño)

**TAREA 00003** — el pedido, numerado por ítem (de ahí sale el `Linea:` de cada comentario):

> 1. Al dar de baja una homologación, preguntar qué hacer con los ítems ya homologados:
>    dejarlos pendientes o cambiarlos a otro concepto.
> 2. Alta y edición de homologaciones desde la misma pantalla.
> 3. Que agrupe bien por perfil.
> 4. Que al invocarla filtre por el perfil desde el cual se la llama.

| Ítem | Secciones de este diseño |
|---|---|
| 1 | El problema de la atribución · El diálogo de baja · Qué se toca de cada movimiento |
| 2 | La pantalla (botones `Nueva` / `Editar`) |
| 3 | La pantalla (orden y agrupación) |
| 4 | La pantalla (combo de perfil) · `ProcesadorForm.cs` |

## Contexto

`AgrupadorConceptos/GestionHomologacionesForm.cs` es hoy una pantalla de consulta: una grilla plana con todas las homologaciones de todos los perfiles y un botón "Eliminar Seleccionada" que llama a `HomologacionStorage.Eliminar(id)` — un `DELETE` seco de la fila de `bancos.HomologacionConceptos`.

El problema es que el concepto homologado no vive sólo en la regla. Al importar, `ImportacionService` escribe el resultado del match **como texto** en cada movimiento (`bancos.MovimientosArchivo.ConceptoEstandar` y `ConceptoFinal`). Cuando se borra la regla, esos movimientos quedan mostrando el concepto viejo sin ninguna regla que lo respalde: ni pendientes ni reasignados. Nadie los vuelve a mirar, porque `SesionMovimientosService.RehomologarPendientes` sólo toca los que están en `Pendiente Homologar`.

Además la pantalla no sabe desde qué perfil se la invocó (`ProcesadorForm.btnGestionarHomologaciones_Click` hace `new GestionHomologacionesForm()` sin argumentos) y lista todo mezclado, ordenado por nombre de banco.

Esta es la única pantalla desde la que se pueden borrar homologaciones, así que es donde tiene que resolverse.

**Fuera de alcance (decisión explícita):** el mantenimiento de `bancos.ConceptosEstandar` — renombrar, borrar sin uso, fusionar duplicados. Los conceptos estándar no se dan de baja en este negocio; se crean solos al tipear uno nuevo en `HomologarForm` y ahí quedan. Meter una solapa para administrarlos complicaría la pantalla sin resolver ningún problema real.

## El problema de la atribución

No hay FK entre movimiento y regla. `MovimientosArchivo.ConceptoEstandar` es texto suelto, y en los perfiles de texto libre (`PerfilBanco.EsCodigo == false`) el match es por substring: `HomologacionMatcher.Resolver` recorre el diccionario y gana la primera clave contenida en la descripción. Eso significa que dos reglas distintas pueden producir el mismo `ConceptoEstandar`, y que un mismo movimiento puede ser candidato de varias.

Consecuencia: **no se puede decidir el impacto de una baja comparando textos**. Borrar la regla `"COMIS"` → `Comisiones` y despegar todo lo que diga `Comisiones` rompería lo que la regla `"COMISION MANT"` → `Comisiones` sigue resolviendo bien.

La atribución correcta es preguntarle al matcher qué clave gana. Para eso `HomologacionMatcher` gana un método que devuelve la clave ganadora en vez del valor:

```csharp
/// <summary>Clave del diccionario que resuelve el valor, o null si no hay match.</summary>
public static string ResolverClave(IDictionary<string, string> dic, string valorABuscar, bool esCodigo)
```

`Resolver` pasa a implementarse sobre `ResolverClave` (misma semántica, mismo orden, sin cambio de comportamiento). Con eso:

- **Los movimientos que cubre una regla** = los movimientos del perfil cuya clave ganadora es el `ValorOriginal` de esa regla.
- **El impacto de una baja** = de esos, los que al re-resolver con el diccionario *sin* la regla quedan sin match. Los que otra regla recoge no se despegan: se recalculan y se les escribe el concepto que corresponde.

El orden importa y ya está establecido: `HomologacionStorage.ObtenerDiccionario` ordena por `ValorOriginal DESC` justamente porque con varias claves candidatas gana la primera. Para excluir una regla del cálculo hay que reconstruir el diccionario salteándola, no quitarle la clave después — así el orden se conserva exactamente:

```csharp
public static Dictionary<string, string> ObtenerDiccionario(int idPerfilBanco, int? idHomologacionAExcluir = null)
```

## La pantalla

Sigue siendo una sola ventana con una grilla y botones. Los cambios:

**Combo de perfil arriba.** `cboPerfil` con los perfiles de `PerfilBancoStorage.ObtenerTodos()`, más una entrada centinela `(Todos los perfiles)` — un `PerfilBanco` con `Id = 0` antepuesto a la lista, mismo truco que evita un flag aparte. Al abrirla desde el Procesador viene precargado con el perfil en uso y la grilla ya filtrada.

**Orden y agrupación.** La grilla lista por `ValorOriginal DESC` dentro de cada perfil: el mismo orden que usa el matcher, así lo que se ve en pantalla es la directiva de precedencia real. Con `(Todos los perfiles)` se agrupa por banco con un `GroupDescriptor` de `RadGridView` sobre la columna `Banco`; con un perfil concreto la agrupación se saca.

**Columna `Movimientos`.** Cuántos movimientos del perfil resuelve hoy esa regla, calculado con `ResolverClave` sobre todos los movimientos del perfil. Es el número que después aparece en el diálogo de baja, así que el usuario ve el impacto antes de tocar nada. Con `(Todos los perfiles)` se calcula igual, una lectura de movimientos por perfil: son extractos bancarios, no tablas de millones de filas. Si el volumen resultara molesto, la salida barata es dejar la columna vacía en ese modo — no se rediseña nada.

**Botones:** `Nueva`, `Editar`, `Eliminar`.

- **Nueva** abre `HomologarForm(perfil.Id, "")` — el form ya deja escribir el `ValorOriginal` en `txtOriginal`. Requiere un perfil concreto: con `(Todos los perfiles)` el botón queda deshabilitado.
- **Editar** reapunta la regla a otro concepto. No permite cambiar el `ValorOriginal`: para eso está eliminar + nueva. Es una limitación deliberada — `HomologacionStorage.Guardar` resuelve el "pisar la homologación previa" borrando por `(IdPerfilBanco, ValorOriginal)`, así que editar la clave dejaría la regla vieja viva y duplicada.
- **Eliminar** abre el diálogo de baja descrito abajo.
- `Editar` y `Eliminar` funcionan también con `(Todos los perfiles)`: la fila trae su `IdPerfilBanco`, así que el perfil sobre el que se calcula el impacto sale de la regla seleccionada, no del combo.

`HuboCambios` público, para que el llamador sepa si tiene que refrescar.

## El diálogo de baja

Un form chico nuevo, `BajaHomologacionDialog`. Muestra el resumen del impacto ya calculado:

> La homologación `COMIS` → `Comisiones` del perfil `Galicia` resuelve hoy **142** movimientos.
> **118** quedan sin ninguna regla que los cubra. Los otros **24** los sigue resolviendo otra homologación y no se tocan.

Y dos opciones excluyentes más cancelar:

- **Dejarlos pendientes** — los 118 vuelven a `Pendiente Homologar` y se van a re-resolver solos si más adelante se crea una regla que los agarre.
- **Reasignarlos a** *(combo de conceptos estándar, permite tipear uno nuevo)* — se les escribe ese concepto. El diálogo aclara abajo que **la regla igual se borra**: una importación futura de ese mismo valor va a quedar pendiente.
- **Cancelar** — no se borra nada.

Con impacto cero (la regla no resuelve ningún movimiento) el diálogo se saltea y se pide una confirmación simple.

## Qué se toca de cada movimiento

En las tres operaciones (baja pendientes, baja reasignando, edición) vale la misma regla, que es la que ya aplica `HomologacionMatcher.AplicarA`:

- `ConceptoEstandar` **siempre** se actualiza al valor nuevo.
- `ConceptoFinal` se actualiza **sólo si** estaba pendiente (`ConceptosBancarios.EstaPendiente`) o si coincidía con el `ConceptoEstandar` viejo. Si el usuario lo editó a mano en la grilla del Procesador, queda intacto.

La comparación contra el `ConceptoEstandar` viejo se hace antes de pisarlo.

**Alcance:** todos los archivos del perfil, no sólo el que esté abierto. La regla es del perfil, así que las sesiones históricas también se corrigen.

**Efecto lateral conocido:** el matcher es la fuente de verdad de qué resuelve cada regla, no el texto guardado. Si un movimiento arrastraba un concepto viejo porque las reglas cambiaron desde su importación, la operación se lo recalcula. Es lo correcto — se auto-corrige la deriva — pero conviene tenerlo presente al leer los conteos.

**Riesgo documentado:** `bancos.ConciliacionSesiones.ConceptosJson` guarda los `ConceptoFinal` elegidos al armar una conciliación (`ConciliacionExternService` filtra por esos nombres). Si una baja cambia el `ConceptoFinal` de movimientos que participan de una sesión guardada, esa sesión puede quedar filtrando por un concepto que ya no existe en los datos. No se agrega chequeo en esta etapa; queda anotado.

## Capas

La lógica de impacto y propagación va a un servicio nuevo, no al form — mismo criterio que `SesionMovimientosService` y `HomologacionMatcher`.

### `Services/HomologacionAdminService.cs` (nuevo)

```csharp
/// Movimientos que resuelve una regla, separados por si otra regla los cubriría sin ella.
public class ImpactoHomologacion
{
    public List<MovimientoProcesado> Afectados { get; }          // se quedan sin cobertura
    public List<MovimientoProcesado> CubiertosPorOtraRegla { get; }
}

public static class HomologacionAdminService
{
    /// Cuántos movimientos del perfil resuelve cada ValorOriginal. Para la columna de la grilla.
    public static Dictionary<string, int> ContarUsoPorRegla(PerfilBanco perfil);

    public static ImpactoHomologacion CalcularImpactoBaja(HomologacionListado regla, PerfilBanco perfil);

    /// Borra la regla y deja los afectados pendientes (o con conceptoDestino si no es null),
    /// recalculando además los que otra regla sigue cubriendo. Todo en una transacción.
    public static void AplicarBaja(HomologacionListado regla, PerfilBanco perfil, string conceptoDestino);

    /// Reapunta la regla al concepto y arrastra los movimientos que hoy resuelve. Una transacción.
    public static void Reapuntar(HomologacionListado regla, PerfilBanco perfil, string nombreConcepto);
}
```

El servicio no abre conexiones: lee por `MovimientoStorage`/`HomologacionStorage`, decide en memoria con el matcher, y delega la escritura atómica al storage.

### `Data/HomologacionStorage.cs`

- `ObtenerDiccionario(int idPerfilBanco, int? idHomologacionAExcluir = null)` — sobrecarga descrita arriba.
- `ObtenerListado(int? idPerfilBanco = null)` — filtro opcional; suma `h.IdPerfilBanco` y `h.IdConceptoEstandar` a la proyección; `ORDER BY p.NombreBanco, h.ValorOriginal DESC`.
- `EliminarYActualizarMovimientos(int idHomologacion, IReadOnlyCollection<MovimientoProcesado> movimientos)` — `DELETE` de la regla + updates de los movimientos en una transacción.
- `ReapuntarYActualizarMovimientos(int idHomologacion, string nombreConcepto, IReadOnlyCollection<MovimientoProcesado> movimientos)` — busca o crea el concepto (mismo bloque que ya tiene `Guardar`), `UPDATE bancos.HomologacionConceptos SET IdConceptoEstandar = @Id WHERE Id = @IdHomologacion`, + updates de los movimientos. Una transacción. Conserva el `Id` de la regla.
- `Eliminar(int id)` queda: la usa el camino sin impacto.

El bloque "buscar o crear concepto estándar" se extrae de `Guardar` a un privado `ObtenerOCrearConcepto(cn, tx, nombre)` para que lo usen los dos.

### `Data/MovimientoStorage.cs`

- `ObtenerPorPerfil(int idPerfilBanco)` — `SELECT m.* FROM bancos.MovimientosArchivo m JOIN bancos.ArchivosImportados a ON m.IdArchivo = a.Id WHERE a.IdPerfilBanco = @IdPerfil`.
- Sobrecarga `ActualizarConceptos(IEnumerable<MovimientoProcesado>, IDbConnection, IDbTransaction)` para que `HomologacionStorage` persista dentro de su propia transacción; la pública actual pasa a delegar en ella. Evita duplicar el SQL de `UpdateConceptos`.

### `Models/HomologacionListado.cs`

Suma `IdPerfilBanco`, `IdConceptoEstandar` y `Movimientos` (el conteo de uso, no viene del SQL).

### `HomologarForm.cs`

- Propiedad `BloquearValorOriginal` (default `false`): con `true`, `txtOriginal` queda `ReadOnly`.
- Modo "sólo seleccionar": una propiedad `SoloSeleccionar` que hace que `btnGuardar_Click` valide y devuelva el concepto en `sConcepto` **sin** llamar a `HomologacionStorage.Guardar`. Lo usa `Editar`, para que el guardado de la regla y el arrastre de los movimientos entren en la misma transacción del servicio.
- Valida que `txtOriginal` no quede vacío. Hoy sólo valida el concepto; con el botón `Nueva` abriendo el form en blanco, un `ValorOriginal` vacío entraría a la base y haría match con todo.

### `ProcesadorForm.cs`

```csharp
var perfil = _perfilEnGrilla ?? cboPerfiles.SelectedItem as PerfilBanco;
var frm = new GestionHomologacionesForm(perfil);
frm.ShowDialog();
if (frm.HuboCambios) RefrescarSesionDesdeBase();
```

`RefrescarSesionDesdeBase` relee los movimientos del archivo en grilla y copia `ConceptoEstandar`/`ConceptoFinal` sobre las instancias que la grilla ya tiene bindeadas, macheadas por `Id`; después `RefrescarGrillaConservandoPosicion()` y `ActualizarResumen(movs)`. No se rebindea: mantiene lo que se arregló en `f9f42e0` — el usuario no pierde la fila donde estaba. La copia va a `SesionMovimientosService.RefrescarDesdeBase(List<MovimientoProcesado> movs, int idArchivo)`.

## Verificación

El repo no tiene proyecto de tests, y montarlo excede este cambio. `HomologacionAdminService` queda escrito sin dependencias de UI y con la decisión separada de la escritura, así que es testeable el día que se agregue.

Checklist manual, con un perfil de texto libre (`EsCodigo = false`) que es donde está el riesgo:

1. Dos reglas que produzcan el mismo concepto y se solapen (`COMIS` y `COMISION MANT` → `Comisiones`). Borrar la más general: los movimientos que la otra sigue cubriendo **no** vuelven a pendientes, y el diálogo los reporta en el conteo de "los sigue resolviendo otra".
2. Baja dejando pendientes: los movimientos afectados quedan en `Pendiente Homologar` en los dos campos, en todos los archivos del perfil. Crear una regla nueva que los agarre y verificar que se resuelven solos al cargar la sesión.
3. Baja reasignando: los afectados quedan con el concepto elegido. Importar de nuevo un archivo con ese valor: queda pendiente (la regla ya no está).
4. Movimiento con `ConceptoFinal` editado a mano en la grilla: después de una baja o una edición, ese `ConceptoFinal` sigue como lo dejó el usuario, aunque el `ConceptoEstandar` cambie.
5. Editar una regla: los movimientos que resolvía quedan con el concepto nuevo, el `Id` de la regla no cambia, y no aparece una regla duplicada.
6. Filtro: abrir la pantalla desde el Procesador con un perfil seleccionado y verificar que arranca filtrada en ese perfil; pasar a `(Todos los perfiles)` y ver la grilla agrupada por banco con `Nueva` deshabilitado.
7. Con un perfil `EsCodigo = true`: los conteos de uso coinciden con el match exacto.
8. Al cerrar la pantalla después de un cambio, la grilla del Procesador refleja los conceptos nuevos sin moverse de la fila donde estaba el cursor.
