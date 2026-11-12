# -*- coding: utf-8 -*-
# Fecha: 12/11/2026 - TAREA: 00180 - Linea: 1-8 - Maquetas de «Asientos para el estudio contable».
# Genera los HTML de _fuente/ y los exporta a PNG en la carpeta del diseño (nombres de la tabla de «## Maquetas»).
# Los datos son de EJEMPLO pero respetan los totales de los criterios de aceptación del prompt (§7): BANCOS 140
# asientos / 965 líneas / 44.074.058.175,97; VARIOS 05–09: 98 / 600 / 74.568.487.094,93; DEVENGAMIENTOS 44 / 156.
import sys
sys.stdout.reconfigure(encoding="utf-8")
import base
from base import fmt, pagina, heading, panel, tabla, campo, aviso, boton, modal, escribir

# ---------------------------------------------------------------- menú de este módulo
base.NAV = base.NAV + [("ASIENTOS PARA EL ESTUDIO", [
    ("generar", "⇪", "Generar asientos", "AsientosGenerar"),
    ("historial", "☰", "Historial", "AsientosGenerar"),
    ("config", "⚙", "Configuración", "AsientosConfig")])]
base.TODOS = {p for _, items in base.NAV for _, _, _, p in items if p}

CSS = """
<style>
.drop{border:2px dashed #9fc3c0;border-radius:12px;background:#f7fbfb;padding:46px 20px;text-align:center}
.drop .big{font-size:38px;color:#65918e;line-height:1} .drop h2{margin:10px 0 4px} .drop p{color:var(--muted);margin:0 0 14px}
.drop.err{border-color:#d9776a;background:#fff8f6}
.file{display:flex;align-items:center;gap:14px;justify-content:space-between;border:1px solid #cfe0de;border-left:4px solid var(--teal);border-radius:7px;background:#fff;padding:14px 18px}
.file b{font-size:15px} .file small{margin:2px 0 0}
.mono{font-family:Consolas,"Courier New",monospace;font-size:12px}
.chips{display:flex;gap:8px;flex-wrap:wrap;align-items:center}
.chip{border:1px solid #cbd6dc;border-radius:16px;background:#fff;padding:5px 12px;font-size:12px;font-weight:600;color:#3d5563}
.chip.on{background:#153445;border-color:#153445;color:#fff}
.chip i{font-style:normal;font-weight:400;opacity:.8;margin-left:4px}
.cards3{display:grid;grid-template-columns:repeat(3,1fr);gap:12px}
.tipo{border:1px solid var(--line);border-radius:8px;padding:14px 16px;background:#fff}
.tipo.on{border:2px solid var(--teal);background:#ecf7f4;padding:13px 15px}
.tipo b{display:block;font-size:14px;margin-bottom:3px} .tipo span{font-size:12px;color:var(--muted);line-height:1.5}
.tipo em{font-style:normal;font-size:10px;font-weight:700;color:#157a70;letter-spacing:.8px}
.meses{display:grid;grid-template-columns:repeat(auto-fit,minmax(210px,1fr));gap:12px}
.mes{border:1px solid var(--line);border-radius:8px;padding:13px 15px;background:#fff;display:block}
.mes label{flex-direction:row;align-items:center;gap:9px;font-size:14px}
.mes small{margin:6px 0 0 25px;line-height:1.55}
.mes.warn{background:#fff8e6;border-color:#e3c27a}
.mes.off{opacity:.6;background:#f7f9fa}
.tree{font-size:13px} .tree .m{font-weight:700;padding:8px 6px;color:#153445;display:flex;justify-content:space-between}
.tree .h{padding:6px 6px 6px 22px;display:flex;justify-content:space-between;border-radius:5px;color:#3d5563}
.tree .h.sel{background:#dff0ee;font-weight:700;color:#0e4f4a} .tree .h .c{color:var(--muted);font-variant-numeric:tabular-nums}
.tree .m span.dot,.tree .h span.dot{display:inline-block;width:8px;height:8px;border-radius:50%;margin-left:7px}
.dot.ok{background:#4aa87a}.dot.w{background:#e0a23a}.dot.e{background:#d9604f}
.vp{display:grid;grid-template-columns:270px minmax(0,1fr);gap:18px;align-items:start}
.crumbs{font-size:12px;color:var(--muted);margin:0 0 6px}.crumbs b{color:#153445}
.sev{display:inline-block;border-radius:4px;padding:4px 8px;font-size:10px;font-weight:700;letter-spacing:.4px;white-space:nowrap}
.sev.b{background:#fde7e3;color:#a2392a}.sev.a{background:#fff1d7;color:#9c6920}.sev.i{background:#e6eef5;color:#3b5f7c}
.est{font-size:11px;font-weight:600}.est.ok{color:#24764f}.est.a{color:#9c6920}.est.o{color:#6c7b84}
tr.adv td{background:#fffaf0} tr.bloq td{background:#fff6f4} tr.inf td{background:#f8fafc}
.banner{display:flex;align-items:center;justify-content:space-between;gap:18px;border-radius:8px;padding:14px 18px;margin:0 0 18px;font-size:13px}
.banner.ok{background:#eaf6ef;border:1px solid #b9dcc8;color:#1f5e3f}
.banner.bad{background:#fff1ed;border:1px solid #efc4b8;color:#923d2b}
.banner.warn{background:#fff8e6;border:1px solid #e3c27a;color:#6f5013}
.notice.warn{background:#fff8e6;border-color:#e3c27a;color:#6f5013}
.banner b{font-size:14px}
.checks{list-style:none;margin:0;padding:0;display:grid;gap:9px;font-size:13px}
.checks li::before{content:"✓";color:#24764f;font-weight:700;margin-right:9px}
.checks li.no::before{content:"✕";color:#a2392a}
.kv{display:grid;grid-template-columns:200px 1fr;gap:9px 14px;font-size:13px;align-items:baseline}
.kv dt{color:var(--muted)} .kv dd{margin:0;font-weight:600}
.txt{background:#16222a;color:#d6e4ea;border-radius:7px;padding:14px 16px;font-family:Consolas,"Courier New",monospace;font-size:12px;line-height:1.75;white-space:pre;overflow:hidden}
.txt i{font-style:normal;color:#e8b04a}
.adj{background:#fff1d7;border-radius:3px;padding:1px 5px;font-weight:700;color:#8a5a14}
.opt{border:1px solid var(--line);border-radius:8px;padding:13px 15px;margin-bottom:10px;display:block}
.opt.on{border:2px solid var(--teal);background:#f2f9f8;padding:12px 14px}
.opt label{flex-direction:row;align-items:flex-start;gap:10px;font-size:13px}
.opt small{margin-left:26px;line-height:1.55}
.sugg{border:1px solid #9fc3c0;border-radius:6px;background:#fff;box-shadow:0 6px 18px rgba(0,0,0,.12);font-size:12px;margin-top:4px}
.sugg div{padding:7px 11px;border-bottom:1px solid #edf1f3;display:flex;justify-content:space-between;gap:12px}
.sugg div:last-child{border:0}.sugg .hi{background:#dff0ee}
.sugg b{font-family:Consolas,monospace;font-weight:600}
.tick{color:#24764f;font-weight:700}
.vacio{text-align:center;padding:54px 20px;color:var(--muted)}.vacio .big{font-size:42px;color:#a9c4c2}.vacio h2{color:#2c4857}
.mini{font-size:11px;color:var(--muted)}
.right{text-align:right}
.hash{font-family:Consolas,monospace;font-size:12px;background:#f0f4f6;border-radius:4px;padding:3px 7px}
.two{display:grid;grid-template-columns:1fr 1fr;gap:18px}
.sticky-foot{display:flex;justify-content:space-between;align-items:center;gap:16px;margin-top:6px}
</style>"""


