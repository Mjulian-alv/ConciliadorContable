using System;
using System.Data;
using System.Threading.Tasks;
using ArcaCliente.Models;
using static ArcaCliente.Services.DbHelpers;

namespace ArcaCliente.Services
{
    /// <summary>
    /// Operaciones de alta y consulta de documentos de Proveedores en Octosis.
    /// Equivale a <c>clsPVConj_Documento</c> + <c>clsPVConjDocumentoTasas</c> en wsmbcg2010.
    /// <para>
    /// Mejoras respecto al código VB original (ver §13.3 del análisis):
    /// <list type="bullet">
    ///   <item>Queries parametrizados — elimina SQL Injection.</item>
    ///   <item><c>NroInternoCore</c> usa <c>UPDLOCK ROWLOCK</c> — elimina la race condition de §10/#2.</item>
    ///   <item><c>AltaAsync</c> envuelve todo en una transacción — rollback completo ante cualquier error.</item>
    ///   <item>Campos de auditoría (<c>idsqlsucur</c>, <c>con_usugra</c>, <c>con_fecgra</c>, <c>con_horgra</c>)
    ///        incluidos explícitamente (antes los agregaba <c>dbfdriver.insertar()</c>).</item>
    /// </list>
    /// </para>
    /// </summary>
    public class OctosisDocumentoService
    {
        // ?? API pública ???????????????????????????????????????????????????????

        /// <summary>
        /// Verifica si un documento ya existe en <c>PV_D5_DOCUM</c> para el proveedor dado.
        /// Equivale a <c>clsPVConj_Documento.documentoExiste()</c>.
        /// No requiere transacción.
        /// </summary>
        /// <param name="provee">Código del proveedor (<c>DOC_PROVEE</c>).</param>
        /// <param name="tipdoc">Tipo de documento (<c>DOC_TIPDOC</c>): "1", "2", "3".</param>
        /// <param name="sucurs">Punto de venta del comprobante (<c>DOC_NROSUC</c>).</param>
        /// <param name="numdoc">Número del comprobante (<c>DOC_NRODOC</c>).</param>
        /// <param name="conn">Conexión abierta.</param>
        /// <param name="tx">Transacción activa; si la conexión ya tiene una transacción pendiente debe pasarse aquí.</param>
        public async Task<bool> ExisteAsync(
            string provee, string tipdoc, string sucurs, string numdoc, IDbConnection conn,
            IDbTransaction? tx = null)
        {
            return await Task.Run(() =>
            {
                using var cmd = conn.CreateCommand();
                cmd.Transaction = tx;
                cmd.CommandText = @"
                    SELECT COUNT(1) FROM pv_d5_docum
                    WHERE doc_provee = @Provee
                      AND doc_tipdoc = @TipDoc
                      AND DOC_NROSUC = @Sucurs
                      AND doc_nrodoc = @NroDoc
                      AND (deleteado IS NULL OR deleteado != 'X')";

                AddParameter(cmd, "@Provee", provee);
                AddParameter(cmd, "@TipDoc", tipdoc);
                AddParameter(cmd, "@Sucurs", sucurs);
                AddParameter(cmd, "@NroDoc",  numdoc);

                return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
            });
        }

        /// <summary>
        /// Genera y reserva el siguiente número interno desde <c>PV_D5_PARAM</c>.
        /// Equivale a <c>clsPVConj_Documento.doc_nroint()</c>, con las siguientes mejoras:
        /// <list type="bullet">
        ///   <item><c>UPDLOCK ROWLOCK</c> — previene la race condition documentada en §10/#2.</item>
        ///   <item>Si la fila no existe la crea (primer documento del período).</item>
        ///   <item>Bucle de colisión — garantiza unicidad aunque haya huecos previos.</item>
        /// </list>
        /// <para>
        /// <b>Debe llamarse dentro de la transacción del llamador.</b>
        /// El lock se libera al hacer commit o rollback de esa transacción.
        /// </para>
        /// </summary>
        /// <param name="tipdoc">Tipo de documento ("1", "2", "3").</param>
        /// <param name="fechaEmision">Fecha del comprobante (se extrae el período AAAAMM).</param>
        /// <param name="sucursal">Código de sucursal (<see cref="OctosisConfig.Sucursal"/>).</param>
        /// <param name="minuta">Valor de <c>DOC_MINUTA</c>; vacío para documentos reales.</param>
        /// <param name="conn">Conexión abierta.</param>
        /// <param name="tx">Transacción activa del llamador.</param>
        /// <returns>Cadena con formato <c>AAAAMM + nroint.PadLeft(5,'0')</c>.</returns>
        public async Task<string> NroInternoAsync(
            string tipdoc, DateTime fechaEmision, string sucursal, string minuta,
            IDbConnection conn, IDbTransaction tx)
        {
            return await Task.Run(() =>
                NroInternoCore(tipdoc, fechaEmision, sucursal, minuta, conn, tx));
        }

