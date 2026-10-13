// Fecha: 13/10/2026 - TAREA: 00083 - Linea: 1 - Cabecera y movimientos se confirman juntos; los IDs sólo se publican después del commit.
using System;
using System.Collections.Generic;
using AgrupadorConceptos.Models;
namespace AgrupadorConceptos.Data;
internal static class ImportacionStorage
{
    public static int GuardarArchivo(int idPerfil,string nombre,DateTime fechaImportacion,List<MovimientoProcesado> movimientos,Action<int,int> progreso=null)
    {
        using var cn=DatabaseHelper.Open(); using var tx=cn.BeginTransaction();
        ConciliacionMutationStorage.Bloquear(cn,tx);
        int archivo=ArchivoImportadoStorage.Insertar(idPerfil,nombre,fechaImportacion,cn,tx);
        var ids=MovimientoStorage.InsertarLote(movimientos,cn,tx,archivo,progreso);
        tx.Commit();
        for(int i=0;i<movimientos.Count;i++) { movimientos[i].IdArchivo=archivo; movimientos[i].Id=ids[i]; }
        return archivo;
    }
}
