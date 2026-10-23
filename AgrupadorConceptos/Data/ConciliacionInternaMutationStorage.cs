// Fecha: 08/10/2026 - TAREA: 00076 - Linea: 1 - Mutaciones internas coordinadas con el borrado seguro.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using AgrupadorConceptos.Models;
using Dapper;
using Microsoft.Data.SqlClient;

namespace AgrupadorConceptos.Data
{
    internal static class ConciliacionInternaMutationStorage
    {
        // Fecha: 13/10/2026 - TAREA: 00083 - Linea: 1 - Operar sobre días tipados evita reinterpretaciones culturales.
        public static List<ConciliacionInternaPar> ObtenerPares(int idSesion)
        {
            using var cn = DatabaseHelper.GetConnection();
            cn.Open();
            return cn.Query<ConciliacionInternaPar>(@"
                SELECT p.*,
                      a.Fecha  AS FechaA, CASE WHEN a.Debitos <> 0 THEN a.Debitos ELSE a.Creditos END AS ImporteA, a.ConceptoFinal AS ConceptoFinalA, a.IdArchivo AS IdArchivoA, pa.NombreBanco as NombreBancoA,
                      b.Fecha  AS FechaB, CASE WHEN b.Debitos <> 0 THEN b.Debitos ELSE b.Creditos END AS ImporteB, b.ConceptoFinal AS ConceptoFinalB, b.IdArchivo AS IdArchivoB, pb.NombreBanco as NombreBancoB
                FROM bancos.ConciliacionInternaPares p
                JOIN bancos.MovimientosArchivo a ON p.IdMovimientoA = a.Id
                JOIN bancos.MovimientosArchivo b ON p.IdMovimientoB = b.Id
                JOIN bancos.ArchivosImportados ia on a.IdArchivo = ia.Id
                JOIN bancos.ArchivosImportados ib on b.IdArchivo = ib.Id
                join bancos.PerfilesBanco pa on pa.Id = ia.IdPerfilBanco
                join bancos.PerfilesBanco pb on pb.Id = ib.IdPerfilBanco
                WHERE p.IdSesion = @Id
                ORDER BY p.FechaConciliacion",
                new { Id = idSesion }).ToList();
        }

        public static ConciliacionInternaSesion CrearSesion(string nombre, DateTime desde, DateTime hasta, IEnumerable<string> conceptos)
        {
            using var cn = DatabaseHelper.Open();
            using var tx = cn.BeginTransaction();
            ConciliacionMutationStorage.Bloquear(cn, tx);
            int id = cn.QuerySingle<int>(@"INSERT INTO bancos.ConciliacionInternaSesiones
                (Nombre, FechaCreacion, FechaDesde, FechaHasta, ConceptosJson, Estado)
                OUTPUT INSERTED.Id VALUES (@Nombre, @Fecha, @Desde, @Hasta, @Conceptos, 'EnProceso')",
                new { Nombre = nombre, Fecha = DateTime.Now, Desde = desde.Date, Hasta = hasta.Date,
                    Conceptos = JsonSerializer.Serialize(conceptos.ToList()) }, tx);
            tx.Commit();
            return cn.QuerySingle<ConciliacionInternaSesion>(
                "SELECT * FROM bancos.ConciliacionInternaSesiones WHERE Id = @Id", new { Id = id });
        }

        public static void EliminarSesion(int idSesion)
        {
            using var cn = DatabaseHelper.Open();
            using var tx = cn.BeginTransaction();
            ConciliacionMutationStorage.Bloquear(cn, tx);
            cn.Execute("DELETE FROM bancos.ConciliacionInternaPares WHERE IdSesion = @Id", new { Id = idSesion }, tx);
            cn.Execute("DELETE FROM bancos.ConciliacionInternaSesiones WHERE Id = @Id", new { Id = idSesion }, tx);
            tx.Commit();
        }

