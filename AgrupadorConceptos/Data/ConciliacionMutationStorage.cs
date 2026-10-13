// Fecha: 08/10/2026 - TAREA: 00076 - Linea: 1, 2 - Serializar las decisiones que dependen de sesiones y movimientos.
using System;
using System.Data;
using Dapper;

namespace AgrupadorConceptos.Data
{
    internal static class ConciliacionMutationStorage
    {
        public static void Bloquear(IDbConnection cn, IDbTransaction tx)
        {
            int resultado = cn.ExecuteScalar<int>(@"DECLARE @resultado int;
                EXEC @resultado = sys.sp_getapplock @Resource = N'bancos.Conciliacion.Mutacion',
                    @LockMode = N'Exclusive', @LockOwner = N'Transaction', @LockTimeout = 15000;
                SELECT @resultado;", transaction: tx);
            if (resultado < 0)
                throw new InvalidOperationException("Hay otra operación de conciliación en curso. Actualizá e intentá nuevamente.");
        }
    }
}
