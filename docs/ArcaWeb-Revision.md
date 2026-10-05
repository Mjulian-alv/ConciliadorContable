# Revisión de entrega — TAREA 00064

Fecha: 05/10/2026. Rama: `codex/arca-web`.
Revisión independiente sobre `30e6924..136dc81`; correcciones verificadas por regresiones.

## Resultado

23 pruebas correctas con SQL Server LocalDB, incluyendo 6 regresiones que primero fallaron
y luego pasaron. Compilan el módulo ArcaCliente y el shell de escritorio.
Navegador: proveedor, ARCA, lista CUIT, exclusiones, error después de la muestra visible,
Excel, sesiones independientes y tamaño móvil. Sin errores de página.

## Hallazgos corregidos

1. ARCA admite tanto fecha ISO actual como dd/MM/yyyy histórico.
2. Un formato cuyo guardado falla permanece bloqueado incluso después de nuevas cargas.
3. Los tiques factura y sus NC/ND forman parte de las familias fiscales permitidas.
4. FC ambiguo no sugiere una letra fiscal; FC C explícito puede sugerir 011.
5. Cambiar mayúsculas no duplica ni impide corregir equivalencias.
6. El período inicial conserva primer día del mes hasta hoy. Se exigen ambas fechas.

Los puntos 4 y 5, inicialmente menores en la revisión, se trataron como funcionalmente
importantes: afectan la identidad fiscal y la posibilidad de corregir equivalencias.
Sin hallazgos menores diferidos. No se realizó una segunda revisión independiente:
las correcciones se probaron con casos reproducibles y la suite completa.

## Decisiones y límites conservados

- Se ejecutó la verificación equivalente en PowerShell: la evidencia se registró manualmente,
  en lugar de usar auxiliares Unix del flujo de trabajo.
- La entrada web utiliza validación estricta adicional; el motor de emparejamiento sigue siendo el mismo.
  Esto requiere mantener esa adaptación de entrada junto con los formatos admitidos.
- La demostración guarda configuración en memoria. Reiniciarla borra esa configuración.
  El despliegue inicial es de una instancia y reiniciar el host exige nuevo ingreso.
- El guardado masivo del escritorio queda intacto. Evitar editar perfiles simultáneamente
  desde escritorio y web; la concurrencia completa entre ambos requiere migrar ese guardado.
- PDF, XLS binario, fórmulas y listas CUIT multicolumna no están incluidos.
  Se necesita exportar valores a XLSX/CSV/TXT y una columna para la lista.
- No se cambió el algoritmo ni el tratamiento de duplicados del motor existente.
- Certificados, instalación y archivos reales requieren validación en el entorno del cliente.
  Esta entrega es local y revisable; no se habilitó producción.

Guía de ejecución y operación: [ArcaWeb.md](ArcaWeb.md).
