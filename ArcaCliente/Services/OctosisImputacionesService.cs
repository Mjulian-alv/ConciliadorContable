using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using ArcaCliente.Models;

namespace ArcaCliente.Services
{
    /// <summary>
    /// Validación de cuentas contables y generación de asientos para documentos de Proveedores.
    /// Equivale a <c>clsPVImputaciones</c> + <c>clsCTConjImputaciones_Uni</c> + <c>clsCtConjCuentas</c> en wsmbcg2010.
    /// <para>
    /// Responsabilidades:
    /// <list type="number">
    ///   <item>Validar que todas las cuentas involucradas estén habilitadas en <c>CT_CT_BLICT</c>.</item>
    ///   <item>Construir la lista de asientos (contrapartida + cuenta proveedor + una línea por tasa).</item>
    ///   <item>Persistir cada asiento via <c>sp_pv_conj_imputaciones_uni_alta</c> dentro de la transacción del llamador.</item>
    /// </list>
    /// </para>
    /// <para>
    /// Casos especiales de <c>imputar_documento()</c> que no aplican a documentos ARCA (§13.3):
    /// <c>es_compra_bienes_usados</c>, <c>tiene_relacion_descuento</c> y diferencia de cambio.
    /// Todos los documentos ARCA van por el flujo normal.
    /// </para>
    /// <para>
    /// Reemplaza <c>frmDifBalanceo</c> por <see cref="InvalidOperationException"/> con mensaje descriptivo.
    /// </para>
    /// </summary>
    public class OctosisImputacionesService
    {
        // Equivalen a las constantes de Globales.vb
        private const string ModuloProveedores       = " 1";  // modulo_proveedores
        private const string TipoSubmayorProveedores = "1";   // cuenta_tipo_submayor_proveedores (el SP lo maneja internamente)

        private readonly OctosisImpuestosService _impuestos;

        public OctosisImputacionesService(OctosisImpuestosService impuestos)
        {
            _impuestos = impuestos ?? throw new ArgumentNullException(nameof(impuestos));
        }

        // ?? API pública ???????????????????????????????????????????????????????

        /// <summary>
        /// Valida cuentas, construye y persiste los asientos contables del documento.
        /// Equivale a <c>clsPVImputaciones.imputar()</c>.
        /// <para>
        /// <b>Debe llamarse dentro de la transacción del llamador</b>
        /// (la misma que usó <c>OctosisDocumentoService.AltaAsync()</c>).
        /// </para>
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Si falta contrapartida, una cuenta está bloqueada, o una tasa no está parametrizada.
        /// El mensaje incluye qué cuenta/tasa falla para facilitar la corrección.
        /// </exception>
        public async Task ImputarAsync(OctosisDocumento doc, IDbConnection conn, IDbTransaction tx)
        {
            await Task.Run(() => ImputarCore(doc, conn, tx));
        }

        // ?? núcleo síncrono ???????????????????????????????????????????????????

        private void ImputarCore(OctosisDocumento doc, IDbConnection conn, IDbTransaction tx)
        {
            // Obtiene cuentas una sola vez; se pasan a validar e imputar
            var cuentas = ObtenerCuentasProveedorCore(doc.CodProveedor, conn, tx);
            ValidarImputacionesCore(doc, cuentas, conn, tx);
            ImputarDocumentoNormalCore(doc, cuentas, conn, tx);
        }

