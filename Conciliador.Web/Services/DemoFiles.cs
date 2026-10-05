// Fecha: 05/10/2026 - TAREA: 00064 - Linea: 4 - Ejemplos ficticios pasan por el mismo importador que los archivos reales.
using ArcaCliente.Web.Models;
using System.Text;
namespace Conciliador.Web.Services;
public static class DemoFiles {
    static UploadedDocument File(string name,string text)=>new(name,Encoding.UTF8.GetBytes(text));
    public static UploadedDocument Provider=>File("proveedor-ejemplo.csv","Tipo;Fecha;Numero;Importe;Proveedor\nFA;05/09/2026;00005-00001234;100000,00;Proveedor de ejemplo\nFA;08/09/2026;00005-00001235;60000,00;Proveedor de ejemplo\nNCA;09/09/2026;00005-00000081;-10000,00;Proveedor de ejemplo\nNDA;10/09/2026;00005-00000032;2500,00;Proveedor de ejemplo\nFA;11/09/2026;00005-00001236;75000,00;Proveedor de ejemplo\nFA;26/09/2026;00005-00001237;34000,00;Proveedor de ejemplo\nREC;;;-45000,00;Proveedor de ejemplo\nPAGO;;;-15000,00;Proveedor de ejemplo");
    public static UploadedDocument System=>File("sistema-ejemplo.csv","Fecha;Tipo;PuntoVenta;Numero;Importe;CUIT;Proveedor\n05/09/2026;001;5;1234;100000,00;30123456781;Proveedor de ejemplo\n08/09/2026;001;5;1235;100000,00;30123456781;Proveedor de ejemplo\n09/09/2026;003;5;81;10000,00;30123456781;Proveedor de ejemplo\n10/09/2026;002;5;32;2500,00;30123456781;Proveedor de ejemplo\n11/09/2026;001;5;1236;75000,00;30123456781;Proveedor de ejemplo\n26/09/2026;001;5;1290;8900,00;30123456781;Proveedor de ejemplo");
    public static UploadedDocument Arca=>File("arca-ejemplo.csv","Fecha de Emisión;Tipo de Comprobante;Punto de Venta;Número Desde;Nro. Doc. Emisor;Denominación Emisor;Imp. Total\n05/09/2026;001;5;1234;30123456781;Proveedor de ejemplo;100000,00\n08/09/2026;001;5;1235;30123456781;Proveedor de ejemplo;60000,00\n09/09/2026;003;5;81;30123456781;Proveedor de ejemplo;10000,00\n10/09/2026;002;5;32;30123456781;Proveedor de ejemplo;2500,00\n11/09/2026;001;5;1236;30123456781;Proveedor de ejemplo;75000,00\n26/09/2026;001;5;1237;30123456781;Proveedor de ejemplo;34000,00");
}
