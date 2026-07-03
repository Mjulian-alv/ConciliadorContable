using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using LiquidacionesAuditar.Data;
using LiquidacionesAuditar.Models;

namespace LiquidacionesAuditar.Services
{
    public static class ExportadorLiquidacion
    {
        private static readonly CultureInfo CulturaAR = new CultureInfo("es-AR");

        /// <summary>
        /// Genera el archivo TXT de liquidacion a partir del CSV y la configuracion de la BD.
        /// </summary>
        /// <param name="idMarca">Id de la marca/procesador.</param>
        /// <param name="filas">Filas del archivo origen (CSV o Excel).</param>
        /// <param name="nroBase">Prefijo del número de liquidación cuando se genera por fecha.</param>
        /// <param name="columnaLiquidacion">
        /// Si no es nulo/vacío, los datos se agrupan por el valor de esta columna y ese valor
        /// se usa directamente como número de liquidación (ignora nroBase y fecha).
        /// Si es nulo/vacío, se usa el comportamiento original: agrupar por fecha.
        /// </param>
        public static string Generar(string idMarca, List<Dictionary<string, string>> filas,
                                     string nroBase = "1000", string columnaLiquidacion = null,
                                     char separadorDecimalEntrada = '\0', char separadorMilesEntrada = '\0')
        {
            // Separadores de entrada: por defecto los del sistema
            char sepDec  = separadorDecimalEntrada == '\0'
                           ? System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator[0]
                           : separadorDecimalEntrada;
            char sepMil  = separadorMilesEntrada == '\0'
                           ? System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberGroupSeparator[0]
                           : separadorMilesEntrada;
            var colsDest = Repositorio.GetLiqCols(idMarca)
                           .Where(c => c.TipoRegistro == "CAB" || c.TipoRegistro == "DET" || c.TipoRegistro == "CON")
                           .ToList();

            var colsCAB = colsDest.Where(c => c.TipoRegistro == "CAB").OrderBy(c => c.Orden).ToList();
            var colsDET = colsDest.Where(c => c.TipoRegistro == "DET").OrderBy(c => c.Orden).ToList();
            var colsCON = colsDest.Where(c => c.TipoRegistro == "CON").OrderBy(c => c.Orden).ToList();
            var lineasCON = Repositorio.GetLineasCON(idMarca);

            bool usarColumnaLiq = !string.IsNullOrWhiteSpace(columnaLiquidacion);

            // Validar que la columna existe si se especificó
            if (usarColumnaLiq && filas.Count > 0 && !filas[0].ContainsKey(columnaLiquidacion))
                throw new InvalidOperationException(
                    $"La columna de liquidación '{columnaLiquidacion}' no existe en el archivo. " +
                    $"Columnas disponibles: {string.Join(", ", filas[0].Keys)}");

            // Agrupar: por columna de liquidación o por fecha
            IEnumerable<IGrouping<string, Dictionary<string, string>>> grupos;
            if (usarColumnaLiq)
            {
                grupos = filas
                    .GroupBy(f => f.TryGetValue(columnaLiquidacion, out var v) ? (v ?? "") : "")
                    .OrderBy(g => g.Key);
            }
            else
            {
                grupos = filas
                    .GroupBy(f => ExtraerFecha(f, colsDET).ToString("yyyyMMdd"))
                    .OrderBy(g => g.Key);
            }

            var sb = new StringBuilder();

            foreach (var grupo in grupos)
            {
                string nroLiquidacion;
                DateTime fecha;

                if (usarColumnaLiq)
                {
                    nroLiquidacion = grupo.Key;
                    // Intentar obtener la fecha de la primera fila del grupo para campos de tipo date
                    fecha = ExtraerFecha(grupo.First(), colsCAB);
                }
                else
                {
                    fecha = ExtraerFecha(grupo.First(), colsDET);
                    nroLiquidacion = $"{nroBase}{fecha:ddMMyyyy}";
                }

                // ── CAB ──────────────────────────────────────────────────────
                var linCAB = new List<string> { "CAB" };
                foreach (var col in colsCAB)
                {
                    // Para campos decimal/int del CAB: sumar todas las filas del grupo
                    if ((col.TipoDato.ToLower() == "decimal" || col.TipoDato.ToLower() == "int")
                        && string.IsNullOrWhiteSpace(col.ValorFijo)
                        && !col.IdColumna.Equals("Nro. de Liquidación", StringComparison.OrdinalIgnoreCase))
                    {
                        var relaciones = Repositorio.GetRelacionesConSigno(col.Id);
                        if (col.TipoDato.ToLower() == "decimal")
                        {
                            decimal suma = 0m;
                            foreach (var fila in grupo)
                            {
                                var signoIf = EvaluarCondicion(col.CondicionSigno, fila);
                                foreach (var rel in relaciones)
                                    if (fila.TryGetValue(rel.IdColumnasCSV, out var v))
                                    {
                                        var signoFinal = CombinarSignos(signoIf, rel.Signo);
                                        suma += ParseDecimal(v, sepDec, sepMil, signoFinal);
                                    }
                            }
                            linCAB.Add(FormatDecimal(suma));
                        }
                        else
                        {
                            long suma = 0;
                            foreach (var fila in grupo)
                            {
                                var signoIf = EvaluarCondicion(col.CondicionSigno, fila);
                                foreach (var rel in relaciones)
                                    if (fila.TryGetValue(rel.IdColumnasCSV, out var v))
                                    {
                                        var signoFinal = CombinarSignos(signoIf, rel.Signo);
                                        suma += ParseLong(v, signoFinal);
                                    }
                            }
                            linCAB.Add(suma.ToString());
                        }
                    }
                    else
                    {
                        // Para el resto (valor fijo, nro liquidación, fecha, string): camino normal
                        linCAB.Add(ResolverCampo(col, null, nroLiquidacion, fecha, idMarca, sepDec, sepMil));
                    }
                }
                sb.AppendLine(string.Join("|", linCAB));

                // ── DET ──────────────────────────────────────────────────────
                foreach (var fila in grupo)
                {
                    var linDET = new List<string> { "DET" };
                    foreach (var col in colsDET)
                        linDET.Add(ResolverCampo(col, fila, nroLiquidacion, fecha, idMarca, sepDec, sepMil));
                    sb.AppendLine(string.Join("|", linDET));
                }

                // ── CON ──────────────────────────────────────────────────────
                foreach (var lineaCon in lineasCON)
                {
                    var colsSuma = Repositorio.GetCONCols(lineaCon.Id);
                    var condicion = Repositorio.GetCONCondicion(lineaCon.Id);
                    decimal total = 0m;
                    foreach (var fila in grupo)
                        foreach (var colSuma in colsSuma)
                            if (fila.TryGetValue(colSuma, out var val))
                            {
                                var signo = EvaluarCondicion(condicion, fila);
                                total += ParseDecimal(val, sepDec, sepMil, signo);
                            }

                    var valosFijos = Repositorio.GetCONValoresFijos(lineaCon.Id)
                                    .OrderBy(v => v.Posicion).ToList();

                    // Estructura: CON|nroLiq|monto|etiquetas...
                    var linCON = new List<string> { "CON", nroLiquidacion,
                        FormatDecimal(total), lineaCon.Descripcion, "Neto" };
                    foreach (var vf in valosFijos)
                        linCON.Add(vf.Valor);

                    sb.AppendLine(string.Join("|", linCON));
                }
            }

            return sb.ToString();
        }

