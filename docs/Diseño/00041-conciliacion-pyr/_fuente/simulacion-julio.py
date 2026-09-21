# Fecha: 21/09/2026 - TAREA: 00041 - Linea: 6, 7 - Simula la conciliación de julio con las reglas de la guía (datos para las maquetas 07)
# No es el código de producción: replica en Python las reglas de la segunda etapa para que las
# maquetas muestren cantidades y filas reales. Escribe simulacion-julio.json al lado.
import io, json, re, unicodedata
import pandas as pd

B = r'D:\DESARROLLOS CONTABLE\CONCILIADOR\PERCEP Y RETEN'
OUT = r'D:\Sistemas\ConciliadorContable\docs\Diseño\00041-conciliacion-pyr\_fuente\simulacion-julio.json'

def nz(s):
    d = re.sub(r'\D', '', str(s)).lstrip('0')
    return d

def prov(s):
    s = unicodedata.normalize('NFD', str(s or '')).encode('ascii', 'ignore').decode()
    return re.sub(r'[^A-Za-z0-9]', '', s).upper()[:6]

def u8(n):
    return n[-8:].rjust(8, '0') if n else ''

# ARCA
iva = pd.read_excel(B + r'\arca\Percepciones IVA 07-2026.xls', dtype=str)
iva.columns = ['cuit','den','imp','dimp','reg','dreg','fecha','cert','op','importe','nro','fcomp','dcomp','freg']
hb = pd.read_html(io.StringIO(open(B + r'\arca\Retenciones y Percepciones II.BB 07-2026.xls','rb').read().decode('utf-8')), converters={0: str})[0]
hb.columns = ['cuit','den','op','fecha','comp','importe']
arca = []
for _, r in iva.iterrows():
    arca.append(dict(g=('IVA','Percepción'), fecha=r.fecha, cuit=r.cuit, den=r.den, tipo=r.dcomp.title(), num=nz(r.nro), imp=round(float(r.importe),2)))
for _, r in hb.iterrows():
    op = 'Percepción' if r.op.startswith('Perc') else 'Retención'
    m = re.match(r'^(.*?)\s*([A-Z])?(\d+)\s*$', r.comp.strip())
    arca.append(dict(g=('IIBB',op), fecha=r.fecha, cuit=r.cuit, den=r.den.strip(), tipo=(m.group(1) if m else r.comp).strip(), num=nz(r.comp), imp=round(float(r.importe),2)))

# PRESEA
pat = re.compile(r'^(SEGUN|POR ANULACION)\s+(.+?)\s+(\d+)(?:\s+de\s+(.*))?$', re.I)
pres = []
for f, cta, g in [('PERC. IVA 07-2026.XLSX','114105 — Percepciones IVA',('IVA','Percepción')),
                  ('PERC. IIBB 07-2026.XLSX','114110 — Percepciones IIBB Santa Fe',('IIBB','Percepción'))]:
    d = pd.read_excel(B + '\\presea\\' + f)
    for _, r in d.iterrows():
        c = re.sub(r'\s+', ' ', str(r.concepto).strip()); m = pat.match(c)
        pres.append(dict(g=g, cta=cta, fecha=r.fecha.strftime('%d/%m/%Y'), asiento=str(r.asiento),
                         tipo=m.group(2).upper() if m else '', num=nz(m.group(3)) if m else '',
                         prov=(m.group(4) or '') if m else '', anul=bool(m and m.group(1).upper().startswith('POR')),
                         imp=round(float(r.debe or 0) - float(r.haber or 0), 2), estado=None))

grupos = {p['g'] for p in pres}
fuera = [a for a in arca if a['g'] not in grupos]
arca = [a for a in arca if a['g'] in grupos]

# Anulaciones
for a in [p for p in pres if p['anul']]:
    for p in pres:
        if p['estado'] is None and not p['anul'] and p['cta'] == a['cta'] and p['tipo'] == a['tipo'] and p['num'] == a['num'] and round(p['imp'] + a['imp'], 2) == 0:
            p['estado'] = a['estado'] = 'Anulada en PRESEA'; break

