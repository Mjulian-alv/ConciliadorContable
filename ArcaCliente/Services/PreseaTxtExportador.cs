using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using ArcaCliente.Models;

namespace ArcaCliente.Services
{
    /// <summary>
    /// Genera el archivo TXT delimitado por '|' para importar comprobantes en PRESEA
    /// (Fase 5). Formatea cada campo segun su Tipo/Largo, aplica signo negativo a las Notas
    /// de Credito y respeta el separador decimal y el encoding configurados. Al finalizar,
    /// registra los comprobantes en la memoria anti-duplicado.
    /// </summary>
    public static class PreseaTxtExportador
    {
        /// <summary>
        /// Escribe el archivo y registra los exportados. Retorna la cantidad de lineas escritas.
        /// </summary>
        public static int Exportar(IList<PreseaLineaExport> lineas, string ruta, ConfigPresea cfg, string perfilId)
        {
            cfg ??= new ConfigPresea();
            string sep = string.IsNullOrEmpty(cfg.Separador) ? "|" : cfg.Separador;
            string sepDec = cfg.SeparadorDecimal == "," ? "," : ".";

            var filas = new List<string>(lineas.Count);
            var registros = new List<PreseaComprobanteExportado>(lineas.Count);
            var ahora = DateTime.Now;

            foreach (var l in lineas)
            {
                var csv = l.Csv ?? new ComprobanteCsv();
                int signo = l.EsNotaCredito ? -1 : 1;

                var (r1, n1, i1, r2, n2, i2) = PreseaCalculos.ResolverSlotIva(csv);
                string fecha     = PreseaCalculos.FormatearFecha(csv.FechaEmision, cfg.FormatoFecha);
                string fechaCont = fecha; // contable = emision (por ahora)
                string sucNum    = PreseaCalculos.BuildSucursalNumero(csv.PuntoVenta, csv.NumeroDesde);
                decimal total    = PreseaCalculos.ParseDecimal(csv.ImpTotal);
                decimal ctz      = PreseaCalculos.ParseDecimal(csv.TipoCambio);
                if (ctz == 0m) ctz = 1m;

                string descComprobante = TablaComprobanteDescripcionMapper.ObtenerDescripcion(csv.TipoComprobante);

                var campos = new string[39];
                campos[0]  = Ent(l.CodigoProveedor);                 // 1  Proveedor I/8
                campos[1]  = San(l.CuentaProveedor, 16, sep);        // 2  Cta. ctble. proveedor B/16
                campos[2]  = San(descComprobante, 24, sep);          // 3  Comprobante C/24
                campos[3]  = PreseaCalculos.MapMoneda(csv.Moneda).ToString(CultureInfo.InvariantCulture); // 4 Moneda I/3
                campos[4]  = Num(ctz, 4, sepDec);                    // 5  Cotizacion B/-14,8
                campos[5]  = Ent(l.Provincia);                       // 6  Provincia I/3
                campos[6]  = Ent(l.Condicion);                       // 7  Condicion I/5
                campos[7]  = Num(l.Descuento, 2, sepDec);            // 8  Descuento B/-12,2
                campos[8]  = sucNum;                                 // 9  Sucursal y numero N/14
                campos[9]  = San1(l.Fiscal);                         // 10 Fiscal C/1
                campos[10] = fecha;                                  // 11 Fecha D/8
                campos[11] = fechaCont;                              // 12 Fecha contable D/8
                campos[12] = Digitos(csv.CodAutorizacion);           // 13 CAI/CAE B/14
                campos[13] = PreseaCalculos.FormatearFecha(l.VencimientoCai, cfg.FormatoFecha); // 14 Vto CAI D/8
                campos[14] = San(l.Observacion, 240, sep);          // 15 Observacion M
                campos[15] = San(l.CuentaIVA, 16, sep);             // 16 Cuenta IVA B/16
                campos[16] = Num(r1, 2, sepDec);                    // 17 % IVA N/-6,2
                campos[17] = Num(n1 * signo, 2, sepDec);            // 18 Neto B/-12,2
                campos[18] = Num(i1 * signo, 2, sepDec);            // 19 IVA B/-12,2
                campos[19] = San(l.CuentaIVA2, 16, sep);           // 20 Cuenta IVA 2 B/16
                campos[20] = Num(r2, 2, sepDec);                    // 21 % IVA 2 N/-6,2
                campos[21] = Num(n2 * signo, 2, sepDec);            // 22 Neto 2 B/-12,2
                campos[22] = Num(i2 * signo, 2, sepDec);            // 23 IVA 2 B/-12,2
                campos[23] = Num(total * signo, 2, sepDec);         // 24 Importe B/-12,2
                campos[24] = Num(0m, 2, sepDec);                    // 25 Sobretasa
                campos[25] = Num(0m, 2, sepDec);                    // 26 Percepcion IB
                campos[26] = Num(0m, 2, sepDec);                    // 27 Percepcion IM
                campos[27] = Num(0m, 2, sepDec);                    // 28 Percepcion IV
                campos[28] = Num(0m, 2, sepDec);                    // 29 Percepcion IN
                campos[29] = (cfg.CodigoPercepcionConfig1 ?? string.Empty).Trim(); // 30 Cod percep 1 I/3
                campos[30] = Num(0m, 2, sepDec);                    // 31 Percep 1 importe
                campos[31] = (cfg.CodigoPercepcionConfig2 ?? string.Empty).Trim(); // 32 Cod percep 2 I/3
                campos[32] = Num(0m, 2, sepDec);                    // 33 Percep 2 importe
                campos[33] = Num(0m, 2, sepDec);                    // 34 Impuestos internos
                campos[34] = San(l.CuentaDebe, 16, sep);           // 35 Cuenta del debe B/16
                campos[35] = San(l.Centro, 10, sep);               // 36 Centro C/10
                campos[36] = string.Empty;                          // 37 Lista de precios (param 459)
                campos[37] = string.Empty;                          // 38 Version (param 459)
                campos[38] = San1(l.Imputa);                        // 39 Imputa C/1

                filas.Add(string.Join(sep, campos));

                registros.Add(new PreseaComprobanteExportado
                {
                    Clave            = l.Clave,
                    CuitEmisor       = l.Cuit,
                    TipoCmp          = csv.TipoComprobante ?? string.Empty,
                    PtoVta           = csv.PuntoVenta ?? string.Empty,
                    Nro              = csv.NumeroDesde ?? string.Empty,
                    CodAut           = Digitos(csv.CodAutorizacion),
                    Importe          = total * signo,
                    FechaComprobante = csv.FechaEmision ?? string.Empty,
                    FechaExportacion = ahora,
                    ArchivoGenerado  = ruta,
                    PerfilOfflineId  = perfilId ?? string.Empty,
                });
            }

            File.WriteAllLines(ruta, filas, ObtenerEncoding(cfg.Encoding));
            PreseaExportMemoryStorage.RegistrarRange(registros);

            return filas.Count;
        }

