using System;
using System.Linq;
using System.Windows.Forms;
using Dapper;
using AgrupadorConceptos.Models;
using AgrupadorConceptos.Data;

namespace AgrupadorConceptos
{
    public partial class HomologarForm : Form
    {
        private int _idPerfilBanco;
        private string _valorOriginal;

        public bool HomologacionExitosa { get; private set; } = false;
        public string sConcepto { get; private set; } = "";
        public HomologarForm(int idPerfilBanco, string valorOriginal)
        {
            InitializeComponent();
            this.Icon = AppIcon.GetIcon();
            _idPerfilBanco = idPerfilBanco;
            _valorOriginal = valorOriginal;
            
            txtOriginal.Text = _valorOriginal;
            CargarConceptosEstandar();
        }

        private void CargarConceptosEstandar()
        {
            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                var conceptos = connection.Query<ConceptoEstandar>("SELECT * FROM bancos.ConceptosEstandar ORDER BY Nombre").ToList();
                
                cmbEstandar.DataSource = conceptos;
                cmbEstandar.DisplayMember = "Nombre";
                cmbEstandar.ValueMember = "Id";
            }
        }

        private void btnGuardar_Click(object sender, EventArgs e)
        {
            string conceptoEstandarTexto = cmbEstandar.Text.Trim();

            if (string.IsNullOrEmpty(conceptoEstandarTexto))
            {
                MessageBox.Show("Debe ingresar o seleccionar un Concepto Estándar.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                using (var connection = DatabaseHelper.GetConnection())
                {
                    connection.Open();

                    // Buscar o crear el concepto estandar
                    var conceptoExistente = connection.QueryFirstOrDefault<ConceptoEstandar>(
                        "SELECT * FROM bancos.ConceptosEstandar WHERE LOWER(Nombre) = LOWER(@Nombre)", new { Nombre = conceptoEstandarTexto });

                    int idConceptoEstandar;
                    if (conceptoExistente != null)
                    {
                        idConceptoEstandar = conceptoExistente.Id;
                    }
                    else
                    {
                        // Insertar nuevo concepto estandar
                        idConceptoEstandar = connection.QuerySingle<int>(
                            "INSERT INTO bancos.ConceptosEstandar (Nombre) VALUES (@Nombre); SELECT CAST(SCOPE_IDENTITY() AS INT);",
                            new { Nombre = conceptoEstandarTexto });
                    }

                    // Guardar homolagacion
                    // Insertamos el mapeo (Si es texto largo, el usuario tal vez modificó txtOriginal para dejar solo la palabra clave)
                    string valorClave = txtOriginal.Text.Trim();
                    
                    // Borrar el anterior para esta clave si existe
                    connection.Execute(
                        "DELETE FROM bancos.HomologacionConceptos WHERE IdPerfilBanco = @IdPerfilBanco AND ValorOriginal = @ValorOriginal",
                        new { IdPerfilBanco = _idPerfilBanco, ValorOriginal = valorClave });

                    // Insertar la nueva homologación
                    connection.Execute(@"
                        INSERT INTO bancos.HomologacionConceptos (IdPerfilBanco, ValorOriginal, IdConceptoEstandar)
                        VALUES (@IdPerfilBanco, @ValorOriginal, @IdConceptoEstandar)",
                        new { IdPerfilBanco = _idPerfilBanco, ValorOriginal = valorClave, IdConceptoEstandar = idConceptoEstandar });

                    HomologacionExitosa = true;
                    sConcepto = conceptoEstandarTexto;
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar homologación: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}