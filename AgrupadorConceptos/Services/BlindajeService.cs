// Fecha: 26/10/2026 - TAREA: 00135 - Linea: 3 - Blindar y desblindar archivos con la clave maestra
using System;
using System.Security.Cryptography;
using System.Text;
using AgrupadorConceptos.Data;

namespace AgrupadorConceptos.Services
{
    /// <summary>
    /// Reglas del blindaje que no son de una tabla: la clave maestra (distinta de la de cualquier usuario), el bloqueo por
    /// intentos y el registro de quién blindó y desblindó. Las guardias de escritura viven en <see cref="BlindajeStorage"/>.
    /// </summary>
    public static class BlindajeService
    {
        // Subir las iteraciones sólo afecta a las claves nuevas: cada registro guarda las que usó.
        private const int IteracionesNuevas = 210_000;
        public const int MaxIntentos = 5;
        public const int MinutosBloqueo = 5;
        public const int LargoMinimoClave = 8;

        // Fecha: 26/10/2026 - TAREA: 00135 - Linea: 3 - Revisión 1, hallazgo 2: definir o cambiar la clave falla cerrado
        // Antes, un anfitrión que no configuraba nada podía administrar la clave, y el servicio ni consultaba el permiso.
        /// <summary>
        /// Quién puede definir o cambiar la clave maestra. La capa de datos no conoce a los usuarios: un anfitrión de un solo
        /// usuario (escritorio) lo fija al ingresar; un anfitrión de varios (web) autoriza cada operación con
        /// <see cref="AutorizarAdministracionClave"/> después de comprobar el permiso. Sin ninguna de las dos, se rechaza.
        /// </summary>
        public static Func<bool> PuedeAdministrarClave { get; set; }

        private static readonly System.Threading.AsyncLocal<bool> _autorizado = new();

        /// <summary>Marca la operación en curso como autorizada a administrar la clave (el anfitrión ya comprobó el permiso).</summary>
        public static IDisposable AutorizarAdministracionClave()
        {
            bool anterior = _autorizado.Value;
            _autorizado.Value = true;
            return new Autorizacion(anterior);
        }

        private sealed class Autorizacion : IDisposable
        {
            private readonly bool _anterior; private bool _liberada;
            public Autorizacion(bool anterior) => _anterior = anterior;
            public void Dispose() { if (_liberada) return; _liberada = true; _autorizado.Value = _anterior; }
        }

        public static bool PuedeAdministrar => _autorizado.Value || (PuedeAdministrarClave?.Invoke() ?? false);

        private static void ExigirPermisoClave()
        {
            if (!PuedeAdministrar) throw new UnauthorizedAccessException("Tu usuario no tiene permiso para administrar la clave maestra.");
        }

        public static bool HayClaveMaestra() => ClaveMaestraStorage.Obtener() != null;

        public static bool EstaBlindado(int idArchivo) => BlindajeStorage.EstaBlindado(idArchivo);

        /// <summary>Define la clave por primera vez. Si ya existe hay que usar <see cref="CambiarClaveMaestra"/>.</summary>
        public static void DefinirClaveMaestra(string clave, string usuario)
        {
            ExigirPermisoClave();
            if (HayClaveMaestra()) throw new InvalidOperationException("Ya hay una clave maestra. Para cambiarla hace falta la clave actual.");
            Guardar(clave, usuario);
        }

        /// <summary>Cambia la clave. Pide la actual y cuenta los intentos fallidos igual que al desblindar.</summary>
        public static void CambiarClaveMaestra(string claveActual, string claveNueva, string usuario)
        {
            ExigirPermisoClave();
            Validar(claveActual);
            Guardar(claveNueva, usuario);
        }

        /// <summary>Blinda el archivo. Devuelve false si ya lo estaba.</summary>
        public static bool Blindar(int idArchivo, string usuario)
        {
            if (!HayClaveMaestra())
                // Fecha: 26/10/2026 - TAREA: 00135 - Linea: 3 - La clave maestra se define en el ABM de usuarios del escritorio, no en el procesador
                throw new InvalidOperationException("Todavía no hay clave maestra: sin ella nadie podría desblindar el archivo. Definila en Administración de usuarios (escritorio) → «Clave maestra del blindaje»; hace falta el permiso AgrClaveMaestra.");
            return BlindajeStorage.Blindar(idArchivo, usuario);
        }

        public static void Desblindar(int idArchivo, string clave, string motivo, string usuario)
        {
            if (string.IsNullOrWhiteSpace(motivo)) throw new ArgumentException("Indicá el motivo del desblindaje.", nameof(motivo));
            try { Validar(clave); }
            catch (ClaveMaestraIncorrectaException) { BlindajeStorage.RegistrarFallido(idArchivo, usuario); throw; }
            BlindajeStorage.Desblindar(idArchivo, usuario, motivo.Trim());
        }

        // ---- clave ----

        private static void Validar(string clave)
        {
            // Fecha: 26/10/2026 - TAREA: 00135 - Linea: 3 - Revisión 1, hallazgo 1: un intento a la vez en toda la base
            using var turno = ClaveMaestraStorage.BloquearVerificacion();
            var reg = ClaveMaestraStorage.Obtener() ?? throw new InvalidOperationException("Todavía no hay clave maestra definida.");
            if (reg.SegundosBloqueo > 0)
                throw new ClaveMaestraBloqueadaException(reg.SegundosBloqueo);
            // El hash se calcula siempre, aun con la clave vacía, para no revelar por tiempo si el dato era válido.
            byte[] calculado = Derivar(clave ?? "", reg.Sal, reg.Iteraciones);
            if (!CryptographicOperations.FixedTimeEquals(calculado, reg.Hash))
            {
                ClaveMaestraStorage.RegistrarFallo(MaxIntentos, MinutosBloqueo);
                var tras = ClaveMaestraStorage.Obtener();
                if (tras != null && tras.SegundosBloqueo > 0) throw new ClaveMaestraBloqueadaException(tras.SegundosBloqueo);
                throw new ClaveMaestraIncorrectaException(Math.Max(0, MaxIntentos - (tras?.IntentosFallidos ?? 0)));
            }
            ClaveMaestraStorage.ReiniciarIntentos();
        }

        private static void Guardar(string clave, string usuario)
        {
            if (string.IsNullOrWhiteSpace(clave) || clave.Length < LargoMinimoClave)
                throw new ArgumentException($"La clave maestra tiene que tener al menos {LargoMinimoClave} caracteres.", nameof(clave));
            byte[] sal = RandomNumberGenerator.GetBytes(32);
            ClaveMaestraStorage.Guardar(Derivar(clave, sal, IteracionesNuevas), sal, IteracionesNuevas, usuario);
        }

        private static byte[] Derivar(string clave, byte[] sal, int iteraciones) =>
            Rfc2898DeriveBytes.Pbkdf2(Encoding.UTF8.GetBytes(clave), sal, iteraciones, HashAlgorithmName.SHA256, 32);
    }

    public class ClaveMaestraIncorrectaException : InvalidOperationException
    {
        public int IntentosRestantes { get; }
        public ClaveMaestraIncorrectaException(int restantes)
            : base($"Clave maestra incorrecta. Intentos restantes: {restantes}.") => IntentosRestantes = restantes;
    }

    public class ClaveMaestraBloqueadaException : InvalidOperationException
    {
        public int Segundos { get; }
        public ClaveMaestraBloqueadaException(int segundos)
            : base($"Demasiados intentos fallidos. Probá de nuevo en {Math.Max(1, (segundos + 59) / 60)} minuto(s).") => Segundos = segundos;
    }
}
