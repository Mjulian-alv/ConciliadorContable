# Fecha: 21/09/2026 - TAREA: 00041 - Linea: 2, 3, 4 - Generador de las maquetas de Conciliación PyR
# Arma un HTML por maqueta (estilo ventana WinForms/Telerik) con filas reales de julio 2026
# tomadas de datos-ejemplo.json, y los exporta a PNG con Chrome headless.
# Uso: python maquetas.py   (desde cualquier carpeta)
import io, json, os, subprocess, html

AQUI = os.path.dirname(os.path.abspath(__file__))
DEST = os.path.dirname(AQUI)
CHROME = r"C:\Program Files\Google\Chrome\Application\chrome.exe"
D = json.load(io.open(os.path.join(AQUI, "datos-ejemplo.json"), encoding="utf-8"))
e = html.escape

CSS = """
*{box-sizing:border-box} body{margin:0;padding:18px;background:#dfe3e8;font:12px 'Segoe UI',Tahoma,sans-serif;color:#1e1e1e}
.win{background:#f3f3f3;border:1px solid #7a8a99;box-shadow:0 4px 18px rgba(0,0,0,.25);display:inline-block;position:relative}
.tb{background:linear-gradient(#2f6fb3,#255b95);color:#fff;padding:6px 10px;font-weight:600;display:flex;justify-content:space-between}
.tb span.x{letter-spacing:10px;font-weight:400}
.body{padding:12px}
fieldset{border:1px solid #b9c3cd;margin:0 0 10px;padding:8px 10px 10px;background:#fafafa}
legend{color:#255b95;font-weight:600;padding:0 4px}
.row{display:flex;gap:10px;align-items:center;margin:4px 0}
label{min-width:110px;color:#333}
input,select{font:12px 'Segoe UI';border:1px solid #9aa7b4;background:#fff;padding:3px 5px;height:23px}
input.err{border:2px solid #d13438;background:#fff4f4}
.btn{display:inline-block;border:1px solid #8a99a8;background:linear-gradient(#fdfdfd,#e4e8ec);padding:4px 12px;border-radius:2px;min-width:78px;text-align:center}
.btn.pri{background:linear-gradient(#3b82c9,#255b95);color:#fff;border-color:#1f4d7e;font-weight:600}
.btn.dis{color:#9aa0a6;background:#eceef0;border-color:#c5cbd1}
.btn.big{padding:8px 26px;font-weight:700}
table.g{border-collapse:collapse;background:#fff;width:100%;border:1px solid #9aa7b4}
table.g th{background:linear-gradient(#f4f6f8,#dde3e9);border:1px solid #b9c3cd;padding:4px 6px;text-align:left;font-weight:600;white-space:nowrap}
table.g td{border:1px solid #e1e5ea;padding:3px 6px;white-space:nowrap}
table.g tr.sel td{background:#cfe3f7}
table.g tr.sin td{background:#fff3c4}
table.g tr.dup td{background:#fde2e2;color:#8b1a1a}
td.n{text-align:right;font-variant-numeric:tabular-nums}
.empty{padding:26px;text-align:center;color:#7a8490;background:#fff;border:1px solid #9aa7b4;font-style:italic}
.st{font-weight:600} .ok{color:#107c10} .warn{color:#a15c00} .bad{color:#c50f1f} .mut{color:#7a8490}
.chip{display:inline-block;border:1px solid #b9c3cd;background:#fff;border-radius:10px;padding:1px 9px;margin-right:6px}
.foot{display:flex;justify-content:flex-end;gap:8px;margin-top:6px}
.msg{position:absolute;left:50%;top:50%;transform:translate(-50%,-50%);width:430px;background:#fff;border:1px solid #6b7b8b;box-shadow:0 8px 30px rgba(0,0,0,.35)}
.msg .c{padding:16px 18px;display:flex;gap:14px} .ico{font-size:30px;line-height:1}
.msg .f{background:#f0f0f0;padding:8px;text-align:right}
.shade{position:absolute;inset:0;background:rgba(255,255,255,.35)}
.tabs{display:flex;gap:2px;margin-bottom:-1px} .tab{border:1px solid #b9c3cd;border-bottom:none;padding:4px 14px;background:#e4e8ec}
.tab.on{background:#fff;font-weight:600}
.hint{color:#5c6773;font-size:11px;margin-top:3px}
"""

