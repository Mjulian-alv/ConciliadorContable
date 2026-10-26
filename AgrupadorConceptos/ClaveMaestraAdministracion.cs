// Fecha: 26/10/2026 - TAREA: 00135 - Linea: 3 - Administración de la clave maestra del blindaje desde el ABM de usuarios del escritorio
using System;
using System.Windows.Forms;
using AgrupadorConceptos.Data;
using AgrupadorConceptos.Services;

namespace AgrupadorConceptos
{
    /// <summary>
    /// Punto de entrada público para definir o cambiar la clave maestra. La pantalla vive en el ABM de usuarios (otro ensamblado) y no en el
    /// procesador de archivos: quien procesa archivos blinda y desblinda, pero no administra la clave.
    /// </summary>
    public static class ClaveMaestraAdministracion
    {
        /// <summary>Texto corto del estado de la clave, para el botón del ABM.</summary>
        public static string Estado()
        {
            try
            {
                var reg = ClaveMaestraStorage.Obtener();
                return reg == null ? "sin definir" : $"definida el {reg.FechaCambio:dd/MM/yyyy}";
            }
            catch (Exception) { return "sin conexión"; }
        }

        /// <summary>Abre el diálogo para definirla (primera vez) o cambiarla (pide la actual). True si se guardó.</summary>
        public static bool Administrar(IWin32Window owner)
        {
            if (!BlindajeService.PuedeAdministrar)
            {
                MessageBox.Show("Tu usuario no tiene permiso para administrar la clave maestra.", "Clave maestra", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            try
            {
                bool existe = BlindajeService.HayClaveMaestra();
                if (!BlindajeDialogos.ClaveMaestra(owner, existe)) return false;
                MessageBox.Show(existe ? "Clave maestra cambiada." : "Clave maestra definida. Ya se pueden blindar archivos.", "Clave maestra", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return true;
            }
            catch (Microsoft.Data.SqlClient.SqlException ex)
            {
                MessageBox.Show("No se pudo acceder a la clave maestra: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }
    }
}