def P(titulo, activo, cuerpo, crumb="", modal_html=""):
    """Página dentro del shell, con los estilos propios del módulo."""
    h = pagina(titulo, activo, cuerpo, crumb=crumb, modal=modal_html)
    return h.replace("</head>", CSS + "</head>")


def pasos(actual):
    nombres = ["1 · Cargar y detectar", "2 · Vista previa", "3 · Alertas", "4 · Generar"]
    out = []
    for i, n in enumerate(nombres):
        c = "done" if i < actual else "current" if i == actual else ""
        out.append(f'<span class="{c}">{n}</span>')
    return f'<div class="steps">{"".join(out)}</div>'


def cab(sub, titulo="Generar asientos"):
    return heading("ASIENTOS PARA EL ESTUDIO CONTABLE", titulo, sub)


SUB = "Cargá la planilla del área contable, revisá las alertas y generá el TXT para Holistor Contable."


def sev(s):
    m = {"B": ("b", "BLOQUEANTE"), "A": ("a", "ADVERTENCIA"), "I": ("i", "INFORMATIVA")}[s]
    return f'<span class="sev {m[0]}">{m[1]}</span>'


def kpis(items):
    return '<div class="stats">' + "".join(f'<div>{k}<strong>{v}</strong></div>' for k, v in items) + '</div>'


def archivo_chip(nombre, detalle, extra=""):
    return (f'<div class="file"><div><b>📄 {nombre}</b><small>{detalle}</small></div>'
            f'<div class="actions">{extra}{boton("Cambiar archivo")}</div></div>')


def tipo_cards(sel):
    d = [("BANCOS", "Una hoja por banco o cuenta. Marcador «MES mm/aaaa» y encabezados «ASIENTO n»."),
         ("DEVENGAMIENTOS", "Una hoja por mes, nombrada MM-AAAA. Cada asiento con su línea de encabezado."),
         ("VARIOS", "Hojas por circuito (ventas, compras, cajas…). Varios meses por hoja, uno debajo de otro.")]
    out = []
    for n, t in d:
        on = n == sel
        out.append(f'<div class="tipo{" on" if on else ""}"><label class="check"><input type="radio" name="t"{" checked" if on else ""}> <b style="margin:0">{n}</b></label>'
                   f'<span>{t}</span>{"<br><em>DETECTADO AUTOMÁTICAMENTE</em>" if on else ""}</div>')
    return f'<div class="cards3">{"".join(out)}</div>'


# ---------------------------------------------------------------- datos de ejemplo
HOJAS_BANCOS = [  # hoja, registrables, informativos
    ("BBVA", 12, 3), ("MUNICIPAL", 6, 1), ("BICA", 8, 2), ("COMAFI", 10, 4), ("CREDICOOP", 9, 2), ("GALICIA", 11, 3),
    ("HIPOTECARIO", 5, 1), ("MACRO 663", 13, 4), ("MACRO 664", 9, 3), ("BNA CA", 14, 5), ("BNA CC", 7, 2),
    ("NBSF", 9, 3), ("PATAGONIA", 8, 2), ("SANTANDER", 19, 3)]
assert sum(h[1] for h in HOJAS_BANCOS) == 140 and sum(h[2] for h in HOJAS_BANCOS) == 38
MESES_BANCOS = [("05/2026", 34, 238, "10.812.447.318,25", "31/05/2026"), ("06/2026", 35, 241, "11.003.912.664,10", "30/06/2026"),
                ("07/2026", 36, 245, "11.140.318.402,38", "31/07/2026"), ("08/2026", 35, 241, "11.117.379.791,24", "31/08/2026")]
assert sum(m[1] for m in MESES_BANCOS) == 140 and sum(m[2] for m in MESES_BANCOS) == 965
JUL = [("BBVA", 3), ("MUNICIPAL", 1), ("BICA", 2), ("COMAFI", 3), ("CREDICOOP", 2), ("GALICIA", 3), ("HIPOTECARIO", 1),
       ("MACRO 663", 3), ("MACRO 664", 2), ("BNA CA", 4), ("BNA CC", 2), ("NBSF", 2), ("PATAGONIA", 2), ("SANTANDER", 6)]
assert sum(j[1] for j in JUL) == 36
TOTAL_B = "44.074.058.175,97"

HOJAS_VARIOS = [("VENTAS", "05-2026", 12), ("COMPRAS", "MAYO 2026", 17), ("CAJAS", "MAYO 2026", 19),
                ("CHEQUES EMITIDOS", "MAYO 2026", 11), ("CHEQUES ENDOSADOS", "MAYO 2026", 19), ("SUELDOS", "MAYO 2026", 20)]
assert sum(h[2] for h in HOJAS_VARIOS) == 98
MESES_VARIOS = [("05/2026", 24, 150, "18.204.557.310,40"), ("06/2026", 23, 144, "17.688.021.902,75"),
                ("07/2026", 23, 144, "18.115.340.667,30"), ("08/2026", 23, 143, "17.906.502.954,60"),
                ("09/2026", 5, 19, "2.654.064.259,88")]
assert sum(m[1] for m in MESES_VARIOS) == 98 and sum(m[2] for m in MESES_VARIOS) == 600


def hojas_table_bancos():
    filas = [[f"<b>{h}</b>", "05/2026 a 08/2026", str(r), f'<span class="mini">{i}</span>'] for h, r, i in HOJAS_BANCOS]
    filas.append(["<b>Total · 14 hojas</b>", "", "<b>140</b>", "<b>38</b>"])
    return tabla(["Hoja", "Meses encontrados", "Asientos a registrar", "Informativos (excluidos)"], filas, numericas=(2, 3))


