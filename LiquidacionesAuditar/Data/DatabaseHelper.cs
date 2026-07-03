using System;
using System.IO;
using Microsoft.Data.Sqlite;

namespace LiquidacionesAuditar.Data
{
    public static class DatabaseHelper
    {
        public static string DbPath { get; } =
            Path.Combine(AppContext.BaseDirectory, "liquidaciones_auditar.db");

        public static SqliteConnection GetConnection() =>
            new SqliteConnection($"Data Source={DbPath}");

        public static void InitializeDatabase()
        {
            using var cn = GetConnection();
            cn.Open();

            // Marcas de tarjeta / procesadores
            cn.ExecuteNonQuery(@"
                CREATE TABLE IF NOT EXISTS MarcasTarjeta (
                    id   TEXT PRIMARY KEY,
                    nombre TEXT NOT NULL
                )");

            // Columnas posibles del CSV por marca
            cn.ExecuteNonQuery(@"
                CREATE TABLE IF NOT EXISTS Auditar_ColumnasCSV (
                    id               INTEGER PRIMARY KEY AUTOINCREMENT,
                    idMarcaTarjeta   TEXT NOT NULL,
                    idColumnaArchivo TEXT NOT NULL
                )");

            // Columnas del archivo destino
            cn.ExecuteNonQuery(@"
                CREATE TABLE IF NOT EXISTS Auditar_LiqCols (
                    id          INTEGER PRIMARY KEY AUTOINCREMENT,
                    idColumna   TEXT NOT NULL,
                    TipoRegistro TEXT,
                    TipoDato    TEXT,
                    Orden       INTEGER NOT NULL DEFAULT 0,
                    ValorFijo   TEXT,
                    Condicion   TEXT
                )");

            // Relacion columnas destino <-> columnas CSV origen
            cn.ExecuteNonQuery(@"
                CREATE TABLE IF NOT EXISTS Auditar_RelacionCols (
                    IdLiqCols     TEXT NOT NULL,
                    IdColumnasCSV TEXT NOT NULL,
                    PRIMARY KEY (IdLiqCols, IdColumnasCSV)
                )");

            // Lineas de registro CON (totalizador): definicion de cada linea CON
            cn.ExecuteNonQuery(@"
                CREATE TABLE IF NOT EXISTS Auditar_LineasCON (
                    id           INTEGER PRIMARY KEY AUTOINCREMENT,
                    idMarcaTarjeta TEXT NOT NULL,
                    Descripcion  TEXT NOT NULL,
                    Orden        INTEGER NOT NULL DEFAULT 0
                )");

            // Columnas que se suman en cada linea CON
            cn.ExecuteNonQuery(@"
                CREATE TABLE IF NOT EXISTS Auditar_CON_Cols (
                    IdLineaCON    INTEGER NOT NULL,
                    IdColumnasCSV TEXT NOT NULL,
                    PRIMARY KEY (IdLineaCON, IdColumnasCSV)
                )");

            // Valores fijos adicionales de cada linea CON (ej: etiqueta descriptiva)
            cn.ExecuteNonQuery(@"
                CREATE TABLE IF NOT EXISTS Auditar_CON_ValoresFijos (
                    id          INTEGER PRIMARY KEY AUTOINCREMENT,
                    IdLineaCON  INTEGER NOT NULL,
                    Posicion    INTEGER NOT NULL,
                    Valor       TEXT NOT NULL
                )");

            // Columnas nuevas en MarcasTarjeta (migracion segura)
            try { cn.ExecuteNonQuery("ALTER TABLE MarcasTarjeta ADD COLUMN FilaEncabezado INTEGER NOT NULL DEFAULT 1"); } catch { }
            try { cn.ExecuteNonQuery("ALTER TABLE MarcasTarjeta ADD COLUMN ColumnaLiquidacion TEXT"); } catch { }
            try { cn.ExecuteNonQuery("ALTER TABLE MarcasTarjeta ADD COLUMN TipoDato TEXT"); } catch { }
            try { cn.ExecuteNonQuery("ALTER TABLE MarcasTarjeta ADD COLUMN ColumnaFiltro TEXT"); } catch { }

            // Columna nueva en Auditar_LiqCols (migracion segura)
            try { cn.ExecuteNonQuery("ALTER TABLE Auditar_LiqCols ADD COLUMN idMarcaTarjeta TEXT NOT NULL DEFAULT ''"); } catch { } 
            try { cn.ExecuteNonQuery("ALTER TABLE Auditar_LiqCols ADD COLUMN CondicionSigno TEXT"); } catch { }
            try { cn.ExecuteNonQuery("ALTER TABLE Auditar_LiqCols ADD COLUMN tieneSigno INTEGER NOT NULL DEFAULT 0"); } catch { }
            try { cn.ExecuteNonQuery("ALTER TABLE Auditar_LiqCols ADD COLUMN esFiltro INTEGER NOT NULL DEFAULT 0"); } catch { }

            //Columna nueva en Auditar_LineasCON (migracion segura)
            try { cn.ExecuteNonQuery("ALTER TABLE Auditar_LineasCON ADD COLUMN CondicionSigno TEXT"); } catch { }

            //Columnas nuevas en MarcasTarjeta (migracion segura)
            try { cn.ExecuteNonQuery("ALTER TABLE MarcasTarjeta ADD COLUMN SeparadorDecimales TEXT NOT NULL DEFAULT ','"); } catch { }
            try { cn.ExecuteNonQuery("ALTER TABLE MarcasTarjeta ADD COLUMN SeparadorMiles TEXT NOT NULL DEFAULT '.'"); } catch { }
        }
    }

    public static class SqliteConnectionExtensions
    {
        public static void ExecuteNonQuery(this SqliteConnection cn, string sql)
        {
            using var cmd = cn.CreateCommand();
            cmd.CommandText = sql;
            cmd.ExecuteNonQuery();
        }
    }
}