def pagina(nombre, titulo, cuerpo, ancho):
    doc = f"""<!doctype html><html lang="es"><head><meta charset="utf-8"><title>{e(titulo)}</title>
<style>{CSS}</style></head><body><div class="win" style="width:{ancho}px">
<div class="tb"><span>{e(titulo)}</span><span class="x">– ▢ ✕</span></div><div class="body">{cuerpo}</div></div></body></html>"""
    ruta = os.path.join(AQUI, nombre + ".html")
    io.open(ruta, "w", encoding="utf-8").write(doc)
    return ruta

def grilla(cols, filas, clases=None, num=()):
    h = "".join(f"<th>{e(c)}</th>" for c in cols)
    b = ""
    for i, f in enumerate(filas):
        cl = (clases or {}).get(i, "")
        b += f'<tr class="{cl}">' + "".join(
            f'<td class="{"n" if j in num else ""}">{e(str(v))}</td>' for j, v in enumerate(f)) + "</tr>"
    return f'<table class="g"><tr>{h}</tr>{b}</table>'

# ── 01 Detalle de perfil ────────────────────────────────────────────────────
CUENTAS = [("114105", "Percepciones IVA", "Percepción", "IVA"),
           ("114110", "Percepciones IIBB Santa Fe", "Percepción", "IIBB"),
           ("114115", "Retenciones IIBB Santa Fe", "Retención", "IIBB")]

def perfil(cuentas, nombre="Supermercado — Percepciones y Retenciones", clases=None, error=None):
    cab = f"""
<div class="row"><label>Nombre del perfil:</label><input style="width:420px" value="{e(nombre)}"></div>
<fieldset><legend>Mayor de PRESEA (archivo)</legend>
 <div class="row"><label>Tipo de archivo:</label><select style="width:150px"><option>Excel (.xlsx)</option></select>
  <label style="min-width:60px">Hoja:</label><input style="width:140px" placeholder="(primera hoja)">
  <label style="min-width:0"><input type="checkbox" checked style="height:auto"> Tiene cabecera</label></div>
 <div class="row"><label>Formato de fecha:</label><input style="width:150px" value="dd/MM/yyyy">
  <label style="min-width:60px">Decimal:</label><select style="width:140px"><option>. (punto)</option></select></div>
</fieldset>
<fieldset><legend>Columnas del mayor (nombre del encabezado)</legend>
 <div class="row"><label>Fecha:</label><input style="width:150px" value="fecha"><label style="min-width:70px">Asiento:</label><input style="width:150px" value="asiento"></div>
 <div class="row"><label>Concepto:</label><input style="width:150px" value="concepto"><label style="min-width:70px">Debe:</label><input style="width:150px" value="debe">
  <label style="min-width:50px">Haber:</label><input style="width:120px" value="haber"></div>
 <div class="hint">El concepto trae tipo, número y proveedor juntos: «SEGUN FACTURA A    36900627623 de CIA INDUSTRIAL C».</div>
</fieldset>"""
    if cuentas:
        g = grilla(["Código", "Nombre", "Tipo", "Impuesto"], cuentas, clases)
    else:
        g = '<div class="empty">Sin cuentas. Agregá al menos una para poder levantar los mayores de PRESEA.</div>'
    cue = f"""<fieldset><legend>Cuentas de percepciones y retenciones</legend>
 <div style="display:flex;gap:10px"><div style="flex:1">{g}</div>
 <div style="display:flex;flex-direction:column;gap:6px"><span class="btn">Agregar…</span>
  <span class="btn {'' if cuentas else 'dis'}">Editar…</span><span class="btn {'' if cuentas else 'dis'}">Quitar</span></div></div>
</fieldset>"""
    pie = '<div class="foot"><span class="btn pri">Guardar</span><span class="btn">Cancelar</span></div>'
    extra = ""
    if error:
        extra = f'<div class="shade"></div><div class="msg"><div class="tb"><span>Perfil PyR</span><span class="x">✕</span></div><div class="c"><div class="ico bad">⛔</div><div>{error}</div></div><div class="f"><span class="btn">Aceptar</span></div></div>'
    return cab + cue + pie + extra