        private void ValidarImputacionesCore(
            OctosisDocumento doc, CuentasProveedor cuentas, IDbConnection conn, IDbTransaction tx)
        {
            // Contrapartida (cuenta de gasto/compra)
            if (string.IsNullOrWhiteSpace(cuentas.Contrapartida))
                throw new InvalidOperationException(
                    $"No se encontró contrapartida contable para el proveedor '{doc.CodProveedor}' ni cuenta de balanceo automática. " +
                    "Verificar parametrización en proveedores o CT_CT_PARA0.");

            if (!ValidarCuentaCore(cuentas.Contrapartida, doc.FecEmision, conn, tx))
                throw new InvalidOperationException(
                    $"La cuenta de contrapartida '{cuentas.Contrapartida}' está bloqueada " +
                    $"a la fecha {doc.FecEmision:dd/MM/yyyy}. Verificar en CT_CT_BLICT.");

            // Cuenta del proveedor (submayor)
            if (!string.IsNullOrWhiteSpace(cuentas.CuentaProveedor)
                && !ValidarCuentaCore(cuentas.CuentaProveedor, doc.FecEmision, conn, tx))
                throw new InvalidOperationException(
                    $"La cuenta del proveedor '{cuentas.CuentaProveedor}' está bloqueada " +
                    $"a la fecha {doc.FecEmision:dd/MM/yyyy}. Verificar en CT_CT_BLICT.");

            // Cuentas de tasas (validadas contra FEC_RECEPCION, igual que el VB)
            foreach (var tasa in doc.Tasas)
            {
                var lineaTrim  = tasa.Linea.Trim();
                var octosisTasa = _impuestos.ObtenerPorLineaAsync(lineaTrim, conn, tx)
                                            .GetAwaiter().GetResult();

                if (octosisTasa == null || octosisTasa.NumeroLinea.Trim() != lineaTrim)
                    throw new InvalidOperationException(
                        $"No se encontró la tasa de línea '{lineaTrim}' en MI_I_PARCF0. " +
                        "Verificar parametrización en módulo Impuestos.");

                string cuentaTasa = ExtraerCuenta(octosisTasa.CuentaContable);
                if (!string.IsNullOrWhiteSpace(cuentaTasa)
                    && !ValidarCuentaCore(cuentaTasa, doc.FecRecepcion, conn, tx))
                    throw new InvalidOperationException(
                        $"La cuenta '{cuentaTasa}' de la tasa {tasa.Porcentaje}% (línea '{lineaTrim}') " +
                        $"está bloqueada a la fecha {doc.FecRecepcion:dd/MM/yyyy}. Verificar en CT_CT_BLICT.");
            }
        }

        private void ImputarDocumentoNormalCore(
            OctosisDocumento doc, CuentasProveedor cuentas, IDbConnection conn, IDbTransaction tx)
        {
            var     imputaciones = new List<OctosisImputacionUni>();
            decimal totalDebe    = 0m;
            decimal totalHaber   = 0m;

            // ?? Tasas ?????????????????????????????????????????????????????????
            // Igual que VB: se saltea si el documento es una minuta "M"
            if (doc.Minuta != "M")
            {
                foreach (var tasa in doc.Tasas)
                {
                    if (tasa.Importe == 0m && tasa.Neto == 0m) continue;

                    var octosisTasa = _impuestos.ObtenerPorLineaAsync(tasa.Linea.Trim(), conn, tx)
                                                .GetAwaiter().GetResult();
                    if (octosisTasa == null) continue;

                    string cuentaTasa = ExtraerCuenta(octosisTasa.CuentaContable);
                    if (!CuentaConValor(cuentaTasa)) continue;

                    var (debe, haber) = CalcularDebeHaber(
                        tasa.Importe, doc.TipoDocumento, octosisTasa.ColumnaImputa);

                    totalDebe  += debe;
                    totalHaber += haber;

                    if (tasa.Importe != 0m)
                        imputaciones.Add(ConstruirImputacion(doc, cuentaTasa, debe, haber));

                    // Neto a cuenta alternativa (lImputaNeto / par_impnet)
                    if (octosisTasa.ImputaNeto && tasa.Neto != 0m)
                    {
                        string cuentaNeto = ExtraerCuenta(octosisTasa.CuentaNeto);
                        if (CuentaConValor(cuentaNeto))
                        {
                            var (debeN, haberN) = CalcularDebeHaber(
                                tasa.Neto, doc.TipoDocumento, octosisTasa.ColumnaImputa);
                            totalDebe  += debeN;
                            totalHaber += haberN;
                            imputaciones.Add(ConstruirImputacion(doc, cuentaNeto, debeN, haberN));
                        }
                    }
                }
            }

            decimal totalTasas = totalDebe + totalHaber;   // VB: total_tasa
            decimal totalNeto  = doc.Importe - totalTasas;  // VB: total_neto

            // ?? Contrapartida (cuenta de gasto / compra) ??????????????????????
            var (cpDebe, cpHaber) = EsFacturaODebito(doc.TipoDocumento)
                ? (totalNeto, 0m)
                : (0m,        totalNeto);

            if (totalNeto != 0m && CuentaConValor(cuentas.Contrapartida))
                imputaciones.Add(ConstruirImputacion(doc, cuentas.Contrapartida, cpDebe, cpHaber));

            // ?? Cuenta del proveedor (submayor) ???????????????????????????????
            var (pvDebe, pvHaber) = EsFacturaODebito(doc.TipoDocumento)
                ? (0m,         doc.Importe)
                : (doc.Importe, 0m);

            if (doc.Importe != 0m && CuentaConValor(cuentas.CuentaProveedor))
                imputaciones.Add(ConstruirImputacion(doc, cuentas.CuentaProveedor, pvDebe, pvHaber));

            // ?? Configurar clave/código/cambio y persistir ????????????????????
            for (int i = 0; i < imputaciones.Count; i++)
            {
                ConfigurarImputacion(imputaciones[i], doc, i + 1);  // item_aux arranca en 1
                AltaAsientoCore(imputaciones[i], conn, tx);
            }
        }

