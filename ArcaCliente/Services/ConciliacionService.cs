using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using ArcaCliente.Models;
using static ArcaCliente.Services.DbHelpers;

namespace ArcaCliente.Services
{
    public class ConciliacionService
    {
        private readonly Func<IDbConnection> _connectionFactory;
        private readonly string _query;
        private readonly bool _usarEquivalenciaOnline;

        public ConciliacionService(Func<IDbConnection> connectionFactory, string query, bool usarEquivalenciaOnline = false)
        {
            _connectionFactory = connectionFactory
                ?? throw new InvalidOperationException(
                    "Configure ConciliacionConfig.ConnectionFactory con su proveedor de base de datos.");
            _query = query;
            _usarEquivalenciaOnline = usarEquivalenciaOnline;
        }

        /// <summary>
        /// Ejecuta el query contra la base local y retorna los comprobantes del período.
        /// </summary>
        public async Task<List<ComprobanteLocal>> ObtenerComprobantesLocalesAsync(
            DateTime fechaDesde, DateTime fechaHasta)
        {
            return await Task.Run(() =>
            {
                var result = new List<ComprobanteLocal>();

                using var conn = _connectionFactory();
                conn.Open();

                using var cmd = conn.CreateCommand();
                cmd.CommandText = _query;
                cmd.CommandTimeout = 30;
                AddParameter(cmd, "@FechaDesde", fechaDesde.Date);
                AddParameter(cmd, "@FechaHasta", fechaHasta.Date);

                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    var comprobanteLocal = new ComprobanteLocal
                    {
                        Fecha = Convert.ToDateTime(reader["Fecha"]),
                        PuntoVenta = Normalizar(reader["PuntoVenta"]?.ToString(), 5),
                        Numero = Normalizar(reader["Numero"]?.ToString(), 8),
                        Total = Convert.ToDecimal(reader["Total"])
                    };

                    // Intentar leer campos opcionales que son necesarios para ciertas directivas
                    if (HasColumn(reader, "Cuit"))
                        comprobanteLocal.Cuit = reader["Cuit"]?.ToString();

                    if (HasColumn(reader, "TipoComprobante"))
                        comprobanteLocal.TipoComprobante = reader["TipoComprobante"]?.ToString();

                    if (HasColumn(reader, "nombre"))
                        comprobanteLocal.NombreProveedor = reader["nombre"]?.ToString();

                    if (HasColumn(reader, "NombreProveedor"))
                        comprobanteLocal.NombreProveedor = reader["NombreProveedor"]?.ToString();

                    result.Add(comprobanteLocal);
                }

                return result;
            });
        }

        /// <summary>
        /// Compara los comprobantes de ARCA con los del sistema local usando la directiva
        /// primaria por defecto (CUIT + Tipo + Pto. Venta + Número).
        /// </summary>
        public List<ItemConciliacion> Conciliar(
            IEnumerable<ComprobanteCsv> comprobantesArca,
            IEnumerable<ComprobanteLocal> comprobantesLocales)
            => Conciliar(comprobantesArca, comprobantesLocales,
                new[] { DirectivaConciliacion.CrearPrimaria() });

