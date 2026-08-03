using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using static ArcaCliente.Services.DbHelpers;

namespace ArcaCliente.Services
{
    /// <summary>
    /// Estado del resultado de búsqueda de proveedor por CUIT (§7.6).
    /// </summary>
    public enum EstadoBusquedaProveedor
    {
        /// <summary>Ningún registro en <c>PV_D3_PROVE</c> con ese CUIT. No se puede dar de alta el documento.</summary>
        NoEncontrado,
        /// <summary>Exactamente un proveedor encontrado. Usar <c>Proveedores[0].Numero</c> directamente.</summary>
        Unico,
        /// <summary>Dos o más proveedores con el mismo CUIT. Mostrar <c>FormSeleccionProveedor</c>.</summary>
        Multiples
    }

    /// <summary>Proveedor encontrado en <c>PV_D3_PROVE</c>.</summary>
    public record ProveedorItem(string Numero, string Nombre);

    /// <summary>Resultado de <see cref="OctosisProveedorService.BuscarPorCuitAsync"/>.</summary>
    public record BusquedaProveedorResult(
        EstadoBusquedaProveedor  Estado,
        IReadOnlyList<ProveedorItem> Proveedores);

    /// <summary>
    /// Búsqueda de proveedores en <c>PV_D3_PROVE</c> por CUIT del emisor (campo <c>NroDocEmisor</c> del CSV de ARCA).
    /// <para>
    /// Implementa los tres casos del §7.6:
    /// <list type="bullet">
    ///   <item>0 resultados ? <see cref="EstadoBusquedaProveedor.NoEncontrado"/>: informar al usuario, no crear el documento.</item>
    ///   <item>1 resultado ? <see cref="EstadoBusquedaProveedor.Unico"/>: usar directamente.</item>
    ///   <item>2+ resultados ? <see cref="EstadoBusquedaProveedor.Multiples"/>: mostrar <c>FormSeleccionProveedor</c>.</item>
    /// </list>
    /// </para>
    /// </summary>
    public class OctosisProveedorService
    {
        /// <summary>
        /// Busca todos los proveedores activos con el CUIT indicado.
        /// </summary>
        /// <param name="cuit">CUIT del emisor sin guiones (campo <c>NroDocEmisor</c> del CSV).</param>
        /// <param name="conn">Conexión abierta a la base Octosis.</param>
        public async Task<BusquedaProveedorResult> BuscarPorCuitAsync(string cuit, IDbConnection conn)
        {
            return await Task.Run(() =>
            {
                var lista = new List<ProveedorItem>();

                using var cmd = conn.CreateCommand();
                cmd.CommandText = @"
                    SELECT
                        LTRIM(RTRIM(ISNULL(pro_numero, ''))) AS pro_numero,
                        LTRIM(RTRIM(ISNULL(pro_nombre, ''))) AS pro_nombre
                    FROM pv_d3_prove
                    WHERE (deleteado IS NULL OR deleteado != 'X')
                      AND replace(pro_cuit,'-','') = @Cuit
                    ORDER BY pro_numero";

                AddParameter(cmd, "@Cuit", cuit?.Trim() ?? string.Empty);

                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                    lista.Add(new ProveedorItem(
                        reader["pro_numero"]?.ToString() ?? string.Empty,
                        reader["pro_nombre"]?.ToString() ?? string.Empty));

                var estado = lista.Count switch
                {
                    0 => EstadoBusquedaProveedor.NoEncontrado,
                    1 => EstadoBusquedaProveedor.Unico,
                    _ => EstadoBusquedaProveedor.Multiples
                };

                return new BusquedaProveedorResult(estado, lista);
            });
        }

    }
}
