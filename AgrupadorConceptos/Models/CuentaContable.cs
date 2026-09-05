// Fecha: 05/09/2026 - TAREA: 00021 - Linea: 1 - Cuenta contable importada del sistema legacy
namespace AgrupadorConceptos.Models
{
    public class CuentaContable
    {
        public int Id { get; set; }
        public string Cuenta { get; set; }
        public string Descripcion { get; set; }
        public string CentroCosto { get; set; }

        // Fecha: 05/09/2026 - TAREA: 00021 - Linea: 1 - Sin Cuenta (sentinel "sin asignar") no debe llevar guion
        public string DisplayName
        {
            get
            {
                if (string.IsNullOrEmpty(Cuenta)) return Descripcion;
                return string.IsNullOrEmpty(CentroCosto)
                    ? $"{Cuenta} — {Descripcion}"
                    : $"{Cuenta} — {Descripcion} ({CentroCosto})";
            }
        }
    }
}
