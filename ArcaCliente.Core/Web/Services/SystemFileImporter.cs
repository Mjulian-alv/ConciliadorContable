// Fecha: 05/10/2026 - TAREA: 00065 - Linea: 1 - Adaptación de archivos del sistema con el perfil vigente del escritorio.
using ArcaCliente.Web.Models;
using System.Text;
using System.Text.Json;
namespace ArcaCliente.Web.Services;
public static class SystemFileImporter
{
    public static ImportPreview Preview(UploadedDocument file,FileFormat format,IReadOnlyCollection<string>? excludedTypes=null)
    {
        var effective=format;bool legacyEncoding=false;
        if(file.Content.Length<=TabularReader.MaxBytes && Path.GetExtension(file.Name).ToLowerInvariant() is ".csv" or ".txt" && format.Encoding=="UTF-8") {
            try{new UTF8Encoding(false,true).GetString(file.Content);}
            catch(DecoderFallbackException){
                effective=JsonSerializer.Deserialize<FileFormat>(JsonSerializer.Serialize(format))!;
                effective.Encoding="Latin-1";legacyEncoding=true;
            }
        }
        var result=ProviderImporter.Preview(file,effective,excludedTypes:excludedTypes);
        if(legacyEncoding)result.Warnings.Add("El archivo no está codificado en UTF-8. Se leyó como Latin-1; revisá los nombres en la vista previa. La configuración del perfil no se modificó.");
        return result;
    }
}
