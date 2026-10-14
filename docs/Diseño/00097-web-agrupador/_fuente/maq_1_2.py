# -*- coding: utf-8 -*-
# TAREA 00097 - Líneas 1 y 2: menú principal y Procesador.
from maq_base import *


def card(glifo, titulo, desc, ok=True):
    off = "" if ok else " off"
    cand = "" if ok else '<div class="lock">Sin permiso</div>'
    return f'<div class="card{off}"><div class="ico">{glifo}</div><div><h3>{titulo}</h3><p>{desc}</p>{cand}</div></div>'


def menu(permisos):
    def ok(p):
        return p in permisos
    arca = "".join([
        card("⇄", "Comprobantes", "Conciliá los comprobantes de ARCA contra el sistema por perfil y CUIT.", ok("ArcaOffline")),
        card("▤", "Perfiles de comprobantes", "Formatos de archivo, listas de CUIT y directivas.", ok("ArcaPerfiles")),
        card("⇄", "Percepciones y retenciones", "Conciliación de percepciones y retenciones de ARCA contra los mayores.", ok("ArcaPyR")),
        card("▤", "Perfiles PyR", "Cuentas, mayores y directivas de percepciones y retenciones.", ok("ArcaPyR")),
    ])
    agr = "".join([
        card("▦", "Procesador de movimientos", "Importá extractos bancarios y homologá sus conceptos.", ok("AgrProcesador")),
        card("≡", "Gestión de homologaciones", "Reglas que traducen el texto del banco a un concepto estándar.", ok("AgrHomologaciones")),
        card("⌕", "Consulta de movimientos", "Movimientos por rango de fechas y conceptos, con exportación.", ok("AgrHomologaciones")),
        card("⇄", "Conciliación externa", "Conciliá el extracto contra un listado del ERP, por sesiones.", ok("AgrConciliacion")),
        card("↔", "Conciliación interna", "Conciliá débitos contra créditos propios dentro del extracto.", ok("AgrConciliacionInterna")),
        card("#", "Cuentas contables", "Catálogo de cuentas importado del sistema y sus conceptos.", ok("AgrCuentasContables")),
    ])
    return (heading("INICIO", "Conciliador Contable", "Elegí con qué querés trabajar. Los accesos dependen de los permisos de tu usuario.")
            + '<div class="grp">ARCA CLIENTE</div><div class="cards">' + arca + '</div>'
            + '<div class="grp">AGRUPADOR DE CONCEPTOS</div><div class="cards">' + agr + '</div>')


def m01():
    escribir("01-menu-principal", pagina("Inicio", "inicio", menu(TODOS), crumb="Inicio"), h=980)
    parcial = {"ArcaOffline", "AgrProcesador", "AgrConciliacion"}
    escribir("01-menu-principal-permisos", pagina("Inicio", "inicio", menu(parcial), permisos=parcial, crumb="Inicio"), h=980)
    sin = (heading("INICIO", "Conciliador Contable")
           + panel('<div style="text-align:center;padding:46px 20px"><div style="font-size:40px;color:#9ab0b8">🔒</div>'
                   '<h2>Tu usuario todavía no tiene permisos asignados</h2>'
                   '<p class="muted">Pedile a un administrador que te habilite los accesos que necesitás y volvé a ingresar.</p></div>'))
    escribir("01-menu-principal-sin-acceso", pagina("Inicio", "inicio", sin, permisos=set(), crumb="Inicio"), h=620)


def grilla_procesador(sel=2, pie=True):
    filas, clases = [], []
    for i, (f, c, e, d, cr, cf, cta) in enumerate(MOV):
        pend = e == "Pendiente de homologar"
        est = '<span class="badge warn">Pendiente de homologar</span>' if pend else e
        edit_cf = f'<td><input value="{cf}" readonly style="min-height:30px;padding:5px 8px;width:215px"></td>' if not pend else '<td><input value="" readonly placeholder="(sin concepto final)" style="min-height:30px;padding:5px 8px;width:215px"></td>'
        edit_ct = f'<td><input value="{cta}" readonly style="min-height:30px;padding:5px 8px;width:92px"></td>'
        chk = f'<td><input type="checkbox"{" checked" if i in (1, 4, 8) else ""}></td>'
        filas.append([chk, f, c, c, est, monto(d), monto(cr), edit_cf, edit_ct])
        clases.append("sel" if i == sel else ("pend" if pend else ""))
    t = tabla(["", "Fecha", "Concepto original", "Descripción", "Concepto estándar", "Débitos", "Créditos", "Concepto final", "Cuenta final"],
              filas, numericas=(5, 6), clases=clases)
    p = '<div class="pager">Mostrando 12 de 7.840 filas · desplazá para ver más (la lista se carga por tramos)</div>' if pie else ''
    return t + p


