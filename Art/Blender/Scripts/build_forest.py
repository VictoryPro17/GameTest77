"""RE:BORN - Wald (Meilenstein 7c, Comic-Look)

Kacheln (64 px, von oben, nahtlos): Waldboden 2 Varianten, Waldweg, Dornenhecke als Rand.
Deko (128 px, Kamera wie build_props.py, Fusspunkt 0,2138): umgestuerzter Baum, Pilze, Farn, Baumstumpf.
Eigene Datei: Art/Blender/Reborn_Forest.blend. Ausgabe: Assets/_Project/Sprites/Forest.

Aufruf:  blender.exe -b --factory-startup --python build_forest.py -- <projektordner>
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
OUT = os.path.join(PROJECT, "Assets", "_Project", "Sprites", "Forest")
BLEND_PATH = os.path.join(PROJECT, "Art", "Blender", "Reborn_Forest.blend")
os.makedirs(OUT, exist_ok=True)

bpy.ops.wm.read_factory_settings(use_empty=True)
scene = bpy.context.scene
scene.name = "Forest"
L.setup_scene(scene)
cam = L.camera(scene, target=(0, 0, 0.85), ortho=2.1)
L.sun(scene)

M = {"moss": L.cel("Moos", (0.2, 0.38, 0.17), shadow=0.85, mid=0.95), "moss2": L.cel("Moos2", (0.18, 0.34, 0.16), shadow=0.85, mid=0.95),
     "leaf_y": L.cel("LaubGelb", (0.75, 0.6, 0.2)), "leaf_r": L.cel("LaubRot", (0.7, 0.3, 0.15)), "leaf_g": L.cel("LaubGruen", (0.3, 0.5, 0.2)),
     "dirt": L.cel("Waldweg", (0.42, 0.3, 0.18), shadow=0.85, mid=0.95), "dirt_d": L.cel("WaldwegDunkel", (0.32, 0.22, 0.13)),
     "thorn": L.cel("Dorn", (0.12, 0.26, 0.12)), "thorn_d": L.cel("DornDunkel", (0.08, 0.16, 0.08)), "berry": L.cel("Beere", (0.6, 0.1, 0.3)),
     "bark": L.cel("Rinde", (0.38, 0.25, 0.14)), "bark_d": L.cel("RindeDunkel", (0.24, 0.15, 0.08)), "ring": L.cel("Jahresring", (0.75, 0.6, 0.38)),
     "cap_r": L.cel("PilzRot", (0.85, 0.15, 0.12)), "dot": L.cel("PilzPunkt", (0.97, 0.95, 0.9)), "stem": L.cel("PilzStiel", (0.92, 0.88, 0.78)),
     "cap_b": L.cel("PilzBraun", (0.6, 0.4, 0.22)), "fern": L.cel("Farn", (0.25, 0.55, 0.22)), "fern_l": L.cel("FarnHell", (0.4, 0.68, 0.28))}
objs = []


def P(kind, mat, loc, sc, rot=(0, 0, 0), **kw):
    o = L.shape(kind, loc, sc, rot, M[mat], None, collection=scene.collection, **kw)
    objs.append(o)
    return o


count = 0
warnings = 0


def shot_prop(name, parts, outline=0.016, smooth=True):
    global count, warnings
    o = L.join(parts, name)
    if smooth:
        L.subdivide(o, 1)
    L.add_outline(o, outline)
    pitch = math.radians(45)
    cam.location = Vector((0, 0, 0.85)) - Vector((0, math.sin(pitch), -math.cos(pitch))) * 10
    path = os.path.join(OUT, name + ".png")
    L.render_still(scene, path)
    bpy.data.objects.remove(o, do_unlink=True)
    objs.clear()
    count += 1
    if L.edge_pixels(path):
        warnings += 1
        print("WARNUNG Randpixel:", name)


# ---------------- Deko
log = [P("cyl", "bark", (0, 0, 0.18), (0.18, 0.18, 0.7), rot=(0, 90, 8), segments=12),
       P("cyl", "ring", (0.71, 0.1, 0.18), (0.16, 0.16, 0.01), rot=(0, 90, 8), segments=12),
       P("cyl", "bark_d", (-0.3, 0.05, 0.38), (0.05, 0.05, 0.15), rot=(0, -30, 0)),
       P("sphere", "moss", (-0.2, 0, 0.33), (0.25, 0.15, 0.05))]
shot_prop("fallen_log", log)
random.seed(2)
mush = []
for k, (x, y, s, mat) in enumerate(((0, 0, 1.0, "cap_r"), (0.2, 0.08, 0.7, "cap_r"), (-0.18, 0.05, 0.6, "cap_b"))):
    mush += [P("cyl", "stem", (x, y, 0.1 * s), (0.04 * s, 0.04 * s, 0.1 * s)),
             P("sphere", mat, (x, y, 0.22 * s), (0.13 * s, 0.13 * s, 0.07 * s))]
    if mat == "cap_r":
        mush += [P("sphere", "dot", (x + 0.06 * s * math.cos(a), y - 0.06 * s * math.sin(a) - 0.04 * s, 0.26 * s), (0.02 * s,) * 3) for a in (0.5, 2.2, 4.0)]
shot_prop("mushrooms", mush)
fern = []
for k in range(7):
    a = 2 * math.pi * k / 7
    fern.append(P("cone", "fern" if k % 2 else "fern_l", (0.18 * math.cos(a), 0.12 * math.sin(a), 0.22), (0.06, 0.03, 0.25),
                  rot=(35 * math.sin(a), 35 * math.cos(a), math.degrees(a)), segments=6))
shot_prop("fern", fern, 0.01)
stump = [P("cyl", "bark", (0, 0, 0.18), (0.24, 0.24, 0.18), segments=14, taper=0.9), P("cyl", "ring", (0, 0, 0.365), (0.21, 0.21, 0.01), segments=14),
         P("cone", "bark_d", (0.22, 0.05, 0.05), (0.08, 0.06, 0.08), rot=(0, 70, 0)), P("cone", "bark_d", (-0.2, 0.1, 0.05), (0.08, 0.06, 0.08), rot=(0, -70, 0))]
shot_prop("stump", stump)

# ---------------- Kacheln (64 px, senkrecht von oben, nahtlos)
for o in list(scene.objects):
    if o.type == "MESH":
        bpy.data.objects.remove(o, do_unlink=True)
scene.render.resolution_x = scene.render.resolution_y = 64
scene.render.film_transparent = False
cd = bpy.data.cameras.new("TileCam"); cd.type = "ORTHO"; cd.ortho_scale = 1.0
tcam = bpy.data.objects.new("TileCam", cd); scene.collection.objects.link(tcam)
tcam.location = (0.5, 0.5, 10); tcam.rotation_euler = (0, 0, 0)
scene.camera = tcam


def wrapped(kind, mat, x, y, z, sc, rot=(0, 0, 0), margin=0.15, **kw):
    for dx in (-1, 0, 1):
        for dy in (-1, 0, 1):
            if -margin <= x + dx <= 1 + margin and -margin <= y + dy <= 1 + margin:
                P(kind, mat, (x + dx, y + dy, z), sc, rot, **kw)


def tile(name, seed, base, scatter):
    global count
    random.seed(seed)
    P("cube", base, (0.5, 0.5, -0.05), (1.5, 1.5, 0.04), smooth=False)
    for mats, n, size in scatter:
        for _ in range(n):
            wrapped("sphere", random.choice(mats), random.random(), random.random(), 0.0,
                    (random.uniform(*size), random.uniform(*size) * 0.7, 0.012), rot=(0, 0, random.uniform(0, 180)), segments=8)
    L.render_still(scene, os.path.join(OUT, name + ".png"))
    for o in objs:
        bpy.data.objects.remove(o, do_unlink=True)
    objs.clear()
    count += 1


tile("Forest_Ground_0", 11, "moss", [(["moss2"], 8, (0.08, 0.16)), (["leaf_y", "leaf_r", "leaf_g"], 10, (0.025, 0.045))])
tile("Forest_Ground_1", 12, "moss2", [(["moss"], 8, (0.08, 0.16)), (["leaf_y", "leaf_g"], 6, (0.025, 0.045))])
tile("Forest_Path", 13, "dirt", [(["dirt_d"], 10, (0.05, 0.1)), (["leaf_y", "leaf_r"], 5, (0.025, 0.04))])
tile("Forest_Thorns", 14, "thorn_d", [(["thorn"], 14, (0.1, 0.2)), (["berry"], 4, (0.02, 0.03))])

scene.render.film_transparent = True
bpy.ops.wm.save_as_mainfile(filepath=BLEND_PATH)
print(f"RESULT rendered={count} randwarnungen={warnings}")