s = []
s.append(("01-perfil-pyr-detalle", pagina("01-perfil-pyr-detalle", "Perfil PyR — Detalle", perfil(CUENTAS, clases={0: "sel"}), 760)))
s.append(("01-perfil-pyr-detalle-vacio", pagina("01-perfil-pyr-detalle-vacio", "Perfil PyR — Nuevo", perfil([], nombre=""), 760)))
dup = CUENTAS + [("114105", "Retenciones IVA", "Retención", "IVA")]
s.append(("01-perfil-pyr-detalle-error", pagina("01-perfil-pyr-detalle-error", "Perfil PyR — Detalle",
    perfil(dup, clases={0: "dup", 3: "dup"}, error="No se puede guardar el perfil.<br><br>El código de cuenta <b>114105</b> está repetido:<br>· Percepciones IVA<br>· Retenciones IVA<br><br>Cada cuenta del perfil tiene que tener un código distinto."), 760)))

# ── 02 Diálogo de cuenta ───────────────────────────────────────────────────
c2 = """
<div class="row"><label>Código de cuenta:</label><input style="width:140px" value="114105"></div>
<div class="row"><label>Nombre:</label><input style="width:260px" value="Percepciones IVA"></div>
<div class="row"><label>Tipo:</label><label style="min-width:0"><input type="radio" checked style="height:auto"> Percepción</label>
 <label style="min-width:0"><input type="radio" style="height:auto"> Retención</label></div>
<div class="row"><label>Impuesto:</label><select style="width:140px"><option>IVA</option></select><span class="mut">IVA · IIBB · Ganancias</span></div>
<div class="hint" style="margin-top:8px">El código es el de la cuenta contable en PRESEA. No puede repetirse dentro del perfil.</div>
<div class="foot" style="margin-top:12px"><span class="btn pri">Aceptar</span><span class="btn">Cancelar</span></div>"""
s.append(("02-cuenta-pyr", pagina("02-cuenta-pyr", "Cuenta de percepción / retención", c2, 440)))

# ── 03 Elegir cuenta al agregar un mayor ───────────────────────────────────
c3 = """
<div class="row"><label>Archivo:</label><b>PERC. IVA 07-2026.XLSX</b></div>
<div class="row"><label>¿De qué cuenta es?</label><select style="width:330px"><option>114105 — Percepciones IVA (Percepción · IVA)</option></select></div>
<table class="g" style="margin:4px 0 0 120px;width:330px"><tr class="sel"><td>114105 — Percepciones IVA (Percepción · IVA)</td></tr>
<tr><td>114110 — Percepciones IIBB Santa Fe (Percepción · IIBB)</td></tr><tr><td>114115 — Retenciones IIBB Santa Fe (Retención · IIBB)</td></tr></table>
<div class="hint" style="margin-left:120px">Si la cuenta ya tiene un mayor cargado, se pregunta si se reemplaza.</div>
<div class="foot" style="margin-top:12px"><span class="btn pri">Aceptar</span><span class="btn">Cancelar</span></div>"""
s.append(("03-elegir-cuenta", pagina("03-elegir-cuenta", "Agregar mayor de PRESEA", c3, 520)))

