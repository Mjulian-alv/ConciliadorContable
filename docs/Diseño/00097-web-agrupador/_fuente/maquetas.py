# -*- coding: utf-8 -*-
# TAREA 00097 - Genera todas las maquetas (HTML en _fuente/, PNG en la carpeta del diseño).
import sys
sys.stdout.reconfigure(encoding="utf-8")
import maq_1_2
maq_1_2.generar()
try:
    import maq_3_7
    maq_3_7.generar()
except ImportError:
    pass
