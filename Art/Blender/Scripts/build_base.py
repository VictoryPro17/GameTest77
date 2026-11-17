"""RE:BORN - Gegenstaende der Basis (Meilenstein 6, Comic-Look)

Haus (Fachwerk, 256 px), Portal (Steinbogen mit Wirbel, 4 Bilder, 256 px), Amboss, Verkaufsstand mit Markise,
Fass, Kiste (128 px) und Gold-Icon (48 px). Gleiche Kamera, gleicher Massstab und Fusspunkt wie build_props.py.
Eigene Datei: Art/Blender/Reborn_Base.blend. Ausgabe: Assets/_Project/Sprites/Base.

Aufruf:  blender.exe -b --factory-startup --python build_base.py -- <projektordner>
"""
import bpy
import math
import os
import sys
from mathutils import Vector

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import reborn_lib as L  # noqa: E402

argv = sys.argv[sys.argv.index("--") + 1:] if "--" in sys.argv else []
PROJECT = argv[0] if argv else os.path.abspath(os.path.join(os.path.dirname(__file__), "..", "..", ".."))
OUT = os.path.join(PROJECT, "Assets", "_Project", "Sprites", "Base")
BLEND_PATH = os.path.join(PROJECT, "Art", "Blender", "Reborn_Base.blend")
os.makedirs(OUT, exist_ok=True)

bpy.ops.wm.read_factory_settings(use_empty=True)
scene = bpy.context.scene
scene.name = "Base"
L.setup_scene(scene)
cam = L.camera(scene, target=(0, 0, 0.85), ortho=2.1)
L.sun(scene)

M = {"plaster": L.cel("Putz", (0.93, 0.86, 0.7)),
     "beam": L.cel("Balken", (0.38, 0.22, 0.12)),
     "roof": L.cel("Dach", (0.72, 0.28, 0.2)),
     "roof_dark": L.cel("DachDunkel", (0.5, 0.18, 0.14)),
     "stone": L.cel("Stein", (0.58, 0.57, 0.62)),
     "stone_dark": L.cel("SteinDunkel", (0.4, 0.4, 0.46)),
     "door": L.cel("Tuer", (0.45, 0.27, 0.14)),
     "window": L.cel("Fenster", (0.55, 0.78, 0.95), emission=0.2),
     "glow": L.cel("FensterLicht", (1.0, 0.85, 0.45), emission=0.6),
     "metal": L.cel("Metall", (0.36, 0.37, 0.42)),
     "metal_light": L.cel("MetallHell", (0.62, 0.64, 0.7)),
     "gold": L.cel("Gold", (1.0, 0.8, 0.28), light=1.15),
     "wood": L.cel("Holz", (0.6, 0.38, 0.2)),
     "wood_dark": L.cel("HolzDunkel", (0.36, 0.2, 0.1)),
     "cloth_r": L.cel("StoffRot", (0.82, 0.22, 0.2)),
     "cloth_w": L.cel("StoffWeiss", (0.95, 0.92, 0.85)),
     "apple": L.cel("Apfel", (0.85, 0.2, 0.15)),
     "bread": L.cel("Brot", (0.85, 0.6, 0.3)),
     "potion": L.cel("TrankRot", (0.9, 0.2, 0.25), emission=0.3),
     "potion_b": L.cel("TrankBlau", (0.25, 0.45, 0.95), emission=0.3),
     "portal": L.cel("Portal", (0.45, 0.25, 0.95), shadow=0.8, emission=0.6),
     "portal_l": L.cel("PortalHell", (0.75, 0.6, 1.0), shadow=0.9, emission=0.8),
     "rune": L.cel("Rune", (0.55, 0.9, 1.0), shadow=1.0, emission=0.8)}


def P(kind, mat, loc, sc, rot=(0, 0, 0), **kw):
    return L.shape(kind, loc, sc, rot, M[mat], None, collection=scene.collection, **kw)


def make(name, parts, outline=0.016, smooth=True):
    o = L.join(parts, name)
    if smooth:
        L.subdivide(o, 1)
    L.add_outline(o, outline)
    o.hide_render = True
    return o


count = 0
warnings = 0


def render(objs, file, size=128):
    global count, warnings
    for o in objs:
        o.hide_render = False
    scene.render.resolution_x = scene.render.resolution_y = size
    cam.data.ortho_scale = 2.1 * size / 128
    t = 0.85 * size / 128
    pitch = math.radians(45)
    fwd = Vector((0, math.sin(pitch), -math.cos(pitch)))
    cam.location = Vector((0, 0, t)) - fwd * 10
    path = os.path.join(OUT, file)
    L.render_still(scene, path)
    for o in objs:
        o.hide_render = True
    count += 1
    if L.edge_pixels(path):
        warnings += 1
        print("WARNUNG Randpixel:", file)


