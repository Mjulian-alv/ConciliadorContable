using System;
using System.Collections.Generic;
using System.Linq;
using ArcaCliente.Models;

namespace ArcaCliente.Services
{
    /// <summary>
    /// Convierte el código de TipoComprobante que devuelve ARCA (portal AFIP)
    /// al tipo Octosis (1 = Factura, 2 = Nota de Débito, 3 = Nota de Crédito) y la letra (A/B/C/M/E).
    /// </summary>
    public static class TipoComprobanteMapper
    {
        // Las claves son los códigos de tipo de comprobante de AFIP (001, 002, 003, etc.).
        // El valor es (TipoOctosis, Letra).
        private static Dictionary<string, (string Tipo, string Letra)> _mapa;

        static TipoComprobanteMapper()
        {
            var _defaultMapa = new Dictionary<string, (string Tipo, string Letra)>(StringComparer.Ordinal)
            {
            // Facturas, Notas de Débito y Crédito A
            ["001"] = ("1", "A"),  // FACTURA A
            ["002"] = ("2", "A"),  // NOTA DE DEBITO A
            ["003"] = ("3", "A"),  // NOTA DE CREDITO A
            ["004"] = ("1", "A"),  // RECIBO A
            ["005"] = ("1", "A"),  // NOTAS DE VENTA AL CONTADO A

            // Facturas, Notas de Débito y Crédito B
            ["006"] = ("1", "B"),  // FACTURA B
            ["007"] = ("2", "B"),  // NOTA DE DEBITO B
            ["008"] = ("3", "B"),  // NOTA DE CREDITO B
            ["009"] = ("1", "B"),  // RECIBO B
            ["010"] = ("1", "B"),  // NOTAS DE VENTA AL CONTADO B

            // Facturas, Notas de Débito y Crédito C
            ["011"] = ("1", "C"),  // FACTURA C
            ["012"] = ("2", "C"),  // NOTA DE DEBITO C
            ["013"] = ("3", "C"),  // NOTA DE CREDITO C
            ["015"] = ("1", "C"),  // RECIBO C
            ["016"] = ("1", "C"),  // NOTAS DE VENTA AL CONTADO C

            // Liquidación de Servicios Públicos
            ["017"] = ("1", "A"),  // LIQUIDACION DE SERVICIOS PUBLICOS CLASE A
            ["018"] = ("1", "B"),  // LIQUIDACION DE SERVICIOS PUBLICOS CLASE B

            // Operaciones con el Exterior
            ["019"] = ("1", "E"),  // FACTURA POR OPERACIONES CON EL EXTERIOR
            ["020"] = ("2", "E"),  // NOTA DE DEBITO POR OPERACIONES CON EL EXTERIOR
            ["021"] = ("3", "E"),  // NOTA DE CREDITO POR OPERACIONES CON EL EXTERIOR
            ["022"] = ("1", "E"),  // FACTURA PERMISO EXPORTACION SIMPLIFICADO - DTO. 855/97

            // Sector Pesquero Marítimo
            ["023"] = ("1", "A"),  // COMPROBANTES CLASE "A" DE COMPRA PRIMARIA PARA EL SECTOR PESQUERO MARITIMO
            ["024"] = ("1", "A"),  // COMPROBANTES CLASE "A" DE CONSIGNACION PRIMARIA PARA EL SECTOR PESQUERO MARITIMO
            ["025"] = ("1", "B"),  // COMPROBANTES CLASE "B" DE COMPRA PRIMARIA PARA EL SECTOR PESQUERO MARITIMO
            ["026"] = ("1", "B"),  // COMPROBANTES CLASE "B" DE CONSIGNACION PRIMARIA PARA EL SECTOR PESQUERO MARITIMO

            // Liquidación Única Comercial Impositiva
            ["027"] = ("1", "A"),  // LIQUIDACION UNICA COMERCIAL IMPOSITIVA CLASE A
            ["028"] = ("1", "B"),  // LIQUIDACION UNICA COMERCIAL IMPOSITIVA CLASE B
            ["029"] = ("1", "C"),  // LIQUIDACION UNICA COMERCIAL IMPOSITIVA CLASE C
            ["043"] = ("3", "B"),  // NOTA DE CREDITO LIQUIDACION UNICA COMERCIAL IMPOSITIVA CLASE B
            ["044"] = ("3", "C"),  // NOTA DE CREDITO LIQUIDACION UNICA COMERCIAL IMPOSITIVA CLASE C
            ["045"] = ("2", "A"),  // NOTA DE DEBITO LIQUIDACION UNICA COMERCIAL IMPOSITIVA CLASE A
            ["046"] = ("2", "B"),  // NOTA DE DEBITO LIQUIDACION UNICA COMERCIAL IMPOSITIVA CLASE B
            ["047"] = ("2", "C"),  // NOTA DE DEBITO LIQUIDACION UNICA COMERCIAL IMPOSITIVA CLASE C
            ["048"] = ("3", "A"),  // NOTA DE CREDITO LIQUIDACION UNICA COMERCIAL IMPOSITIVA CLASE A

            // Compra de Bienes Usados y Reciclables
            ["030"] = ("1", "C"),  // COMPROBANTE DE COMPRA DE BIENES USADOS
            ["032"] = ("1", "C"),  // COMPROBANTE DE COMPRA DE MATERIALES A RECICLAR PROVENIENTE DE RESIDUOS
            ["049"] = ("1", "C"),  // COMPROBANTES DE COMPRA DE BIENES USADOS NO REGISTRABLES A CONSUMIDORES FINALES

            // Mandatos y Consignación
            ["031"] = ("1", "C"),  // MANDATO - CONSIGNACION

            // Liquidación de Granos
            ["033"] = ("1", "C"),  // LIQUIDACION PRIMARIA DE GRANOS
            ["331"] = ("1", "C"),  // LIQUIDACION SECUNDARIA DE GRANOS
            ["332"] = ("1", "C"),  // CERTIFICACION ELECTRONICA (GRANOS)

            // Comprobantes R.G. 1415
            ["034"] = ("1", "A"),  // COMPROBANTES A DEL ANEXO I, APARTADO A, INCISO F) R.G. N° 1415
            ["035"] = ("1", "B"),  // COMPROBANTES B DEL ANEXO I, APARTADO A, INC. F), R.G. N° 1415
            ["036"] = ("1", "C"),  // COMPROBANTES C DEL ANEXO I, APARTADO A, INC.F), R.G. N° 1415
            ["037"] = ("2", "C"),  // NOTAS DE DEBITO O DOCUMENTOS EQUIVALENTES QUE CUMPLAN CON LA R.G. N° 1415
            ["038"] = ("3", "C"),  // NOTAS DE CREDITO O DOCUMENTOS EQUIVALENTES QUE CUMPLAN CON LA R.G. N° 1415
            ["039"] = ("1", "A"),  // OTROS COMPROBANTES A QUE CUMPLEN CON LA R G N° 1415
            ["040"] = ("1", "B"),  // OTROS COMPROBANTES B QUE CUMPLAN CON LA R.G. N° 1415
            ["041"] = ("1", "C"),  // OTROS COMPROBANTES C QUE CUMPLAN CON LA R.G. N° 1415

            // Comprobantes con Leyenda "Operación Sujeta a Retención"
            ["051"] = ("1", "A"),  // FACTURA A CON LEYENDA "OPERACIÓN SUJETA A RETENCIÓN"
            ["052"] = ("2", "A"),  // NOTA DE DEBITO A CON LEYENDA "OPERACIÓN SUJETA A RETENCIÓN"
            ["053"] = ("3", "A"),  // NOTA DE CREDITO A CON LEYENDA "OPERACIÓN SUJETA A RETENCIÓN"
            ["054"] = ("1", "A"),  // RECIBO A CON LEYENDA "OPERACIÓN SUJETA A RETENCIÓN"
            ["055"] = ("1", "A"),  // NOTAS DE VENTA AL CONTADO A CON LEYENDA "OPERACIÓN SUJETA A RETENCIÓN"
            ["056"] = ("1", "A"),  // COMPROBANTES A CON LEYENDA "OPERACIÓN SUJETA A RETENCIÓN" DEL ANEXO I, APARTADO A, INC. F), R.G. N° 1.415
            ["057"] = ("1", "A"),  // OTROS COMPROBANTES A CON LEYENDA "OPERACIÓN SUJETA A RETENCIÓN"QUE CUMPLAN CON LA R.G. N° 1.415
            ["058"] = ("1", "A"),  // CUENTA DE VENTA Y LIQUIDO PRODUCTO A CON LEYENDA "OPERACIÓN SUJETA A RETENCIÓN"
            ["059"] = ("1", "A"),  // LIQUIDACION A CON LEYENDA "OPERACIÓN SUJETA A RETENCIÓN"

            // Cuentas de Venta y Líquido Producto
            ["060"] = ("1", "A"),  // CUENTAS DE VENTA Y LIQUIDO PRODUCTO A
            ["061"] = ("1", "B"),  // CUENTAS DE VENTA Y LIQUIDO PRODUCTO B

            // Liquidaciones
            ["063"] = ("1", "A"),  // LIQUIDACIONES A
            ["064"] = ("1", "B"),  // LIQUIDACIONES B

            // Otros Documentos
            ["066"] = ("1", "E"),  // DESPACHO DE IMPORTACION
            ["088"] = ("1", "C"),  // REMITO ELECTRONICO
            ["089"] = ("1", "C"),  // RESUMEN DE DATOS
            ["090"] = ("3", "C"),  // OTROS COMPROBANTES - DOCUMENTOS EXCEPTUADOS - NOTAS DE CREDITO
            ["091"] = ("1", "R"),  // REMITOS R
            ["099"] = ("1", "C"),  // OTROS COMPROBANTES QUE NO CUMPLEN O ESTÁN EXCEPTUADOS DE LA R.G. 1415 Y SUS MODIF

            // Controladores Fiscales (Tickets)
            ["080"] = ("1", "C"),  // INFORME DIARIO DE CIERRE (ZETA) - CONTROLADORES FISCALES
            ["081"] = ("1", "A"),  // TIQUE FACTURA A
            ["082"] = ("1", "B"),  // TIQUE FACTURA B
            ["083"] = ("1", "C"),  // TIQUE
            ["109"] = ("1", "C"),  // TIQUE C
            ["110"] = ("3", "C"),  // TIQUE NOTA DE CREDITO
            ["111"] = ("1", "C"),  // TIQUE FACTURA C
            ["112"] = ("3", "A"),  // TIQUE NOTA DE CREDITO A
            ["113"] = ("3", "B"),  // TIQUE NOTA DE CREDITO B
            ["114"] = ("3", "C"),  // TIQUE NOTA DE CREDITO C
            ["115"] = ("2", "A"),  // TIQUE NOTA DE DEBITO A
            ["116"] = ("2", "B"),  // TIQUE NOTA DE DEBITO B
            ["117"] = ("2", "C"),  // TIQUE NOTA DE DEBITO C

            // FCE MiPyMEs (Ley 27.440)
            ["201"] = ("1", "A"),  // FACTURA DE CRÉDITO ELECTRÓNICA MiPyMEs (FCE) A
            ["202"] = ("2", "A"),  // NOTA DE DEBITO ELECTRÓNICA MiPyMEs (FCE) A
            ["203"] = ("3", "A"),  // NOTA DE CREDITO ELECTRÓNICA MiPyMEs (FCE) A
            ["206"] = ("1", "B"),  // FACTURA DE CRÉDITO ELECTRÓNICA MiPyMEs (FCE) B
            ["207"] = ("2", "B"),  // NOTA DE DEBITO ELECTRÓNICA MiPyMEs (FCE) B
            ["208"] = ("3", "B"),  // NOTA DE CREDITO ELECTRÓNICA MiPyMEs (FCE) B
            ["211"] = ("1", "C"),  // FACTURA DE CRÉDITO ELECTRÓNICA MiPyMEs (FCE) C
            ["212"] = ("2", "C"),  // NOTA DE DEBITO ELECTRÓNICA MiPyMEs (FCE) C
            ["213"] = ("3", "C"),  // NOTA DE CREDITO ELECTRÓNICA MiPyMEs (FCE) C

            // Remitos Electrónicos Sectoriales
            ["991"] = ("1", "C"),  // REMITO ELECTRÓNICO DE TABACO EN HEBRA
            ["992"] = ("1", "C"),  // RESUMEN DE DATOS DE TABACO EN HEBRA
            ["993"] = ("1", "C"),  // REMITO ELECTRÓNICO HARINERO - AUTOMOTOR
            ["994"] = ("1", "C"),  // REMITO ELECTRÓNICO HARINERO - FERROVIARIO
            ["995"] = ("1", "C"),  // REMITO ELECTRÓNICO CÁRNICO
            ["997"] = ("1", "C"),  // Remito Electrónico para Azúcar, Alcohol y Subproductos -Mercado Interno-
            ["998"] = ("1", "E"),  // Remito Electrónico para Azúcar, Alcohol y Subproductos -Exportación-
        };

            TipoComprobanteStorage.SeedIfEmpty(_defaultMapa);
            Reload();
        }