        // ?? helpers de datos ??????????????????????????????????????????????????

        private static CuentasProveedor ObtenerCuentasProveedorCore(
            string codProveedor, IDbConnection conn, IDbTransaction tx)
        {
            string grImputa        = string.Empty;
            string contrapartidaPV = string.Empty;

            // 1. Leer pro_impaut (grupo) y pro_imputa (contrapartida directa) del proveedor
            using (var cmd = conn.CreateCommand())
            {
                cmd.Transaction = tx;
                cmd.CommandText = @"
                    SELECT TOP 1
                        ISNULL(pro_impaut, '') AS GrImputa,
                        ISNULL(pro_imputa, '') AS Contrapartida
                    FROM pv_d3_prove
                    WHERE (deleteado IS NULL OR deleteado != 'X')
                      AND ISNULL(pro_numero, '') = @CodProveedor";
                AddParameter(cmd, "@CodProveedor", codProveedor);

                using var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    grImputa        = reader["GrImputa"]?.ToString()     ?? string.Empty;
                    contrapartidaPV = reader["Contrapartida"]?.ToString() ?? string.Empty;
                }
            }

            // 2. Leer imp_ia5 (contrapartida) e imp_ia1 (cuenta proveedor) del grupo
            string contrapartidaIMP  = string.Empty;
            string cuentaProveedorIMP = string.Empty;

            if (!string.IsNullOrWhiteSpace(grImputa))
            {
                using var cmd2 = conn.CreateCommand();
                cmd2.Transaction = tx;
                // SUBSTRING(str, 3, 18) en SQL (1-based) = skip 2 chars = Substring(2,18) en C#
                cmd2.CommandText = @"
                    SELECT TOP 1
                        SUBSTRING(ISNULL(imp_ia5, '') + SPACE(21), 3, 18) AS Contrapartida,
                        SUBSTRING(ISNULL(imp_ia1, '') + SPACE(21), 3, 18) AS CuentaProv
                    FROM pv_p_impau0
                    WHERE (deleteado IS NULL OR deleteado != 'X')
                      AND ISNULL(imp_codigo, '') = @GrImputa";
                AddParameter(cmd2, "@GrImputa", grImputa);

                using var reader2 = cmd2.ExecuteReader();
                if (reader2.Read())
                {
                    contrapartidaIMP  = reader2["Contrapartida"]?.ToString()?.Trim() ?? string.Empty;
                    cuentaProveedorIMP = reader2["CuentaProv"]?.ToString()?.Trim()    ?? string.Empty;
                }
            }

