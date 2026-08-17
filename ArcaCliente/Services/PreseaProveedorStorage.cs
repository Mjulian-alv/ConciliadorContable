using System.Collections.Generic;
using ArcaCliente.Models;

namespace ArcaCliente.Services
{
    /// <summary>
    /// Acceso al mapa de proveedores PRESEA (por CUIT) persistido en SQLite.
    /// </summary>
    internal class PreseaProveedorStorage
    {
        
        public static List<ConfigPreseaProveedor> Load() =>
            ArcaSqlStorage.LoadPreseaProveedores();

        /// <summary>Devuelve el proveedor por CUIT o null si no esta cargado.</summary>
        public static ConfigPreseaProveedor Get(string cuit) =>
            ArcaSqlStorage.GetPreseaProveedor(cuit);

        /// <summary>Inserta o actualiza un proveedor (upsert por CUIT).</summary>
        public static void Upsert(ConfigPreseaProveedor proveedor) =>
            ArcaSqlStorage.UpsertPreseaProveedor(proveedor);

        /// <summary>Inserta o actualiza un lote de proveedores en una sola transaccion.</summary>
        public static void UpsertRange(IEnumerable<ConfigPreseaProveedor> proveedores) =>
            ArcaSqlStorage.Instancia.UpsertPreseaProveedores(proveedores);
    }
}
