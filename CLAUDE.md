# ConciliadorContable

Solución .NET 8 (WinForms, Telerik UI) sobre SQL Server. `ConciliadorContable` es el shell con
login; los módulos son librerías que el shell instancia: `AgrupadorConceptos` (schema `bancos`
— importación de extractos, homologación de conceptos, conciliación), `ArcaCliente` (schema
`arca` — comprobantes y exportación a PRESEA), `LiquidacionesAuditar`, y `Conciliador.Comun`
con el acceso a datos compartido. Acceso a datos con Dapper; el esquema se crea desde
`Data/SqlSchema.cs` de cada módulo.

Cada módulo se auto-inicializa: cualquier host que referencie la DLL tiene que poder abrir sus
forms sin pasos de inicialización especiales.

## Versionado de cambios

Todo cambio acordado queda trazable hasta el pedido que lo originó. Tres reglas:

**1. Los pedidos vienen numerados por ítem.** Si el usuario manda un pedido sin numerar, hay
que pedírselo antes de planificar: el número de ítem es parte del comentario y no se puede
inventar después.

**2. Cada bloque agregado o modificado lleva su comentario.**

```
Fecha: dd/MM/yyyy - TAREA: NNNNN - Linea: N - Descripción
```

- `TAREA`: correlativo de 5 dígitos **compartido por todos los proyectos**, no sólo por éste.
- `Linea`: el número de ítem del pedido que originó ese bloque.
- Si el cambio es complejo, debajo va una descripción técnica en el mismo comentario.

`//` en C#, `--` en SQL. En un archivo nuevo el comentario va arriba de todo; en una edición,
inmediatamente encima del bloque que cambia.

**3. Se anota en los dos registros.**

| Dónde | Qué |
|---|---|
| `~/.claude/TAREAS.md` | Fuera de git, compartido entre proyectos. Es la **fuente de verdad del correlativo**: se lee antes de empezar y se escribe al terminar. |
| `docs/Historial.md` | Versionado en este repo. El pedido textual y los cambios ejecutados, tarea por tarea. |

El compromiso es de las dos partes: un cambio hecho a mano, sin pasar por Claude, también se
anota en los dos lados. Si no, el correlativo miente y el historial deja de servir.

## Diseños y planes

Los specs de brainstorming van a `docs/superpowers/specs/YYYY-MM-DD-<tema>-design.md` y los
planes de implementación a `docs/superpowers/plans/`. Ambos se commitean.

## Convenciones de código

- Comentarios y mensajes de UI en español. Los comentarios explican **por qué**, no qué hace
  la línea: el estilo del repo documenta la decisión y el bug que la motivó.
- SQL con Dapper, siempre parametrizado (`@Param`), nunca interpolado.
- Las operaciones de varias sentencias van en una transacción explícita.
- El acceso a datos vive en `Data/*Storage.cs`; la lógica de negocio en `Services/`; los forms
  no tienen SQL.