        /// <summary>
        /// Da de alta un documento completo (cabecera + tasas + asientos contables) en una única transacción.
        /// Equivale a <c>clsPVConj_Documento.alta()</c>.
        /// </summary>
        /// <param name="doc">Documento a persistir. <c>NroInterno</c> se genera internamente.</param>
        /// <param name="conn">Conexión abierta o cerrada; se abre si está cerrada.</param>
        /// <param name="imputaciones">
        /// Servicio de imputaciones contables. Si se pasa, se generan los asientos
        /// dentro de la misma transacción (PASO 3b-4). Si es <c>null</c>, solo se graban
        /// cabecera y tasas (útil para pruebas o fases de desarrollo anteriores).
        /// </param>
        /// <returns>El <c>NroInterno</c> generado y asignado al documento.</returns>
        /// <exception cref="Exception">Cualquier error de BD provoca rollback completo.</exception>
        public async Task<string> AltaAsync(
            OctosisDocumento doc, IDbConnection conn,
            OctosisImputacionesService? imputaciones = null)
        {
            return await Task.Run(async () =>
            {
                if (conn.State != ConnectionState.Open)
                    conn.Open();

                using var tx = conn.BeginTransaction();
                try
                {
                    // 1. Genera y reserva NroInterno (con UPDLOCK sobre PV_D5_PARAM)
                    string nroInterno = NroInternoCore(
                        doc.TipoDocumento, doc.FecEmision,
                        doc.NumeroSucursal, doc.Minuta,
                        conn, tx);

                    doc.NroInterno = nroInterno;

                    // 2. INSERT cabecera
                    InsertDocumentoCore(doc, conn, tx);

                    // 3. INSERT tasas  (§10/#5: Linea padded a 4 chars)
                    foreach (var tasa in doc.Tasas)
                    {
                        tasa.NroInterno     = nroInterno;
                        tasa.TipoDocumento  = doc.TipoDocumento;
                        tasa.NumeroSucursal = doc.NumeroSucursal;
                        tasa.Linea          = tasa.Linea.PadLeft(4);
                        InsertTasaCore(tasa, conn, tx);
                    }

                    // 4. Asientos contables (si se proporcionó el servicio de imputaciones)
                    if (imputaciones != null)
                        await imputaciones.ImputarAsync(doc, conn, tx);

                    tx.Commit();
                    return nroInterno;
                }
                catch
                {
                    tx.Rollback();
                    throw;
                }
            });
        }

        // ?? núcleo síncrono ???????????????????????????????????????????????????
        // Los métodos privados son síncronos para evitar Task.Run anidados.
        // Se llaman desde dentro del Task.Run de los métodos públicos.

        private static string NroInternoCore(
            string tipdoc, DateTime fechaEmision, string sucursal, string minuta,
            IDbConnection conn, IDbTransaction tx)
        {
            string periodo = fechaEmision.ToString("yyyyMM");
            int nroint;

            // 1. Lee el contador actual con UPDLOCK para bloquear lecturas concurrentes
            using (var cmdRead = conn.CreateCommand())
            {
                cmdRead.Transaction = tx;
                cmdRead.CommandText = @"
                    SELECT PAR_NROINT FROM PV_D5_PARAM WITH (UPDLOCK, ROWLOCK)
                    WHERE par_tipdoc = @TipDoc
                      AND par_period  = @Periodo
                      AND par_sucurs  = @Sucursal";

                AddParameter(cmdRead, "@TipDoc",   tipdoc);
                AddParameter(cmdRead, "@Periodo",  periodo);
                AddParameter(cmdRead, "@Sucursal", sucursal);

                var stored = cmdRead.ExecuteScalar();

                if (stored != null && stored != DBNull.Value)
                {
                    // Fila existente: incrementa y actualiza
                    nroint = Convert.ToInt32(stored) + 1;
                    ActualizarParamCore(tipdoc, periodo, sucursal, nroint, conn, tx);
                }
                else
                {
                    // Primera vez para este tipo/período/sucursal
                    nroint = 1;
                    InsertarParamCore(tipdoc, periodo, sucursal, nroint, conn, tx);
                }
            }

            string nroInterno = periodo + nroint.ToString().PadLeft(5, '0');

            // 2. Bucle de colisión: garantiza que el NroInterno no esté ya usado
            while (NroInternoExisteCore(tipdoc, sucursal, minuta.Trim(), nroInterno, conn, tx))
            {
                nroint++;
                ActualizarParamCore(tipdoc, periodo, sucursal, nroint, conn, tx);
                nroInterno = periodo + nroint.ToString().PadLeft(5, '0');
            }

            return nroInterno;
        }

