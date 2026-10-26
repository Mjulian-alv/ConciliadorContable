// Fecha: 26/10/2026 - TAREA: 00135 - Linea: 2 - Quién y por qué proceso se escribe en la base, para el historial de MovimientosArchivo
using System;
using System.Threading;

namespace AgrupadorConceptos.Data
{
    /// <summary>
    /// Contexto de auditoría de la operación en curso. <see cref="DatabaseHelper.Open"/> lo copia a SESSION_CONTEXT
    /// de la conexión y el trigger del historial lo lee, así cada fila del historial dice qué proceso la escribió
    /// sin pasar parámetros por todos los métodos de escritura.
    /// </summary>
    /// <remarks>
    /// El usuario no se puede deducir en la capa de datos: el escritorio lo fija una vez al ingresar
    /// (<see cref="UsuarioPredeterminado"/>) y la web lo fija por operación, porque ahí es el del circuito, no el del proceso.
    /// </remarks>
    public static class AuditoriaContexto
    {
        // Orígenes que se guardan en el historial: son parte del contrato con las pantallas de consulta.
        public const string Manual = "Manual";
        public const string Homologacion = "Homologación";
        public const string Rehomologacion = "Re-homologación";
        public const string BajaDeRegla = "Baja de regla";
        public const string ReapuntarRegla = "Reapuntar regla";
        public const string ConciliacionManual = "Conciliación manual";
        public const string ConciliacionInterna = "Conciliación interna";
        public const string ConciliacionExterna = "Conciliación externa";

        private static readonly AsyncLocal<(string Origen, string Usuario)> _actual = new();

        /// <summary>Usuario del proceso (escritorio) cuando la operación no informa uno propio. Null: se usa el login de SQL.</summary>
        public static string UsuarioPredeterminado { get; set; }

        public static string Origen => _actual.Value.Origen;
        public static string Usuario => _actual.Value.Usuario ?? UsuarioPredeterminado;

        /// <summary>
        /// Abre un ámbito con origen y/o usuario. Lo que no se informa se hereda del ámbito exterior, así la web fija el
        /// usuario una vez y cada servicio sólo agrega su origen.
        /// </summary>
        public static IDisposable Usar(string origen = null, string usuario = null)
        {
            var anterior = _actual.Value;
            _actual.Value = (origen ?? anterior.Origen, usuario ?? anterior.Usuario);
            return new Ambito(anterior);
        }

        private sealed class Ambito : IDisposable
        {
            private readonly (string Origen, string Usuario) _anterior;
            private bool _liberado;
            public Ambito((string Origen, string Usuario) anterior) => _anterior = anterior;
            public void Dispose()
            {
                if (_liberado) return;
                _liberado = true;
                _actual.Value = _anterior;
            }
        }
    }
}
