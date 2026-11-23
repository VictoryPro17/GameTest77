"""RE:BORN - Boss-Extras (Meilenstein 8, Comic-Look)

Wurzelstacheln (3 Stufen) und rote Warnflaeche fuer den Waldboss (128 px, Fusspunkt-Kamera wie Props)
und das Amulett "Herz des Waldes" (Item-Icon, 48 px).
Eigene Datei: Art/Blender/Reborn_BossExtras.blend. Ausgabe: Sprites/Boss und Sprites/Items.

Aufruf:  blender.exe -b --factory-startup --python build_boss_extras.py -- <projektordner>
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
OUT = os.path.join(PROJECT, "Assets", "_Project", "Sprites", "Boss")
ICON_DIR = os.path.join(PROJECT, "Assets", "_Project", "Sprites", "Items")
BLEND_PATH = os.path.join(PROJECT, "Art", "Blender", "Reborn_BossExtras.blend")
os.makedirs(OUT, exist_ok=True)
os.makedirs(ICON_DIR, exist_ok=True)

bpy.ops.wm.read_factory_settings(use_empty=True)
scene = bpy.context.scene
scene.name = "BossExtras"
L.setup_scene(scene)
cam = L.camera(scene, target=(0, 0, 0.85), ortho=2.1)
L.sun(scene)

M = {"root": L.cel("Wurzel", (0.45, 0.28, 0.14)), "root_d": L.cel("WurzelDunkel", (0.3, 0.18, 0.1)),
     "moss": L.cel("Moos", (0.3, 0.62, 0.22)), "moss_d": L.cel("MoosDunkel", (0.2, 0.45, 0.16)),
     "dirt": L.cel("Erde", (0.4, 0.27, 0.17)), "dirt_d": L.cel("ErdeDunkel", (0.3, 0.2, 0.13)),
     "warn": L.cel("WarnRing", (1.0, 0.15, 0.1), shadow=1.0, mid=1.0, emission=1.6),
     "warn_f": L.cel("WarnFlaeche", (0.55, 0.05, 0.05), shadow=1.0, mid=1.0, emission=0.8),
     "gold": L.cel("Gold", (1.0, 0.78, 0.28), light=1.15),
     "gold_d": L.cel("GoldDunkel", (0.8, 0.58, 0.18)),
     "gem": L.cel("Herzstein", (0.2, 0.9, 0.35), shadow=0.8, mid=0.95, emission=1.0),
     "gem_l": L.cel("HerzLicht", (0.7, 1.0, 0.75), shadow=1.0, mid=1.0, emission=0.8)}
objs = []


def P(kind, mat, loc, sc, rot=(0, 0, 0), **kw):
    o = L.shape(kind, loc, sc, rot, M[mat], None, collection=scene.collection, **kw)
    objs.append(o)
    return o


count = 0
warnings = 0


def shot(name, parts, out_dir, size=128, outline=0.016, smooth=True, subdiv=True):
    global count, warnings
    o = L.join(parts, name)
    if smooth and subdiv:
        L.subdivide(o, 1)
    L.add_outline(o, outline)
    scene.render.resolution_x = scene.render.resolution_y = size
    cam.data.ortho_scale = 2.1 * size / 128
    pitch = math.radians(45)
    fwd = Vector((0, math.sin(pitch), -math.cos(pitch)))
    cam.location = Vector((0, 0, 0.85 * size / 128)) - fwd * 10
    path = os.path.join(out_dir, name + ".png")
    L.render_still(scene, path)
    bpy.data.objects.remove(o, do_unlink=True)
    objs.clear()
    count += 1
    e = L.edge_pixels(path)
    if e:
        warnings += 1
        print("WARNUNG Randpixel:", name, e)


def root(bx, by, height, lean, ang, r0, n=6, mat="root"):
    """gebogene, spitz zulaufende Wurzel: Kette aus Kegelstumpf-Stuecken auf einer Kurve"""
    d = Vector((math.cos(ang), math.sin(ang), 0))

    def pt(t):
        return Vector((bx, by, 0)) + d * (lean * t * t) + Vector((0, 0, height * t))
    for i in range(n):
        t0, t1 = i / n, (i + 1) / n
        a, b = pt(t0), pt(t1)
        v = b - a
        ra, rb = r0 * (1 - 0.92 * t0), r0 * (1 - 0.92 * t1)
        e = v.to_track_quat("Z", "Y").to_euler()
        P("cyl", mat if i % 2 == 0 else "root_d" if mat == "root" else mat, (a + b) / 2, (ra, ra, v.length / 2 * 1.08),
          rot=tuple(math.degrees(x) for x in e), taper=rb / ra, segments=8)
    # Moosflecken am Fuss
    P("sphere", "moss", pt(0.12) + Vector((0, 0, 0.02)), (r0 * 1.15, r0 * 1.05, r0 * 0.6), segments=8)


def spikes(level):
    h = (0.3, 0.7, 1.1)[level]
    parts_n = (4, 5, 5)[level]
    mound = [P("sphere", "dirt", (0, 0, 0.02), (0.42 + 0.06 * level, 0.36 + 0.05 * level, 0.1 + 0.03 * level), segments=12),
             P("sphere", "dirt_d", (0.2, -0.12, 0.03), (0.15, 0.12, 0.06), segments=8),
             P("sphere", "dirt_d", (-0.22, 0.08, 0.03), (0.14, 0.11, 0.06), segments=8)]
    random.seed(7 + level)
    for k in range(parts_n):
        ang = 2 * math.pi * k / parts_n + 0.4
        rad = 0.14 + 0.05 * level
        hk = h * (0.7 + 0.3 * ((k * 37) % 10) / 10) if k else h
        root(rad * math.cos(ang), rad * math.sin(ang), hk, 0.12 + 0.14 * level, ang + 0.5, 0.05 + 0.025 * level, n=5 + level)
    # kleine Moosbueschel auf dem Erdhaufen
    for k in range(3):
        a = k * 2.1
        P("sphere", "moss_d", (0.3 * math.cos(a), 0.25 * math.sin(a), 0.07), (0.06, 0.06, 0.03), segments=6)


# ================= Wurzelstacheln und Warnflaeche (128 px, Fusspunkt; Warnflaeche Radius 0,6 m, sonst ragt sie unten aus dem Bild - in Unity hochskalieren)
for lv in range(3):
    spikes(lv)
    shot(f"root_spike_{lv}", list(objs), OUT, 128, outline=0.014)

warn = [P("cyl", "warn_f", (0, 0, 0.006), (0.57, 0.57, 0.006), segments=40),
        P("cyl", "warn", (0, 0, 0.014), (0.6, 0.6, 0.006), segments=40),
        P("cyl", "warn_f", (0, 0, 0.022), (0.51, 0.51, 0.006), segments=40),
        P("cyl", "warn", (0, 0, 0.03), (0.27, 0.27, 0.006), segments=32),
        P("cyl", "warn_f", (0, 0, 0.038), (0.22, 0.22, 0.006), segments=32)]
shot("warning_circle", warn, OUT, 128, outline=0.004, smooth=False)

# ================= Icon: Herz des Waldes (48 px, Icon-Kamera wie build_equipment.py)
R90 = math.radians(90)


def heart(mat, s, y, dz=0.0):
    return [P("sphere", mat, (-0.11 * s, y, 0.1 * s + dz), (0.15 * s, 0.1 * s, 0.15 * s), segments=16),
            P("sphere", mat, (0.11 * s, y, 0.1 * s + dz), (0.15 * s, 0.1 * s, 0.15 * s), segments=16),
            P("cone", mat, (0, y, -0.1 * s + dz), (0.255 * s, 0.1 * s, 0.22 * s), rot=(180, 0, 0), segments=16)]


heart("gold", 1.28, 0.03, -0.0)
heart("gem", 1.0, -0.04)
P("sphere", "gem_l", (-0.14, -0.15, 0.17), (0.04, 0.02, 0.05))
P("torus", "gold", (0, 0.0, 0.47), (0.08, 0.08, 0.08), rot=(90, 0, 0))                # Oese
P("torus", "gold_d", (0, 0.04, 0.78), (0.36, 0.36, 0.36), rot=(90, 0, 0), segments=20)  # Kette
o = L.join(list(objs), "ICON_herz_des_waldes")
objs.clear()
L.subdivide(o, 1)
L.add_outline(o, 0.012)
bpy.context.view_layer.update()
ev = o.evaluated_get(bpy.context.evaluated_depsgraph_get())
me = ev.to_mesh()
pts = [ev.matrix_world @ v.co for v in me.vertices]
ev.to_mesh_clear()
lo = Vector((min(p.x for p in pts), min(p.y for p in pts), min(p.z for p in pts)))
hi = Vector((max(p.x for p in pts), max(p.y for p in pts), max(p.z for p in pts)))
c, size = (lo + hi) / 2, hi - lo
cd = bpy.data.cameras.new("IconCamera")
cd.type = "ORTHO"
icam = bpy.data.objects.new("IconCamera", cd)
scene.collection.objects.link(icam)
scene.camera = icam
icam.location = (c.x, c.y - 10, c.z + 1.2)
icam.rotation_euler = (math.radians(83), 0, 0)
cd.ortho_scale = max(0.2, max(size.x, size.z) * 1.2)
scene.render.resolution_x = scene.render.resolution_y = 48
path = os.path.join(ICON_DIR, "Item_herz_des_waldes.png")
L.render_still(scene, path)
count += 1
e = L.edge_pixels(path)
if e:
    warnings += 1
    print("WARNUNG Randpixel: Item_herz_des_waldes", e)

bpy.ops.wm.save_as_mainfile(filepath=BLEND_PATH)
print(f"RESULT rendered={count} randwarnungen={warnings}")
