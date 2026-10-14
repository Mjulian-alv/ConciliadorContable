# -*- coding: utf-8 -*-
# TAREA 00097 - Líneas 3 a 7: homologaciones, consulta, conciliaciones, cuentas y formato de importes.
from maq_base import *

CRUMB = "Agrupador / "


def combo_perfil(extra=""):
    return ('<div class="context-strip"><div><span class="eyebrow">PERFIL</span><select style="width:260px"><option>BNA CA</option></select>' + extra + '</div></div>')


# ---------------------------------------------------------------- Línea 3
def m03():
    acc = boton("Nueva", True) + boton("Editar") + boton("Eliminar") + boton("Conceptos estándar…") + boton("Consultar movimientos…")
    reglas = [["PAGO CON TRANSF-BANE", "Texto contiene", "TRANSFERENCIAS RECIBIDAS", "110201", "7.436"],
              ["DB.TRANSF.INTER-LINK", "Texto contiene", "TRANSF ANTONIAZZI", "110105", "1.204"],
              ["DEB. TRANSF.ELE-SUC", "Texto contiene", "TRANSF ANTONIAZZI", "110105", "311"],
              ["GRAVAMEN LEY 25413", "Texto contiene", "IMPUESTO A LOS DÉBITOS Y CRÉDITOS", "520115", "962"],
              ["COM.P/DB.TRANSF", "Texto contiene", "COMISIONES BANCARIAS", "520118", "540"],
              ["I.V.A. BASE", "Texto contiene", "IVA SOBRE COMISIONES", "520120", "498"],
              ["DEVOL. PAGO CON-LINK", "Texto contiene", "DEVOLUCIONES DE COBROS", "410330", "88"]]
    cuerpo = (heading("AGRUPADOR DE CONCEPTOS", "Gestión de homologaciones", "Reglas que traducen el texto del banco a un concepto estándar.")
              + combo_perfil() + '<div style="height:16px"></div>'
              + panel('<div class="actions">' + acc + '</div><div style="height:12px"></div>'
                      + tabla(["Texto del banco", "Coincidencia", "Concepto estándar", "Cuenta", "Movimientos alcanzados"], reglas, numericas=(4,), clases=["", "sel", "", "", "", "", ""])
                      + '<p class="note">7 reglas · «Movimientos alcanzados» cuenta los movimientos ya homologados con la regla, en todos los archivos del perfil.</p>'))
    escribir("03-homologaciones", pagina("Gestión de homologaciones", "homolog", cuerpo, crumb=CRUMB + "Homologaciones"), h=900)

    vacio = (heading("AGRUPADOR DE CONCEPTOS", "Gestión de homologaciones") + combo_perfil() + '<div style="height:16px"></div>'
             + panel('<div class="actions">' + boton("Nueva", True) + boton("Editar", off=True) + boton("Eliminar", off=True) + boton("Conceptos estándar…") + '</div>'
                     '<div class="empty" style="text-align:center"><div style="font-size:36px;color:#9ab0b8">≡</div><h2>Este perfil todavía no tiene reglas</h2>'
                     '<p class="muted">Se crean desde el Procesador, homologando una fila, o con «Nueva».</p></div>'))
    escribir("03-homologaciones-vacio", pagina("Gestión de homologaciones", "homolog", vacio, crumb=CRUMB + "Homologaciones"), h=700)

    desglose = tabla(["Archivo", "Perfil", "Movimientos homologados con esta regla"],
                     [["BNA CA 05-2026.xlsx", "BNA CA", "1.402"], ["BNA CA 06-2026.xlsx", "BNA CA", "1.755"], ["BNA CA 07-2026.xlsx", "BNA CA", "1.801"],
                      ["BNA CA 08-2026.xlsx", "BNA CA", "1.571"], ["BNA 09-2026 A MANO MERI.xlsx", "BNA CA", "907"]], numericas=(2,))
    cuerpo_m = (aviso("Se va a eliminar la regla <strong>PAGO CON TRANSF-BANE → TRANSFERENCIAS RECIBIDAS</strong>. Hoy alcanza a <strong>7.436 movimientos</strong> ya homologados.")
                + desglose + '<div style="height:14px"></div>'
                + '<label class="check"><input type="radio" name="b" checked> Dejarlos como «Pendiente de homologar»</label>'
                + '<label class="check" style="margin-top:6px"><input type="radio" name="b"> Reasignarlos a otro concepto:</label>'
                + '<div style="max-width:360px;margin-top:8px"><select><option>Elegí un concepto estándar…</option></select></div>')
    m = modal("Eliminar homologación", "Impacto calculado en este momento sobre los archivos ya importados", cuerpo_m, boton("Cancelar") + boton("Confirmar baja", True))
    fondo = (heading("AGRUPADOR DE CONCEPTOS", "Gestión de homologaciones") + combo_perfil()
             + panel(tabla(["Texto del banco", "Coincidencia", "Concepto estándar", "Cuenta", "Movimientos alcanzados"], reglas, numericas=(4,))))
    escribir("03-baja-impacto", pagina("Gestión de homologaciones", "homolog", fondo, crumb=CRUMB + "Homologaciones", modal=m), h=900)

    # consulta de movimientos
    conceptos = ["ACREDITACIONES TARJETAS", "COMISIONES BANCARIAS", "DEVOLUCIONES DE COBROS", "IMPUESTO A LOS DÉBITOS Y CRÉDITOS", "IVA SOBRE COMISIONES",
                 "TRANSF ANTONIAZZI", "TRANSFERENCIAS RECIBIDAS", "TRANSFERENCIAS PROPIAS", "SERVICIOS PÚBLICOS", "SUELDOS Y JORNALES", "ANTICIPO DE SUELDOS", "ARCA - IVA"]
    marcados = {1, 5, 6}
    lista = '<div class="cols">' + "".join(f'<label><input type="checkbox"{" checked" if i in marcados else ""}> {c}</label>' for i, c in enumerate(conceptos)) + '</div>'
    filtros = ('<div class="form-row"><label style="max-width:190px">Desde<input value="01/09/2026" readonly></label><label style="max-width:190px">Hasta<input value="30/09/2026" readonly></label>'
               + boton("Consultar", True) + boton("Exportar a Excel") + '</div><div style="height:14px"></div>'
               '<div class="actions"><strong style="font-size:13px">Conceptos finales</strong>' + boton("Marcar todos") + boton("Desmarcar todos") + '</div><div style="height:10px"></div>' + lista
               + '<p class="note">Rango inclusivo por fecha del movimiento.</p>')
    rows, cl = [], []
    for (f, c, e, d, cr, cf, cta) in MOV[:9]:
        if not cf:
            continue
        rows.append([f, c, c, monto(d), monto(cr), e, cf, cta, "BNA CA"])
        cl.append("")
    t = tabla(["Fecha", "Concepto original", "Descripción", "Débitos", "Créditos", "Concepto estándar", "Concepto final", "Cuenta final", "Perfil"], rows, numericas=(3, 4))
    stats = ('<div class="stats"><div>Movimientos<strong>36.721</strong></div><div>Débitos<strong>' + fmt(269576305.44) + '</strong></div>'
             '<div>Créditos<strong>' + fmt(25614706918.23) + '</strong></div><div>Diferencia (C − D)<strong>' + dif(25345130612.79) + '</strong></div></div>')
    cuerpo = (heading("AGRUPADOR DE CONCEPTOS", "Consulta de movimientos", "Movimientos por rango de fechas y conceptos, de todos los perfiles y archivos.")
              + panel(filtros, "Filtros", "", 1) + panel(stats + t + '<div class="pager">Mostrando 8 de 36.721 filas · desplazá para ver más</div>', "Resultado", "", 2))
    escribir("03-consulta-movimientos", pagina("Consulta de movimientos", "consulta", cuerpo, crumb=CRUMB + "Consulta de movimientos"), h=1420)

    v = (heading("AGRUPADOR DE CONCEPTOS", "Consulta de movimientos") + panel(filtros, "Filtros", "", 1)
         + panel('<div class="empty" style="text-align:center"><div style="font-size:36px;color:#9ab0b8">⌕</div><h2>Sin resultados para esos filtros</h2>'
                 '<p class="muted">Probá con otro rango de fechas o con más conceptos. La consulta sólo se ejecuta al tocar «Consultar».</p></div>', "Resultado", "", 2))
    escribir("03-consulta-movimientos-vacio", pagina("Consulta de movimientos", "consulta", v, crumb=CRUMB + "Consulta de movimientos"), h=1000)

    e = (heading("AGRUPADOR DE CONCEPTOS", "Consulta de movimientos")
         + aviso("«Hasta» no puede ser anterior a «Desde». Elegí además al menos un concepto.", True)
         + panel(filtros.replace('<input value="30/09/2026" readonly>', '<input class="err" value="01/08/2026" readonly><div class="ferr">Debe ser igual o posterior a «Desde»</div>'), "Filtros", "", 1))
    escribir("03-consulta-movimientos-error", pagina("Consulta de movimientos", "consulta", e, crumb=CRUMB + "Consulta de movimientos"), h=900)


