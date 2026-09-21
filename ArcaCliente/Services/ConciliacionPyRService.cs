// Fecha: 21/09/2026 - TAREA: 00041 - Linea: 7 - Conciliación de percepciones y retenciones: ARCA contra los mayores de PRESEA
// Reglas (guía docs/Diseño/00041-conciliacion-pyr, "Segunda etapa"):
//  1. Alcance: sólo los grupos (impuesto, tipo) de las cuentas con mayor cargado. Pedido del
//     usuario: "solo debe conciliar contra las cuentas subidas en el archivo de PRESEA". El resto
//     de ARCA no aparece como faltante, sólo se cuenta.
//  2. ARCA con importe 0 (notas de crédito de IIBB sin percepción) se excluye y se cuenta.
//  3. Anulaciones de PRESEA que suman cero con su factura se neutralizan antes de conciliar.
//  4. Directivas en orden, uno a uno, importe exacto al centavo.
// Sin estado ni acceso a datos: recibe lo que la pantalla ya tiene en memoria.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using ArcaCliente.Models;

namespace ArcaCliente.Services
{
    public static class ConciliacionPyRService
    {
        public static ResultadoConciliacionPyR Conciliar(
            IEnumerable<RegistroArcaPyR> registrosArca,
            IEnumerable<MayorPreseaPyR> mayores,
            IReadOnlyList<DirectivaPyR> directivas)
        {
            if (directivas == null || directivas.Count == 0)
                directivas = DirectivaPyR.CrearPredeterminadas();

            var resultado = new ResultadoConciliacionPyR();
            var listaMayores = mayores.ToList();

            // ── 1. Grupos del alcance ────────────────────────────────────────────
            var grupos = listaMayores
                .GroupBy(m => (m.Cuenta.Impuesto, m.Cuenta.Tipo))
                .ToDictionary(g => g.Key, g => g.ToList());

            var arcaPorGrupo = grupos.Keys.ToDictionary(k => k, _ => new List<RegistroArcaPyR>());
            foreach (var a in registrosArca)
            {
                var clave = (a.Impuesto, a.Operacion);
                if (!arcaPorGrupo.TryGetValue(clave, out var lista))
                {
                    var texto = $"{CuentaPyR.NombreImpuesto(a.Impuesto)} · {CuentaPyR.NombreTipo(a.Operacion)}";
                    resultado.FueraDeAlcance[texto] = resultado.FueraDeAlcance.GetValueOrDefault(texto) + 1;
                    continue;
                }
                // ── 2. Importe cero ──────────────────────────────────────────────
                if (a.Importe == 0m) { resultado.ArcaImporteCero++; continue; }
                lista.Add(a);
            }

            foreach (var (clave, mayoresGrupo) in grupos)
            {
                // Los "Sólo ARCA" de un grupo con varias cuentas se asignan a la primera.
                var cuentaGrupo = mayoresGrupo[0].Cuenta;

                // ── 3. Anulaciones ───────────────────────────────────────────────
                var anuladas = new HashSet<RegistroPreseaPyR>();
                foreach (var mayor in mayoresGrupo)
                {
                    foreach (var anul in mayor.Registros.Where(r => r.EsAnulacion))
                    {
                        var original = mayor.Registros.FirstOrDefault(r =>
                            !r.EsAnulacion && !anuladas.Contains(r) && !r.SinComprobante
                            && r.Numero == anul.Numero
                            && string.Equals(r.TipoComprobante, anul.TipoComprobante, StringComparison.OrdinalIgnoreCase)
                            && r.Importe + anul.Importe == 0m);
                        if (original == null) continue;   // sin par: la anulación concilia como una fila más
                        anuladas.Add(original);
                        anuladas.Add(anul);
                    }
                    foreach (var r in mayor.Registros.Where(anuladas.Contains))
                        resultado.Items.Add(new ItemConciliacionPyR
                        {
                            Estado = EstadoConciliacionPyR.AnuladaPresea, Cuenta = mayor.Cuenta, Presea = r
                        });
                }

                // ── 4. Directivas ────────────────────────────────────────────────
                var arcaPendiente   = arcaPorGrupo[clave];
                var preseaPendiente = mayoresGrupo.SelectMany(m => m.Registros).Where(r => !anuladas.Contains(r)).ToList();

                for (int d = 0; d < directivas.Count; d++)
                {
                    var campos = directivas[d].Campos;
                    if (campos.Count == 0) continue;

                    // Si hay claves repetidas, gana el primero libre (mismo criterio que Offline).
                    var indice = new Dictionary<string, RegistroPreseaPyR>(StringComparer.Ordinal);
                    foreach (var p in preseaPendiente)
                    {
                        var k = Clave(campos, p.Numero, p.Importe, p.Fecha, p.Proveedor);
                        if (k != null) indice.TryAdd(k, p);
                    }

                    var restoArca = new List<RegistroArcaPyR>();
                    var usados = new HashSet<RegistroPreseaPyR>();
                    foreach (var a in arcaPendiente)
                    {
                        var k = Clave(campos, a.Numero, a.Importe, a.Fecha, a.Denominacion);
                        if (k == null || !indice.Remove(k, out var p)) { restoArca.Add(a); continue; }

                        usados.Add(p);
                        bool iguales = a.Importe == p.Importe;
                        resultado.Items.Add(new ItemConciliacionPyR
                        {
                            Estado     = iguales ? EstadoConciliacionPyR.Conciliado : EstadoConciliacionPyR.DiferenciaImporte,
                            Cuenta     = p.Cuenta,
                            Directiva  = d + 1,
                            DescripcionDirectiva = $"{d + 1}-{directivas[d].Descripcion}",
                            Arca       = a,
                            Presea     = p,
                            Diferencia = iguales ? null : a.Importe - p.Importe
                        });
                        resultado.PorDirectiva[d + 1] = resultado.PorDirectiva.GetValueOrDefault(d + 1) + 1;
                    }

                    arcaPendiente   = restoArca;
                    preseaPendiente = preseaPendiente.Where(p => !usados.Contains(p)).ToList();
                }

                // ── 5. Sin par ───────────────────────────────────────────────────
                resultado.Items.AddRange(arcaPendiente.Select(a => new ItemConciliacionPyR
                {
                    Estado = EstadoConciliacionPyR.SoloArca, Cuenta = cuentaGrupo, Arca = a
                }));
                resultado.Items.AddRange(preseaPendiente.Select(p => new ItemConciliacionPyR
                {
                    Estado = EstadoConciliacionPyR.SoloPresea, Cuenta = p.Cuenta, Presea = p
                }));
            }

            return resultado;
        }

