# ArcaCliente web — TAREA 00064

Primera entrega: conciliación offline contra ARCA y cuenta corriente de proveedor.
No incluye percepciones/retenciones, conexión online ni exportación fiscal a PRESEA.

## Ejecución local

Requiere .NET SDK 10 (Core compartido apunta a .NET 8).
Desde la raíz del worktree:

```powershell
dotnet restore Conciliador.Web.slnx --configfile Conciliador.Web/NuGet.Config
dotnet run --project Conciliador.Web --launch-profile Demo
```

Abrir http://127.0.0.1:5084/account/login y elegir **Entrar a la demostración**.
Seleccionar un perfil; para proveedor, usar CUIT ficticio **30-12345678-1**.
**Cargar ejemplo** carga archivos sintéticos por el importador real.
Confirmar FA→001, NCA→003, NDA→002; REC y PAGO→Excluir.
Confirmar que el archivo contiene pendientes, y conciliar.

Resultado esperado: 4 conciliados, 1 diferencia, 1 solo proveedor/ARCA y 1 solo sistema.
Resumen con signo: fuente 261.500, sistema 276.400, diferencia -14.900.
La demostración conserva perfiles y formatos en memoria hasta reiniciar; no abre SQL.
Sólo se admite en Development y desde loopback. Los archivos y resultados no se comparten entre circuitos.

## Entorno con datos reales

El host requiere configuración explícita. No toma la conexión por defecto de `SqlDb`.
Establecer mediante secretos del host o variables de entorno:

- `ConnectionStrings__Arca`: conexión SQL Server a la misma base del módulo actual.
- `Web__UsersDatabasePath`: ruta absoluta a `conciliador.db` del shell, accesible en modo lectura.
- `Web__Demo=false` y `ASPNETCORE_ENVIRONMENT=Production`.
- Configurar HTTPS con certificado válido y una URL del host. Las cookies reales requieren HTTPS.

Arrancar con `dotnet run --project Conciliador.Web --launch-profile Configurado`,
o publicar con `dotnet publish Conciliador.Web -c Release` y hospedar ASP.NET Core.

El arranque auto-inicializa el esquema actual y las tablas complementarias:
`arca.WebPerfiles`, `arca.WebPerfilCuits`, `arca.WebFormatosProveedor`.
Agrega `WebVersion rowversion` a `arca.ArcaPerfilesOffline`.
La identidad de SQL necesita permisos de DDL para el primer arranque y lectura/escritura
sobre la configuración. Las migraciones se ejecutan en transacción y con bloqueo de aplicación.

Los usuarios se validan con el mecanismo de contraseña del shell, sin alterar credenciales.
Permisos: `ArcaOffline` para conciliar y configurar columnas del proveedor,
`ArcaPerfiles` para perfiles/formato del sistema/listas/directivas,
`ArcaEquivalencias` para equivalencias y exclusiones. Admin conserva acceso total.
Las acciones verifican permisos actuales; sesión máxima de ocho horas.
Cerrar sesión revoca los circuitos abiertos de ese ingreso. Reiniciar el host exige volver a ingresar.
Esta primera entrega está preparada para **una instancia del servidor web**, sin balanceador.

### Conexión local al escritorio — TAREA 00065, 05/10/2026

La instancia local se conectó a la misma base SQL Server que el escritorio ejecutado desde
`Y:\ConciliadorContable`. Reutiliza `ConciliadorContable.dll.config` y su archivo externo,
sin guardar contraseñas en el repositorio. La base `conciliador.db` de esa carpeta se abre
únicamente para leer usuarios. El perfil SQL existente se llama **PRESEA**.

```powershell
./Tools/Start-ArcaWeb.ps1 -DesktopDirectory 'Y:\ConciliadorContable'
```

Requiere restauración de paquetes previa y certificado HTTPS local confiable. El lanzador
publica Release en `Conciliador.Web/bin/LocalHost` y ejecuta esa salida con su directorio de
contenido, para servir también Blazor, los módulos JavaScript y los estilos generados.
Antes de volver a ejecutarlo, detener su instancia anterior. Acceso:
**https://localhost:5085/account/login**, con las credenciales habituales del escritorio.
Escucha sólo en localhost; no es un despliegue para otras estaciones.

El archivo PRESEA de septiembre usa Latin-1 aunque el perfil indica UTF-8. Para el archivo
del sistema, si la decodificación UTF-8 estricta falla, se lee como Latin-1 y se muestra un
aviso para revisar nombres. No se modifica la codificación ni las columnas del perfil.
Los archivos del proveedor mantienen su validación de codificación configurada.

El usuario confirmó que los dos registros PRESEA con código AFIP `0` son no fiscales.
Se guardó `000` en `ExcludedSystemTypes` dentro de `arca.WebPerfiles` del perfil PRESEA.
La lista es específica del perfil; un código desconocido sin exclusión sigue bloqueando.
No se modificó el registro original de `arca.ArcaPerfilesOffline`, verificado antes/después
del guardado de la exclusión. El arranque agregó las tablas y rowversion descritos arriba.