# ---------------------------------------------------------------- Línea 4
def m04():
    ses = tabla(["Sesión", "Estado", "Extractos", "Registros externos", "Pares", "Creada", ""],
                [["Proveedores 09-2026", '<span class="badge warn">En proceso</span>', "BNA CA 09-2026 · MACRO 09-2026", "1.992", "1.204", "03/10/2026", boton("▶ Retomar") + boton("🗑 Eliminar")],
                 ["Proveedores 08-2026", '<span class="badge warn">En proceso</span>', "BNA CA 08-2026 · +3", "1.846", "1.790", "12/09/2026", boton("▶ Retomar") + boton("🗑 Eliminar")],
                 ["Proveedores 07-2026", '<span class="badge ok">Finalizada</span>', "BNA CA 07-2026 · +4", "1.901", "1.901", "05/08/2026", boton("Ver") + boton("🗑 Eliminar")]],
                numericas=(3, 4))
    cuerpo = (heading("AGRUPADOR DE CONCEPTOS", "Conciliación externa", "Conciliá el extracto bancario contra un listado del ERP, por sesiones.")
              + panel('<div class="actions">' + boton("➕ Nueva sesión", True) + '</div><div style="height:12px"></div>' + ses, "Sesiones", "Una sesión puede incluir varios extractos."))
    escribir("04-conciliacion-externa-sesiones", pagina("Conciliación externa", "ext", cuerpo, crumb=CRUMB + "Conciliación externa"), h=760)

    def nueva(error=False):
        arch = tabla(["Elegir", "Perfil", "Archivo", "Desde", "Hasta", "Cantidad"],
                     [['<input type="checkbox">' if error else '<input type="checkbox" checked>', "BNA CA", ARCHIVO, "01/09/2026", "30/09/2026", "7.840"],
                      ['<input type="checkbox">' if error else '<input type="checkbox" checked>', "MACRO", "MACRO 09-2026.xlsx", "01/09/2026", "30/09/2026", "320"],
                      ['<input type="checkbox">', "COMAFI", "COMAFI 09-2026.xlsx", "01/09/2026", "30/09/2026", "43"]], numericas=(5,))
        ext = ('<div class="form-row"><label>Archivo externo (ERP)<input type="file"><small>' + ('Proveedores_09_2026.xlsx · 1.992 registros'
               if not error else 'Proveedores_09_2026.xlsx') + '</small></label></div>')
        if error:
            ext += aviso("El archivo externo tiene <strong>3 fechas vacías o inválidas</strong> (filas 12, 87 y 1.402). No se creó la sesión.", True)
        else:
            ext += '<p class="note">Rango sugerido por el archivo externo: 01/09/2026 al 30/09/2026.</p>'
        cons = '<div class="cols">' + "".join(f'<label><input type="checkbox"{" checked" if i in (0, 3, 6) else ""}> {c}</label>' for i, c in enumerate(
            ["TRANSFERENCIAS RECIBIDAS", "COMISIONES BANCARIAS", "ACREDITACIONES TARJETAS", "PAGO A PROVEEDORES", "SERVICIOS PÚBLICOS", "SUELDOS Y JORNALES"])) + '</div>'
        err = aviso("Seleccioná al menos un extracto para crear la sesión.", True) if error else ""
        return (heading("AGRUPADOR DE CONCEPTOS", "Nueva sesión de conciliación externa") + err
                + panel(campo("Nombre de la sesión", "Proveedores 09-2026"), "Nombre", "", 1)
                + panel('<div class="form-row"><label style="max-width:190px">Desde<input value="01/09/2026" readonly></label><label style="max-width:190px">Hasta<input value="30/09/2026" readonly></label>'
                        '<label class="check"><input type="checkbox"> Mostrar todos</label></div><p class="note">El rango filtra la lista de archivos; la sesión incluye los archivos elegidos completos.</p>' + arch,
                        "Extractos", "Elegí uno o varios extractos del rango.", 2)
                + panel(cons, "Conceptos a conciliar", "", 3) + panel(ext, "Archivo externo", "", 4)
                + '<div class="actions">' + boton("✔ Confirmar", True) + boton("✖ Cancelar") + '</div>')
    escribir("04-nueva-sesion", pagina("Nueva sesión", "ext", nueva(False), crumb=CRUMB + "Conciliación externa / Nueva"), h=1500)
    escribir("04-nueva-sesion-error", pagina("Nueva sesión", "ext", nueva(True), crumb=CRUMB + "Conciliación externa / Nueva"), h=1560)

    ext_rows = [["02/09/2026", "Cobranza tarjeta Visa 01/09", "100.000,00", '<span class="badge warn">Pendiente</span>'],
                ["02/09/2026", "Cobranza tarjeta Master 01/09", "84.512,30", '<span class="badge warn">Pendiente</span>'],
                ["03/09/2026", "Transferencia cliente 4411", "1.250.000,00", '<span class="badge warn">Pendiente</span>'],
                ["03/09/2026", "Cobranza Cabal 02/09", "36.480,00", '<span class="badge ok">Conciliado</span>']]
    mov = tabla(["", "Perfil", "Fecha", "Concepto original", "Concepto estándar", "Débitos", "Créditos", "Concepto final", "Cuenta final"],
                [['<input type="checkbox" checked>', "BNA CA", "02/09/2026", "PAGO CON TRANSF-BANE", "TRANSFERENCIAS RECIBIDAS", monto(0), monto(60000.00), "TRANSFERENCIAS RECIBIDAS", "110201"],
                 ['<input type="checkbox" checked>', "BNA CA", "02/09/2026", "PAGO CON TRANSF-BANE", "TRANSFERENCIAS RECIBIDAS", monto(0), monto(40000.00), "TRANSFERENCIAS RECIBIDAS", "110201"],
                 ['<input type="checkbox">', "MACRO", "02/09/2026", "CR. TARJ. MASTER", "ACREDITACIONES TARJETAS", monto(0), monto(84512.30), "ACREDITACIONES TARJETAS", "110203"],
                 ['<input type="checkbox">', "BNA CA", "03/09/2026", "CREDITO POR CONCEP.VARIOS", "Pendiente de homologar", monto(0), monto(1250000.00), "", ""]],
                numericas=(5, 6), clases=["sel", "sel", "", "pend"])
    izq = panel('<strong style="font-size:13px">Archivo externo — pendientes</strong><div style="height:8px"></div>' + tabla(["Fecha", "Detalle", "Importe", "Estado"], ext_rows, numericas=(2,), clases=["sel", "", "", ""]), "", "")
    der = panel('<div class="tabs"><button class="active">Con concepto seleccionado</button><button>Resto del extracto</button><button>✅ Conciliados (1.204)</button></div><div style="height:10px"></div>' + mov, "", "")
    acc = ('<div class="actions">' + boton("⚡ Auto-conciliar") + boton("Conciliar selección", True) + boton("↩ Desconciliar") + boton("🏁 Finalizar") + boton("📤 Exportar Excel") + '</div>')
    resumen = ('<div class="sum"><span>Registro externo seleccionado: <strong>' + fmt(100000.00) + '</strong></span><span>Movimientos seleccionados (2): <strong>' + fmt(100000.00) + '</strong></span>'
               '<span>Diferencia: <strong class="ok">' + fmt(0) + ' ✔ exacta</strong></span><span class="muted">Sólo se concilia si la suma coincide.</span></div>')
    ctx = ('<div class="context-strip"><div><span class="eyebrow">SESIÓN ACTIVA</span><strong>Proveedores 09-2026</strong><span class="badge warn">En proceso</span>'
           '<span>BNA CA 09-2026 · MACRO 09-2026 · 1.992 registros externos · 1.204 pares</span></div></div>')
    cuerpo = (heading("AGRUPADOR DE CONCEPTOS", "Conciliación externa") + ctx + '<div style="height:14px"></div>' + acc + '<div style="height:14px"></div>'
              + '<div class="split">' + izq + der + '</div>' + resumen)
    escribir("04-conciliacion-externa", pagina("Conciliación externa", "ext", cuerpo, crumb=CRUMB + "Conciliación externa"), w=1800, h=980)

    vacio = (heading("AGRUPADOR DE CONCEPTOS", "Conciliación externa")
             + panel('<div class="empty" style="text-align:center"><div style="font-size:36px;color:#9ab0b8">⇄</div><h2>Sin sesión activa</h2>'
                     '<p class="muted">Creá una sesión nueva o retomá una en proceso desde la lista de sesiones.</p>' + boton("➕ Nueva sesión", True) + '</div>'))
    escribir("04-conciliacion-externa-vacio", pagina("Conciliación externa", "ext", vacio, crumb=CRUMB + "Conciliación externa"), h=640)

    cand = tabla(["", "Perfil", "Fecha", "Concepto original", "Débitos", "Créditos"],
                 [['<input type="radio" name="c" checked>', "BNA CA", "02/09/2026", "PAGO CON TRANSF-BANE", monto(0), monto(100000.00)],
                  ['<input type="radio" name="c">', "MACRO", "02/09/2026", "CR. TARJ. VISA", monto(0), monto(100000.00)],
                  ['<input type="radio" name="c">', "BNA CA", "03/09/2026", "PAGO CON TRANSF-BANE", monto(0), monto(100000.00)]], numericas=(4, 5))
    m = modal("Más de un candidato", "Registro externo: 02/09/2026 · Cobranza tarjeta Visa 01/09 · " + fmt(100000.00),
              '<p class="muted" style="margin-bottom:10px">Hay 3 movimientos del extracto con el mismo importe. Elegí el que corresponde o salteá este registro.</p>' + cand,
              boton("Omitir") + boton("Asignar", True))
    escribir("04-seleccion-candidato", pagina("Conciliación externa", "ext", cuerpo, crumb=CRUMB + "Conciliación externa", modal=m), w=1800, h=980)


