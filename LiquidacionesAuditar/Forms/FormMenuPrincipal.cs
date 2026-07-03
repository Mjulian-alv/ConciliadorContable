using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using Telerik.WinControls.UI;
using LiquidacionesAuditar.Data;

namespace LiquidacionesAuditar
{
    public partial class FormMenuPrincipal : RadForm
    {
        public FormMenuPrincipal()
        {
            InitializeComponent();
            lblDBPath.Text = $"BD: {DatabaseHelper.DbPath}";
        }

        private void btnColumnasCSV_Click(object sender, EventArgs e)
            => AbrirVentana(new FormColumnasCSV());

        private void btnColumnasDestino_Click(object sender, EventArgs e)
            => AbrirVentana(new FormColumnasDestino());

        private void btnLineasCON_Click(object sender, EventArgs e)
            => AbrirVentana(new FormLineasCON());

        private void btnProcesador_Click(object sender, EventArgs e)
            => AbrirVentana(new FormProcesadorCSV());

        private void btnAbrirCarpetaDB_Click(object sender, EventArgs e)
        {
            var carpeta = Path.GetDirectoryName(DatabaseHelper.DbPath);
            if (Directory.Exists(carpeta))
                Process.Start("explorer.exe", carpeta);
        }

        private void AbrirVentana(Form f)
        {
            f.WindowState = FormWindowState.Maximized;
            f.Show();
        }
    }
}

