// Fecha: 05/10/2026 - TAREA: 00064 - Linea: 1 - La web exige conexión explícita y no usa los valores productivos por defecto.
using Conciliador.Web.Components;
using Conciliador.Web.Data;
using Conciliador.Web.Security;
using Conciliador.Web.Services;
using ArcaCliente.Web.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Threading.RateLimiting;
using System.Globalization;
var builder=WebApplication.CreateBuilder(args);
CultureInfo.DefaultThreadCurrentCulture=CultureInfo.GetCultureInfo("es-AR");CultureInfo.DefaultThreadCurrentUICulture=CultureInfo.GetCultureInfo("es-AR");
bool demo=builder.Configuration.GetValue<bool>("Web:Demo");
if(demo&&!builder.Environment.IsDevelopment())throw new InvalidOperationException("La demostración requiere Development y acceso local.");
builder.Services.AddSingleton(new WebMode(demo));builder.Services.AddSingleton<SessionRegistry>();
// Fecha: 06/10/2026 - TAREA: 00067 - Linea: 1 - Permite resolver la carpeta compartida cuando un perfil aún no tiene ubicación ARCA.
builder.Services.AddSingleton(new ArcaFolderOptions(builder.Configuration["Web:ArcaDirectory"]??""));
builder.Services.AddSingleton(new UserStorage(builder.Configuration["Web:UsersDatabasePath"]??""));
// Fecha: 06/10/2026 - TAREA: 00072 - Linea: 1,2 - PyR comparte identidad y conexión, con persistencia independiente de los perfiles fiscales.
if(demo){builder.Services.AddSingleton<IWebConfigurationStore,DemoConfigurationStorage>();builder.Services.AddSingleton<IPyRProfileStore,DemoPyRProfileStorage>();}
else {
    string connection=builder.Configuration.GetConnectionString("Arca")??throw new InvalidOperationException("Configurá ConnectionStrings:Arca o activá Web:Demo en Development.");
    if(string.IsNullOrWhiteSpace(builder.Configuration["Web:UsersDatabasePath"]))throw new InvalidOperationException("Configurá Web:UsersDatabasePath.");
    // Fecha: 14/10/2026 - TAREA: 00097 - Linea: 1 - El Agrupador usa SqlDb: se fija una vez al arrancar, con la misma conexión explícita de ARCA.
    Conciliador.Comun.SqlDb.ConnectionString=connection;
    builder.Services.AddSingleton<IWebConfigurationStore>(new SqlConfigurationStorage(connection));
    builder.Services.AddSingleton<IPyRProfileStore>(new SqlPyRProfileStorage(connection));
}
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(o=>{
    o.LoginPath="/account/login";o.AccessDeniedPath="/account/login";o.Cookie.Name="Conciliador.Web.Auth";o.Cookie.HttpOnly=true;o.Cookie.SameSite=SameSiteMode.Strict;o.Cookie.SecurePolicy=demo?CookieSecurePolicy.SameAsRequest:CookieSecurePolicy.Always;o.ExpireTimeSpan=TimeSpan.FromHours(8);o.SlidingExpiration=false;
    o.Events.OnValidatePrincipal=async context=>{if(!context.HttpContext.RequestServices.GetRequiredService<SessionRegistry>().IsActive(context.Principal!) || !demo && await context.HttpContext.RequestServices.GetRequiredService<UserStorage>().Find(context.Principal?.Identity?.Name??"")==null)context.RejectPrincipal();};
});
// Fecha: 14/10/2026 - TAREA: 00097 - Linea: 1 - Las políticas de ARCA y del Agrupador salen de un único catálogo de permisos.
builder.Services.AddAuthorization(Permisos.Registrar);
builder.Services.AddCascadingAuthenticationState();builder.Services.AddScoped<AccessGuard>();builder.Services.AddScoped<Workspace>();
builder.Services.AddScoped<PyRWorkspace>();
builder.Services.AddRazorComponents().AddInteractiveServerComponents(o=>{o.DisconnectedCircuitRetentionPeriod=TimeSpan.FromMinutes(2);});
builder.Services.AddRazorPages();
builder.Services.AddRateLimiter(o=>{o.RejectionStatusCode=429;o.AddPolicy("login",context=>RateLimitPartition.GetFixedWindowLimiter(context.Connection.RemoteIpAddress?.ToString()??"unknown",_=>new FixedWindowRateLimiterOptions{PermitLimit=10,Window=TimeSpan.FromMinutes(1),QueueLimit=0}));});
var app=builder.Build();
if(!demo){app.UseExceptionHandler("/Error");app.UseHsts();app.UseHttpsRedirection();await ((SqlConfigurationStorage)app.Services.GetRequiredService<IWebConfigurationStore>()).Initialize();await ((SqlPyRProfileStorage)app.Services.GetRequiredService<IPyRProfileStore>()).Initialize();
    // Fecha: 14/10/2026 - TAREA: 00097 - Linea: 1 - Si la base no está migrada a fechas tipadas, el arranque se detiene con el mensaje que pide migrar.
    AgrupadorConceptos.Data.DatabaseHelper.InitializeDatabase();}
if(demo)app.Use(async(ctx,next)=>{if(ctx.Connection.RemoteIpAddress==null||!System.Net.IPAddress.IsLoopback(ctx.Connection.RemoteIpAddress)){ctx.Response.StatusCode=403;return;}await next();});
app.Use(async(ctx,next)=>{ctx.Response.Headers["X-Content-Type-Options"]="nosniff";ctx.Response.Headers["X-Frame-Options"]="DENY";await next();});
app.UseAuthentication();app.UseAuthorization();app.UseRateLimiter();app.UseAntiforgery();
app.MapStaticAssets();app.MapRazorPages().RequireRateLimiting("login");app.MapRazorComponents<App>().AddInteractiveServerRenderMode();
app.Run();
