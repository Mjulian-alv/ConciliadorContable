// Fecha: 05/10/2026 - TAREA: 00064 - Linea: 2 - La sustitución de CUIT se entrega completa únicamente cuando todas las filas son válidas.
using ArcaCliente.Web.Models;
using System.Text.RegularExpressions;
namespace ArcaCliente.Web.Services;
public static class CuitListImporter
{
    public static string Normalize(string value) {
        string clean=Regex.Replace(value??"",@"[\s-]","");
        if(!Regex.IsMatch(clean,@"^[0-9]{11}$"))throw new ImportException("El CUIT debe tener 11 dígitos; sólo se admiten espacios y guiones como formato.");
        return clean;
    }
    public static List<string> Parse(UploadedDocument file) {
        var data=TabularReader.Read(file,new FileFormat{Header=false});var values=new HashSet<string>();
        foreach(var row in data.Rows) {
            if(row.Line==1 && row.Cells[0].Trim().Equals("CUIT",StringComparison.OrdinalIgnoreCase))continue;
            if(row.Cells.Length!=1)throw new ImportException($"Fila {row.Line}: la lista debe tener una sola columna CUIT.");
            try{values.Add(Normalize(row.Cells[0]));}catch(ImportException){throw new ImportException($"Fila {row.Line}: CUIT inválido. La lista anterior se conserva.");}
        }
        if(values.Count==0)throw new ImportException("La lista está vacía.");return values.Order().ToList();
    }
}
