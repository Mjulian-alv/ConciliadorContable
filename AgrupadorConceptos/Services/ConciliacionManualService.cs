// Fecha: 23/10/2026 - TAREA: 00125 - Linea: 3 - Marcado manual de movimientos como conciliados desde el procesador.
using System.Collections.Generic;
using AgrupadorConceptos.Data;

namespace AgrupadorConceptos.Services
{
    public static class ConciliacionManualService
    {
        /// <summary>
        /// Marca como conciliados los movimientos libres. Los que están en un par de conciliación
        /// (interna o externa, en proceso o finalizada) o ya estaban conciliados se omiten con su motivo.
        /// </summary>
        public static (int Marcados, List<(int Id, string Motivo)> Omitidos) MarcarConciliados(IReadOnlyCollection<int> idsMovimientos) =>
            MovimientoStorage.MarcarConciliados(idsMovimientos);
    }
}