# ---------------------------------------------------------------- Línea 5
def m05():
    deb = tabla(["Fecha", "Concepto", "Débitos"], [["01/09/2026", "DB.TRANSF.INTER-LINK", monto(5000000.00)], ["02/09/2026", "DB.TRANSF.INTER-LINK", monto(7500000.00)],
                                                  ["03/09/2026", "DEB. TRANSF.ELE-SUC 0001", monto(12000000.00)], ["05/09/2026", "D.TR.INT CTA.DI-SUC 0001", monto(2300000.00)]],
                numericas=(2,), clases=["sel", "", "", ""])
    cre = tabla(["Fecha", "Concepto", "Créditos"], [["01/09/2026", "CR.TRANSF.INTER-LINK", monto(5000000.00)], ["02/09/2026", "CR.TRANSF.INTER-LINK", monto(7500000.00)],
                                                   ["04/09/2026", "CR.TRANSF.INTER-LINK", monto(11980000.00)], ["05/09/2026", "CR.TRANSF.INTER-LINK", monto(2300000.00)]],
                numericas=(2,), clases=["sel", "", "", ""])
    ctx = ('<div class="context-strip"><div><span class="eyebrow">SESIÓN ACTIVA</span><strong>Transferencias propias 09-2026</strong><span class="badge warn">En proceso</span>'
           '<span>01/09/2026 al 30/09/2026 · TRANSF ANTONIAZZI · 347 pares</span></div></div>')
    acc = '<div class="actions">' + boton("Auto-conciliar") + boton("Conciliar manual", True) + boton("Desconciliar") + boton("Finalizar") + boton("Exportar") + '</div>'
    tabs = '<div class="tabs"><button class="active">⏳ Pendientes</button><button>✅ Conciliados (347)</button></div>'
    resumen = ('<div class="sum"><span>Débito elegido: <strong>' + fmt(5000000.00) + '</strong></span><span>Crédito elegido: <strong>' + fmt(5000000.00) + '</strong></span>'
               '<span>Diferencia: <strong class="ok">' + fmt(0) + ' ✔</strong></span><span class="muted">Cada movimiento puede estar en un solo par por sesión.</span></div>')
    cuerpo = (heading("AGRUPADOR DE CONCEPTOS", "Conciliación interna") + ctx + '<div style="height:14px"></div>' + acc + '<div style="height:14px"></div>'
              + panel(tabs + '<div style="height:12px"></div><div class="grid2"><div><strong style="font-size:13px">Débitos pendientes (4)</strong><div style="height:8px"></div>' + deb
                      + '</div><div><strong style="font-size:13px">Créditos pendientes (4)</strong><div style="height:8px"></div>' + cre + '</div></div>' + resumen))
    escribir("05-conciliacion-interna", pagina("Conciliación interna", "int", cuerpo, crumb=CRUMB + "Conciliación interna"), h=900)

    vacio = (heading("AGRUPADOR DE CONCEPTOS", "Conciliación interna")
             + panel('<div class="actions">' + boton("Nueva", True) + boton("Retomar", off=True) + boton("Eliminar", off=True) + '</div>'
                     '<div class="empty" style="text-align:center"><div style="font-size:36px;color:#9ab0b8">↔</div><h2>Sin sesión activa</h2>'
                     '<p class="muted">Creá una sesión indicando el rango de fechas y los conceptos a conciliar entre sí.</p></div>'))
    escribir("05-conciliacion-interna-vacio", pagina("Conciliación interna", "int", vacio, crumb=CRUMB + "Conciliación interna"), h=640)

    cons = '<div class="cols">' + "".join(f'<label><input type="checkbox"{" checked" if i == 0 else ""}> {c}</label>' for i, c in enumerate(
        ["TRANSF ANTONIAZZI", "TRANSFERENCIAS PROPIAS", "ACREDITACIONES TARJETAS", "ANTICIPO DE SUELDOS", "ARCA - IVA", "COMISIONES BANCARIAS"])) + '</div>'
    n = (heading("AGRUPADOR DE CONCEPTOS", "Nueva sesión de conciliación interna")
         + panel(campo("Nombre de la sesión", "Transferencias propias 09-2026"), "Nombre", "", 1)
         + panel('<div class="form-row"><label style="max-width:190px">Desde<input value="01/09/2026" readonly></label><label style="max-width:190px">Hasta<input value="30/09/2026" readonly></label></div>', "Rango de fechas a conciliar", "", 2)
         + panel(cons, "Conceptos a conciliar", "Débitos y créditos de estos conceptos se concilian entre sí.", 3)
         + '<div class="actions">' + boton("Confirmar", True) + boton("Cancelar") + '</div>')
    escribir("05-nueva-sesion", pagina("Nueva sesión", "int", n, crumb=CRUMB + "Conciliación interna / Nueva"), h=1060)


