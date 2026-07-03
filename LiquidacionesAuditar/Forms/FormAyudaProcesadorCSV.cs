using System.Drawing;
using System.Windows.Forms;

namespace LiquidacionesAuditar
{
    /// <summary>
    /// Guía de uso del formulario Procesar / Exportar CSV.
    /// </summary>
    public partial class FormAyudaProcesadorCSV : Form
    {
        public FormAyudaProcesadorCSV()
        {
            InitializeComponent();
            CargarContenido();
        }

        private void CargarContenido()
        {
            _rtb.Clear();

            Titulo("Guía de uso — Procesar / Exportar CSV");
            Separador();

            Seccion("¿Para qué sirve esta pantalla?");
            Parrafo(
                "Esta pantalla permite tomar un archivo CSV o Excel de liquidación de tarjetas, " +
                "aplicar opciones de filtrado, generar la vista previa del archivo de salida TXT " +
                "con el formato esperado por el sistema contable, y finalmente exportar ese archivo.");

            Separador();
            Seccion("Paso 1 — Seleccionar la Marca / Procesador");
            Parrafo(
                "Elegí del combo superior la marca o procesador correspondiente al archivo que vas a " +
                "importar (ej: Visa, Mastercard, Naranja, etc.). Esta selección determina:");
            Lista(new[]
            {
                "Qué columnas se esperan en el archivo.",
                "Cómo se agrupan las filas (por número de liquidación o por fecha).",
                "Los separadores decimal y de miles a usar al parsear importes.",
                "La columna y tipo de dato disponibles para filtrar."
            });

            Separador();
            Seccion("Paso 2 — Configurar el Nro. base de liquidación");
            Parrafo(
                "El campo \"Nro. base liquidación\" define el prefijo numérico que se usa cuando el " +
                "agrupamiento es por fecha (no hay columna de liquidación configurada). " +
                "Por defecto es 1000. Si la marca tiene configurada una columna de liquidación, " +
                "este valor se ignora y se usa el valor de esa columna directamente.");

            Separador();
            Seccion("Paso 3 — Separadores decimal y de miles");
            Parrafo(
                "Los campos \"Sep. decimal\" y \"Sep. miles\" se cargan automáticamente desde la " +
                "configuración de la marca seleccionada. Podés modificarlos manualmente si el archivo " +
                "puntual usa un formato diferente. Estos separadores se usan para interpretar " +
                "correctamente los importes del CSV antes de sumarlos o exportarlos.");
            Lista(new[]
            {
                "Ejemplo Argentina: decimal = , (coma)  |  miles = . (punto)",
                "Ejemplo internacional: decimal = . (punto)  |  miles = , (coma)"
            });

            Separador();
            Seccion("Paso 4 — Seleccionar el archivo CSV o Excel");
            Parrafo(
                "Hacé clic en \"Seleccionar CSV...\" y elegí el archivo a procesar. " +
                "Se aceptan archivos .csv, .xlsx y .xls. Al cargar:");
            Lista(new[]
            {
                "La barra de estado muestra la cantidad de filas y columnas encontradas.",
                "La grilla de la izquierda muestra una vista previa (hasta 50 filas y 20 columnas).",
                "Se validan automáticamente las columnas del archivo contra las registradas en la base " +
                "de datos para la marca seleccionada.",
                "Si hay diferencias de columnas, aparece el botón \"Ver diferencias\" con el detalle."
            });

            Separador();
            Seccion("Paso 5 — Definir filtro de liquidación (opcional)");
            Parrafo(
                "El botón \"Definir Filtros\" permite acotar las filas a procesar antes de generar " +
                "el archivo de salida. El filtro disponible depende de la configuración de la marca:");
            Lista(new[]
            {
                "STRING: filtra filas donde la columna configurada contenga el texto indicado.",
                "INT / DECIMAL: filtra filas cuyo valor numérico esté dentro del rango Desde–Hasta.",
                "DATE: filtra filas cuya fecha esté dentro del rango de fechas Desde–Hasta."
            });
            Parrafo(
                "Cuando hay un filtro activo, el botón cambia a color verde y muestra el nombre de la " +
                "columna con un tilde (✔). Para desactivarlo, abrí el diálogo y hacé clic en \"Limpiar\". " +
                "Al cambiar de marca el filtro se borra automáticamente.");
            Nota("El filtro de liquidación debe estar configurado previamente en la pantalla " +
                 "\"Columnas CSV\" → sección \"Filtro de Liquidacion\" (columna y tipo de dato).");

            Separador();
            Seccion("Paso 6 — Procesar Vista Previa");
            Parrafo(
                "Hacé clic en \"Procesar Vista Previa\" para generar el contenido del archivo de " +
                "salida. El resultado aparece en el panel derecho en texto monocromático. " +
                "Cada línea tiene el formato:");
            Lista(new[]
            {
                "CAB|nroLiquidacion|campo1|campo2|...   → encabezado de liquidación",
                "DET|campo1|campo2|...                  → detalle de cada transacción",
                "CON|nroLiquidacion|total|descripcion|... → línea de totales"
            });
            Parrafo(
                "Si el filtro está activo, la barra de estado informa cuántas filas quedaron " +
                "incluidas del total cargado.");

            Separador();
            Seccion("Paso 7 — Generar / Exportar");
            Parrafo(
                "Una vez conforme con la vista previa, hacé clic en \"Generar / Exportar\" para " +
                "guardar el archivo TXT. El sistema propone un nombre automático con el formato:");
            Lista(new[]
            {
                "Liquidacion_[Marca]_[NroBase]_[ddMMaaaa].txt"
            });
            Parrafo("El archivo se guarda en la ubicación que elijas con codificación UTF-8.");

            Separador();
            Seccion("Validación de columnas");
            Parrafo(
                "Cada vez que se carga un archivo, el sistema compara sus columnas con las " +
                "registradas en la base de datos para la marca seleccionada:");
            Lista(new[]
            {
                "✔ Columnas OK: el archivo coincide exactamente con lo esperado.",
                "⚠ Diferencias: aparece el botón \"Ver diferencias\" que muestra en detalle " +
                "qué columnas están solo en el archivo y cuáles solo en la base de datos. " +
                "Desde ese diálogo también podés agregar columnas nuevas a la configuración."
            });

            Separador();
            Seccion("Configuración previa requerida");
            Parrafo("Para que esta pantalla funcione correctamente, previamente debe estar configurado:");
            Lista(new[]
            {
                "La marca/procesador con su columna de liquidación y separadores (pantalla \"Columnas CSV\").",
                "Las columnas CSV esperadas para esa marca.",
                "Las columnas de destino (CAB/DET/CON) y sus relaciones con las columnas CSV " +
                "(pantalla \"Columnas Destino\").",
                "Las líneas de totalizador CON y las columnas que suman (pantalla \"Líneas CON\").",
                "Opcionalmente: columna y tipo de dato para el filtro de liquidación."
            });

            // Posicionar al inicio
            _rtb.SelectionStart = 0;
            _rtb.ScrollToCaret();
        }

