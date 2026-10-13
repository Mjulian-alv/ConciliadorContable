// Fecha: 13/10/2026 - TAREA: 00083 - Linea: 1 - Las columnas autogeneradas sólo existen después de enlazar; el Designer conecta este formato común.
using System;
using Telerik.WinControls.UI;
namespace AgrupadorConceptos.Services;
internal static class FechasPresentacion
{
    public static void AlEnlazar(object sender, EventArgs e)
    {
        if(sender is not RadGridView grid) return;
        foreach(var column in grid.Columns)
            if(column.Name is "Fecha" or "FechaA" or "FechaB" or "FechaExterno" or "FechaExtracto")
                column.FormatString="{0:dd/MM/yyyy}";
    }
}
