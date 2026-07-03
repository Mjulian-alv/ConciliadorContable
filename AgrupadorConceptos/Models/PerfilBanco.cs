namespace AgrupadorConceptos.Models
{
    public class PerfilBanco
    {
        public int Id { get; set; }
        public string NombreBanco { get; set; }
        public string ColumnaConcepto { get; set; }
        public string ColumnaDescripcion { get; set; }
        public bool EsCodigo { get; set; }
        public int FilaEncabezado { get; set; } = 1;
        public int TipoImporte { get; set; } // 1: Unico, 2: Debe/Haber
        public string ColumnaImporteUnico { get; set; }
        public string ColumnaDebe { get; set; }
        public string ColumnaHaber { get; set; }
        public string ColumnaFecha { get; set; }
    }
}