# -*- coding: utf-8 -*-
# TAREA 00097 - Exporta los .mmd a PNG: Mermaid (CDN) renderizado por Edge sin interfaz, en dos pasadas
# (la primera mide el SVG; la segunda saca la captura con el tamaño exacto).
import os, re, subprocess, sys
sys.stdout.reconfigure(encoding="utf-8")
AQUI = os.path.dirname(os.path.abspath(__file__))
DOC = os.path.dirname(AQUI)
EDGE = r"C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe"
MERMAID = "https://cdn.jsdelivr.net/npm/mermaid@10.9.1/dist/mermaid.min.js"
PNGS = {"00-flujograma": "00-flujograma", "00b-flujograma-agrupador": "00b-flujograma-agrupador", "00c-flujograma-conciliaciones": "00c-flujograma-conciliaciones"}

PLANTILLA = """<!doctype html><html><head><meta charset="utf-8"><style>
body{{margin:0;background:#fff;font-family:"Segoe UI",Arial,sans-serif}} .m{{padding:24px;display:inline-block}}
</style></head><body><div class="m"><pre class="mermaid">
{codigo}
</pre></div>
<script src="{cdn}"></script>
<script>mermaid.initialize({{startOnLoad:true,theme:"base",flowchart:{{useMaxWidth:false,htmlLabels:true,curve:"basis"}},
themeVariables:{{fontFamily:"Segoe UI, Arial",fontSize:"15px",primaryColor:"#e6f2f0",primaryBorderColor:"#007e79",primaryTextColor:"#153445",lineColor:"#647786",
secondaryColor:"#fff1d7",tertiaryColor:"#f3f5f7"}}}});</script></body></html>"""


def uri(ruta):
    return "file:///" + ruta.replace("\\", "/")


def correr(args, salida=None, tiempo=120):
    return subprocess.run([EDGE, "--headless=new", "--disable-gpu", "--hide-scrollbars"] + args, capture_output=True, timeout=tiempo)


def exportar(nombre):
    codigo = open(os.path.join(AQUI, nombre + ".mmd"), encoding="utf-8").read()
    ruta = os.path.join(AQUI, nombre + ".html")
    open(ruta, "w", encoding="utf-8").write(PLANTILLA.format(codigo=codigo, cdn=MERMAID))
    r = correr(["--virtual-time-budget=20000", "--window-size=1800,1200", "--dump-dom", uri(ruta)])
    dom = r.stdout.decode("utf-8", errors="replace")
    m = re.search(r'<svg[^>]*viewBox="([\d.\- ]+)"', dom)
    if not m:
        print("FALLA (no se renderizó el SVG)", nombre)
        return False
    x, y, w, h = [float(v) for v in m.group(1).split()]
    ancho, alto = int(w) + 60, int(h) + 60
    png = os.path.join(DOC, nombre + ".png")
    if os.path.exists(png):
        os.remove(png)
    correr(["--virtual-time-budget=20000", f"--window-size={ancho},{alto}", "--force-device-scale-factor=1", f"--screenshot={png}", uri(ruta)])
    ok = os.path.exists(png) and os.path.getsize(png) > 5000
    print(("OK  " if ok else "FALLA ") + nombre + f".png  ({ancho}x{alto})")
    return ok


if __name__ == "__main__":
    res = [exportar(n) for n in PNGS]
    sys.exit(0 if all(res) else 1)