# ── 04 Conciliación PyR ────────────────────────────────────────────────────
CARPETA = r"D:\DESARROLLOS CONTABLE\CONCILIADOR\PERCEP Y RETEN\arca"
COLS_A = ["Impuesto", "Operación", "Fecha", "CUIT", "Denominación", "Tipo", "Letra", "Número", "Importe", "Archivo"]
COLS_P = ["Cuenta", "Fecha", "Asiento", "Tipo", "Número", "Proveedor", "Anul.", "Importe", "Concepto original"]

def conc(estado):
    cargado = estado == "ok"
    err = estado == "error"
    if cargado:
        st_a = '<span class="st ok">2 archivos · 3.851 registros</span>'
        res = '<span class="chip">IVA · Percepción <b>2.569</b></span><span class="chip">IIBB · Percepción <b>890</b></span><span class="chip">IIBB · Retención <b>392</b></span>'
    elif err:
        st_a = '<span class="st warn">⚠ 1 archivo cargado · 1 no reconocido: <u>Ganancias 07-2026.pdf.xls</u></span>'
        res = '<span class="chip">IIBB · Percepción <b>890</b></span><span class="chip">IIBB · Retención <b>392</b></span>'
    else:
        st_a = '<span class="st mut">Sin cargar</span>'; res = ""
    arca = f"""<fieldset style="flex:1"><legend>ARCA — carpeta de certificados</legend>
 <div class="row"><label style="min-width:55px">Carpeta:</label><input style="flex:1" value="{e(CARPETA)}"><span class="btn" style="min-width:30px">…</span>
 <span class="btn pri">Cargar carpeta</span></div>
 <div class="row">{st_a}</div><div class="row">{res}</div></fieldset>"""
    if cargado:
        lista = grilla(["Archivo", "Cuenta", "Filas", "Sin comprobante"],
                       [("PERC. IVA 07-2026.XLSX", "114105 — Percepciones IVA", "2.056", "17"),
                        ("PERC. IIBB 07-2026.XLSX", "114110 — Percepciones IIBB Santa Fe", "984", "0")], {0: "sel"}, num=(2, 3))
        st_p = '<span class="st ok">2 mayores · 3.040 filas · 17 sin comprobante</span>'
    elif err:
        lista = grilla(["Archivo", "Cuenta", "Filas", "Sin comprobante"],
                       [("PERC. IIBB 07-2026.XLSX", "114110 — Percepciones IIBB Santa Fe", "984", "0")], num=(2, 3))
        st_p = '<span class="st bad">✖ MAYOR RET 07-2026.XLSX no se agregó: faltan las columnas asiento, haber</span>'
    else:
        lista = '<div class="empty" style="padding:14px">Todavía no se agregó ningún mayor.</div>'
        st_p = '<span class="st mut">Sin cargar</span>'
    pres = f"""<fieldset style="flex:1"><legend>PRESEA — mayores por cuenta</legend>
 <div style="display:flex;gap:8px"><div style="flex:1">{lista}</div><div style="display:flex;flex-direction:column;gap:6px">
 <span class="btn pri">Agregar…</span><span class="btn {'' if (cargado or err) else 'dis'}">Quitar</span></div></div>
 <div class="row">{st_p}</div></fieldset>"""
    arriba = f'<div style="display:flex;gap:10px">{arca}{pres}</div>'
    if cargado:
        ga = grilla(COLS_A, [a[:9] + [a[9]] for a in D["arca"]], num=(8,))
        pr = [p[:8] + [p[9]] for p in D["pres"]]
        gp = grilla(COLS_P, pr, {i: "sin" for i, p in enumerate(D["pres"]) if p[8]}, num=(7,))
        abajo = f"""<div style="display:flex;gap:10px"><div style="flex:1;min-width:0">
 <div class="tabs"><span class="tab on">Registros ARCA (3.851)</span></div>{ga}</div></div>
 <div style="margin-top:10px"><div class="tabs"><span class="tab on">Mayor PRESEA (3.040)</span><span class="tab"><span style="background:#fff3c4;border:1px solid #d9b84a;padding:0 6px">&nbsp;</span> sin comprobante (17)</span></div>{gp}</div>"""
    elif err:
        ga = grilla(COLS_A, [a[:9] + [a[9]] for a in D["arca"] if a[0] == "IIBB"], num=(8,))
        abajo = f'<div class="tabs"><span class="tab on">Registros ARCA (1.282)</span></div>{ga}<div style="margin-top:10px"><div class="tabs"><span class="tab on">Mayor PRESEA (984)</span></div><div class="empty">(filas del mayor de IIBB)</div></div>'
    else:
        abajo = '<div class="tabs"><span class="tab on">Registros ARCA</span></div><div class="empty">Cargá la carpeta de ARCA para ver los certificados.</div><div style="margin-top:10px"><div class="tabs"><span class="tab on">Mayor PRESEA</span></div><div class="empty">Agregá los mayores de PRESEA eligiendo a qué cuenta corresponde cada uno.</div></div>'
    pie = """<div class="row" style="justify-content:space-between;margin-top:10px">
 <div class="row"><label style="min-width:0">Perfil:</label><select style="width:320px"><option>Supermercado — Percepciones y Retenciones</option></select></div>
 <div class="row"><span class="mut">Directivas y conciliación: próxima etapa</span><span class="btn big dis">CONCILIAR</span></div></div>"""
    return arriba + abajo + pie

