using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.Json;
using ArcaCliente.Models;
using Conciliador.Comun;
using Microsoft.Data.SqlClient;

namespace ArcaCliente.Services
{
    /// <summary>
    /// Acceso SQL unificado para la configuración de ArcaCliente.
    /// Todas las tablas viven en el schema arca de la base común (Conciliador.Comun.SqlDb).
    /// </summary>
    internal static class ArcaSqlStorage
    {
        private static readonly JsonSerializerOptions JsonOpts = new() { WriteIndented = false };

        // ── Inicialización ────────────────────────────────────────────────────────

        public static void InitializeDatabase()
        {
            using var cn = Open();
            try
            {
                Execute(cn, ArcaSqlSchema.Ddl);
            }
            catch (SqlException ex) when (ex.Number == 2714)
            {
                // Ventana de carrera entre el chequeo IF OBJECT_ID(...) IS NULL y el
                // CREATE TABLE: dos instancias de ArcaCliente abriendo el modulo por
                // primera vez casi simultaneamente contra un schema arca recien
                // provisionado pueden pasar ambas el chequeo antes de que la otra
                // confirme el CREATE. El objeto ya existe (creado por el proceso
                // concurrente) => no-op.
            }
        }

        // ── Perfiles Offline ──────────────────────────────────────────────────────

        public static List<PerfilOffline> LoadPerfilesOffline()
        {
            var list = new List<PerfilOffline>();
            using var cn = Open();
            using var cmd = cn.CreateCommand();
            cmd.CommandText = "SELECT * FROM arca.ArcaPerfilesOffline ORDER BY Nombre";
            using var r = cmd.ExecuteReader();
            while (r.Read())
            {
                var p = new PerfilOffline
                {
                    Id                 = Guid.Parse(r.GetString(r.GetOrdinal("Id"))),
                    Nombre             = r.GetString(r.GetOrdinal("Nombre")),
                    TipoArchivo        = (TipoArchivoOffline)r.GetInt32(r.GetOrdinal("TipoArchivo")),
                    Separador          = r.GetString(r.GetOrdinal("Separador")),
                    Encoding           = r.GetString(r.GetOrdinal("Encoding")),
                    HojaExcel          = r.IsDBNull(r.GetOrdinal("HojaExcel")) ? null : r.GetString(r.GetOrdinal("HojaExcel")),
                    TieneCabecera      = r.GetBoolean(r.GetOrdinal("TieneCabecera")),
                    ColFecha           = Str(r, "ColFecha"),
                    ColPuntoVenta      = Str(r, "ColPuntoVenta"),
                    ColNumero          = Str(r, "ColNumero"),
                    ColTipoComprobante = Str(r, "ColTipoComprobante"),
                    ColCuit            = Str(r, "ColCuit"),
                    ColNombreProveedor = Str(r, "ColNombreProveedor"),
                    ColTotal           = Str(r, "ColTotal"),
                    PosFecha           = r.GetInt32(r.GetOrdinal("PosFecha")),
                    PosPuntoVenta      = r.GetInt32(r.GetOrdinal("PosPuntoVenta")),
                    PosNumero          = r.GetInt32(r.GetOrdinal("PosNumero")),
                    PosTipoComprobante = r.GetInt32(r.GetOrdinal("PosTipoComprobante")),
                    PosCuit            = r.GetInt32(r.GetOrdinal("PosCuit")),
                    PosNombreProveedor = r.GetInt32(r.GetOrdinal("PosNombreProveedor")),
                    PosTotal           = r.GetInt32(r.GetOrdinal("PosTotal")),
                    FormatoFecha       = r.GetString(r.GetOrdinal("FormatoFecha")),
                    SeparadorDecimal   = r.GetString(r.GetOrdinal("SeparadorDecimal")),
                    CarpetaCsvArca     = Str(r, "CarpetaCsvArca"),
                    SistemaExportacion = (SistemaExportacionOffline)r.GetInt32(r.GetOrdinal("SistemaExportacion")),
                    ConfigPresea       = DeserializeOrNull<ConfigPresea>(Str(r, "ConfigPreseaJson")),
                    DirectivasConciliacion = Deserialize<List<DirectivaConciliacion>>(Str(r, "DirectivasJson")) ?? new()
                };

                if (p.DirectivasConciliacion.Count == 0)
                    p.DirectivasConciliacion.Add(DirectivaConciliacion.CrearPrimaria());

                list.Add(p);
            }
            return list;
        }

