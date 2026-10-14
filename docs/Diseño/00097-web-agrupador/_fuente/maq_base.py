# -*- coding: utf-8 -*-
# TAREA 00097 - Base común de las maquetas: estilo de la web real (web.css), menú lateral por permisos,
# formateo de importes es-AR (Línea 7) y exportación a PNG con Edge sin interfaz.
import os, subprocess, html as H

AQUI = os.path.dirname(os.path.abspath(__file__))
DOC = os.path.dirname(AQUI)
EDGE = r"C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe"


def fmt(v):
    """1234567.891 -> '1.234.567,89' (miles con punto, dos decimales, valor absoluto). Línea 7."""
    s = f"{abs(v):,.2f}"
    return s.replace(",", "X").replace(".", ",").replace("X", ".")


def monto(v):
    """Celda numérica de movimiento: absoluto, miles y decimales; cero en gris; nulo en blanco."""
    if v is None:
        return '<td class="numeric"></td>'
    cls = ' class="numeric cero"' if v == 0 else ' class="numeric"'
    return f'<td{cls}>{fmt(v)}</td>'


def dif(v):
    """La diferencia (C - D) es un resultado: conserva su signo."""
    return ("+" if v > 0 else "−" if v < 0 else "") + fmt(v)


EXTRA_CSS = """
body{background:#f3f5f7}
.sidebar nav a{padding:10px 14px;font-size:13px}
.sidebar nav span{font-size:16px;width:20px;text-align:center}
.nav-caption{padding:18px 14px 8px}
.sidebar{overflow:hidden}
.cards{display:grid;grid-template-columns:repeat(3,1fr);gap:16px;margin-bottom:26px}
.card{background:#fff;border:1px solid var(--line);border-radius:9px;padding:18px 20px;display:flex;gap:14px;min-height:104px}
.card .ico{width:40px;height:40px;border-radius:9px;background:#e6f2f0;color:#157a70;display:grid;place-items:center;font-size:19px;flex-shrink:0}
.card h3{margin:0 0 4px;font-size:15px} .card p{margin:0;font-size:12px;color:var(--muted);line-height:1.5}
.card.off{opacity:.55;background:#f7f9fa} .card.off .ico{background:#eceff1;color:#8a9aa3}
.lock{display:inline-block;margin-top:7px;font-size:10px;font-weight:700;color:#8a6a2a;background:#fff1d7;border-radius:4px;padding:3px 7px}
.grp{font-size:10px;letter-spacing:1.6px;font-weight:700;color:#647d87;margin:6px 0 12px}
.cero{color:#a9b6bd}
th.numeric{text-align:right}
.stats{display:flex;gap:26px;flex-wrap:wrap;margin:0 0 12px}
.stats div{font-size:12px;color:var(--muted)} .stats strong{display:block;font-size:19px;color:#183d4e;font-variant-numeric:tabular-nums;margin-top:2px}
.badge.bad{background:#fde7e3;color:#a2392a}
tr.sel td{background:#dff0ee} tr.pend td{background:#fff8e6}
.actions{display:flex;gap:10px;flex-wrap:wrap;align-items:center}
.split{display:grid;grid-template-columns:1fr 1.5fr;gap:18px}
.bar{height:12px;background:#e4ecee;border-radius:7px;overflow:hidden;margin:12px 0}
.bar i{display:block;height:100%;background:var(--teal)}
.stepper{display:flex;gap:26px;font-size:12px;color:#83939c;margin:6px 0 8px}
.stepper .done{color:#24764f;font-weight:600} .stepper .now{color:#157a70;font-weight:700}
.backdrop{position:fixed;inset:0;background:rgba(18,46,62,.45);display:grid;place-items:center;z-index:9}
.modal{background:#fff;border-radius:10px;box-shadow:0 18px 60px rgba(0,0,0,.35);padding:24px 26px;max-width:880px;width:calc(100% - 340px);max-height:80vh;overflow:hidden}
.modal h2{margin:0 0 4px} .modal .sub{color:var(--muted);font-size:12px;margin-bottom:14px}
.modal .foot{display:flex;justify-content:flex-end;gap:10px;margin-top:18px}
.err{border-color:#d9776a!important;background:#fff8f6!important} .ferr{color:#a2392a;font-size:11px;margin-top:2px;font-weight:400}
.grid2{display:grid;grid-template-columns:1fr 1fr;gap:18px}.grid3{display:grid;grid-template-columns:repeat(3,1fr);gap:16px}
.cols{columns:3;column-gap:20px;font-size:12px} .cols label{font-weight:400;flex-direction:row;align-items:center;gap:8px;margin-bottom:7px}
.pager{font-size:11px;color:var(--muted);padding:10px 4px}
.kbd{font-size:11px;background:#eef3f5;border:1px solid #d6e0e5;border-radius:4px;padding:1px 6px}
h1{font-size:26px}
.note{font-size:11px;color:var(--muted);margin-top:8px}
.sum{display:flex;justify-content:space-between;align-items:center;gap:16px;background:#f2f8f7;border:1px solid #cfe0de;border-radius:7px;padding:12px 16px;margin-top:14px;font-size:12px}
.sum strong{font-variant-numeric:tabular-nums;font-size:14px}
.ok{color:#24764f;font-weight:700} .no{color:#a2392a;font-weight:700}
"""