        // ── Formateo por tipo ─────────────────────────────────────────────────────

        /// <summary>Entero: solo digitos, sin ceros a la izquierda. Vacio -> "0".</summary>
        private static string Ent(string s)
        {
            string d = Digitos(s).TrimStart('0');
            return d.Length == 0 ? "0" : d;
        }

        private static string Digitos(string s) =>
            string.IsNullOrEmpty(s) ? string.Empty : new string(s.Where(char.IsDigit).ToArray());

        /// <summary>Numero con signo, cantidad fija de decimales y separador decimal configurable.</summary>
        private static string Num(decimal v, int decimales, string sepDec)
        {
            string s = v.ToString("F" + decimales, CultureInfo.InvariantCulture);
            return sepDec == "," ? s.Replace(".", ",") : s;
        }

        /// <summary>Texto saneado (sin separador ni saltos de linea) y truncado al largo.</summary>
        private static string San(string s, int maxLen, string sep)
        {
            if (string.IsNullOrEmpty(s)) return string.Empty;
            s = s.Replace(sep, " ").Replace("\r", " ").Replace("\n", " ").Trim();
            return s.Length > maxLen ? s.Substring(0, maxLen) : s;
        }

        private static string San1(string s) =>
            string.IsNullOrWhiteSpace(s) ? string.Empty : s.Trim().Substring(0, 1).ToUpperInvariant();

        private static Encoding ObtenerEncoding(string nombre) =>
            nombre?.ToUpperInvariant() switch
            {
                "LATIN-1" or "ISO-8859-1" or "LATIN1" => Encoding.Latin1,
                _ => new UTF8Encoding(encoderShouldEmitUTF8Identifier: false)
            };
    }
}