        private static bool NroInternoExisteCore(
            string tipdoc, string sucursal, string minuta, string nroInterno,
            IDbConnection conn, IDbTransaction tx)
        {
            using var cmd = conn.CreateCommand();
            cmd.Transaction = tx;
            cmd.CommandText = @"
                SELECT COUNT(1) FROM pv_d5_docum
                WHERE (deleteado IS NULL OR deleteado != 'X')
                  AND ISNULL(doc_sucurs,  '') = @Sucursal
                  AND ISNULL(doc_tipdoc,  '') = @TipDoc
                  AND LTRIM(ISNULL(doc_minuta, ' ')) = @Minuta
                  AND ISNULL(doc_nroint,  '') = @NroInterno";

            AddParameter(cmd, "@Sucursal",   sucursal);
            AddParameter(cmd, "@TipDoc",     tipdoc);
            AddParameter(cmd, "@Minuta",     minuta);
            AddParameter(cmd, "@NroInterno", nroInterno);

            return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
        }

        private static void ActualizarParamCore(
            string tipdoc, string periodo, string sucursal, int nroint,
            IDbConnection conn, IDbTransaction tx)
        {
            using var cmd = conn.CreateCommand();
            cmd.Transaction = tx;
            cmd.CommandText = @"
                UPDATE PV_D5_PARAM SET PAR_NROINT = @Nroint
                WHERE par_tipdoc = @TipDoc
                  AND par_period  = @Periodo
                  AND par_sucurs  = @Sucursal";

            AddParameter(cmd, "@Nroint",   nroint);
            AddParameter(cmd, "@TipDoc",   tipdoc);
            AddParameter(cmd, "@Periodo",  periodo);
            AddParameter(cmd, "@Sucursal", sucursal);
            cmd.ExecuteNonQuery();
        }

        private static void InsertarParamCore(
            string tipdoc, string periodo, string sucursal, int nroint,
            IDbConnection conn, IDbTransaction tx)
        {
            using var cmd = conn.CreateCommand();
            cmd.Transaction = tx;
            cmd.CommandText = @"
                INSERT INTO PV_D5_PARAM (PAR_TIPDOC, PAR_PERIOD, PAR_SUCURS, PAR_NROINT)
                VALUES (@TipDoc, @Periodo, @Sucursal, @Nroint)";

            AddParameter(cmd, "@TipDoc",   tipdoc);
            AddParameter(cmd, "@Periodo",  periodo);
            AddParameter(cmd, "@Sucursal", sucursal);
            AddParameter(cmd, "@Nroint",   nroint);
            cmd.ExecuteNonQuery();
        }

