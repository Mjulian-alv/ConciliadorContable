<!-- Fecha: 05/10/2026 - TAREA: 00064 - Linea: 1 - La migración se aísla para permitir cambios simultáneos en el homologador. -->
# ArcaCliente web primera entrega

El usuario autorizó comenzar en un worktree separado y eligió primero conciliación ARCA y
proveedores. Se implementa el alcance confirmado de la TAREA 00063 con la maqueta como guía.
No se implementan PyR, conexión ARCA online ni envío de comprobantes PRESEA en esta entrega.

## Pedidos numerados

1. Crear el worktree y compartir el backend sin intervenir en el homologador.
2. Implementar la interfaz web de conciliación ARCA y proveedores.
3. Validar, documentar y dejar una versión revisable.

## Arquitectura

- `ArcaCliente.Core` compila por referencia los mismos modelos y servicios actuales, sin
  WinForms. `ArcaCliente` referencia ese proyecto y excluye esas fuentes de su compilación.
  No se duplican algoritmos ni se trasladan archivos que puedan generar conflictos innecesarios.
- `Conciliador.Web` es una aplicación ASP.NET Core/Blazor con interactividad en servidor,
  compatible con los SDK instalados (.NET 10 para el host, .NET 8 para la biblioteca).
- SQL Server sigue siendo la base de negocio. Los perfiles existentes se leen y editan
  en `arca.ArcaPerfilesOffline`; las opciones nuevas se almacenan en tablas complementarias
  del esquema `arca`, sin alterar tablas del homologador.
- El motor `ConciliacionService` actual recibe entradas normalizadas. No se cambian sus
  directivas ni tolerancias. El catálogo existente aporta tipos y descripciones.
- La web valida acceso por usuario, usando el archivo de usuarios SQLite actual del shell
  mediante una ruta de configuración explícita y solo lectura. Cookies y permisos por usuario;
  no utiliza el usuario estático del escritorio ni su archivo de token ARCA.
- Los archivos y resultados viven dentro de la sesión/circuito de cada usuario. Se limitan
  las cargas; no se usan rutas aportadas por el cliente ni carpetas de otros usuarios.

## Decisiones de operación

La ejecución normal exige configuración explícita de SQL y ruta de usuarios. Nunca se conecta
al servidor de producción por el fallback de escritorio. La prueba local utiliza un modo demo
explícito, restringido a entorno Development y dirección de bucle local, con datos sintéticos
y persistencia separada. No se ejecutan migraciones ni escrituras contra la base compartida
durante el desarrollo.

Los cambios locales no registrados de `ArcaCliente/Models/PerfilOffline.cs` y del diseñador
permanecen en el directorio original. La modalidad y lista de CUIT web se modelan como opciones
complementarias, sin sobrescribir ese trabajo. El worktree parte del commit `30e6924`.

## Funciones

- Login, listado y edición de perfiles con modalidades ARCA/proveedor y directivas ordenadas.
- Importación/reemplazo íntegro de la lista CUIT con vista previa, normalización y errores.
- Selección del CUIT antes de abrir el circuito proveedor.
- Carga web de archivo del sistema y archivos CSV de ARCA, o Excel/CSV/TXT de proveedor.
- Configuración por proveedor de encabezado, columnas, formatos y extracción de número.
- Equivalencias confirmadas por CUIT contra catálogo existente, incluidas exclusiones por tipo.
- Previsualización de solo lectura, con filas válidas, excluidas y errores; importación atómica.
- Comparación por magnitud en proveedor, resumen con NC negativa, estados actuales y detalle.
- Exportación Excel de resultados con etiqueta de fuente correcta.

## Persistencia y concurrencia

Los perfiles se guardan individualmente, sin el borrado global del almacenamiento anterior.
Las configuraciones y listas usan control de concurrencia; un guardado obsoleto se rechaza.
Las relaciones complementarias no usan borrado en cascada que pueda destruirlas al guardar
perfiles desde el escritorio. SQL nuevo parametrizado con Dapper en `Data`, transacciones
explícitas en operaciones de varias sentencias e inicialización idempotente desde el host.

Guardar formato ocurre al cambiar opciones válidas; las sugerencias de tipos solo se guardan
cuando se confirman. Un error de persistencia o una edición incompleta no habilita importar.
Toda mutación de datos o configuración invalida resultados anteriores antes de exportar.

## Verificación

Pruebas de entradas inválidas, exclusiones, equivalencias, CUIT, magnitudes, directivas y
concurrencia; comparación de resultados con el motor reutilizado. Compilación del escritorio
y web, recorridos de navegador con archivos sintéticos, aislamiento de sesiones y permisos.
La base inicial de ArcaCliente compila con advertencias preexistentes tras restaurar paquetes.

## Referencias

- [Especificación funcional](2026-10-05-conciliacion-arca-cuits-proveedor-design.md).
- [Maqueta](../../Diseno/00063-conciliaciones/pantallas.html).
- [Autenticación Blazor](https://learn.microsoft.com/en-us/aspnet/core/blazor/security/?view=aspnetcore-10.0).
- [Carga de archivos](https://learn.microsoft.com/en-us/aspnet/core/blazor/file-uploads?view=aspnetcore-10.0).

Se ejecuta en esta sesión por autorización del usuario, sin delegación ni modificación del
directorio donde continúa trabajando en el homologador.
