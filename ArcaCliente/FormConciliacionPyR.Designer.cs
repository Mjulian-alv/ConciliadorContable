// Fecha: 21/09/2026 - TAREA: 00041 - Linea: 3, 4 - Pantalla de conciliación de percepciones y retenciones (maqueta 04-conciliacion-pyr)
namespace ArcaCliente
{
    partial class FormConciliacionPyR
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            tblTop            = new System.Windows.Forms.TableLayoutPanel();
            grpArca           = new System.Windows.Forms.GroupBox();
            lblCarpeta        = new Telerik.WinControls.UI.RadLabel();
            txtCarpeta        = new Telerik.WinControls.UI.RadTextBox();
            btnBrowseCarpeta  = new Telerik.WinControls.UI.RadButton();
            btnCargarCarpeta  = new Telerik.WinControls.UI.RadButton();
            lblEstadoArca     = new Telerik.WinControls.UI.RadLabel();
            lblResumenArca    = new Telerik.WinControls.UI.RadLabel();
            grpPresea         = new System.Windows.Forms.GroupBox();
            gridMayores       = new Telerik.WinControls.UI.RadGridView();
            lblSinMayores     = new Telerik.WinControls.UI.RadLabel();
            btnAgregarMayor   = new Telerik.WinControls.UI.RadButton();
            btnQuitarMayor    = new Telerik.WinControls.UI.RadButton();
            lblEstadoPresea   = new Telerik.WinControls.UI.RadLabel();
            splitGrillas      = new System.Windows.Forms.SplitContainer();
            pvArca            = new Telerik.WinControls.UI.RadPageView();
            pageArca          = new Telerik.WinControls.UI.RadPageViewPage();
            gridArca          = new Telerik.WinControls.UI.RadGridView();
            lblVacioArca      = new Telerik.WinControls.UI.RadLabel();
            pvPresea          = new Telerik.WinControls.UI.RadPageView();
            pagePresea        = new Telerik.WinControls.UI.RadPageViewPage();
            gridPresea        = new Telerik.WinControls.UI.RadGridView();
            lblVacioPresea    = new Telerik.WinControls.UI.RadLabel();
            pageSinComprobante = new Telerik.WinControls.UI.RadPageViewPage();
            gridSinComprobante = new Telerik.WinControls.UI.RadGridView();
            pnlPie            = new System.Windows.Forms.Panel();
            lblPerfil         = new Telerik.WinControls.UI.RadLabel();
            cmbPerfil         = new Telerik.WinControls.UI.RadDropDownList();
            lblProximaEtapa   = new Telerik.WinControls.UI.RadLabel();
            btnConciliar      = new Telerik.WinControls.UI.RadButton();

