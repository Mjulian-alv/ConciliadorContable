using System.Collections.Generic;
using ArcaCliente.Models;

namespace ArcaCliente.Services
{
    internal static class PerfilOfflineStorage
    {
        public static void Save(List<PerfilOffline> perfiles) =>
            ArcaSqlStorage.SavePerfilesOffline(perfiles);

        public static List<PerfilOffline> Load() =>
            ArcaSqlStorage.LoadPerfilesOffline();
    }
}
