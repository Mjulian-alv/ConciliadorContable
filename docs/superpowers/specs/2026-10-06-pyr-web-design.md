# PyR web — TAREA 00072

Fecha: 06/10/2026. Pedido: llevar la TAREA 00041 a la web existente, con continuidad indicada por el usuario.

## Alcance

1. Incorporar navegación y autorización ArcaPyR, incluido ingreso de usuarios sin ArcaOffline.
2. Administrar los perfiles SQL existentes de arca.ArcaPerfilesPyR: cuentas, formato del mayor y directivas.
3. Leer toda la carpeta ARCA con el importador actual; cargar un mayor Excel por cuenta desde el navegador.
4. Conciliar y exportar con los servicios actuales, preservando resultados, colores y estructura Excel.

La experiencia se incorpora al mismo host Blazor, identidad y diseño de la web actual. No se cambia
el motor ni se incorpora exportación fiscal a PRESEA. Se incluyen las correcciones de TAREA 00046
(operador y asiento opcional). El período de PyR conserva el funcionamiento actual: se concilia
todo lo cargado, sin imponer el filtro de fechas de comprobantes.

## Pantallas

- Perfiles PyR: lista, alta, edición y eliminación confirmada; nombre, carpeta ARCA, columnas,
  hoja, cabecera, formatos, cuentas con impuesto/operación y directivas ordenadas. Guardado explícito
  de la edición completa para no persistir cuentas/directivas incompletas. Primera directiva fija.
- Percepciones y retenciones: selector de perfil, lectura de carpeta accesible desde el servidor,
  resumen de archivos leídos, repetidos/no reconocidos y vista previa paginada. Un archivo de mayor
  por cuenta, con reemplazo explícito y eliminación; aviso de conceptos no reconocidos y anulaciones.
- Resultado: cinco estados, colores del catálogo compartido, filtros por cuenta/estado/búsqueda,
  ambas fechas, números, importes, operador y directiva; resumen por cuenta, exclusiones y descarga.
  Directivas editables también en conciliación, guardadas en el mismo perfil.

## Datos y reglas

IPyRProfileStore guarda una entidad por transacción con Dapper y SQL parametrizado. Una columna
WebVersion rowversion detecta modificaciones desde web y escritorio. Se conserva el JSON existente.
El guardado masivo de WinForms sigue siendo una limitación de convivencia, sin modificarlo aquí.
Las configuraciones demo son separadas. El espacio PyR es por circuito y no comparte archivos.

Los importadores actuales se adaptan mediante temporales exclusivos por operación, eliminados al
finalizar. Los mayores admiten XLS/XLSX, 20 MB por archivo y límite de contenido XLSX expandido.
La carpeta ARCA mantiene su lectura completa, deduplicación por contenido y avisos del importador.
Sólo participan los grupos de las cuentas cargadas; ceros ARCA y pares anulados conservan tratamiento.
Cambiar archivos, carpeta, directivas o perfil invalida el resultado; antes de conciliar/exportar se
controla la versión persistida. No se exportan resultados obsoletos ni se editan celdas importadas.

## Verificación

Paridad con motor/exportador compartidos; mayor Excel con operador, asiento opcional y anulaciones;
alcance por cuenta y exclusiones; perfiles/versiones; cargas fallidas y sesiones aisladas; permiso
ArcaPyR sin ArcaOffline. Compilación web/escritorio y recorrido en navegador con datos sintéticos.
La prueba de datos reales y la publicación piloto se hacen sin alterar archivos comerciales.