            ((System.ComponentModel.ISupportInitialize)gridMayores).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pvArca).BeginInit();
            ((System.ComponentModel.ISupportInitialize)gridArca).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pvPresea).BeginInit();
            ((System.ComponentModel.ISupportInitialize)gridPresea).BeginInit();
            ((System.ComponentModel.ISupportInitialize)gridSinComprobante).BeginInit();
            ((System.ComponentModel.ISupportInitialize)splitGrillas).BeginInit();
            ((System.ComponentModel.ISupportInitialize)this).BeginInit();
            splitGrillas.Panel1.SuspendLayout();
            splitGrillas.Panel2.SuspendLayout();
            splitGrillas.SuspendLayout();
            tblTop.SuspendLayout();
            grpArca.SuspendLayout();
            grpPresea.SuspendLayout();
            pnlPie.SuspendLayout();
            SuspendLayout();

            // ── tblTop: ARCA a la izquierda, PRESEA a la derecha ─────────────────
            tblTop.Dock        = System.Windows.Forms.DockStyle.Top;
            tblTop.Height      = 150;
            tblTop.Padding     = new System.Windows.Forms.Padding(8, 6, 8, 0);
            tblTop.ColumnCount = 2;
            tblTop.RowCount    = 1;
            tblTop.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            tblTop.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            tblTop.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tblTop.Controls.Add(grpArca, 0, 0);
            tblTop.Controls.Add(grpPresea, 1, 0);

            // ── grpArca ──────────────────────────────────────────────────────────
            // Tamaño inicial explícito: los anchors de los controles internos se calculan contra
            // este tamaño; con el default de GroupBox (200x100) los botones quedaban fuera de lugar.
            grpArca.Dock = System.Windows.Forms.DockStyle.Fill;
            grpArca.Size = new System.Drawing.Size(686, 138);
            grpArca.Text = "ARCA — carpeta de certificados";

            lblCarpeta.Location = new System.Drawing.Point(12, 28);
            lblCarpeta.Text     = "Carpeta:";

            txtCarpeta.Location = new System.Drawing.Point(70, 25);
            txtCarpeta.Size     = new System.Drawing.Size(426, 24);
            txtCarpeta.Anchor   = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;

            btnBrowseCarpeta.Location = new System.Drawing.Point(502, 24);
            btnBrowseCarpeta.Size     = new System.Drawing.Size(34, 26);
            btnBrowseCarpeta.Text     = "...";
            btnBrowseCarpeta.Anchor   = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            btnBrowseCarpeta.Click   += BtnBrowseCarpeta_Click;

            btnCargarCarpeta.Font     = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            btnCargarCarpeta.Location = new System.Drawing.Point(542, 24);
            btnCargarCarpeta.Size     = new System.Drawing.Size(130, 26);
            btnCargarCarpeta.Text     = "Cargar carpeta";
            btnCargarCarpeta.Anchor   = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            btnCargarCarpeta.Click   += BtnCargarCarpeta_Click;

            lblEstadoArca.Location = new System.Drawing.Point(12, 60);
            lblEstadoArca.AutoSize = false;
            lblEstadoArca.Size     = new System.Drawing.Size(660, 20);
            lblEstadoArca.Anchor   = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            lblEstadoArca.Font     = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);

            lblResumenArca.Location = new System.Drawing.Point(12, 86);
            lblResumenArca.AutoSize = false;
            lblResumenArca.Size     = new System.Drawing.Size(660, 40);
            lblResumenArca.Anchor   = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;

            grpArca.Controls.AddRange(new System.Windows.Forms.Control[]
            {
                lblCarpeta, txtCarpeta, btnBrowseCarpeta, btnCargarCarpeta, lblEstadoArca, lblResumenArca
            });

            // ── grpPresea ────────────────────────────────────────────────────────
            grpPresea.Dock = System.Windows.Forms.DockStyle.Fill;
            grpPresea.Size = new System.Drawing.Size(686, 138);
            grpPresea.Text = "PRESEA — mayores por cuenta";

            gridMayores.Location = new System.Drawing.Point(12, 22);
            gridMayores.Size     = new System.Drawing.Size(544, 88);
            gridMayores.Anchor   = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            gridMayores.MasterTemplate.AllowAddNewRow = false;
            gridMayores.MasterTemplate.AllowDeleteRow = false;
            gridMayores.MasterTemplate.AllowEditRow   = false;
            gridMayores.MasterTemplate.ShowRowHeaderColumn = false;
            gridMayores.MasterTemplate.AutoSizeColumnsMode = Telerik.WinControls.UI.GridViewAutoSizeColumnsMode.Fill;

            lblSinMayores.AutoSize      = false;
            lblSinMayores.Location      = new System.Drawing.Point(12, 22);
            lblSinMayores.Size          = new System.Drawing.Size(544, 88);
            lblSinMayores.Anchor        = gridMayores.Anchor;
            lblSinMayores.TextAlignment = System.Drawing.ContentAlignment.MiddleCenter;
            lblSinMayores.ForeColor     = System.Drawing.Color.Gray;
            lblSinMayores.Font          = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Italic);
            lblSinMayores.BackColor     = System.Drawing.Color.White;
            lblSinMayores.BorderVisible = true;
            lblSinMayores.Text          = "Todavía no se agregó ningún mayor.";

            btnAgregarMayor.Font     = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            btnAgregarMayor.Location = new System.Drawing.Point(564, 22);
            btnAgregarMayor.Size     = new System.Drawing.Size(108, 28);
            btnAgregarMayor.Text     = "Agregar...";
            btnAgregarMayor.Anchor   = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            btnAgregarMayor.Click   += BtnAgregarMayor_Click;

            btnQuitarMayor.Location = new System.Drawing.Point(564, 56);
            btnQuitarMayor.Size     = new System.Drawing.Size(108, 28);
            btnQuitarMayor.Text     = "Quitar";
            btnQuitarMayor.Anchor   = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            btnQuitarMayor.Click   += BtnQuitarMayor_Click;

            lblEstadoPresea.Location = new System.Drawing.Point(12, 116);
            lblEstadoPresea.AutoSize = false;
            lblEstadoPresea.Size     = new System.Drawing.Size(660, 20);
            lblEstadoPresea.Anchor   = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            lblEstadoPresea.Font     = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);

            grpPresea.Controls.AddRange(new System.Windows.Forms.Control[]
            {
                lblSinMayores, gridMayores, btnAgregarMayor, btnQuitarMayor, lblEstadoPresea
            });

            // ── Grillas: ARCA arriba, PRESEA abajo ───────────────────────────────
            splitGrillas.Dock        = System.Windows.Forms.DockStyle.Fill;
            splitGrillas.Orientation = System.Windows.Forms.Orientation.Horizontal;
            splitGrillas.Padding     = new System.Windows.Forms.Padding(8, 4, 8, 0);
            splitGrillas.Panel1.Controls.Add(pvArca);
            splitGrillas.Panel2.Controls.Add(pvPresea);

            pvArca.Dock = System.Windows.Forms.DockStyle.Fill;
            pvArca.Pages.Add(pageArca);
            pageArca.Text = "Registros ARCA";
            pageArca.Controls.Add(gridArca);
            pageArca.Controls.Add(lblVacioArca);

            ConfigurarGrillaSoloLectura(gridArca);

            lblVacioArca.Dock          = System.Windows.Forms.DockStyle.Fill;
            lblVacioArca.AutoSize      = false;
            lblVacioArca.TextAlignment = System.Drawing.ContentAlignment.MiddleCenter;
            lblVacioArca.ForeColor     = System.Drawing.Color.Gray;
            lblVacioArca.Font          = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Italic);
            lblVacioArca.Text          = "Cargá la carpeta de ARCA para ver los certificados.";

            pvPresea.Dock = System.Windows.Forms.DockStyle.Fill;
            pvPresea.Pages.Add(pagePresea);
            pvPresea.Pages.Add(pageSinComprobante);
            pagePresea.Text = "Mayor PRESEA";
            pagePresea.Controls.Add(gridPresea);
            pagePresea.Controls.Add(lblVacioPresea);
            pageSinComprobante.Text = "Sin comprobante";
            pageSinComprobante.Controls.Add(gridSinComprobante);

            ConfigurarGrillaSoloLectura(gridPresea);
            ConfigurarGrillaSoloLectura(gridSinComprobante);

            lblVacioPresea.Dock          = System.Windows.Forms.DockStyle.Fill;
            lblVacioPresea.AutoSize      = false;
            lblVacioPresea.TextAlignment = System.Drawing.ContentAlignment.MiddleCenter;
            lblVacioPresea.ForeColor     = System.Drawing.Color.Gray;
            lblVacioPresea.Font          = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Italic);
            lblVacioPresea.Text          = "Agregá los mayores de PRESEA eligiendo a qué cuenta corresponde cada uno.";

            // ── pnlPie ───────────────────────────────────────────────────────────
            pnlPie.Dock   = System.Windows.Forms.DockStyle.Bottom;
            pnlPie.Height = 52;

            lblPerfil.Location = new System.Drawing.Point(12, 17);
            lblPerfil.Text     = "Perfil:";
            cmbPerfil.Location      = new System.Drawing.Point(58, 14);
            cmbPerfil.Size          = new System.Drawing.Size(320, 24);
            cmbPerfil.DropDownStyle = Telerik.WinControls.RadDropDownStyle.DropDownList;

            lblProximaEtapa.Anchor    = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            lblProximaEtapa.Location  = new System.Drawing.Point(1010, 17);
            lblProximaEtapa.ForeColor = System.Drawing.Color.Gray;
            lblProximaEtapa.Text      = "Directivas y conciliación: próxima etapa";

            btnConciliar.Anchor   = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            btnConciliar.Font     = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            btnConciliar.Location = new System.Drawing.Point(1256, 9);
            btnConciliar.Size     = new System.Drawing.Size(130, 34);
            btnConciliar.Text     = "CONCILIAR";
            btnConciliar.Enabled  = false;   // directivas y conciliación: próxima etapa de la TAREA 00041

            pnlPie.Controls.AddRange(new System.Windows.Forms.Control[] { lblPerfil, cmbPerfil, lblProximaEtapa, btnConciliar });

            // ── Form ─────────────────────────────────────────────────────────────
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode       = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize          = new System.Drawing.Size(1400, 820);
            MinimumSize         = new System.Drawing.Size(1100, 640);
            Controls.Add(splitGrillas);
            Controls.Add(tblTop);
            Controls.Add(pnlPie);
            Name          = "FormConciliacionPyR";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text          = "ARCA Cliente — Conciliación Percepciones y Retenciones";

            grpArca.ResumeLayout(false);
            grpPresea.ResumeLayout(false);
            tblTop.ResumeLayout(false);
            pnlPie.ResumeLayout(false);
            splitGrillas.Panel1.ResumeLayout(false);
            splitGrillas.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitGrillas).EndInit();
            splitGrillas.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)gridMayores).EndInit();
            ((System.ComponentModel.ISupportInitialize)pvArca).EndInit();
            ((System.ComponentModel.ISupportInitialize)gridArca).EndInit();
            ((System.ComponentModel.ISupportInitialize)pvPresea).EndInit();
            ((System.ComponentModel.ISupportInitialize)gridPresea).EndInit();
            ((System.ComponentModel.ISupportInitialize)gridSinComprobante).EndInit();
            ((System.ComponentModel.ISupportInitialize)this).EndInit();
            ResumeLayout(false);
        }

        private static void ConfigurarGrillaSoloLectura(Telerik.WinControls.UI.RadGridView grid)
        {
            grid.Dock = System.Windows.Forms.DockStyle.Fill;
            grid.MasterTemplate.AllowAddNewRow = false;
            grid.MasterTemplate.AllowDeleteRow = false;
            grid.MasterTemplate.AllowEditRow   = false;
            grid.MasterTemplate.ShowRowHeaderColumn = false;
            grid.MasterTemplate.AutoGenerateColumns = false;
        }

        private System.Windows.Forms.TableLayoutPanel  tblTop;
        private System.Windows.Forms.GroupBox          grpArca;
        private Telerik.WinControls.UI.RadLabel        lblCarpeta;
        private Telerik.WinControls.UI.RadTextBox      txtCarpeta;
        private Telerik.WinControls.UI.RadButton       btnBrowseCarpeta;
        private Telerik.WinControls.UI.RadButton       btnCargarCarpeta;
        private Telerik.WinControls.UI.RadLabel        lblEstadoArca;
        private Telerik.WinControls.UI.RadLabel        lblResumenArca;
        private System.Windows.Forms.GroupBox          grpPresea;
        private Telerik.WinControls.UI.RadGridView     gridMayores;
        private Telerik.WinControls.UI.RadLabel        lblSinMayores;
        private Telerik.WinControls.UI.RadButton       btnAgregarMayor;
        private Telerik.WinControls.UI.RadButton       btnQuitarMayor;
        private Telerik.WinControls.UI.RadLabel        lblEstadoPresea;
        private System.Windows.Forms.SplitContainer    splitGrillas;
        private Telerik.WinControls.UI.RadPageView     pvArca;
        private Telerik.WinControls.UI.RadPageViewPage pageArca;
        private Telerik.WinControls.UI.RadGridView     gridArca;
        private Telerik.WinControls.UI.RadLabel        lblVacioArca;
        private Telerik.WinControls.UI.RadPageView     pvPresea;
        private Telerik.WinControls.UI.RadPageViewPage pagePresea;
        private Telerik.WinControls.UI.RadGridView     gridPresea;
        private Telerik.WinControls.UI.RadLabel        lblVacioPresea;
        private Telerik.WinControls.UI.RadPageViewPage pageSinComprobante;
        private Telerik.WinControls.UI.RadGridView     gridSinComprobante;
        private System.Windows.Forms.Panel             pnlPie;
        private Telerik.WinControls.UI.RadLabel        lblPerfil;
        private Telerik.WinControls.UI.RadDropDownList cmbPerfil;
        private Telerik.WinControls.UI.RadLabel        lblProximaEtapa;
        private Telerik.WinControls.UI.RadButton       btnConciliar;
    }
}
