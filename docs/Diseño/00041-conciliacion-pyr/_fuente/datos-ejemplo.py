# Fecha: 21/09/2026 - TAREA: 00041 - Linea: 2, 3, 4 - Extrae filas reales de julio 2026 para las maquetas (datos-ejemplo.json)
import pandas as pd, re, io, os, html
B=r'D:\DESARROLLOS CONTABLE\CONCILIADOR\PERCEP Y RETEN'
OUT=r'D:\Sistemas\ConciliadorContable\docs\Diseño\00041-conciliacion-pyr\_fuente'
def ar(x): 
    s=f'{abs(x):,.2f}'.replace(',','X').replace('.',',').replace('X','.'); return ('-' if x<0 else '')+s
def nz(s): d=re.sub(r'\D','',str(s)).lstrip('0'); return d or '0'
# ARCA
iva=pd.read_excel(B+r'\arca\Percepciones IVA 07-2026.xls',dtype=str)
iva.columns=['cuit','den','imp','dimp','reg','dreg','fecha','cert','op','importe','nro','fcomp','dcomp','freg']
hb=pd.read_html(io.StringIO(open(B+r'\arca\Retenciones y Percepciones II.BB 07-2026.xls','rb').read().decode('utf-8')),converters={0:str})[0]; hb.columns=['cuit','den','op','fecha','comp','importe']
arca=[]
for _,r in iva.sort_values('fecha').head(7).iterrows():
    arca.append(('IVA','Percepción',r.fecha,r.cuit,r.den[:26],r.dcomp.title(),'',nz(r.nro),ar(float(r.importe)),'Percepciones IVA 07-2026.xls'))
for _,r in hb.head(6).iterrows():
    m=re.match(r'^(\D*?)\s*([A-Z])?(\d+)$',r.comp.strip()); tipo=(m.group(1) if m else r.comp); letra=(m.group(2) or '') if m else ''
    arca.append(('IIBB',r.op,r.fecha,r.cuit,r.den[:26],tipo,letra,nz(r.comp),ar(float(r.importe)),'Retenciones y Percepciones II.BB 07-2026.xls'))
# PRESEA
pv=pd.read_excel(B+r'\presea\PERC. IVA 07-2026.XLSX')
pat=re.compile(r'^(SEGUN|POR ANULACION)\s+(.+?)\s+(\d+)(?:\s+de\s+(.*))?$',re.I)
pres=[]
sel=pd.concat([pv.head(8), pv[pv.concepto.str.startswith('POR ANULACION')].head(1), pv[~pv.concepto.str.upper().str.startswith(('SEGUN','POR '))].head(2)])
for _,r in sel.iterrows():
    c=re.sub(r'\s+',' ',r.concepto.strip()); m=pat.match(c)
    imp=float(r.debe or 0)-float(r.haber or 0)
    if m: pres.append(('Percepciones IVA',r.fecha.strftime('%d/%m/%Y'),str(r.asiento),m.group(2).upper(),nz(m.group(3)),(m.group(4) or ''),('Sí' if m.group(1).upper().startswith('POR') else ''),ar(imp),False,r.concepto))
    else: pres.append(('Percepciones IVA',r.fecha.strftime('%d/%m/%Y'),str(r.asiento),'—','—','—','',ar(imp),True,r.concepto))
import json; json.dump({'arca':arca,'pres':pres},io.open(OUT+r'\datos-ejemplo.json','w',encoding='utf-8'),ensure_ascii=False,indent=1)
print(len(arca),len(pres)); [print(p) for p in pres]; [print(a) for a in arca]