        // ── Helpers de formato ────────────────────────────────────────────

        private void Titulo(string texto)
        {
            Append(texto + "\n", new Font("Segoe UI", 14F, FontStyle.Bold), Color.FromArgb(30, 60, 120));
        }

        private void Seccion(string texto)
        {
            Append("\n" + texto + "\n", new Font("Segoe UI", 11F, FontStyle.Bold), Color.FromArgb(20, 80, 160));
        }

        private void Parrafo(string texto)
        {
            Append(texto + "\n", new Font("Segoe UI", 10F, FontStyle.Regular), Color.FromArgb(30, 30, 30));
        }

        private void Lista(string[] items)
        {
            foreach (var item in items)
                Append("  • " + item + "\n", new Font("Segoe UI", 10F, FontStyle.Regular), Color.FromArgb(50, 50, 50));
        }

        private void Nota(string texto)
        {
            Append("\n  ℹ " + texto + "\n", new Font("Segoe UI", 9.5F, FontStyle.Italic), Color.FromArgb(100, 80, 0));
        }

        private void Separador()
        {
            Append("\n" + new string('─', 80) + "\n", new Font("Segoe UI", 8F, FontStyle.Regular), Color.FromArgb(180, 180, 180));
        }

        private void Append(string texto, Font fuente, Color color)
        {
            int start = _rtb.TextLength;
            _rtb.AppendText(texto);
            _rtb.Select(start, texto.Length);
            _rtb.SelectionFont = fuente;
            _rtb.SelectionColor = color;
            _rtb.SelectionLength = 0;
        }
    }
}
