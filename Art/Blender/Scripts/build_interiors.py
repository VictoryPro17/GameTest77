"""RE:BORN - Innenraeume (Meilenstein 7b, Comic-Look)

Kacheln (64 px, von oben, nahtlos): Holzdielen, Steinboden, dunkles Holz, Hoehlenfels, dazu je eine Wandkachel.
Einrichtung (128 px, Kamera wie build_props.py, Fusspunkt 0,2138; grosse Teile 256 px):
Krieger: Waffenstaender, Uebungspuppe, Banner, langer Tisch. Magier: Buecherregal, Alchemietisch, schwebender Kristall,
Runenkreis. Assassinen: Vorhang, Zielscheibe, Kerzenstaender. Hoehle: Stalagmit, Kristallgruppe. Alle: Fussmatte.
Eigene Datei: Art/Blender/Reborn_Interiors.blend. Ausgabe: Assets/_Project/Sprites/Interiors.

Aufruf:  blender.exe -b --factory-startup --python build_interiors.py -- <projektordner>
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
OUT = os.path.join(PROJECT, "Assets", "_Project", "Sprites", "Interiors")
BLEND_PATH = os.path.join(PROJECT, "Art", "Blender", "Reborn_Interiors.blend")
os.makedirs(OUT, exist_ok=True)

bpy.ops.wm.read_factory_settings(use_empty=True)
scene = bpy.context.scene
scene.name = "Interiors"
L.setup_scene(scene)
cam = L.camera(scene, target=(0, 0, 0.85), ortho=2.1)
L.sun(scene)

M = {"plank": L.cel("Diele", (0.62, 0.42, 0.24), shadow=0.8, mid=0.92), "plank2": L.cel("Diele2", (0.55, 0.36, 0.2), shadow=0.8, mid=0.92),
     "gap": L.cel("Fuge", (0.25, 0.15, 0.08), shadow=0.9, mid=0.95),
     "slab": L.cel("Platte", (0.5, 0.52, 0.6), shadow=0.8, mid=0.92), "slab2": L.cel("Platte2", (0.44, 0.46, 0.55), shadow=0.8, mid=0.92),
     "dark": L.cel("DunkelHolz", (0.3, 0.2, 0.22), shadow=0.8, mid=0.92), "dark2": L.cel("DunkelHolz2", (0.25, 0.16, 0.19), shadow=0.8, mid=0.92),
     "cave": L.cel("Hoehlenboden", (0.36, 0.33, 0.34), shadow=0.8, mid=0.92), "cave2": L.cel("Hoehlenboden2", (0.3, 0.28, 0.3), shadow=0.8, mid=0.92),
     "pebble": L.cel("Kiesel", (0.5, 0.48, 0.5)),
     "wall_w": L.cel("WandHolz", (0.45, 0.28, 0.15)), "wall_s": L.cel("WandStein", (0.38, 0.38, 0.46)),
     "wall_d": L.cel("WandDunkel", (0.2, 0.15, 0.2)), "wall_c": L.cel("WandFels", (0.26, 0.24, 0.27)),
     "wood": L.cel("Holz", (0.58, 0.37, 0.2)), "wood_d": L.cel("HolzDunkel", (0.34, 0.2, 0.1)),
     "steel": L.cel("Stahl", (0.66, 0.68, 0.74), light=1.05), "red": L.cel("BannerRot", (0.75, 0.15, 0.15)),
     "gold": L.cel("Gold", (1.0, 0.8, 0.28), light=1.1), "straw": L.cel("Stroh", (0.9, 0.75, 0.38)),
     "cloth": L.cel("Leinen", (0.85, 0.8, 0.68)), "book_r": L.cel("BuchRot", (0.7, 0.2, 0.2)),
     "book_b": L.cel("BuchBlau", (0.2, 0.35, 0.7)), "book_g": L.cel("BuchGruen", (0.2, 0.55, 0.3)),
     "glass": L.cel("Glas", (0.7, 0.9, 0.95), emission=0.2), "potion": L.cel("TrankLila", (0.7, 0.3, 0.9), emission=0.5),
     "crystal": L.cel("Kristall", (0.4, 0.85, 1.0), shadow=0.8, emission=0.8), "rune": L.cel("Rune", (0.45, 0.8, 1.0), shadow=1.0, emission=0.9),
     "curtain": L.cel("Vorhang", (0.45, 0.08, 0.15)), "target_w": L.cel("ZielWeiss", (0.92, 0.9, 0.85)),
     "target_r": L.cel("ZielRot", (0.8, 0.15, 0.15)), "candle": L.cel("Kerze", (0.95, 0.92, 0.8)),
     "flame": L.cel("Flamme", (1.0, 0.75, 0.3), shadow=1.0, emission=1.0), "rock": L.cel("Fels", (0.42, 0.4, 0.44)),
     "rock_d": L.cel("FelsDunkel", (0.3, 0.28, 0.32)), "mat": L.cel("Matte", (0.6, 0.25, 0.2)), "mat_b": L.cel("MatteRand", (0.9, 0.75, 0.35))}

objs = []


def P(kind, mat, loc, sc, rot=(0, 0, 0), **kw):
    o = L.shape(kind, loc, sc, rot, M[mat], None, collection=scene.collection, **kw)
    objs.append(o)
    return o


count = 0
warnings = 0


def shot_prop(name, parts, size=128, outline=0.016, smooth=True):
    """Teile verbinden, rendern (Kamera wie build_props), danach loeschen"""
    global count, warnings
    o = L.join(parts, name)
    if smooth:
        L.subdivide(o, 1)
    L.add_outline(o, outline)
    scene.render.resolution_x = scene.render.resolution_y = size
    cam.data.ortho_scale = 2.1 * size / 128
    pitch = math.radians(45)
    fwd = Vector((0, math.sin(pitch), -math.cos(pitch)))
    cam.location = Vector((0, 0, 0.85 * size / 128)) - fwd * 10
    path = os.path.join(OUT, name + ".png")
    L.render_still(scene, path)
    bpy.data.objects.remove(o, do_unlink=True)
    objs.clear()
    count += 1
    if L.edge_pixels(path):
        warnings += 1
        print("WARNUNG Randpixel:", name)


# ================= Einrichtung (128 px, Fusspunkt)
# Krieger
rack = [P("cube", "wood_d", (0, 0, 0.05), (0.5, 0.15, 0.05)), P("cube", "wood", (0, 0.05, 0.7), (0.5, 0.04, 0.04))]
rack += [P("cyl", "wood_d", (x, 0.05, 0.45), (0.03, 0.03, 0.45)) for x in (-0.45, 0.45)]
for i, x in enumerate((-0.3, -0.1, 0.1, 0.3)):
    rack.append(P("cube", "steel", (x, 0, 0.55), (0.025, 0.01, 0.4)))
    rack.append(P("cube", "gold" if i % 2 else "wood_d", (x, 0, 0.98), (0.07, 0.02, 0.02)))
shot_prop("weapon_rack", rack)
dummy = [P("cyl", "wood_d", (0, 0, 0.35), (0.04, 0.04, 0.35)), P("cyl", "straw", (0, 0, 0.85), (0.17, 0.15, 0.25)),
         P("sphere", "cloth", (0, 0, 1.22), (0.13, 0.13, 0.13)), P("cyl", "straw", (0, 0, 0.98), (0.4, 0.06, 0.05), rot=(0, 90, 0)),
         P("cube", "red", (0, -0.15, 0.85), (0.08, 0.01, 0.08)), P("cyl", "wood_d", (0, 0, 0.03), (0.2, 0.2, 0.03))]
shot_prop("dummy", dummy)
banner = [P("cyl", "wood_d", (0, 0, 1.5), (0.03, 0.03, 0.45), rot=(0, 90, 0)), P("cube", "red", (0, 0, 1.0), (0.32, 0.015, 0.48)),
          P("cone", "red", (0, 0, 0.47), (0.32, 0.015, 0.1), rot=(180, 0, 0), segments=3),
          P("cube", "gold", (0, -0.02, 1.1), (0.12, 0.01, 0.12), rot=(0, 45, 0), smooth=False)]
shot_prop("banner", banner)
table = [P("cube", "wood", (0, 0, 0.5), (1.1, 0.35, 0.04))]
table += [P("cube", "wood_d", (x, y, 0.24), (0.04, 0.04, 0.24)) for x in (-1.0, 1.0) for y in (-0.28, 0.28)]
table += [P("cyl", "steel", (x, -0.05, 0.58), (0.06, 0.06, 0.06)) for x in (-0.6, 0.2, 0.7)]
table += [P("sphere", "straw", (-0.15, 0.05, 0.57), (0.12, 0.08, 0.04))]
shot_prop("table_long", table, 256)

# Magier
shelf = [P("cube", "wood_d", (0, 0, 0.8), (0.55, 0.18, 0.8))]
random.seed(3)
for row, z in enumerate((0.35, 0.75, 1.15)):
    shelf.append(P("cube", "wood", (0, -0.15, z - 0.12), (0.5, 0.04, 0.02), smooth=False))
    x = -0.45
    while x < 0.42:
        w = random.uniform(0.04, 0.07)
        h = random.uniform(0.11, 0.16)
        shelf.append(P("cube", random.choice(["book_r", "book_b", "book_g"]), (x + w, -0.16, z - 0.1 + h), (w, 0.03, h), smooth=False))
        x += 2 * w + 0.01
shot_prop("bookshelf", shelf, 128, smooth=False)
alch = [P("cube", "wood", (0, 0, 0.45), (0.5, 0.25, 0.04))]
alch += [P("cube", "wood_d", (x, y, 0.22), (0.035, 0.035, 0.22)) for x in (-0.45, 0.45) for y in (-0.2, 0.2)]
alch += [P("sphere", "glass", (-0.25, 0, 0.58), (0.09, 0.09, 0.1)), P("cyl", "glass", (-0.25, 0, 0.72), (0.03, 0.03, 0.06)),
         P("sphere", "potion", (-0.25, 0, 0.56), (0.07, 0.07, 0.06)), P("cyl", "potion", (0.1, -0.05, 0.56), (0.04, 0.04, 0.08)),
         P("cyl", "glass", (0.3, 0.05, 0.58), (0.05, 0.05, 0.1)), P("cube", "book_r", (0.15, 0.12, 0.5), (0.12, 0.08, 0.02))]
shot_prop("alchemy_table", alch)
crystal = [P("cyl", "slab2", (0, 0, 0.12), (0.25, 0.25, 0.12), segments=8), P("cyl", "slab", (0, 0, 0.26), (0.18, 0.18, 0.03), segments=8),
           P("cone", "crystal", (0, 0, 0.95), (0.16, 0.16, 0.28), segments=6), P("cone", "crystal", (0, 0, 0.55), (0.16, 0.16, 0.14), rot=(180, 0, 0), segments=6)]
shot_prop("crystal_float", crystal, 128, smooth=False)
rune = [P("cyl", "rune", (0, 0, 0.01), (0.9, 0.9, 0.008), segments=32), P("cyl", "slab2", (0, 0, 0.012), (0.82, 0.82, 0.008), segments=32),
        P("cyl", "rune", (0, 0, 0.014), (0.6, 0.6, 0.008), segments=32), P("cyl", "slab2", (0, 0, 0.016), (0.54, 0.54, 0.008), segments=32)]
for k in range(6):
    a = math.radians(k * 60)
    rune.append(P("cube", "rune", (0.7 * math.cos(a), 0.7 * math.sin(a), 0.02), (0.05, 0.05, 0.006), rot=(0, 0, k * 60 + 45), smooth=False))
shot_prop("rune_circle", rune, 256, outline=0.004, smooth=False)

# Assassinen
curtain = [P("cyl", "wood_d", (0, 0, 1.55), (0.025, 0.025, 0.55), rot=(0, 90, 0))]
curtain += [P("cube", "curtain", (x, 0, 0.8), (0.1, 0.03 + 0.02 * (i % 2), 0.75)) for i, x in enumerate((-0.4, -0.2, 0.0, 0.2, 0.4))]
shot_prop("curtain", curtain)
target = [P("cyl", "wood_d", (x, 0.1, 0.45), (0.03, 0.03, 0.45)) for x in (-0.2, 0.2)]
for k, (r, m) in enumerate(((0.36, "target_w"), (0.27, "target_r"), (0.18, "target_w"), (0.09, "target_r"))):
    target.append(P("cyl", m, (0, -0.02 * k, 0.75), (r, r, 0.03), rot=(90, 0, 0), segments=24))
target += [P("cube", "steel", (0.07 * k - 0.05, -0.12, 0.78 + 0.05 * k), (0.008, 0.06, 0.008), rot=(15, 0, 0)) for k in range(3)]
shot_prop("target", target, 128, smooth=False)
cand = [P("cyl", "gold", (0, 0, 0.03), (0.15, 0.15, 0.03)), P("cyl", "gold", (0, 0, 0.45), (0.025, 0.025, 0.42)),
        P("cyl", "gold", (0, 0, 0.88), (0.2, 0.05, 0.02))]
for x in (-0.18, 0.0, 0.18):
    cand += [P("cyl", "candle", (x, 0, 0.98), (0.03, 0.03, 0.08)), P("cone", "flame", (x, 0, 1.11), (0.025, 0.025, 0.05))]
shot_prop("candles", cand)

# Hoehle
stal = [P("cone", "rock", (0, 0, 0.5), (0.25, 0.22, 0.5), segments=8), P("cone", "rock_d", (0.2, 0.05, 0.3), (0.13, 0.12, 0.3), segments=7),
        P("sphere", "rock_d", (0, 0, 0.05), (0.32, 0.28, 0.08))]
shot_prop("stalagmite", stal)
cluster = [P("sphere", "rock_d", (0, 0, 0.06), (0.3, 0.25, 0.1))]
for k, (x, y, h, tilt) in enumerate(((0, 0, 0.45, 0), (-0.15, 0.05, 0.3, -20), (0.16, 0.02, 0.33, 18), (0.05, -0.12, 0.22, 10))):
    cluster.append(P("cone", "crystal", (x, y, 0.1 + h / 2), (0.07, 0.07, h / 2), rot=(0, tilt, 0), segments=6))
shot_prop("crystal_cluster", cluster, 128, smooth=False)

# Fussmatte (Ausgang)
mat = [P("cube", "mat_b", (0, 0, 0.01), (0.48, 0.28, 0.01), smooth=False), P("cube", "mat", (0, 0, 0.02), (0.42, 0.22, 0.01), smooth=False)]
shot_prop("doormat", mat, 128, outline=0.006, smooth=False)

# ================= Kacheln (64 px, senkrecht von oben, nahtlos)
for o in list(scene.objects):
    if o.type == "MESH":
        bpy.data.objects.remove(o, do_unlink=True)
scene.render.resolution_x = scene.render.resolution_y = 64
scene.render.film_transparent = False
cd = bpy.data.cameras.new("TileCam"); cd.type = "ORTHO"; cd.ortho_scale = 1.0
tcam = bpy.data.objects.new("TileCam", cd); scene.collection.objects.link(tcam)
tcam.location = (0.5, 0.5, 10); tcam.rotation_euler = (0, 0, 0)
scene.camera = tcam


def tile(name, build):
    global count
    build()
    L.render_still(scene, os.path.join(OUT, name + ".png"))
    for o in objs:
        bpy.data.objects.remove(o, do_unlink=True)
    objs.clear()
    count += 1


def planks(a, b):
    P("cube", "gap", (0.5, 0.5, -0.06), (1.5, 1.5, 0.04), smooth=False)
    for i in range(4):
        y = i * 0.25 + 0.125
        off = 0.5 if i % 2 else 0.0
        for k in (-1, 0, 1):
            P("cube", a if (i + k) % 2 else b, (off + k + 0.0, y, 0), (0.49, 0.115, 0.03), smooth=False)


def slabs():
    P("cube", "gap", (0.5, 0.5, -0.06), (1.5, 1.5, 0.04), smooth=False)
    for i in range(2):
        for j in range(2):
            P("cube", "slab" if (i + j) % 2 else "slab2", (0.25 + 0.5 * i, 0.25 + 0.5 * j, 0), (0.235, 0.235, 0.03))


def cavefloor():
    random.seed(9)
    P("cube", "cave", (0.5, 0.5, -0.05), (1.5, 1.5, 0.04), smooth=False)
    for _ in range(9):
        x, y = random.random(), random.random()
        for dx in (-1, 0, 1):
            for dy in (-1, 0, 1):
                if -0.2 <= x + dx <= 1.2 and -0.2 <= y + dy <= 1.2:
                    P("sphere", random.choice(["cave2", "pebble"]), (x + dx, y + dy, 0), (random.uniform(0.05, 0.12),) * 2 + (0.02,), segments=8)


def wall(mat, mortar):
    P("cube", mortar, (0.5, 0.5, -0.06), (1.5, 1.5, 0.04), smooth=False)
    for row in range(3):
        off = 0.25 if row % 2 else 0.0
        for col in (-1, 0, 1, 2):
            P("cube", mat, ((col * 0.5 + off + 0.25), row * 0.333 + 0.166, 0), (0.235, 0.15, 0.05))


tile("Floor_Wood", lambda: planks("plank", "plank2"))
tile("Floor_Stone", slabs)
tile("Floor_Dark", lambda: planks("dark", "dark2"))
tile("Floor_Cave", cavefloor)
tile("Wall_Wood", lambda: wall("wall_w", "gap"))
tile("Wall_Mage", lambda: wall("wall_s", "gap"))
tile("Wall_Dark", lambda: wall("wall_d", "gap"))
tile("Wall_Cave", lambda: wall("wall_c", "rock_d"))

scene.render.film_transparent = True
bpy.ops.wm.save_as_mainfile(filepath=BLEND_PATH)
print(f"RESULT rendered={count} randwarnungen={warnings}")
