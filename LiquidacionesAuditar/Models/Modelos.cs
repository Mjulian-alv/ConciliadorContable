using System;

namespace LiquidacionesAuditar.Models
{
    /// <summary>Tipos de dato soportados para el filtro de liquidación.</summary>
    public enum TipoFiltro { String, Int, Decimal, Date }

    /// <summary>Rango de filtro que el usuario configuró antes de procesar.</summary>
    public class FiltroLiquidacion
    {
        public string Columna { get; set; } = "";
        public TipoFiltro Tipo { get; set; } = TipoFiltro.String;

        // Para String: filtro de texto libre (contains)
        public string TextoContiene { get; set; } = "";

        // Para Int / Decimal: rango numérico
        public decimal? NumDesde { get; set; }
        public decimal? NumHasta { get; set; }

        // Para Date: rango de fechas
        public DateTime? FechaDesde { get; set; }
        public DateTime? FechaHasta { get; set; }

        public bool EstaActivo =>
            !string.IsNullOrWhiteSpace(Columna) &&
            (Tipo == TipoFiltro.String  ? !string.IsNullOrWhiteSpace(TextoContiene)
           : Tipo == TipoFiltro.Date    ? FechaDesde.HasValue || FechaHasta.HasValue
           :                             NumDesde.HasValue    || NumHasta.HasValue);
    }

    public class MarcaTarjeta
    {
        public string Id { get; set; } = "";
        public string Nombre { get; set; } = "";
        /// <summary>Fila (1-based) donde están los encabezados en los archivos Excel.</summary>
        public int FilaEncabezado { get; set; } = 1;
        /// <summary>Nombre de la columna del archivo que actúa como Nro. de Liquidación y clave de agrupamiento. Null/vacío = comportamiento original (agrupa por fecha).</summary>
        public string ColumnaLiquidacion { get; set; } = "";
        public string SeparadorDecimales { get; set; } = ",";
        public string SeparadorMiles { get; set; } = ".";
        public string TipoDato { get; set; } = "";       // string, int, decimal, date
        public string ColumnaFiltro { get; set; } = "";
        public override string ToString() => Nombre;
    }

    public class ColumnaCSV
    {
        public int Id { get; set; }
        public string IdMarcaTarjeta { get; set; } = "";
        public string IdColumnaArchivo { get; set; } = "";
        public override string ToString() => IdColumnaArchivo;
    }

    public class LiqCol
    {
        public int Id { get; set; }
        public string IdMarcaTarjeta { get; set; } = "";
        public string IdColumna { get; set; } = "";
        public string TipoRegistro { get; set; } = "";   // CAB, DET, CON
        public string TipoDato { get; set; } = "";       // string, int, decimal, date
        public int Orden { get; set; }
        public string ValorFijo { get; set; } = "";
        public string Condicion { get; set; } = "";
        public bool tieneSigno { get; set; } = false;
        public string CondicionSigno { get; set; } =""; // Idem a Condicion pero para determinar el signo del valor (positivo o negativo)
        public bool esFiltro { get; set; } = false;
        public override string ToString() => $"{TipoRegistro} | {IdColumna}";
    }

    public class RelacionCol
    {
        public string IdLiqCols { get; set; } = "";
        public string IdColumnasCSV { get; set; } = "";
        public string Signo { get; set; } = "+";   // "+" (suma) o "-" (resta)
    }

    public class LineaCON
    {
        public int Id { get; set; }
        public string IdMarcaTarjeta { get; set; } = "";
        public string Descripcion { get; set; } = "";
        public int Orden { get; set; }
        public string CondicionSigno { get; set; } = "";
        public override string ToString() => Descripcion;
    }

    public class CONCol
    {
        public int IdLineaCON { get; set; }
        public string IdColumnasCSV { get; set; } = "";
    }

    public class CONValorFijo
    {
        public int Id { get; set; }
        public int IdLineaCON { get; set; }
        public int Posicion { get; set; }
        public string Valor { get; set; } = "";
    }
}
