using Microsoft.Data.Sqlite;
using Dapper;
using System.Data;
using System.IO;

namespace AgrupadorConceptos.Data
{
    public static class DatabaseHelper
    {
        private static string DbPath = "DataBanks.db";
        private static string ConnectionString => $"Data Source={DbPath};";

        public static void InitializeDatabase()
        {
            if (!File.Exists(DbPath))
            {
                File.Create(DbPath).Close();
            }

            using var connection = GetConnection();
            connection.Open();

            // Crear tabla PerfilesBanco
            connection.Execute(@"
                CREATE TABLE IF NOT EXISTS PerfilesBanco (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    NombreBanco TEXT NOT NULL,
                    ColumnaConcepto TEXT NOT NULL,
                    ColumnaDescripcion TEXT,
                    EsCodigo INTEGER NOT NULL,
                    FilaEncabezado INTEGER NOT NULL DEFAULT 1,
                    TipoImporte INTEGER NOT NULL,
                    ColumnaImporteUnico TEXT,
                    ColumnaDebe TEXT,
                    ColumnaHaber TEXT
                );
            ");

            // Intentar agregar la columna por si la base de datos ya fue creada en una corrida anterior
            try { connection.Execute("ALTER TABLE PerfilesBanco ADD COLUMN FilaEncabezado INTEGER NOT NULL DEFAULT 1;"); } catch { }
            try { connection.Execute("ALTER TABLE PerfilesBanco ADD COLUMN ColumnaDescripcion TEXT;"); } catch { }
            try { connection.Execute("ALTER TABLE PerfilesBanco ADD COLUMN ColumnaFecha TEXT;"); } catch { }

            // Crear tabla ConceptosEstandar
            connection.Execute(@"
                CREATE TABLE IF NOT EXISTS ConceptosEstandar (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Nombre TEXT NOT NULL UNIQUE
                );
            ");

            // Crear tabla HomologacionConceptos
            connection.Execute(@"
                CREATE TABLE IF NOT EXISTS HomologacionConceptos (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    IdPerfilBanco INTEGER NOT NULL,
                    ValorOriginal TEXT NOT NULL,
                    IdConceptoEstandar INTEGER NOT NULL,
                    FOREIGN KEY (IdPerfilBanco) REFERENCES PerfilesBanco(Id),
                    FOREIGN KEY (IdConceptoEstandar) REFERENCES ConceptosEstandar(Id)
                );
            ");

            // Crear tabla ArchivosImportados
            connection.Execute(@"
                CREATE TABLE IF NOT EXISTS ArchivosImportados (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    IdPerfilBanco INTEGER NOT NULL,
                    NombreArchivo TEXT NOT NULL,
                    Fecha DATETIME NOT NULL
                );
            ");

            // Crear tabla MovimientosArchivo
            connection.Execute(@"
                CREATE TABLE IF NOT EXISTS MovimientosArchivo (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    IdArchivo INTEGER NOT NULL,
                    Fecha TEXT,
                    ConceptoOriginal TEXT,
                    DescripcionOriginal TEXT,
                    Debitos REAL,
                    Creditos REAL,
                    ConceptoEstandar TEXT,
                    ConceptoFinal TEXT,
                    FOREIGN KEY (IdArchivo) REFERENCES ArchivosImportados(Id) ON DELETE CASCADE
                );
            ");

            try { connection.Execute("ALTER TABLE MovimientosArchivo ADD COLUMN Fecha TEXT;"); } catch { }

            // Tabla sesiones de conciliación externa
            connection.Execute(@"
                CREATE TABLE IF NOT EXISTS ConciliacionSesiones (
                    Id                 INTEGER PRIMARY KEY AUTOINCREMENT,
                    Nombre             TEXT NOT NULL,
                    FechaCreacion      DATETIME NOT NULL,
                    IdArchivoImportado INTEGER NOT NULL,
                    ConceptosJson      TEXT NOT NULL DEFAULT '[]',
                    Estado             TEXT NOT NULL DEFAULT 'EnProceso',
                    FOREIGN KEY (IdArchivoImportado) REFERENCES ArchivosImportados(Id)
                );
            ");
            try { connection.Execute("ALTER TABLE ConciliacionSesiones ADD COLUMN ArchivosJson TEXT;"); } catch { }

            // Tabla ítems del archivo externo por sesión
            connection.Execute(@"
                CREATE TABLE IF NOT EXISTS ConciliacionItemsExternos (
                    Id         INTEGER PRIMARY KEY AUTOINCREMENT,
                    IdSesion   INTEGER NOT NULL,
                    Fecha      TEXT,
                    Importe    REAL NOT NULL,
                    Detalle    TEXT,
                    Conciliado INTEGER NOT NULL DEFAULT 0,
                    FOREIGN KEY (IdSesion) REFERENCES ConciliacionSesiones(Id) ON DELETE CASCADE
                );
            ");

            // Tabla pares conciliados
            connection.Execute(@"
                CREATE TABLE IF NOT EXISTS ConciliacionPares (
                    Id                    INTEGER PRIMARY KEY AUTOINCREMENT,
                    IdSesion              INTEGER NOT NULL,
                    IdItemExterno         INTEGER NOT NULL,
                    IdMovimientoProcesado INTEGER NOT NULL,
                    TipoMatch             TEXT NOT NULL,
                    FechaConciliacion     DATETIME NOT NULL,
                    FOREIGN KEY (IdSesion)      REFERENCES ConciliacionSesiones(Id)      ON DELETE CASCADE,
                    FOREIGN KEY (IdItemExterno) REFERENCES ConciliacionItemsExternos(Id) ON DELETE CASCADE
                );
            ");

            // Índices sobre las FK que se usan como filtro. Sin ellos cada lectura por
            // IdArchivo / IdSesion / IdPerfilBanco recorre la tabla entera.
            connection.Execute(@"
                CREATE INDEX IF NOT EXISTS IX_MovimientosArchivo_IdArchivo   ON MovimientosArchivo(IdArchivo);
                CREATE INDEX IF NOT EXISTS IX_HomologacionConceptos_IdPerfil ON HomologacionConceptos(IdPerfilBanco);
                CREATE INDEX IF NOT EXISTS IX_ArchivosImportados_IdPerfil    ON ArchivosImportados(IdPerfilBanco);
                CREATE INDEX IF NOT EXISTS IX_ConciliacionItemsExt_IdSesion  ON ConciliacionItemsExternos(IdSesion);
                CREATE INDEX IF NOT EXISTS IX_ConciliacionPares_IdSesion     ON ConciliacionPares(IdSesion);
            ");
        }

        /// <summary>
        /// WAL + synchronous=NORMAL. Sin esto SQLite hace un fsync por cada commit,
        /// lo que vuelve inviable cualquier escritura fila por fila.
        /// synchronous es por conexión, así que se aplica en cada apertura.
        /// </summary>
        public static SqliteConnection GetConnection()
        {
            var cn = new SqliteConnection(ConnectionString);
            cn.StateChange += (s, e) =>
            {
                if (e.CurrentState != ConnectionState.Open) return;
                ((SqliteConnection)s).Execute(
                    "PRAGMA journal_mode=WAL; PRAGMA synchronous=NORMAL;");
            };
            return cn;
        }
    }
}