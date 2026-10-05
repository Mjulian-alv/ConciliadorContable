<!-- Fecha: 05/10/2026 - TAREA: 00063 - Linea: 4 - Se resume el alcance acordado para revisión del cliente sin presentar el desarrollo como realizado. -->
# Nuevas modalidades de conciliación

Resumen funcional para revisión del cliente · 5 de octubre de 2026

Se propone ampliar la conciliación de comprobantes con dos modalidades seleccionables en
los perfiles. Se mantendrán las directivas de comparación y el tratamiento de fechas actuales.

## Conciliación contra ARCA

El perfil permitirá activar un filtro por lista de CUIT. La lista se cargará desde un archivo
y se aplicará tanto a los comprobantes de ARCA como a los del sistema, antes de conciliar.
Cada carga reemplazará la lista anterior. Con el filtro activado será obligatorio contar
con una lista válida y no vacía; con el filtro desactivado se conservará el alcance habitual.

## Conciliación contra el proveedor

Se compararán facturas, notas de crédito y notas de débito informadas en el archivo de
pendientes del proveedor contra los comprobantes del sistema. Al ingresar se solicitará
el CUIT del proveedor, que se asignará a los registros importados y filtrará los del sistema.
El archivo del proveedor no necesitará contener su CUIT.

Se configurarán tipo de comprobante, fecha, importe, punto de venta y número, permitiendo
interpretar el número cuando venga combinado. Habrá un único formato por proveedor,
ajustable al importar y guardado automáticamente para próximas cargas.

## Revisión antes de importar

La previsualización mostrará los valores originales y su interpretación. Solo podrá
modificarse el formato de lectura, nunca los datos del archivo. Las equivalencias de tipos
se sugerirán contra el catálogo existente; el usuario confirmará las nuevas una sola vez.

Pagos, recibos y otros movimientos ajenos al alcance podrán clasificarse para excluirlos.
La decisión se recordará por proveedor y se informará la cantidad excluida. Un tipo desconocido
o un error en los comprobantes que deban importarse bloqueará la carga hasta resolverlo.
No se importarán parcialmente los comprobantes válidos dejando errores sin resolver.

## Resultados y alcance

| Resultado | Significado |
|---|---|
| Conciliado | Los comprobantes se emparejan y sus importes coinciden según las reglas actuales. |
| Diferencia de importe | Los comprobantes se emparejan, pero sus importes difieren. |
| Solo sistema | El comprobante aparece únicamente en el sistema. |
| Solo proveedor | El comprobante aparece únicamente en el archivo del proveedor. |

Se comparará la magnitud del importe informado con la del sistema, sin determinar si es
el total original o un saldo pendiente. En los resúmenes, facturas y notas de débito sumarán
y notas de crédito restarán. El importe original permanecerá visible en la previsualización.

En modalidad proveedor se podrán exportar los resultados de la conciliación. No se enviarán
comprobantes a PRESEA ni se calcularán pagos, cancelaciones o deuda. La modalidad ARCA
conservará sus opciones de exportación actuales.

Este documento describe el alcance propuesto para validar. La implementación aún no se realizó.