def meses_cards(meses, marcar09=False, cols=4):
    out = []
    for m in meses:
        warn = marcar09 and m[0] == "09/2026"
        extra = '<br><span class="badge warn">Fuera del rango del nombre del archivo · A9</span>' if warn else ""
        fecha = ""
        out.append(f'<div class="mes{" warn" if warn else ""}"><label><input type="checkbox" checked> <b>{m[0]}</b></label>'
                   f'<small>{m[1]} asientos · {m[2]} líneas<br>Debe: {m[3]}{extra}</small></div>')
    return f'<div class="meses">{"".join(out)}</div>'


# ================================================================ 01 · CARGA
def m01_carga():
    cuerpo = (cab(SUB) + pasos(0)
        + panel(archivo_chip("BANCOS FCE 2027.xlsx", "1,8 MB · leído el 12/11/2026 08:12 · SHA-256 <span class='hash'>9f3c1d…a71e</span> · 14 hojas"), "Archivo", num=1)
        + '<div class="two">'
        + panel(tipo_cards("BANCOS") + '<p class="note">Detectado por: marcador «MES mm/aaaa» en las 14 hojas y encabezados «ASIENTO n». Si no es el tipo correcto, elegí otro: se vuelve a leer.</p>', "Tipo de planilla", num=2)
        + panel(meses_cards(MESES_BANCOS) + '<p class="note">El nombre del archivo no indica un rango: se incluyen todos los meses encontrados. Podés desmarcar los que no quieras.</p>', "Meses a incluir", num=3)
        + '</div>'
        + panel(hojas_table_bancos(), "Hojas encontradas", "Una por banco o cuenta, en el orden del Excel (es el orden de salida del TXT).", num=4)
        + f'<div class="sum"><span>Selección: <strong>140</strong> asientos a registrar · <strong>965</strong> líneas · <strong>38</strong> informativos excluidos · Debe <strong>{TOTAL_B}</strong></span>'
          f'<span class="actions">{boton("Volver")}{boton("Ver vista previa →", True)}</span></div>')
    return P("Generar asientos", "generar", cuerpo, crumb="Asientos para el estudio / Generar")


def m01_vacio():
    cuerpo = (cab(SUB) + pasos(0)
        + '<section class="panel"><div class="drop"><div class="big">⇪</div><h2>Arrastrá la planilla acá</h2>'
          '<p>o elegí un archivo de tu equipo · solo .xlsx · hasta 20 MB</p>' + boton("Elegir archivo…", True) + '</div></section>'
        + '<div class="intro-grid"><div><span class="intro-number">01</span><h3>BANCOS</h3><p>Una hoja por banco o cuenta, con los asientos de cada mes. Los asientos «INFORMATIVO — NO SE REGISTRA» se excluyen.</p></div>'
          '<div><span class="intro-number">02</span><h3>DEVENGAMIENTOS</h3><p>Una hoja por mes (MM-AAAA). El concepto de cada asiento es el texto de su línea de encabezado.</p></div>'
          '<div><span class="intro-number">03</span><h3>VARIOS</h3><p>Hojas por circuito con varios meses cada una. Las notas y aclaraciones debajo de un asiento se ignoran.</p></div></div>'
        + aviso("Última generación: <b>12/11/2026 08:14</b> · BANCOS FCE 2027.xlsx · 140 asientos · <a href='#'>ver en el historial</a>"))
    return P("Generar asientos", "generar", cuerpo, crumb="Asientos para el estudio / Generar")


def m01_error():
    cuerpo = (cab(SUB) + pasos(0)
        + '<section class="panel"><div class="drop err"><div class="big" style="color:#c0604f">⚠</div><h2>No se pudo cargar el archivo</h2>'
          '<p>Arrastrá otra planilla o elegí un archivo .xlsx</p>' + boton("Elegir archivo…", True) + '</div></section>'
        + aviso("<b>ASIENTOS VARIOS 05-2026.xlsm</b> fue rechazado.<br><b>Dónde:</b> archivo subido (extensión .xlsm).<br>"
                "<b>Causa:</b> los libros con macros no se procesan: una macro podría ejecutarse al abrirlos.<br>"
                "<b>Arreglo:</b> en Excel, <span class='kbd'>Archivo → Guardar como → Libro de Excel (.xlsx)</span> y volvé a cargarlo.", True)
        + panel('<ul class="checks" style="font-weight:400"><li class="no">.xlsm o .xls: se piden en .xlsx</li><li class="no">Más de 20 MB (el máximo se cambia en Configuración)</li>'
                '<li class="no">Archivo protegido con contraseña o dañado</li><li class="no">Libro sin ninguna hoja con asientos reconocibles</li></ul>',
                "Qué más se rechaza", "Cada rechazo dice dónde, por qué y cómo arreglarlo."))
    return P("Generar asientos", "generar", cuerpo, crumb="Asientos para el estudio / Generar")


def m01_meses():
    hojas = [[f"<b>{n}</b>", f'<span class="mini">{fm}</span>', str(c)] for n, fm, c in HOJAS_VARIOS]
    hojas.append(["<b>Total · 6 hojas</b>", "", "<b>98</b>"])
    cuerpo = (cab(SUB) + pasos(0)
        + panel(archivo_chip("ASIENTOS VARIOS 05-2026 a 08-2026.xlsx", "2,4 MB · leído el 10/11/2026 15:38 · SHA-256 <span class='hash'>c58e02…44bd</span> · 6 hojas"), "Archivo", num=1)
        + '<div class="two">' + panel(tipo_cards("VARIOS"), "Tipo de planilla", num=2)
        + panel(meses_cards(MESES_VARIOS, marcar09=True) +
                aviso("<b>Septiembre 2026 aparece en 2 hojas</b> (CHEQUES EMITIDOS y CHEQUES ENDOSADOS), pero el nombre del archivo dice «05-2026 a 08-2026». "
                      "Decidí si se incluye: está marcado porque así se resolvió la vez anterior. Queda registrado en el historial."), "Meses a incluir", num=3) + '</div>'
        + panel(tabla(["Hoja (circuito)", "Formato del marcador de mes", "Asientos a registrar (05–09)"], hojas, numericas=(2,))
                + '<p class="note">Se omiten 9 asientos vacíos o en cero (A8) y 1 bloque «ASIENTO QUE NO SE REGISTRA» del ejercicio anterior (A10). Se aceptan los dos formatos de mes: «05-2026» y «MAYO 2026».</p>',
                "Hojas encontradas", num=4)
        + '<div class="sum"><span>Con 05–09: <strong>98</strong> asientos · <strong>600</strong> líneas · Debe <strong>74.568.487.094,93</strong> '
          '&nbsp;|&nbsp; sin septiembre: 93 · 581 · 71.914.422.835,05</span>'
          f'<span class="actions">{boton("Volver")}{boton("Ver vista previa →", True)}</span></div>')
    return P("Generar asientos", "generar", cuerpo, crumb="Asientos para el estudio / Generar")


