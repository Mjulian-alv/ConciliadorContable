// Fecha: 13/10/2026 - TAREA: 00083 - Linea: 2 - Punto de entrada independiente del shell.
using System.Xml.Linq;
using System.Text.Json;
using Microsoft.Data.SqlClient;
using MigradorFechasBancos.Services;
try
{
    if(args.Length<1 || args[0] is not ("diagnosticar" or "aplicar")) throw new ArgumentException("Uso: diagnosticar|aplicar --config secrets.config --salida informe.json [--respaldo referencia]");
    var opciones=new Dictionary<string,string>();
    for(int i=1;i<args.Length;i+=2) { if(i+1>=args.Length || args[i] is not ("--config" or "--salida" or "--respaldo")) throw new ArgumentException("Opciones incompletas o desconocidas."); opciones.Add(args[i],args[i+1]); }
    if(!opciones.TryGetValue("--config",out var config)||!opciones.TryGetValue("--salida",out var salida)) throw new ArgumentException("Falta --config o --salida.");
    opciones.TryGetValue("--respaldo",out var respaldo);
    if(args[0]=="aplicar" && string.IsNullOrWhiteSpace(respaldo)) throw new ArgumentException("Aplicar requiere --respaldo: identifique el respaldo verificado antes del corte.");
    string cadena=XDocument.Load(config).Descendants("add").Single(e=>(string)e.Attribute("key")=="ConciliadorSqlConnectionString").Attribute("value").Value;
    using var cn=new SqlConnection(cadena); cn.Open();
    var d=args[0]=="aplicar" ? MigracionFechaService.Aplicar(cn) : MigracionFechaService.Diagnosticar(cn);
    File.WriteAllText(salida,JsonSerializer.Serialize(new {Fecha=DateTimeOffset.Now,Servidor=cn.DataSource,Base=cn.Database,Operacion=args[0],Respaldo=respaldo,Diagnostico=d},new JsonSerializerOptions {WriteIndented=true}));
    Console.WriteLine($"{cn.DataSource}/{cn.Database}: {d.Tablas.Sum(t=>t.Cantidad)} filas, {d.Incidencias.Count} incidencias. Informe: {salida}");
    return d.PuedeAplicar?0:2;
}
catch(Exception ex) { Console.Error.WriteLine("Operación no completada: "+ex.Message); return 1; }
