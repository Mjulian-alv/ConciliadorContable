// Fecha: 05/10/2026 - TAREA: 00064 - Linea: 3 - Comprueba persistencia, permisos, aislamiento y adaptación real al motor.
using ArcaCliente.Web.Data;
using ArcaCliente.Web.Models;
using ArcaCliente.Web.Services;
using Conciliador.Web.Services;
using Conciliador.Web.Data;
using Conciliador.Web.Security;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Data.Sqlite;
using Microsoft.Data.SqlClient;
using Dapper;
using ClosedXML.Excel;
using System.Security.Cryptography;
using System.Text;
static class IntegrationTests
{
    static void Check(bool condition){if(!condition)throw new Exception("Falló verificación de integración");}
    static T Wait<T>(Task<T> task)=>task.GetAwaiter().GetResult();
    sealed class Auth(WebUser user) : AuthenticationStateProvider {public System.Security.Claims.ClaimsPrincipal Principal{get;}=user.Principal(true);public override Task<AuthenticationState> GetAuthenticationStateAsync()=>Task.FromResult(new AuthenticationState(Principal));}
    public static void Run(Action<string,Action> test){
        test("Cerrar sesión revoca también circuitos que siguen abiertos",()=>{
            var registry=new SessionRegistry();var principal=new WebUser("u","U","Admin",new()).Principal(true);registry.Register(principal);Check(registry.IsActive(principal));registry.Revoke(principal);Check(!registry.IsActive(principal));
        });
        test("ARCA UTF8 con tildes se normaliza y conserva cuatro estados",()=>{
            var a=ReconciliationRunner.Arca(DemoFiles.Arca);var b=ProviderImporter.Preview(DemoFiles.System,new FileFormat());Check(a.Ready&&b.Ready);
            var r=ReconciliationRunner.Run(new(),a,b,null);Check(r.Items.Count==7&&r.Items.Count(x=>x.Estado==ArcaCliente.Models.EstadoConciliacion.Conciliado)==4);
            Check(r.Summary.Single().External==261500&&r.Summary.Single().System==276400);
            // Fecha: 05/10/2026 - TAREA: 00066 - Linea: 2 - El contrato es ahora la planilla WinForms, con encabezado en fila 2 y total ARCA en columna 12.
            using var book=new XLWorkbook(new MemoryStream(ResultExporter.Export(r)));Check(book.Worksheet(1).Cell(2,12).GetString()=="Total ARCA");
        });
        test("CUIT filtra ambos lados sin generar remanentes ajenos",()=>{
            var a=ReconciliationRunner.Arca(DemoFiles.Arca);var b=ProviderImporter.Preview(DemoFiles.System,new FileFormat());
            var r=ReconciliationRunner.Run(new(){FilterCuits=true,Cuits=new(){"30999999999"}},a,b,null);Check(r.Items.Count==0&&r.ExternalOmitted==6&&r.SystemOmitted==6);
        });
        test("Proveedor usa CUIT contextual y resumen fiscal",()=>{
            var p=new ProviderConfiguration{Cuit="30123456781",Format=new(){NumberMode=NumberMode.Separated},Types=new(){["FA"]="001",["NCA"]="003",["NDA"]="002",["REC"]="EXCLUIR",["PAGO"]="EXCLUIR"}};
            var a=ProviderImporter.Preview(DemoFiles.Provider,p);Check(a.Ready&&a.ExcludedCount==2);
            var r=ReconciliationRunner.Run(new(){Source=ReconciliationSource.Provider},a,ProviderImporter.Preview(DemoFiles.System,new FileFormat()),p.Cuit);Check(r.Items.Count==7&&r.Source=="Proveedor"&&r.Summary.Single().External==261500);
        });
        test("Versiones obsoletas no sobrescriben perfiles ni formatos",()=>{
            var store=new DemoConfigurationStorage();var p=Wait(store.Profiles())[0];var stale=Wait(store.Profiles())[0];p.Profile.Nombre="Cambio válido";Wait(store.Save(p));
            try{Wait(store.Save(stale));throw new Exception("Aceptó versión obsoleta");}catch(ConfigurationConflictException){}
            Check(Wait(store.Profiles()).Any(x=>x.Profile.Nombre=="Cambio válido"));
            var a=Wait(store.Provider("30123456781"));var b=Wait(store.Provider("30123456781"));Wait(store.Save(a));try{Wait(store.Save(b));throw new Exception("Aceptó formato obsoleto");}catch(ConfigurationConflictException){}
        });
        test("Sesiones aisladas e invalidación de nueva carga",()=>{
            var store=new DemoConfigurationStorage();var auth=new Auth(new("test","Test","Admin",new()));var registry=new SessionRegistry();registry.Register(auth.Principal);var access=new AccessGuard(auth,new UserStorage(""),new WebMode(true),registry);
            var a=new Workspace(store,access);var b=new Workspace(store,access);var id=Wait(store.Profiles()).First(x=>x.Source==ReconciliationSource.Arca).Profile.Id;
            a.Select(id).GetAwaiter().GetResult();a.UploadExternal(new(){DemoFiles.Arca}).GetAwaiter().GetResult();a.UploadSystem(DemoFiles.System).GetAwaiter().GetResult();a.From=new DateTime(2026,9,1);a.To=new DateTime(2026,9,30);a.Reconcile().GetAwaiter().GetResult();Check(a.Result!=null&&b.Result==null&&b.SystemFile==null);
            a.BeginExternalUpload();Check(a.Result==null&&a.External==null&&a.ExternalFiles.Count==0);
        });
        test("SQLite respeta contraseña, inactividad y permisos",()=>{
            var path=Path.Combine(Path.GetTempPath(),"arca-web-auth-"+Guid.NewGuid()+".db");
            using(var c=new SqliteConnection("Data Source="+path)){c.Open();using var cmd=c.CreateCommand();cmd.CommandText="CREATE TABLE Usuarios(Username TEXT,Nombre TEXT,Rol TEXT,PermisosJson TEXT,PasswordHash TEXT,Activo INTEGER); INSERT INTO Usuarios VALUES('test','Prueba','Usuario','[\"ArcaOffline\"]',@Hash,1)";cmd.Parameters.AddWithValue("@Hash",Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes("clave-prueba"))).ToLowerInvariant());cmd.ExecuteNonQuery();}
            var users=new UserStorage(path);Check(Wait(users.ValidateAsync("test","incorrecta"))==null);var u=Wait(users.ValidateAsync("test","clave-prueba"));Check(u!=null&&u.Allows("ArcaOffline")&&!u.Allows("ArcaPerfiles"));
            using(var c=new SqliteConnection("Data Source="+path)){c.Open();using var cmd=c.CreateCommand();cmd.CommandText="UPDATE Usuarios SET Activo=0";cmd.ExecuteNonQuery();}Check(Wait(users.ValidateAsync("test","clave-prueba"))==null);SqliteConnection.ClearAllPools();File.Delete(path);
        });
        if(Environment.GetEnvironmentVariable("ARCA_WEB_TEST_SQL")=="1")test("SQL Server local: esquema, lectura, versiones y preservación",SqlTest);
    }
    static void SqlTest(){
        const string master="Server=(localdb)\\ArcaWeb00064;Database=master;Integrated Security=true;TrustServerCertificate=true";
        string name="ArcaWebTest_"+Guid.NewGuid().ToString("N");
        // El identificador se genera internamente; ninguna entrada ni credencial de usuario participa en el DDL de la base temporal.
        using var admin=new SqlConnection(master);admin.Open();admin.Execute("DECLARE @sql nvarchar(max)='CREATE DATABASE '+QUOTENAME(@Name);EXEC(@sql)",new{Name=name});
        try {
            var store=new SqlConfigurationStorage(master.Replace("Database=master","Database="+name));store.Initialize().GetAwaiter().GetResult();store.Initialize().GetAwaiter().GetResult();
            var p=Wait(store.Save(new WebProfile()));p.FilterCuits=true;p.Cuits=new(){"30123456781"};p.Profile.Nombre="Prueba SQL";p=Wait(store.Save(p));
            var stale=Wait(store.Profiles()).Single();Check(stale.Profile.Nombre=="Prueba SQL"&&stale.Cuits.Count==1);p.Profile.Nombre="Actualizado";Wait(store.Save(p));
            try{Wait(store.Save(stale));throw new Exception("No detectó conflicto SQL");}catch(ConfigurationConflictException){}
            var provider=Wait(store.Provider("30123456781"));provider.Types["FA"]="001";Wait(store.Save(provider));Check(Wait(store.Provider("30123456781")).Types["FA"]=="001");
        }finally{SqlConnection.ClearAllPools();admin.Execute("DECLARE @sql nvarchar(max)='ALTER DATABASE '+QUOTENAME(@Name)+' SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE '+QUOTENAME(@Name);EXEC(@sql)",new{Name=name});}
    }
}