        public static void Reload()
        {
            var list = TipoComprobanteStorage.LoadAll();
            var newMap = new Dictionary<string, (string Tipo, string Letra)>(StringComparer.Ordinal);
            foreach (var item in list)
            {
                newMap[item.CodigoAfip] = (item.TipoSistema, item.Letra);
            }
            _mapa = newMap;
        }

        /// <summary>
        /// Parsea el código de TipoComprobante de ARCA y retorna el tipo Octosis y la letra.
        /// </summary>
        /// <param name="tipoComprobanteArca">Código del tipo de comprobante (001, 002, 003, etc.).</param>
        public static TipoComprobanteParseado Parse(string tipoComprobanteArca)
        {
            if (string.IsNullOrWhiteSpace(tipoComprobanteArca))
                return new TipoComprobanteParseado(string.Empty, string.Empty, false);

            var codigo = tipoComprobanteArca.Trim();

            if (_mapa.TryGetValue(codigo, out var encontrado))
                return new TipoComprobanteParseado(encontrado.Tipo, encontrado.Letra, true);

            return new TipoComprobanteParseado(string.Empty, string.Empty, false);
        }

        /// <summary>
        /// Devuelve los códigos de TipoComprobante de una colección que no pudieron reconocerse.
        /// Útil para detectar tipos nuevos durante el testing y actualizar el diccionario.
        /// </summary>
        public static IEnumerable<string> ObtenerDesconocidos(IEnumerable<string> codigos)
            => codigos
               .Where(c => !string.IsNullOrWhiteSpace(c))
               .Where(c => !Parse(c).Reconocido)
               .Distinct(StringComparer.OrdinalIgnoreCase);
    }
}
