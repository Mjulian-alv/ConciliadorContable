using System.Collections.Generic;
using ArcaCliente.Models;

namespace ArcaCliente.Services
{
    internal static class PerfilStorage
    {
        public static void Save(List<PerfilFiscal> perfiles) =>
            ArcaSqlStorage.SavePerfilesFiscales(perfiles);

        public static List<PerfilFiscal> Load() =>
            ArcaSqlStorage.LoadPerfilesFiscales();
    }
}
