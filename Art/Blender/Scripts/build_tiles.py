"""RE:BORN - Bodenkacheln (Meilenstein 5, Comic-Look)

Gras (4 Varianten), Weg und Steinmauer als nahtlose 64x64-Kacheln (1 Kachel = 1 Einheit in Unity).
Kamera senkrecht von oben; alles, was ueber den Rand ragt, wird auf der Gegenseite wiederholt,
damit die Kacheln ohne Naht aneinanderpassen. Szene "Tiles" in Art/Blender/Reborn_World.blend.

Aufruf:  blender.exe -b Art/Blender/Reborn_World.blend --python build_tiles.py -- <projektordner>
"""
import bpy
import math
import os
import random
import sys

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import reborn_lib as L  # noqa: E402

argv = sys.argv[sys.argv.index("--") + 1:] if "--" in sys.argv else []
PROJECT = argv[0] if argv else os.path.abspath(os.path.join(os.path.dirname(__file__), "..", "..", ".."))
OUT = os.path.join(PROJECT, "Assets", "_Project", "Sprites", "Tiles")
BLEND_PATH = os.path.join(PROJECT, "Art", "Blender", "Reborn_World.blend")
os.makedirs(OUT, exist_ok=True)

scene = bpy.context.scene  # Szene "Props" aus build_props.py; Kacheln bekommen eine eigene Sammlung
coll = bpy.data.collections.new("Tiles")
scene.collection.children.link(coll)
for o in scene.collection.objects:
    o.hide_render = True
for c in scene.collection.children:
    if c is not coll:
        c.hide_render = True
scene.render.resolution_x = scene.render.resolution_y = 64
scene.render.film_transparent = False
cd = bpy.data.cameras.new("TileCamera"); cd.type = "ORTHO"; cd.ortho_scale = 1.0
tcam = bpy.data.objects.new("TileCamera", cd); coll.objects.link(tcam)
tcam.location = (0.5, 0.5, 10); tcam.rotation_euler = (0, 0, 0)
scene.camera = tcam
sun = bpy.data.lights.new("TileSun", "SUN"); sun.energy = 3.0
so = bpy.data.objects.new("TileSun", sun); coll.objects.link(so)
so.rotation_euler = (math.radians(35), math.radians(-20), math.radians(-30))

M = {"ground": L.cel("Boden", (0.26, 0.49, 0.19), shadow=0.85, mid=0.95),
     "ground2": L.cel("Boden2", (0.255, 0.48, 0.185), shadow=0.85, mid=0.95),
     "blade": L.cel("Halm", (0.42, 0.72, 0.28)),
     "blade_dark": L.cel("HalmDunkel", (0.17, 0.36, 0.13)),
     "flower_y": L.cel("BlueteGelb", (1.0, 0.88, 0.35), emission=0.2),
     "flower_w": L.cel("BlueteWeiss", (0.97, 0.97, 0.95), emission=0.2),
     "dirt": L.cel("Erde", (0.6, 0.45, 0.28), shadow=0.85, mid=0.95),
     "dirt_dark": L.cel("ErdeDunkel", (0.48, 0.35, 0.2)),
     "pebble": L.cel("Kiesel", (0.7, 0.68, 0.64)),
     "mortar": L.cel("Fuge", (0.22, 0.2, 0.24), shadow=0.9, mid=0.95),
     "stone": L.cel("Mauerstein", (0.56, 0.55, 0.6)),
     "stone2": L.cel("Mauerstein2", (0.48, 0.47, 0.53)),
     "stone_moss": L.cel("MauerMoos", (0.36, 0.55, 0.26))}
objs = []


def P(kind, mat, loc, sc, rot=(0, 0, 0), **kw):
    o = L.shape(kind, loc, sc, rot, M[mat], None, collection=coll, **kw)
    objs.append(o)
    return o


def wrapped(kind, mat, x, y, z, sc, rot=(0, 0, 0), margin=0.12, **kw):
    """Teil an (x, y) plus Kopien jenseits der Kanten, damit die Kachel nahtlos ist"""
    for dx in (-1, 0, 1):
        for dy in (-1, 0, 1):
            px, py = x + dx, y + dy
            if -margin <= px <= 1 + margin and -margin <= py <= 1 + margin:
                P(kind, mat, (px, py, z), sc, rot, **kw)


def clear():
    for o in objs:
        bpy.data.objects.remove(o, do_unlink=True)
    objs.clear()


def shot(name):
    L.render_still(scene, os.path.join(OUT, name))
    clear()


count = 0
for v in range(4):
    random.seed(100 + v)
    P("cube", "ground" if v % 2 == 0 else "ground2", (0.5, 0.5, -0.05), (1.5, 1.5, 0.05), smooth=False)
    for _ in range(30):
        x, y = random.random(), random.random()
        wrapped("cone", random.choice(["blade", "blade", "blade_dark"]), x, y, 0.02, (0.028, 0.028, 0.08), rot=(random.uniform(-25, 25), random.uniform(-25, 25), 0))
    for _ in range(v % 3):
        x, y = random.random(), random.random()
        wrapped("sphere", random.choice(["flower_y", "flower_w"]), x, y, 0.03, (0.025, 0.025, 0.012))
    shot(f"Ground_Grass_{v}.png")
    count += 1

random.seed(200)
P("cube", "dirt", (0.5, 0.5, -0.05), (1.5, 1.5, 0.05), smooth=False)
for _ in range(10):
    wrapped("sphere", "dirt_dark", random.random(), random.random(), 0.0, (random.uniform(0.05, 0.1), random.uniform(0.04, 0.08), 0.01))
for _ in range(12):
    wrapped("sphere", "pebble", random.random(), random.random(), 0.01, (random.uniform(0.015, 0.03),) * 2 + (0.012,), segments=8)
shot("Ground_Path.png")
count += 1

random.seed(300)
P("cube", "mortar", (0.5, 0.5, -0.05), (1.5, 1.5, 0.05), smooth=False)
for row in range(4):
    off = 0.25 if row % 2 else 0.0
    for col in range(3):
        x = (col * 0.5 + off + 0.25) % 1.0
        y = row * 0.25 + 0.125
        mat = random.choice(["stone", "stone", "stone2"])
        wrapped("cube", mat, x, y, 0.03, (0.225, 0.105, 0.05), margin=0.3)
for _ in range(2):
    wrapped("sphere", "stone_moss", random.random(), random.random(), 0.07, (0.06, 0.04, 0.01))
shot("Wall_Stone.png")
count += 1

scene.render.film_transparent = True
bpy.ops.wm.save_as_mainfile(filepath=BLEND_PATH)
print(f"RESULT rendered={count}")
