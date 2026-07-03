using System;
using System.Windows.Forms;
using ArcaCliente.Services;

namespace ArcaCliente
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Escribir el .ico en disco para la barra de tareas y el acceso directo
            EnsureIcoFile();

            Application.Run(new FormMenu());
        }

        /// <summary>
        /// Genera arca.ico junto al ejecutable si no existe todavía.
        /// El archivo se puede referenciar en el proyecto como ApplicationIcon.
        /// </summary>
        private static void EnsureIcoFile()
        {
            try
            {
                string path = System.IO.Path.Combine(
                    AppContext.BaseDirectory, "arca.ico");

                if (!System.IO.File.Exists(path))
                    using (var fs = System.IO.File.Create(path))
                        AppIcons.Arca.Save(fs);
            }
            catch { /* no crítico */ }
        }
    }
}