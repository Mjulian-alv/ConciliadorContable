// Fecha: 21/09/2026 - TAREA: 00041 - Linea: 2 - Detalle de perfil PyR (maqueta 01-perfil-pyr-detalle)
namespace ArcaCliente
{
    partial class FormPerfilPyRDetalle
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
            lblNombre          = new Telerik.WinControls.UI.RadLabel();
            txtNombre          = new Telerik.WinControls.UI.RadTextBox();
            grpArchivo         = new System.Windows.Forms.GroupBox();
            lblTipoArchivo     = new Telerik.WinControls.UI.RadLabel();
            cmbTipoArchivo     = new Telerik.WinControls.UI.RadDropDownList();
            lblHoja            = new Telerik.WinControls.UI.RadLabel();
            txtHojaExcel       = new Telerik.WinControls.UI.RadTextBox();
            chkTieneCabecera   = new Telerik.WinControls.UI.RadCheckBox();
            lblFormatoFecha    = new Telerik.WinControls.UI.RadLabel();
            cmbFormatoFecha    = new Telerik.WinControls.UI.RadDropDownList();
            lblDecimal         = new Telerik.WinControls.UI.RadLabel();
            cmbSeparadorDecimal = new Telerik.WinControls.UI.RadDropDownList();
            grpColumnas        = new System.Windows.Forms.GroupBox();
            lblColFecha        = new Telerik.WinControls.UI.RadLabel();
            txtColFecha        = new Telerik.WinControls.UI.RadTextBox();
            lblColAsiento      = new Telerik.WinControls.UI.RadLabel();
            txtColAsiento      = new Telerik.WinControls.UI.RadTextBox();
            lblColConcepto     = new Telerik.WinControls.UI.RadLabel();
            txtColConcepto     = new Telerik.WinControls.UI.RadTextBox();
            lblColDebe         = new Telerik.WinControls.UI.RadLabel();
            txtColDebe         = new Telerik.WinControls.UI.RadTextBox();
            lblColHaber        = new Telerik.WinControls.UI.RadLabel();
            txtColHaber        = new Telerik.WinControls.UI.RadTextBox();
            lblAyudaConcepto   = new Telerik.WinControls.UI.RadLabel();
            grpCuentas         = new System.Windows.Forms.GroupBox();
            gridCuentas        = new Telerik.WinControls.UI.RadGridView();
            lblSinCuentas      = new Telerik.WinControls.UI.RadLabel();
            btnAgregarCuenta   = new Telerik.WinControls.UI.RadButton();
            btnEditarCuenta    = new Telerik.WinControls.UI.RadButton();
            btnQuitarCuenta    = new Telerik.WinControls.UI.RadButton();
            btnGuardar         = new Telerik.WinControls.UI.RadButton();
            btnCancelar        = new Telerik.WinControls.UI.RadButton();

            ((System.ComponentModel.ISupportInitialize)gridCuentas).BeginInit();
            ((System.ComponentModel.ISupportInitialize)this).BeginInit();
            grpArchivo.SuspendLayout();
            grpColumnas.SuspendLayout();
            grpCuentas.SuspendLayout();
            SuspendLayout();

            // ── Nombre ───────────────────────────────────────────────────────────
            lblNombre.Location = new System.Drawing.Point(14, 16);
            lblNombre.Text     = "Nombre del perfil:";
            txtNombre.Location = new System.Drawing.Point(140, 13);
            txtNombre.Size     = new System.Drawing.Size(420, 24);
            txtNombre.TabIndex = 0;

            // ── grpArchivo ───────────────────────────────────────────────────────
            grpArchivo.Location = new System.Drawing.Point(12, 48);
            grpArchivo.Size     = new System.Drawing.Size(736, 90);
            grpArchivo.Text     = "Mayor de PRESEA (archivo)";
            grpArchivo.TabIndex = 1;

