// Fecha: 21/09/2026 - TAREA: 00041 - Linea: 2 - Persistencia de los perfiles PyR (mismo patron que PerfilOfflineStorage)
using System.Collections.Generic;
using ArcaCliente.Models;

namespace ArcaCliente.Services
{
    internal static class PerfilPyRStorage
    {
        public static void Save(List<PerfilOfflinePyR> perfiles) =>
            ArcaSqlStorage.SavePerfilesPyR(perfiles);

        public static List<PerfilOfflinePyR> Load() =>
            ArcaSqlStorage.LoadPerfilesPyR();
    }
}