Validación de septiembre: PRESEA 3.364 filas (3.362 válidas y 2 excluidas); ARCA 4.448
(4.412 válidas y 36 excluidas). Fechas, CUIT, tipo, punto, número e importes PRESEA cotejados
fila por fila con `LocalFileImporter` de WinForms. Con las directivas SQL existentes:
3.216 conciliados, 49 diferencias, 1.147 sólo ARCA y 43 sólo sistema.
24 pruebas automatizadas correctas sin LocalDB. Login HTTPS comprobado en navegador;
el ingreso con contraseña real y la carga visual de estos archivos quedan para el usuario.
Los archivos comerciales no se incorporaron al repositorio.

Corrección del arranque local (misma tarea): ejecutar `dotnet run` bajo Production devolvía
500 para Blazor, CSS aislado y el módulo de reconexión; el login por sí solo sí respondía.
Con la salida publicada, login y los cinco recursos de pantalla comprobados responden 200.
Formulario de ingreso abierto y revisado visualmente en el navegador integrado.

### Colores y planilla del escritorio — TAREA 00066, 06/10/2026

Tarjetas y filas del resultado comparten `EstadoConciliacionColores` con WinForms: verde,
amarillo, rosado y lavanda para los cuatro estados. La descarga utiliza directamente el
exportador original de WinForms: una hoja, 14 columnas, detalle de directivas y resumen
al pie, con sus mismos estilos. Se retiró la planilla web abreviada de dos hojas.
En el circuito de proveedor se adaptan las etiquetas ARCA a Proveedor.
El resumen por CUIT sigue disponible en pantalla.

26 pruebas correctas, incluyendo comparación celda/estilo contra WinForms y proveedor.
El 06/10 se recuperó el acceso a Y luego de una interrupción temporal. La salida Release
está iniciada con la configuración del escritorio; login HTTPS verificado en navegador
en https://localhost:5085/account/login y recursos web con HTTP 200.

## Reglas implementadas

- Perfiles existentes se leen como ARCA sin filtro CUIT, conservando sus directivas.
- Lista CUIT de una columna, cabecera CUIT opcional, normalización de guiones/espacios,
  11 dígitos, deduplicación. La vista previa se confirma antes de reemplazar la lista completa.
  Un error conserva la lista persistida. No se comprueba el dígito verificador.
- Modo proveedor solicita CUIT antes de la pantalla. Lo asigna a sus filas y filtra el sistema.
  El operador confirma que son pendientes de pago; no se infiere saldo ni importe original.
- Un formato por CUIT, compartido entre perfiles: XLSX, CSV y TXT delimitado.
  Separador, codificación, hoja, primera fila, cabecera/posiciones, fecha, decimal y columnas.
  Punto y número separados, combinados por delimitador o por longitud del punto.
- Los cambios válidos de formato se guardan automáticamente. Las sugerencias de equivalencia
  requieren confirmación. Un tipo sin resolver bloquea el lote; las exclusiones se recuerdan.
- Sólo FC, NC y ND del catálogo existente. Los recibos/pagos excluidos no necesitan campos fiscales.
  Las filas fiscales inválidas no se pueden omitir individualmente.
- Validación completa de todas las filas antes de conciliar. No hay edición de valores ni importación parcial.
  Vista previa paginada con fila original y errores. El número original queda accesible en el detalle.
- Importes comparados como magnitud con dos decimales. El resumen usa FC/ND positivo y NC negativo.
  No hay conversiones de moneda ni inferencias sobre pagos.
- Se reutiliza `ConciliacionService`, con las directivas del perfil en el mismo orden.
  Período obligatorio, inicialmente desde la fecha mínima hasta la máxima del archivo del sistema.
  Acota ambos archivos; la fecha sólo participa del emparejamiento si está en la directiva.
- Estados: conciliado, diferencia de importe, solo ARCA/proveedor, solo sistema.
  Se muestra directiva y clave utilizada. Resumen por CUIT y Excel con el conjunto completo de resultados.
- Cada carga/cambio invalida resultados. Se verifica la versión del perfil y del formato antes de conciliar
  o exportar. Un conflicto exige recargar: nunca sobrescribe automáticamente otra versión.

Límites: 20 MB, 50.000 filas y 256 columnas por archivo.
La carpeta ARCA reúne todos los CSV del directorio, sin límite agregado de diez archivos o 50.000 filas;
100 MB de contenido XLSX expandido. No se admiten PDF, XLS binario, TXT de ancho fijo ni fórmulas Excel.
Exportar las fórmulas como valores. Fechas y separadores deben coincidir con la configuración.
El sistema utiliza códigos fiscales del catálogo; las equivalencias de texto del proveedor se guardan por CUIT.
Los archivos y resultados son transitorios: al recargar/cerrar la página se pierde el trabajo sin exportar.

