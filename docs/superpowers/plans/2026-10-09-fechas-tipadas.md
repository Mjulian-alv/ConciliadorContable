# Fechas tipadas y migración con Fecha2 — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Resolver los dos ítems de la TAREA 00083: importar y operar fechas tipadas, y convertir la base existente mediante `fecha2` sin perder los textos originales ni las conciliaciones.

**Architecture:** Un parser compartido valida únicamente las entradas. El módulo opera con `DateTime.Date` y SQL `date NOT NULL`; un migrador independiente diagnostica y convierte el esquema anterior en una transacción, sin invocar la inicialización del módulo. La actualización de ejecutables y base se realiza en un corte coordinado después de las pruebas.

**Tech Stack:** C#/.NET 8 para librerías y herramientas, shell WinForms net10.0-windows existente, Telerik 2024.4.1113, SQL Server, Dapper, Microsoft.Data.SqlClient, ExcelDataReader y ClosedXML existentes.

**Spec:** `docs/superpowers/specs/2026-10-09-fechas-tipadas-design.md`, aprobado el 09/10/2026.

## Global Constraints

- `bancos.MovimientosArchivo.Fecha` y `bancos.ConciliacionItemsExternos.Fecha` serán `date NOT NULL`.
- Usar `DateTime` en los modelos .NET existentes, normalizado a `.Date`, y parámetros de fecha tipados al persistir.
- El formato de pantalla y Excel será `dd/MM/yyyy`.
- Las marcas de auditoría conservarán sus tipos con hora.
- Si hay incidencias, se rechazará el archivo antes de crear su cabecera o movimientos.
- La configuración visual que resulte necesaria irá en los archivos Designer.
- `FechaTextoOriginal` es evidencia histórica, nullable para las nuevas filas. No se usará para conciliar ni se mantendrá como una segunda fecha operativa.
- Los IDs, importes, conceptos, archivos y relaciones de conciliación se conservan.
- SQL parametrizado y acceso en `Data/*Storage.cs`; lógica en `Services/`; sin SQL nuevo en forms.
- Cada bloque tocado lleva `Fecha: dd/MM/yyyy - TAREA: 00083 - Linea: 1` o `Linea: 2`, según el ítem. Registrar resultados en ambos registros globales y `docs/Historial.md`.
- Conservar el trabajo local previo de las tareas 00073–00082 y los cambios ajenos de ARCA. No usar `git add .`; no incluir modificaciones previas en commits por accidente.
- No ejecutar el inicializador de la aplicación sobre la base operativa durante el diagnóstico. Los tests usan bases aisladas con nombre único y eliminan sólo las que crearon.

## Review Focus

1. Fecha con offset cerca de medianoche: conservar el día escrito, no convertir a la zona horaria de Windows. Prueba en tarea 1.
2. Incidencia cerca del final de un archivo y error a mitad de la escritura: cero movimientos y cero cabeceras del intento fallido. Pruebas en tarea 3.
3. Cambios en la base entre diagnóstico y aplicación: releer bajo bloqueo y abortar ante diferencias, sin reutilizar un diagnóstico obsoleto. Prueba en tarea 2.
4. Una tabla ya migrada y otra anterior, o nombres residuales de un intento manual: reconocer sólo estados admitidos y no pisar columnas. Pruebas en tarea 2.
5. Un host alternativo abre el módulo sobre una base vieja: informar incompatibilidad antes de cualquier DDL lateral, manteniendo auto-inicialización en base nueva. Prueba en tarea 3.

## Archivos y límites

Rutas relativas a `D:\Sistemas\ConciliadorContable`. El parser vive en `Conciliador.Comun/Services/` para que la importación WinForms y las herramientas net8.0 usen exactamente la misma interpretación, sin dependencias de UI.