def m02_procesador():
    ctx = ('<div class="context-strip"><div><span class="eyebrow">PERFIL DE BANCO</span>'
           '<select style="width:260px"><option>BNA CA</option></select>' + boton("+ Nuevo perfil") + boton("Editar perfil") +
           '<span>Perfil 8 · importe único en «Monto» · encabezado en la fila 6</span></div></div>')
    paso1 = panel('<div class="form-row"><label>Archivo del banco<input type="file"><small>.xls, .xlsx o .csv · hasta 25 MB</small></label>'
                  + boton("Importar archivo", True) + '</div>', "Cargar archivo del banco", "Se lee, se homologa y se guarda. Si alguna fecha es inválida, no se guarda nada.", 1)
    paso2 = panel('<div class="form-row"><label>Archivos leídos<select><option>' + ARCHIVO + ' (14/10/2026 10:52)</option></select></label>'
                  + boton("Cargar datos") + boton("Borrar archivo") + '</div>', "Archivo en trabajo", "", 2)
    stats = ('<div class="stats"><div>Registros leídos<strong>7.840</strong></div><div>Homologados<strong>7.402</strong></div>'
             '<div>Pendientes<strong>438</strong></div><div>Débitos<strong>' + fmt(253073251.55) + '</strong></div>'
             '<div>Créditos<strong>' + fmt(255024569.21) + '</strong></div><div>Diferencia (C − D)<strong>' + dif(1951317.66) + '</strong></div></div>')
    acc = ('<div class="actions">' + boton("Homologar fila seleccionada…", True) + boton("Pendientes agrupados…") + boton("Exportar consolidado…")
           + boton("Exportar detalle…") + '<span style="flex:1"></span>' + boton("Eliminar seleccionados (3)") + '</div>')
    cuerpo = (heading("AGRUPADOR DE CONCEPTOS", "Procesador de movimientos", "Importá el extracto, homologá los conceptos y exportá el resultado.")
              + ctx + '<div style="height:16px"></div>' + paso1 + paso2
              + panel(stats + acc + '<div style="height:10px"></div>' + grilla_procesador()))
    escribir("02-procesador", pagina("Procesador de movimientos", "procesador", cuerpo, crumb="Agrupador / Procesador"), h=1720)


def m02_vacio():
    ctx = ('<div class="context-strip"><div><span class="eyebrow">PERFIL DE BANCO</span><select style="width:260px"><option>Elegí un perfil…</option></select>'
           + boton("+ Nuevo perfil") + boton("Editar perfil", off=True) + '</div></div>')
    vacio = panel('<div class="empty" style="text-align:center"><div style="font-size:38px;color:#9ab0b8">▦</div><h2>Todavía no hay un archivo en trabajo</h2>'
                  '<p class="muted">Elegí un perfil de banco, importá un extracto o abrí uno de los archivos ya leídos.<br>'
                  'Si el banco no tiene perfil, creá uno con «+ Nuevo perfil».</p></div>')
    cuerpo = heading("AGRUPADOR DE CONCEPTOS", "Procesador de movimientos") + ctx + '<div style="height:16px"></div>' + vacio
    escribir("02-procesador-vacio", pagina("Procesador de movimientos", "procesador", cuerpo, crumb="Agrupador / Procesador"), h=720)


def m02_error():
    ctx = ('<div class="context-strip"><div><span class="eyebrow">PERFIL DE BANCO</span><select style="width:260px"><option>BNA CA</option></select>'
           + boton("+ Nuevo perfil") + boton("Editar perfil") + '</div></div>')
    err = aviso('<strong>No se importó el archivo.</strong> Hay 3 fechas vacías o inválidas. <strong>No se guardó ningún movimiento ni la cabecera.</strong> '
                'Corregí esas filas en el Excel y volvé a importarlo.', True)
    t = tabla(["Archivo", "Hoja", "Fila", "Valor leído", "Motivo"],
              [["BNA 09-2026.xlsx", "Hoja1", "1.204", "31/02/2026", "La fecha no existe"],
               ["BNA 09-2026.xlsx", "Hoja1", "1.987", "(vacía)", "Falta la fecha"],
               ["BNA 09-2026.xlsx", "Hoja1", "4.310", "15-13-2026", "Mes fuera de rango"]])
    cuerpo = (heading("AGRUPADOR DE CONCEPTOS", "Procesador de movimientos") + ctx + '<div style="height:16px"></div>'
              + panel('<div class="form-row"><label>Archivo del banco<input type="file"><small>BNA 09-2026.xlsx · 289 KB</small></label>' + boton("Importar archivo", True) + '</div>', "Cargar archivo del banco", "", 1)
              + err + panel(t, "Filas con problemas", "Los números de fila son los del Excel."))
    escribir("02-procesador-error", pagina("Procesador de movimientos", "procesador", cuerpo, crumb="Agrupador / Procesador"), h=900)