# ---------------- Haus (Fachwerk): 2,6 m breit, 1,8 m tief, Waende 1,5 m, Satteldach
W, D, H = 1.3, 0.9, 1.5
house = [P("cube", "stone_dark", (0, 0, 0.12), (W + 0.04, D + 0.04, 0.12)),
         P("cube", "plaster", (0, 0, H / 2 + 0.1), (W, D, H / 2 - 0.02))]
for x in (-W, -W / 3, W / 3, W):                                                    # senkrechte Balken vorne
    house.append(P("cube", "beam", (x, -D - 0.01, H / 2 + 0.1), (0.05, 0.03, H / 2), smooth=False))
for z in (0.25, H + 0.05):                                                          # waagrechte Balken
    house.append(P("cube", "beam", (0, -D - 0.015, z), (W + 0.03, 0.03, 0.045), smooth=False))
house.append(P("cube", "beam", (-W * 2 / 3, -D - 0.012, 0.95), (0.03, 0.025, 0.42), rot=(0, 35, 0), smooth=False))
house += [P("cube", "door", (0, -D - 0.02, 0.62), (0.24, 0.03, 0.42)),
          P("sphere", "gold", (0.14, -D - 0.06, 0.6), (0.03, 0.02, 0.03)),
          P("cube", "beam", (0, -D - 0.035, 1.06), (0.3, 0.03, 0.04), smooth=False)]
for x in (-0.85, 0.85):                                                             # Fenster mit Licht und Kreuz
    house += [P("cube", "glow", (x, -D - 0.015, 0.95), (0.2, 0.02, 0.18)),
              P("cube", "beam", (x, -D - 0.03, 0.95), (0.22, 0.02, 0.02), smooth=False),
              P("cube", "beam", (x, -D - 0.03, 0.95), (0.02, 0.02, 0.2), smooth=False),
              P("cube", "wood_dark", (x, -D - 0.05, 0.74), (0.24, 0.05, 0.03), smooth=False)]
roof_slope = 34
for s in (-1, 1):                                                                   # Dachflaechen
    house.append(P("cube", "roof" if s < 0 else "roof_dark", (0, s * (D / 2 + 0.05), H + 0.1 + 0.42),
                   (W + 0.2, D / 2 + 0.28, 0.05), rot=(s * roof_slope, 0, 0), smooth=False))
for x in (-W + 0.02, W - 0.02):                                                     # Giebel
    for z, h in ((H + 0.2, 0.62), (H + 0.5, 0.42), (H + 0.75, 0.2)):
        house.append(P("cube", "plaster", (x, 0, z), (0.02, h, 0.13), smooth=False))
house += [P("cube", "stone", (0.75, 0.25, H + 0.95), (0.13, 0.13, 0.32)),             # Schornstein
          P("cube", "stone_dark", (0.75, 0.25, H + 1.28), (0.15, 0.15, 0.04))]
house_obj = make("House", house, 0.024, smooth=False)
render([house_obj], "house.png", 256)

# ---------------- Portal: Steinbogen mit Runen, Sockel, Wirbel (4 Bilder)
arch = [P("cyl", "stone_dark", (0, 0, 0.06), (0.95, 0.5, 0.06), segments=24),
        P("cyl", "stone", (0, 0, 0.14), (0.85, 0.42, 0.04), segments=24)]
for x in (-0.62, 0.62):
    arch += [P("cube", "stone", (x, 0, 0.75), (0.13, 0.15, 0.6)),
             P("cube", "stone_dark", (x, 0, 0.2), (0.17, 0.19, 0.08)),
             P("cube", "rune", (x, -0.152, 0.75), (0.04, 0.01, 0.12), smooth=False)]
arch.append(P("torus", "stone", (0, 0, 1.35), (0.62, 0.62, 0.62), rot=(90, 0, 0), segments=16))
arch.append(P("cube", "rune", (0, -0.17, 1.98), (0.07, 0.015, 0.07), rot=(0, 45, 0), smooth=False))
arch_obj = make("PortalArch", arch, 0.022)
for frame in range(4):
    a0 = frame * 90 / 4
    swirl = [P("cyl", "portal", (0, 0.02, 1.05), (0.5, 0.5, 0.02), rot=(90, 0, 0), segments=24)]
    for k in range(3):
        swirl.append(P("torus", "portal_l" if k % 2 == 0 else "portal", (0, -0.01 - 0.01 * k, 1.05),
                       (0.42 - k * 0.12, 0.42 - k * 0.12, 0.42 - k * 0.12), rot=(90, a0 * (k + 1), 0), segments=12))
    for k in range(6):
        ang = math.radians(a0 * 2 + k * 60)
        r = 0.18 + 0.07 * (k % 2)
        swirl.append(P("sphere", "rune", (r * math.cos(ang), -0.05, 1.05 + r * math.sin(ang)), (0.035, 0.02, 0.035)))
    swirl_obj = make(f"PortalSwirl{frame}", swirl, 0.012)
    swirl_obj.scale = (1, 1, 0.92)
    render([arch_obj, swirl_obj], f"portal_{frame}.png", 256)