- Crear `Conciliador.Comun/Services/FechaBancariaParser.cs`.
- Crear `Tools/FechasBancarias.Tests/{FechasBancarias.Tests.csproj,Program.cs,ParserTests.cs,MigracionTests.cs,ImportacionTests.cs,PresentacionTests.cs}`: ejecutable de pruebas con aserciones y salida distinta de cero si falla, consistente con los runners actuales.
- Crear `Tools/MigradorFechasBancos/{MigradorFechasBancos.csproj,Program.cs}`, `Models/DiagnosticoFechas.cs`, `Services/MigracionFechaService.cs` y `Data/MigracionFechaStorage.cs`.
- Crear `AgrupadorConceptos/Models/IncidenciaFechaImportacion.cs`, `Services/FechasImportacionException.cs`, `Data/ImportacionStorage.cs`.
- Modificar los modelos `MovimientoProcesado`, `ConciliacionItemExterno`, `ConciliacionPar`, `ConciliacionInternaPar`; servicios `ImportacionService`, `ArchivoExternoReader`, `ComparadorConciliacion`, `ComparadorConciliacionInterna`, `SelectorExtractosService`, `ConciliacionExternService`, `ConciliacionInternaService`.
- Modificar `Data/SqlSchema.cs`, `DatabaseHelper.cs`, `MovimientoStorage.cs`, `ArchivoImportadoStorage.cs`, `ConciliacionExternStorage.cs`, `ConsultaMovimientosStorage.cs`, `EliminacionMovimientosStorage.cs`; las consultas de conciliación modificadas se trasladan a los storage correspondientes si todavía viven en servicios.
- Modificar `ProcesadorForm`, `ConciliacionExternForm`, `ConciliacionInternaForm`, `SeleccionCandidatoDialog`, `SeleccionCandidatoInternoDialog`, `Services/ConciliacionExcelExporter.cs` y sus Designer cuando se configure presentación fija. Conservar comportamiento de homologación e importes.
- Actualizar `Tools/Homologador.Tests/Program.cs`, `ConciliacionSeguraTests.cs` y `Tools/MigradorDataBanks/{Program.cs,MigradorDataBanks.csproj}`.
- Crear `docs/FechasBancarias.md`: uso, diagnóstico, aplicación, ensayos, corte y recuperación.

## Tarea 1: interpretación única en la entrada — ítem 1

**Interfaces:** `public static bool FechaBancariaParser.TryParse(object valor, out DateTime fecha)` en namespace `Conciliador.Comun.Services`. En éxito devuelve el día con `DateTimeKind.Unspecified`; en fallo devuelve `false`. No consulta SQL.

- [ ] Crear el runner con selección `--grupo parser|migracion|importacion|presentacion|todos`. El grupo parser no abre SQL; los demás reciben la instancia de pruebas por variable `FECHAS_TEST_SQL` y crean bases temporales.
- [ ] Escribir pruebas de `TryParse`: `03/04/2026` → `2026-04-03`; `3-4-2026` → el mismo día; `2026-04-03` → el mismo día; `29/02/2024` válido; `29/02/2026`, `04/30/2026`, `null`, `DBNull.Value`, blancos y `46000d` inválidos. Ejecutar en `es-AR` y `en-US` con idénticos resultados.
- [ ] Ejecutar `dotnet run --project Tools/FechasBancarias.Tests -- --grupo parser` y comprobar el fallo por contrato aún no implementado.
- [ ] Implementar fechas `d/M/yyyy`, `dd/MM/yyyy`, `d-M-yyyy`, `dd-MM-yyyy`, `yyyy-MM-dd`, y las mismas con hora `H:mm`, `H:mm:ss` o `H:mm:ss.FFFFFFF`. Para ISO, admitir espacio o `T`, segundos fraccionarios y `Z`/offset. Interpretar componentes con cultura invariante; con offset usar el día representado, nunca `LocalDateTime`/`UtcDateTime`.
- [ ] Añadir pruebas de `DateTime` tipado con hora y de `2026-04-03T00:15:00+14:00`, que conserva `2026-04-03`. Rechazar textos de meses, años de dos cifras y números sin tipo fecha; no emplear parseo cultural abierto.
- [ ] Reejecutar grupo parser; exigir todos los casos correctos. Revisar y commitear únicamente este entregable y su comentario de tarea.

## Tarea 2: migrador independiente y verificable — ítem 2

**Interfaces:** `DiagnosticoFechas MigracionFechaService.Diagnosticar(SqlConnection cn)` y `DiagnosticoFechas MigracionFechaService.Aplicar(SqlConnection cn)`. El storage recibe la conexión y la transacción explícitas; no usa `DatabaseHelper`. El diagnóstico contiene por tabla su estado, conteo y lista de incidencias con tabla, ID, texto, fecha intermedia y motivo. Consume el parser de tarea 1.