            lblTipoArchivo.Location = new System.Drawing.Point(12, 25);
            lblTipoArchivo.Text     = "Tipo de archivo:";
            cmbTipoArchivo.Location = new System.Drawing.Point(128, 22);
            cmbTipoArchivo.Size     = new System.Drawing.Size(150, 24);
            cmbTipoArchivo.Enabled  = false;   // PRESEA exporta el mayor en Excel: no hay otra opción
            cmbTipoArchivo.TabIndex = 0;

            lblHoja.Location = new System.Drawing.Point(296, 25);
            lblHoja.Text     = "Hoja:";
            txtHojaExcel.Location = new System.Drawing.Point(340, 22);
            txtHojaExcel.Size     = new System.Drawing.Size(140, 24);
            txtHojaExcel.NullText = "(primera hoja)";
            txtHojaExcel.TabIndex = 1;

            chkTieneCabecera.Location = new System.Drawing.Point(500, 24);
            chkTieneCabecera.Text     = "Tiene cabecera";
            chkTieneCabecera.TabIndex = 2;
            chkTieneCabecera.ToggleStateChanged += ChkTieneCabecera_ToggleStateChanged;

            lblFormatoFecha.Location = new System.Drawing.Point(12, 57);
            lblFormatoFecha.Text     = "Formato de fecha:";
            cmbFormatoFecha.Location = new System.Drawing.Point(128, 54);
            cmbFormatoFecha.Size     = new System.Drawing.Size(150, 24);
            cmbFormatoFecha.DropDownStyle = Telerik.WinControls.RadDropDownStyle.DropDownList;
            cmbFormatoFecha.TabIndex = 3;

            lblDecimal.Location = new System.Drawing.Point(296, 57);
            lblDecimal.Text     = "Decimal:";
            cmbSeparadorDecimal.Location = new System.Drawing.Point(340, 54);
            cmbSeparadorDecimal.Size     = new System.Drawing.Size(140, 24);
            cmbSeparadorDecimal.DropDownStyle = Telerik.WinControls.RadDropDownStyle.DropDownList;
            cmbSeparadorDecimal.TabIndex = 4;

            grpArchivo.Controls.AddRange(new System.Windows.Forms.Control[]
            {
                lblTipoArchivo, cmbTipoArchivo, lblHoja, txtHojaExcel, chkTieneCabecera,
                lblFormatoFecha, cmbFormatoFecha, lblDecimal, cmbSeparadorDecimal
            });

            // ── grpColumnas ──────────────────────────────────────────────────────
            grpColumnas.Location = new System.Drawing.Point(12, 146);
            grpColumnas.Size     = new System.Drawing.Size(736, 112);
            grpColumnas.Text     = "Columnas del mayor (nombre del encabezado)";
            grpColumnas.TabIndex = 2;

            lblColFecha.Location = new System.Drawing.Point(12, 27);
            lblColFecha.Text     = "Fecha:";
            txtColFecha.Location = new System.Drawing.Point(128, 24);
            txtColFecha.Size     = new System.Drawing.Size(150, 24);
            txtColFecha.TabIndex = 0;

            lblColAsiento.Location = new System.Drawing.Point(296, 27);
            lblColAsiento.Text     = "Asiento:";
            txtColAsiento.Location = new System.Drawing.Point(360, 24);
            txtColAsiento.Size     = new System.Drawing.Size(150, 24);
            txtColAsiento.TabIndex = 1;

            lblColConcepto.Location = new System.Drawing.Point(12, 57);
            lblColConcepto.Text     = "Concepto:";
            txtColConcepto.Location = new System.Drawing.Point(128, 54);
            txtColConcepto.Size     = new System.Drawing.Size(150, 24);
            txtColConcepto.TabIndex = 2;

            lblColDebe.Location = new System.Drawing.Point(296, 57);
            lblColDebe.Text     = "Debe:";
            txtColDebe.Location = new System.Drawing.Point(360, 54);
            txtColDebe.Size     = new System.Drawing.Size(150, 24);
            txtColDebe.TabIndex = 3;