        private static void InsertDocumentoCore(
            OctosisDocumento doc, IDbConnection conn, IDbTransaction tx)
        {
            using var cmd = conn.CreateCommand();
            cmd.Transaction = tx;
            cmd.CommandText = @"
                INSERT INTO PV_D5_DOCUM (
                    DOC_TIPDOC, DOC_PROVEE,  DOC_DESCRI,  DOC_NROINT,
                    DOC_FECREC, DOC_FECEMI,
                    DOC_LETRA,  DOC_NRODOC,  DOC_NROSUC,
                    DOC_SUCURS,
                    DOC_IMPAUS, DOC_SALDO,
                    DOC_MINUTA, DOC_MULTIM,  DOC_MEMO1,
                    DOC_FECVTO,
                    DOC_CCITI,  DOC_CAPR,    DOC_CAIVA,   DOC_CAIB,
                    DOC_TIPOIB, DOC_STAFE,
                    DOC_EXEIB,  DOC_EXEGAN,  DOC_EXEIVA,  DOC_EXESFE,  DOC_EXESUS,  DOC_EXEMUN,
                    DOC_CASS,   DOC_CAMUNI,
                    DOC_DOLARE, DOC_CAMBIO,  DOC_IMPDOL,  DOC_SALDOL,
                    DOC_TIPCAN, DOC_TIPDIF,
                    DOC_NROREL, DOC_REEMP,
                    DOC_NROCAI, DOC_CAIVEN,
                    DOC_TIPFAC, DOC_FACIV,
                    DOC_MONEDA, DOC_RUBRO,   DOC_CLASIF,
                    DOC_CODIGO,
                    idsqlsucur, con_usugra,  con_fecgra,  con_horgra
                ) VALUES (
                    @TipoDoc,  @CodProvee,  @Descri,     @NroInterno,
                    @FecRec,   @FecEmi,
                    @Letra,    @NroDoc,     @NroSuc,
                    @Sucurs,
                    @Impaus,   @Saldo,
                    @Minuta,   @MultiMin,   @Memo1,
                    @FecVto,
                    @Cciti,    @Capr,       @Caiva,      @Caib,
                    @TipoIb,   @StaFe,
                    @ExeIb,    @ExeGan,     @ExeIva,     @ExeSfe,     @ExeSus,     @ExeMun,
                    @Cass,     @CaMuni,
                    @Dolare,   @Cambio,     @ImpDol,     @SalDol,
                    @TipCan,   @TipDif,
                    @NroRel,   @Reemp,
                    @NroCai,   @CaiVen,
                    @TipFac,   @FacIv,
                    @Moneda,   @Rubro,      @Clasif,
                    @Codigo,
                    @IdSucur,  @UsuGra,     @FecGra,     @HorGra
                )";

            var ahora = DateTime.Now;

            AddParameter(cmd, "@TipoDoc",   doc.TipoDocumento);
            AddParameter(cmd, "@CodProvee", doc.CodProveedor);
            AddParameter(cmd, "@Descri",    doc.Descripcion);
            AddParameter(cmd, "@NroInterno",doc.NroInterno);
            AddParameter(cmd, "@FecRec",    doc.FecRecepcion);
            AddParameter(cmd, "@FecEmi",    doc.FecEmision);
            AddParameter(cmd, "@Letra",     doc.Letra);
            AddParameter(cmd, "@NroDoc",    doc.NroDoc);
            AddParameter(cmd, "@NroSuc",    doc.NroSuc);
            AddParameter(cmd, "@Sucurs",    doc.NumeroSucursal);
            AddParameter(cmd, "@Impaus",    doc.Importe);
            AddParameter(cmd, "@Saldo",     doc.Saldo);
            AddParameter(cmd, "@Minuta",    doc.Minuta);
            AddParameter(cmd, "@MultiMin",  doc.MultiMinuta);
            AddParameter(cmd, "@Memo1",     doc.Comentario);
            AddParameter(cmd, "@FecVto",    doc.FecVto);
            AddParameter(cmd, "@Cciti",     doc.Citi);
            AddParameter(cmd, "@Capr",      doc.Capr);
            AddParameter(cmd, "@Caiva",     doc.Caiva);
            AddParameter(cmd, "@Caib",      doc.Caib);
            AddParameter(cmd, "@TipoIb",    doc.TipoIibb);
            AddParameter(cmd, "@StaFe",     doc.RetStaFe ? OctosisConfig.MarcaDolaresSi : OctosisConfig.MarcaDolaresNo);
            AddParameter(cmd, "@ExeIb",     doc.ExencionIibb);
            AddParameter(cmd, "@ExeGan",    doc.ExencionGanancias);
            AddParameter(cmd, "@ExeIva",    doc.ExencionIva);
            AddParameter(cmd, "@ExeSfe",    doc.ExencionStafe);
            AddParameter(cmd, "@ExeSus",    doc.ExencionSuss);
            AddParameter(cmd, "@ExeMun",    doc.ExencionMunicipal);
            AddParameter(cmd, "@Cass",      doc.Cass);
            AddParameter(cmd, "@CaMuni",    doc.CaMuni);
            AddParameter(cmd, "@Dolare",    doc.DocumentoEnDolares ? OctosisConfig.MarcaDolaresSi : OctosisConfig.MarcaDolaresNo);
            AddParameter(cmd, "@Cambio",    doc.TipoCambio);
            AddParameter(cmd, "@ImpDol",    doc.ImporteDolares);
            AddParameter(cmd, "@SalDol",    doc.SaldoDolares);
            AddParameter(cmd, "@TipCan",    doc.TipoCancelacion);
            AddParameter(cmd, "@TipDif",    doc.TipoDiferencia);
            AddParameter(cmd, "@NroRel",    doc.NroRelacion);
            AddParameter(cmd, "@Reemp",     doc.IdReemplazo);
            AddParameter(cmd, "@NroCai",    doc.NroCai);
            AddParameter(cmd, "@CaiVen",    doc.VencimientoCai);
            AddParameter(cmd, "@TipFac",    doc.TipoFactura);
            AddParameter(cmd, "@FacIv",     doc.FacturaIv);
            AddParameter(cmd, "@Moneda",    doc.MonedaOrigen);
            AddParameter(cmd, "@Rubro",     doc.Rubro);
            AddParameter(cmd, "@Clasif",    doc.Clasif);
            AddParameter(cmd, "@Codigo",    doc.CodigoFactura);
            // Auditoría — equivale a los campos que dbfdriver.insertar() agregaba automáticamente (§13.3)
            AddParameter(cmd, "@IdSucur",   OctosisConfig.Sucursal);
            AddParameter(cmd, "@UsuGra",    OctosisConfig.UsuarioGrabacion);
            AddParameter(cmd, "@FecGra",    FormatoFechaAuditoria(ahora));
            AddParameter(cmd, "@HorGra",    FormatoHoraAuditoria(ahora));

            cmd.ExecuteNonQuery();
        }