        public static string ConciliarPar(int idSesion, int idMovimientoA, int idMovimientoB, TipoMatch tipoMatch)
        {
            using var cn = DatabaseHelper.Open();
            using var tx = cn.BeginTransaction();
            ConciliacionMutationStorage.Bloquear(cn, tx);
            string estado = cn.QuerySingleOrDefault<string>("SELECT Estado FROM bancos.ConciliacionInternaSesiones WHERE Id = @Id", new { Id = idSesion }, tx);
            if (estado != "EnProceso") return "La sesión ya no está en proceso. Actualizá la pantalla.";
            int[] ids = { idMovimientoA, idMovimientoB };
            int encontrados = cn.ExecuteScalar<int>("SELECT COUNT(*) FROM bancos.MovimientosArchivo WHERE Id IN @Ids", new { Ids = ids }, tx);
            if (idMovimientoA == idMovimientoB || encontrados != 2) return "Uno de los movimientos ya no existe. Actualizá la pantalla.";
            // Fecha: 23/10/2026 - TAREA: 00125 - Linea: 3 - Un movimiento ya marcado como conciliado no entra en otro par
            if (cn.ExecuteScalar<int>("SELECT COUNT(*) FROM bancos.MovimientosArchivo WHERE Id IN @Ids AND Conciliado = 1", new { Ids = ids }, tx) != 0)
                return "Uno de los movimientos ya figura como conciliado. Actualizá la pantalla.";
            int ocupados = cn.ExecuteScalar<int>(@"SELECT COUNT(*) FROM bancos.ConciliacionInternaPares
                WHERE IdSesion = @Sesion AND (IdMovimientoA IN @Ids OR IdMovimientoB IN @Ids)",
                new { Sesion = idSesion, Ids = ids }, tx);
            if (ocupados != 0) return "Uno de los movimientos ya está conciliado en esta sesión. Actualizá la pantalla.";
            string conflicto = cn.QueryFirstOrDefault<string>(@"SELECT TOP 1 s.Nombre
                FROM bancos.ConciliacionInternaPares p JOIN bancos.ConciliacionInternaSesiones s ON s.Id = p.IdSesion
                WHERE (p.IdMovimientoA IN @Ids OR p.IdMovimientoB IN @Ids) AND s.Estado = 'EnProceso' AND s.Id <> @Sesion",
                new { Ids = ids, Sesion = idSesion }, tx);
            if (conflicto != null)
                return $"Uno de los dos movimientos ya está conciliado en la sesión \"{conflicto}\", todavía en proceso. Cerrala o desconcilialo ahí antes de continuar.";
            try
            {
                cn.Execute(@"INSERT INTO bancos.ConciliacionInternaPares
                    (IdSesion, IdMovimientoA, IdMovimientoB, TipoMatch, FechaConciliacion)
                    VALUES (@IdSesion, @A, @B, @Tipo, @Fecha)",
                    new { IdSesion = idSesion, A = idMovimientoA, B = idMovimientoB,
                        Tipo = tipoMatch.ToString(), Fecha = DateTime.Now }, tx);
                tx.Commit();
            }
            catch (SqlException ex) when (ex.Number is 2601 or 2627)
            {
                return "Uno de los movimientos ya quedó conciliado con otro par en esta sesión. Actualizá la pantalla.";
            }
            return null;
        }

        public static void DesconciliarPar(int idPar)
        {
            using var cn = DatabaseHelper.Open();
            using var tx = cn.BeginTransaction();
            ConciliacionMutationStorage.Bloquear(cn, tx);
            int cambios = cn.Execute(@"DELETE p FROM bancos.ConciliacionInternaPares p
                JOIN bancos.ConciliacionInternaSesiones s ON s.Id = p.IdSesion
                WHERE p.Id = @Id AND s.Estado = 'EnProceso'", new { Id = idPar }, tx);
            if (cambios == 0) throw new InvalidOperationException("El par ya no está disponible en una sesión en proceso.");
            tx.Commit();
        }

        public static void Finalizar(int idSesion, IReadOnlyCollection<(int IdMovimientoA, string CuentaParaA, int IdMovimientoB, string CuentaParaB)> cuentas)
        {
            using var cn = DatabaseHelper.Open();
            using var tx = cn.BeginTransaction();
            ConciliacionMutationStorage.Bloquear(cn, tx);
            string estado = cn.QuerySingleOrDefault<string>("SELECT Estado FROM bancos.ConciliacionInternaSesiones WHERE Id = @Id", new { Id = idSesion }, tx);
            if (estado != "EnProceso") throw new InvalidOperationException("La sesión ya no está en proceso. Actualizá la pantalla.");
            var paresActuales = cn.Query<(int IdMovimientoA, int IdMovimientoB)>(
                "SELECT IdMovimientoA, IdMovimientoB FROM bancos.ConciliacionInternaPares WHERE IdSesion = @Id", new { Id = idSesion }, tx).ToList();
            if (paresActuales.Count != cuentas.Count || paresActuales.Any(p => !cuentas.Any(c => c.IdMovimientoA == p.IdMovimientoA && c.IdMovimientoB == p.IdMovimientoB)))
                throw new InvalidOperationException("Los pares cambiaron durante el cierre. Actualizá la pantalla.");
            foreach (var cuenta in cuentas)
            {
                // Fecha: 23/10/2026 - TAREA: 00125 - Linea: 1 - El cierre deja el movimiento marcado como conciliado, con su fecha
                // GETDATE() ya es la fecha del servidor: CAST a date evita reinterpretar un texto con estilo 103.
                int a = cn.Execute("UPDATE bancos.MovimientosArchivo SET CuentaFinal = @Cuenta, Conciliado = 1, ConciliadoFecha = CAST(GETDATE() AS date) WHERE Id = @Id",
                    new { Cuenta = cuenta.CuentaParaA, Id = cuenta.IdMovimientoA }, tx);
                int b = cn.Execute("UPDATE bancos.MovimientosArchivo SET CuentaFinal = @Cuenta, Conciliado = 1, ConciliadoFecha = CAST(GETDATE() AS date) WHERE Id = @Id",
                    new { Cuenta = cuenta.CuentaParaB, Id = cuenta.IdMovimientoB }, tx);
                if (a != 1 || b != 1) throw new InvalidOperationException("Un movimiento del cierre ya no existe. Actualizá la pantalla.");
            }
            // Fecha: 23/10/2026 - TAREA: 00125 - Linea: 1 - Fecha de finalización de la sesión
            cn.Execute("UPDATE bancos.ConciliacionInternaSesiones SET Estado = 'Finalizada', FinalizacionFecha = CAST(GETDATE() AS date) WHERE Id = @Id", new { Id = idSesion }, tx);
            tx.Commit();
        }
    }
}
