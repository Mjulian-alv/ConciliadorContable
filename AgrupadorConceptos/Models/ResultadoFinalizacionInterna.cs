// Fecha: 05/09/2026 - TAREA: 00021 - Linea: 5 - Resultado de finalizar una sesion interna
using System.Collections.Generic;

namespace AgrupadorConceptos.Models
{
    /// <summary>
    /// Resultado de intentar finalizar una sesión de conciliación interna. Si algún perfil
    /// involucrado no tiene cuenta contable asignada, la finalización se bloquea entera
    /// (no finaliza nada) y acá vienen los pares que la están bloqueando, para mostrarlos.
    /// </summary>
    public class ResultadoFinalizacionInterna
    {
        public bool Exito { get; set; }
        public List<string> ParesSinCuenta { get; } = new List<string>();
    }
}
