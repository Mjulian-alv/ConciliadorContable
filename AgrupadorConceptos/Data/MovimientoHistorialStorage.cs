// Fecha: 26/10/2026 - TAREA: 00135 - Linea: 2 - Consulta del historial de cambios de un movimiento
using System.Collections.Generic;
using System.Linq;
using AgrupadorConceptos.Models;
using Dapper;

namespace AgrupadorConceptos.Data
{
    internal static class MovimientoHistorialStorage
    {
        /// <summary>Cambios del movimiento, del más reciente al más viejo. Se pueden consultar aunque el movimiento ya no exista.</summary>
        public static List<CambioMovimiento> ObtenerPorMovimiento(int idMovimiento)
        {
            using var cn = DatabaseHelper.Open();
            return cn.Query<CambioMovimiento>(@"SELECT Fecha, Campo, ValorAnterior, ValorNuevo, Origen, Usuario
                FROM bancos.MovimientosArchivoHistorial WHERE IdMovimiento = @Id ORDER BY Id DESC",
                new { Id = idMovimiento }).ToList();
        }
    }
}
