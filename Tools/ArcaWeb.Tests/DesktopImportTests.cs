// Fecha: 05/10/2026 - TAREA: 00065 - Linea: 1 - El perfil existente se conserva aunque un exportador entregue Latin-1 bajo configuración UTF-8.
using ArcaCliente.Web.Models;
using ArcaCliente.Web.Services;
using System.Text;
static class DesktopImportTests
{
    public static void Run(Action<string,Action> test)
    {
        void Check(bool ok,string message){if(!ok)throw new Exception(message);}
        var format=new FileFormat{Type="codigoafip",Point="ptovta",Name="nombre",DecimalSeparator="."};
        UploadedDocument File(string type)=>new("sistema.csv",Encoding.Latin1.GetBytes("fecha;importe;cuit;codigoafip;ptovta;numero;original;nombre\n01/09/2026;986999.90;30123456781;"+type+";        10;00278089;1000278089;Ejemplo Ñ"));
        test("Sistema conserva el perfil y lee Latin-1 recibido bajo configuración UTF-8",()=>{
            var p=SystemFileImporter.Preview(File("1"),format);
            Check(p.Ready,"El sistema rechazó un archivo válido con el perfil existente");
            Check(p.Included.Single().Total==986999.90m&&p.Included.Single().NombreProveedor=="Ejemplo Ñ","Alteró valores");
            Check(format.Encoding=="UTF-8","Cambió el perfil compartido");
            Check(!ProviderImporter.Preview(File("1"),format).Ready,"La tolerancia del sistema afectó al proveedor");
        });
        test("Código cero sólo se excluye con configuración explícita del perfil",()=>{
            var file=File("1");var text=Encoding.Latin1.GetString(file.Content)+"\n02/09/2026;10.00;30123456781;0;1;86;100000086;Ejemplo Ñ";
            file=new(file.Name,Encoding.Latin1.GetBytes(text));
            var pending=SystemFileImporter.Preview(file,format);
            Check(!pending.Ready&&pending.UnknownTypes.Contains("0")&&pending.ExcludedCount==0,"Excluyó cero sin autorización");
            var resolved=SystemFileImporter.Preview(file,format,new[]{"000"});
            Check(resolved.Ready&&resolved.Included.Count==1&&resolved.ExcludedCount==1,"No aplicó la exclusión confirmada");
            Check(resolved.Rows[1].RawAmount=="10.00","Perdió el valor excluido original");
        });
    }
}
