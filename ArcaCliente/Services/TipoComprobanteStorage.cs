using System.Collections.Generic;

namespace ArcaCliente.Services
{
    public class EquivalenciaTipoComprobante
    {
        public string CodigoAfip { get; set; }
        public string TipoSistema { get; set; }
        public string Letra { get; set; }
    }

    public static class TipoComprobanteStorage
    {
        public static void InitializeDatabase()
        {
            // Las tablas se crean en ArcaSqliteStorage.InitializeDatabase()
        }

        public static List<EquivalenciaTipoComprobante> LoadAll() =>
            ArcaSqliteStorage.LoadEquivalencias();

        public static void SaveAll(IEnumerable<EquivalenciaTipoComprobante> items) =>
            ArcaSqliteStorage.SaveEquivalencias(items);

        public static void SeedIfEmpty(Dictionary<string, (string Tipo, string Letra)> defaultMap)
        {
            var existing = LoadAll();
            if (existing.Count > 0) return;

            var list = new List<EquivalenciaTipoComprobante>();
            var todosLosCodigos = TablaComprobanteDescripcionMapper.ObtenerTodosLosCodigos();

            foreach (var codigo in todosLosCodigos)
            {
                string tipo = string.Empty;
                string letra = string.Empty;

                if (defaultMap.TryGetValue(codigo, out var def))
                {
                    tipo = def.Tipo;
                    letra = def.Letra;
                }

                list.Add(new EquivalenciaTipoComprobante
                {
                    CodigoAfip  = codigo,
                    TipoSistema = tipo,
                    Letra       = letra
                });
            }
            SaveAll(list);
        }
    }
}