# ================================================================ 02 · VISTA PREVIA
CON_AVISO = ("COMAFI", "MACRO 663", "MACRO 664", "BNA CA", "NBSF", "SANTANDER")


def arbol():
    out = ['<div class="panel tree" style="padding:14px">']
    for m, n, *_ in MESES_BANCOS:
        if m == "07/2026":
            out.append(f'<div class="m"><span>▾ {m}</span><span class="c">{n} <span class="dot w"></span></span></div>')
            for h, c in JUL:
                sel = h == "MACRO 663"
                dot = "w" if h in CON_AVISO else "ok"
                out.append(f'<div class="h{" sel" if sel else ""}"><span>{h}</span><span class="c">{c}<span class="dot {dot}"></span></span></div>')
        else:
            out.append(f'<div class="m"><span>▸ {m}</span><span class="c">{n} <span class="dot w"></span></span></div>')
    out.append('<p class="mini" style="margin:10px 6px 0"><span class="dot ok" style="display:inline-block;width:8px;height:8px;border-radius:50%"></span> OK &nbsp;'
               '<span class="dot w" style="display:inline-block;width:8px;height:8px;border-radius:50%"></span> advertencia &nbsp;'
               '<span class="dot e" style="display:inline-block;width:8px;height:8px;border-radius:50%"></span> error</p></div>')
    return "".join(out)


def m02_vp():
    filas = [
        ["1", "07-2026-MACRO 663", "4", "20.753.350,01", "20.753.350,01", "0,00", '<span class="est a">⚠ Advertencia · ajuste de centavo</span>', "<b>0085</b>"],
        ["2", "07-2026-MACRO 663", "3", "1.480.220,55", "1.480.220,55", "0,00", '<span class="est a">⚠ Advertencia · 1 línea sin cuenta (A1)</span>', "<b>0086</b>"],
        ["3", "07-2026-MACRO 663", "2", "3.250.000,00", "0,00", "—", '<span class="est o">Omitido · informativo (A7)</span>', "—"],
        ["4", "07-2026-MACRO 663", "3", "612.904,18", "612.904,18", "0,00", '<span class="est ok">✓ OK</span>', "<b>0087</b>"],
    ]
    clases = ["adv", "adv", "inf", ""]
    cuerpo = (cab(SUB) + pasos(1)
        + kpis([("Asientos a registrar", "140"), ("Informativos excluidos", "38"), ("Líneas", "965"), ("Total Debe", TOTAL_B), ("Total Haber", TOTAL_B), ("Diferencia", "0,00")])
        + '<div class="toolbar"><div class="chips"><span class="chip on">Todos<i>178</i></span><span class="chip">OK<i>127</i></span><span class="chip">Advertencia<i>13</i></span>'
          '<span class="chip">Error<i>0</i></span><span class="chip">Omitido<i>38</i></span></div>'
          f'<span class="actions">{boton("Ver alertas (11)")}{boton("Continuar →", True)}</span></div>'
        + '<div class="vp">' + arbol()
        + panel('<div class="crumbs"><b>07/2026</b> › <b>MACRO 663</b> · fecha del asiento <b>31/07/2026</b></div>'
                + tabla(["Asiento en el Excel", "Concepto en el TXT", "Líneas", "Debe", "Haber", "Diferencia", "Estado", "Nº en el TXT"], filas, numericas=(2, 3, 4, 5), clases=clases)
                + '<p class="note">Orden de salida: mes → hoja (en el orden del Excel) → asiento. La numeración es correlativa, no se reinicia y los informativos no consumen número. '
                  'Hacé clic en un asiento para ver sus líneas.</p>', "Asientos de la hoja", num=None)
        + '</div>')
    return P("Vista previa", "generar", cuerpo, crumb="Asientos para el estudio / Vista previa")


def m02_detalle():
    filas = [
        ["214", "2.01.01.017", "RICARDO", "8.452.100,0049", "", "8.452.100,01", "", '<span class="adj">+0,01 · ajuste de centavo</span>'],
        ["215", "2.01.01.003", "PROVEEDORES VARIOS", "12.301.250,0044", "", "12.301.250,00", "", ""],
        ["216", "1.01.02.007", "MACRO 663", "", "10.376.675,0050", "", "10.376.675,01", ""],
        ["217", "4.01.05.012", "INTERESES GANADOS", "", "10.376.675,0043", "", "10.376.675,00", ""],
    ]
    filas.append(["", "", "<b>Totales</b>", "<b>20.753.350,0093</b>", "<b>20.753.350,0093</b>", "<b>20.753.350,01</b>", "<b>20.753.350,01</b>", '<span class="est ok">✓ cuadra</span>'])
    txt = ("31/07/2026<i>⇥</i>0085<i>⇥</i>2.01.01.017<i>⇥</i>8452100,01<i>⇥</i>0,00<i>⇥</i>07-2026-MACRO 663<i>⇥⇥⇥⇥⇥</i>\n"
           "31/07/2026<i>⇥</i>0085<i>⇥</i>2.01.01.003<i>⇥</i>12301250,00<i>⇥</i>0,00<i>⇥</i>07-2026-MACRO 663<i>⇥⇥⇥⇥⇥</i>\n"
           "31/07/2026<i>⇥</i>0085<i>⇥</i>1.01.02.007<i>⇥</i>0,00<i>⇥</i>10376675,01<i>⇥</i>07-2026-MACRO 663<i>⇥⇥⇥⇥⇥</i>\n"
           "31/07/2026<i>⇥</i>0085<i>⇥</i>4.01.05.012<i>⇥</i>0,00<i>⇥</i>10376675,00<i>⇥</i>07-2026-MACRO 663<i>⇥⇥⇥⇥⇥</i>")
    cuerpo = (cab(SUB) + pasos(1)
        + '<div class="crumbs"><a href="#">Vista previa</a> › 07/2026 › MACRO 663 › <b>ASIENTO 1</b></div>'
        + panel(kpis([("Nº en el TXT", "0085"), ("Fecha", "31/07/2026"), ("Concepto", "07-2026-MACRO 663"), ("Líneas", "4"), ("Estado", "⚠ Advertencia (A5)")])
                + tabla(["Fila del Excel", "Código", "Cuenta", "Debe (origen)", "Haber (origen)", "Debe (TXT)", "Haber (TXT)", "Nota"], filas, numericas=(3, 4, 5, 6)),
                "Líneas del asiento", "Importes de origen con 4 decimales: al redondear a 2 queda 0,01 de diferencia.")
        + aviso("<b>A5 · Descuadre por redondeo.</b> La suma sin redondear cuadra (20.753.350,0093 en cada lado) y tras redondear la diferencia es exactamente 0,01. "
                "Se ajustó <b>+0,01 en RICARDO</b>, la línea con mayor residuo de redondeo en la dirección correcta. Si preferís otra línea, se cambia desde el panel de alertas.")
        + panel('<div class="txt">' + txt + '</div><p class="note">Así sale en el archivo: 11 columnas separadas por TAB (⇥), coma decimal, sin separador de miles; Leyenda, Tipo, Centro de Costo, Fecha alternat. y Unidad vacíos.</p>',
                "Líneas tal como se escriben en el TXT")
        + f'<div class="sum"><span></span><span class="actions">{boton("← Volver a la vista previa")}{boton("Ver alerta A5")}</span></div>')
    return P("Detalle del asiento", "generar", cuerpo, crumb="Asientos para el estudio / Vista previa / Asiento")


