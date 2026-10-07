// Fecha: 06/10/2026 - TAREA: 00072 - Linea: 3,4 - Adapta bytes web a los importadores existentes sin duplicar su interpretación fiscal.
using ArcaCliente.Models;
using ArcaCliente.Services;
using ArcaCliente.Web.Models;
using System.IO.Compression;
namespace ArcaCliente.Web.Services;
public static class PyRFileAdapter
{
    public static MayorPreseaPyR ImportMajor(UploadedDocument file, PerfilOfflinePyR profile, CuentaPyR account)
    {
        if(file.Content.Length==0 || file.Content.Length>TabularReader.MaxBytes)throw new ImportException("El mayor debe tener contenido y no superar 20 MB.");
        if(!new[]{".xls",".xlsx"}.Contains(Path.GetExtension(file.Name).ToLowerInvariant()))throw new ImportException("El mayor debe ser un archivo Excel XLS o XLSX.");
        if(file.Content.Length>2 && file.Content[0]=='P' && file.Content[1]=='K') {
            using var zip=new ZipArchive(new MemoryStream(file.Content),ZipArchiveMode.Read);
            if(zip.Entries.Sum(e=>e.Length)>100*1024*1024)throw new ImportException("El contenido del Excel supera 100 MB descomprimidos.");
        }
        var folder=Path.Combine(Path.GetTempPath(),"ArcaWebPyR",Guid.NewGuid().ToString("N"));Directory.CreateDirectory(folder);
        try {
            var path=Path.Combine(folder,Path.GetFileName(file.Name));File.WriteAllBytes(path,file.Content);
            var major=PreseaPyRImporter.Importar(path,profile,account);
            if(major.Registros.Count==0)throw new ImportException("El mayor no contiene movimientos reconocibles. Revisá la hoja y las columnas del perfil.");
            if(major.Registros.Count>TabularReader.MaxRows)throw new ImportException("El mayor supera 50.000 movimientos.");
            major.Ruta="";return major;
        } finally { Directory.Delete(folder,true); }
    }
    public static byte[] Export(ResultadoConciliacionPyR result,string profileName)
    {
        var folder=Path.Combine(Path.GetTempPath(),"ArcaWebPyR",Guid.NewGuid().ToString("N"));Directory.CreateDirectory(folder);
        try {var path=Path.Combine(folder,"Conciliacion.xlsx");ConciliacionPyRExcelExporter.Exportar(result,profileName,path);return File.ReadAllBytes(path);}
        finally {Directory.Delete(folder,true);}
    }
}
