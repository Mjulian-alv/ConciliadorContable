// Fecha: 13/10/2026 - TAREA: 00083 - Linea: 1 - Mostrar todas las incidencias antes de guardar cualquier fila.
using AgrupadorConceptos.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
namespace AgrupadorConceptos.Services;
public sealed class FechasImportacionException : InvalidOperationException
{
    public IReadOnlyList<IncidenciaFechaImportacion> Incidencias { get; }
    public FechasImportacionException(IReadOnlyList<IncidenciaFechaImportacion> incidencias)
        : base("Se rechazó el archivo por fechas inválidas:"+Environment.NewLine+string.Join(Environment.NewLine,incidencias.Select(i=>$"{i.Archivo} - {i.Hoja} - fila {i.Fila}: '{i.ValorOriginal}' ({i.Motivo})"))) => Incidencias=incidencias.ToArray();
    internal static IncidenciaFechaImportacion Incidencia(string archivo,string hoja,int fila,object valor,string motivo="Fecha vacía o formato inválido; use día/mes/año o ISO.") => new(archivo,hoja,fila,Convert.ToString(valor,CultureInfo.InvariantCulture)??"",motivo);
}