            // 3. Resolver contrapartida: primero la del proveedor, si no la del grupo
            string cpPVClean = contrapartidaPV.Length >= 20
                ? contrapartidaPV.Substring(2, 18).Trim()
                : string.Empty;

            string contrapartida = CuentaConValor(cpPVClean)
                ? cpPVClean
                : (CuentaConValor(contrapartidaIMP) ? contrapartidaIMP : string.Empty);

            if (!CuentaConValor(contrapartida))
            {
                contrapartida = TraeCuentaBalanceoCore(conn, tx);
            }

            string cuentaProveedor = CuentaConValor(cuentaProveedorIMP)
                ? cuentaProveedorIMP
                : string.Empty;

            return new CuentasProveedor(contrapartida, cuentaProveedor);
        }

        private static string TraeCuentaBalanceoCore(IDbConnection conn, IDbTransaction tx)
        {
            using var cmd = conn.CreateCommand();
            cmd.Transaction = tx;
            cmd.CommandText = @"
                SELECT TOP 1 par_cuenta 
                FROM ct_ct_para0 
                WHERE PAR_CODIGO = 'BALANC' 
                  AND (deleteado IS NULL OR deleteado != 'X')";

            var result = cmd.ExecuteScalar();
            string rawCuenta = result?.ToString() ?? string.Empty;

            return ExtraerCuenta(rawCuenta);
        }

        /// <summary>
        /// Verifica si una cuenta está habilitada en <c>CT_CT_BLICT</c> a la fecha dada.
        /// Si no hay registro en la tabla de bloqueos ? válida (igual que el VB original).
        /// </summary>
        private static bool ValidarCuentaCore(string cuenta, DateTime fecha, IDbConnection conn, IDbTransaction? tx)
        {
            if (string.IsNullOrWhiteSpace(cuenta)) return true;

            using var cmd = conn.CreateCommand();
            cmd.Transaction = tx;
            cmd.CommandText = @"
                SELECT TOP 1 bli_habili
                FROM CT_CT_BLICT
                WHERE BLI_CUENTA = @Cuenta
                  AND bli_fecha  <= @Fecha
                ORDER BY bli_fecha DESC";
            AddParameter(cmd, "@Cuenta", cuenta.Trim());
            AddParameter(cmd, "@Fecha",  fecha.Date);

            var result = cmd.ExecuteScalar();
            if (result == null || result == DBNull.Value)
                return true;  // sin registro ? habilitada
            return string.Equals(result.ToString()?.Trim(), "S", StringComparison.OrdinalIgnoreCase);
        }

        private static void AltaAsientoCore(
            OctosisImputacionUni imp, IDbConnection conn, IDbTransaction tx)
        {
            if (imp.Debe == 0m && imp.Haber == 0m) return;

            using var cmd = conn.CreateCommand();
            cmd.Transaction = tx;
            cmd.CommandText = "sp_pv_conj_imputaciones_uni_alta";
            cmd.CommandType = CommandType.StoredProcedure;

            AddParameter(cmd, "@imp_clave",   imp.Clave);
            AddParameter(cmd, "@imp_codigo",  imp.Codigo);
            AddParameter(cmd, "@imp_fecha",   imp.Fecha);
            AddParameter(cmd, "@imp_cuenta",  imp.Cuenta);
            AddParameter(cmd, "@imp_moneda",  imp.Moneda);
            AddParameter(cmd, "@imp_concep",  imp.Concepto);
            AddParameter(cmd, "@imp_ctclas",  string.Empty);    // nunca asignado en el flujo PV
            AddParameter(cmd, "@imp_ctsubc",  string.Empty);    // nunca asignado en el flujo PV
            AddParameter(cmd, "@imp_debe",    imp.Debe);
            AddParameter(cmd, "@imp_haber",   imp.Haber);
            AddParameter(cmd, "@imp_tipasi",  imp.TipoAsiento);
            AddParameter(cmd, "@imp_patent",  string.Empty);    // nunca asignado en el flujo PV
            AddParameter(cmd, "@imp_numcos",  imp.NroCosto);
            AddParameter(cmd, "@imp_sistem",  imp.Sistema);
            AddParameter(cmd, "@imp_tipo",    imp.Tipo);
            AddParameter(cmd, "@imp_subsis",  imp.SubSistema);
            AddParameter(cmd, "@imp_cierre",  imp.CodCierre);
            AddParameter(cmd, "@imp_tipdoc",  imp.TipoDocumento);
            AddParameter(cmd, "@imp_numdoc",  imp.NroDocumento);
            AddParameter(cmd, "@imp_cambio",  imp.Cambio);
            AddParameter(cmd, "@imp_exclus",  imp.Exclusiva);

            cmd.ExecuteNonQuery();
        }