# (clave, glifo, texto, permiso)
NAV = [
    ("INICIO", [("inicio", "⌂", "Inicio", None)]),
    ("ARCA CLIENTE", [("arca-comp", "⇄", "Comprobantes", "ArcaOffline"),
                      ("arca-perf", "▤", "Perfiles de comprobantes", "ArcaPerfiles"),
                      ("arca-pyr", "⇄", "Percepciones y retenciones", "ArcaPyR"),
                      ("arca-pyr-perf", "▤", "Perfiles PyR", "ArcaPyR")]),
    ("AGRUPADOR DE CONCEPTOS", [("procesador", "▦", "Procesador", "AgrProcesador"),
                                ("homolog", "≡", "Homologaciones", "AgrHomologaciones"),
                                ("consulta", "⌕", "Consulta de movimientos", "AgrHomologaciones"),
                                ("ext", "⇄", "Conciliación externa", "AgrConciliacion"),
                                ("int", "↔", "Conciliación interna", "AgrConciliacionInterna"),
                                ("cuentas", "#", "Cuentas contables", "AgrCuentasContables"),
                                ("conceptos", "◇", "Conceptos estándar", "AgrCuentasContables")]),
]
TODOS = {p for _, items in NAV for _, _, _, p in items if p}


def sidebar(activo, permisos):
    out = ['<aside class="sidebar"><a href="#" class="brand"><span class="brand-mark">C</span><span>Conciliador<small>CONTABLE</small></span></a>']
    for grupo, items in NAV:
        visibles = [i for i in items if i[3] is None or i[3] in permisos]
        if not visibles:
            continue
        out.append(f'<div class="nav-caption">{grupo}</div><nav>')
        for clave, glifo, texto, _ in visibles:
            c = ' class="active"' if clave == activo else ''
            out.append(f'<a href="#"{c}><span>{glifo}</span> {texto}</a>')
        out.append('</nav>')
    out.append('<div class="sidebar-foot"><span class="status-dot"></span> M. Julián<small>Alvear Supermercados</small><a href="#">Cerrar sesión ↗</a></div></aside>')
    return "".join(out)


def pagina(titulo, activo, cuerpo, permisos=None, crumb="", modal=""):
    permisos = TODOS if permisos is None else permisos
    return f"""<!doctype html><html lang="es"><head><meta charset="utf-8"><title>{H.escape(titulo)}</title>
<link rel="stylesheet" href="web.css"><style>{EXTRA_CSS}</style></head><body>
<div class="app-shell">{sidebar(activo, permisos)}<div class="main-shell">
<header class="topbar"><span>Conciliador Contable <span class="muted"> / {crumb or titulo}</span></span><span class="pill">ENTORNO CONFIGURADO</span></header>
<main class="content">{cuerpo}</main></div></div>{modal}</body></html>"""


def heading(eyebrow, titulo, sub=""):
    p = f'<p>{sub}</p>' if sub else ''
    return f'<div class="page-heading"><div><p class="eyebrow">{eyebrow}</p><h1>{titulo}</h1>{p}</div></div>'


def panel(cuerpo, titulo="", sub="", num=None):
    cab = ""
    if titulo:
        n = f'<span class="step-number">{num}</span>' if num else ''
        s = f'<p>{sub}</p>' if sub else ''
        cab = f'<div class="section-heading">{n}<div><h2>{titulo}</h2>{s}</div></div>'
    return f'<section class="panel">{cab}{cuerpo}</section>'


def tabla(cols, filas, numericas=(), clases=None):
    """cols: encabezados; filas: listas de celdas (str con HTML) o ya <td>; numericas: índices de columnas de importe."""
    th = "".join(f'<th class="numeric">{c}</th>' if i in numericas else f'<th>{c}</th>' for i, c in enumerate(cols))
    trs = []
    for k, f in enumerate(filas):
        tds = []
        for i, c in enumerate(f):
            if isinstance(c, str) and c.startswith("<td"):
                tds.append(c)
            else:
                tds.append(f'<td class="numeric">{c}</td>' if i in numericas else f'<td>{c}</td>')
        cl = f' class="{clases[k]}"' if clases and clases[k] else ''
        trs.append(f'<tr{cl}>{"".join(tds)}</tr>')
    return f'<div class="table-wrap"><table><thead><tr>{th}</tr></thead><tbody>{"".join(trs)}</tbody></table></div>'