        // NOTA - riesgo de lost update: este patron (DELETE de toda la tabla + reinsert
        // de la lista completa en memoria del llamador, en una sola transaccion) era
        // seguro bajo SQLite por-estacion (un unico escritor por construccion). Contra
        // la base SQL Server compartida, si dos estaciones guardan casi al mismo tiempo,
        // el segundo Save pisa en silencio los cambios del primero con una lista stale.
        // No se resuelve aca (requeriria concurrencia optimista real: columna de
        // version/rowversion, o upserts por fila en lugar de delete-all+reinsert).
        // Mismo riesgo aplica a SavePerfilesFiscales y SaveEquivalencias mas abajo.
        public static void SavePerfilesOffline(List<PerfilOffline> perfiles)
        {
            using var cn = Open();
            using var tx = cn.BeginTransaction();

            Execute(cn, "DELETE FROM arca.ArcaPerfilesOffline", tx);

            foreach (var p in perfiles)
            {
                using var cmd = cn.CreateCommand();
                cmd.Transaction = tx;
                cmd.CommandText = @"
                    INSERT INTO arca.ArcaPerfilesOffline
                        (Id, Nombre, TipoArchivo, Separador, Encoding, HojaExcel,
                         TieneCabecera, ColFecha, ColPuntoVenta, ColNumero, ColTipoComprobante,
                         ColCuit, ColNombreProveedor, ColTotal,
                         PosFecha, PosPuntoVenta, PosNumero, PosTipoComprobante,
                         PosCuit, PosNombreProveedor, PosTotal,
                         FormatoFecha, SeparadorDecimal, CarpetaCsvArca,
                         SistemaExportacion, ConfigPreseaJson, DirectivasJson)
                    VALUES
                        (@Id, @Nombre, @TipoArchivo, @Separador, @Encoding, @HojaExcel,
                         @TieneCabecera, @ColFecha, @ColPuntoVenta, @ColNumero, @ColTipoComprobante,
                         @ColCuit, @ColNombreProveedor, @ColTotal,
                         @PosFecha, @PosPuntoVenta, @PosNumero, @PosTipoComprobante,
                         @PosCuit, @PosNombreProveedor, @PosTotal,
                         @FormatoFecha, @SeparadorDecimal, @CarpetaCsvArca,
                         @SistemaExportacion, @ConfigPreseaJson, @DirectivasJson)";

                cmd.Parameters.AddWithValue("@Id",                  p.Id.ToString());
                cmd.Parameters.AddWithValue("@Nombre",              p.Nombre ?? "");
                cmd.Parameters.AddWithValue("@TipoArchivo",         (int)p.TipoArchivo);
                cmd.Parameters.AddWithValue("@Separador",           p.Separador ?? ";");
                cmd.Parameters.AddWithValue("@Encoding",            p.Encoding ?? "UTF-8");
                cmd.Parameters.AddWithValue("@HojaExcel",           (object?)p.HojaExcel ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@TieneCabecera",       p.TieneCabecera ? 1 : 0);
                cmd.Parameters.AddWithValue("@ColFecha",            (object?)p.ColFecha ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@ColPuntoVenta",       (object?)p.ColPuntoVenta ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@ColNumero",           (object?)p.ColNumero ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@ColTipoComprobante",  (object?)p.ColTipoComprobante ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@ColCuit",             (object?)p.ColCuit ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@ColNombreProveedor",  (object?)p.ColNombreProveedor ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@ColTotal",            (object?)p.ColTotal ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@PosFecha",            p.PosFecha);
                cmd.Parameters.AddWithValue("@PosPuntoVenta",       p.PosPuntoVenta);
                cmd.Parameters.AddWithValue("@PosNumero",           p.PosNumero);
                cmd.Parameters.AddWithValue("@PosTipoComprobante",  p.PosTipoComprobante);
                cmd.Parameters.AddWithValue("@PosCuit",             p.PosCuit);
                cmd.Parameters.AddWithValue("@PosNombreProveedor",  p.PosNombreProveedor);
                cmd.Parameters.AddWithValue("@PosTotal",            p.PosTotal);
                cmd.Parameters.AddWithValue("@FormatoFecha",        p.FormatoFecha ?? "dd/MM/yyyy");
                cmd.Parameters.AddWithValue("@SeparadorDecimal",    p.SeparadorDecimal ?? ".");
                cmd.Parameters.AddWithValue("@CarpetaCsvArca",      (object?)p.CarpetaCsvArca ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@SistemaExportacion",  (int)p.SistemaExportacion);
                cmd.Parameters.AddWithValue("@ConfigPreseaJson",    p.ConfigPresea == null ? DBNull.Value : JsonSerializer.Serialize(p.ConfigPresea, JsonOpts));
                cmd.Parameters.AddWithValue("@DirectivasJson",      JsonSerializer.Serialize(p.DirectivasConciliacion, JsonOpts));
                cmd.ExecuteNonQuery();
            }

            tx.Commit();
        }

        // ── Perfiles Fiscales ─────────────────────────────────────────────────────

        public static List<PerfilFiscal> LoadPerfilesFiscales()
        {
            var list = new List<PerfilFiscal>();
            using var cn = Open();
            using var cmd = cn.CreateCommand();
            cmd.CommandText = "SELECT * FROM arca.ArcaPerfilesFiscales ORDER BY Nombre";
            using var r = cmd.ExecuteReader();
            while (r.Read())
            {
                list.Add(new PerfilFiscal
                {
                    Id                           = Guid.Parse(r.GetString(r.GetOrdinal("Id"))),
                    Nombre                       = r.GetString(r.GetOrdinal("Nombre")),
                    Username                     = r.GetString(r.GetOrdinal("Username")),
                    Password                     = r.GetString(r.GetOrdinal("Password")),
                    Cuit                         = r.GetString(r.GetOrdinal("Cuit")),
                    IntegracionHabilitada        = r.GetBoolean(r.GetOrdinal("IntegracionHabilitada")),
                    Sistema                      = (SistemaIntegracion)r.GetInt32(r.GetOrdinal("Sistema")),
                    ConciliacionConnectionString = Str(r, "ConciliacionConnectionString"),
                    ConciliacionQuery            = Str(r, "ConciliacionQuery"),
                    OctosisConnectionString      = Str(r, "OctosisConnectionString"),
                    ArcaApiUrl                   = Str(r, "ArcaApiUrl"),
                    DirectivasConciliacion       = Deserialize<List<DirectivaConciliacion>>(Str(r, "DirectivasJson")) ?? new()
                });
            }
            return list;
        }

        // NOTA - riesgo de lost update: ver comentario en SavePerfilesOffline.
        public static void SavePerfilesFiscales(List<PerfilFiscal> perfiles)
        {
            using var cn = Open();
            using var tx = cn.BeginTransaction();

            Execute(cn, "DELETE FROM arca.ArcaPerfilesFiscales", tx);

            foreach (var p in perfiles)
            {
                using var cmd = cn.CreateCommand();
                cmd.Transaction = tx;
                cmd.CommandText = @"
                    INSERT INTO arca.ArcaPerfilesFiscales
                        (Id, Nombre, Username, Password, Cuit,
                         IntegracionHabilitada, Sistema,
                         ConciliacionConnectionString, ConciliacionQuery,
                         OctosisConnectionString, ArcaApiUrl, DirectivasJson)
                    VALUES
                        (@Id, @Nombre, @Username, @Password, @Cuit,
                         @IntegracionHabilitada, @Sistema,
                         @ConciliacionConnectionString, @ConciliacionQuery,
                         @OctosisConnectionString, @ArcaApiUrl, @DirectivasJson)";

                cmd.Parameters.AddWithValue("@Id",                           p.Id.ToString());
                cmd.Parameters.AddWithValue("@Nombre",                       p.Nombre ?? "");
                cmd.Parameters.AddWithValue("@Username",                     p.Username ?? "");
                cmd.Parameters.AddWithValue("@Password",                     p.Password ?? "");
                cmd.Parameters.AddWithValue("@Cuit",                         p.Cuit ?? "");
                cmd.Parameters.AddWithValue("@IntegracionHabilitada",        p.IntegracionHabilitada ? 1 : 0);
                cmd.Parameters.AddWithValue("@Sistema",                      (int)p.Sistema);
                cmd.Parameters.AddWithValue("@ConciliacionConnectionString", (object?)p.ConciliacionConnectionString ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@ConciliacionQuery",            (object?)p.ConciliacionQuery ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@OctosisConnectionString",      (object?)p.OctosisConnectionString ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@ArcaApiUrl",                   (object?)p.ArcaApiUrl ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@DirectivasJson",               JsonSerializer.Serialize(p.DirectivasConciliacion, JsonOpts));
                cmd.ExecuteNonQuery();
            }

            tx.Commit();
        }

        // ── Equivalencias ─────────────────────────────────────────────────────────

        public static List<EquivalenciaTipoComprobante> LoadEquivalencias()
        {
            var list = new List<EquivalenciaTipoComprobante>();
            using var cn = Open();
            using var cmd = cn.CreateCommand();
            cmd.CommandText = "SELECT CodigoAfip, TipoSistema, Letra FROM arca.ArcaEquivalencias";
            using var r = cmd.ExecuteReader();
            while (r.Read())
                list.Add(new EquivalenciaTipoComprobante
                {
                    CodigoAfip  = r.GetString(0),
                    TipoSistema = r.GetString(1),
                    Letra       = r.GetString(2)
                });
            return list;
        }

        // NOTA - riesgo de lost update: ver comentario en SavePerfilesOffline.
        public static void SaveEquivalencias(IEnumerable<EquivalenciaTipoComprobante> items)
        {
            using var cn = Open();
            using var tx = cn.BeginTransaction();

            Execute(cn, "DELETE FROM arca.ArcaEquivalencias", tx);

            foreach (var i in items)
            {
                using var cmd = cn.CreateCommand();
                cmd.Transaction = tx;
                cmd.CommandText = "INSERT INTO arca.ArcaEquivalencias (CodigoAfip, TipoSistema, Letra) VALUES (@c, @t, @l)";
                cmd.Parameters.AddWithValue("@c", i.CodigoAfip ?? "");
                cmd.Parameters.AddWithValue("@t", i.TipoSistema ?? "");
                cmd.Parameters.AddWithValue("@l", i.Letra ?? "");
                cmd.ExecuteNonQuery();
            }

            tx.Commit();
        }

        // ── Presea: proveedores (mapa por CUIT) ────────────────────────────────────

        public static List<ConfigPreseaProveedor> LoadPreseaProveedores()
        {
            var list = new List<ConfigPreseaProveedor>();
            using var cn = Open();
            using var cmd = cn.CreateCommand();
            cmd.CommandText = "SELECT * FROM arca.PreseaProveedores ORDER BY Nombre";
            using var r = cmd.ExecuteReader();
            while (r.Read())
                list.Add(ReadPreseaProveedor(r));
            return list;
        }

        public static ConfigPreseaProveedor GetPreseaProveedor(string cuit)
        {
            string key = SoloDigitos(cuit);
            using var cn = Open();
            using var cmd = cn.CreateCommand();
            cmd.CommandText = "SELECT * FROM arca.PreseaProveedores WHERE Cuit = @cuit";
            cmd.Parameters.AddWithValue("@cuit", key);
            using var r = cmd.ExecuteReader();
            return r.Read() ? ReadPreseaProveedor(r) : null;
        }

        public static void UpsertPreseaProveedor(ConfigPreseaProveedor p)
        {
            using var cn = Open();
            UpsertPreseaProveedor(cn, null, p);
        }

        public static void UpsertPreseaProveedores(IEnumerable<ConfigPreseaProveedor> proveedores)
        {
            using var cn = Open();
            using var tx = cn.BeginTransaction();
            foreach (var p in proveedores)
                UpsertPreseaProveedor(cn, tx, p);
            tx.Commit();
        }

        private static void UpsertPreseaProveedor(SqlConnection cn, SqlTransaction tx, ConfigPreseaProveedor p)
        {
            using var cmd = cn.CreateCommand();
            cmd.Transaction = tx;
            cmd.CommandText = @"
                MERGE arca.PreseaProveedores AS t
                USING (SELECT @Cuit AS Cuit) AS s ON t.Cuit = s.Cuit
                WHEN MATCHED THEN UPDATE SET
                    Nombre = @Nombre, CodigoProveedor = @CodigoProveedor,
                    CuentaContableProveedor = @CuentaContableProveedor, CuentaDebe = @CuentaDebe,
                    Centro = @Centro, Provincia = @Provincia, Condicion = @Condicion,
                    Descuento = @Descuento, Fiscal = @Fiscal
                WHEN NOT MATCHED THEN INSERT
                    (Cuit, Nombre, CodigoProveedor, CuentaContableProveedor, CuentaDebe, Centro, Provincia, Condicion, Descuento, Fiscal)
                    VALUES (@Cuit, @Nombre, @CodigoProveedor, @CuentaContableProveedor, @CuentaDebe, @Centro, @Provincia, @Condicion, @Descuento, @Fiscal);";

            cmd.Parameters.AddWithValue("@Cuit",                    SoloDigitos(p.Cuit));
            cmd.Parameters.AddWithValue("@Nombre",                  p.Nombre ?? "");
            cmd.Parameters.AddWithValue("@CodigoProveedor",         p.CodigoProveedor ?? "");
            cmd.Parameters.AddWithValue("@CuentaContableProveedor", p.CuentaContableProveedor ?? "");
            cmd.Parameters.AddWithValue("@CuentaDebe",              p.CuentaDebe ?? "");
            cmd.Parameters.AddWithValue("@Centro",                  p.Centro ?? "");
            cmd.Parameters.AddWithValue("@Provincia",               p.Provincia ?? "");
            cmd.Parameters.AddWithValue("@Condicion",               p.Condicion ?? "");
            cmd.Parameters.AddWithValue("@Descuento",               p.Descuento);
            cmd.Parameters.AddWithValue("@Fiscal",                  p.Fiscal ?? "");
            cmd.ExecuteNonQuery();
        }

        private static ConfigPreseaProveedor ReadPreseaProveedor(SqlDataReader r) => new()
        {
            Cuit                    = r.GetString(r.GetOrdinal("Cuit")),
            Nombre                  = r.GetString(r.GetOrdinal("Nombre")),
            CodigoProveedor         = r.GetString(r.GetOrdinal("CodigoProveedor")),
            CuentaContableProveedor = r.GetString(r.GetOrdinal("CuentaContableProveedor")),
            CuentaDebe              = r.GetString(r.GetOrdinal("CuentaDebe")),
            Centro                  = r.GetString(r.GetOrdinal("Centro")),
            Provincia               = r.GetString(r.GetOrdinal("Provincia")),
            Condicion               = r.GetString(r.GetOrdinal("Condicion")),
            Descuento               = r.GetDecimal(r.GetOrdinal("Descuento")),
            Fiscal                  = r.GetString(r.GetOrdinal("Fiscal")),
        };

        // ── Presea: memoria de exportados ───────────────────────────────────────────

        public static bool ExisteComprobanteExportado(string clave)
        {
            using var cn = Open();
            using var cmd = cn.CreateCommand();
            cmd.CommandText = "SELECT 1 FROM arca.PreseaComprobantesExportados WHERE Clave = @clave";
            cmd.Parameters.AddWithValue("@clave", clave ?? "");
            return cmd.ExecuteScalar() != null;
        }

        public static HashSet<string> LoadClavesExportadas()
        {
            var set = new HashSet<string>();
            using var cn = Open();
            using var cmd = cn.CreateCommand();
            cmd.CommandText = "SELECT Clave FROM arca.PreseaComprobantesExportados";
            using var r = cmd.ExecuteReader();
            while (r.Read())
                set.Add(r.GetString(0));
            return set;
        }

        public static void RegistrarComprobanteExportado(PreseaComprobanteExportado e)
        {
            using var cn = Open();
            RegistrarComprobanteExportado(cn, null, e);
        }

        public static void RegistrarComprobantesExportados(IEnumerable<PreseaComprobanteExportado> exportados)
        {
            using var cn = Open();
            using var tx = cn.BeginTransaction();
            foreach (var e in exportados)
                RegistrarComprobanteExportado(cn, tx, e);
            tx.Commit();
        }

        private static void RegistrarComprobanteExportado(SqlConnection cn, SqlTransaction tx, PreseaComprobanteExportado e)
        {
            using var cmd = cn.CreateCommand();
            cmd.Transaction = tx;
            // Ignorar duplicados: registrar dos veces la misma clave no es un error.
            cmd.CommandText = @"
                IF NOT EXISTS (SELECT 1 FROM arca.PreseaComprobantesExportados WHERE Clave = @Clave)
                    INSERT INTO arca.PreseaComprobantesExportados
                        (Clave, CuitEmisor, TipoCmp, PtoVta, Nro, CodAut, Importe,
                         FechaComprobante, FechaExportacion, ArchivoGenerado, PerfilOfflineId)
                    VALUES
                        (@Clave, @CuitEmisor, @TipoCmp, @PtoVta, @Nro, @CodAut, @Importe,
                         @FechaComprobante, @FechaExportacion, @ArchivoGenerado, @PerfilOfflineId)";

            cmd.Parameters.AddWithValue("@Clave",            e.Clave ?? "");
            cmd.Parameters.AddWithValue("@CuitEmisor",       e.CuitEmisor ?? "");
            cmd.Parameters.AddWithValue("@TipoCmp",          e.TipoCmp ?? "");
            cmd.Parameters.AddWithValue("@PtoVta",           e.PtoVta ?? "");
            cmd.Parameters.AddWithValue("@Nro",              e.Nro ?? "");
            cmd.Parameters.AddWithValue("@CodAut",           e.CodAut ?? "");
            cmd.Parameters.AddWithValue("@Importe",          e.Importe);
            cmd.Parameters.AddWithValue("@FechaComprobante", e.FechaComprobante ?? "");
            cmd.Parameters.AddWithValue("@FechaExportacion", e.FechaExportacion);
            cmd.Parameters.AddWithValue("@ArchivoGenerado",  e.ArchivoGenerado ?? "");
            cmd.Parameters.AddWithValue("@PerfilOfflineId",  e.PerfilOfflineId ?? "");
            cmd.ExecuteNonQuery();
        }

        public static List<PreseaComprobanteExportado> LoadComprobantesExportados()
        {
            var list = new List<PreseaComprobanteExportado>();
            using var cn = Open();
            using var cmd = cn.CreateCommand();
            cmd.CommandText = "SELECT * FROM arca.PreseaComprobantesExportados ORDER BY FechaExportacion DESC";
            using var r = cmd.ExecuteReader();
            while (r.Read())
            {
                list.Add(new PreseaComprobanteExportado
                {
                    Clave            = r.GetString(r.GetOrdinal("Clave")),
                    CuitEmisor       = r.GetString(r.GetOrdinal("CuitEmisor")),
                    TipoCmp          = r.GetString(r.GetOrdinal("TipoCmp")),
                    PtoVta           = r.GetString(r.GetOrdinal("PtoVta")),
                    Nro              = r.GetString(r.GetOrdinal("Nro")),
                    CodAut           = r.GetString(r.GetOrdinal("CodAut")),
                    Importe          = r.GetDecimal(r.GetOrdinal("Importe")),
                    FechaComprobante = r.GetString(r.GetOrdinal("FechaComprobante")),
                    FechaExportacion = r.GetDateTime(r.GetOrdinal("FechaExportacion")),
                    ArchivoGenerado  = r.GetString(r.GetOrdinal("ArchivoGenerado")),
                    PerfilOfflineId  = r.GetString(r.GetOrdinal("PerfilOfflineId")),
                });
            }
            return list;
        }

        // ── Presea: mapeo de columnas de importacion (por entidad) ──────────────────

        public static MapeoColumnasArchivo LoadMapeoColumnas(string entidad)
        {
            using var cn = Open();
            using var cmd = cn.CreateCommand();
            cmd.CommandText = "SELECT ConfigJson FROM arca.PreseaMapeoColumnas WHERE Entidad = @e";
            cmd.Parameters.AddWithValue("@e", entidad ?? "");
            var json = cmd.ExecuteScalar() as string;
            return Deserialize<MapeoColumnasArchivo>(json);
        }

        public static void SaveMapeoColumnas(MapeoColumnasArchivo mapeo)
        {
            using var cn = Open();
            using var cmd = cn.CreateCommand();
            cmd.CommandText = @"
                MERGE arca.PreseaMapeoColumnas AS t
                USING (SELECT @e AS Entidad) AS s ON t.Entidad = s.Entidad
                WHEN MATCHED THEN UPDATE SET ConfigJson = @j
                WHEN NOT MATCHED THEN INSERT (Entidad, ConfigJson) VALUES (@e, @j);";
            cmd.Parameters.AddWithValue("@e", mapeo.Entidad ?? "");
            cmd.Parameters.AddWithValue("@j", JsonSerializer.Serialize(mapeo, JsonOpts));
            cmd.ExecuteNonQuery();
        }

        // ── Helpers ───────────────────────────────────────────────────────────────

        private static SqlConnection Open()
        {
            var cn = SqlDb.GetConnection();
            cn.Open();
            return cn;
        }

        private static void Execute(SqlConnection cn, string sql, SqlTransaction? tx = null)
        {
            using var cmd = cn.CreateCommand();
            cmd.Transaction = tx;
            cmd.CommandText = sql;
            cmd.ExecuteNonQuery();
        }

        private static string? Str(SqlDataReader r, string col)
        {
            int ord = r.GetOrdinal(col);
            return r.IsDBNull(ord) ? null : r.GetString(ord);
        }

        private static string SoloDigitos(string? s) =>
            string.IsNullOrEmpty(s) ? string.Empty : new string(System.Linq.Enumerable.ToArray(System.Linq.Enumerable.Where(s, char.IsDigit)));

        private static T? Deserialize<T>(string? json) where T : class
        {
            if (string.IsNullOrWhiteSpace(json)) return null;
            try { return JsonSerializer.Deserialize<T>(json); }
            catch { return null; }
        }

        private static T? DeserializeOrNull<T>(string? json) where T : class
            => Deserialize<T>(json);
    }
}