def m02_vacio():
    cuerpo = (cab(SUB) + pasos(1)
        + archivo_chip("ASIENTOS VARIOS 10-2026.xlsx", "VARIOS · rango elegido 10/2026")
        + '<div style="height:16px"></div><section class="panel"><div class="vacio"><div class="big">∅</div><h2>No hay asientos registrables en el rango elegido</h2>'
          '<p>El mes 10/2026 tiene 6 asientos, pero ninguno se puede registrar.</p>'
          + tabla(["Qué se encontró", "Cantidad", "Alerta"], [["Asientos vacíos o en cero, omitidos", "6", '<span class="sev i">A8</span>'],
                                                              ["Bloque «ASIENTO QUE NO SE REGISTRA» (ejercicio anterior)", "1", '<span class="sev i">A10</span>']], numericas=(1,))
          + '<p style="margin-top:18px">' + boton("← Elegir otros meses", True) + " " + boton("Cargar otro archivo") + '</p></div></section>')
    return P("Vista previa", "generar", cuerpo, crumb="Asientos para el estudio / Vista previa")


def m02_error():
    celdas = [["COMPRAS", "05/2026", "118", "D118", "<span class='mono'>=+H14</span>", "Fórmula sin valor calculado guardado"],
              ["CAJAS", "07/2026", "37", "F37", "<span class='mono'>='[1]05-2026'!F25</span>", "Vínculo a otro libro sin valor guardado"],
              ["SUELDOS", "08/2026", "203", "C203", "<span class='mono'>=#REF!</span>", "La celda devuelve un error (#REF!)"]]
    cuerpo = (cab(SUB) + pasos(1)
        + archivo_chip("ASIENTOS VARIOS 05-2026 a 08-2026.xlsx", "VARIOS · lectura interrumpida")
        + '<div style="height:16px"></div>'
        + aviso("<b>No se puede armar la vista previa: 3 celdas de importe no tienen un valor calculado válido (A11 · bloqueante).</b><br>"
                "<b>Causa:</b> el archivo se guardó sin recalcular, o una fórmula depende de otro libro o de una referencia rota. La herramienta lee el valor guardado y no recalcula.<br>"
                "<b>Arreglo:</b> abrí el libro en Excel, recalculá (<span class='kbd'>F9</span>), corregí o reemplazá por el valor las celdas de abajo, guardá y volvé a cargarlo.", True)
        + panel(tabla(["Hoja", "Mes", "Fila", "Celda", "Contenido", "Problema"], celdas, numericas=(2,))
                + '<p class="note">Además, 1 nota de texto con vínculo externo en CHEQUES ENDOSADOS (K12) se ignoró: no es un importe.</p>', "Celdas a corregir")
        + f'<div class="sum"><span></span><span class="actions">{boton("Cargar otro archivo")}{boton("Reintentar lectura", True)}</span></div>')
    return P("Vista previa", "generar", cuerpo, crumb="Asientos para el estudio / Vista previa")


# ================================================================ 03 · ALERTAS
def alertas_filtros(b, a, i, sel="Todas"):
    def c(t, n):
        return f'<span class="chip{" on" if t == sel else ""}">{t}<i>{n}</i></span>'
    return (f'<div class="toolbar"><div class="chips">{c("Todas", b + a + i)}{c("Bloqueantes", b)}{c("Advertencias", a)}{c("Informativas", i)}</div>'
            f'<span class="actions"><select style="width:190px"><option>Todos los códigos</option></select>{boton("Exportar listado (CSV)")}</span></div>')


COLS_AL = ["Severidad", "Cód.", "Alerta", "Hoja", "Mes", "Asiento", "Fila", "Detalle y causa", "Estado", ""]


def m03_alertas():
    f = []
    for n, h, c in A1_ROWS:
        f.append([sev("A"), "A1", "Línea sin código de cuenta", h, "05–08/2026", f"{c} líneas", "—",
                  f"La cuenta <b>{n}</b> no tiene código. <b>El TXT sale con la cuenta vacía</b> en esas líneas: completala en el Excel si querés que salga con código.",
                  '<span class="est a">Sin cuenta</span>', "<a href='#'>Ver líneas</a>"])
    f += [
        [sev("A"), "A3", "Fila de totales suelta", "SANTANDER", "05/2026", "1", "14", "Fila sin código ni nombre con Debe y Haber cargados a mano (488.708.350,94) dentro del rango de la suma. Se ignora.", '<span class="est ok">Ignorada</span>', "<a href='#'>Ver asiento</a>"],
        [sev("A"), "A5", "Descuadre por redondeo", "MACRO 663", "07/2026", "1", "214", "Diferencia de 0,01 tras redondear. Ajuste <span class='adj'>RICARDO +0,01</span>", '<span class="est ok">Ajustada</span>', "<a href='#'>Ver asiento</a>"],
        [sev("A"), "A5", "Descuadre por redondeo", "MACRO 663", "08/2026", "1", "203", "Diferencia de 0,01 tras redondear. Ajuste <span class='adj'>RICARDO +0,01</span>", '<span class="est ok">Ajustada</span>', "<a href='#'>Ver asiento</a>"],
        [sev("A"), "A5", "Descuadre por redondeo", "BNA CA", "07/2026", "1", "96", "Diferencia de 0,01 tras redondear. Ajuste <span class='adj'>IMP A LOS DEB Y CRED COMP −0,01</span>", '<span class="est ok">Ajustada</span>', "<a href='#'>Ver asiento</a>"],
        [sev("I"), "A7", "Asientos informativos excluidos", "14 hojas", "05–08/2026", "38", "—", "«INFORMATIVO — NO SE REGISTRA». No se valida su balance: algunos solo tienen Debe.", '<span class="est o">Excluidos</span>', "<a href='#'>Ver lista</a>"],
        [sev("I"), "A15", "Código repetido con nombres distintos", "—", "—", "—", "—", "2.01.04.005 se usa para 3 nombres de IVA crédito fiscal. Es normal; solo se muestra en el detalle.", '<span class="est o">Sin acción</span>', ""],
    ]
    cl = ["adv"] * 9 + ["inf"] * 2
    cuerpo = (cab(SUB) + pasos(2)
        + '<div class="banner warn"><span><b>0 alertas bloqueantes: podés generar el TXT.</b> Hay 15 líneas sin código de cuenta: van a salir con la cuenta vacía. '
          'Las otras 4 advertencias se resolvieron solas y quedan registradas en el historial.</span>' + boton("Continuar al resumen →", True) + '</div>'
        + alertas_filtros(0, 19, 2, "Todas")
        + panel(tabla(COLS_AL, f, clases=cl), "Alertas de BANCOS FCE 2027.xlsx", "Cada alerta dice dónde está (hoja, mes, asiento y fila del Excel), por qué y qué hacer. Se exportan para que el contador corrija el origen."))
    return P("Alertas", "generar", cuerpo, crumb="Asientos para el estudio / Alertas")