def m02_progreso():
    paso = ('<div class="stepper"><span class="done">✔ Leyendo archivo</span><span class="done">✔ Homologando</span>'
            '<span class="now">● Guardando en la base · 6.100 / 7.840</span><span>○ Mostrando datos</span></div><div class="bar"><i style="width:78%"></i></div>')
    cuerpo = (heading("AGRUPADOR DE CONCEPTOS", "Procesador de movimientos")
              + panel('<div class="form-row"><label>Archivo del banco<input type="file" disabled><small>' + ARCHIVO + '</small></label>'
                      + boton("Importar archivo", True, off=True) + '</div>' + paso
                      + '<p class="note">Una importación por vez. Mientras corre no se puede borrar ni cargar otro archivo. Si cerrás la página, la importación se cancela y no queda nada a medias.</p>',
                      "Importando", "BNA CA · 7.840 movimientos", 1))
    escribir("02-importacion-progreso", pagina("Procesador de movimientos", "procesador", cuerpo, crumb="Agrupador / Procesador"), h=640)


def m02_perfil(error=False):
    e1 = "Escribí el nombre del banco" if error else ""
    e2 = "Elegí la columna de fecha" if error else ""
    e3 = "Elegí la columna del importe" if error else ""
    nombre = "" if error else "BNA CA"
    fecha = "Elegí…" if error else "Fecha"
    imp = "Elegí…" if error else "Monto"
    cab = (heading("AGRUPADOR DE CONCEPTOS", "Perfil de banco", "Indicá cómo está armado el extracto de este banco.")
           + (aviso("No se pudo guardar el perfil. Revisá los campos marcados.", True) if error else ""))
    p1 = panel('<div class="form-row">' + campo("Nombre del banco", nombre, e1) +
               '<label>Cuenta contable (opcional)<select><option>365 · BANCO NACIÓN CTA. CTE.</option></select><small>Sólo de referencia; no define la cuenta de los movimientos.</small></label></div>',
               "Banco", "", 1)
    p2 = panel('<div class="form-row"><label>Cargar ejemplo de Excel<input type="file"><small>Sirve para ver los encabezados y elegir las columnas.</small></label>'
               '<label>Fila de encabezados<input value="6" readonly><small>Encabezados detectados: Fecha · Comprobante · Concepto · Monto · SALDO</small></label></div>'
               '<div class="grid3" style="margin-top:14px">' + campo("Columna fecha", fecha, e2, "select") + campo("Columna concepto / id", "Concepto", "", "select")
               + campo("Columna descripción", "Concepto", "", "select") + '</div>'
               '<label class="check" style="margin-top:12px"><input type="checkbox"> ¿Es código exacto? (si no, es un texto largo)</label>',
               "Mapeo de columnas", "", 2)
    p3 = panel('<div class="actions"><label class="check"><input type="radio" name="t" checked> Importe único (positivo = crédito, negativo = débito)</label>'
               '<label class="check"><input type="radio" name="t"> Debe / Haber (dos columnas)</label></div>'
               '<div class="grid3" style="margin-top:14px">' + campo("Columna importe", imp, e3, "select") + '</div>',
               "Mapeo de importes", "", 3)
    pie = '<div class="actions">' + boton("Guardar perfil", True) + boton("Cancelar") + '</div>'
    return cab + p1 + p2 + p3 + pie


def m02_perfiles():
    escribir("02-perfil-banco", pagina("Perfil de banco", "procesador", m02_perfil(False), crumb="Agrupador / Perfil de banco"), h=1100)
    escribir("02-perfil-banco-error", pagina("Perfil de banco", "procesador", m02_perfil(True), crumb="Agrupador / Perfil de banco"), h=1180)