# ---------------------------------------------------------------- Línea 6
def m06():
    cuentas = tabla(["Cuenta", "Descripción", "Centro de costo", "Conceptos asignados"],
                    [["110105", "TRANSFERENCIAS ENTRE CUENTAS PROPIAS", "—", "1"], ["110201", "DEUDORES POR TRANSFERENCIAS", "—", "1"], ["110203", "TARJETAS A COBRAR", "—", "2"],
                     ["365", "BANCO NACIÓN CTA. CTE.", "—", "0"], ["410330", "DEVOLUCIONES DE COBROS", "VENTAS", "1"], ["520115", "IMPUESTO A LOS DÉBITOS Y CRÉDITOS", "ADMINISTRACIÓN", "1"],
                     ["520118", "COMISIONES BANCARIAS", "ADMINISTRACIÓN", "1"], ["520120", "IVA SOBRE COMISIONES", "ADMINISTRACIÓN", "1"]], numericas=(3,))
    cuerpo = (heading("AGRUPADOR DE CONCEPTOS", "Cuentas contables", "Catálogo importado del sistema contable.")
              + panel('<div class="actions"><input placeholder="Buscar por cuenta o descripción…" style="max-width:340px">' + '<span style="flex:1"></span>' + boton("Importar desde Excel/CSV…", True) + '</div><div style="height:12px"></div>'
                      + cuentas + '<p class="note">126 cuentas · importadas el 05/09/2026</p>'))
    escribir("06-cuentas-contables", pagina("Cuentas contables", "cuentas", cuerpo, crumb=CRUMB + "Cuentas contables"), h=900)
    v = (heading("AGRUPADOR DE CONCEPTOS", "Cuentas contables")
         + panel('<div class="empty" style="text-align:center"><div style="font-size:36px;color:#9ab0b8">#</div><h2>El catálogo todavía está vacío</h2>'
                 '<p class="muted">Importá el plan de cuentas desde un Excel o CSV del sistema.</p>' + boton("Importar desde Excel/CSV…", True) + '</div>'))
    escribir("06-cuentas-contables-vacio", pagina("Cuentas contables", "cuentas", v, crumb=CRUMB + "Cuentas contables"), h=620)

    def imp(error=False):
        pre = tabla(["Columna A", "Columna B", "Columna C"], [["110105", "TRANSFERENCIAS ENTRE CUENTAS PROPIAS", ""], ["110201", "DEUDORES POR TRANSFERENCIAS", ""], ["410330", "DEVOLUCIONES DE COBROS", "VENTAS"]])
        e = "Elegí la columna de la cuenta" if error else ""
        cuerpo_i = (panel('<div class="form-row"><label>Archivo<input type="file"><small>Plan de cuentas homologador.xlsx · 12 KB · 126 filas</small></label></div>', "Archivo", "", 1)
                    + panel('<div class="grid3">' + campo("Columna → Cuenta", "Elegí…" if error else "Columna A", e, "select") + campo("Columna → Descripción", "Columna B", "", "select")
                            + campo("Columna → Centro de costo", "Columna C", "", "select") + '</div><div style="height:12px"></div>' + pre, "Mapeo de columnas", "Vista previa de las primeras filas.", 2)
                    + (aviso("No se importó nada: falta indicar la columna de la cuenta.", True) if error else "")
                    + '<div class="actions">' + boton("Confirmar importación", True) + boton("Cancelar") + '</div>')
        return heading("AGRUPADOR DE CONCEPTOS", "Importar cuentas contables") + cuerpo_i
    escribir("06-importar-cuentas", pagina("Importar cuentas", "cuentas", imp(False), crumb=CRUMB + "Cuentas contables / Importar"), h=1000)
    escribir("06-importar-cuentas-error", pagina("Importar cuentas", "cuentas", imp(True), crumb=CRUMB + "Cuentas contables / Importar"), h=1080)

    conc = tabla(["Concepto estándar", "Cuenta asignada", "Reglas", ""],
                 [["TRANSFERENCIAS RECIBIDAS", "110201 · DEUDORES POR TRANSFERENCIAS", "3", boton("Asignar cuenta…")], ["TRANSF ANTONIAZZI", "110105 · TRANSFERENCIAS ENTRE CUENTAS PROPIAS", "2", boton("Asignar cuenta…")],
                  ["COMISIONES BANCARIAS", "520118 · COMISIONES BANCARIAS", "1", boton("Asignar cuenta…")], ["SUELDOS Y JORNALES", '<span class="badge warn">Sin cuenta</span>', "0", boton("Asignar cuenta…")]],
                 numericas=(2,), clases=["", "sel", "", ""])
    m = modal("Asignar cuenta a «TRANSF ANTONIAZZI»", "La cuenta pasa a ser la cuenta final de todos los movimientos homologados con este concepto, de ahora en más.",
              '<label>Cuenta contable<select><option>110105 · TRANSFERENCIAS ENTRE CUENTAS PROPIAS</option></select></label>',
              boton("Cancelar") + boton("Asignar", True))
    cuerpo_c = (heading("AGRUPADOR DE CONCEPTOS", "Conceptos estándar", "Cada concepto estándar puede tener una cuenta contable.") + panel(conc))
    escribir("06-conceptos-estandar", pagina("Conceptos estándar", "conceptos", cuerpo_c, crumb=CRUMB + "Conceptos estándar", modal=m), h=860)