        private static void InsertTasaCore(
            OctosisDocumentoTasa tasa, IDbConnection conn, IDbTransaction tx)
        {
            using var cmd = conn.CreateCommand();
            cmd.Transaction = tx;
            cmd.CommandText = @"
                INSERT INTO PV_P_TASAS0 (
                    TAS_TIPDOC, TAS_SUCURS, TAS_NROINT, TAS_LINEA,
                    TAS_PORCEN, TAS_IMPORT, TAS_NETO,
                    TAS_DOLARE, TAS_CAMBIO, TAS_IMPDOL, TAS_NETDOL,
                    idsqlsucur, con_usugra, con_fecgra, con_horgra
                ) VALUES (
                    @TipoDoc, @Sucurs,  @NroInt,  @Linea,
                    @Porcen,  @Import,  @Neto,
                    @Dolare,  @Cambio,  @ImpDol,  @NetDol,
                    @IdSucur, @UsuGra,  @FecGra,  @HorGra
                )";

            var ahora = DateTime.Now;

            AddParameter(cmd, "@TipoDoc", tasa.TipoDocumento);
            AddParameter(cmd, "@Sucurs",  tasa.NumeroSucursal);
            AddParameter(cmd, "@NroInt",  tasa.NroInterno);
            AddParameter(cmd, "@Linea",   tasa.Linea);
            AddParameter(cmd, "@Porcen",  tasa.Porcentaje);
            AddParameter(cmd, "@Import",  tasa.Importe);
            AddParameter(cmd, "@Neto",    tasa.Neto);
            AddParameter(cmd, "@Dolare",  tasa.DocumentoEnDolares ? OctosisConfig.MarcaDolaresSi : OctosisConfig.MarcaDolaresNo);
            AddParameter(cmd, "@Cambio",  tasa.TipoCambio);
            AddParameter(cmd, "@ImpDol",  tasa.ImporteDolares);
            AddParameter(cmd, "@NetDol",  tasa.NetoDolares);
            // Auditoría
            AddParameter(cmd, "@IdSucur", OctosisConfig.Sucursal);
            AddParameter(cmd, "@UsuGra",  OctosisConfig.UsuarioGrabacion);
            AddParameter(cmd, "@FecGra",  FormatoFechaAuditoria(ahora));
            AddParameter(cmd, "@HorGra",  FormatoHoraAuditoria(ahora));

            cmd.ExecuteNonQuery();
        }

        // ?? helpers ???????????????????????????????????????????????????????????

        /// <summary>
        /// Formato de fecha para <c>con_fecgra</c>: <c>DD/MM/YYYY</c> (igual al VB original).
        /// </summary>
        private static string FormatoFechaAuditoria(DateTime d) =>
            $"{d.Day:D2}/{d.Month:D2}/{d.Year:D4}";

        /// <summary>
        /// Formato de hora para <c>con_horgra</c>: <c>HH:mm:ss</c> (primeros 8 chars de TimeOfDay).
        /// </summary>
        private static string FormatoHoraAuditoria(DateTime d) =>
            d.ToString("HH:mm:ss");

    }
}