def m03_vacio():
    cuerpo = (cab(SUB) + pasos(2)
        + '<div class="banner ok"><span><b>Sin alertas.</b> La planilla no tiene nada para revisar.</span>' + boton("Continuar al resumen →", True) + '</div>'
        + alertas_filtros(0, 0, 0)
        + '<section class="panel"><div class="vacio"><div class="big">✓</div><h2>Todo en orden</h2>'
          '<p>Ningún asiento descuadrado, ninguna línea sin código y ningún importe sin valor.<br>Las validaciones se corrieron sobre 140 asientos y 965 líneas.</p></div></section>')
    return P("Alertas", "generar", cuerpo, crumb="Asientos para el estudio / Alertas")


A1_ROWS = [("COMAFI BURSATIL", "COMAFI", 3), ("MACRO SECURITIES", "MACRO 663 · MACRO 664", 2), ("SANTANDER VALORES", "SANTANDER", 3),
           ("PRESTAMO NBSF · A PRÉSTAMO NBSF", "NBSF", 5), ("MULTAS", "BNA CA · GALICIA", 2)]
assert sum(r[2] for r in A1_ROWS) == 15


def cuerpo_03c_fondo():
    f = []
    for n, mes, orden in [("IVA - Asiento principal", "05/2026", 1), ("IVA - Asiento principal", "08/2026", 1),
                          ("IVA CF no computable", "05/2026", 2), ("IVA CF no computable", "06/2026", 2)]:
        f.append([sev("B"), "A6", "Asiento descuadrado en el origen", mes.replace("/", "-"), mes, n, "—", "El Excel ya trae una diferencia de 0,01. No se ajusta en silencio.", '<span class="est a">Abierta</span>', boton("Resolver…")])
    f.append([sev("I"), "A8", "Asiento vacío omitido", "05-2026", "05/2026", "Ret. de 3eros. 1era quincena SUSS", "—", "Sin ninguna línea de cuenta. No consume número.", '<span class="est o">Omitido</span>', ""])
    return (cab(SUB) + pasos(2)
        + '<div class="banner bad"><span><b>No se puede generar: 4 asientos descuadrados en el origen.</b> Sin resolver: Debe 6.936.847.173,80 vs Haber 6.936.847.173,78.</span>'
          f'{boton("Continuar al resumen →", False, True)}</div>'
        + alertas_filtros(4, 0, 1, "Bloqueantes")
        + panel(tabla(["Severidad", "Cód.", "Alerta", "Hoja", "Mes", "Asiento", "Fila", "Detalle y causa", "Estado", ""], f, clases=["bloq"] * 4 + ["inf"]),
                "Alertas de ASIENTO DEVENGAMIENTOS.xlsx"))


def m03_error():
    return P("Alertas", "generar", cuerpo_03c_fondo(), crumb="Asientos para el estudio / Alertas")


def m03c_descuadre():
    lineas = [("2.01.04.001", "IVA DÉBITO FISCAL", "Haber", "812.446.903,27"), ("2.01.04.020", "A SALDO IVA A PAGAR", "Haber", "0,00 → +0,01"),
              ("1.01.05.003", "IVA CRÉDITO FISCAL", "Debe", "812.446.903,28")]
    cuerpo = ('<p class="note" style="margin-top:0"><b>IVA - Asiento principal · 05/2026</b> (hoja 05-2026, encabezado en la fila 7)</p>'
              + kpis([("Debe", "812.446.903,28"), ("Haber", "812.446.903,27"), ("Diferencia", "0,01"), ("Falta en", "el Haber")])
              + '<div class="opt"><label><input type="radio" name="o"> <span><b>a · Corregir en el Excel y recargar</b></span></label><small>La opción más limpia: el origen queda bien y no hay ajuste.</small></div>'
              + '<div class="opt on"><label><input type="radio" name="o" checked> <span><b>b · Ajustar el centavo en una línea que elijo</b> (sugerida)</span></label>'
                '<small>Se suma 0,01 al Haber de la línea elegida y queda registrada la decisión con tu usuario.</small>'
                '<div style="margin:10px 0 0 26px;max-width:470px"><select><option>2.01.04.020 · A SALDO IVA A PAGAR (sugerida)</option><option>2.01.04.001 · IVA DÉBITO FISCAL</option></select></div></div>'
              + '<div class="opt"><label><input type="radio" name="o"> <span><b>c · Generar igual, aceptando la alerta</b></span></label><small>El asiento sale descuadrado por 0,01 y el archivo no pasa «Debe = Haber». Queda registrado quién lo aceptó.</small></div>'
              + '<p class="note">Una diferencia mayor a 0,01 es siempre bloqueante: en ese caso solo existe la opción a.</p>')
    m = modal("Asiento descuadrado en el origen", "A6 · DEVENGAMIENTOS · 05/2026", cuerpo, boton("Cancelar") + boton("Aplicar decisión", True))
    return P("Alertas", "generar", cuerpo_03c_fondo(), crumb="Asientos para el estudio / Alertas", modal_html=m)


# ================================================================ 04 · RESUMEN
def resumen_base(generado=False, error=False):
    kp = kpis([("Asientos", "140"), ("Líneas", "965"), ("Total Debe", TOTAL_B), ("Total Haber", TOTAL_B), ("Diferencia", "0,00")])
    meses = tabla(["Mes", "Fecha de los asientos", "Asientos", "Líneas", "Numeración"],
                  [["05/2026", "31/05/2026", "34", "238", "0001 – 0034"], ["06/2026", "30/06/2026", "35", "241", "0035 – 0069"],
                   ["07/2026", "31/07/2026", "36", "245", "0070 – 0105"], ["08/2026", "31/08/2026", "35", "241", "0106 – 0140"]], numericas=(2, 3))
    valid = ('<ul class="checks"><li>Total Debe = total Haber del archivo</li><li>965 líneas con 11 columnas (10 tabuladores)</li>'
             '<li>Fechas válidas: último día real de cada mes (31/05, 30/06, 31/07, 31/08/2026)</li><li>Numeración correlativa 0001–0140, sin huecos ni repeticiones</li></ul>')
    alert = ('<dl class="kv"><dt>Bloqueantes</dt><dd>0</dd><dt>Advertencias</dt><dd>19 · 15 líneas sin cuenta y 4 ajustadas o ignoradas</dd><dt>Informativas</dt><dd>2 · 38 informativos excluidos</dd></dl>'
             '<p class="note"><a href="#">Ver el detalle</a> · todas quedan en el historial y se pueden exportar a CSV.</p>')
    dest = ('<div class="two"><label>Carpeta de destino<input value="D:\\DESARROLLOS CONTABLE\\HOMOLOGADOR\\ASIENTO CONTABLE ESTUDIO" readonly></label>'
            '<label>Nombre del archivo<input value="BANCOS FCE 2027 05-2026 a 08-2026.txt"' + (' class="err"' if error else '') + '></label></div>'
            '<p class="note">Windows-1252 (ANSI) · fin de línea CRLF · fecha dd/mm/aaaa · sin fila de títulos · numeración correlativa global. Se cambia en <a href="#">Configuración</a>.</p>')
    return kp, meses, valid, alert, dest


