using System;
using System.Linq;
using System.Windows.Forms;

namespace AgrupadorConceptos
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            
            // Required for ExcelDataReader in .NET Core / .NET 5+
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

            // Inicializamos la base de datos SQlite local
            Data.DatabaseHelper.InitializeDatabase();

            // Generamos el archivo ico
            IconGenerator.GenerateIconFile();

            Application.Run(new ProcesadorForm());
        }
    }
}