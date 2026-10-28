using System;
using System.Data;
using System.IO;
using Microsoft.Data.Sqlite;

namespace ConciliadorContable.Data
{
    public static class DatabaseHelper
    {
        public static string DbPath { get; } =
            Path.Combine(AppContext.BaseDirectory, "conciliador.db");

        public static SqliteConnection GetConnection()
        {
            var cn = new SqliteConnection($"Data Source={DbPath}");
            SqlitePragmas.ApplyOnOpen(cn);
            return cn;
        }

        public static void InitializeDatabase()
        {
            using var cn = GetConnection();
            cn.Open();

            cn.ExecuteNonQuery(@"
                CREATE TABLE IF NOT EXISTS Usuarios (
                    Id           INTEGER PRIMARY KEY AUTOINCREMENT,
                    Username     TEXT NOT NULL UNIQUE COLLATE NOCASE,
                    PasswordHash TEXT NOT NULL,
                    Nombre       TEXT NOT NULL DEFAULT '',
                    Activo       INTEGER NOT NULL DEFAULT 1,
                    Rol          TEXT NOT NULL DEFAULT 'Usuario',
                    PermisosJson TEXT NOT NULL DEFAULT '[]'
                )");

            // Migrar columnas si la tabla ya existía sin ellas
            TryAddColumn(cn, "Usuarios", "Rol",          "TEXT NOT NULL DEFAULT 'Usuario'");
            TryAddColumn(cn, "Usuarios", "PermisosJson", "TEXT NOT NULL DEFAULT '[]'");
            // Fecha: 28/10/2026 - TAREA: 00140 - Linea: 2,3,4 - Ingreso con Microsoft a través de AppHub: correo y id de Microsoft del usuario (la web guarda el id al primer ingreso). Un usuario de Microsoft no tiene contraseña: PasswordHash queda en «!», que nunca coincide con un hash SHA-256.
            TryAddColumn(cn, "Usuarios", "Correo",       "TEXT NULL");
            TryAddColumn(cn, "Usuarios", "OidMicrosoft", "TEXT NULL");
            cn.ExecuteNonQuery("CREATE UNIQUE INDEX IF NOT EXISTS UX_Usuarios_Oid ON Usuarios(OidMicrosoft) WHERE OidMicrosoft IS NOT NULL");
            cn.ExecuteNonQuery("CREATE UNIQUE INDEX IF NOT EXISTS UX_Usuarios_Correo ON Usuarios(Correo COLLATE NOCASE) WHERE Correo IS NOT NULL AND Correo <> ''");

            // Admin por defecto: rol Admin, todos los permisos
            cn.ExecuteNonQuery(@"
                INSERT OR IGNORE INTO Usuarios (Username, PasswordHash, Nombre, Activo, Rol, PermisosJson)
                VALUES ('admin', '" + Auth.AuthService.HashPassword("admin123") + @"', 'Administrador', 1, 'Admin', '[]')");

            // Asegurar que el admin existente sea rol Admin
            cn.ExecuteNonQuery(@"
                UPDATE Usuarios SET Rol = 'Admin' WHERE Username = 'admin' AND Rol = 'Usuario'");
        }

        private static void TryAddColumn(SqliteConnection cn, string table, string col, string def)
        {
            try { cn.ExecuteNonQuery($"ALTER TABLE {table} ADD COLUMN {col} {def}"); }
            catch { /* columna ya existe */ }
        }
    }

    /// <summary>
    /// WAL + synchronous=NORMAL. Sin esto SQLite hace un fsync por cada commit,
    /// lo que vuelve inviable cualquier escritura fila por fila.
    /// synchronous es por conexión, así que hay que aplicarlo en cada apertura.
    /// </summary>
    internal static class SqlitePragmas
    {
        public static void ApplyOnOpen(SqliteConnection cn)
        {
            cn.StateChange += (s, e) =>
            {
                if (e.CurrentState != ConnectionState.Open) return;
                using var cmd = ((SqliteConnection)s).CreateCommand();
                cmd.CommandText = "PRAGMA journal_mode=WAL; PRAGMA synchronous=NORMAL;";
                cmd.ExecuteNonQuery();
            };
        }
    }

    internal static class SqliteConnectionExtensions
    {
        public static void ExecuteNonQuery(this SqliteConnection cn, string sql)
        {
            using var cmd = cn.CreateCommand();
            cmd.CommandText = sql;
            cmd.ExecuteNonQuery();
        }
    }
}
