// Fecha: 26/10/2026 - TAREA: 00135 - Linea: 3 - Clave maestra del blindaje: sólo el hash, la sal y el control de intentos
using System;
using Dapper;

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
