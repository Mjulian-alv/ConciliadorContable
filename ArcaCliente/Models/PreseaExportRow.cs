using System.ComponentModel;

namespace ArcaCliente.Models
{
    /// <summary>
    /// Fila de la grilla de seleccion por QR (Fase 3). Envuelve un
    /// <see cref="ItemConciliacion"/> SOLO ARCA con su estado de seleccion y de exportacion.
    /// Implementa <see cref="INotifyPropertyChanged"/> para que el tilde se refleje en la
    /// grilla cuando se marca por escaneo.
    /// </summary>
    public class PreseaExportRow : INotifyPropertyChanged
    {
        private bool _seleccionado;

        /// <summary>Marca de exportacion (editable por checkbox o por escaneo).</summary>
        public bool Seleccionado
        {
            get => _seleccionado;
            set
            {
                if (_seleccionado == value) return;
                _seleccionado = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Seleccionado)));
            }
        }

        /// <summary>"Nuevo" o "Ya exportado" segun la memoria anti-duplicado.</summary>
        public string EstadoExport { get; set; }

        public string Emisor { get; set; }
        public string Cuit   { get; set; }
        public string Tipo   { get; set; }
        public string Numero { get; set; }
        public string Fecha  { get; set; }
        public string Total  { get; set; }

        /// <summary>Clave normalizada de identidad (matchea con el QR).</summary>
        public string Clave { get; set; }

        /// <summary>True si ya figura en la memoria de exportados.</summary>
        [Browsable(false)]
        public bool YaExportado { get; set; }

        /// <summary>Item original SOLO ARCA (se pasa a la fase de completado).</summary>
        [Browsable(false)]
        public ItemConciliacion Item { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
