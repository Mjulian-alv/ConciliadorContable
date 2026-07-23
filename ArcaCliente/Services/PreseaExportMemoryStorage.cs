using System.Collections.Generic;
using ArcaCliente.Models;

namespace ArcaCliente.Services
{
    /// <summary>
    /// Memoria anti-duplicado de comprobantes exportados a PRESEA, persistida en SQLite.
    /// </summary>
    internal static class PreseaExportMemoryStorage
    {
        /// <summary>True si el comprobante (por su clave) ya fue exportado.</summary>
        public static bool YaExportado(string clave) =>
            ArcaSqlStorage.ExisteComprobanteExportado(clave);

        /// <summary>Conjunto de todas las claves ya exportadas (para marcar la grilla en bloque).</summary>
        public static HashSet<string> ClavesExportadas() =>
            ArcaSqlStorage.LoadClavesExportadas();

        /// <summary>Registra un comprobante como exportado (idempotente).</summary>
        public static void Registrar(PreseaComprobanteExportado exportado) =>
            ArcaSqlStorage.RegistrarComprobanteExportado(exportado);

        /// <summary>Registra un lote de comprobantes como exportados en una sola transaccion.</summary>
        public static void RegistrarRange(IEnumerable<PreseaComprobanteExportado> exportados) =>
            ArcaSqlStorage.RegistrarComprobantesExportados(exportados);

        public static List<PreseaComprobanteExportado> Load() =>
            ArcaSqlStorage.LoadComprobantesExportados();
    }
}
