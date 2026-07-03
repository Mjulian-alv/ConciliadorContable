using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using LiquidacionesAuditar.Models;

namespace LiquidacionesAuditar.Data
{
    public static class Repositorio
    {
        // ── MarcasTarjeta ─────────────────────────────────────────────────

        public static List<MarcaTarjeta> GetMarcas()
        {
            var list = new List<MarcaTarjeta>();
            using var cn = DatabaseHelper.GetConnection(); cn.Open();
            using var cmd = cn.CreateCommand();
            cmd.CommandText = "SELECT id, nombre, COALESCE(FilaEncabezado,1), COALESCE(ColumnaLiquidacion,''), COALESCE(SeparadorDecimales,''), COALESCE(SeparadorMiles,''), COALESCE(TipoDato,''), COALESCE(ColumnaFiltro,'') FROM MarcasTarjeta ORDER BY nombre";
            using var r = cmd.ExecuteReader();
            while (r.Read())
                list.Add(new MarcaTarjeta
                {
                    Id = r.GetString(0),
                    Nombre = r.GetString(1),
                    FilaEncabezado = r.GetInt32(2),
                    ColumnaLiquidacion = r.GetString(3),
                    SeparadorDecimales = r.GetString(4),
                    SeparadorMiles = r.GetString(5),
                    TipoDato = r.GetString(6),
                    ColumnaFiltro = r.GetString(7)
                });
            return list;
        }

