// Fecha: 08/10/2026 - TAREA: 00076 - Linea: 1 - Contrato de negocio para rechazar lotes mixtos sin bajas parciales.
using System.Collections.Generic;
using AgrupadorConceptos.Data;

namespace AgrupadorConceptos.Services
{
    public static class EliminacionMovimientosService
    {
        public static void EliminarSeleccion(int idArchivo, IReadOnlyCollection<int> idsMovimientos) =>
            EliminacionMovimientosStorage.EliminarMovimientos(idArchivo, idsMovimientos);

        public static void EliminarArchivo(int idArchivo) =>
            EliminacionMovimientosStorage.EliminarArchivo(idArchivo);
    }
}
