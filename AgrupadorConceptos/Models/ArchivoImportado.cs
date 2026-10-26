namespace AgrupadorConceptos.Models
{
    public class ArchivoImportado
    {
        public int Id { get; set; }
        public int IdPerfilBanco { get; set; }
        public string NombreArchivo { get; set; }
        public System.DateTime Fecha { get; set; }
        // Fecha: 26/10/2026 - TAREA: 00135 - Linea: 3 - Blindaje (cierre de homologación) y baja como archivo inactivo
        public bool Blindado { get; set; }
        public System.DateTime? BlindadoFecha { get; set; }
        public string BlindadoPor { get; set; }
        public bool Activo { get; set; } = true;
        public string DisplayName => $"{NombreArchivo} ({Fecha:dd/MM/yyyy HH:mm})";
    }
}