# ---------------------------------------------------------------- Línea 7
def m07():
    def fila(dato, d, c, nota):
        return [dato, monto(d), monto(c), nota]
    t = tabla(["Dato guardado", "Débitos", "Créditos", "Qué pasa"],
              [fila("Débito 1234567,89", 1234567.89, 0, "Miles con punto, dos decimales con coma; el crédito en gris con 0,00."),
               fila("Crédito 58049,41", 0, 58049.41, "Va en la columna Créditos."),
               fila("Débito −1234,56 (dato histórico con signo)", -1234.56, 0, "Se muestra 1.234,56: nunca con signo."),
               fila("Crédito −1234,56 (dato histórico con signo)", 0, -1234.56, "Se muestra 1.234,56 en Créditos."),
               fila("Importe 0", 0, 0, "Ambas columnas 0,00 en gris."),
               fila("Sin dato (nulo)", None, None, "La celda queda en blanco; no se inventa un cero."),
               fila("Débito 0,005", 0.005, 0, "Redondeo a dos decimales: 0,01 (si el dato tiene más decimales, no se pierde en la base).")],
              numericas=(1, 2))
    r = tabla(["Resultado", "Valor", "Se muestra"],
              [["Diferencia (C − D) mayor que cero", "1951317,66", "+" + fmt(1951317.66)], ["Diferencia (C − D) menor que cero", "−3100,00", "−" + fmt(3100.00)], ["Diferencia cero", "0", fmt(0)]], numericas=(2,))
    xl = ('<div class="notice">En <strong>Excel</strong> cada importe es un <strong>número real</strong> con formato <span class="kbd">#.##0,00</span> '
          '(no un texto), en valor absoluto y en la columna que le corresponde. Los totales y resúmenes siguen la misma regla; sólo la diferencia lleva signo.</div>')
    cuerpo = (heading("LÍNEA 7", "Importes: cómo se muestran", "Formato es-AR fijo (no depende de la cultura del servidor ni del navegador). Un solo formateador en AgrupadorConceptos.Core.")
              + panel(t, "Importes de movimientos", "Débitos y créditos: absoluto, con miles y decimales, en su columna.", 1)
              + panel(r, "Resultados derivados", "La diferencia es un resultado, no un importe de movimiento: conserva el signo.", 2) + xl)
    escribir("07-importes-formato", pagina("Importes", "procesador", cuerpo, crumb="Referencia de diseño / Importes"), h=1180)


def generar():
    m03(); m04(); m05(); m06(); m07()