            lblColHaber.Location = new System.Drawing.Point(528, 57);
            lblColHaber.Text     = "Haber:";
            txtColHaber.Location = new System.Drawing.Point(580, 54);
            txtColHaber.Size     = new System.Drawing.Size(140, 24);
            txtColHaber.TabIndex = 4;

            lblAyudaConcepto.Location  = new System.Drawing.Point(12, 86);
            lblAyudaConcepto.ForeColor = System.Drawing.Color.DimGray;
            lblAyudaConcepto.Text      = "El concepto trae tipo, número y proveedor juntos: «SEGUN FACTURA A    36900627623 de CIA INDUSTRIAL C».";

            grpColumnas.Controls.AddRange(new System.Windows.Forms.Control[]
            {
                lblColFecha, txtColFecha, lblColAsiento, txtColAsiento, lblColConcepto, txtColConcepto,
                lblColDebe, txtColDebe, lblColHaber, txtColHaber, lblAyudaConcepto
            });

            // ── grpCuentas ───────────────────────────────────────────────────────
            grpCuentas.Location = new System.Drawing.Point(12, 266);
            grpCuentas.Size     = new System.Drawing.Size(736, 190);
            grpCuentas.Text     = "Cuentas de percepciones y retenciones";
            grpCuentas.TabIndex = 3;

            gridCuentas.Location = new System.Drawing.Point(12, 22);
            gridCuentas.Size     = new System.Drawing.Size(592, 156);
            gridCuentas.TabIndex = 0;
            gridCuentas.MasterTemplate.AllowAddNewRow   = false;
            gridCuentas.MasterTemplate.AllowDeleteRow   = false;
            gridCuentas.MasterTemplate.AllowEditRow     = false;
            gridCuentas.MasterTemplate.ShowRowHeaderColumn = false;
            gridCuentas.MasterTemplate.AutoSizeColumnsMode = Telerik.WinControls.UI.GridViewAutoSizeColumnsMode.Fill;
            gridCuentas.CellDoubleClick += GridCuentas_CellDoubleClick;
            gridCuentas.RowFormatting   += GridCuentas_RowFormatting;