def fondo_proc():
    return (heading("AGRUPADOR DE CONCEPTOS", "Procesador de movimientos")
            + panel('<div class="stats"><div>Registros leídos<strong>7.840</strong></div><div>Pendientes<strong>438</strong></div></div>' + grilla_procesador(pie=False)))


def m02_dialogos():
    cuerpo = (campo("Concepto de banco", "PAGO CON TRANSF-BANE", "", "input", "Si es una descripción larga, podés acortar el texto para que busque esa palabra clave (ej. «TRANSF»).")
              + '<div style="height:12px"></div>' + campo("Concepto estándar", "TRANSFERENCIAS RECIBIDAS", "", "select")
              + '<div style="height:12px"></div>' + campo("Cuenta contable", "110201 · DEUDORES POR TRANSFERENCIAS", "", "input", "Sale del concepto estándar; se cambia en Conceptos estándar."))
    m = modal("Crear homologación de conceptos", "Perfil BNA CA · afecta a 7.436 movimientos hoy pendientes o ya homologados con otra regla", cuerpo,
              boton("Cancelar") + boton("Guardar homologación", True))
    escribir("02-homologar-fila", pagina("Procesador de movimientos", "procesador", fondo_proc(), crumb="Agrupador / Procesador", modal=m), h=900)

    filas = [["CREDITO POR CONCEP.VARIOS", "69", monto(0), monto(3125400.00), boton("Homologar")],
             ["D.TR.INT CTA.DI-SUC 0001", "27", monto(48230000.00), monto(0), boton("Homologar")],
             ["TRANSF.INTERBANCARIA REC", "112", monto(0), monto(7650318.40), boton("Homologar")],
             ["DEB.AUT.PROVEEDORES", "58", monto(1904412.15), monto(0), boton("Homologar")],
             ["AJUSTE CAJA", "3", monto(1200.00), monto(0), boton("Homologar")]]
    t = tabla(["Concepto original", "Movimientos", "Débitos", "Créditos", ""], filas, numericas=(1, 2, 3))
    m2 = modal("Pendientes agrupados", "438 movimientos sin homologar, agrupados por concepto original. Homologar un grupo asigna la regla a todos sus movimientos.", t,
               boton("Cerrar", True))
    escribir("02-pendientes-agrupados", pagina("Procesador de movimientos", "procesador", fondo_proc(), crumb="Agrupador / Procesador", modal=m2), h=900)

    cons = tabla(["Concepto estándar", "Cuenta", "Débitos", "Créditos"],
                 [["TRANSFERENCIAS RECIBIDAS", "110201", monto(0), monto(251902115.78)],
                  ["TRANSF ANTONIAZZI", "110105", monto(231400000.00), monto(2400000.00)],
                  ["COMISIONES BANCARIAS", "520118", monto(178422.61), monto(0)],
                  ["IMPUESTO A LOS DÉBITOS Y CRÉDITOS", "520115", monto(412977.40), monto(0)],
                  ["Total", "", monto(253073251.55), monto(255024569.21)]], numericas=(2, 3), clases=["", "", "", "", "sel"])
    m3 = modal("Consolidado bancario · 14/10/2026 10:55", ARCHIVO,
               aviso("Hay <strong>438 movimientos sin homologar</strong>. Se exportan sólo los homologados; el total puede no coincidir con el extracto.") + cons +
               '<p class="note">Los importes se exportan como números con formato #.##0,00, en valor absoluto y en su columna.</p>',
               boton("Cerrar") + boton("Exportar a Excel", True))
    escribir("02-consolidado-vista-previa", pagina("Procesador de movimientos", "procesador", fondo_proc(), crumb="Agrupador / Procesador", modal=m3), h=900)

    m4 = modal("No se puede borrar", "Archivo: " + ARCHIVO,
               aviso("La sesión externa «Proveedores 09-2026» [En proceso] utiliza este archivo. <strong>No se eliminó nada.</strong>", True) +
               '<p class="note">Los motivos posibles: movimientos tomados por pares de conciliación, o una sesión externa o interna en proceso o finalizada que alcanza el archivo. '
               'Un error de base de datos se informa aparte, con su mensaje.</p>', boton("Entendido", True))
    escribir("02-baja-bloqueada", pagina("Procesador de movimientos", "procesador", fondo_proc(), crumb="Agrupador / Procesador", modal=m4), h=900)


def generar():
    m01(); m02_procesador(); m02_vacio(); m02_error(); m02_progreso(); m02_perfiles(); m02_dialogos()
