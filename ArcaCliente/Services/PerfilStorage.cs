using System.Collections.Generic;
using ArcaCliente.Models;

namespace ArcaCliente.Services
{
    internal static class PerfilStorage
    {
        public static void Save(List<PerfilFiscal> perfiles) =>
            ArcaSqliteStorage.SavePerfilesFiscales(perfiles);

        public static List<PerfilFiscal> Load() =>
            ArcaSqliteStorage.LoadPerfilesFiscales();
    }
}
