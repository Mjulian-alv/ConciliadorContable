using System.Collections.Generic;

namespace ArcaCliente.Models
{
    /// <summary>
    /// Paleta de colores por <see cref="EstadoConciliacion"/>, compartida entre la grilla
    /// (System.Drawing.Color) y el export a Excel (ClosedXML.XLColor). Antes cada lado tenía
    /// su propia copia de los mismos RGB (FormComprobantes, FormComprobantesOffline y
    /// ConciliacionExcelExporter): un cambio de paleta requería sincronizar 3 lugares a mano.
    /// Sin dependencia a System.Drawing ni ClosedXML a propósito, para que cualquiera de los
    /// dos lados pueda convertir el RGB al tipo de color que necesite.
    /// </summary>
    public static class EstadoConciliacionColores
    {
        public static readonly IReadOnlyDictionary<EstadoConciliacion, (byte R, byte G, byte B)> Rgb =
            new Dictionary<EstadoConciliacion, (byte R, byte G, byte B)>
            {
                [EstadoConciliacion.Conciliado]        = (170, 240, 209),
                [EstadoConciliacion.DiferenciaImporte] = (254, 240, 186),
                [EstadoConciliacion.SoloARCA]          = (255, 205, 210),
                [EstadoConciliacion.SoloSistema]       = (206, 212, 237),
            };
    }
}
