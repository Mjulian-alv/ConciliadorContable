using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using ArcaCliente.Models;

namespace ArcaCliente.Services
{
    /// <summary>
    /// Consultas de solo lectura sobre el master de tasas impositivas (<c>MI_I_PARCF0</c>).
    /// Equivale a <c>clsMIConjTasas</c> en wsmbcg2010.
    /// <para>Todos los métodos son parametrizados y asíncronos. Sin escrituras, sin transacciones.</para>
    /// <para>
    /// La conexión se recibe por parámetro en cada método para permitir reutilizarla
    /// dentro de la transacción del llamador (ej: <c>OctosisDocumentoService.AltaAsync()</c>).
    /// </para>
    /// </summary>
    public class OctosisImpuestosService
    {
        /// <summary>
        /// Busca la tasa con el porcentaje indicado habilitada para el módulo Proveedores.
        /// Equivale a <c>clsMIConjTasas.Obtener()</c> filtrado por <c>par_porcen</c>.
        /// Usado por <c>TaxDisaggregationService</c> para desagregar los impuestos del CSV de ARCA.
        /// </summary>
        /// <param name="porcentaje">Porcentaje a buscar (ej: 21, 10.5, 27, 5, 2.5).</param>
        /// <param name="conn">Conexión abierta. No se abre ni se cierra internamente.</param>
        /// <returns>
        /// La primera tasa encontrada con <c>par_habmpv = 'S'</c> para ese porcentaje,
        /// o <c>null</c> si no existe ninguna parametrizada.
        /// </returns>
        public async Task<OctosisTasa?> ObtenerPorPorcentajeAsync(decimal porcentaje, IDbConnection conn, IDbTransaction? tx = null)
        {
            return await Task.Run(() =>
            {
                using var cmd = conn.CreateCommand();
                cmd.Transaction = tx;
                cmd.CommandText = @"
                    SELECT par_linea, par_descri, par_decret, par_porcen,
                           par_cuenta, par_coliva, par_lintot, par_habili,
                           par_habmpv, par_resnet, par_genper, par_column,
                           par_ivagra, par_impnet, par_ctneto, par_relmba
                    FROM MI_I_PARCF0
                    WHERE par_porcen = @Porcentaje
                      AND par_habmpv = 'S'
                      AND (deleteado IS NULL OR deleteado != 'X')";

                AddParameter(cmd, "@Porcentaje", porcentaje);

                using var reader = cmd.ExecuteReader();
                return reader.Read() ? MapearTasa(reader) : null;
            });
        }

        /// <summary>
        /// Busca la tasa con el número de línea indicado.
        /// Equivale a <c>clsMIConjTasas.Obtener(linea)</c>.
        /// Usado por <c>OctosisImputacionesService</c> para obtener la cuenta contable de cada tasa.
        /// </summary>
        /// <param name="linea">Número de línea (clave de <c>MI_I_PARCF0</c>).</param>
        /// <param name="conn">Conexión abierta. No se abre ni se cierra internamente.</param>
        /// <returns>La tasa encontrada, o <c>null</c> si no existe.</returns>
        public async Task<OctosisTasa?> ObtenerPorLineaAsync(string linea, IDbConnection conn, IDbTransaction? tx = null)
        {
            if (string.IsNullOrWhiteSpace(linea))
                throw new ArgumentNullException(nameof(linea));

            return await Task.Run(() =>
            {
                using var cmd = conn.CreateCommand();
                cmd.Transaction = tx;
                cmd.CommandText = @"
                    SELECT par_linea, par_descri, par_decret, par_porcen,
                           par_cuenta, par_coliva, par_lintot, par_habili,
                           par_habmpv, par_resnet, par_genper, par_column,
                           par_ivagra, par_impnet, par_ctneto, par_relmba
                    FROM MI_I_PARCF0
                    WHERE ltrim(rtrim(par_linea)) = ltrim(rtrim(@Linea))
                      AND (deleteado IS NULL OR deleteado != 'X')";

                AddParameter(cmd, "@Linea", linea.Trim());

                using var reader = cmd.ExecuteReader();
                return reader.Read() ? MapearTasa(reader) : null;
            });
        }

        /// <summary>
        /// Retorna todas las tasas habilitadas para el módulo Proveedores (<c>par_habmpv = 'S'</c>),
        /// ordenadas por número de línea.
        /// Útil para validar previamente que todas las tasas requeridas estén parametrizadas
        /// antes de procesar un lote de comprobantes.
        /// </summary>
        /// <param name="conn">Conexión abierta. No se abre ni se cierra internamente.</param>
        public async Task<List<OctosisTasa>> ObtenerTodasParaProveedoresAsync(IDbConnection conn, IDbTransaction? tx = null)
        {
            return await Task.Run(() =>
            {
                var result = new List<OctosisTasa>();

                using var cmd = conn.CreateCommand();
                cmd.Transaction = tx;
                cmd.CommandText = @"
                    SELECT par_linea, par_descri, par_decret, par_porcen,
                           par_cuenta, par_coliva, par_lintot, par_habili,
                           par_habmpv, par_resnet, par_genper, par_column,
                           par_ivagra, par_impnet, par_ctneto, par_relmba
                    FROM MI_I_PARCF0
                    WHERE par_habmpv = 'S'
                      AND (deleteado IS NULL OR deleteado != 'X')
                    ORDER BY par_linea";

                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                    result.Add(MapearTasa(reader));

                return result;
            });
        }

        // ?? helpers ???????????????????????????????????????????????????????????????

        private static OctosisTasa MapearTasa(IDataReader r) => new()
        {
            NumeroLinea          = r["par_linea"]?.ToString()  ?? string.Empty,
            Descripcion          = r["par_descri"]?.ToString() ?? string.Empty,
            Decreto              = r["par_decret"]?.ToString() ?? string.Empty,
            Porcentaje           = r["par_porcen"]  == DBNull.Value ? 0m : Convert.ToDecimal(r["par_porcen"]),
            CuentaContable       = r["par_cuenta"]?.ToString() ?? string.Empty,
            ColumnaLibroIva      = r["par_coliva"]?.ToString() ?? string.Empty,
            TotalizaSeparado     = EsS(r["par_lintot"]),
            Habilitado           = EsS(r["par_habili"]),
            IncluyeEnProveedores = EsS(r["par_habmpv"]),
            Resta                = EsS(r["par_resnet"]),
            Genera               = EsS(r["par_genper"]),
            ColumnaImputa        = r["par_column"]?.ToString() ?? string.Empty,
            EsIvaGeneral         = EsS(r["par_ivagra"]),
            ImputaNeto           = EsS(r["par_impnet"]),
            CuentaNeto           = r["par_ctneto"]?.ToString() ?? string.Empty,
            RelTasaMba           = r["par_relmba"]?.ToString() ?? string.Empty,
        };

        private static bool EsS(object valor) =>
            valor != DBNull.Value
            && string.Equals(valor?.ToString(), "S", StringComparison.OrdinalIgnoreCase);

        private static void AddParameter(IDbCommand cmd, string name, object? value)
        {
            var p = cmd.CreateParameter();
            p.ParameterName = name;
            p.Value         = value ?? DBNull.Value;
            cmd.Parameters.Add(p);
        }
    }
}
