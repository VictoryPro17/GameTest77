"""RE:BORN - Gegenstaende in der Welt (v2, Meilenstein 5, Comic-Look)

Truhe (zu/offen), Eiche, Tanne, Fels, Busch, Grasbueschel, Fackel (2 Bilder).
Gleiche Kamera und gleicher Massstab wie die Figuren (128 px = 2,1 m; grosse Teile 256 px = 4,2 m),
Fusspunkt am selben Bildpunkt. Eigene Datei: Art/Blender/Reborn_World.blend (Szene "Props")

Aufruf:  blender.exe -b --factory-startup --python build_props.py -- <projektordner>
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
OUT = os.path.join(PROJECT, "Assets", "_Project", "Sprites", "Props")
BLEND_PATH = os.path.join(PROJECT, "Art", "Blender", "Reborn_World.blend")
os.makedirs(OUT, exist_ok=True)

bpy.ops.wm.read_factory_settings(use_empty=True)
scene = bpy.context.scene
scene.name = "Props"
L.setup_scene(scene)
cam = L.camera(scene, target=(0, 0, 0.85), ortho=2.1)
L.sun(scene)

M = {"wood": L.cel("TruheHolz", (0.55, 0.32, 0.15)),
     "wood_dark": L.cel("TruheHolzDunkel", (0.33, 0.18, 0.08)),
     "metal": L.cel("TruheMetall", (0.45, 0.46, 0.5)),
     "gold": L.cel("TruheGold", (1.0, 0.78, 0.25), light=1.15),
     "inside": L.cel("TruheInnen", (0.12, 0.07, 0.05)),
     "coin": L.cel("Muenzen", (1.0, 0.85, 0.3), emission=0.3),
     "bark": L.cel("Baumrinde", (0.42, 0.28, 0.16)),
     "bark_dark": L.cel("BaumrindeDunkel", (0.26, 0.16, 0.08)),
     "leaf": L.cel("Blaetter", (0.24, 0.55, 0.2)),
     "leaf_light": L.cel("BlaetterHell", (0.4, 0.7, 0.28)),
     "leaf_dark": L.cel("BlaetterDunkel", (0.14, 0.38, 0.14)),
     "pine": L.cel("Tanne", (0.12, 0.42, 0.25)),
     "pine_light": L.cel("TanneHell", (0.22, 0.56, 0.32)),
     "rock": L.cel("Fels", (0.55, 0.55, 0.58)),
     "rock_dark": L.cel("FelsDunkel", (0.36, 0.36, 0.4)),
     "moss": L.cel("FelsMoos", (0.35, 0.58, 0.22)),
     "berry": L.cel("Beeren", (0.85, 0.15, 0.2), emission=0.2),
     "grass": L.cel("GrasHalm", (0.36, 0.66, 0.25)),
     "grass_light": L.cel("GrasHalmHell", (0.55, 0.8, 0.3)),
     "flower": L.cel("Bluete", (1.0, 0.9, 0.35), emission=0.2),
     "fire": L.cel("Flamme", (1.0, 0.6, 0.15), shadow=0.95, emission=1.2),
     "fire_core": L.cel("FlammeKern", (1.0, 0.95, 0.6), shadow=1, emission=1.4)}


def P(kind, mat, loc, sc, rot=(0, 0, 0), **kw):
    return L.shape(kind, loc, sc, rot, M[mat], None, collection=scene.collection, **kw)


def make(name, parts, outline=0.016):
    o = L.join(parts, name)
    L.subdivide(o, 1)
    L.add_outline(o, outline)
    o.hide_render = True
    return o


def render(obj, file, size=128):
    obj.hide_render = False
    scene.render.resolution_x = scene.render.resolution_y = size
    cam.data.ortho_scale = 2.1 * size / 128
    t = 0.85 * size / 128
    pitch = math.radians(45)
    from mathutils import Vector
    fwd = Vector((0, math.sin(pitch), -math.cos(pitch)))
    cam.location = Vector((0, 0, t)) - fwd * 10
    L.render_still(scene, os.path.join(OUT, file))
    obj.hide_render = True


count = 0
# ---------------- Truhe: Koerper, Beschlaege, Schloss, Muenzen; Deckel am hinteren Scharnier
W, D, H, LID = 0.8, 0.5, 0.38, 0.16
body = [P("cube", "wood", (0, 0, H / 2), (W / 2, D / 2, H / 2)),
        P("cube", "wood_dark", (0, 0, 0.03), (W / 2 + 0.01, D / 2 + 0.01, 0.03)),
        P("cube", "inside", (0, 0, H - 0.01), (W / 2 - 0.04, D / 2 - 0.04, 0.015), smooth=False),
        P("cube", "gold", (0, -D / 2 - 0.02, H - 0.06), (0.05, 0.02, 0.06))]
body += [P("cube", "metal", (x, 0, H / 2), (0.035, D / 2 + 0.01, H / 2 + 0.005)) for x in (-0.25, 0.25)]
body += [P("cube", "wood_dark", (0, -D / 2 - 0.003, z), (W / 2 - 0.02, 0.004, 0.006), smooth=False) for z in (0.13, 0.26)]
random.seed(4)
body += [P("sphere", "coin", (random.uniform(-0.28, 0.28), random.uniform(-0.15, 0.15), H + 0.02), (0.06, 0.06, 0.02)) for _ in range(9)]
chest_body = make("ChestBody", body)
hinge = bpy.data.objects.new("ChestHinge", None)
scene.collection.objects.link(hinge)
hinge.location = (0, D / 2, H)
lid_parts = [P("cube", "wood", (0, -D / 2, LID / 2), (W / 2 + 0.01, D / 2 + 0.01, LID / 2)),
             P("cube", "wood_dark", (0, -D / 2, LID + 0.005), (W / 2 - 0.04, D / 2 - 0.05, 0.01), smooth=False)]
lid_parts += [P("cube", "metal", (x, -D / 2, LID / 2 + 0.01), (0.035, D / 2 + 0.02, LID / 2 + 0.02)) for x in (-0.25, 0.25)]
lid = make("ChestLid", lid_parts)
lid.parent = hinge
lid.location = (0, 0, 0)
for name, angle in (("chest_closed", 0), ("chest_open", -105)):
    hinge.rotation_euler = (math.radians(angle), 0, 0)
    lid.hide_render = False
    render(chest_body, name + ".png")
    count += 1
lid.hide_render = True
hinge.rotation_euler = (0, 0, 0)

# ---------------- Eiche (256 px)
random.seed(7)
oak = [P("cyl", "bark", (0, 0, 0.6), (0.17, 0.17, 0.6), taper=0.75, segments=14),
       P("cone", "bark_dark", (0, 0, 0.08), (0.3, 0.3, 0.12)),
       P("cyl", "bark", (0.25, 0, 1.25), (0.06, 0.06, 0.3), rot=(0, 40, 0), taper=0.6),
       P("cyl", "bark", (-0.22, 0, 1.3), (0.06, 0.06, 0.28), rot=(0, -45, 0), taper=0.6)]
for i in range(9):
    a = 2 * math.pi * i / 9
    oak.append(P("sphere", random.choice(["leaf", "leaf", "leaf_dark"]), (0.45 * math.cos(a), 0.3 * math.sin(a), 1.75 + 0.15 * math.sin(3 * a)),
                 (0.45, 0.42, 0.38), segments=14))
oak += [P("sphere", "leaf_light", (0.1, -0.2, 2.2), (0.42, 0.38, 0.34), segments=14),
        P("sphere", "leaf", (0, 0.1, 2.05), (0.55, 0.5, 0.45), segments=14)]
render(make("Oak", oak, 0.024), "tree_oak.png", 256)
count += 1

# ---------------- Tanne (256 px)
pine = [P("cyl", "bark_dark", (0, 0, 0.35), (0.11, 0.11, 0.35), taper=0.8)]
for i, (z, r) in enumerate(((0.9, 0.75), (1.45, 0.6), (1.95, 0.45), (2.4, 0.3))):
    pine.append(P("cone", "pine" if i % 2 == 0 else "pine_light", (0, 0, z), (r, r, 0.5)))
pine.append(P("cone", "pine_light", (0, 0, 2.75), (0.15, 0.15, 0.25)))
render(make("Pine", pine, 0.024), "tree_pine.png", 256)
count += 1

# ---------------- Fels, Busch, Grasbueschel
rock = [P("sphere", "rock", (0, 0, 0.18), (0.42, 0.34, 0.26), segments=10),
        P("sphere", "rock_dark", (0.25, 0.1, 0.12), (0.22, 0.2, 0.16), segments=8),
        P("sphere", "moss", (-0.1, -0.05, 0.36), (0.2, 0.16, 0.06))]
render(make("Rock", rock), "rock.png")
random.seed(11)
bush = [P("sphere", random.choice(["leaf", "leaf_dark", "leaf_light"]), (0.22 * math.cos(a), 0.15 * math.sin(a), 0.25 + 0.05 * math.sin(2 * a)), (0.25, 0.23, 0.22), segments=12)
        for a in [2 * math.pi * i / 6 for i in range(6)]]
bush += [P("sphere", "leaf", (0, 0, 0.38), (0.28, 0.26, 0.24), segments=12)]
bush += [P("sphere", "berry", (random.uniform(-0.3, 0.3), random.uniform(-0.3, -0.15), random.uniform(0.25, 0.5)), (0.035, 0.035, 0.035)) for _ in range(6)]
render(make("Bush", bush), "bush.png")
grass = [P("cone", random.choice(["grass", "grass_light"]), (0.12 * math.cos(a), 0.08 * math.sin(a), 0.12), (0.035, 0.035, 0.14), rot=(15 * math.sin(a), 15 * math.cos(a), 0))
         for a in [2 * math.pi * i / 7 for i in range(7)]]
grass += [P("sphere", "flower", (0.05, -0.08, 0.26), (0.03, 0.03, 0.03))]
render(make("GrassTuft", grass, 0.01), "grass_tuft.png")
count += 3

# ---------------- Fackel (2 Bilder: Flamme flackert)
torch_base = [P("cyl", "wood_dark", (0, 0, 0.45), (0.04, 0.04, 0.45)), P("cyl", "metal", (0, 0, 0.92), (0.07, 0.07, 0.05), taper=1.4)]
tb = make("TorchPost", torch_base)
for i, (h, rz) in enumerate(((0.3, 0), (0.24, 25))):
    flame = make(f"Flame{i}", [P("cone", "fire", (0, 0, 1.05 + h / 2), (0.08, 0.08, h / 2 + 0.05), rot=(0, 0, rz)),
                              P("cone", "fire_core", (0, -0.02, 1.02 + h / 3), (0.04, 0.04, h / 3), rot=(0, 0, rz))], 0.01)
    flame.hide_render = False
    render(tb, f"torch_{i}.png")
    flame.hide_render = True
    count += 1

scene.render.resolution_x = scene.render.resolution_y = 128
bpy.ops.wm.save_as_mainfile(filepath=BLEND_PATH)
print(f"RESULT rendered={count} pivot={L.foot_pivot(scene)}")