        // ── Claves ───────────────────────────────────────────────────────────────

        /// <summary>
        /// Clave de una fila para los campos de la directiva, o null si le falta algún valor:
        /// dos vacíos no son una coincidencia (una minuta sin número no empareja por número).
        /// </summary>
        private static string Clave(IEnumerable<CampoPyR> campos, string numero, decimal importe, DateTime? fecha, string proveedor)
        {
            var partes = new List<string>();
            foreach (var c in campos.Distinct().OrderBy(c => c))
            {
                string v = c switch
                {
                    CampoPyR.Numero         => string.IsNullOrEmpty(numero) ? null : numero,
                    CampoPyR.NumeroUltimos8 => string.IsNullOrEmpty(numero) ? null : Ultimos8(numero),
                    CampoPyR.Importe        => importe.ToString("F2", CultureInfo.InvariantCulture),
                    CampoPyR.Fecha          => fecha?.ToString("yyyyMMdd"),
                    CampoPyR.Proveedor      => ProveedorClave(proveedor),
                    _                       => null
                };
                if (string.IsNullOrEmpty(v)) return null;
                partes.Add(v);
            }
            return string.Join("|", partes);
        }

        /// <summary>"10200315520" y "315520" → "00315520".</summary>
        public static string Ultimos8(string numero) =>
            numero.Length >= 8 ? numero[^8..] : numero.PadLeft(8, '0');

        /// <summary>
        /// Primeras 6 letras o dígitos, sin tildes, espacios ni puntos, en mayúsculas: PRESEA corta
        /// el proveedor a 16 caracteres y lo escribe distinto ("DREAMCO S.C.A." contra "DREAMCO S. A.").
        /// </summary>
        public static string ProveedorClave(string nombre)
        {
            if (string.IsNullOrWhiteSpace(nombre)) return null;
            var sb = new StringBuilder(6);
            foreach (char ch in nombre.Normalize(NormalizationForm.FormD))
            {
                if (CharUnicodeInfo.GetUnicodeCategory(ch) == UnicodeCategory.NonSpacingMark) continue;
                if (ch < 128 && char.IsLetterOrDigit(ch)) sb.Append(char.ToUpperInvariant(ch));
                if (sb.Length == 6) break;
            }
            return sb.Length == 0 ? null : sb.ToString();
        }
    }
}