        public static void UpsertMarca(MarcaTarjeta m)
        {
            using var cn = DatabaseHelper.GetConnection(); cn.Open();
            using var cmd = cn.CreateCommand();
            cmd.CommandText = "INSERT OR REPLACE INTO MarcasTarjeta(id,nombre,FilaEncabezado,ColumnaLiquidacion,SeparadorDecimales,SeparadorMiles,TipoDato,ColumnaFiltro) VALUES(@id,@n,@fe,@cl,@sd,@sm,@td,@cf)";
            cmd.Parameters.AddWithValue("@id", m.Id);
            cmd.Parameters.AddWithValue("@n", m.Nombre);
            cmd.Parameters.AddWithValue("@fe", m.FilaEncabezado < 1 ? 1 : m.FilaEncabezado);
            cmd.Parameters.AddWithValue("@cl", (object?)m.ColumnaLiquidacion ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@sd", m.SeparadorDecimales);
            cmd.Parameters.AddWithValue("@sm", m.SeparadorMiles);
            cmd.Parameters.AddWithValue("@td", m.TipoDato ?? "");
            cmd.Parameters.AddWithValue("@cf", m.ColumnaFiltro ?? "");
            cmd.ExecuteNonQuery();
        }

        public static void DeleteMarca(string id)
        {
            using var cn = DatabaseHelper.GetConnection(); cn.Open();
            using var cmd = cn.CreateCommand();
            cmd.CommandText = "DELETE FROM MarcasTarjeta WHERE id=@id";
            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();
        }

        // ── ColumnasCSV ───────────────────────────────────────────────────

        public static List<ColumnaCSV> GetColumnasCSV(string idMarca)
        {
            var list = new List<ColumnaCSV>();
            using var cn = DatabaseHelper.GetConnection(); cn.Open();
            using var cmd = cn.CreateCommand();
            cmd.CommandText = "SELECT id, idMarcaTarjeta, idColumnaArchivo FROM Auditar_ColumnasCSV WHERE idMarcaTarjeta=@m ORDER BY id";
            cmd.Parameters.AddWithValue("@m", idMarca);
            using var r = cmd.ExecuteReader();
            while (r.Read())
                list.Add(new ColumnaCSV { Id = r.GetInt32(0), IdMarcaTarjeta = r.GetString(1), IdColumnaArchivo = r.GetString(2) });
            return list;
        }

        public static void InsertColumnaCSV(ColumnaCSV c)
        {
            using var cn = DatabaseHelper.GetConnection(); cn.Open();
            using var cmd = cn.CreateCommand();
            cmd.CommandText = "INSERT INTO Auditar_ColumnasCSV(idMarcaTarjeta,idColumnaArchivo) VALUES(@m,@col)";
            cmd.Parameters.AddWithValue("@m", c.IdMarcaTarjeta);
            cmd.Parameters.AddWithValue("@col", c.IdColumnaArchivo);
            cmd.ExecuteNonQuery();
        }

        public static void UpdateColumnaCSV(ColumnaCSV c)
        {
            using var cn = DatabaseHelper.GetConnection(); cn.Open();
            using var cmd = cn.CreateCommand();
            cmd.CommandText = "UPDATE Auditar_ColumnasCSV SET idColumnaArchivo=@col WHERE id=@id";
            cmd.Parameters.AddWithValue("@col", c.IdColumnaArchivo);
            cmd.Parameters.AddWithValue("@id", c.Id);
            cmd.ExecuteNonQuery();
        }

        public static void DeleteColumnaCSV(int id)
        {
            using var cn = DatabaseHelper.GetConnection(); cn.Open();
            using var cmd = cn.CreateCommand();
            cmd.CommandText = "DELETE FROM Auditar_ColumnasCSV WHERE id=@id";
            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();
        }

        // ── LiqCols ───────────────────────────────────────────────────────

        public static List<LiqCol> GetLiqCols(string idMarca = null, string tipoRegistro = null)
        {
            var list = new List<LiqCol>();
            using var cn = DatabaseHelper.GetConnection(); cn.Open();
            using var cmd = cn.CreateCommand();

            var where = new System.Collections.Generic.List<string>();
            if (idMarca != null)      where.Add("idMarcaTarjeta=@m");
            if (tipoRegistro != null) where.Add("TipoRegistro=@tr");
            var whereClause = where.Count > 0 ? "WHERE " + string.Join(" AND ", where) : "";

            cmd.CommandText = $"SELECT id,idMarcaTarjeta,idColumna,TipoRegistro,TipoDato,Orden,ValorFijo,Condicion,CondicionSigno,tieneSigno,EsFiltro FROM Auditar_LiqCols {whereClause} ORDER BY TipoRegistro,Orden";
            if (idMarca != null)      cmd.Parameters.AddWithValue("@m",  idMarca);
            if (tipoRegistro != null) cmd.Parameters.AddWithValue("@tr", tipoRegistro);

            using var r = cmd.ExecuteReader();
            while (r.Read())
                list.Add(new LiqCol
                {
                    Id             = r.GetInt32(0),
                    IdMarcaTarjeta = r.IsDBNull(1) ? "" : r.GetString(1),
                    IdColumna      = r.GetString(2),
                    TipoRegistro   = r.IsDBNull(3) ? "" : r.GetString(3),
                    TipoDato       = r.IsDBNull(4) ? "" : r.GetString(4),
                    Orden          = r.GetInt32(5),
                    ValorFijo      = r.IsDBNull(6) ? "" : r.GetString(6),
                    Condicion      = r.IsDBNull(7) ? "" : r.GetString(7),
                    CondicionSigno = r.IsDBNull(8) ? "" : r.GetString(8),
                    tieneSigno = r.IsDBNull(9) ? false : r.GetBoolean(9),
                    esFiltro = r.IsDBNull(10) ? false : r.GetBoolean(10)
                });
            return list;
        }

        public static void InsertLiqCol(LiqCol c)
        {
            using var cn = DatabaseHelper.GetConnection(); cn.Open();
            using var cmd = cn.CreateCommand();
            cmd.CommandText = @"INSERT INTO Auditar_LiqCols(idMarcaTarjeta,idColumna,TipoRegistro,TipoDato,Orden,ValorFijo,Condicion,CondicionSigno,tieneSigno)
                                VALUES(@m,@col,@tr,@td,@ord,@vf,@cond,@condSigno,@tieneSigno)";
            cmd.Parameters.AddWithValue("@m",    c.IdMarcaTarjeta);
            cmd.Parameters.AddWithValue("@col",  c.IdColumna);
            cmd.Parameters.AddWithValue("@tr",   c.TipoRegistro);
            cmd.Parameters.AddWithValue("@td",   c.TipoDato);
            cmd.Parameters.AddWithValue("@ord",  c.Orden);
            cmd.Parameters.AddWithValue("@vf",   c.ValorFijo);
            cmd.Parameters.AddWithValue("@cond", c.Condicion);
            cmd.Parameters.AddWithValue("@condSigno", c.CondicionSigno);
            cmd.Parameters.AddWithValue("@tieneSigno", c.tieneSigno);
            cmd.Parameters.AddWithValue("@esFiltro", c.esFiltro);
            cmd.ExecuteNonQuery();
        }

        public static void UpdateLiqCol(LiqCol c)
        {
            using var cn = DatabaseHelper.GetConnection(); cn.Open();
            using var cmd = cn.CreateCommand();
            cmd.CommandText = @"UPDATE Auditar_LiqCols SET idMarcaTarjeta=@m,idColumna=@col,TipoRegistro=@tr,TipoDato=@td,
                                Orden=@ord,ValorFijo=@vf,Condicion=@cond,CondicionSigno=@condSigno,tieneSigno=@tieneSigno WHERE id=@id";
            cmd.Parameters.AddWithValue("@m",    c.IdMarcaTarjeta);
            cmd.Parameters.AddWithValue("@col",  c.IdColumna);
            cmd.Parameters.AddWithValue("@tr",   c.TipoRegistro);
            cmd.Parameters.AddWithValue("@td",   c.TipoDato);
            cmd.Parameters.AddWithValue("@ord",  c.Orden);
            cmd.Parameters.AddWithValue("@vf",   c.ValorFijo);
            cmd.Parameters.AddWithValue("@cond", c.Condicion);
            cmd.Parameters.AddWithValue("@condSigno", c.CondicionSigno);
            cmd.Parameters.AddWithValue("@tieneSigno", c.tieneSigno);
            cmd.Parameters.AddWithValue("@id",   c.Id);
            cmd.Parameters.AddWithValue("@esFiltro", c.esFiltro);
            cmd.ExecuteNonQuery();
        }

        public static void DeleteLiqCol(int id)
        {
            using var cn = DatabaseHelper.GetConnection(); cn.Open();
            cn.ExecuteNonQuery($"DELETE FROM Auditar_RelacionCols WHERE IdLiqCols='{id}'");
            cn.ExecuteNonQuery($"DELETE FROM Auditar_LiqCols WHERE id={id}");
        }

        /// <summary>
        /// Clona todas las columnas destino de <paramref name="idOrigen"/> a <paramref name="idDestino"/>.
        /// Las relaciones CSV no se copian porque los idColumna del CSV pueden diferir entre marcas.
        /// Si ya existen columnas en la marca destino se eliminan primero.
        /// </summary>
        public static void ClonarLiqCols(string idOrigen, string idDestino)
        {
            var cols = GetLiqCols(idOrigen);
            using var cn = DatabaseHelper.GetConnection(); cn.Open();
            using var tx = cn.BeginTransaction();

            // Borrar columnas existentes de la marca destino (y sus relaciones)
            using (var del = cn.CreateCommand())
            {
                del.Transaction = tx;
                del.CommandText = @"DELETE FROM Auditar_RelacionCols WHERE IdLiqCols IN
                                    (SELECT id FROM Auditar_LiqCols WHERE idMarcaTarjeta=@d)";
                del.Parameters.AddWithValue("@d", idDestino);
                del.ExecuteNonQuery();
            }
            using (var del2 = cn.CreateCommand())
            {
                del2.Transaction = tx;
                del2.CommandText = "DELETE FROM Auditar_LiqCols WHERE idMarcaTarjeta=@d";
                del2.Parameters.AddWithValue("@d", idDestino);
                del2.ExecuteNonQuery();
            }

            // Insertar copias con la marca destino
            foreach (var c in cols)
            {
                using var ins = cn.CreateCommand();
                ins.Transaction = tx;
                ins.CommandText = @"INSERT INTO Auditar_LiqCols(idMarcaTarjeta,idColumna,TipoRegistro,TipoDato,Orden,ValorFijo,Condicion,CondicionSigno,tieneSigno)
                                    VALUES(@m,@col,@tr,@td,@ord,@vf,@cond,@condSigno,@tieneSigno)";
                ins.Parameters.AddWithValue("@m",    idDestino);
                ins.Parameters.AddWithValue("@col",  c.IdColumna);
                ins.Parameters.AddWithValue("@tr",   c.TipoRegistro);
                ins.Parameters.AddWithValue("@td",   c.TipoDato);
                ins.Parameters.AddWithValue("@ord",  c.Orden);
                ins.Parameters.AddWithValue("@vf",   c.ValorFijo ?? "");
                ins.Parameters.AddWithValue("@cond", c.Condicion ?? "");
                ins.Parameters.AddWithValue("@condSigno", c.CondicionSigno ?? "");
                ins.Parameters.AddWithValue("@tieneSigno", c.tieneSigno);
                ins.Parameters.AddWithValue("@esFiltro", c.esFiltro);
                ins.ExecuteNonQuery();
            }

            tx.Commit();
        }

        // ── RelacionCols ──────────────────────────────────────────────────

        public static List<string> GetRelaciones(int idLiqCol)
        {
            var list = new List<string>();
            using var cn = DatabaseHelper.GetConnection(); cn.Open();
            using var cmd = cn.CreateCommand();
            cmd.CommandText = "SELECT IdColumnasCSV FROM Auditar_RelacionCols WHERE IdLiqCols=@id";
            cmd.Parameters.AddWithValue("@id", idLiqCol.ToString());
            using var r = cmd.ExecuteReader();
            while (r.Read()) list.Add(r.GetString(0));
            return list;
        }

        public static List<RelacionCol> GetRelacionesConSigno(int idLiqCol)
        {
            var list = new List<RelacionCol>();
            using var cn = DatabaseHelper.GetConnection(); cn.Open();
            using var cmd = cn.CreateCommand();
            cmd.CommandText = "SELECT IdColumnasCSV, COALESCE(Signo,'+') FROM Auditar_RelacionCols WHERE IdLiqCols=@id";
            cmd.Parameters.AddWithValue("@id", idLiqCol.ToString());
            using var r = cmd.ExecuteReader();
            while (r.Read())
                list.Add(new RelacionCol
                {
                    IdLiqCols     = idLiqCol.ToString(),
                    IdColumnasCSV = r.GetString(0),
                    Signo         = r.IsDBNull(1) ? "+" : r.GetString(1)
                });
            return list;
        }

        public static void SetRelaciones(int idLiqCol, List<string> columnasCSV)
        {
            using var cn = DatabaseHelper.GetConnection(); cn.Open();
            using var tx = cn.BeginTransaction();
            using var del = cn.CreateCommand();
            del.Transaction = tx;
            del.CommandText = "DELETE FROM Auditar_RelacionCols WHERE IdLiqCols=@id";
            del.Parameters.AddWithValue("@id", idLiqCol.ToString());
            del.ExecuteNonQuery();
            foreach (var col in columnasCSV)
            {
                using var ins = cn.CreateCommand();
                ins.Transaction = tx;
                ins.CommandText = "INSERT OR IGNORE INTO Auditar_RelacionCols(IdLiqCols,IdColumnasCSV) VALUES(@l,@c)";
                ins.Parameters.AddWithValue("@l", idLiqCol.ToString());
                ins.Parameters.AddWithValue("@c", col);
                ins.ExecuteNonQuery();
            }
            tx.Commit();
        }

        // ── LineasCON ─────────────────────────────────────────────────────

        public static List<LineaCON> GetLineasCON(string idMarca)
        {
            var list = new List<LineaCON>();
            using var cn = DatabaseHelper.GetConnection(); cn.Open();
            using var cmd = cn.CreateCommand();
            cmd.CommandText = "SELECT id,idMarcaTarjeta,Descripcion,Orden,CondicionSigno FROM Auditar_LineasCON WHERE idMarcaTarjeta=@m ORDER BY Orden";
            cmd.Parameters.AddWithValue("@m", idMarca);
            using var r = cmd.ExecuteReader();
            while (r.Read())
                list.Add(new LineaCON { Id = r.GetInt32(0), IdMarcaTarjeta = r.GetString(1), Descripcion = r.GetString(2), Orden = r.GetInt32(3), CondicionSigno = r.IsDBNull(4) ? "" : r.GetString(4) });
            return list;
        }

        public static int InsertLineaCON(LineaCON l)
        {
            using var cn = DatabaseHelper.GetConnection(); cn.Open();
            using var cmd = cn.CreateCommand();
            cmd.CommandText = "INSERT INTO Auditar_LineasCON(idMarcaTarjeta,Descripcion,Orden,CondicionSigno) VALUES(@m,@d,@o,@c); SELECT last_insert_rowid();";
            cmd.Parameters.AddWithValue("@m", l.IdMarcaTarjeta);
            cmd.Parameters.AddWithValue("@d", l.Descripcion);
            cmd.Parameters.AddWithValue("@o", l.Orden);
            cmd.Parameters.AddWithValue("@c", l.CondicionSigno);
            return (int)(long)cmd.ExecuteScalar();
        }

        public static void UpdateLineaCON(LineaCON l)
        {
            using var cn = DatabaseHelper.GetConnection(); cn.Open();
            using var cmd = cn.CreateCommand();
            cmd.CommandText = "UPDATE Auditar_LineasCON SET Descripcion=@d,Orden=@o,CondicionSigno=@c WHERE id=@id";
            cmd.Parameters.AddWithValue("@d", l.Descripcion);
            cmd.Parameters.AddWithValue("@o", l.Orden);
            cmd.Parameters.AddWithValue("@c", l.CondicionSigno);
            cmd.Parameters.AddWithValue("@id", l.Id);
            cmd.ExecuteNonQuery();
        }

        public static void DeleteLineaCON(int id)
        {
            using var cn = DatabaseHelper.GetConnection(); cn.Open();
            cn.ExecuteNonQuery($"DELETE FROM Auditar_CON_Cols WHERE IdLineaCON={id}");
            cn.ExecuteNonQuery($"DELETE FROM Auditar_CON_ValoresFijos WHERE IdLineaCON={id}");
            cn.ExecuteNonQuery($"DELETE FROM Auditar_LineasCON WHERE id={id}");
        }

        // ── CON_Cols ──────────────────────────────────────────────────────

        public static List<string> GetCONCols(int idLineaCON)
        {
            var list = new List<string>();
            using var cn = DatabaseHelper.GetConnection(); cn.Open();
            using var cmd = cn.CreateCommand();
            cmd.CommandText = "SELECT IdColumnasCSV FROM Auditar_CON_Cols WHERE IdLineaCON=@id";
            cmd.Parameters.AddWithValue("@id", idLineaCON);
            using var r = cmd.ExecuteReader();
            while (r.Read()) list.Add(r.GetString(0));
            return list;
        }

        public static string GetCONCondicion(int idLineaCON)
        {
            using var cn = DatabaseHelper.GetConnection(); cn.Open();
            using var cmd = cn.CreateCommand();
            cmd.CommandText = "SELECT CondicionSigno FROM Auditar_LineasCON WHERE id=@id";
            cmd.Parameters.AddWithValue("@id", idLineaCON);
            using var r = cmd.ExecuteReader();
            if (r.Read()) return r.IsDBNull(0) ? "" : r.GetString(0);
            return "";
        }



        public static void SetCONCols(int idLineaCON, List<string> cols)
        {
            using var cn = DatabaseHelper.GetConnection(); cn.Open();
            using var tx = cn.BeginTransaction();
            using var del = cn.CreateCommand();
            del.Transaction = tx;
            del.CommandText = "DELETE FROM Auditar_CON_Cols WHERE IdLineaCON=@id";
            del.Parameters.AddWithValue("@id", idLineaCON);
            del.ExecuteNonQuery();
            foreach (var col in cols)
            {
                using var ins = cn.CreateCommand();
                ins.Transaction = tx;
                ins.CommandText = "INSERT OR IGNORE INTO Auditar_CON_Cols(IdLineaCON,IdColumnasCSV) VALUES(@l,@c)";
                ins.Parameters.AddWithValue("@l", idLineaCON);
                ins.Parameters.AddWithValue("@c", col);
                ins.ExecuteNonQuery();
            }
            tx.Commit();
        }

        // ── CON_ValoresFijos ──────────────────────────────────────────────

        public static List<CONValorFijo> GetCONValoresFijos(int idLineaCON)
        {
            var list = new List<CONValorFijo>();
            using var cn = DatabaseHelper.GetConnection(); cn.Open();
            using var cmd = cn.CreateCommand();
            cmd.CommandText = "SELECT id,IdLineaCON,Posicion,Valor FROM Auditar_CON_ValoresFijos WHERE IdLineaCON=@id ORDER BY Posicion";
            cmd.Parameters.AddWithValue("@id", idLineaCON);
            using var r = cmd.ExecuteReader();
            while (r.Read())
                list.Add(new CONValorFijo { Id = r.GetInt32(0), IdLineaCON = r.GetInt32(1), Posicion = r.GetInt32(2), Valor = r.GetString(3) });
            return list;
        }

        public static void SetCONValoresFijos(int idLineaCON, List<CONValorFijo> vals)
        {
            using var cn = DatabaseHelper.GetConnection(); cn.Open();
            using var tx = cn.BeginTransaction();
            using var del = cn.CreateCommand();
            del.Transaction = tx;
            del.CommandText = "DELETE FROM Auditar_CON_ValoresFijos WHERE IdLineaCON=@id";
            del.Parameters.AddWithValue("@id", idLineaCON);
            del.ExecuteNonQuery();
            foreach (var v in vals)
            {
                using var ins = cn.CreateCommand();
                ins.Transaction = tx;
                ins.CommandText = "INSERT INTO Auditar_CON_ValoresFijos(IdLineaCON,Posicion,Valor) VALUES(@l,@p,@v)";
                ins.Parameters.AddWithValue("@l", idLineaCON);
                ins.Parameters.AddWithValue("@p", v.Posicion);
                ins.Parameters.AddWithValue("@v", v.Valor);
                ins.ExecuteNonQuery();
            }
            tx.Commit();
        }
    }
}