        /// <summary>
        /// Compara los comprobantes de ARCA con los del sistema local aplicando
        /// las directivas de conciliación en orden.  Cada directiva opera sobre
        /// el residual (registros no emparejados) de la directiva anterior.
        /// </summary>
        public List<ItemConciliacion> Conciliar(
            IEnumerable<ComprobanteCsv> comprobantesArca,
            IEnumerable<ComprobanteLocal> comprobantesLocales,
            IReadOnlyList<DirectivaConciliacion> directivas)
        {
            if (directivas == null || directivas.Count == 0)
                directivas = new[] { DirectivaConciliacion.CrearPrimaria() };

            var resultado = new List<ItemConciliacion>();
            var arcaPendientes = comprobantesArca.ToList();
            var localesDisponibles = comprobantesLocales.ToList();
            var localesUsados = new HashSet<int>();

            for (int d = 0; d < directivas.Count; d++)
            {
                var directiva = directivas[d];
                var numDirectiva = d + 1;

                var localIndex = new Dictionary<string, (int idx, ComprobanteLocal local)>(
                    StringComparer.OrdinalIgnoreCase);

                for (int i = 0; i < localesDisponibles.Count; i++)
                {
                    if (localesUsados.Contains(i)) continue;
                    var key = BuildKeyLocal(localesDisponibles[i], directiva.Campos);
                    if (!localIndex.ContainsKey(key))
                        localIndex[key] = (i, localesDisponibles[i]);
                }

                var arcaRestante = new List<ComprobanteCsv>();

                foreach (var arca in arcaPendientes)
                {
                    var key = BuildKeyArca(arca, directiva.Campos);
                    var totalArca = ParseImporte(arca.ImpTotal);

                    if (localIndex.TryGetValue(key, out var match))
                    {
                        localesUsados.Add(match.idx);
                        localIndex.Remove(key);

                        var diferencia = totalArca - match.local.Total;
                        resultado.Add(new ItemConciliacion
                        {
                            Estado = Math.Abs(diferencia) < 0.01m
                                                           ? EstadoConciliacion.Conciliado
                                                           : EstadoConciliacion.DiferenciaImporte,
                            DirectivaAplicada = numDirectiva,
                            DescripcionDirectiva = numDirectiva + "-" + directiva.Descripcion,
                            DetalleMatcheo = BuildDetalleMatcheo(arca, directiva.Campos),
                            // Para directiva 1 no hay previas; a partir de la 2 se muestran
                            // las diferencias de campo entre ARCA y el local que sí matcheó
                            FalloDirectivas = d == 0
                                                           ? string.Empty
                                                           : BuildDiffDirectivasPrevias(arca, match.local, directivas, d),
                            Fecha = arca.FechaEmision,
                            TipoComprobante = arca.TipoComprobante,
                            DescripcionTipoComprobante = TablaComprobanteDescripcionMapper.ObtenerDescripcion(arca.TipoComprobante),
                            PuntoVenta = arca.PuntoVenta,
                            Numero = arca.NumeroDesde,
                            Emisor = arca.DenominacionEmisor,
                            CuitProveedor = arca.NroDocEmisor,
                            NombreProveedor = arca.DenominacionEmisor,
                            TotalARCA = totalArca.ToString("F2"),
                            TotalSistema = match.local.Total.ToString("F2"),
                            Diferencia = diferencia != 0m ? diferencia.ToString("F2") : string.Empty
                        });
                    }
                    else
                    {
                        arcaRestante.Add(arca);
                    }
                }

                arcaPendientes = arcaRestante;
            }

            // Registros ARCA sin emparejar: no hay local con quien comparar,
            // se muestran las claves que se intentaron en cada directiva
            foreach (var arca in arcaPendientes)
            {
                var clavesTentadas = string.Join("\n", directivas.Select(
                    (dir, i) => $"✗ Dir.{i + 1}: {BuildDetalleMatcheo(arca, dir.Campos)}"));

                var totalArca = ParseImporte(arca.ImpTotal);
                resultado.Add(new ItemConciliacion
                {
                    Estado = EstadoConciliacion.SoloARCA,
                    DirectivaAplicada = 0,
                    FalloDirectivas = clavesTentadas,
                    Fecha = arca.FechaEmision,
                    TipoComprobante = arca.TipoComprobante,
                    DescripcionTipoComprobante = TablaComprobanteDescripcionMapper.ObtenerDescripcion(arca.TipoComprobante),
                    PuntoVenta = arca.PuntoVenta,
                    Numero = arca.NumeroDesde,
                    Emisor = arca.DenominacionEmisor,
                    CuitProveedor = arca.NroDocEmisor,
                    NombreProveedor = arca.DenominacionEmisor,
                    TotalARCA = totalArca.ToString("F2"),
                    TotalSistema = string.Empty,
                    Diferencia = string.Empty,
                    SourceArca = arca
                });
            }

            // Registros locales sin emparejar
            for (int i = 0; i < localesDisponibles.Count; i++)
            {
                if (localesUsados.Contains(i)) continue;
                var local = localesDisponibles[i];
                resultado.Add(new ItemConciliacion
                {
                    Estado = EstadoConciliacion.SoloSistema,
                    DirectivaAplicada = 0,
                    Fecha = local.Fecha.ToString("dd/MM/yyyy"),
                    TipoComprobante = local.TipoComprobante,
                    DescripcionTipoComprobante = TablaComprobanteDescripcionMapper.ObtenerDescripcion(local.TipoComprobante),
                    PuntoVenta = local.PuntoVenta,
                    Numero = local.Numero,
                    Emisor = local.NombreProveedor,
                    CuitProveedor = local.Cuit,
                    NombreProveedor = local.NombreProveedor,
                    TotalARCA = string.Empty,
                    TotalSistema = local.Total.ToString("N2"),
                    Diferencia = string.Empty
                });
            }

            return resultado;
        }

