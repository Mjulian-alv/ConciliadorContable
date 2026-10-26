using Conciliador.Comun;
using Dapper;
using Microsoft.Data.SqlClient;

namespace AgrupadorConceptos.Data
{
    public static class DatabaseHelper
    {
        private static bool _inicializada;

        /// <summary>
        /// Conexión a la base común Conciliador (schema bancos).
        /// Mismo contrato que antes: se devuelve cerrada.
        /// Garantiza la inicialización del schema (idempotente), así cualquier
        /// punto de entrada al módulo queda cubierto sin depender de que cada
        /// form nuevo recuerde llamar a InitializeDatabase() explícitamente.
        /// </summary>
        public static SqlConnection GetConnection()
        {
            InitializeDatabase();
            return SqlDb.GetConnection();
        }

        /// <summary>
        /// Igual que <see cref="GetConnection"/> pero ya abierta. Es lo que necesitan
        /// los *Storage en casi todos sus métodos; existe para no repetir el
        /// GetConnection()+Open() en cada uno.
        /// </summary>
        public static SqlConnection Open()
        {
            var cn = GetConnection();
            cn.Open();
            // Fecha: 26/10/2026 - TAREA: 00135 - Linea: 2 - El historial de MovimientosArchivo lee de SESSION_CONTEXT quién y qué proceso escribe
            // Se informa en cada apertura porque el pool de conexiones limpia el contexto al reutilizarla.
            AplicarAuditoria(cn);
            return cn;
        }

        // Fecha: 26/10/2026 - TAREA: 00135 - Linea: 2 - Sin origen ni usuario no se hace el viaje: el trigger asume 'Desconocido' y el login de SQL.
        private static void AplicarAuditoria(SqlConnection cn)
        {
            string origen = AuditoriaContexto.Origen, usuario = AuditoriaContexto.Usuario;
            if (origen != null) cn.Execute("EXEC sp_set_session_context @k, @v", new { k = "origen", v = origen });
            if (usuario != null) cn.Execute("EXEC sp_set_session_context @k, @v", new { k = "usuario", v = usuario });
        }

        /// <summary>
        /// Crea schema/tablas/índices si no existen. Run-once por proceso:
        /// idempotente gracias al flag _inicializada.
        /// </summary>
        public static void InitializeDatabase()
        {
            if (_inicializada) return;
            using var cn = SqlDb.GetConnection();
            cn.Open();
            // Fecha: 13/10/2026 - TAREA: 00083 - Linea: 1 - Una base anterior requiere corte explícito antes de cualquier DDL lateral.
            if (cn.ExecuteScalar<int>(@"SELECT COUNT(*) FROM sys.tables t
                WHERE t.object_id IN (OBJECT_ID('bancos.MovimientosArchivo'),OBJECT_ID('bancos.ConciliacionItemsExternos'))
                AND NOT EXISTS(SELECT 1 FROM sys.columns c WHERE c.object_id=t.object_id AND c.name='Fecha'
                    AND TYPE_NAME(c.system_type_id)='date' AND c.is_nullable=0)") > 0)
                throw new System.InvalidOperationException("La base requiere la migración de fechas. Ejecute MigradorFechasBancos antes de abrir el módulo.");
            try
            {
                cn.Execute(SqlSchema.Ddl);
                // Fecha: 26/10/2026 - TAREA: 00135 - Linea: 2 - CREATE TRIGGER no puede ir dentro del lote del esquema
                if (cn.ExecuteScalar<int>("SELECT COUNT(*) FROM sys.triggers WHERE name = N'TR_MovimientosArchivo_Historial'") == 0)
                    cn.Execute(SqlSchema.TriggerHistorial);
            }
            catch (SqlException ex) when (ex.Number == 2714)
            {
                // Ventana de carrera entre el chequeo IF OBJECT_ID(...) IS NULL y el
                // CREATE TABLE: dos instalaciones abriendo el módulo por primera vez
                // casi simultáneamente contra una base recién provisionada pueden
                // pasar ambas el chequeo antes de que la otra confirme el CREATE.
                // El objeto ya existe (creado por el proceso concurrente) => no-op.
            }
            _inicializada = true;
        }
    }
}