TIT = "ARCA Cliente — Conciliación Percepciones y Retenciones"
s.append(("04-conciliacion-pyr", pagina("04-conciliacion-pyr", TIT, conc("ok"), 1500)))
s.append(("04-conciliacion-pyr-vacio", pagina("04-conciliacion-pyr-vacio", TIT, conc("vacio"), 1500)))
s.append(("04-conciliacion-pyr-error", pagina("04-conciliacion-pyr-error", TIT, conc("error"), 1500)))

# ── Segunda etapa (Líneas 6 y 7) ─────────────────────────────────────────────
# Fecha: 21/09/2026 - TAREA: 00041 - Linea: 6, 7 - Maquetas de directivas y del resultado de la conciliación
SIM = json.load(io.open(os.path.join(AQUI, "simulacion-julio.json"), encoding="utf-8"))
CSS2 = """
tr.e-ok td{background:#aaf0d1} tr.e-dif td{background:#fef0ba} tr.e-sa td{background:#ffcdd2}
tr.e-sp td{background:#ced4ed} tr.e-an td{background:#e4e4e4;color:#555}
tr.fija td{background:#dcebff;color:#00008b;font-weight:600}
.filtro span{display:inline-block;border:1px solid #9aa7b4;background:#fff;padding:2px 10px;margin-right:4px;border-radius:2px}
.filtro span.on{background:#255b95;color:#fff;border-color:#1f4d7e}
.aviso{background:#fff8e1;border:1px solid #e0c46c;padding:4px 8px;margin:6px 0;color:#7a5a00}
td.hdr{background:#eef2f6;font-weight:600}
"""
CSS += CSS2

DIRS = [("1", "Número completo", "Número"),
        ("2", "Últimos 8 dígitos + Importe", "Número (últimos 8) + Importe"),
        ("3", "Importe + Fecha + Proveedor", "Importe + Fecha + Proveedor")]

