using System.Collections.Generic;
using System.Linq;
using AgrupadorConceptos.Models;
using Dapper;

namespace AgrupadorConceptos.Data
{
    /// <summary>
    /// Acceso a bancos.PerfilesBanco: el mapeo entre las columnas del Excel
    /// que emite cada banco y los campos que entiende el importador.
    /// </summary>
    internal static class PerfilBancoStorage
    {
        public static List<PerfilBanco> ObtenerTodos()
        {
            using var cn = DatabaseHelper.Open();
            return cn.Query<PerfilBanco>("SELECT * FROM bancos.PerfilesBanco").ToList();
        }

        public static PerfilBanco ObtenerPorId(int id)
        {
            using var cn = DatabaseHelper.Open();
            return cn.QueryFirstOrDefault<PerfilBanco>(
                "SELECT * FROM bancos.PerfilesBanco WHERE Id = @Id", new { Id = id });
        }

        public static void Insertar(PerfilBanco perfil)
        {
            using var cn = DatabaseHelper.Open();
            cn.Execute(@"
                INSERT INTO bancos.PerfilesBanco
                    (NombreBanco, ColumnaConcepto, ColumnaDescripcion, EsCodigo, FilaEncabezado,
                     TipoImporte, ColumnaImporteUnico, ColumnaDebe, ColumnaHaber, ColumnaFecha)
                VALUES
                    (@NombreBanco, @ColumnaConcepto, @ColumnaDescripcion, @EsCodigo, @FilaEncabezado,
                     @TipoImporte, @ColumnaImporteUnico, @ColumnaDebe, @ColumnaHaber, @ColumnaFecha)",
                perfil);
        }

        public static void Actualizar(PerfilBanco perfil)
        {
            using var cn = DatabaseHelper.Open();
            cn.Execute(@"
                UPDATE bancos.PerfilesBanco
                SET NombreBanco         = @NombreBanco,
                    ColumnaConcepto     = @ColumnaConcepto,
                    ColumnaDescripcion  = @ColumnaDescripcion,
                    EsCodigo            = @EsCodigo,
                    FilaEncabezado      = @FilaEncabezado,
                    TipoImporte         = @TipoImporte,
                    ColumnaImporteUnico = @ColumnaImporteUnico,
                    ColumnaDebe         = @ColumnaDebe,
                    ColumnaHaber        = @ColumnaHaber,
                    ColumnaFecha        = @ColumnaFecha
                WHERE Id = @Id",
                perfil);
        }
    }
}
