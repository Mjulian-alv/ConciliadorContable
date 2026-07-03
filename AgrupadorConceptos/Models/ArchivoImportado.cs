namespace AgrupadorConceptos.Models
{
    public class ArchivoImportado
    {
        public int Id { get; set; }
        public int IdPerfilBanco { get; set; }
        public string NombreArchivo { get; set; }
        public System.DateTime Fecha { get; set; }
        public string DisplayName => $"{NombreArchivo} ({Fecha:dd/MM/yyyy HH:mm})";
    }
}