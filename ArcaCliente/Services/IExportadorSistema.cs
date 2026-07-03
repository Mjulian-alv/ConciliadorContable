using System.Collections.Generic;
using ArcaCliente.Models;

namespace ArcaCliente.Services
{
    /// <summary>
    /// Contrato que debe implementar cada exportador de sistema contable externo.
    /// Agrega un nuevo sistema: crear una clase que implemente esta interfaz y
    /// registrarla en <see cref="ExportadorSistemaFactory"/>.
    /// </summary>
    public interface IExportadorSistema
    {
        /// <summary>Nombre legible del sistema destino (p.ej. "PRESEA").</summary>
        string NombreSistema { get; }

        /// <summary>
        /// Genera el archivo Excel de importación con los comprobantes indicados.
        /// </summary>
        /// <param name="comprobantes">Comprobantes ARCA a exportar.</param>
        /// <param name="rutaArchivo">Ruta completa del archivo .xlsx a crear.</param>
        void Exportar(List<ComprobanteCsv> comprobantes, string rutaArchivo);
    }
}