        // ?? construcción de imputaciones ??????????????????????????????????????

        private static OctosisImputacionUni ConstruirImputacion(
            OctosisDocumento doc, string cuenta, decimal debe, decimal haber) => new()
        {
            Cuenta = cuenta.Trim().PadLeft(18),
            Debe   = debe,
            Haber  = haber,
            Fecha  = doc.FecRecepcion,
        };

        /// <summary>
        /// Asigna <c>Clave</c>, <c>Codigo</c> y <c>Cambio</c> (equivale a <c>Configurar_imputacion</c>).
        /// La clave es: <c>ModuloProveedores + NumeroSucursal + CodProveedor + TipoDocumento + NroInterno</c>
        /// padded a 30 chars.
        /// </summary>
        private static void ConfigurarImputacion(
            OctosisImputacionUni imp, OctosisDocumento doc, int itemIdx)
        {
            string clave = ModuloProveedores
                         + doc.NumeroSucursal
                         + doc.CodProveedor
                         + doc.TipoDocumento
                         + doc.NroInterno;
            imp.Clave  = clave.PadRight(30);
            imp.Codigo = itemIdx.ToString().PadLeft(6);
            imp.Cambio = doc.TipoCambio;
        }

        // ?? cálculo debe/haber ????????????????????????????????????????????????

        /// <summary>
        /// Determina debe/haber según tipo de documento y columna de imputación de la tasa.
        /// Para Factura/ND ("1","2","B"): columna "D" ? debe, "H" ? haber.
        /// Para NC ("3"): columna "D" ? haber, "H" ? debe (invertido).
        /// </summary>
        private static (decimal Debe, decimal Haber) CalcularDebeHaber(
            decimal importe, string tipoDocumento, string columnaImputa)
        {
            if (EsFacturaODebito(tipoDocumento))
                return columnaImputa == "D" ? (importe, 0m) : (0m, importe);
            else
                return columnaImputa == "D" ? (0m, importe) : (importe, 0m);
        }

        private static bool EsFacturaODebito(string tipoDocumento) =>
            tipoDocumento is "1" or "2" or "B";

        /// <summary>
        /// Extrae los 18 chars de cuenta saltando el prefijo de 2 chars del campo raw.
        /// Equivale a <c>.Substring(2, 18)</c> usado en el VB para <c>par_cuenta</c>, <c>par_ctneto</c>, etc.
        /// </summary>
        private static string ExtraerCuenta(string rawCuenta)
        {
            if (string.IsNullOrEmpty(rawCuenta)) return string.Empty;
            return rawCuenta.Length >= 20 ? rawCuenta.Substring(2, 18) : rawCuenta.TrimStart();
        }

        /// <summary>Devuelve true si la cadena representa un número mayor que cero (mismo criterio que <c>Val(cuenta) &gt; 0</c> en VB).</summary>
        private static bool CuentaConValor(string cuenta)
        {
            if (string.IsNullOrWhiteSpace(cuenta)) return false;
            return decimal.TryParse(cuenta.Trim(), out var val) && val > 0;
        }

        private static void AddParameter(IDbCommand cmd, string name, object? value)
        {
            var p = cmd.CreateParameter();
            p.ParameterName = name;
            p.Value         = value ?? DBNull.Value;
            cmd.Parameters.Add(p);
        }

        private readonly record struct CuentasProveedor(string Contrapartida, string CuentaProveedor);
    }
}
