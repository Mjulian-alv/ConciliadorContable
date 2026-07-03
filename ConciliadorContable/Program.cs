using System;
using System.Windows.Forms;
using ConciliadorContable.Data;
using ConciliadorContable.Forms;

namespace ConciliadorContable
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            DatabaseHelper.InitializeDatabase();

            using var login = new FormLogin();
            if (login.ShowDialog() != DialogResult.OK)
                return;

            Application.Run(new FormMenuPrincipal());
        }
    }
}