        private static string ResolverCampo(LiqCol col, Dictionary<string, string> fila,
                                            string nroLiq, DateTime fecha, string idMarca,
                                            char sepDec, char sepMil)
        {
            // Condición IF antes que todo: COLUMNA==VALOR|RESULTADO_SI|RESULTADO_NO
            if (!string.IsNullOrWhiteSpace(col.Condicion) && fila != null)
            {
                var resultado = EvaluarCondicion(col.Condicion, fila);
                return resultado;
            }

            // Valor fijo explícito
            if (!string.IsNullOrWhiteSpace(col.ValorFijo))
                return col.ValorFijo;

            // Numero de liquidacion
            if (col.IdColumna.Equals("Nro. de Liquidación", StringComparison.OrdinalIgnoreCase))
                return nroLiq;

            // Fecha formateada
            if (col.IdColumna.Equals("Fecha de Pago", StringComparison.OrdinalIgnoreCase))
                return fecha.ToString("dd/MM/yyyy");

            if (fila == null) return "";

            var relaciones = Repositorio.GetRelaciones(col.Id);
            if (relaciones.Count == 0) return "";

            // Segun tipo dato: sum numericos, concat strings
            if (col.TipoDato.ToLower() == "decimal")
            {
                decimal suma = 0m;
                foreach (var r in relaciones)
                    if (fila.TryGetValue(r, out var v))
                    {                        
                        var signo = EvaluarCondicion(col.CondicionSigno, fila);
                        suma += ParseDecimal(v, sepDec, sepMil, signo);
                    }
                return FormatDecimal(suma);
            }
            else if (col.TipoDato.ToLower() == "int")
            {
                long suma = 0;
                foreach (var r in relaciones)
                    if (fila.TryGetValue(r, out var v))
                    {
                        var signo = EvaluarCondicion(col.CondicionSigno, fila);
                        suma += ParseLong(v, signo);
                    }
                return suma.ToString();
            }
            else if (col.TipoDato.ToLower() == "date")
            {
                foreach (var r in relaciones)
                    if (fila.TryGetValue(r, out var v) && !string.IsNullOrWhiteSpace(v))
                        return ExtraerFechaStr(v);
                return "";
            }
            else
            {
                // string: concatenar
                var partes = new List<string>();
                foreach (var r in relaciones)
                    if (fila.TryGetValue(r, out var v) && !string.IsNullOrWhiteSpace(v))
                        partes.Add(v.Trim());
                return string.Join(" ", partes);
            }
        }

