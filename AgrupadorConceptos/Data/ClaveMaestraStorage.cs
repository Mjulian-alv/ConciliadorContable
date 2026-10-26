// Fecha: 26/10/2026 - TAREA: 00135 - Linea: 3 - Clave maestra del blindaje: sólo el hash, la sal y el control de intentos
using System;
using Dapper;
using Microsoft.Data.SqlClient;

namespace AgrupadorConceptos.Data
{
    internal sealed class ClaveMaestraRegistro
    {
        public byte[] Hash { get; set; }
        public byte[] Sal { get; set; }
        public int Iteraciones { get; set; }
        public DateTime FechaCambio { get; set; }
        public string CambiadoPor { get; set; }
        public int IntentosFallidos { get; set; }
        /// <summary>Segundos que faltan para que se levante el bloqueo; 0 si no hay. Lo calcula la base para no depender del reloj del cliente.</summary>
        public int SegundosBloqueo { get; set; }
    }

    internal static class ClaveMaestraStorage
    {
        // Fecha: 26/10/2026 - TAREA: 00135 - Linea: 3 - Revisión 1, hallazgo 1: la verificación y el conteo de intentos se serializan entre todos los procesos
        // Sin esto, varias solicitudes en paralelo leían «sin bloqueo» antes de que alguna registrara el quinto fallo y todas llegaban a comparar su clave:
        // se podían probar más de cinco claves a la vez. El bloqueo de aplicación es de la base, así que vale entre varios servidores y escritorios.
        /// <summary>Toma el bloqueo exclusivo de la verificación de clave; liberarlo con Dispose.</summary>
        public static IDisposable BloquearVerificacion()
        {
            var cn = DatabaseHelper.Open();
            try
            {
                int r = cn.ExecuteScalar<int>(@"DECLARE @r int; EXEC @r = sp_getapplock @Resource = N'bancos.ClaveMaestra', @LockMode = N'Exclusive',
                    @LockOwner = N'Session', @LockTimeout = 15000; SELECT @r");
                if (r < 0) throw new InvalidOperationException("La clave maestra está siendo verificada por otra operación. Probá de nuevo en unos segundos.");
            }
            catch { cn.Dispose(); throw; }
            return new Verificacion(cn);
        }

        private sealed class Verificacion : IDisposable
        {
            private SqlConnection _cn;
            public Verificacion(SqlConnection cn) => _cn = cn;
            public void Dispose()
            {
                var cn = _cn; _cn = null;
                if (cn == null) return;
                try { cn.Execute("EXEC sp_releaseapplock @Resource = N'bancos.ClaveMaestra', @LockOwner = N'Session'"); } catch { }
                cn.Dispose(); // cerrar la conexión también libera un bloqueo de sesión
            }
        }

        public static ClaveMaestraRegistro Obtener()
        {
            using var cn = DatabaseHelper.Open();
            return cn.QuerySingleOrDefault<ClaveMaestraRegistro>(@"SELECT Hash, Sal, Iteraciones, FechaCambio, CambiadoPor, IntentosFallidos,
                    CASE WHEN BloqueadoHasta > SYSDATETIME() THEN DATEDIFF(SECOND, SYSDATETIME(), BloqueadoHasta) ELSE 0 END AS SegundosBloqueo
                FROM bancos.ClaveMaestra WHERE Id = 1");
        }

        /// <summary>Guarda la clave (alta o cambio) y reinicia los intentos.</summary>
        public static void Guardar(byte[] hash, byte[] sal, int iteraciones, string usuario)
        {
            using var cn = DatabaseHelper.Open();
            using var tx = cn.BeginTransaction();
            cn.Execute(@"MERGE bancos.ClaveMaestra AS t
                USING (SELECT 1 AS Id) AS s ON t.Id = s.Id
                WHEN MATCHED THEN UPDATE SET Hash = @Hash, Sal = @Sal, Iteraciones = @It, FechaCambio = SYSDATETIME(), CambiadoPor = @Usuario,
                                             IntentosFallidos = 0, BloqueadoHasta = NULL
                WHEN NOT MATCHED THEN INSERT (Id, Hash, Sal, Iteraciones, FechaCambio, CambiadoPor) VALUES (1, @Hash, @Sal, @It, SYSDATETIME(), @Usuario);",
                new { Hash = hash, Sal = sal, It = iteraciones, Usuario = usuario }, tx);
            tx.Commit();
        }

        /// <summary>Suma un intento fallido; al llegar al máximo bloquea y reinicia la cuenta. Una sola sentencia: no se pierden intentos concurrentes.</summary>
        public static void RegistrarFallo(int maxIntentos, int minutosBloqueo)
        {
            using var cn = DatabaseHelper.Open();
            cn.Execute(@"UPDATE bancos.ClaveMaestra SET
                    BloqueadoHasta   = CASE WHEN IntentosFallidos + 1 >= @Max THEN DATEADD(MINUTE, @Min, SYSDATETIME()) ELSE BloqueadoHasta END,
                    IntentosFallidos = CASE WHEN IntentosFallidos + 1 >= @Max THEN 0 ELSE IntentosFallidos + 1 END
                WHERE Id = 1", new { Max = maxIntentos, Min = minutosBloqueo });
        }

        public static void ReiniciarIntentos()
        {
            using var cn = DatabaseHelper.Open();
            cn.Execute("UPDATE bancos.ClaveMaestra SET IntentosFallidos = 0, BloqueadoHasta = NULL WHERE Id = 1 AND (IntentosFallidos <> 0 OR BloqueadoHasta IS NOT NULL)");
        }
    }
}