def campo(etq, valor, err="", tipo="input", ayuda=""):
    e = f'<div class="ferr">{err}</div>' if err else ''
    c = ' class="err"' if err else ''
    a = f'<small>{ayuda}</small>' if ayuda else ''
    if tipo == "select":
        return f'<label>{etq}<select{c}><option>{valor}</option></select>{a}{e}</label>'
    return f'<label>{etq}<input{c} value="{valor}" readonly>{a}{e}</label>'


def aviso(texto, error=False):
    return f'<div class="notice{" error" if error else ""}">{texto}</div>'


def boton(t, primario=False, off=False):
    c = " primary" if primario else ""
    d = " disabled" if off else ""
    return f'<button class="{c.strip()}"{d}>{t}</button>'


def modal(titulo, sub, cuerpo, pie):
    return f'<div class="backdrop"><div class="modal"><h2>{titulo}</h2><div class="sub">{sub}</div>{cuerpo}<div class="foot">{pie}</div></div></div>'


def escribir(nombre, html_texto, w=1440, h=900):
    ruta = os.path.join(AQUI, nombre + ".html")
    with open(ruta, "w", encoding="utf-8") as f:
        f.write(html_texto)
    png = os.path.join(DOC, nombre + ".png")
    if os.path.exists(png):
        os.remove(png)
    subprocess.run([EDGE, "--headless=new", "--disable-gpu", "--hide-scrollbars", "--force-device-scale-factor=1",
                    f"--window-size={w},{h}", f"--screenshot={png}", "file:///" + ruta.replace("\\", "/")],
                   timeout=90, stdout=subprocess.DEVNULL, stderr=subprocess.DEVNULL)
    ok = os.path.exists(png) and os.path.getsize(png) > 4000
    print(("OK  " if ok else "FALLA ") + nombre + ".png")
    return ok


# ---------------- datos de ejemplo (BNA CA, septiembre de 2026) ----------------
# fecha, concepto original, estándar, débito, crédito, concepto final, cuenta
MOV = [
    ("01/09/2026", "PAGO CON TRANSF-BANE", "TRANSFERENCIAS RECIBIDAS", 0, 58049.41, "TRANSFERENCIAS RECIBIDAS", "110201"),
    ("01/09/2026", "PAGO CON TRANSF-BANE", "TRANSFERENCIAS RECIBIDAS", 0, 6351.55, "TRANSFERENCIAS RECIBIDAS", "110201"),
    ("01/09/2026", "DB.TRANSF.INTER-LINK", "TRANSF ANTONIAZZI", 5000000.00, 0, "TRANSF ANTONIAZZI", "110105"),
    ("01/09/2026", "DEVOL. PAGO CON-LINK", "DEVOLUCIONES DE COBROS", 34980.31, 0, "DEVOLUCIONES DE COBROS", "410330"),
    ("02/09/2026", "GRAVAMEN LEY 25413/DEB", "IMPUESTO A LOS DÉBITOS Y CRÉDITOS", 3412.18, 0, "IMPUESTO A LOS DÉBITOS Y CRÉDITOS", "520115"),
    ("02/09/2026", "CREDITO POR CONCEP.VARIOS", "Pendiente de homologar", 0, 1250000.00, "", ""),
    ("02/09/2026", "DB.TRANSF.INTER-LINK", "TRANSF ANTONIAZZI", 7500000.00, 0, "TRANSF ANTONIAZZI", "110105"),
    ("03/09/2026", "I.V.A. BASE", "IVA SOBRE COMISIONES", 716.57, 0, "IVA SOBRE COMISIONES", "520120"),
    ("03/09/2026", "COM.P/DB.TRANSF.INTERPYME", "COMISIONES BANCARIAS", 3412.00, 0, "COMISIONES BANCARIAS", "520118"),
    ("03/09/2026", "DEB. TRANSF.ELE-SUC 0001", "TRANSF ANTONIAZZI", 12000000.00, 0, "TRANSF ANTONIAZZI", "110105"),
    ("04/09/2026", "PAGO CON TRANSF-BANE", "TRANSFERENCIAS RECIBIDAS", 0, 24317.53, "TRANSFERENCIAS RECIBIDAS", "110201"),
    ("04/09/2026", "D.TR.INT CTA.DI-SUC 0001", "Pendiente de homologar", 2300000.00, 0, "", ""),
]
ARCHIVO = "BNA 09-2026 A MANO MERI.xlsx"