        /// <summary>
        /// Evalúa una condición en formato: COLUMNA_CSV OPERADOR VALOR|RESULTADO_SI|RESULTADO_NO
        /// Operadores soportados: == != contains startswith
        /// Ejemplo: MONEDA==ARS|Pesos|Dólares
        /// Retorna null si el formato es inválido (para que el llamador siga con la lógica normal).
        /// </summary>
        private static string EvaluarCondicion(string condicion, Dictionary<string, string> fila)
        {
            // Separar la expresión de los resultados: "EXPR|SI|NO"
            var partes = condicion.Split('|');
            if (partes.Length != 3) return null;

            var expresion  = partes[0].Trim();
            var resultadoSi = partes[1];
            var resultadoNo = partes[2];

            // Detectar operador
            string[] operadores = { "contains", "startswith", "==", "!=" };
            string opEncontrado = null;
            foreach (var op in operadores)
                if (expresion.IndexOf(op, StringComparison.OrdinalIgnoreCase) >= 0)
                { opEncontrado = op; break; }

            if (opEncontrado == null) return null;

            var idx = expresion.IndexOf(opEncontrado, StringComparison.OrdinalIgnoreCase);
            var columna = expresion.Substring(0, idx).Trim();
            var valorEsperado = expresion.Substring(idx + opEncontrado.Length).Trim();

            if (!fila.TryGetValue(columna, out var valorReal))
                valorReal = "";

            bool cumple = opEncontrado switch
            {
                "=="         => string.Equals(valorReal.Trim(), valorEsperado, StringComparison.OrdinalIgnoreCase),
                "!="         => !string.Equals(valorReal.Trim(), valorEsperado, StringComparison.OrdinalIgnoreCase),
                "contains"   => valorReal.IndexOf(valorEsperado, StringComparison.OrdinalIgnoreCase) >= 0,
                "startswith" => valorReal.StartsWith(valorEsperado, StringComparison.OrdinalIgnoreCase),
                _            => false
            };

            return cumple ? resultadoSi : resultadoNo;
        }

        private static DateTime ExtraerFecha(Dictionary<string, string> fila, List<LiqCol> colsDET)
        {
            // Buscar columna de tipo date en los DET
            var colFecha = colsDET.FirstOrDefault(c => c.TipoDato.ToLower() == "date");

            if (colFecha != null)
            {
                var rels = Repositorio.GetRelaciones(colFecha.Id);
                foreach (var r in rels)
                    if (fila.TryGetValue(r, out var val) && !string.IsNullOrWhiteSpace(val))
                    {
                        val = val.Trim().Replace("hs", "");

                        if (DateTime.TryParse(val, out var dt))
                            return dt.Date;

                    }
            }
            return DateTime.Today;
        }

        private static string ExtraerFechaStr(string val)
        {
            if (DateTime.TryParse(val, out var dt))
                return dt.ToString("dd/MM/yyyy");
            return val;
        }

        /// <summary>Formatea un decimal para el archivo de salida: coma decimal, sin separador de miles.</summary>
        private static string FormatDecimal(decimal v) =>
            v.ToString("N2", CulturaAR).Replace(CulturaAR.NumberFormat.NumberGroupSeparator, "");

        private static decimal ParseDecimal(string val, char sepDec, char sepMil, string signo = "")
        {
            if (string.IsNullOrWhiteSpace(val)) return 0m;
            val = val.Trim();
            // Quitar separador de miles y normalizar decimal a punto
            //if (sepMil != '\0' && sepMil != '.') val = val.Replace(sepMil.ToString(), "");
            //if (sepDec != '\0' && sepDec != '.') val = val.Replace(sepDec.ToString(), ".");
            val = val.Replace(sepMil.ToString(), "");
            val = val.Replace(sepDec.ToString(), ".");
            if (!string.IsNullOrWhiteSpace(signo) && signo.Equals("-", StringComparison.OrdinalIgnoreCase))
                val = "-" + val;
            return decimal.TryParse(val, NumberStyles.Any, CultureInfo.InvariantCulture, out var d) ? d : 0m;
        }

        private static long ParseLong(string val, string signo = "")
        {
            if (string.IsNullOrWhiteSpace(val)) return 0;
            val = val.Trim();
            if (!string.IsNullOrWhiteSpace(signo) && signo.Equals("-", StringComparison.OrdinalIgnoreCase))
                val = "-" + val;
            return long.TryParse(val, out var l) ? l : 0;
        }

        /// <summary>
        /// Combina el signo del IF por fila con el signo fijo de la relación (XOR de negativos).
        /// Negativo solo si exactamente uno de los dos es "-". Cualquier otro valor cuenta como "+".
        /// </summary>
        private static string CombinarSignos(string signoIf, string signoRel)
        {
            bool negIf  = signoIf  != null && signoIf.Trim()  == "-";
            bool negRel = signoRel != null && signoRel.Trim() == "-";
            return (negIf ^ negRel) ? "-" : "+";
        }
    }
}
