namespace ArcaCliente.Services
{
    /// <summary>
    /// Gate de inicialización del módulo ArcaCliente. Autosuficiente: cualquier
    /// host que abra un form de ArcaCliente dispara la inicialización sola,
    /// sin coordinación externa (a diferencia del viejo esquema SQLite, donde
    /// el host tenía que asignar la ruta del archivo antes de abrir cualquier form).
    /// </summary>
    public static class ArcaStorageConfig
    {
        private static bool _inicializado;

        /// <summary>Crea el schema/tablas si no existen. Run-once por proceso.</summary>
        public static void Initialize()
        {
            if (_inicializado) return;
            ArcaSqlStorage.InitializeDatabase();
            _inicializado = true;
        }
    }
}