            // Estado vacío de la maqueta: el aviso tapa la grilla cuando no hay cuentas.
            lblSinCuentas.AutoSize  = false;
            lblSinCuentas.Location  = new System.Drawing.Point(12, 22);
            lblSinCuentas.Size      = new System.Drawing.Size(592, 156);
            lblSinCuentas.TextAlignment = System.Drawing.ContentAlignment.MiddleCenter;
            lblSinCuentas.ForeColor = System.Drawing.Color.Gray;
            lblSinCuentas.Font      = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Italic);
            lblSinCuentas.BackColor = System.Drawing.Color.White;
            lblSinCuentas.BorderVisible = true;
            lblSinCuentas.Text      = "Sin cuentas. Agregá al menos una para poder levantar los mayores de PRESEA.";

            btnAgregarCuenta.Location = new System.Drawing.Point(616, 22);
            btnAgregarCuenta.Size     = new System.Drawing.Size(108, 28);
            btnAgregarCuenta.Text     = "Agregar...";
            btnAgregarCuenta.TabIndex = 1;
            btnAgregarCuenta.Click   += BtnAgregarCuenta_Click;

            btnEditarCuenta.Location = new System.Drawing.Point(616, 58);
            btnEditarCuenta.Size     = new System.Drawing.Size(108, 28);
            btnEditarCuenta.Text     = "Editar...";
            btnEditarCuenta.TabIndex = 2;
            btnEditarCuenta.Click   += BtnEditarCuenta_Click;

            btnQuitarCuenta.Location = new System.Drawing.Point(616, 94);
            btnQuitarCuenta.Size     = new System.Drawing.Size(108, 28);
            btnQuitarCuenta.Text     = "Quitar";
            btnQuitarCuenta.TabIndex = 3;
            btnQuitarCuenta.Click   += BtnQuitarCuenta_Click;

            grpCuentas.Controls.AddRange(new System.Windows.Forms.Control[]
            {
                lblSinCuentas, gridCuentas, btnAgregarCuenta, btnEditarCuenta, btnQuitarCuenta
            });

            // ── Botones ──────────────────────────────────────────────────────────
            btnGuardar.Font     = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            btnGuardar.Location = new System.Drawing.Point(532, 466);
            btnGuardar.Size     = new System.Drawing.Size(104, 30);
            btnGuardar.Text     = "Guardar";
            btnGuardar.TabIndex = 4;
            btnGuardar.Click   += BtnGuardar_Click;

            btnCancelar.Location = new System.Drawing.Point(644, 466);
            btnCancelar.Size     = new System.Drawing.Size(104, 30);
            btnCancelar.Text     = "Cancelar";
            btnCancelar.TabIndex = 5;
            btnCancelar.Click   += BtnCancelar_Click;

            // ── Form ─────────────────────────────────────────────────────────────
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode       = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize          = new System.Drawing.Size(760, 508);
            Controls.AddRange(new System.Windows.Forms.Control[]
            {
                lblNombre, txtNombre, grpArchivo, grpColumnas, grpCuentas, btnGuardar, btnCancelar
            });
            AcceptButton    = null;
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            MaximizeBox     = false;
            MinimizeBox     = false;
            Name            = "FormPerfilPyRDetalle";
            StartPosition   = System.Windows.Forms.FormStartPosition.CenterParent;
            Text            = "Perfil PyR — Detalle";

            grpArchivo.ResumeLayout(false);
            grpColumnas.ResumeLayout(false);
            grpCuentas.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)gridCuentas).EndInit();
            ((System.ComponentModel.ISupportInitialize)this).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        private Telerik.WinControls.UI.RadLabel        lblNombre;
        private Telerik.WinControls.UI.RadTextBox      txtNombre;
        private System.Windows.Forms.GroupBox          grpArchivo;
        private Telerik.WinControls.UI.RadLabel        lblTipoArchivo;
        private Telerik.WinControls.UI.RadDropDownList cmbTipoArchivo;
        private Telerik.WinControls.UI.RadLabel        lblHoja;
        private Telerik.WinControls.UI.RadTextBox      txtHojaExcel;
        private Telerik.WinControls.UI.RadCheckBox     chkTieneCabecera;
        private Telerik.WinControls.UI.RadLabel        lblFormatoFecha;
        private Telerik.WinControls.UI.RadDropDownList cmbFormatoFecha;
        private Telerik.WinControls.UI.RadLabel        lblDecimal;
        private Telerik.WinControls.UI.RadDropDownList cmbSeparadorDecimal;
        private System.Windows.Forms.GroupBox          grpColumnas;
        private Telerik.WinControls.UI.RadLabel        lblColFecha;
        private Telerik.WinControls.UI.RadTextBox      txtColFecha;
        private Telerik.WinControls.UI.RadLabel        lblColAsiento;
        private Telerik.WinControls.UI.RadTextBox      txtColAsiento;
        private Telerik.WinControls.UI.RadLabel        lblColConcepto;
        private Telerik.WinControls.UI.RadTextBox      txtColConcepto;
        private Telerik.WinControls.UI.RadLabel        lblColDebe;
        private Telerik.WinControls.UI.RadTextBox      txtColDebe;
        private Telerik.WinControls.UI.RadLabel        lblColHaber;
        private Telerik.WinControls.UI.RadTextBox      txtColHaber;
        private Telerik.WinControls.UI.RadLabel        lblAyudaConcepto;
        private System.Windows.Forms.GroupBox          grpCuentas;
        private Telerik.WinControls.UI.RadGridView     gridCuentas;
        private Telerik.WinControls.UI.RadLabel        lblSinCuentas;
        private Telerik.WinControls.UI.RadButton       btnAgregarCuenta;
        private Telerik.WinControls.UI.RadButton       btnEditarCuenta;
        private Telerik.WinControls.UI.RadButton       btnQuitarCuenta;
        private Telerik.WinControls.UI.RadButton       btnGuardar;
        private Telerik.WinControls.UI.RadButton       btnCancelar;
    }
}
