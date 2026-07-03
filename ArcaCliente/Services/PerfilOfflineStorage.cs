using System.Collections.Generic;
using ArcaCliente.Models;

namespace ArcaCliente.Services
{
    internal static class PerfilOfflineStorage
    {
        public static void Save(List<PerfilOffline> perfiles) =>
            ArcaSqliteStorage.SavePerfilesOffline(perfiles);

        public static List<PerfilOffline> Load() =>
            ArcaSqliteStorage.LoadPerfilesOffline();
    }
}