- [ ] Crear el proyecto net8.0 con referencia a `Conciliador.Comun` y las dependencias SQL ya usadas. CLI: `diagnosticar --config <secrets.config> --salida <informe.json>` y `aplicar --config <secrets.config> --respaldo <referencia> --salida <informe.json>`. Nunca mostrar ni recibir contraseñas como argumentos. Aplicar requiere una referencia de respaldo registrada; eso no sustituye la verificación del respaldo en el corte.
- [ ] Preparar bases anteriores con ambas tablas, archivos, sesiones y pares; sembrar fechas DMY/ISO y `fecha2` parcial, completa e inexistente. Comprobar que diagnóstico no crea objetos ni modifica valores. Antes de implementar, ejecutar el grupo migracion y registrar el fallo esperado.
- [ ] Implementar estados admitidos: ambas columnas `Fecha` textuales (con intermedia ausente o date/smalldatetime); ambas finales `date NOT NULL`, con original histórico opcional sólo en una instalación nueva. Rechazar combinación de tabla vieja/nueva, columnas inesperadas o `FechaTextoOriginal` que colisionaría con un renombrado.
- [ ] Leer metadatos y dependencias de las columnas: índices, restricciones, columnas calculadas, objetos SQL y triggers relevantes. Si hay una dependencia que el migrador no preserva explícitamente, informar y abortar; no eliminarla automáticamente. Identificadores SQL sólo de una lista fija de las dos tablas y columnas; valores parametrizados.
- [ ] Usar el parser compartido sobre el texto; para cada `fecha2` cargada exigir misma fecha y hora cero. Informar todas las filas incompatibles antes de aplicar. Un diagnóstico satisfactorio no permite omitir la segunda validación.
- [ ] En `Aplicar`, iniciar una única transacción, adquirir el mismo bloqueo de mutaciones del módulo y bloquear ambas tablas en orden fijo (`MovimientosArchivo`, `ConciliacionItemsExternos`) para impedir escrituras durante el cambio. Releer metadatos y filas bajo esos bloqueos.
- [ ] Crear la intermedia sólo si falta; convertir su tipo a `date`, completar sólo nulos con parámetros `DbType.Date` y comprobar cada ID. Renombrar `Fecha` a `FechaTextoOriginal`, después `fecha2` a `Fecha`; fijar `NOT NULL`. Ejecutar DDL dependiente en comandos separados para evitar referencias compiladas antes de crear/renombrar columnas.
- [ ] Antes del commit, cotejar por ID todas las fechas y textos contra la lectura inicial, más conteos/IDs de archivos, sesiones y pares. No tocar importes ni conceptos. Rollback de ambas tablas ante cualquier error. En estado final, verificar y devolver éxito sin repetir cambios; distinguir filas nuevas cuyo texto original sea nulo.
- [ ] Probar discrepancia `03/04/2026` frente a `2026-03-04`, fecha intermedia con hora, texto inválido/vacío, externa con filas, schema parcial, dependencia desconocida y repetición. Verificar rollback provocando un fallo en la segunda tabla dentro de una transacción de prueba; comprobar texto, tipos y nombres de ambas tablas.
- [ ] Probar una modificación confirmada después de diagnosticar y antes de aplicar: la nueva lectura la detecta y aborta; una escritura concurrente durante aplicar espera o falla por timeout sin colarse en la conversión.
- [ ] Ejecutar grupo migracion y compilar el migrador. Commits sólo de la herramienta, pruebas y documentación de uso; el ensayo sobre datos reales corresponde a tarea 5.

## Tarea 3: sistema con fechas tipadas e importación atómica — ítem 1

**Interfaces:** `MovimientoProcesado.Fecha`, `ConciliacionItemExterno.Fecha`, `ConciliacionPar.FechaExterno/FechaExtracto` y `ConciliacionInternaPar.FechaA/FechaB` pasan a `DateTime`. `ComparadorConciliacion.FechasIguales(DateTime, DateTime)` y `ComparadorConciliacionInterna.EstaEnRango(DateTime, DateTime, DateTime)` usan `.Date`.

