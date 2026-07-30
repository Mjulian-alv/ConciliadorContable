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
        /// <summary>
        /// Las fechas se guardan como texto (MovimientosArchivo.Fecha es NVARCHAR),
        /// tal como vinieron del archivo del banco, así que hay que tolerar formatos.
        /// </summary>
        private static readonly string[] FormatosFecha =
            { "dd/MM/yyyy", "yyyy-MM-dd", "d/M/yyyy", "MM/dd/yyyy", "dd-MM-yyyy" };

        public static bool FechasIguales(string fechaExterno, string fechaExtracto)
        {
            if (string.IsNullOrWhiteSpace(fechaExterno) || string.IsNullOrWhiteSpace(fechaExtracto))
                return false;

            if (DateTime.TryParseExact(fechaExterno.Trim(), FormatosFecha, CultureInfo.InvariantCulture, DateTimeStyles.None, out var d1) &&
                DateTime.TryParseExact(fechaExtracto.Trim(), FormatosFecha, CultureInfo.InvariantCulture, DateTimeStyles.None, out var d2))
                return d1.Date == d2.Date;

            // Si alguna no parsea con ningún formato conocido, comparamos el texto crudo.
            return string.Equals(fechaExterno.Trim(), fechaExtracto.Trim(), StringComparison.OrdinalIgnoreCase);
        }

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
