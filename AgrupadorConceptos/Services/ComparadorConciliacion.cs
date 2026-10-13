using System;
using System.Globalization;
using AgrupadorConceptos.Models;

namespace AgrupadorConceptos.Services
{
    /// <summary>
    /// Criterios de igualdad usados para emparejar un ítem del archivo externo
    /// con un movimiento del extracto.
    ///
    /// Vive acá y es público a propósito: antes FechasIguales era private en
    /// ConciliacionExternService, así que ConciliacionExternForm tenía una copia
    /// para el resaltado manual, y las dos versiones divergieron (a la copia le
    /// faltaba el formato "dd-MM-yyyy"). La auto-conciliación y el resaltado
    /// visual tienen que usar exactamente el mismo criterio.
    /// </summary>
    public static class ComparadorConciliacion
    {
        // Fecha: 13/10/2026 - TAREA: 00083 - Linea: 1 - Operar sobre días tipados evita reinterpretaciones culturales.
        public static bool FechasIguales(DateTime fechaExterno, DateTime fechaExtracto) => fechaExterno.Date == fechaExtracto.Date;

        /// <summary>
        /// El movimiento trae el importe en Debitos o en Creditos según el signo;
        /// para comparar contra el externo se usa el valor absoluto del que esté cargado.
        /// </summary>
        public static decimal ImporteEfectivo(MovimientoProcesado m) =>
            m.Debitos != 0 ? Math.Abs(m.Debitos) : Math.Abs(m.Creditos);

        public static bool ImportesIguales(decimal importeExterno, MovimientoProcesado m) =>
            Math.Abs(importeExterno) == ImporteEfectivo(m);
    }
}
