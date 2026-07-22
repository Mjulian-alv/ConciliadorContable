using System.Runtime.CompilerServices;

namespace AgrupadorConceptos
{
    internal static class ModuleInit
    {
        /// <summary>
        /// Corre al cargar el assembly. Requerido por ExcelDataReader en .NET 5+
        /// para leer archivos con encodings ANSI. Vive acá y no en el host:
        /// el módulo debe funcionar con cualquier exe que lo referencie.
        /// </summary>
        [ModuleInitializer]
        internal static void Init()
        {
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
        }
    }
}
