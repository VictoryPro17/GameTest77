"""RE:BORN - Boss-Stufe-2-Effekte (Meilenstein 8b, Comic-Look)

Dornenring (3 Stufen), Ansturm-Streifen und Ultimate-Warnflaeche, je 256 px.
Massstab wie der Boss (ortho 4,2 bei 256 px, 45 Grad). Damit die grossen Flaechen ins Bild passen, liegt der
Ursprung nicht am Fusspunkt 0,2138, sondern bildmittig (Ring, Kreis) bzw. links (Streifen); die Pivots stehen im RESULT.
Eigene Datei: Art/Blender/Reborn_Boss2Fx.blend. Ausgabe: Sprites/Boss2.

Aufruf:  blender.exe -b --factory-startup --python build_boss2_fx.py -- <projektordner>
"""
import bpy
import math
import os
import random
import sys
from mathutils import Vector

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import reborn_lib as L  # noqa: E402

argv = sys.argv[sys.argv.index("--") + 1:] if "--" in sys.argv else []
PROJECT = argv[0] if argv else os.path.abspath(os.path.join(os.path.dirname(__file__), "..", "..", ".."))
OUT = os.path.join(PROJECT, "Assets", "_Project", "Sprites", "Boss2")
BLEND_PATH = os.path.join(PROJECT, "Art", "Blender", "Reborn_Boss2Fx.blend")
os.makedirs(OUT, exist_ok=True)

bpy.ops.wm.read_factory_settings(use_empty=True)
scene = bpy.context.scene
scene.name = "Boss2Fx"
L.setup_scene(scene, 256)
cam = L.camera(scene, target=(0, 0, 0), ortho=4.2)
L.sun(scene)

M = {"thorn": L.cel("Dorn", (0.14, 0.09, 0.12)), "thorn_l": L.cel("DornHell", (0.3, 0.18, 0.2)),
     "thorn_t": L.cel("DornSpitze", (1.0, 0.35, 0.1), shadow=1.0, mid=1.0, emission=0.2),
     "dirt": L.cel("ErdeDunkel", (0.22, 0.14, 0.14)),
     "glow": L.cel("WarnRing", (1.0, 0.3, 0.08), shadow=1.0, mid=1.0, emission=0.25),
     "red_f": L.cel("WarnFlaeche", (0.5, 0.04, 0.05), shadow=1.0, mid=1.0, emission=0.1),
     "red_d": L.cel("RotDunkel", (0.3, 0.02, 0.04), shadow=1.0, mid=1.0, emission=0.05),
     "orange": L.cel("OrangeHell", (1.0, 0.55, 0.15), shadow=1.0, mid=1.0, emission=0.25)}
objs = []
count = warnings = 0
pivots = {}


def P(kind, mat, loc, sc, rot=(0, 0, 0), **kw):
    o = L.shape(kind, loc, sc, rot, M[mat], None, collection=scene.collection, **kw)
    objs.append(o)
    return o


def shot(name, target, outline=0.01):
    global count, warnings
    o = L.join(list(objs), name)
    L.add_outline(o, outline)
    fwd = Vector((0, math.sin(math.radians(45)), -math.cos(math.radians(45))))
    cam.location = Vector(target) - fwd * 10
    path = os.path.join(OUT, name + ".png")
    L.render_still(scene, path)
    bpy.data.objects.remove(o, do_unlink=True)
    objs.clear()
    count += 1
    pivots[name] = L.foot_pivot(scene)
    e = L.edge_pixels(path)
    if e:
        warnings += 1
        print("WARNUNG Randpixel:", name, e)


def thorns(level):
    random.seed(11 + level)
    hmul = (0.5, 0.8, 1.05)[level]
    P("torus", "dirt", (0, 0, 0.02), (1.62, 1.62, 0.12), segments=40)
    P("torus", "glow", (0, 0, 0.04), (1.45, 1.45, 0.05), segments=40)
    for ring, rad, n, h0 in ((0, 1.7, 30, 0.55), (1, 1.42, 24, 0.42), (2, 1.2, 16, 0.3)):
        for k in range(n):
            a = 2 * math.pi * (k + 0.5 * ring + random.uniform(-0.15, 0.15)) / n
            h = h0 * hmul * random.uniform(0.7, 1.3)
            r = rad + random.uniform(-0.05, 0.05)
            w = 0.075 + 0.02 * random.random()
            lean = random.uniform(8, 20)
            rot = (lean * math.sin(a), -lean * math.cos(a), 0)
            P("cone", "thorn", (r * math.cos(a), r * math.sin(a), h), (w, w, h), rot=rot, segments=6, smooth=False)
            if level == 2 and ring == 0 and k % 3 == 0:
                P("cone", "thorn_t", (r * math.cos(a), r * math.sin(a), h * 1.55), (w * 0.3, w * 0.3, h * 0.28),
                  rot=rot, segments=6, smooth=False)


for lv in range(3):
    thorns(lv)
    shot(f"thorn_ring_{lv}", (0, 0, 0.15), 0.012)

# ---- Ansturm-Streifen (+X)
P("cube", "red_d", (1.5, 0, 0.012), (1.5, 0.3, 0.012), smooth=False)
P("cube", "glow", (1.5, 0, 0.026), (1.5, 0.08, 0.008), smooth=False)
for i in range(5):
    x = 0.45 + i * 0.6
    P("cone", "orange", (x, 0, 0.034), (0.03, 0.22, 0.16), rot=(0, 90, 0), segments=4, smooth=False)
P("cone", "red_f", (3.4, 0, 0.012), (0.03, 0.5, 0.4), rot=(0, 90, 0), segments=4, smooth=False)
P("cone", "glow", (3.4, 0, 0.028), (0.02, 0.36, 0.3), rot=(0, 90, 0), segments=4, smooth=False)
shot("charge_line", (1.9, 0, 0), 0.008)

# ---- Ultimate-Warnflaeche
P("cyl", "glow", (0, 0, 0.006), (1.9, 1.9, 0.006), segments=48)
P("cyl", "red_f", (0, 0, 0.014), (1.76, 1.76, 0.006), segments=48)
P("cyl", "orange", (0, 0, 0.022), (1.22, 1.22, 0.006), segments=48)
P("cyl", "red_d", (0, 0, 0.03), (1.14, 1.14, 0.006), segments=48)
P("cyl", "glow", (0, 0, 0.038), (0.5, 0.5, 0.006), segments=32)
P("cyl", "red_f", (0, 0, 0.046), (0.42, 0.42, 0.006), segments=32)
for k in range(12):                         # Runen-Striche zwischen den Ringen
    a = 2 * math.pi * k / 12
    rm = 1.5
    for j, (da, ln, ang) in enumerate(((0.0, 0.17, 0), (0.07, 0.1, 40), (-0.07, 0.1, -40))):
        aa = a + da
        P("cube", "orange", (rm * math.cos(aa), rm * math.sin(aa), 0.05), (0.018, ln, 0.006),
          rot=(0, 0, math.degrees(a) + ang), smooth=False)
for k in range(8):                          # Zacken zur Mitte
    a = 2 * math.pi * (k + 0.5) / 8
    P("cone", "orange", (0.8 * math.cos(a), 0.8 * math.sin(a), 0.05), (0.02, 0.1, 0.26),
      rot=(0, 90, math.degrees(a)), segments=4, smooth=False)
shot("ultimate_circle", (0, 0, 0), 0.008)

bpy.ops.wm.save_as_mainfile(filepath=BLEND_PATH)
print(f"RESULT rendered={count} randwarnungen={warnings} pivots={pivots}")