        /// <summary>
        /// Agrupa los comprobantes de ARCA y del sistema local por CUIT y compara los totales.
        /// Las Notas de Crédito (TipoOctosis = "3") se suman con signo negativo en ambos lados.
        /// </summary>
        public static List<ItemConciliacionXCuit> ConciliarPorTotalesXCuit(
            IEnumerable<ComprobanteCsv> comprobantesArca,
            IEnumerable<ComprobanteLocal> comprobantesLocales)
        {
            var arcaPorCuit = comprobantesArca
                .GroupBy(a => NormalizarCuit(a.NroDocEmisor))
                .Where(g => g.Key.Length > 0)
                .ToDictionary(
                    g => g.Key,
                    g => (
                        Total: g.Sum(a => SignoArca(a) * ParseImporte(a.ImpTotal)),
                        Cant: g.Count(),
                        Nombre: g.First().DenominacionEmisor ?? string.Empty
                    ));

            var localPorCuit = comprobantesLocales
                .GroupBy(l => NormalizarCuit(l.Cuit))
                .Where(g => g.Key.Length > 0)
                .ToDictionary(
                    g => g.Key,
                    g => (
                        Total: g.Sum(l => SignoLocal(l) * l.Total),
                        Cant: g.Count(),
                        Nombre: g.First().NombreProveedor ?? string.Empty
                    ));

            var todosLosCuits = arcaPorCuit.Keys.Union(localPorCuit.Keys);

            return todosLosCuits
                .Select(cuit =>
                {
                    arcaPorCuit.TryGetValue(cuit, out var arca);
                    localPorCuit.TryGetValue(cuit, out var local);
                    return new ItemConciliacionXCuit
                    {
                        CuitProveedor = cuit,
                        NombreProveedor = !string.IsNullOrWhiteSpace(arca.Nombre) ? arca.Nombre : local.Nombre,
                        TotalARCA = arca.Total,
                        TotalSistema = local.Total,
                        CantARCA = arca.Cant,
                        CantSistema = local.Cant
                    };
                })
                .OrderBy(x => x.NombreProveedor)
                .ToList();
        }

        private static decimal SignoArca(ComprobanteCsv c)
        {
            var parsed = TipoComprobanteMapper.Parse((c.TipoComprobante ?? string.Empty).PadLeft(3, '0'));
            return parsed.TipoOctosis == "3" ? -1m : 1m;
        }

        private static decimal SignoLocal(ComprobanteLocal l)
        {
            var parsed = TipoComprobanteMapper.Parse((l.TipoComprobante ?? string.Empty).PadLeft(3, '0'));
            return parsed.TipoOctosis == "3" ? -1m : 1m;
        }

        /// <summary>
        /// Construye un texto legible con los valores normalizados usados como clave de matcheo.
        /// Ejemplo: "CUIT=20332965523 | Tipo=1 | Pto.Vta.=00004 | Núm.=00000686"
        /// </summary>
        private string BuildDetalleMatcheo(ComprobanteCsv arca, IEnumerable<CampoConciliacion> campos)
        {
            var parts = campos.Select(c => c switch
            {
                CampoConciliacion.Cuit => $"CUIT={NormalizarCuit(arca.NroDocEmisor)}",
                CampoConciliacion.TipoComprobante => $"Tipo={NormalizarTipo(arca.TipoComprobante, _usarEquivalenciaOnline)}",
                CampoConciliacion.PuntoVenta => $"Pto.Vta.={Normalizar(arca.PuntoVenta, 5)}",
                CampoConciliacion.Numero => $"Núm.={Normalizar(arca.NumeroDesde, 8)}",
                CampoConciliacion.Fecha => $"Fecha={NormalizarFechaArca(arca.FechaEmision)}",
                _ => string.Empty
            }).Where(p => p.Length > 0);
            return string.Join(" | ", parts);
        }

        /// <summary>
        /// Para un registro emparejado en la directiva <paramref name="matchedAt"/>,
        /// compara campo a campo los valores normalizados de ARCA vs local para cada
        /// directiva anterior que no encontró coincidencia, explicando la causa.
        /// Ejemplo de línea: "Dir.1: CUIT ARCA=20332965523 ≠ Sist.=20332965520"
        /// </summary>
        private string BuildDiffDirectivasPrevias(
            ComprobanteCsv arca,
            ComprobanteLocal local,
            IReadOnlyList<DirectivaConciliacion> directivas,
            int matchedAt)
        {
            var lines = new List<string>(matchedAt);
            for (int i = 0; i < matchedAt; i++)
            {
                var diffs = directivas[i].Campos
                    .Select(c =>
                    {
                        var av = ValorNormalizadoArca(arca, c);
                        var lv = ValorNormalizadoLocal(local, c);
                        var nombreproveedorlocal = DirectivaConciliacion.NombreCampo(c) == "CUIT" ? local.NombreProveedor : "";
                        var nombreproveedor = DirectivaConciliacion.NombreCampo(c) == "CUIT" ? arca.DenominacionEmisor : "";

                        return string.Equals(av, lv, StringComparison.OrdinalIgnoreCase)
                            ? null
                            : $"{DirectivaConciliacion.NombreCampo(c)}: ARCA={av} {nombreproveedor} ≠  Sist.={lv}  {nombreproveedorlocal}";
                    })
                    .Where(s => s != null);

                lines.Add($"Dir.{i + 1}: " + string.Join(" | ", diffs));
            }
            return string.Join("\n", lines);
        }