DIRS = [('1-Número completo', lambda x, lado: x['num']),
        ('2-Últimos 8 + Importe', lambda x, lado: u8(x['num']) and f"{u8(x['num'])}|{x['imp']:.2f}"),
        ('3-Importe + Fecha + Proveedor', lambda x, lado: prov(x['den'] if lado == 'a' else x['prov']) and f"{x['imp']:.2f}|{x['fecha']}|{prov(x['den'] if lado == 'a' else x['prov'])}")]
res = []
for g in sorted(grupos):
    pa = [a for a in arca if a['g'] == g]
    pp = [p for p in pres if p['g'] == g and p['estado'] is None]
    for nombre, key in DIRS:
        idx = {}
        for p in pp:
            k = key(p, 'p')
            if k and k not in idx: idx[k] = p
        resto = []
        for a in pa:
            k = key(a, 'a'); p = idx.pop(k, None) if k else None
            if p is None: resto.append(a); continue
            pp.remove(p)
            est = 'Conciliado' if a['imp'] == p['imp'] else 'Diferencia de importe'
            res.append(dict(estado=est, dir=nombre, a=a, p=p))
        pa = resto
    res += [dict(estado='Sólo ARCA', dir='', a=a, p=None) for a in pa]
    res += [dict(estado='Sólo PRESEA', dir='', a=None, p=p) for p in pp]
res += [dict(estado='Anulada en PRESEA', dir='', a=None, p=p) for p in pres if p['estado'] == 'Anulada en PRESEA']

def cta(r): return r['p']['cta'] if r['p'] else ('114105 — Percepciones IVA' if r['a']['g'][0] == 'IVA' else '114110 — Percepciones IIBB Santa Fe')
resumen = {}
for r in res:
    s = resumen.setdefault(cta(r), {})
    s[r['estado']] = s.get(r['estado'], 0) + 1
    s['_arca'] = round(s.get('_arca', 0) + (r['a']['imp'] if r['a'] else 0), 2)
    s['_presea'] = round(s.get('_presea', 0) + (r['p']['imp'] if r['p'] else 0), 2)
porDir = {}
for r in res:
    if r['dir']: porDir[r['dir']] = porDir.get(r['dir'], 0) + 1
fuera_txt = {}
for a in fuera: fuera_txt[' · '.join(a['g'])] = fuera_txt.get(' · '.join(a['g']), 0) + 1

# Muestra: algunas filas de cada estado
muestra = []
for est, n in [('Conciliado', 5), ('Diferencia de importe', 3), ('Sólo ARCA', 3), ('Sólo PRESEA', 3), ('Anulada en PRESEA', 2)]:
    filas = [r for r in res if r['estado'] == est]
    if est == 'Conciliado':
        filas = [r for r in filas if r['dir'].startswith('1')][:3] + [r for r in filas if r['dir'].startswith('2')][:1] + [r for r in filas if r['dir'].startswith('3')][:1]
    for r in filas[:n]:
        a, p = r['a'], r['p']
        muestra.append(dict(estado=est, cuenta=cta(r).split(' — ')[1], dir=r['dir'].split('-')[0] if r['dir'] else '',
            fa=a['fecha'] if a else '', cuit=a['cuit'] if a else '', den=(a['den'][:24] if a else ''), tipo=(a['tipo'] if a else p['tipo']),
            na=a['num'] if a else '', ia=a['imp'] if a else None,
            fp=p['fecha'] if p else '', asi=p['asiento'] if p else '', np=p['num'] if p else '', prov=p['prov'] if p else '', ip=p['imp'] if p else None,
            dif=round(a['imp'] - p['imp'], 2) if (a and p and a['imp'] != p['imp']) else None))
json.dump(dict(resumen=resumen, porDir=porDir, fuera=fuera_txt, total=len(res), muestra=muestra),
          io.open(OUT, 'w', encoding='utf-8'), ensure_ascii=False, indent=1)
print(json.dumps(dict(resumen=resumen, porDir=porDir, fuera=fuera_txt), ensure_ascii=False, indent=1))