def m04_resumen():
    kp, meses, valid, alert, dest = resumen_base()
    cuerpo = (cab(SUB) + pasos(3) + kp
        + '<div class="notice warn"><b>15 líneas van a salir con la cuenta vacía (A1)</b>, en 9 asientos. El estudio va a tener que completarlas. <a href="#">Ver las líneas</a></div>'
        + '<div class="two">' + panel(valid, "Validaciones globales", "Se corren antes de escribir el archivo.") + panel(alert, "Alertas de esta generación") + '</div>'
        + panel(meses, "Qué va a contener el TXT", "Origen: BANCOS FCE 2027.xlsx · SHA-256 <span class='hash'>9f3c1d…a71e</span>")
        + panel(dest, "Destino")
        + f'<div class="sum"><span>Se va a generar <strong>1</strong> archivo con <strong>965</strong> líneas. Nada se escribe hasta que lo confirmes.</span><span class="actions">{boton("← Alertas")}{boton("Generar TXT", True)}</span></div>')
    return P("Resumen y generación", "generar", cuerpo, crumb="Asientos para el estudio / Resumen")


def m04_generado():
    kp, meses, valid, alert, dest = resumen_base()
    ok = (aviso("<b>✓ Archivo generado y guardado.</b> Quedó registrado en el <a href='#'>historial</a> con el usuario, los parámetros y las alertas.")
          + '<dl class="kv"><dt>Archivo</dt><dd>D:\\DESARROLLOS CONTABLE\\HOMOLOGADOR\\ASIENTO CONTABLE ESTUDIO\\BANCOS FCE 2027 05-2026 a 08-2026.txt</dd>'
            '<dt>Tamaño</dt><dd>76,4 KB · 965 líneas · Windows-1252 · CRLF</dd>'
            '<dt>SHA-256 del TXT</dt><dd><span class="hash">7a41c9d0e85f3b26a1d94c07b8e2f5a364d19c0b7e8a2f53d1c6b9047a8e3f52</span></dd>'
            '<dt>SHA-256 del Excel</dt><dd><span class="hash">9f3c1d84b0a27e65c3d1f9a8e4b70c52d6a1e93f8b47c20d5e6a91b3c8f4a71e</span></dd>'
            '<dt>Líneas sin cuenta</dt><dd>15 (A1) · listadas en el historial</dd><dt>Generado</dt><dd>12/11/2026 08:14 · M. Julián</dd></dl>'
          + f'<div class="actions" style="margin-top:16px">{boton("⬇ Descargar TXT", True)}{boton("Abrir carpeta")}{boton("Exportar alertas (CSV)")}{boton("Ver en el historial")}{boton("Generar otro")}</div>')
    cuerpo = (cab(SUB) + pasos(4) + kp
        + panel(ok, "Listo para enviar al estudio contable")
        + panel(meses, "Qué contiene el TXT"))
    return P("Resumen y generación", "generar", cuerpo, crumb="Asientos para el estudio / Resumen")


def m04_error():
    kp, meses, valid, alert, dest = resumen_base(error=True)
    avisos = (aviso("<b>A16 · Este Excel ya se generó antes con los mismos parámetros.</b><br>12/11/2026 08:14 por M. Julián · meses 05–08/2026 · 140 asientos · SHA-256 del TXT <span class='hash'>7a41c9…3f52</span> · <a href='#'>ver en el historial</a><br>"
                    "Generarlo de nuevo puede duplicar el envío al estudio.")
              + aviso("<b>Ya existe un archivo con ese nombre en la carpeta de destino:</b> BANCOS FCE 2027 05-2026 a 08-2026.txt (76,4 KB · modificado 12/11/2026 08:14).<br>"
                      "No se pisa sin confirmar.", True))
    acc = (f'<div class="actions" style="margin-top:6px"><label class="check"><input type="checkbox"> Entiendo que ya se generó y quiero generarlo de nuevo</label></div>'
           f'<div class="actions" style="margin-top:14px">{boton("Pisar el archivo existente")}{boton("Guardar como «… (2).txt»", True)}{boton("Cancelar")}</div>')
    cuerpo = (cab(SUB) + pasos(3) + kp + panel(avisos + acc, "Antes de generar")
        + '<div class="two">' + panel(valid, "Validaciones globales") + panel(alert, "Alertas de esta generación") + '</div>' + panel(dest, "Destino"))
    return P("Resumen y generación", "generar", cuerpo, crumb="Asientos para el estudio / Resumen")


