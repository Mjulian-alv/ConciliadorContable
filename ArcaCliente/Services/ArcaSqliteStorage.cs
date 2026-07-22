using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.Json;
using ArcaCliente.Models;
using Microsoft.Data.Sqlite;

namespace ArcaCliente.Services
{
    /// <summary>
    /// Acceso SQLite unificado para la configuración de ArcaCliente.
    /// Todas las tablas viven en la base indicada por <see cref="ArcaStorageConfig.DbPath"/>.
    /// </summary>
    internal static class ArcaSqliteStorage
    {
        private static readonly JsonSerializerOptions JsonOpts = new() { WriteIndented = false };

        // ── Inicialización ────────────────────────────────────────────────────────

        public static void InitializeDatabase()
        {
            using var cn = Open();

            Execute(cn, @"
                CREATE TABLE IF NOT EXISTS ArcaPerfilesOffline (
                    Id                  TEXT PRIMARY KEY,
                    Nombre              TEXT NOT NULL DEFAULT '',
                    TipoArchivo         INTEGER NOT NULL DEFAULT 0,
                    Separador           TEXT NOT NULL DEFAULT ';',
                    Encoding            TEXT NOT NULL DEFAULT 'UTF-8',
                    HojaExcel           TEXT,
                    TieneCabecera       INTEGER NOT NULL DEFAULT 1,
                    ColFecha            TEXT, ColPuntoVenta     TEXT, ColNumero           TEXT,
                    ColTipoComprobante  TEXT, ColCuit           TEXT, ColNombreProveedor  TEXT,
                    ColTotal            TEXT,
                    PosFecha            INTEGER NOT NULL DEFAULT 1, PosPuntoVenta  INTEGER NOT NULL DEFAULT 2,
                    PosNumero           INTEGER NOT NULL DEFAULT 3, PosTipoComprobante INTEGER NOT NULL DEFAULT 4,
                    PosCuit             INTEGER NOT NULL DEFAULT 5, PosNombreProveedor INTEGER NOT NULL DEFAULT 6,
                    PosTotal            INTEGER NOT NULL DEFAULT 7,
                    FormatoFecha        TEXT NOT NULL DEFAULT 'dd/MM/yyyy',
                    SeparadorDecimal    TEXT NOT NULL DEFAULT '.',
                    CarpetaCsvArca      TEXT,
                    SistemaExportacion  INTEGER NOT NULL DEFAULT 0,
                    ConfigPreseaJson    TEXT,
                    DirectivasJson      TEXT NOT NULL DEFAULT '[]'
                )");

            Execute(cn, @"
                CREATE TABLE IF NOT EXISTS ArcaPerfilesFiscales (
                    Id                          TEXT PRIMARY KEY,
                    Nombre                      TEXT NOT NULL DEFAULT '',
                    Username                    TEXT NOT NULL DEFAULT '',
                    Password                    TEXT NOT NULL DEFAULT '',
                    Cuit                        TEXT NOT NULL DEFAULT '',
                    IntegracionHabilitada       INTEGER NOT NULL DEFAULT 0,
                    Sistema                     INTEGER NOT NULL DEFAULT 0,
                    ConciliacionConnectionString TEXT,
                    ConciliacionQuery           TEXT,
                    OctosisConnectionString     TEXT,
                    ArcaApiUrl                  TEXT,
                    DirectivasJson              TEXT NOT NULL DEFAULT '[]'
                )");

            Execute(cn, @"
                CREATE TABLE IF NOT EXISTS ArcaEquivalencias (
                    CodigoAfip  TEXT PRIMARY KEY,
                    TipoSistema TEXT NOT NULL DEFAULT '',
                    Letra       TEXT NOT NULL DEFAULT ''
                )");

            // Mapa de proveedores PRESEA por CUIT (datos fijos por proveedor).
            Execute(cn, @"
                CREATE TABLE IF NOT EXISTS PreseaProveedores (
                    Cuit                    TEXT PRIMARY KEY,
                    Nombre                  TEXT NOT NULL DEFAULT '',
                    CodigoProveedor         TEXT NOT NULL DEFAULT '',
                    CuentaContableProveedor TEXT NOT NULL DEFAULT '',
                    CuentaDebe              TEXT NOT NULL DEFAULT '',
                    Centro                  TEXT NOT NULL DEFAULT '',
                    Provincia               TEXT NOT NULL DEFAULT '',
                    Condicion               TEXT NOT NULL DEFAULT '',
                    Descuento               TEXT NOT NULL DEFAULT '0',
                    Fiscal                  TEXT NOT NULL DEFAULT ''
                )");

            // Memoria anti-duplicado: comprobantes ya exportados a PRESEA.
            Execute(cn, @"
                CREATE TABLE IF NOT EXISTS PreseaComprobantesExportados (
                    Clave            TEXT PRIMARY KEY,
                    CuitEmisor       TEXT NOT NULL DEFAULT '',
                    TipoCmp          TEXT NOT NULL DEFAULT '',
                    PtoVta           TEXT NOT NULL DEFAULT '',
                    Nro              TEXT NOT NULL DEFAULT '',
                    CodAut           TEXT NOT NULL DEFAULT '',
                    Importe          TEXT NOT NULL DEFAULT '0',
                    FechaComprobante TEXT NOT NULL DEFAULT '',
                    FechaExportacion TEXT NOT NULL DEFAULT '',
                    ArchivoGenerado  TEXT NOT NULL DEFAULT '',
                    PerfilOfflineId  TEXT NOT NULL DEFAULT ''
                )");

            // Mapeo de columnas para importar maestros PRESEA desde CSV/Excel (por entidad).
            Execute(cn, @"
                CREATE TABLE IF NOT EXISTS PreseaMapeoColumnas (
                    Entidad    TEXT PRIMARY KEY,
                    ConfigJson TEXT NOT NULL DEFAULT '{}'
                )");
        }

        // ── Perfiles Offline ──────────────────────────────────────────────────────

        public static List<PerfilOffline> LoadPerfilesOffline()
        {
            var list = new List<PerfilOffline>();
            using var cn = Open();
            using var cmd = cn.CreateCommand();
            cmd.CommandText = "SELECT * FROM ArcaPerfilesOffline ORDER BY Nombre";
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
                    TieneCabecera      = r.GetInt32(r.GetOrdinal("TieneCabecera")) == 1,
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

        public static void SavePerfilesOffline(List<PerfilOffline> perfiles)
        {
            using var cn = Open();
            using var tx = cn.BeginTransaction();

            Execute(cn, "DELETE FROM ArcaPerfilesOffline", tx);

            foreach (var p in perfiles)
            {
                using var cmd = cn.CreateCommand();
                cmd.Transaction = tx;
                cmd.CommandText = @"
                    INSERT INTO ArcaPerfilesOffline
                        (Id, Nombre, TipoArchivo, Separador, Encoding, HojaExcel,
                         TieneCabecera, ColFecha, ColPuntoVenta, ColNumero, ColTipoComprobante,
                         ColCuit, ColNombreProveedor, ColTotal,
                         PosFecha, PosPuntoVenta, PosNumero, PosTipoComprobante,
                         PosCuit, PosNombreProveedor, PosTotal,
                         FormatoFecha, SeparadorDecimal, CarpetaCsvArca,
                         SistemaExportacion, ConfigPreseaJson, DirectivasJson)
                    VALUES
                        ($Id, $Nombre, $TipoArchivo, $Separador, $Encoding, $HojaExcel,
                         $TieneCabecera, $ColFecha, $ColPuntoVenta, $ColNumero, $ColTipoComprobante,
                         $ColCuit, $ColNombreProveedor, $ColTotal,
                         $PosFecha, $PosPuntoVenta, $PosNumero, $PosTipoComprobante,
                         $PosCuit, $PosNombreProveedor, $PosTotal,
                         $FormatoFecha, $SeparadorDecimal, $CarpetaCsvArca,
                         $SistemaExportacion, $ConfigPreseaJson, $DirectivasJson)";

                cmd.Parameters.AddWithValue("$Id",                  p.Id.ToString());
                cmd.Parameters.AddWithValue("$Nombre",              p.Nombre ?? "");
                cmd.Parameters.AddWithValue("$TipoArchivo",         (int)p.TipoArchivo);
                cmd.Parameters.AddWithValue("$Separador",           p.Separador ?? ";");
                cmd.Parameters.AddWithValue("$Encoding",            p.Encoding ?? "UTF-8");
                cmd.Parameters.AddWithValue("$HojaExcel",           (object?)p.HojaExcel ?? DBNull.Value);
                cmd.Parameters.AddWithValue("$TieneCabecera",       p.TieneCabecera ? 1 : 0);
                cmd.Parameters.AddWithValue("$ColFecha",            (object?)p.ColFecha ?? DBNull.Value);
                cmd.Parameters.AddWithValue("$ColPuntoVenta",       (object?)p.ColPuntoVenta ?? DBNull.Value);
                cmd.Parameters.AddWithValue("$ColNumero",           (object?)p.ColNumero ?? DBNull.Value);
                cmd.Parameters.AddWithValue("$ColTipoComprobante",  (object?)p.ColTipoComprobante ?? DBNull.Value);
                cmd.Parameters.AddWithValue("$ColCuit",             (object?)p.ColCuit ?? DBNull.Value);
                cmd.Parameters.AddWithValue("$ColNombreProveedor",  (object?)p.ColNombreProveedor ?? DBNull.Value);
                cmd.Parameters.AddWithValue("$ColTotal",            (object?)p.ColTotal ?? DBNull.Value);
                cmd.Parameters.AddWithValue("$PosFecha",            p.PosFecha);
                cmd.Parameters.AddWithValue("$PosPuntoVenta",       p.PosPuntoVenta);
                cmd.Parameters.AddWithValue("$PosNumero",           p.PosNumero);
                cmd.Parameters.AddWithValue("$PosTipoComprobante",  p.PosTipoComprobante);
                cmd.Parameters.AddWithValue("$PosCuit",             p.PosCuit);
                cmd.Parameters.AddWithValue("$PosNombreProveedor",  p.PosNombreProveedor);
                cmd.Parameters.AddWithValue("$PosTotal",            p.PosTotal);
                cmd.Parameters.AddWithValue("$FormatoFecha",        p.FormatoFecha ?? "dd/MM/yyyy");
                cmd.Parameters.AddWithValue("$SeparadorDecimal",    p.SeparadorDecimal ?? ".");
                cmd.Parameters.AddWithValue("$CarpetaCsvArca",      (object?)p.CarpetaCsvArca ?? DBNull.Value);
                cmd.Parameters.AddWithValue("$SistemaExportacion",  (int)p.SistemaExportacion);
                cmd.Parameters.AddWithValue("$ConfigPreseaJson",    p.ConfigPresea == null ? DBNull.Value : JsonSerializer.Serialize(p.ConfigPresea, JsonOpts));
                cmd.Parameters.AddWithValue("$DirectivasJson",      JsonSerializer.Serialize(p.DirectivasConciliacion, JsonOpts));
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
            cmd.CommandText = "SELECT * FROM ArcaPerfilesFiscales ORDER BY Nombre";
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
                    IntegracionHabilitada        = r.GetInt32(r.GetOrdinal("IntegracionHabilitada")) == 1,
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

        public static void SavePerfilesFiscales(List<PerfilFiscal> perfiles)
        {
            using var cn = Open();
            using var tx = cn.BeginTransaction();

            Execute(cn, "DELETE FROM ArcaPerfilesFiscales", tx);

            foreach (var p in perfiles)
            {
                using var cmd = cn.CreateCommand();
                cmd.Transaction = tx;
                cmd.CommandText = @"
                    INSERT INTO ArcaPerfilesFiscales
                        (Id, Nombre, Username, Password, Cuit,
                         IntegracionHabilitada, Sistema,
                         ConciliacionConnectionString, ConciliacionQuery,
                         OctosisConnectionString, ArcaApiUrl, DirectivasJson)
                    VALUES
                        ($Id, $Nombre, $Username, $Password, $Cuit,
                         $IntegracionHabilitada, $Sistema,
                         $ConciliacionConnectionString, $ConciliacionQuery,
                         $OctosisConnectionString, $ArcaApiUrl, $DirectivasJson)";

                cmd.Parameters.AddWithValue("$Id",                           p.Id.ToString());
                cmd.Parameters.AddWithValue("$Nombre",                       p.Nombre ?? "");
                cmd.Parameters.AddWithValue("$Username",                     p.Username ?? "");
                cmd.Parameters.AddWithValue("$Password",                     p.Password ?? "");
                cmd.Parameters.AddWithValue("$Cuit",                         p.Cuit ?? "");
                cmd.Parameters.AddWithValue("$IntegracionHabilitada",        p.IntegracionHabilitada ? 1 : 0);
                cmd.Parameters.AddWithValue("$Sistema",                      (int)p.Sistema);
                cmd.Parameters.AddWithValue("$ConciliacionConnectionString", (object?)p.ConciliacionConnectionString ?? DBNull.Value);
                cmd.Parameters.AddWithValue("$ConciliacionQuery",            (object?)p.ConciliacionQuery ?? DBNull.Value);
                cmd.Parameters.AddWithValue("$OctosisConnectionString",      (object?)p.OctosisConnectionString ?? DBNull.Value);
                cmd.Parameters.AddWithValue("$ArcaApiUrl",                   (object?)p.ArcaApiUrl ?? DBNull.Value);
                cmd.Parameters.AddWithValue("$DirectivasJson",               JsonSerializer.Serialize(p.DirectivasConciliacion, JsonOpts));
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
            cmd.CommandText = "SELECT CodigoAfip, TipoSistema, Letra FROM ArcaEquivalencias";
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

        public static void SaveEquivalencias(IEnumerable<EquivalenciaTipoComprobante> items)
        {
            using var cn = Open();
            using var tx = cn.BeginTransaction();

            Execute(cn, "DELETE FROM ArcaEquivalencias", tx);

            foreach (var i in items)
            {
                using var cmd = cn.CreateCommand();
                cmd.Transaction = tx;
                cmd.CommandText = "INSERT INTO ArcaEquivalencias (CodigoAfip, TipoSistema, Letra) VALUES ($c, $t, $l)";
                cmd.Parameters.AddWithValue("$c", i.CodigoAfip ?? "");
                cmd.Parameters.AddWithValue("$t", i.TipoSistema ?? "");
                cmd.Parameters.AddWithValue("$l", i.Letra ?? "");
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
            cmd.CommandText = "SELECT * FROM PreseaProveedores ORDER BY Nombre";
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
            cmd.CommandText = "SELECT * FROM PreseaProveedores WHERE Cuit = $cuit";
            cmd.Parameters.AddWithValue("$cuit", key);
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

        private static void UpsertPreseaProveedor(SqliteConnection cn, SqliteTransaction tx, ConfigPreseaProveedor p)
        {
            using var cmd = cn.CreateCommand();
            cmd.Transaction = tx;
            cmd.CommandText = @"
                INSERT INTO PreseaProveedores
                    (Cuit, Nombre, CodigoProveedor, CuentaContableProveedor, CuentaDebe,
                     Centro, Provincia, Condicion, Descuento, Fiscal)
                VALUES
                    ($Cuit, $Nombre, $CodigoProveedor, $CuentaContableProveedor, $CuentaDebe,
                     $Centro, $Provincia, $Condicion, $Descuento, $Fiscal)
                ON CONFLICT(Cuit) DO UPDATE SET
                    Nombre = excluded.Nombre,
                    CodigoProveedor = excluded.CodigoProveedor,
                    CuentaContableProveedor = excluded.CuentaContableProveedor,
                    CuentaDebe = excluded.CuentaDebe,
                    Centro = excluded.Centro,
                    Provincia = excluded.Provincia,
                    Condicion = excluded.Condicion,
                    Descuento = excluded.Descuento,
                    Fiscal = excluded.Fiscal";

            cmd.Parameters.AddWithValue("$Cuit",                    SoloDigitos(p.Cuit));
            cmd.Parameters.AddWithValue("$Nombre",                  p.Nombre ?? "");
            cmd.Parameters.AddWithValue("$CodigoProveedor",         p.CodigoProveedor ?? "");
            cmd.Parameters.AddWithValue("$CuentaContableProveedor", p.CuentaContableProveedor ?? "");
            cmd.Parameters.AddWithValue("$CuentaDebe",              p.CuentaDebe ?? "");
            cmd.Parameters.AddWithValue("$Centro",                  p.Centro ?? "");
            cmd.Parameters.AddWithValue("$Provincia",               p.Provincia ?? "");
            cmd.Parameters.AddWithValue("$Condicion",               p.Condicion ?? "");
            cmd.Parameters.AddWithValue("$Descuento",               p.Descuento.ToString(CultureInfo.InvariantCulture));
            cmd.Parameters.AddWithValue("$Fiscal",                  p.Fiscal ?? "");
            cmd.ExecuteNonQuery();
        }

        private static ConfigPreseaProveedor ReadPreseaProveedor(SqliteDataReader r) => new()
        {
            Cuit                    = r.GetString(r.GetOrdinal("Cuit")),
            Nombre                  = r.GetString(r.GetOrdinal("Nombre")),
            CodigoProveedor         = r.GetString(r.GetOrdinal("CodigoProveedor")),
            CuentaContableProveedor = r.GetString(r.GetOrdinal("CuentaContableProveedor")),
            CuentaDebe              = r.GetString(r.GetOrdinal("CuentaDebe")),
            Centro                  = r.GetString(r.GetOrdinal("Centro")),
            Provincia               = r.GetString(r.GetOrdinal("Provincia")),
            Condicion               = r.GetString(r.GetOrdinal("Condicion")),
            Descuento               = ParseDec(Str(r, "Descuento")),
            Fiscal                  = r.GetString(r.GetOrdinal("Fiscal")),
        };

        // ── Presea: memoria de exportados ───────────────────────────────────────────

        public static bool ExisteComprobanteExportado(string clave)
        {
            using var cn = Open();
            using var cmd = cn.CreateCommand();
            cmd.CommandText = "SELECT 1 FROM PreseaComprobantesExportados WHERE Clave = $clave LIMIT 1";
            cmd.Parameters.AddWithValue("$clave", clave ?? "");
            return cmd.ExecuteScalar() != null;
        }

        public static HashSet<string> LoadClavesExportadas()
        {
            var set = new HashSet<string>();
            using var cn = Open();
            using var cmd = cn.CreateCommand();
            cmd.CommandText = "SELECT Clave FROM PreseaComprobantesExportados";
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

        private static void RegistrarComprobanteExportado(SqliteConnection cn, SqliteTransaction tx, PreseaComprobanteExportado e)
        {
            using var cmd = cn.CreateCommand();
            cmd.Transaction = tx;
            // INSERT OR IGNORE: registrar dos veces la misma clave no es un error.
            cmd.CommandText = @"
                INSERT OR IGNORE INTO PreseaComprobantesExportados
                    (Clave, CuitEmisor, TipoCmp, PtoVta, Nro, CodAut, Importe,
                     FechaComprobante, FechaExportacion, ArchivoGenerado, PerfilOfflineId)
                VALUES
                    ($Clave, $CuitEmisor, $TipoCmp, $PtoVta, $Nro, $CodAut, $Importe,
                     $FechaComprobante, $FechaExportacion, $ArchivoGenerado, $PerfilOfflineId)";

            cmd.Parameters.AddWithValue("$Clave",            e.Clave ?? "");
            cmd.Parameters.AddWithValue("$CuitEmisor",       e.CuitEmisor ?? "");
            cmd.Parameters.AddWithValue("$TipoCmp",          e.TipoCmp ?? "");
            cmd.Parameters.AddWithValue("$PtoVta",           e.PtoVta ?? "");
            cmd.Parameters.AddWithValue("$Nro",              e.Nro ?? "");
            cmd.Parameters.AddWithValue("$CodAut",           e.CodAut ?? "");
            cmd.Parameters.AddWithValue("$Importe",          e.Importe.ToString(CultureInfo.InvariantCulture));
            cmd.Parameters.AddWithValue("$FechaComprobante", e.FechaComprobante ?? "");
            cmd.Parameters.AddWithValue("$FechaExportacion", e.FechaExportacion.ToString("o", CultureInfo.InvariantCulture));
            cmd.Parameters.AddWithValue("$ArchivoGenerado",  e.ArchivoGenerado ?? "");
            cmd.Parameters.AddWithValue("$PerfilOfflineId",  e.PerfilOfflineId ?? "");
            cmd.ExecuteNonQuery();
        }

        public static List<PreseaComprobanteExportado> LoadComprobantesExportados()
        {
            var list = new List<PreseaComprobanteExportado>();
            using var cn = Open();
            using var cmd = cn.CreateCommand();
            cmd.CommandText = "SELECT * FROM PreseaComprobantesExportados ORDER BY FechaExportacion DESC";
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
                    Importe          = ParseDec(Str(r, "Importe")),
                    FechaComprobante = r.GetString(r.GetOrdinal("FechaComprobante")),
                    FechaExportacion = ParseFecha(Str(r, "FechaExportacion")),
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
            cmd.CommandText = "SELECT ConfigJson FROM PreseaMapeoColumnas WHERE Entidad = $e";
            cmd.Parameters.AddWithValue("$e", entidad ?? "");
            var json = cmd.ExecuteScalar() as string;
            return Deserialize<MapeoColumnasArchivo>(json);
        }

        public static void SaveMapeoColumnas(MapeoColumnasArchivo mapeo)
        {
            using var cn = Open();
            using var cmd = cn.CreateCommand();
            cmd.CommandText = @"
                INSERT INTO PreseaMapeoColumnas (Entidad, ConfigJson)
                VALUES ($e, $j)
                ON CONFLICT(Entidad) DO UPDATE SET ConfigJson = excluded.ConfigJson";
            cmd.Parameters.AddWithValue("$e", mapeo.Entidad ?? "");
            cmd.Parameters.AddWithValue("$j", JsonSerializer.Serialize(mapeo, JsonOpts));
            cmd.ExecuteNonQuery();
        }

        // ── Migración desde JSON ──────────────────────────────────────────────────

        /// <summary>
        /// Si existen los archivos JSON/DB legacy, importa los datos y los elimina.
        /// Llamar una sola vez después de <see cref="InitializeDatabase"/>.
        /// </summary>
        public static void MigrarDesdeLegacyIfNeeded()
        {
            MigrarPerfilesOffline();
            MigrarPerfilesFiscales();
            MigrarEquivalencias();
        }

        private static void MigrarPerfilesOffline()
        {
            string jsonPath = System.IO.Path.Combine(AppContext.BaseDirectory, "perfiles_offline.json");
            if (!System.IO.File.Exists(jsonPath)) return;

            try
            {
                var existentes = LoadPerfilesOffline();
                if (existentes.Count > 0) { SafeDelete(jsonPath); return; }

                var texto = System.IO.File.ReadAllText(jsonPath);
                var lista = System.Text.Json.JsonSerializer.Deserialize<List<PerfilOffline>>(texto);
                if (lista != null && lista.Count > 0)
                {
                    foreach (var p in lista)
                        if (p.DirectivasConciliacion.Count == 0)
                            p.DirectivasConciliacion.Add(DirectivaConciliacion.CrearPrimaria());
                    SavePerfilesOffline(lista);
                }
                SafeDelete(jsonPath);
            }
            catch { /* no bloquear el arranque */ }
        }

        private static void MigrarPerfilesFiscales()
        {
            string jsonPath = System.IO.Path.Combine(AppContext.BaseDirectory, "perfiles.json");
            if (!System.IO.File.Exists(jsonPath)) return;

            try
            {
                var existentes = LoadPerfilesFiscales();
                if (existentes.Count > 0) { SafeDelete(jsonPath); return; }

                var texto = System.IO.File.ReadAllText(jsonPath);
                var lista = System.Text.Json.JsonSerializer.Deserialize<List<PerfilFiscal>>(texto);
                if (lista != null && lista.Count > 0)
                    SavePerfilesFiscales(lista);
                SafeDelete(jsonPath);
            }
            catch { }
        }

        private static void MigrarEquivalencias()
        {
            string legacyDb = System.IO.Path.Combine(AppContext.BaseDirectory, "equivalencias.db");
            if (!System.IO.File.Exists(legacyDb)) return;

            try
            {
                var existentes = LoadEquivalencias();
                if (existentes.Count > 0) { SafeDelete(legacyDb); return; }

                using var legacyCn = new SqliteConnection($"Data Source={legacyDb}");
                legacyCn.Open();
                using var cmd = legacyCn.CreateCommand();
                cmd.CommandText = "SELECT CodigoAfip, TipoSistema, Letra FROM Equivalencias";
                var lista = new List<EquivalenciaTipoComprobante>();
                using var r = cmd.ExecuteReader();
                while (r.Read())
                    lista.Add(new EquivalenciaTipoComprobante
                    {
                        CodigoAfip  = r.GetString(0),
                        TipoSistema = r.GetString(1),
                        Letra       = r.GetString(2)
                    });

                if (lista.Count > 0)
                    SaveEquivalencias(lista);
            }
            catch { }
            // No eliminamos equivalencias.db si está en uso; se limpiará en próximo arranque
        }

        // ── Helpers ───────────────────────────────────────────────────────────────

        private static SqliteConnection Open()
        {
            var cn = new SqliteConnection(ArcaStorageConfig.ConnectionString);
            cn.Open();
            // WAL + synchronous=NORMAL: sin esto SQLite hace un fsync por cada commit.
            // synchronous es por conexión, así que se aplica en cada apertura.
            Execute(cn, "PRAGMA journal_mode=WAL; PRAGMA synchronous=NORMAL;");
            return cn;
        }

        private static void Execute(SqliteConnection cn, string sql, SqliteTransaction? tx = null)
        {
            using var cmd = cn.CreateCommand();
            cmd.Transaction = tx;
            cmd.CommandText = sql;
            cmd.ExecuteNonQuery();
        }

        private static string? Str(SqliteDataReader r, string col)
        {
            int ord = r.GetOrdinal(col);
            return r.IsDBNull(ord) ? null : r.GetString(ord);
        }

        private static string SoloDigitos(string? s) =>
            string.IsNullOrEmpty(s) ? string.Empty : new string(System.Linq.Enumerable.ToArray(System.Linq.Enumerable.Where(s, char.IsDigit)));

        private static decimal ParseDec(string? s) =>
            decimal.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var d) ? d : 0m;

        private static DateTime ParseFecha(string? s) =>
            DateTime.TryParse(s, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var dt) ? dt : DateTime.MinValue;

        private static T? Deserialize<T>(string? json) where T : class
        {
            if (string.IsNullOrWhiteSpace(json)) return null;
            try { return JsonSerializer.Deserialize<T>(json); }
            catch { return null; }
        }

        private static T? DeserializeOrNull<T>(string? json) where T : class
            => Deserialize<T>(json);

        private static void SafeDelete(string path)
        {
            try { System.IO.File.Delete(path); } catch { }
        }
    }
}