def directivas(error=False):
    g = grilla(["#", "Descripción", "Campos"], [d for d in DIRS] + ([("4", "Nueva directiva", "(sin campos)")] if error else []),
               {0: "fija", **({3: "dup sel"} if error else {1: "sel"})})
    btns = "".join(f'<span class="btn {c}">{t}</span>' for t, c in
                   [("Agregar...", ""), ("Editar...", ""), ("Eliminar", ""), ("▲ Subir", "dis"), ("▼ Bajar", ""), ("Restablecer predeterminadas", "")])
    cuerpo = f"""<div class="hint" style="margin-bottom:6px">Se aplican en orden: cada una trabaja sobre lo que las anteriores no emparejaron. La primera es fija.</div>
{g}<div class="row" style="margin-top:8px;flex-wrap:wrap">{btns}</div>
<div class="foot" style="margin-top:10px"><span class="btn pri">Aceptar</span><span class="btn">Cancelar</span></div>"""
    if error:
        cuerpo += '<div class="shade"></div><div class="msg"><div class="tb"><span>Directivas PyR</span><span class="x">✕</span></div><div class="c"><div class="ico bad">⛔</div><div>La directiva 4 «Nueva directiva» no tiene campos.<br><br>Elegí al menos un campo o eliminala.</div></div><div class="f"><span class="btn">Aceptar</span></div></div>'
    return cuerpo

s.append(("05-directivas-pyr", pagina("05-directivas-pyr", "Directivas de conciliación — Supermercado — Percepciones y Retenciones", directivas(), 720)))
s.append(("05-directivas-pyr-error", pagina("05-directivas-pyr-error", "Directivas de conciliación — Supermercado — Percepciones y Retenciones", directivas(True), 720)))

campos = [("Número (completo)", False), ("Número (últimos 8 dígitos)", True), ("Importe", True), ("Fecha", False), ("Proveedor (primeras 6 letras)", False)]
det = """<div class="row"><label>Descripción:</label><input style="width:320px" value="Últimos 8 dígitos + Importe"></div>
<fieldset style="margin-top:8px"><legend>Campos que tienen que coincidir</legend>""" + "".join(
    f'<div class="row"><label style="min-width:0"><input type="checkbox" {"checked" if c else ""} style="height:auto"> {e(n)}</label></div>' for n, c in campos) + """
<div class="hint">Si a una fila le falta alguno de estos valores (por ejemplo, una minuta no tiene número), esta directiva no la empareja.</div></fieldset>
<div class="foot"><span class="btn pri">Aceptar</span><span class="btn">Cancelar</span></div>"""
s.append(("06-directiva-pyr-detalle", pagina("06-directiva-pyr-detalle", "Directiva de conciliación", det, 480)))

def m(x): return "" if x is None else f"{x:,.2f}".replace(",", "X").replace(".", ",").replace("X", ".")
CLS = {"Conciliado": "e-ok", "Diferencia de importe": "e-dif", "Sólo ARCA": "e-sa", "Sólo PRESEA": "e-sp", "Anulada en PRESEA": "e-an"}
EST = ["Conciliado", "Diferencia de importe", "Sólo ARCA", "Sólo PRESEA", "Anulada en PRESEA"]

