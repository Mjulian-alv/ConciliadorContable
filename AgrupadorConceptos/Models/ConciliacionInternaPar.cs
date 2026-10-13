// Fecha: 05/09/2026 - TAREA: 00021 - Linea: 5 - Par conciliado entre los dos extractos
using System;

namespace AgrupadorConceptos.Models
{
    public class ConciliacionInternaPar
    {
        public int Id { get; set; }
        public int IdSesion { get; set; }
        public int IdMovimientoA { get; set; }
        public int IdMovimientoB { get; set; }
        public string TipoMatch { get; set; }
        public DateTime FechaConciliacion { get; set; }

        // Campos de visualización (no persisten)
        // Fecha: 13/10/2026 - TAREA: 00083 - Linea: 1 - Operar sobre días tipados evita reinterpretaciones culturales.
        public System.DateTime FechaA { get; set; }
        public decimal ImporteA { get; set; }
        public string ConceptoFinalA { get; set; }
        // Fecha: 13/10/2026 - TAREA: 00083 - Linea: 1 - Operar sobre días tipados evita reinterpretaciones culturales.
        public System.DateTime FechaB { get; set; }
        public decimal ImporteB { get; set; }
        public string ConceptoFinalB { get; set; }

        // Fecha: 05/09/2026 - TAREA: 00021 - Linea: 5 - Para resolver el perfil de cada lado al finalizar
        // Ya no hay un perfil fijo por sesion (cada par puede venir de dos bancos distintos), asi
        // que Finalizar necesita el archivo de cada movimiento para llegar a su PerfilBanco.
        public int IdArchivoA { get; set; }
        public int IdArchivoB { get; set; }

        /// <summary>
        /// para mostrar el nombre del banco y poder identificar cualquier error de conciliacion. 
        /// </summary>
        public string NombreBancoA { get; set; }
        public string NombreBancoB { get; set; }


    }
}