## Compatibilidad y aislamiento

### Carpeta ARCA y período del sistema — TAREA 00067, 06/10/2026

En perfiles ARCA, usar **Cargar / actualizar carpeta ARCA**. La ubicación inicial proviene
de `CarpetaCsvArca` del perfil del escritorio; si está vacía, del parámetro `ArcaDirectory`
del lanzador (por defecto `Y:\ArcaCliente\ARCHIVOSARCA`). La ruta se puede ajustar para la sesión.
Se leen todos los CSV del directorio, sin subcarpetas ni deduplicación automática, como en WinForms.
Proveedor y sistema siguen recibiendo un único archivo cada uno.

Las fechas quedan vacías hasta importar correctamente el sistema. Se toma la mínima y la
máxima de sus comprobantes incluidos; pueden ajustarse manualmente. Actualizar ARCA no las
reemplaza. Cambiar el sistema o corregir su formato vuelve a calcularlas. Una carga fallida
descarta entradas anteriores y evita conciliar con resultados obsoletos.

ARCA conserva el mapeo por posición del escritorio cuando hay columnas vacías o sobrantes.
Los valores originales no se editan; diferencias de columnas con contenido generan un aviso
para revisión. Los campos fiscales mantienen sus validaciones. Proveedor y sistema continúan
con validación estricta de cabeceras y cantidad de columnas.

Verificación: 29 pruebas correctas, incluida lectura real de 8 CSV (75.595 filas) y el archivo
PRESEA de septiembre. Su rango efectivo es 04/05/2026 a 30/09/2026; 53.744 comprobantes ARCA
quedan fuera del filtro. Recorrido y fechas derivados verificados también en navegador con
datos de ejemplo; captura en `docs/Diseno/00067-carpeta-arca/ejemplo.jpg`.

`ArcaCliente.Core` compila las mismas fuentes de Models/Services/AppServices del módulo actual.
El proyecto WinForms referencia esa biblioteca; no se duplicó ni reescribió el algoritmo.
La web no utiliza los usuarios, tokens o clientes API estáticos del escritorio.
`Conciliador.Web.slnx` es independiente; no se modificó AgrupadorConceptos.

La web actualiza perfiles individualmente y comprueba versiones, incluso ante cambios de escritorio.
**El guardado masivo del escritorio sigue siendo el anterior**: una estación de escritorio con
una lista antigua puede sobrescribir campos base de otros perfiles. Evitar editar perfiles simultáneamente
desde escritorio y web hasta migrar ese guardado. La configuración complementaria de CUIT/proveedor
está fuera de ese guardado masivo. Los modos nuevos se operan desde la web.

El worktree parte de `30e6924`. Los cambios locales no confirmados del usuario en homologador
y en `PerfilOffline`/`FormPerfilOfflineDetalle` no se copiaron ni se modificaron.
La rama queda separada para revisar e integrar esos cambios posteriormente.

## Verificación reproducible

```powershell
dotnet run --project Tools/ArcaWeb.Tests
# Prueba SQL opcional, exclusivamente sobre la instancia local dedicada:
SqlLocalDB create ArcaWeb00064 -s
$env:ARCA_WEB_TEST_SQL='1'
dotnet run --project Tools/ArcaWeb.Tests
dotnet build ArcaCliente/ArcaCliente.csproj
dotnet build ConciliadorContable/ConciliadorContable.csproj
```

La prueba SQL crea una base aleatoria `ArcaWebTest_...` y la elimina al terminar;
nunca usa la conexión configurada de la aplicación.
23 pruebas de servicios/integración verificadas: motor y directivas, CSV/XLSX, validación completa,
tipos desconocidos, exclusiones, CUIT, exportación, versiones, sesiones y autenticación. Las regresiones de revisión cubren fechas ISO, período,
  tiques FC/NC/ND, ambigüedad de FC, mayúsculas y guardados fallidos.
SQL verificado con LocalDB 17 en Windows; no con la instalación del cliente.

`Tools/ArcaWeb.Tests/browser-smoke.cjs` usa Playwright con el servidor Demo en 5084.
Definir `PLAYWRIGHT_PATH` (módulo instalado) y, si corresponde, `EDGE_PATH` (ejecutable).
Ejecutar con Node. Verifica proveedor, ARCA, Excel, error después de la muestra, lista CUIT,
sesión independiente y pantalla móvil. Las capturas se guardan bajo `.superpowers/` (ignorado).

Referencias de arquitectura:
[Seguridad de Blazor](https://learn.microsoft.com/en-us/aspnet/core/blazor/security/?view=aspnetcore-10.0)
y [cargas de archivos](https://learn.microsoft.com/en-us/aspnet/core/blazor/file-uploads?view=aspnetcore-10.0).
