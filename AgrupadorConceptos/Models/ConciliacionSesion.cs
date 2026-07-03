using System;
using System.Collections.Generic;
using System.Text.Json;

namespace AgrupadorConceptos.Models
{
    public class ConciliacionSesion
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public DateTime FechaCreacion { get; set; }
        public int IdArchivoImportado { get; set; }   // primer archivo (compatibilidad)
        public string ArchivosJson { get; set; }       // JSON array de ids (multi-archivo)
        public string ConceptosJson { get; set; }
        public string Estado { get; set; }

        public List<int> IdsArchivos =>
            string.IsNullOrWhiteSpace(ArchivosJson)
                ? new List<int> { IdArchivoImportado }
                : JsonSerializer.Deserialize<List<int>>(ArchivosJson) ?? new List<int> { IdArchivoImportado };

        public string DisplayName => $"{Nombre} ({FechaCreacion:dd/MM/yyyy HH:mm}) [{Estado}]";
    }
}
