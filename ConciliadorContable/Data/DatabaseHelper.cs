using System;
using System.IO;
using Microsoft.Data.Sqlite;

namespace ConciliadorContable.Data
{
    public static class DatabaseHelper
    {
        public static string DbPath { get; } =
            Path.Combine(AppContext.BaseDirectory, "conciliador.db");

        public static SqliteConnection GetConnection() =>
            new SqliteConnection($"Data Source={DbPath}");

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