def resultado(estado):
    arriba = conc("ok").split('<div style="display:flex;gap:10px"><div style="flex:1;min-width:0">')[0]
    total = sum(sum(v for k, v in r.items() if not k.startswith("_")) for r in SIM["resumen"].values())
    tabs = f'<div class="tabs"><span class="tab">Mayor PRESEA (3.040)</span><span class="tab">Sin comprobante (17)</span><span class="tab on">Conciliación{f" ({total:,})".replace(",", ".") if estado == "ok" else ""}</span></div>'
    if estado == "vacio":
        cuerpo = '<div class="empty" style="padding:60px">Todavía no se concilió. Tocá CONCILIAR para cruzar ARCA con los mayores cargados.</div>'
    elif estado == "error":
        cuerpo = '<div class="empty" style="padding:60px;color:#a15c00;font-style:normal">⚠ Los datos cambiaron (se agregó el mayor «PERC. IIBB 07-2026.XLSX»): volvé a conciliar.</div>'
    else:
        filas = []
        for cta, r in SIM["resumen"].items():
            filas.append([cta] + [f'{r.get(k, 0):,}'.replace(",", ".") for k in EST] + [m(r["_arca"]), m(r["_presea"]), m(round(r["_arca"] - r["_presea"], 2))])
        resumen = grilla(["Cuenta", "Conciliados", "Diferencias", "Sólo ARCA", "Sólo PRESEA", "Anuladas", "Total ARCA", "Total PRESEA", "Diferencia"], filas, num=(1, 2, 3, 4, 5, 6, 7, 8))
        fuera = " · ".join(f"{n} registros de ARCA fuera de alcance: {g}" for g, n in SIM["fuera"].items())
        pd_ = "   ".join(f"Dir. {k.split('-')[0]}: {v:,}".replace(",", ".") for k, v in sorted(SIM["porDir"].items()))
        filtro = '<div class="row filtro"><label style="min-width:0">Mostrar:</label>' + "".join(
            f'<span class="{"on" if t == "Todos" else ""}">{t}</span>' for t in ["Todos"] + EST) + f'<span style="border:none;background:none" class="mut">Emparejados por {pd_}</span></div>'
        det = []
        clases = {}
        for i, x in enumerate(SIM["muestra"]):
            clases[i] = CLS[x["estado"]]
            det.append([x["estado"], x["cuenta"], x["dir"], x["fa"], x["cuit"], x["den"], x["tipo"], x["na"], m(x["ia"]),
                        x["fp"], x["asi"], x["np"], x["prov"], m(x["ip"]), m(x["dif"])])
        grid = grilla(["Estado", "Cuenta", "Dir.", "Fecha ARCA", "CUIT", "Denominación", "Tipo", "Número ARCA", "Importe ARCA",
                       "Fecha PRESEA", "Asiento", "Número PRESEA", "Proveedor", "Importe PRESEA", "Diferencia"], det, clases, num=(8, 13, 14))
        cuerpo = f'{resumen}<div class="aviso">ⓘ {fuera}</div>{filtro}{grid}'
    pie = """<div class="row" style="justify-content:space-between;margin-top:10px">
 <div class="row"><label style="min-width:0">Perfil:</label><select style="width:320px"><option>Supermercado — Percepciones y Retenciones</option></select></div>
 <div class="row"><span class="btn">Directivas...</span><span class="btn """ + ("" if estado == "ok" else "dis") + """">Exportar conciliación...</span><span class="btn big pri">CONCILIAR</span></div></div>"""
    arca_min = '<div class="tabs"><span class="tab on">Registros ARCA (3.851)</span></div><div class="empty" style="padding:8px;font-style:normal;text-align:left">(grilla de ARCA — igual que en 04, más baja)</div>'
    return arriba + arca_min + f'<div style="margin-top:10px">{tabs}{cuerpo}</div>' + pie

s.append(("07-conciliacion-pyr-resultado", pagina("07-conciliacion-pyr-resultado", TIT, resultado("ok"), 1500)))
s.append(("07-conciliacion-pyr-resultado-vacio", pagina("07-conciliacion-pyr-resultado-vacio", TIT, resultado("vacio"), 1500)))
s.append(("07-conciliacion-pyr-resultado-error", pagina("07-conciliacion-pyr-resultado-error", TIT, resultado("error"), 1500)))

for nombre, ruta in s:
    png = os.path.join(DEST, nombre + ".png")
    TAM = {"01": (820, 720), "04": (1560, 1000), "05": (780, 420), "06": (540, 400), "07": (1560, 1060)}
    ancho, alto = TAM.get(nombre[:2], (600, 340))
    subprocess.run([CHROME, "--headless=new", "--disable-gpu", "--hide-scrollbars", "--force-device-scale-factor=1.5",
                    f"--window-size={ancho},{alto}", f"--screenshot={png}", "file:///" + ruta.replace("\\", "/")],
                   check=True, capture_output=True)
    print("ok", nombre)
