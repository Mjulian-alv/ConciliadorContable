using ArcaCliente.Models;

namespace ArcaCliente.Services
{
    /// <summary>
    /// Persistencia del mapeo de columnas por entidad (para recordar la configuracion de
    /// importacion entre sesiones).
    /// </summary>
    internal static class MapeoColumnasStorage
    {
        /// <summary>Devuelve el mapeo guardado para la entidad, o null si no existe.</summary>
        public static MapeoColumnasArchivo Load(string entidad) =>
            ArcaSqliteStorage.LoadMapeoColumnas(entidad);

        public static void Save(MapeoColumnasArchivo mapeo) =>
            ArcaSqliteStorage.SaveMapeoColumnas(mapeo);
    }
}
