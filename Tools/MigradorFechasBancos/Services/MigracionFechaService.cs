// Fecha: 13/10/2026 - TAREA: 00083 - Linea: 2 - La migración no inicializa el módulo ni reinterpreta fechas con reglas distintas.
using Microsoft.Data.SqlClient;
using MigradorFechasBancos.Models;
using MigradorFechasBancos.Data;
using Conciliador.Comun.Services;
using System.Data;
namespace MigradorFechasBancos.Services;
public static class MigracionFechaService
{
    public static DiagnosticoFechas Diagnosticar(SqlConnection cn) => Leer(cn, null).Diagnostico;
    sealed record Estado(DiagnosticoFechas Diagnostico, Dictionary<string,List<FilaFecha>> Filas, Dictionary<string,bool> Intermedias);
    static Estado Leer(SqlConnection cn, SqlTransaction tx)
    {
        var d=new DiagnosticoFechas(); var filas=new Dictionary<string,List<FilaFecha>>(); var intermedias=new Dictionary<string,bool>();
        foreach(string tabla in MigracionFechaStorage.Tablas)
        {
            void Error(string motivo,int? id=null,string texto=null,DateTime? intermedia=null) => d.Incidencias.Add(new(tabla,id,texto,intermedia,motivo));
            var cols=MigracionFechaStorage.Columnas(cn,tx,tabla);
            var fecha=cols.Find(c=>c.Nombre.Equals("Fecha",StringComparison.OrdinalIgnoreCase));
            var auxiliar=cols.Find(c=>c.Nombre.Equals("fecha2",StringComparison.OrdinalIgnoreCase));
            var original=cols.Find(c=>c.Nombre.Equals("FechaTextoOriginal",StringComparison.OrdinalIgnoreCase));
            bool final=fecha?.Tipo=="date" && !fecha.Nula;
            bool texto=fecha?.Tipo is "nvarchar" or "varchar" or "nchar" or "char";
            if((!final&&!texto)||!cols.Any(c=>c.Nombre=="Id" && c.Tipo=="int")) { Error("Esquema no reconocido: se requiere Id int y Fecha textual o date NOT NULL."); continue; }
            if(final && auxiliar!=null || texto && original!=null) { Error("Esquema parcial: columnas auxiliares o históricas inesperadas."); continue; }
            if(auxiliar!=null && auxiliar.Tipo is not ("date" or "smalldatetime")) { Error("Tipo no admitido para fecha2."); continue; }
            if(original!=null && (original.Tipo is not ("nvarchar" or "varchar" or "nchar" or "char") || !original.Nula)) { Error("FechaTextoOriginal debe ser texto nullable."); continue; }
            if(!final) foreach(string dep in MigracionFechaStorage.Dependencias(cn,tx,tabla)) Error(dep);
            var rows=MigracionFechaStorage.Filas(cn,tx,tabla,final,auxiliar!=null,original!=null);
            filas[tabla]=rows; intermedias[tabla]=auxiliar!=null;
            d.Tablas.Add(new(tabla,rows.Count,final?0:rows.Count(r=>!r.Intermedia.HasValue),final));
            foreach(var row in rows)
            {
                if(final && row.Texto==null) continue;
                if(!FechaBancariaParser.TryParse(row.Texto,out var parsed)) { Error("Fecha textual vacía o inválida.",row.Id,row.Texto,row.Intermedia); continue; }
                if(final && (row.Fecha is not DateTime tipada || tipada!=parsed)) Error("Fecha final difiere del texto histórico.",row.Id,row.Texto,row.Fecha as DateTime?);
                if(!final && row.Intermedia.HasValue && row.Intermedia.Value!=parsed) Error("fecha2 difiere del día original o contiene hora.",row.Id,row.Texto,row.Intermedia);
            }
        }
        if(d.Tablas.Select(t=>t.Migrada).Distinct().Count()>1) d.Incidencias.Add(new("bancos",null,null,null,"Una tabla está migrada y la otra no; resolver el estado parcial."));
        return new(d,filas,intermedias);
    }
    public static DiagnosticoFechas Aplicar(SqlConnection cn)
    {
        using var tx=cn.BeginTransaction(IsolationLevel.Serializable);
        MigracionFechaStorage.Bloquear(cn,tx);
        var antes=Leer(cn,tx);
        if(!antes.Diagnostico.PuedeAplicar) throw new InvalidOperationException("Migración cancelada: "+string.Join("; ",antes.Diagnostico.Incidencias.Select(i=>$"{i.Tabla} ID {i.Id}: {i.Motivo}")));
        if(antes.Diagnostico.YaMigrada) { tx.Commit(); return antes.Diagnostico; }
        string relaciones=MigracionFechaStorage.Relaciones(cn,tx);
        var huellas=MigracionFechaStorage.Tablas.ToDictionary(t=>t,t=>MigracionFechaStorage.Huella(cn,tx,t));
        foreach(string tabla in MigracionFechaStorage.Tablas)
        {
            var fechas=antes.Filas[tabla].ToDictionary(r=>r.Id,r=>{FechaBancariaParser.TryParse(r.Texto,out var fecha); return fecha;});
            MigracionFechaStorage.Convertir(cn,tx,tabla,antes.Intermedias[tabla],fechas);
        }
        var despues=Leer(cn,tx);
        if(!despues.Diagnostico.YaMigrada || relaciones!=MigracionFechaStorage.Relaciones(cn,tx)) throw new InvalidOperationException("Falló la verificación posterior de fechas o relaciones.");
        foreach(string tabla in MigracionFechaStorage.Tablas)
        {
            var nuevas=despues.Filas[tabla].ToDictionary(r=>r.Id);
            if(huellas[tabla]!=MigracionFechaStorage.Huella(cn,tx,tabla)) throw new InvalidOperationException("Cambiaron importes, conceptos u otras columnas de "+tabla);
            if(nuevas.Count!=antes.Filas[tabla].Count || antes.Filas[tabla].Any(r=>!nuevas.TryGetValue(r.Id,out var n)||n.Texto!=r.Texto)) throw new InvalidOperationException("Cambiaron IDs o textos originales.");
        }
        tx.Commit(); return despues.Diagnostico;
    }
}