`IncidenciaFechaImportacion` expone `Archivo`, `Hoja`, `Fila`, `ValorOriginal`, `Motivo`. `FechasImportacionException` hereda `InvalidOperationException` y expone `IReadOnlyList<IncidenciaFechaImportacion> Incidencias`. `ImportacionStorage.GuardarArchivo(int idPerfil, string nombre, DateTime fechaImportacion, List<MovimientoProcesado> movimientos, Action<int,int> progreso)` devuelve el ID de archivo y asigna los IDs sólo al confirmar.

- [ ] Escribir pruebas de archivos CSV/Excel con una fecha inválida al inicio y otra al final; verificar números reales de fila, ambas incidencias y ninguna cabecera/movimiento guardado. Probar columna de fecha ausente, Excel tipado y culturas diferentes. Ejecutar grupo importacion y registrar el fallo previo.
- [ ] En los dos lectores conservar el objeto de celda hasta llamar al parser. Validar sólo filas importables; acumular todas las incidencias de fecha antes de persistir. Mantener criterios de concepto/importe actuales, informar explícitamente la columna ausente y no asignar `DateTime.MinValue` para salvar un error.
- [ ] Implementar `ImportacionStorage` y sobrecargas de inserción que reciban conexión/transacción en `ArchivoImportadoStorage` y `MovimientoStorage`. Una transacción contiene cabecera, bloqueo de mutaciones y lote completo. No publicar IDs ni listas como persistidos antes del commit. Probar un error real de escritura después de una fila válida y verificar rollback de cabecera y lote.
- [ ] Cambiar juntos modelos y consumidores de datos para mantener un build coherente. Retirar el parser de los comparadores: fechas iguales son días iguales; no existe fallback textual. La migración conserva el único lugar donde se relee texto histórico.
- [ ] Reemplazar conversiones SQL sobre `m.Fecha` por lecturas y comparaciones tipadas en consulta de movimientos, listado global, pares internos/externos y alcance de conciliación. Extraer sólo las consultas modificadas desde los servicios a `Data/*Storage.cs`; no refactorizar las independientes.
- [ ] Ajustar selector y protección de bajas para fechas tipadas. Los archivos sin movimientos siguen siendo un caso válido con rango vacío; sus fechas agregadas pueden ser `DateTime?`, sin volver nullable la fecha de un movimiento.
- [ ] Crear tablas nuevas con `date NOT NULL`. Detectar esquema anterior antes de ejecutar el resto de `SqlSchema.Ddl`; mostrar «La base requiere la migración de fechas. Ejecute MigradorFechasBancos antes de abrir el módulo.» y no marcarla inicializada. No alterar timestamps de auditoría.
- [ ] Actualizar fixtures operativos a fechas tipadas y llevar los casos inválidos antiguos a parser/migración. No borrar las regresiones existentes de sumas exactas, bajas y concurrencia para hacer pasar los tests.
- [ ] Probar auto-inicialización desde otro host en proceso nuevo, aviso contra esquema viejo sin DDL lateral y lecturas/escrituras Dapper posteriores a la migración. Ejecutar grupo importacion, `dotnet build ConciliadorContable/ConciliadorContable.csproj` y las 13 pruebas seleccionadas por `CONCILIACION_SEGURA_ONLY=1`.
- [ ] Revisar el diff frente a la copia inicial de archivos modificados; commitear sólo los cambios atribuibles a esta tarea, sin incluir trabajo previo pendiente.

## Tarea 4: presentación, exportaciones y migrador SQLite — ítem 1

**Interfaces:** conservar APIs públicas de exportación y formularios; `ConciliacionExcelExporter.EscribirFecha` recibe `DateTime`. El migrador SQLite consume `FechaBancariaParser.TryParse` y entrega columnas DataTable de tipo `DateTime` al bulk copy.

