// Fecha: 06/10/2026 - TAREA: 00072 - Linea: 1-4 - Verifica paridad e invalidez de resultados sin usar la base compartida.
using ArcaCliente.Models;
using ArcaCliente.Services;
using ArcaCliente.Web.Data;
using ArcaCliente.Web.Models;
using ArcaCliente.Web.Services;
using Conciliador.Web.Data;
using Conciliador.Web.Security;
using Conciliador.Web.Services;
using Conciliador.Web.Pages.Account;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Data.Sqlite;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
static class PyRWebTests
{
    static void Check(bool ok,string message="Falló la verificación PyR"){if(!ok)throw new Exception(message);}
    static T Wait<T>(Task<T> t)=>t.GetAwaiter().GetResult();
    static void Wait(Task t)=>t.GetAwaiter().GetResult();
    sealed class Auth : AuthenticationStateProvider {public ClaimsPrincipal Principal=new WebUser("demo","Demo","Admin",new()).Principal(true);public override Task<AuthenticationState> GetAuthenticationStateAsync()=>Task.FromResult(new AuthenticationState(Principal));}
    static PyRWorkspace Work(IPyRProfileStore store){var auth=new Auth();var registry=new SessionRegistry();registry.Register(auth.Principal);return new(store,new AccessGuard(auth,new UserStorage(""),new WebMode(true),registry));}
    sealed class CaptureAuth : IAuthenticationService {
        public bool SignedIn;
        public Task<AuthenticateResult> AuthenticateAsync(HttpContext c,string? s)=>Task.FromResult(AuthenticateResult.NoResult());
        public Task ChallengeAsync(HttpContext c,string? s,AuthenticationProperties? p)=>Task.CompletedTask;
        public Task ForbidAsync(HttpContext c,string? s,AuthenticationProperties? p)=>Task.CompletedTask;
        public Task SignInAsync(HttpContext c,string? s,ClaimsPrincipal user,AuthenticationProperties? p){SignedIn=true;return Task.CompletedTask;}
        public Task SignOutAsync(HttpContext c,string? s,AuthenticationProperties? p)=>Task.CompletedTask;
    }
    public static void Run(Action<string,Action> test)
    {
        test("PyR: mayor conserva operador, asiento opcional, conceptos y anulaciones",()=>{
            var store=new DemoPyRProfileStorage();var p=Wait(store.Profiles()).Single().Profile;var doc=PyRExamples.Major();
            var actual=PyRFileAdapter.ImportMajor(doc,p,p.Cuentas[0]);
            var path=Path.Combine(Path.GetTempPath(),Guid.NewGuid()+".xlsx");
            try {File.WriteAllBytes(path,doc.Content);var expected=PreseaPyRImporter.Importar(path,p,p.Cuentas[0]);
                Check(actual.Registros.Count==expected.Registros.Count&&actual.Filas==6&&actual.FilasSinComprobante==1);
                Check(actual.Registros.Select(r=>(r.Numero,r.Importe,r.Operador,r.EsAnulacion)).SequenceEqual(expected.Registros.Select(r=>(r.Numero,r.Importe,r.Operador,r.EsAnulacion))));
                Check(actual.Registros.All(r=>r.Operador=="Operador ejemplo"));Check(actual.Registros.Count(r=>r.EsAnulacion)==1);
                Check(actual.Ruta==""&&!Directory.EnumerateFiles(Path.GetTempPath(),"Mayor-ejemplo.xlsx").Any());
            }finally{File.Delete(path);}
        });
        test("PyR: motor conserva alcance, ceros, cinco estados y planilla WinForms",()=>{
            var store=new DemoPyRProfileStorage();var work=Work(store);Wait(work.Select(Wait(store.Profiles()).Single().Profile.Id));Wait(work.Example());Wait(work.Reconcile());
            var r=work.Result!;Check(r.Items.Count==7);Check(r.Items.Count(i=>i.Estado==EstadoConciliacionPyR.Conciliado)==1);Check(r.Items.Count(i=>i.Estado==EstadoConciliacionPyR.AnuladaPresea)==2);
            Check(r.ArcaImporteCero==1&&r.FueraDeAlcance.Values.Sum()==1);Check(Enum.GetValues<EstadoConciliacionPyR>().All(e=>r.Items.Any(i=>i.Estado==e)));
            var path=Path.Combine(Path.GetTempPath(),Guid.NewGuid()+".xlsx");try{
                ConciliacionPyRExcelExporter.Exportar(r,work.Profile!.Profile.Nombre,path);
                using var expected=new XLWorkbook(path);using var actual=new XLWorkbook(new MemoryStream(Wait(work.Export())));
                Check(actual.Worksheets.Count==2);
                foreach(var e in expected.Worksheets){var a=actual.Worksheet(e.Name);Check(a.LastRowUsed()!.RowNumber()==e.LastRowUsed()!.RowNumber());
                    foreach(var cell in e.CellsUsed()){var compared=a.Cell(cell.Address.RowNumber,cell.Address.ColumnNumber);Check(cell.Value.Equals(compared.Value)&&cell.Style.Equals(compared.Style),"Diferencia de valor o estilo en Excel PyR");}}
            }finally{File.Delete(path);}
        });
        // Fecha: 19/10/2026 - TAREA: 00112 - Linea: 1 - Rango de fechas de los mayores aplicado a ARCA
        test("PyR: ARCA se recorta al rango máximo de fechas de los mayores del grupo",()=>{
            var iva=new CuentaPyR{Codigo="1",Nombre="IVA",Tipo=TipoOperacionPyR.Percepcion,Impuesto=ImpuestoPyR.Iva};
            var iibb=new CuentaPyR{Codigo="2",Nombre="IIBB",Tipo=TipoOperacionPyR.Retencion,Impuesto=ImpuestoPyR.Iibb};
            RegistroPreseaPyR P(CuentaPyR c,string n,DateTime? f)=>new(){Cuenta=c,Numero=n,Importe=100m,Fecha=f};
            RegistroArcaPyR A(ImpuestoPyR i,TipoOperacionPyR t,string n,DateTime? f)=>new(){Impuesto=i,Operacion=t,Numero=n,Importe=100m,Fecha=f};
            var mayores=new List<MayorPreseaPyR>{
                new(){Cuenta=iva,Registros={P(iva,"1",new(2026,7,10)),P(iva,"2",new(2026,7,31)),P(iva,"3",new(2026,7,1))}},
                new(){Cuenta=iibb,Registros={P(iibb,"9",null)}}};   // sin ninguna fecha: no se recorta
            var arca=new[]{
                A(ImpuestoPyR.Iva,TipoOperacionPyR.Percepcion,"1",new(2026,7,10)),
                A(ImpuestoPyR.Iva,TipoOperacionPyR.Percepcion,"4",new(2026,7,1)),    // borde inferior: queda
                A(ImpuestoPyR.Iva,TipoOperacionPyR.Percepcion,"5",new(2026,7,31)),   // borde superior: queda
                A(ImpuestoPyR.Iva,TipoOperacionPyR.Percepcion,"6",new(2026,6,30)),   // antes del rango
                A(ImpuestoPyR.Iva,TipoOperacionPyR.Percepcion,"7",new(2026,8,1)),    // después del rango
                A(ImpuestoPyR.Iva,TipoOperacionPyR.Percepcion,"8",null),             // sin fecha: queda
                A(ImpuestoPyR.Iibb,TipoOperacionPyR.Retencion,"9",new(2020,1,1))};   // grupo sin rango: queda
            var r=ConciliacionPyRService.Conciliar(arca,mayores,null);
            Check(r.FueraDeRango["IVA · Percepción"]==2&&r.FueraDeRango.Count==1,"Debían quedar 2 registros fuera de rango");
            Check(r.RangoFechas["IVA · Percepción"]==(new DateTime(2026,7,1),new DateTime(2026,7,31)));
            Check(!r.Items.Any(i=>i.Arca?.Numero is "6" or "7"),"Un registro fuera de rango entró a la conciliación");
            Check(r.Items.Count(i=>i.Arca!=null)==5,"Debían quedar 5 registros de ARCA conciliando");
            Check(r.AvisosFueraDeRango().Single().Contains("01/07/2026 a 31/07/2026"));
        });
        test("PyR: resultado caduca por carga fallida, cambio de carpeta, directivas y concurrencia",()=>{
            var store=new DemoPyRProfileStorage();var work=Work(store);var id=Wait(store.Profiles()).Single().Profile.Id;Wait(work.Select(id));Wait(work.Example());Wait(work.Reconcile());
            var other=Work(store);Wait(other.Select(id));Check(other.Arca==null&&other.Majors.Count==0&&other.Result==null);
            var account=work.Majors[0].Cuenta.Id;
            try{Wait(work.LoadMajor(new("roto.txt",Encoding.UTF8.GetBytes("roto")),account));throw new Exception("Se aceptó TXT");}catch(ImportException){}
            Check(work.Result==null&&work.Majors.Count==0);Wait(work.Example());Wait(work.Reconcile());
            work.ChangeDirectory("no-existe");Check(work.Arca==null&&work.Result==null);
            Wait(work.Example());Wait(work.Reconcile());var directives=work.Profile!.Profile.DirectivasConciliacion.Select(d=>d.Clonar()).ToList();Wait(work.SaveDirectives(directives));Check(work.Result==null);
            Wait(work.Reconcile());var edited=Wait(store.Profiles()).Single();edited.Profile.Nombre="Modificado en otra sesión";Wait(store.Save(edited));
            try{Wait(work.Export());throw new Exception("Exportó resultado obsoleto");}catch(ConfigurationConflictException){}Check(work.Result==null);
        });
        test("PyR: perfiles se guardan por entidad y una versión obsoleta no sobrescribe",()=>{
            var store=new DemoPyRProfileStorage();var a=Wait(store.Profiles()).Single();var old=a.Copy();a.Profile.Nombre="Nuevo nombre";Wait(store.Save(a));
            try{Wait(store.Save(old));throw new Exception("Sobrescribió la versión nueva");}catch(ConfigurationConflictException){}
            var b=Wait(store.Save(new()));Check(Wait(store.Profiles()).Count==2);Wait(store.Delete(b));Check(Wait(store.Profiles()).Single().Profile.Nombre=="Nuevo nombre");
            old.Profile.DirectivasConciliacion[0].Campos.Clear();try{PyRProfileValidator.Validate(old);throw new Exception("Aceptó directiva vacía");}catch(ImportException){}
        });
        test("PyR: carpeta ARCA reutiliza detección de contenido y deduplicación",()=>{
            var folder=Path.Combine(Path.GetTempPath(),"pyr-import-"+Guid.NewGuid());Directory.CreateDirectory(folder);
            try {using(var book=new XLWorkbook()){var s=book.AddWorksheet("ARCA");var names=new[]{"CUIT Agente Ret./Perc.","Impuesto","Descripcion Operacion","Fecha Ret./Perc.","Numero Comprobante","Importe Ret./Perc."};
                for(int i=0;i<names.Length;i++)s.Cell(1,i+1).Value=names[i];s.Cell(2,1).Value="30123456781";s.Cell(2,2).Value="767";s.Cell(2,3).Value="Percepción";s.Cell(2,4).Value=new DateTime(2026,7,10);s.Cell(2,5).Value="000100";s.Cell(2,6).Value=100;book.SaveAs(Path.Combine(folder,"arca.xlsx"));}
                File.Copy(Path.Combine(folder,"arca.xlsx"),Path.Combine(folder,"duplicado.xls"));File.WriteAllText(Path.Combine(folder,"no-reconocido.xls"),"No es Excel");
                var store=new DemoPyRProfileStorage();var work=Work(store);Wait(work.Select(Wait(store.Profiles()).Single().Profile.Id));work.ChangeDirectory(folder);Wait(work.LoadFolder());
                Check(work.Arca!.Registros.Count==1&&work.Arca.ArchivosRepetidos.Count==1&&work.Arca.ArchivosNoReconocidos.Count==1);Check(work.Arca.Registros[0].Numero=="100");
                Check(Wait(store.Profiles()).Single().Profile.CarpetaArca==folder,"No conservó la carpeta leída");
            }finally{Directory.Delete(folder,true);}
        });
        test("PyR: un usuario exclusivo ingresa y el servidor rechaza acciones sin permiso",AuthTest);
    }
    static void AuthTest()
    {
        var path=Path.Combine(Path.GetTempPath(),"pyr-users-"+Guid.NewGuid()+".db");
        try {
            using(var c=new SqliteConnection("Data Source="+path)){c.Open();using var cmd=c.CreateCommand();cmd.CommandText="CREATE TABLE Usuarios(Username TEXT,Nombre TEXT,Rol TEXT,PermisosJson TEXT,PasswordHash TEXT,Activo INTEGER); INSERT INTO Usuarios VALUES('pyr','Usuario PyR','Usuario','[\"ArcaPyR\"]',@Hash,1)";cmd.Parameters.AddWithValue("@Hash",Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes("prueba"))).ToLowerInvariant());cmd.ExecuteNonQuery();}
            var capture=new CaptureAuth();using var services=new ServiceCollection().AddSingleton<IAuthenticationService>(capture).BuildServiceProvider();
            var context=new DefaultHttpContext{RequestServices=services};var users=new UserStorage(path);var sessions=new SessionRegistry();
            var model=new LoginModel(users,new WebMode(false),sessions,NullLogger<LoginModel>.Instance){Username="pyr",Password="prueba",PageContext=new PageContext{HttpContext=context}};
            var response=Wait(model.OnPostAsync());Check(capture.SignedIn&&response is LocalRedirectResult redirect&&redirect.Url=="~/"); // Fecha: 28/10/2026 - TAREA: 00125 - Linea: 4 - El login redirige a «~/» (relativo al PathBase de AppHub); la prueba esperaba «/». Fecha: 14/10/2026 - TAREA: 00097 - Linea: 1 - Todos aterrizan en el menú principal.
            var auth=new Auth{Principal=Wait(users.ValidateAsync("pyr","prueba"))!.Principal()};sessions.Register(auth.Principal);
            var access=new AccessGuard(auth,users,new WebMode(false),sessions);Wait(access.Require("ArcaPyR"));
            try{Wait(access.Require("ArcaOffline"));throw new Exception("Permitió comprobantes");}catch(UnauthorizedAccessException){}
            sessions.Revoke(auth.Principal);try{Wait(access.Require("ArcaPyR"));throw new Exception("Aceptó sesión revocada");}catch(UnauthorizedAccessException){}
        }finally{SqliteConnection.ClearAllPools();File.Delete(path);}
    }
}