        private string ValorNormalizadoArca(ComprobanteCsv arca, CampoConciliacion campo) => campo switch
        {
            CampoConciliacion.Cuit => NormalizarCuit(arca.NroDocEmisor),
            CampoConciliacion.TipoComprobante => NormalizarTipo(arca.TipoComprobante, _usarEquivalenciaOnline),
            CampoConciliacion.PuntoVenta => Normalizar(arca.PuntoVenta, 5),
            CampoConciliacion.Numero => Normalizar(arca.NumeroDesde, 8),
            CampoConciliacion.Fecha => NormalizarFechaArca(arca.FechaEmision),
            CampoConciliacion.Total => Normalizar(ParseImporte(arca.ImpTotal).ToString(), 18),
            _ => string.Empty
        };

        private static string ValorNormalizadoLocal(ComprobanteLocal local, CampoConciliacion campo) => campo switch
        {
            CampoConciliacion.Cuit => NormalizarCuit(local.Cuit),
            CampoConciliacion.TipoComprobante => NormalizarTipo(local.TipoComprobante, false),
            CampoConciliacion.PuntoVenta => Normalizar(local.PuntoVenta, 5),
            CampoConciliacion.Numero => Normalizar(local.Numero, 8),
            CampoConciliacion.Fecha => local.Fecha.ToString("yyyy-MM-dd"),
            CampoConciliacion.Total => Normalizar(local.Total.ToString(), 18),
            _ => string.Empty
        };

        private string BuildKeyArca(ComprobanteCsv arca, IEnumerable<CampoConciliacion> campos) =>
            string.Join("|", campos.Select(c => ValorNormalizadoArca(arca, c)));

        private static string BuildKeyLocal(ComprobanteLocal local, IEnumerable<CampoConciliacion> campos) =>
            string.Join("|", campos.Select(c => ValorNormalizadoLocal(local, c)));

        private static string NormalizarFechaArca(string fecha)
        {
            if (string.IsNullOrWhiteSpace(fecha)) return string.Empty;
            // Formato actual de los CSV de ARCA
            if (DateTime.TryParseExact(fecha, "yyyy-MM-dd",
                    CultureInfo.InvariantCulture, DateTimeStyles.None, out var d))
                return d.ToString("yyyy-MM-dd");
            // Fallback para archivos con formato anterior
            if (DateTime.TryParseExact(fecha, "dd/MM/yyyy",
                    CultureInfo.InvariantCulture, DateTimeStyles.None, out d))
                return d.ToString("yyyy-MM-dd");
            return fecha;
        }

        // ────────────────────────────────────────────────────────────────────────
        private static bool HasColumn(IDataRecord reader, string columnName)
        {
            for (int i = 0; i < reader.FieldCount; i++)
            {
                if (reader.GetName(i).Equals(columnName, StringComparison.InvariantCultureIgnoreCase))
                    return true;
            }
            return false;
        }


        private static readonly CultureInfo _culturaArca =
            CultureInfo.GetCultureInfo("es-AR");

        /// <summary>
        /// Parsea importes en formato argentino: "." = separador de miles, "," = separador decimal.
        /// Ejemplo: "1.105,50" => 1105.50
        /// </summary>
        private static decimal ParseImporte(string valor)
        {
            if (string.IsNullOrWhiteSpace(valor)) return 0m;
            return decimal.TryParse(valor, NumberStyles.Any, _culturaArca, out var result)
                   ? result : 0m;
        }

        private static string Normalizar(string valor, int longitud)
        {
            string result =
            (valor ?? string.Empty).TrimStart('0').PadLeft(longitud, '0');
            return result;
        }

        private static string NormalizarCuit(string cuit) =>
            string.IsNullOrWhiteSpace(cuit)
                ? string.Empty
                : new string(cuit.Where(char.IsDigit).ToArray());

        private static string NormalizarTipo(string tipo, bool usarEquivalenciaOnline)
        {
            string t = (tipo ?? string.Empty).Trim().ToUpperInvariant();
            
            if (usarEquivalenciaOnline && t.Length > 0)
            {
                // Mapear de "001" (ARCA) a "1" (Octosis/Local)
                var parsed = TipoComprobanteMapper.Parse(t.PadLeft(3, '0'));
                if (parsed.Reconocido)
                    return parsed.TipoOctosis;
            }
            
            return t;
        }
    }
}