- [ ] Escribir pruebas para grillas y Excel: fecha `2026-04-03` se muestra `03/04/2026`, sin hora; la celda Excel es DateTime y mantiene orden cronológico. Incluir un grupo 1:N para verificar que fecha/importe externos aparecen sólo en la primera fila. Ejecutar grupo presentacion antes del cambio.
- [ ] Ajustar formatos fijos en los Designer. En eventos de formato dependientes de la fila, formatear el `DateTime` recibido sin `ToString()` seguido de reparsing. Conservar selección múltiple, rango, resúmenes e importes. No mostrar `FechaTextoOriginal` ni controles nuevos creados en el constructor.
- [ ] Revisar las exportaciones de conciliación interna/externa, consulta de movimientos y los diálogos de candidatos. Escribir fechas reales, con formato `dd/mm/yyyy` de Excel; retirar mensajes de fecha ilegible que el nuevo contrato ya impide.
- [ ] Actualizar `MigradorDataBanks` con referencia al parser compartido. Prevalidar las fechas de ambas tablas SQLite antes de escribir cualquier tabla destino; convertir el DataTable, conservar IDs y ejecutar la copia en transacción explícita. Excluir una `fecha2` de prueba de los mapeos automáticos. Rechazar una fecha mala sin dejar un destino parcialmente cargado.
- [ ] Probar origen SQLite con DMY, ISO, `fecha2` auxiliar y una fila inválida; verificar conversión tipada, preservación de IDs y cero tablas copiadas ante error. Ejecutar grupo presentacion y `dotnet build Tools/MigradorDataBanks/MigradorDataBanks.csproj`.
- [ ] Revisar una captura del selector y recorridos de conciliación sobre base de pruebas. Registrar comandos/resultados y commitear sólo los cambios propios.

## Tarea 5: ensayo real, entrega y corte — ítems 1 y 2

**Interfaces:** consume los ejecutables verificados, los comandos de tarea 2 y el contrato final de tarea 3. Produce informe reproducible de ensayo y estado separado «sistema preparado» / «base migrada».

- [ ] Crear una copia aislada del schema `bancos` con sus datos actuales por lectura del origen y copia local, preservando tipos, IDs, relaciones y la columna intermedia. Registrar servidor/base de origen sin secretos, fecha de extracción y conteos. No usar el inicializador nuevo para recrear el esquema anterior del ensayo.
- [ ] Diagnosticar y aplicar sobre la copia. Comparar cada ID/texto/fecha y todas las relaciones, incluidas sesiones internas. Los valores observados anteriormente (64.875 filas, 45.992 intermedias cargadas y 18.883 nulas) son referencia, nunca conteos fijados en el código.
- [ ] Ejecutar todos los grupos de `FechasBancarias.Tests`, las regresiones de `Homologador.Tests`, builds de shell y ambas herramientas. Verificar formularios y exportaciones sobre la copia migrada, sin escrituras de prueba en el origen.
- [ ] Completar `docs/FechasBancarias.md` con los comandos exactos, la referencia del ensayo, resultados, ubicación de ejecutables preparados y recuperación. Preparar la versión nueva en una salida separada; no sustituir la instalación operativa antes del corte.
- [ ] Antes de aplicar a la base real, comprobar que las aplicaciones estén cerradas, identificar y verificar la copia de seguridad disponible y presentar el diagnóstico fresco con el resultado del ensayo. Resolver únicamente cualquier dato operativo o autorización de escritura que siga faltando; no repetir aprobaciones de diseño.
- [ ] Con esas condiciones cumplidas, ejecutar una vez el comando aplicar sobre la base identificada y verificar los tipos y datos tras commit. Registrar el resultado; si falla, reportar el rollback y conservar el sistema anterior operativo, sin declarar migración completada.
- [ ] Actualizar los ejecutables del mismo destino y abrir el módulo para una comprobación de lectura. Confirmar rangos y sesiones existentes. No crear conciliaciones o movimientos ficticios en producción.
- [ ] Actualizar historial y registros globales con evidencia y diferencias entre lo probado en copia y lo realizado en destino. Revisar y commitear sólo los cambios propios; no hacer push o mezclar cambios ajenos.

## Revisión del plan y ejecución

Cobertura revisada contra el spec: parser, archivo completo, transacción de importación,
consumidores, UI por Designer, herramientas antiguas, migración preservando `fecha2`,
auto-inicialización y corte coordinado tienen entregables y verificaciones explícitos.

Método recomendado: ejecución nativa en esta sesión, seguida de revisión independiente
al final. Los cambios de tipos atraviesan modelos, consultas y formularios; una ejecución
secuencial facilita mantener esos contratos coherentes. La alternativa es ejecución por
subagentes, con revisión por tarea y mayor coste de contexto. Pendiente revisión del plan
y elección del método por el usuario; todavía no comenzar implementación.
