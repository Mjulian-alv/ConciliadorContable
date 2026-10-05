// Fecha: 05/10/2026 - TAREA: 00064 - Linea: 3 - Regresiones de la revisión independiente y compatibilidad con fechas actuales.
using ArcaCliente.Web.Data;
using ArcaCliente.Web.Models;
using ArcaCliente.Web.Services;
using Conciliador.Web.Services;
using Conciliador.Web.Data;
using Conciliador.Web.Security;
using Microsoft.AspNetCore.Components.Authorization;
using System.Text;
static class ReviewRegressionTests
{
    static void Check(bool condition){if(!condition)throw new Exception("Falló regresión de revisión");}
    sealed class Auth : AuthenticationStateProvider { public System.Security.Claims.ClaimsPrincipal Principal=new WebUser("demo","Demo","Admin",new()).Principal(true);public override Task<AuthenticationState> GetAuthenticationStateAsync()=>Task.FromResult(new AuthenticationState(Principal)); }
    static Workspace Workspace(IWebConfigurationStore store){var auth=new Auth();var registry=new SessionRegistry();registry.Register(auth.Principal);return new(store,new AccessGuard(auth,new UserStorage(""),new WebMode(true),registry));}
    sealed class FailingStore : IWebConfigurationStore {
        public DemoConfigurationStorage Inner=new();public bool Fail;
        public Task<List<WebProfile>> Profiles()=>Inner.Profiles();public Task<ProviderConfiguration> Provider(string c)=>Inner.Provider(c);
        public Task<WebProfile> Save(WebProfile p)=>Fail?throw new IOException("Error simulado"):Inner.Save(p);
        public Task<ProviderConfiguration> Save(ProviderConfiguration p)=>Fail?throw new IOException("Error simulado"):Inner.Save(p);
    }
    public static void Run(Action<string,Action> test){
        test("Tiques FC, NC y ND conservan familia fiscal y signo",()=>{
            foreach(var code in new[]{"081","082","111","112","113","114","115","116","117"}) {
                var file=new UploadedDocument("t.csv",Encoding.UTF8.GetBytes("Tipo;Fecha;PuntoVenta;Numero;Importe;CUIT\n"+code+";05/09/2026;5;1234;100,00;30123456781"));
                var p=ProviderImporter.Preview(file,new FileFormat());Check(p.Ready&&p.ExcludedCount==0&&p.Included.Count==1);
                Check(FiscalCatalog.Sign(code)==(new[]{"112","113","114"}.Contains(code)?-1:1));
            }
        });
        test("FC ambiguo no sugiere una letra fiscal",()=>{Check(FiscalCatalog.Suggest("FC")=="");Check(FiscalCatalog.Suggest("FC C")=="011");});
        test("Equivalencias se corrigen aunque cambien mayúsculas",()=>{
            var store=new DemoConfigurationStorage();store.Save(new ProviderConfiguration{Cuit="30123456781",Types=new(){["FA"]="001"}}).GetAwaiter().GetResult();
            var work=Workspace(store);work.Select(store.Profiles().Result.First(x=>x.Source==ReconciliationSource.Provider).Profile.Id,"30123456781").GetAwaiter().GetResult();
            work.MapType("fa","006").GetAwaiter().GetResult();var saved=store.Provider("30123456781").Result;Check(saved.Types.Count==1&&saved.Types.Values.Single()=="006");
        });
        test("ARCA admite fecha ISO actual además de fecha histórica",()=>{
            var original=Encoding.UTF8.GetString(DemoFiles.Arca.Content);
            var iso=original.Replace("05/09/2026","2026-09-05");
            Check(ReconciliationRunner.Arca(new("arca.csv",Encoding.UTF8.GetBytes(iso))).Ready);
        });
        test("Período inicial conserva primer día del mes a hoy",()=>{
            var store=new DemoConfigurationStorage();var work=Workspace(store);var id=store.Profiles().Result.First(x=>x.Source==ReconciliationSource.Arca).Profile.Id;work.Select(id).GetAwaiter().GetResult();
            var today=DateTime.Today;Check(work.From==new DateTime(today.Year,today.Month,1)&&work.To==today);
        });
        test("Un formato no guardado no vuelve a habilitarse al subir otro archivo",()=>{
            var store=new FailingStore();var config=new ProviderConfiguration{Cuit="30123456781",Format=new(){NumberMode=NumberMode.Separated},Types=new(){["FA"]="001",["NCA"]="003",["NDA"]="002",["REC"]="EXCLUIR",["PAGO"]="EXCLUIR"}};
            store.Save(config).GetAwaiter().GetResult();var work=Workspace(store);work.Select(store.Profiles().Result.First(x=>x.Source==ReconciliationSource.Provider).Profile.Id,config.Cuit).GetAwaiter().GetResult();
            work.UploadExternal(new(){DemoFiles.Provider}).GetAwaiter().GetResult();work.UploadSystem(DemoFiles.System).GetAwaiter().GetResult();Check(work.External!.Ready);
            work.Provider!.Format.DecimalSeparator=".";store.Fail=true;try{work.SaveProviderFormat().GetAwaiter().GetResult();throw new Exception("No falló guardado");}catch(IOException){}
            var changed=new UploadedDocument("nuevo.csv",Encoding.UTF8.GetBytes(Encoding.UTF8.GetString(DemoFiles.Provider.Content).Replace(",00",".00")));
            work.UploadExternal(new(){changed}).GetAwaiter().GetResult();Check(work.External?.Ready!=true);
            store.Fail=false;work.SaveProviderFormat().GetAwaiter().GetResult();Check(work.External!.Ready);
        });
    }
}
