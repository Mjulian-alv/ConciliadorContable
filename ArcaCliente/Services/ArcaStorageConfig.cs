using System;
using System.IO;

namespace ArcaCliente.Services
{
    /// <summary>
    /// Centraliza la ruta de la base de datos SQLite usada por ArcaCliente para
    /// configuración (perfiles offline, perfiles fiscales, equivalencias).
    /// <para>
    /// Cuando ArcaCliente corre dentro de ConciliadorContable, el host puede
    /// asignar <see cref="DbPath"/> antes de abrir cualquier formulario para que
    /// todos los datos queden en la misma base central.
    /// Si no se asigna, apunta al directorio del ejecutable de ArcaCliente.
    /// </para>
    /// </summary>
    public static class ArcaStorageConfig
    {
        private static string? _dbPath;

        public static string DbPath
        {
            get => _dbPath ??= Path.Combine(AppContext.BaseDirectory, "conciliador.db");
            set => _dbPath = value;
        }

        public static string ConnectionString => $"Data Source={DbPath}";

        /// <summary>
        /// Inicializa las tablas de ArcaCliente y migra datos legacy si corresponde.
        /// Llamar desde el host (ConciliadorContable) después de asignar <see cref="DbPath"/>.
        /// </summary>
        public static void Initialize()
        {
            ArcaSqliteStorage.InitializeDatabase();
            ArcaSqliteStorage.MigrarDesdeLegacyIfNeeded();
        }
    }
}