# ================================================================ 05 · HISTORIAL
def m05_historial():
    f = [
        ["12/11/2026 08:14", "M. Julián", "BANCOS FCE 2027.xlsx<br><span class='mini'>BANCOS · 05–08/2026</span>", "140", "965", TOTAL_B, "0 aceptadas<br><span class='mini'>15 sin cuenta · 4 ajustadas</span>", "<span class='hash'>7a41c9…3f52</span>", "<a href='#'>Descargar</a> · <a href='#'>Alertas</a>"],
        ["10/11/2026 16:02", "M. Julián", "ASIENTOS VARIOS 05-2026 a 08-2026.xlsx<br><span class='mini'>VARIOS · 05–09/2026 (09 incluido a mano)</span>", "98", "600", "74.568.487.094,93", "0 aceptadas<br><span class='mini'>1 ajustada · A9 incluido</span>", "<span class='hash'>b20e6d…81c4</span>", "<a href='#'>Descargar</a> · <a href='#'>Alertas</a>"],
        ["10/11/2026 15:40", "M. Julián", "ASIENTOS VARIOS 05-2026 a 08-2026.xlsx<br><span class='mini'>VARIOS · 05–08/2026</span>", "93", "581", "71.914.422.835,05", "0 aceptadas<br><span class='mini'>1 ajustada</span>", "<span class='hash'>d97f03…5a2b</span>", "<a href='#'>Descargar</a> · <a href='#'>Alertas</a>"],
        ["06/11/2026 11:25", "M. Julián", "ASIENTO DEVENGAMIENTOS.xlsx<br><span class='mini'>DEVENGAMIENTOS · 05–08/2026</span>", "44", "156", "6.936.847.173,80", "0 aceptadas<br><span class='mini'>4 ajustadas (A6)</span>", "<span class='hash'>13aa58…e07d</span>", "<a href='#'>Descargar</a> · <a href='#'>Alertas</a>"],
    ]
    det = tabla(["Hoja", "Mes", "Nombre de la cuenta en el Excel", "Líneas sin cuenta"],
                [[h, "05–08/2026", n, str(c)] for n, h, c in A1_ROWS], numericas=(3,))
    cuerpo = (cab("Cada generación queda registrada: quién, cuándo, de qué archivo, con qué parámetros y qué alertas tuvo.", "Historial de generaciones")
        + '<div class="toolbar"><div class="chips"><span class="chip on">Todas<i>4</i></span><span class="chip">BANCOS<i>1</i></span><span class="chip">DEVENGAMIENTOS<i>1</i></span><span class="chip">VARIOS<i>2</i></span><span class="chip">Con líneas sin cuenta<i>1</i></span></div>'
          '<span class="actions"><input placeholder="Buscar por archivo o usuario" style="width:260px"></span></div>'
        + panel(tabla(["Fecha y hora", "Usuario", "Origen", "Asientos", "Líneas", "Total Debe = Haber", "Alertas", "TXT (SHA-256)", ""], f, numericas=(3, 4, 5), clases=["adv", "", "", ""])
                + '<div class="crumbs" style="margin-top:18px"><b>Líneas sin cuenta · 12/11/2026 08:14 · BANCOS FCE 2027.xlsx</b></div>' + det
                + '<p class="note">Las 15 líneas salieron con la cuenta vacía en el TXT. Queda registrado para saber qué se le mandó al estudio.</p>'))
    return P("Historial", "historial", cuerpo, crumb="Asientos para el estudio / Historial")


def m05_vacio():
    cuerpo = (cab("Cada generación queda registrada: quién, cuándo, de qué archivo, con qué parámetros y qué alertas tuvo.", "Historial de generaciones")
        + '<section class="panel"><div class="vacio"><div class="big">☰</div><h2>Todavía no se generó ningún archivo</h2>'
          '<p>Cuando generes el primer TXT va a aparecer acá, con sus parámetros y sus alertas.</p>' + boton("Generar asientos", True) + '</div></section>')
    return P("Historial", "historial", cuerpo, crumb="Asientos para el estudio / Historial")


# ================================================================ 06 · CONFIGURACIÓN
def config(error=False):
    carpeta = "D:\\DESARROLLOS CONTABLE\\HOMOLOGADOR\\ASIENTO CONTABLE" if error else "D:\\DESARROLLOS CONTABLE\\HOMOLOGADOR\\ASIENTO CONTABLE ESTUDIO"
    err_c = "La carpeta no existe o el servicio no tiene permiso de escritura." if error else ""
    estado = ('<div class="notice error"><b>Carpeta de destino no válida.</b><br><b>Dónde:</b> Carpeta de destino · <b>Causa:</b> la ruta no existe o el usuario del servicio no puede escribir en ella.<br>'
              '<b>Arreglo:</b> creala o corregí la ruta, y probá de nuevo. Mientras tanto no se puede generar a disco (la descarga sigue disponible).</div>') if error else ""
    cuerpo = (cab("Parámetros del archivo que se genera. Los valores de fábrica siguen el layout de Holistor Contable.", "Configuración")
        + estado
        + panel('<div class="two"><label>Carpeta de destino<input value="' + carpeta.replace("\\", "\\") + '"' + (' class="err"' if error else '') + '>'
                + (f'<div class="ferr">{err_c}</div>' if error else '<small>Se sugiere el nombre «&lt;origen&gt; &lt;mes desde&gt; a &lt;mes hasta&gt;.txt». Nunca se pisa un archivo sin confirmar.</small>') + '</label>'
                '<label>Tamaño máximo del Excel (MB)<input value="20" readonly><small>Se rechazan los archivos más grandes. Solo se aceptan .xlsx.</small></label></div>'
                '<div style="height:6px"></div><button>Probar carpeta</button>', "Destino y límites", num=1)
        + panel('<div class="grid3"><label>Codificación<select><option>Windows-1252 (ANSI)</option></select><small>Los conceptos llevan acentos. La otra opción es UTF-8.</small></label>'
                '<label>Formato de fecha<select><option>dd/mm/aaaa</option></select><small>Alternativa: aaaammdd.</small></label>'
                '<label>Numeración de asientos<select><option>Correlativo global, no se reinicia</option></select><small>Alternativa: reiniciar por fecha («número de orden dentro de la fecha» del layout).</small></label></div>'
                '<div style="height:14px"></div><div class="grid3"><label>Fila de títulos<select><option>No</option></select><small>El layout de Holistor no la lleva.</small></label>'
                '<label>Fin de línea<select><option>CRLF</option></select><small>Fijo: lo exige el formato.</small></label><label>Decimales<select><option>2, half-up por línea</option></select><small>Fijo: se calcula con decimal.</small></label></div>',
                "Formato del TXT", "Codificación, fecha, numeración y fila de títulos son supuestos a confirmar con el estudio: por eso son configurables.", num=2)
        + f'<div class="sum"><span></span><span class="actions">{boton("Descartar cambios")}{boton("Guardar configuración", True)}</span></div>')
    return P("Configuración", "config", cuerpo, crumb="Asientos para el estudio / Configuración")


def main():
    pantallas = {
        "01-carga": m01_carga, "01-carga-vacio": m01_vacio, "01-carga-error": m01_error, "01-carga-meses": m01_meses,
        "02-vista-previa": m02_vp, "02-vista-previa-detalle": m02_detalle, "02-vista-previa-vacio": m02_vacio, "02-vista-previa-error": m02_error,
        "03-alertas": m03_alertas, "03-alertas-vacio": m03_vacio, "03-alertas-error": m03_error,
        "03c-descuadre-origen": m03c_descuadre,
        "04-resumen": m04_resumen, "04-resumen-generado": m04_generado, "04-resumen-error": m04_error,
        "05-historial": m05_historial, "05-historial-vacio": m05_vacio,
        "06-configuracion": lambda: config(False), "06-configuracion-error": lambda: config(True),
    }
    solo = sys.argv[1:]
    ok = True
    for n, f in pantallas.items():
        if solo and n not in solo:
            continue
        ok &= escribir(n, f(), h=1080)  # alto mínimo: el menú lateral (15 opciones) no se corta en las pantallas cortas
    sys.exit(0 if ok else 1)


if __name__ == "__main__":
    main()
