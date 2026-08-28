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

Rige la convención general de `~/.claude/CLAUDE.md`: pedidos numerados por ítem, comentario
`Fecha: dd/MM/yyyy - TAREA: NNNNN - Linea: N - Descripción` en cada bloque tocado, y anotación
en `~/.claude/TAREAS.md` (correlativo global) y en `docs/Historial.md` (bitácora de este repo).

En este repo: `//` para C#, `--` para el SQL de `Data/SqlSchema.cs`.

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
