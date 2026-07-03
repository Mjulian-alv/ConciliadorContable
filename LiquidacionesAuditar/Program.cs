using System;
using System.Windows.Forms;
using LiquidacionesAuditar.Data;

namespace LiquidacionesAuditar
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            DatabaseHelper.InitializeDatabase();

            Application.Run(new FormMenuPrincipal());
        }
    }
}