# ---------------- Amboss (mit Hammer) auf Holzklotz
anvil = [P("cyl", "wood_dark", (0, 0, 0.2), (0.22, 0.22, 0.2), segments=12),
         P("cube", "metal", (0, 0, 0.45), (0.12, 0.1, 0.06)),
         P("cube", "metal", (0, 0, 0.56), (0.28, 0.13, 0.06)),
         P("cone", "metal", (0.36, 0, 0.58), (0.08, 0.08, 0.12), rot=(0, 90, 0)),
         P("cube", "metal_light", (0, 0, 0.625), (0.27, 0.12, 0.01), smooth=False),
         P("cyl", "wood", (-0.1, -0.05, 0.66), (0.02, 0.02, 0.16), rot=(0, 80, 20)),
         P("cube", "metal", (-0.26, -0.1, 0.66), (0.05, 0.04, 0.06))]
render([make("Anvil", anvil)], "anvil.png")

# ---------------- Verkaufsstand mit Markise, Waren
stall = [P("cube", "wood", (0, 0, 0.42), (0.7, 0.3, 0.05)),
         P("cube", "wood_dark", (0, -0.29, 0.22), (0.7, 0.02, 0.2), smooth=False)]
for x in (-0.66, 0.66):
    stall.append(P("cyl", "wood_dark", (x, -0.25, 0.7), (0.035, 0.035, 0.7)))
    stall.append(P("cyl", "wood_dark", (x, 0.25, 0.7), (0.035, 0.035, 0.7)))
for i in range(6):                                                                  # Markise rot-weiss
    x = -0.65 + i * 0.26
    stall.append(P("cube", "cloth_r" if i % 2 == 0 else "cloth_w", (x, -0.05, 1.42), (0.13, 0.42, 0.02), rot=(-18, 0, 0), smooth=False))
stall += [P("sphere", "apple", (-0.45 + 0.1 * k, -0.1, 0.5), (0.05, 0.05, 0.05)) for k in range(3)]
stall += [P("sphere", "bread", (0.0, -0.05, 0.5), (0.14, 0.07, 0.05)),
          P("cyl", "potion", (0.35, -0.1, 0.53), (0.045, 0.045, 0.07)),
          P("cyl", "potion_b", (0.48, -0.08, 0.53), (0.045, 0.045, 0.07)),
          P("sphere", "gold", (0.6, -0.12, 0.49), (0.04, 0.04, 0.015))]
render([make("Stall", stall, smooth=False)], "stall.png")   # ohne Glaetten: Markisenstreifen bleiben eckig

# ---------------- Fass, Kiste
barrel = [P("cyl", "wood", (0, 0, 0.32), (0.22, 0.22, 0.32), segments=14),
          P("cyl", "metal", (0, 0, 0.12), (0.23, 0.23, 0.025), segments=14),
          P("cyl", "metal", (0, 0, 0.52), (0.23, 0.23, 0.025), segments=14),
          P("cyl", "wood_dark", (0, 0, 0.645), (0.19, 0.19, 0.01), segments=14)]
render([make("Barrel", barrel)], "barrel.png")
crate = [P("cube", "wood", (0, 0, 0.22), (0.24, 0.22, 0.22)),
         P("cube", "wood_dark", (0, -0.225, 0.22), (0.03, 0.01, 0.2), rot=(0, 45, 0), smooth=False),
         P("cube", "wood_dark", (0, -0.225, 0.22), (0.03, 0.01, 0.2), rot=(0, -45, 0), smooth=False)]
render([make("Crate", crate)], "crate.png")

# ---------------- Gold-Icon (48 px): Muenzstapel von oben-vorne
coins = [P("cyl", "gold", (0.08 * math.cos(k), 0.05 * math.sin(k), 0.03 + 0.045 * k), (0.2, 0.2, 0.02), segments=16) for k in range(4)]
coins.append(P("cyl", "gold", (-0.22, -0.1, 0.03), (0.18, 0.18, 0.02), rot=(70, 0, 15), segments=16))
coin_obj = make("GoldIcon", coins, 0.02)
coin_obj.hide_render = False
scene.render.resolution_x = scene.render.resolution_y = 48
cam.data.ortho_scale = 0.75
cam.location = Vector((0, 0, 0.12)) - Vector((0, math.sin(math.radians(45)), -math.cos(math.radians(45)))) * 10
L.render_still(scene, os.path.join(OUT, "gold.png"))
coin_obj.hide_render = True
count += 1

scene.render.resolution_x = scene.render.resolution_y = 128
bpy.ops.wm.save_as_mainfile(filepath=BLEND_PATH)
print(f"RESULT rendered={count} randwarnungen={warnings} pivot={L.foot_pivot(scene)}")
