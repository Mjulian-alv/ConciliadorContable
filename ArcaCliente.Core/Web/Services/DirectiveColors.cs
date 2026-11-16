// Fecha: 16/11/2026 - TAREA: 00192 - Linea: 3 - Un color de directiva llega desde el navegador y termina en un atributo style y en el Excel: sólo se acepta «#RRGGBB».
using ArcaCliente.Models;
using System.Text.RegularExpressions;
namespace ArcaCliente.Web.Services;
public static class DirectiveColors
{
    static readonly Regex Hex=new("^#[0-9A-Fa-f]{6}$",RegexOptions.Compiled);
    public static string? Valid(string? color)=>color!=null&&Hex.IsMatch(color)?color.ToUpperInvariant():null;
    // DirectivaAplicada cuenta desde 1 y 0 es «sin coincidencia».
    public static string? For(int applied,IReadOnlyList<DirectivaConciliacion>? directives)=>applied>=1&&directives!=null&&applied<=directives.Count?Valid(directives[applied-1].Color):null;
}
