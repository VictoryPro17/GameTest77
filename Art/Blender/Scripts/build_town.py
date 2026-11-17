"""RE:BORN - Stadt mit Mauer und Gilden (Meilenstein 7a, Comic-Look)

Gebaeude: Rathaus, Gasthaus, Schmiede (je 384 px), 4 Laeden mit Schild, 6 Wohnhaeuser (256/384 px), Stadttor (384 px),
Gildenhaeuser Magier, Krieger, Assassinen (512 px), Felsen mit Hoehleneingang und Eckturm (384 px),
Mauerstueck (128 px, nahtlos alle 1 Einheit), Mauerturm (256 px), Brunnen (2 Bilder, 256 px).
Kleinzeug (128 px): Laterne, Bank, Blumenkasten, Zaun (nahtlos), Hecke (nahtlos), Heuballen, Marktkiste,
Ziehbrunnen, Wegweiser, Karren, Rauch (3 Bilder).
Bodenkacheln (64 px, senkrecht von oben, nahtlos): Pflaster (3), Pflaster mit Grasrand, Wasser (2), Bruecke.

Gleiche Kamera, gleicher Massstab und Fusspunkt (0,5 / 0,2138) wie build_props.py und build_base.py.
Eigene Datei: Art/Blender/Reborn_Town.blend. Ausgabe: Assets/_Project/Sprites/Town (oder 2. Argument).

Aufruf:  blender.exe -b --factory-startup --python build_town.py -- <projektordner> [<ausgabeordner>]
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
OUT = argv[1] if len(argv) > 1 else os.path.join(PROJECT, "Assets", "_Project", "Sprites", "Town")
BLEND_PATH = os.path.join(PROJECT, "Art", "Blender", "Reborn_Town.blend")
os.makedirs(OUT, exist_ok=True)

bpy.ops.wm.read_factory_settings(use_empty=True)
scene = bpy.context.scene
scene.name = "Town"
L.setup_scene(scene)
cam = L.camera(scene, target=(0, 0, 0.85), ortho=2.1)
L.sun(scene)

M = {"plaster": L.cel("Putz", (0.93, 0.86, 0.7)),
     "plaster_rose": L.cel("PutzRosa", (0.92, 0.72, 0.66)),
     "plaster_blue": L.cel("PutzBlau", (0.72, 0.8, 0.88)),
     "plaster_yellow": L.cel("PutzGelb", (0.95, 0.84, 0.5)),
     "beam": L.cel("Balken", (0.38, 0.22, 0.12)),
     "roof": L.cel("Dach", (0.72, 0.28, 0.2)),
     "roof_dark": L.cel("DachDunkel", (0.5, 0.18, 0.14)),
     "roof_blue": L.cel("DachBlau", (0.3, 0.38, 0.6)),
     "roof_blue_dark": L.cel("DachBlauDunkel", (0.2, 0.26, 0.44)),
     "roof_green": L.cel("DachGruen", (0.3, 0.52, 0.3)),
     "roof_green_dark": L.cel("DachGruenDunkel", (0.2, 0.36, 0.2)),
     "stone": L.cel("Stein", (0.58, 0.57, 0.62)),
     "stone_dark": L.cel("SteinDunkel", (0.4, 0.4, 0.46)),
     "stone_light": L.cel("SteinHell", (0.74, 0.72, 0.74)),
     "door": L.cel("Tuer", (0.45, 0.27, 0.14)),
     "glow": L.cel("FensterLicht", (1.0, 0.85, 0.45), emission=0.6),
     "fire": L.cel("Feuer", (1.0, 0.55, 0.15), shadow=0.9, emission=1.2),
     "fire_core": L.cel("FeuerKern", (1.0, 0.9, 0.4), shadow=1.0, emission=1.5),
     "metal": L.cel("Metall", (0.36, 0.37, 0.42)),
     "metal_light": L.cel("MetallHell", (0.62, 0.64, 0.7)),
     "gold": L.cel("Gold", (1.0, 0.8, 0.28), light=1.15),
     "wood": L.cel("Holz", (0.6, 0.38, 0.2)),
     "wood_dark": L.cel("HolzDunkel", (0.36, 0.2, 0.1)),
     "cloth_r": L.cel("StoffRot", (0.82, 0.22, 0.2)),
     "cloth_b": L.cel("StoffBlau", (0.22, 0.36, 0.78)),
     "cloth_w": L.cel("StoffWeiss", (0.95, 0.92, 0.85)),
     "leaf": L.cel("Laub", (0.3, 0.58, 0.24)),
     "leaf_dark": L.cel("LaubDunkel", (0.2, 0.42, 0.18)),
     "flower_r": L.cel("BlumeRot", (0.92, 0.25, 0.3), emission=0.1),
     "flower_y": L.cel("BlumeGelb", (1.0, 0.85, 0.3), emission=0.1),
     "flower_p": L.cel("BlumeLila", (0.7, 0.4, 0.9), emission=0.1),
     "hay": L.cel("Heu", (0.9, 0.78, 0.38)),
     "hay_dark": L.cel("HeuDunkel", (0.72, 0.58, 0.24)),
     "cabbage": L.cel("Kohl", (0.55, 0.78, 0.35)),
     "carrot": L.cel("Moehre", (0.95, 0.5, 0.15)),
     "apple": L.cel("Apfel", (0.85, 0.2, 0.15)),
     "water": L.cel("Wasser", (0.3, 0.6, 0.9), shadow=0.85, mid=0.95, emission=0.25),
     "water_light": L.cel("WasserHell", (0.65, 0.88, 1.0), shadow=0.95, emission=0.5),
     "smoke": L.cel("Rauch", (0.72, 0.72, 0.76), shadow=0.8, mid=0.92),
     "smoke_light": L.cel("RauchHell", (0.86, 0.86, 0.9), shadow=0.85, mid=0.95),
     # Gilden und Hoehle (Meilenstein 7a)
     "mage_roof": L.cel("MagierDach", (0.42, 0.25, 0.7)),
     "mage_glow": L.cel("MagierLicht", (0.6, 0.85, 1.0), shadow=0.95, emission=0.9),
     "assassin_trim": L.cel("AssassinenRot", (0.45, 0.1, 0.12)),
     "assassin_glow": L.cel("AssassinenLicht", (1.0, 0.3, 0.25), shadow=0.95, emission=0.9),
     "assassin_roof": L.cel("AssassinenDach", (0.22, 0.2, 0.26)),
     "assassin_roof_dark": L.cel("AssassinenDachDunkel", (0.13, 0.12, 0.16)),
     "rock": L.cel("Fels", (0.55, 0.52, 0.5)),
     "rock_dark": L.cel("FelsDunkel", (0.4, 0.38, 0.37)),
     "moss": L.cel("Moos", (0.36, 0.55, 0.26)),
     "cave_dark": L.cel("Hoehle", (0.06, 0.05, 0.08), shadow=1.0, mid=1.0),
     "portal_glow": L.cel("PortalSchein", (0.6, 0.4, 1.0), shadow=0.95, emission=1.0)}


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
    """Wie build_base.py: Kamera mit der Bildgroesse mitskaliert, Fusspunkt bleibt gleich."""
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


# ================================================================ Bausteine
def window(parts, x, y, z, w=0.2, h=0.18, frame="beam", box=None):
    """Leuchtendes Fenster mit Kreuz (und optional Blumenkasten) an der Vorderwand y."""
    parts += [P("cube", "glow", (x, y + 0.005, z), (w, 0.02, h)),
              P("cube", frame, (x, y - 0.01, z), (w + 0.02, 0.02, 0.02), smooth=False),
              P("cube", frame, (x, y - 0.01, z), (0.02, 0.02, h + 0.02), smooth=False),
              P("cube", "wood_dark", (x, y - 0.03, z - h - 0.03), (w + 0.04, 0.05, 0.03), smooth=False)]
    if box:
        parts.append(P("cube", "wood", (x, y - 0.08, z - h - 0.09), (w + 0.02, 0.06, 0.05)))
        for k in range(4):
            parts.append(P("sphere", box[k % len(box)], (x - w + 0.06 + k * (2 * w - 0.12) / 3, y - 0.09, z - h - 0.02),
                           (0.05, 0.05, 0.05), segments=8))


def gable_roof(parts, W, D, z0, mat, mat_dark, slope=34, over=0.2, gable="plaster"):
    """Satteldach (First entlang x) mit Ziegelreihen, Firstbalken und Giebelwaenden auf Hoehe z0."""
    half = D / 2 + 0.28
    zc = z0 + 0.1 + 0.42 * D / 0.9
    for s in (-1, 1):
        a = math.radians(-s * slope)
        cy = s * (D / 2 + 0.05)
        parts.append(P("cube", mat if s < 0 else mat_dark, (0, cy, zc), (W + over, half, 0.05), rot=(-s * slope, 0, 0), smooth=False))
        if s < 0:                                          # Ziegelreihen auf der sichtbaren Dachflaeche
            rows = max(3, int(half / 0.11))
            for k in range(rows):
                t = -half + (k + 0.6) * 2 * half / rows
                pos = (0, cy + t * math.cos(a) - 0.055 * math.sin(a), zc + t * math.sin(a) + 0.055 * math.cos(a))
                parts.append(P("cube", mat_dark, pos, (W + over, 0.012, 0.014), rot=(-s * slope, 0, 0), smooth=False))
            for k in range(int((W + over) / 0.18)):        # versetzte Ziegelkanten
                x = -W - over + 0.12 + k * 0.36
                parts.append(P("cube", mat_dark, (x, cy - 0.06 * math.sin(a), zc + 0.06 * math.cos(a)),
                               (0.008, half * 0.95, 0.008), rot=(-s * slope, 0, 0), smooth=False))
    top = zc + half * math.sin(math.radians(slope)) + 0.02
    parts.append(P("cyl", mat_dark, (0, 0, top), (0.05, 0.05, W + over), rot=(0, 90, 0), segments=8))
    ridge = 0.62 * D / 0.9
    for x in (-W + 0.02, W - 0.02):
        for z, h in ((z0 + 0.2, ridge), (z0 + 0.2 + 0.3 * D / 0.9, ridge * 0.68), (z0 + 0.2 + 0.55 * D / 0.9, ridge * 0.32)):
            parts.append(P("cube", gable, (x, 0, z), (0.02, h, 0.13 * D / 0.9), smooth=False))


def chimney(parts, x, y, z, h=0.32):
    parts += [P("cube", "stone", (x, y, z + h), (0.13, 0.13, h)),
              P("cube", "stone_dark", (x, y, z + 2 * h + 0.03), (0.15, 0.15, 0.04))]


def timber_front(parts, W, D, z0, h, xs):
    """Fachwerk-Balken an der Vorderwand: Senkrechte bei xs, Waagrechte oben und unten."""
    for x in xs:
        parts.append(P("cube", "beam", (x, -D - 0.01, z0 + h / 2), (0.05, 0.03, h / 2), smooth=False))
    for z in (z0 + 0.03, z0 + h):
        parts.append(P("cube", "beam", (0, -D - 0.015, z), (W + 0.03, 0.03, 0.045), smooth=False))


def door(parts, x, y, h=0.42, w=0.24, mat="door"):
    parts += [P("cube", mat, (x, y - 0.02, 0.12 + h), (w, 0.03, h)),
              P("sphere", "gold", (x + w * 0.6, y - 0.06, 0.1 + h), (0.03, 0.02, 0.03)),
              P("cube", "beam", (x, y - 0.035, 0.14 + 2 * h), (w + 0.06, 0.03, 0.04), smooth=False),
              P("cube", "stone_light", (x, y - 0.12, 0.05), (w + 0.08, 0.1, 0.05))]


# ================================================================ Wohnhaeuser (6 Varianten, 1 oder 2 Stockwerke)
def town_house(name, plaster, roof, roof_dark, flowers, chim_x, variant, floors=1, W=1.2, size=256):
    D, H1 = 0.85, 1.45
    H = H1 if floors == 1 else H1 + 1.05
    p = [P("cube", "stone_dark", (0, 0, 0.12), (W + 0.04, D + 0.04, 0.12)),
         P("cube", plaster, (0, 0, H / 2 + 0.1), (W, D, H / 2 - 0.02))]
    xs = (-W, -W / 3, W / 3, W) if variant != 1 else (-W, 0, W)
    timber_front(p, W, D, 0.22, H1 - 0.1, xs)
    door(p, -0.35 if variant == 2 else 0.0, -D)
    wx = (0.45, 0.85) if variant == 2 else (-0.8, 0.8)
    for x in wx:
        window(p, x * W / 1.2, -D, 0.95, box=flowers)
    if floors == 2:                                        # Obergeschoss mit Balkon oder Fensterreihe
        timber_front(p, W, D, H1 + 0.02, 1.0, xs)
        for x in (-0.75, 0.0, 0.75):
            window(p, x * W / 1.2, -D, H1 + 0.6, w=0.17, h=0.17, box=flowers if variant != 1 else None)
        if variant == 1:
            p.append(P("cube", "wood", (0, -D - 0.2, H1 + 0.12), (W * 0.7, 0.2, 0.03), smooth=False))
            for k in range(7):
                p.append(P("cube", "wood_dark", (-W * 0.7 + k * W * 0.7 / 3, -D - 0.38, H1 + 0.28), (0.02, 0.02, 0.15), smooth=False))
            p.append(P("cube", "wood_dark", (0, -D - 0.38, H1 + 0.44), (W * 0.7, 0.025, 0.02), smooth=False))
    gable_roof(p, W, D, H, roof, roof_dark, gable=plaster)
    chimney(p, chim_x * W / 1.2, 0.25, H + 0.6)
    if variant == 1 and floors == 1:                       # Dachgaube
        p += [P("cube", plaster, (0.0, -0.35, H + 0.55), (0.22, 0.14, 0.16)),
              P("cube", "glow", (0.0, -0.5, H + 0.55), (0.11, 0.01, 0.09)),
              P("cube", roof_dark, (0.0, -0.38, H + 0.76), (0.28, 0.2, 0.04), rot=(20, 0, 0), smooth=False)]
    render([make(name, p, 0.024, smooth=False)], name + ".png", size)


town_house("house_red", "plaster", "roof", "roof_dark", ("flower_r", "flower_y"), 0.75, 0)
town_house("house_blue", "plaster_blue", "roof_blue", "roof_blue_dark", ("flower_y", "flower_p"), -0.7, 1)
town_house("house_green", "plaster_rose", "roof_green", "roof_green_dark", ("flower_p", "flower_r"), 0.8, 2)
town_house("house_tall_red", "plaster_yellow", "roof", "roof_dark", ("flower_r", "flower_p"), -0.75, 0, floors=2, size=384)
town_house("house_tall_blue", "plaster", "roof_blue", "roof_blue_dark", ("flower_y", "flower_r"), 0.75, 1, floors=2, size=384)
town_house("house_wide_green", "plaster_blue", "roof_green", "roof_green_dark", ("flower_p", "flower_y"), 1.2, 0, W=1.8, size=384)


# ================================================================ Laeden mit Schild (256 px)
def town_shop(name, plaster, roof, roof_dark, cloth, sign):
    W, D, H = 1.2, 0.8, 1.4
    p = [P("cube", "stone_dark", (0, 0, 0.12), (W + 0.04, D + 0.04, 0.12)),
         P("cube", plaster, (0, 0, H / 2 + 0.1), (W, D, H / 2 - 0.02))]
    timber_front(p, W, D, 0.22, H - 0.1, (-W, W))
    door(p, 0.0, -D)
    window(p, -0.72, -D, 0.85, w=0.3, h=0.24)
    window(p, 0.72, -D, 0.85, w=0.3, h=0.24)
    for i in range(8):                                     # Markise ueber der Front, vorne tiefer
        p.append(P("cube", cloth if i % 2 == 0 else "cloth_w", (-1.05 + i * 0.3, -D - 0.25, 1.3), (0.15, 0.28, 0.02),
                   rot=(25, 0, 0), smooth=False))
    p.append(P("cube", "wood", (0, -D - 0.06, 1.66), (0.34, 0.03, 0.16)))       # Schild
    y = -D - 0.11
    if sign == "sword":
        p += [P("cube", "metal_light", (0, y, 1.69), (0.025, 0.01, 0.12), smooth=False),
              P("cube", "gold", (0, y, 1.56), (0.08, 0.01, 0.02), smooth=False),
              P("cube", "metal_light", (0.0, y, 1.66), (0.025, 0.01, 0.12), rot=(0, 90, 0), smooth=False)]
    elif sign == "shield":
        p += [P("cyl", "cloth_b", (0, y, 1.66), (0.12, 0.02, 0.12), rot=(90, 0, 0), segments=12),
              P("cube", "gold", (0, y - 0.01, 1.66), (0.02, 0.01, 0.1), smooth=False),
              P("cube", "gold", (0, y - 0.01, 1.68), (0.08, 0.01, 0.02), smooth=False)]
    elif sign == "potion":
        p += [P("sphere", "cloth_r", (0, y, 1.62), (0.09, 0.03, 0.09), segments=10),
              P("cyl", "glow", (0, y, 1.74), (0.03, 0.02, 0.04), segments=8)]
    else:
        p += [P("cyl", "cloth_r", (-0.14, y, 1.66), (0.05, 0.02, 0.08), rot=(90, 0, 0)),
              P("sphere", "gold", (0.14, y, 1.66), (0.07, 0.02, 0.07))]
    gable_roof(p, W, D, H, roof, roof_dark, gable=plaster)
    render([make(name, p, 0.024, smooth=False)], name + ".png", 256)


town_shop("shop", "plaster_yellow", "roof_green", "roof_green_dark", "cloth_b", "goods")
town_shop("shop_weapons", "plaster", "roof", "roof_dark", "cloth_r", "sword")
town_shop("shop_armor", "plaster_blue", "roof_blue", "roof_blue_dark", "cloth_b", "shield")
town_shop("shop_potions", "plaster_rose", "roof_green", "roof_green_dark", "cloth_r", "potion")

# ================================================================ Gildenhaeuser (von aussen, 768 px)
# Die Gildenhaeuser sind die Wahrzeichen der Stadt: Modell x GUILD_SCALE, um den Fusspunkt (Ursprung) skaliert,
# Kontur entsprechend duenner, damit sie im Bild gleich dick bleibt.
GUILD_SCALE = 1.5


def scaled(obj, k=None):
    k = k or GUILD_SCALE
    obj.scale = (k, k, k)
    return obj


# Magierturm: runder Steinturm mit violettem Spitzdach, Sternen und leuchtenden Fenstern
mage = [P("cyl", "stone_dark", (0, 0.2, 0.12), (1.0, 1.0, 0.12), segments=20),
        P("cyl", "stone_light", (0, 0.2, 1.6), (0.85, 0.85, 1.5), segments=20),
        P("cyl", "stone", (0, 0.2, 3.15), (0.95, 0.95, 0.08), segments=20),
        P("cyl", "stone_light", (0, 0.2, 3.8), (0.7, 0.7, 0.6), segments=20),
        P("cone", "mage_roof", (0, 0.2, 5.0), (0.95, 0.95, 0.95), segments=20),
        P("sphere", "mage_glow", (0, 0.2, 6.05), (0.12, 0.12, 0.12)),
        P("cube", "door", (0, -0.62, 0.7), (0.3, 0.05, 0.55)),
        P("cube", "mage_glow", (0, -0.66, 1.33), (0.12, 0.01, 0.06), smooth=False),
        P("cube", "stone_light", (0, -0.85, 0.06), (0.5, 0.2, 0.06))]
for z in (1.9, 2.7, 3.8):
    for x in (-0.38, 0.38):
        r = 0.85 if z < 3.5 else 0.7
        mage.append(P("cube", "mage_glow", (x, 0.2 - math.sqrt(r * r - x * x) - 0.015, z), (0.08, 0.02, 0.16)))
for k in range(5):                                         # Sterne am Dach
    a = 2 * math.pi * k / 5
    mage.append(P("sphere", "gold", (0.45 * math.cos(a), 0.2 - 0.45 * abs(math.sin(a)) - 0.2, 4.7 + 0.3 * (k % 2)), (0.05, 0.02, 0.05), segments=6))
for x in (-1.2, 1.2):                                      # Kristallsaeulen
    mage += [P("cyl", "stone", (x, -0.5, 0.35), (0.12, 0.12, 0.35), segments=8),
             P("cone", "mage_glow", (x, -0.5, 0.9), (0.1, 0.1, 0.25), segments=6)]
render([scaled(make("GuildMage", mage, 0.03 / GUILD_SCALE, smooth=False))], "guild_mage.png", 768)

# Kriegerhalle: breite Steinhalle, rotes Dach, Banner, Schild mit gekreuzten Schwertern
W, D, H = 2.6, 1.2, 2.0
war = [P("cube", "stone_dark", (0, 0, 0.12), (W + 0.06, D + 0.06, 0.12)),
       P("cube", "stone", (0, 0, H / 2 + 0.1), (W, D, H / 2 - 0.02), smooth=False),
       P("cube", "stone_dark", (0, -D - 0.01, H + 0.04), (W + 0.04, 0.04, 0.08), smooth=False)]
for x in (-2.0, -1.2, 1.2, 2.0):
    window(war, x, -D, 1.3, w=0.16, h=0.3, frame="stone_dark")
for x in (-0.8, 0.8):
    war += [P("cube", "cloth_r", (x, -D - 0.03, 1.25), (0.2, 0.02, 0.6), smooth=False),
            P("cone", "cloth_r", (x, -D - 0.03, 0.55), (0.2, 0.02, 0.12), rot=(180, 0, 0), segments=3)]
door(war, 0.0, -D, h=0.5, w=0.4)
war += [P("cyl", "metal", (0, -D - 0.06, 1.62), (0.24, 0.03, 0.24), rot=(90, 0, 0), segments=16),     # Schild ueber der Tuer
        P("cyl", "cloth_r", (0, -D - 0.09, 1.62), (0.18, 0.01, 0.18), rot=(90, 0, 0), segments=16),
        P("cube", "metal_light", (0, -D - 0.12, 1.62), (0.035, 0.01, 0.34), rot=(0, 40, 0), smooth=False),
        P("cube", "metal_light", (0, -D - 0.12, 1.62), (0.035, 0.01, 0.34), rot=(0, -40, 0), smooth=False)]
for x in (-W - 0.15, W + 0.15):                            # Fackeln
    war += [P("cyl", "wood_dark", (x, -D - 0.1, 0.6), (0.04, 0.04, 0.6)),
            P("cone", "fire", (x, -D - 0.1, 1.32), (0.08, 0.08, 0.14))]
gable_roof(war, W, D, H, "roof", "roof_dark", slope=30, gable="stone")
render([scaled(make("GuildWarrior", war, 0.03 / GUILD_SCALE, smooth=False))], "guild_warrior.png", 768)

# Assassinenhaus: dunkles Holz, schwarz-rotes Dach, schmale Fenster, Dolch-Zeichen, Laternen mit rotem Licht
W, D, H = 1.9, 1.0, 1.9
sin = [P("cube", "stone_dark", (0, 0, 0.12), (W + 0.05, D + 0.05, 0.12)),
       P("cube", "wood_dark", (0, 0, H / 2 + 0.1), (W, D, H / 2 - 0.02), smooth=False)]
for x in (-W, -0.95, 0.95, W):
    sin.append(P("cube", "assassin_trim", (x, -D - 0.01, H / 2 + 0.1), (0.05, 0.03, H / 2), smooth=False))
for x in (-1.45, 1.45):
    sin += [P("cube", "assassin_glow", (x, -D - 0.005, 1.2), (0.07, 0.02, 0.28)),
            P("cube", "assassin_trim", (x, -D - 0.02, 1.2), (0.09, 0.02, 0.02), smooth=False)]
door(sin, 0.0, -D, h=0.55, w=0.3, mat="assassin_trim")
sin += [P("cube", "assassin_trim", (0, -D - 0.03, 1.55), (0.28, 0.02, 0.2)),               # Dolch-Zeichen
        P("cube", "metal_light", (0, -D - 0.05, 1.58), (0.025, 0.01, 0.14), smooth=False),
        P("cube", "gold", (0, -D - 0.05, 1.43), (0.07, 0.01, 0.015), smooth=False)]
for x in (-0.6, 0.6):
    sin += [P("cube", "metal", (x, -D - 0.14, 1.45), (0.02, 0.12, 0.02), smooth=False),
            P("cube", "assassin_glow", (x, -D - 0.26, 1.32), (0.05, 0.05, 0.08), smooth=False)]
gable_roof(sin, W, D, H, "assassin_roof", "assassin_roof_dark", slope=40, gable="wood_dark")
chimney(sin, 1.1, 0.3, H + 0.8)
render([scaled(make("GuildAssassin", sin, 0.03 / GUILD_SCALE, smooth=False))], "guild_assassin.png", 768)

# ================================================================ Felsen mit Hoehleneingang zum Portal (384 px)
random.seed(21)
rock = [P("sphere", "rock", (0, 0.35, 0.9), (2.0, 1.1, 1.15), segments=14),
        P("sphere", "rock_dark", (-1.4, 0.1, 0.55), (0.8, 0.7, 0.6), segments=10),
        P("sphere", "rock_dark", (1.5, 0.2, 0.6), (0.75, 0.65, 0.7), segments=10),
        P("sphere", "rock", (0.5, 0.6, 1.8), (1.0, 0.7, 0.6), segments=10),
        P("sphere", "moss", (-0.7, 0.4, 2.0), (0.6, 0.45, 0.15), segments=10),
        P("sphere", "moss", (1.0, 0.5, 2.15), (0.5, 0.35, 0.12), segments=10),
        P("cyl", "cave_dark", (0, -0.62, 0.62), (0.55, 0.62, 0.05), rot=(90, 0, 0), segments=16),  # Hoehlenloch
        P("cyl", "portal_glow", (0, -0.6, 0.55), (0.32, 0.4, 0.02), rot=(90, 0, 0), segments=14)]
for k in range(7):                                         # Steinbogen um den Eingang
    a = math.pi * k / 6
    rock.append(P("sphere", "rock_dark", (0.65 * math.cos(a), -0.68, 0.55 + 0.7 * math.sin(a)), (0.16, 0.1, 0.14), segments=8))
for x in (-0.9, 0.9):
    rock += [P("cyl", "wood_dark", (x, -0.85, 0.5), (0.04, 0.04, 0.5)),
             P("cone", "fire", (x, -0.85, 1.08), (0.08, 0.08, 0.14))]
rock += [P("cube", "wood", (1.4, -0.95, 0.75), (0.32, 0.02, 0.14)),                        # Schild "Portal"
         P("cyl", "wood_dark", (1.4, -0.95, 0.35), (0.03, 0.03, 0.35)),
         P("sphere", "portal_glow", (1.4, -0.98, 0.75), (0.07, 0.01, 0.07), segments=8)]
render([make("CaveRock", rock, 0.03)], "cave_rock.png", 384)

# ================================================================ Mauer-Eckturm (256 px, mit Fahne)
ctower = [P("cyl", "stone", (0, 0, 1.5), (0.75, 0.75, 1.5), segments=18),
          P("cyl", "stone_dark", (0, 0, 0.1), (0.8, 0.8, 0.1), segments=18),
          P("cyl", "stone_light", (0, 0, 3.04), (0.85, 0.85, 0.06), segments=18),
          P("cube", "glow", (0, -0.74, 2.0), (0.06, 0.02, 0.15)),
          P("cyl", "wood_dark", (0, 0.2, 3.7), (0.025, 0.025, 0.6)),
          P("cube", "cloth_b", (0.18, 0.2, 4.1), (0.18, 0.01, 0.11), smooth=False)]
for k in range(10):
    a = 2 * math.pi * k / 10
    ctower.append(P("cube", "stone", (0.78 * math.cos(a), 0.78 * math.sin(a), 3.22), (0.11, 0.11, 0.13), rot=(0, 0, math.degrees(a)), smooth=False))
render([make("CornerTower", ctower, 0.025, smooth=False)], "corner_tower.png", 384)

# ================================================================ Rathaus (384 px): Steinbau mit Uhrturm
W, D, H = 2.3, 1.1, 2.1
hall = [P("cube", "stone_dark", (0, 0, 0.12), (W + 0.05, D + 0.05, 0.12)),
        P("cube", "stone_light", (0, 0, H / 2 + 0.1), (W, D, H / 2 - 0.02), smooth=False),
        P("cube", "stone", (0, -D - 0.01, H + 0.04), (W + 0.04, 0.04, 0.08), smooth=False)]
for x in (-1.9, -1.1, 1.1, 1.9):
    window(hall, x, -D, 1.25, w=0.2, h=0.32, frame="stone_dark")
for x in (-0.7, 0.7):                                      # Saeulen und Banner neben der Tuer
    hall += [P("cyl", "stone", (x, -D - 0.12, 1.0), (0.09, 0.09, 0.9), segments=12),
             P("cube", "cloth_r", (x * 1.2, -D - 0.03, 1.4), (0.12, 0.02, 0.38), smooth=False),
             P("cube", "gold", (x * 1.2, -D - 0.05, 1.45), (0.05, 0.01, 0.05), rot=(0, 45, 0), smooth=False)]
door(hall, 0.0, -D, h=0.6, w=0.36)
hall += [P("cube", "stone", (0, -D - 0.25, 0.06), (0.9, 0.2, 0.06)),            # Treppe
         P("cube", "stone_light", (0, -D - 0.12, 0.14), (0.75, 0.14, 0.04))]
gable_roof(hall, W, D, H, "roof_blue", "roof_blue_dark", slope=30, gable="stone_light")
TW = 0.45                                                   # Uhrturm in der Mitte
hall += [P("cube", "stone_light", (0, -0.2, H + 0.8), (TW, TW, 0.8), smooth=False),
         P("cube", "stone", (0, -0.2, H + 1.62), (TW + 0.05, TW + 0.05, 0.05), smooth=False),
         P("cyl", "cloth_w", (0, -0.2 - TW - 0.01, H + 1.05), (0.26, 0.26, 0.02), rot=(90, 0, 0), segments=20),
         P("torus", "gold", (0, -0.2 - TW - 0.02, H + 1.05), (0.27, 0.27, 0.27), rot=(90, 0, 0), segments=16),
         P("cube", "metal", (0, -0.2 - TW - 0.04, H + 1.13), (0.015, 0.01, 0.1), smooth=False),
         P("cube", "metal", (0.06, -0.2 - TW - 0.04, H + 1.05), (0.07, 0.01, 0.015), smooth=False),
         P("cone", "roof_blue", (0, -0.2, H + 2.05), (0.58, 0.58, 0.45), segments=4, rot=(0, 0, 45)),
         P("sphere", "gold", (0, -0.2, H + 2.55), (0.06, 0.06, 0.06))]
for x in (-0.25, 0.25):
    hall.append(P("cube", "glow", (x, -0.2 - TW - 0.01, H + 0.45), (0.06, 0.01, 0.16)))
render([make("TownHall", hall, 0.03, smooth=False)], "town_hall.png", 384)

# ================================================================ Gasthaus (384 px): Steinsockel, Fachwerk oben, Schild
W, D, H1, H2 = 2.0, 1.0, 1.25, 1.2
inn = [P("cube", "stone_dark", (0, 0, 0.12), (W + 0.05, D + 0.05, 0.12)),
       P("cube", "stone", (0, 0, H1 / 2 + 0.12), (W, D, H1 / 2 - 0.02), smooth=False),
       P("cube", "plaster", (0, -0.05, H1 + H2 / 2 + 0.1), (W + 0.08, D + 0.1, H2 / 2), smooth=False),
       P("cube", "beam", (0, -D - 0.12, H1 + 0.12), (W + 0.12, 0.05, 0.05), smooth=False)]
for x in (-W - 0.05, -1.0, 0.0, 1.0, W + 0.05):
    inn.append(P("cube", "beam", (x, -D - 0.16, H1 + H2 / 2 + 0.1), (0.05, 0.03, H2 / 2), smooth=False))
for x in (-1.5, -0.5, 0.5, 1.5):
    window(inn, x, -D - 0.15, H1 + 0.72, w=0.2, h=0.2, box=("flower_r", "flower_y", "flower_p"))
for x in (-1.3, 1.3):
    window(inn, x, -D, 0.75, w=0.3, h=0.22)
door(inn, 0.0, -D, h=0.5, w=0.32)
gable_roof(inn, W + 0.05, D + 0.1, H1 + H2, "roof", "roof_dark", gable="plaster")
chimney(inn, -1.4, 0.3, H1 + H2 + 0.7, h=0.38)
inn += [P("cube", "metal", (0.75, -D - 0.45, 1.25), (0.3, 0.02, 0.02), smooth=False),       # Schild am Ausleger
        P("cube", "wood", (0.9, -D - 0.45, 1.0), (0.2, 0.025, 0.16)),
        P("cyl", "gold", (0.9, -D - 0.48, 1.0), (0.07, 0.07, 0.09), rot=(90, 0, 0), segments=12),
        P("cube", "cloth_w", (0.9, -D - 0.5, 1.09), (0.07, 0.01, 0.02), smooth=False),
        P("torus", "gold", (1.0, -D - 0.48, 1.0), (0.04, 0.04, 0.04), rot=(90, 0, 0), segments=8)]
for x in (-0.6, 0.6):                                       # Fass neben der Tuer
    inn += [P("cyl", "wood", (x, -D - 0.35, 0.25), (0.17, 0.17, 0.25), segments=12),
            P("cyl", "metal", (x, -D - 0.35, 0.4), (0.18, 0.18, 0.02), segments=12)]
render([make("Inn", inn, 0.03, smooth=False)], "inn.png", 384)

# ================================================================ Schmiede (384 px): Steinwaende, offene Front, Esse
# Geschlossenes Steinhaus hinten, Esse mit Feuer und eigenem Kamin vorne links (von oben gut sichtbar),
# Schleifstein, Wasserfass und Waffenstaender vorne rechts. Der Amboss steht in Unity davor (anvil.png).
W, D, H = 1.5, 0.7, 1.45
Y0 = 0.35                                                   # Haus nach hinten versetzt
smithy = [P("cube", "stone_dark", (0.35, Y0, 0.1), (W + 0.04, D + 0.04, 0.1), smooth=False),
          P("cube", "stone", (0.35, Y0, H / 2 + 0.1), (W, D, H / 2 - 0.02), smooth=False),
          P("cube", "fire", (0.35, Y0 - D - 0.005, 0.62), (0.34, 0.02, 0.45)),           # offene Tuer mit Glut
          P("cube", "beam", (0.35, Y0 - D - 0.03, 1.1), (0.42, 0.03, 0.05), smooth=False),
          P("cube", "beam", (-0.07, Y0 - D - 0.03, 0.6), (0.05, 0.03, 0.5), smooth=False),
          P("cube", "beam", (0.77, Y0 - D - 0.03, 0.6), (0.05, 0.03, 0.5), smooth=False)]
window(smithy, 1.35, Y0 - D, 0.95, w=0.18, h=0.16, frame="stone_dark")
smithy += [P("cube", "metal_light", (0.35, Y0 - D - 0.05, 1.42), (0.24, 0.02, 0.1)),    # Schild: Hammer
           P("cube", "wood_dark", (0.35, Y0 - D - 0.07, 1.42), (0.12, 0.01, 0.02), smooth=False),
           P("cube", "metal", (0.47, Y0 - D - 0.07, 1.42), (0.04, 0.01, 0.06), smooth=False)]
for s in (-1, 1):
    smithy.append(P("cube", "roof_dark" if s < 0 else "roof", (0.35, Y0 + s * (D / 2 + 0.05), H + 0.1 + 0.33),
                    (W + 0.18, D / 2 + 0.25, 0.05), rot=(-s * 34, 0, 0), smooth=False))
for x in (0.35 - W + 0.02, 0.35 + W - 0.02):
    for z, h in ((H + 0.2, 0.48), (H + 0.42, 0.3), (H + 0.6, 0.14)):
        smithy.append(P("cube", "stone", (x, Y0, z), (0.02, h, 0.1), smooth=False))
EX, EY = -1.45, -0.35                                       # Esse vorne links
smithy += [P("cube", "stone_dark", (EX, EY, 0.35), (0.42, 0.36, 0.35), smooth=False),
           P("cube", "stone", (EX, EY, 0.72), (0.44, 0.38, 0.03), smooth=False),
           P("sphere", "fire", (EX, EY - 0.03, 0.77), (0.3, 0.24, 0.08), segments=12),
           P("sphere", "fire_core", (EX, EY - 0.05, 0.82), (0.16, 0.12, 0.06), segments=10),
           P("cube", "stone", (EX, EY + 0.3, 1.45), (0.2, 0.16, 0.7), smooth=False),       # Kamin der Esse
           P("cube", "stone_dark", (EX, EY + 0.3, 2.17), (0.23, 0.19, 0.04), smooth=False),
           P("cube", "metal", (EX + 0.45, EY - 0.1, 0.45), (0.04, 0.12, 0.08), smooth=False),  # Blasebalg
           P("cube", "wood", (EX + 0.5, EY - 0.1, 0.35), (0.06, 0.16, 0.04), rot=(0, 20, 0))]
smithy += [P("cyl", "wood_dark", (1.25, -0.55, 0.3), (0.26, 0.26, 0.05), rot=(90, 0, 0), segments=16),  # Schleifstein
           P("cyl", "stone_light", (1.25, -0.6, 0.38), (0.22, 0.22, 0.05), rot=(90, 0, 0), segments=16),
           P("cyl", "wood", (1.75, -0.2, 0.26), (0.18, 0.18, 0.26), segments=12),                    # Wasserfass
           P("cyl", "water", (1.75, -0.2, 0.525), (0.16, 0.16, 0.01), segments=12),
           P("cube", "wood", (-0.55, -0.45, 0.45), (0.3, 0.05, 0.03), smooth=False)]                 # Waffenstaender
for k in range(3):
    x = -0.75 + k * 0.2
    smithy += [P("cube", "metal_light", (x, -0.5, 0.55), (0.025, 0.02, 0.3), smooth=False),
               P("cube", "gold" if k == 1 else "metal", (x, -0.52, 0.32), (0.07, 0.02, 0.02), smooth=False)]
render([make("Smithy", smithy, 0.03, smooth=False)], "smithy.png", 384)

# ---------------- Rauch aus dem Kamin (3 Bilder, 128 px): steigt und waechst
for f in range(3):
    random.seed(40 + f)
    puffs = []
    for k in range(4):
        z = 0.3 + 0.35 * k + 0.12 * f
        r = 0.12 + 0.05 * k + 0.02 * f
        puffs.append(P("sphere", "smoke" if (k + f) % 2 else "smoke_light", (0.08 * math.sin(k + f), 0, z), (r, r, r * 0.85), segments=10))
    render([make(f"Smoke{f}", puffs, 0.012)], f"smoke_{f}.png")

# ================================================================ Stadttor (384 px) und Mauer
tower_r, tower_h, half = 0.55, 2.6, 1.75
gate = []
for x in (-half, half):
    gate += [P("cyl", "stone", (x, 0, tower_h / 2), (tower_r, tower_r, tower_h / 2), segments=16),
             P("cyl", "stone_dark", (x, 0, 0.1), (tower_r + 0.05, tower_r + 0.05, 0.1), segments=16),
             P("cyl", "stone_light", (x, 0, tower_h + 0.06), (tower_r + 0.08, tower_r + 0.08, 0.06), segments=16),
             P("cone", "roof_blue", (x, 0, tower_h + 0.55), (tower_r + 0.12, tower_r + 0.12, 0.5), segments=16),
             P("cube", "glow", (x, -tower_r + 0.02, 1.7), (0.06, 0.02, 0.14)),
             P("cube", "cloth_b", (x, -tower_r - 0.02, 1.05), (0.13, 0.02, 0.32), smooth=False),
             P("cube", "gold", (x, -tower_r - 0.04, 1.1), (0.05, 0.01, 0.05), rot=(0, 45, 0), smooth=False)]
gate += [P("cube", "stone", (0, 0, 2.05), (half - 0.3, 0.36, 0.4), smooth=False),   # Mauer ueber dem Durchgang
         P("cube", "stone_light", (0, 0, 2.47), (half - 0.25, 0.4, 0.04), smooth=False)]
for k in range(9):                                         # Torbogen aus Keilsteinen (obere Halbkreis-Haelfte)
    a = math.pi * k / 8
    gate.append(P("cube", "stone_light" if k % 2 else "stone_dark", (1.0 * math.cos(a), -0.38, 0.75 + 0.95 * math.sin(a)),
                  (0.13, 0.05, 0.1), rot=(0, -math.degrees(a) + 90, 0), smooth=False))
gate.append(P("cube", "gold", (0, -0.44, 1.72), (0.07, 0.02, 0.07), rot=(0, 45, 0), smooth=False))
for k in range(5):                                         # Zinnen
    gate.append(P("cube", "stone", (-1.0 + k * 0.5, -0.25, 2.62), (0.14, 0.12, 0.12), smooth=False))
render([make("Gate", gate, 0.03, smooth=False)], "gate.png", 384)

# ---------------- Mauerstueck (128 px = genau 1 Einheit breit, 2,1 m): nahtlos nebeneinander
HW = 1.05
wall = [P("cube", "stone", (0, 0, 0.75), (HW, 0.28, 0.75), smooth=False),
        P("cube", "stone_dark", (0, 0, 0.06), (HW, 0.3, 0.06), smooth=False),
        P("cube", "stone_light", (0, 0, 1.52), (HW, 0.3, 0.04), smooth=False)]
for row in range(4):                                       # Steinfugen vorne
    z = 0.3 + row * 0.33
    wall.append(P("cube", "stone_dark", (0, -0.285, z), (HW, 0.006, 0.012), smooth=False))
    off = 0.26 if row % 2 else 0.0
    for k in range(5):
        x = -HW + off + k * 0.52
        if -HW < x < HW:
            wall.append(P("cube", "stone_dark", (x, -0.285, z + 0.165), (0.012, 0.006, 0.16), smooth=False))
for x in (-0.79, -0.26, 0.26, 0.79):                       # Zinnen (Abstand passt beim Aneinanderlegen)
    wall.append(P("cube", "stone", (x, 0, 1.68), (0.14, 0.26, 0.12), smooth=False))
wall.append(P("sphere", "leaf", (0.5, -0.29, 0.2), (0.18, 0.05, 0.12), segments=8))
render([make("WallSegment", wall, 0.02, smooth=False)], "wall_segment.png")

tower = [P("cyl", "stone", (0, 0, 1.15), (0.6, 0.6, 1.15), segments=16),
         P("cyl", "stone_dark", (0, 0, 0.1), (0.65, 0.65, 0.1), segments=16),
         P("cyl", "stone_light", (0, 0, 2.34), (0.68, 0.68, 0.06), segments=16)]
for k in range(8):
    a = 2 * math.pi * k / 8
    tower.append(P("cube", "stone", (0.6 * math.cos(a), 0.6 * math.sin(a), 2.5), (0.1, 0.1, 0.12), rot=(0, 0, math.degrees(a)), smooth=False))
tower.append(P("cube", "glow", (0, -0.59, 1.5), (0.05, 0.02, 0.13)))
render([make("WallTower", tower, 0.025, smooth=False)], "wall_tower.png", 256)

# ================================================================ Brunnen (256 px, 2 Bilder)
basin = [P("cyl", "stone", (0, 0, 0.18), (0.95, 0.75, 0.18), segments=8, rot=(0, 0, 22.5)),
         P("cyl", "stone_light", (0, 0, 0.37), (0.98, 0.78, 0.03), segments=8, rot=(0, 0, 22.5)),
         P("cyl", "stone_dark", (0, 0, 0.02), (1.0, 0.8, 0.02), segments=8, rot=(0, 0, 22.5)),
         P("cyl", "stone", (0, 0, 0.75), (0.12, 0.12, 0.45), segments=12),
         P("cyl", "stone_light", (0, 0, 1.18), (0.4, 0.4, 0.05), segments=16, taper=1.3),
         P("sphere", "gold", (0, 0, 1.55), (0.08, 0.08, 0.08))]
basin_obj = make("FountainBasin", basin, 0.024, smooth=False)
for f in range(2):
    water = [P("cyl", "water", (0, 0, 0.33), (0.86, 0.66, 0.02), segments=16),
             P("cyl", "water", (0, 0, 1.25), (0.36, 0.36, 0.02), segments=16),
             P("cone", "water_light", (0, 0, 1.4), (0.07, 0.07, 0.16 + 0.04 * f))]
    for k in range(6):                                     # Wasserstrahlen vom oberen Becken
        a = 2 * math.pi * (k + 0.5 * f) / 6
        water.append(P("cyl", "water_light", (0.38 * math.cos(a), 0.38 * math.sin(a), 0.8), (0.025, 0.025, 0.45)))
    for k in range(3):                                     # Wellenringe
        r = 0.25 + 0.2 * k + 0.1 * f
        water.append(P("torus", "water_light", (0, 0, 0.35), (r * 1.25, r, 0.06), segments=16))
    render([basin_obj, make(f"FountainWater{f}", water, 0.008)], f"fountain_{f}.png", 256)

# ================================================================ Kleinzeug (128 px)
lantern = [P("cyl", "metal", (0, 0, 0.75), (0.035, 0.035, 0.75)),
           P("cyl", "stone_dark", (0, 0, 0.06), (0.12, 0.12, 0.06), segments=10),
           P("cube", "metal", (0, 0, 1.52), (0.11, 0.11, 0.02), smooth=False),
           P("cube", "glow", (0, 0, 1.65), (0.08, 0.08, 0.11), smooth=False),
           P("cone", "metal", (0, 0, 1.84), (0.13, 0.13, 0.09), segments=4, rot=(0, 0, 45))]
render([make("Lantern", lantern, 0.012)], "lantern.png")

bench = [P("cube", "wood", (0, 0, 0.3), (0.55, 0.16, 0.025)),
         P("cube", "wood", (0, 0.15, 0.55), (0.55, 0.02, 0.1), rot=(-10, 0, 0))]
for x in (-0.45, 0.45):
    bench += [P("cube", "metal", (x, 0, 0.15), (0.03, 0.14, 0.15), smooth=False),
              P("cube", "metal", (x, 0.15, 0.42), (0.03, 0.02, 0.12), smooth=False)]
render([make("Bench", bench)], "bench.png")

random.seed(5)
planter = [P("cube", "wood", (0, 0, 0.15), (0.45, 0.2, 0.15)),
           P("cube", "wood_dark", (0, 0, 0.3), (0.47, 0.22, 0.02), smooth=False)]
for k in range(7):
    x = -0.38 + k * 0.127
    planter += [P("sphere", "leaf_dark", (x, 0, 0.36), (0.08, 0.08, 0.07), segments=8),
                P("sphere", random.choice(["flower_r", "flower_y", "flower_p"]), (x + 0.02, -0.05, 0.44), (0.05, 0.05, 0.04), segments=8)]
render([make("Planter", planter)], "planter.png")

fence = []                                                 # 2,1 m breit, nahtlos
for x in (-0.79, -0.26, 0.26, 0.79):
    fence += [P("cube", "wood", (x, 0, 0.32), (0.04, 0.04, 0.32), smooth=False),
              P("cone", "wood", (x, 0, 0.68), (0.045, 0.045, 0.05), segments=4, rot=(0, 0, 45))]
for z in (0.22, 0.48):
    fence.append(P("cube", "wood_dark", (0, -0.045, z), (HW, 0.015, 0.035), smooth=False))
render([make("Fence", fence, 0.012, smooth=False)], "fence.png")

random.seed(9)
hedge = [P("cube", "leaf_dark", (0, 0, 0.3), (HW, 0.3, 0.3))]                              # nahtlos
for k in range(5):
    x = -HW + 0.21 + k * 0.42
    hedge.append(P("sphere", random.choice(["leaf", "leaf_dark"]), (x, -0.05, 0.55), (0.26, 0.28, 0.22), segments=10))
hedge += [P("sphere", "flower_y", (random.uniform(-0.9, 0.9), -0.31, random.uniform(0.2, 0.5)), (0.03, 0.02, 0.03), segments=6) for _ in range(5)]
render([make("Hedge", hedge)], "hedge.png")

hay = [P("cyl", "hay", (0, 0, 0.3), (0.3, 0.3, 0.42), rot=(0, 90, 0), segments=14),
       P("cyl", "hay_dark", (-0.43, 0, 0.3), (0.28, 0.28, 0.01), rot=(0, 90, 0), segments=14),
       P("torus", "hay_dark", (0, 0, 0.3), (0.31, 0.31, 0.31), rot=(0, 90, 0), segments=10)]
render([make("Hay", hay)], "hay.png")

random.seed(13)
mcrate = [P("cube", "wood", (0, 0, 0.2), (0.3, 0.22, 0.2)),
          P("cube", "wood_dark", (0, -0.225, 0.2), (0.3, 0.01, 0.025), smooth=False)]
for k in range(6):
    kind = random.choice(["cabbage", "apple", "carrot"])
    x, y = -0.2 + (k % 3) * 0.2, -0.08 + (k // 3) * 0.14
    if kind == "carrot":
        mcrate.append(P("cone", kind, (x, y, 0.45), (0.04, 0.04, 0.09), rot=(70, 0, random.uniform(0, 90))))
    else:
        r = 0.09 if kind == "cabbage" else 0.06
        mcrate.append(P("sphere", kind, (x, y, 0.42), (r, r, r * 0.9), segments=10))
render([make("MarketCrate", mcrate)], "market_crate.png")

well = [P("cyl", "stone", (0, 0, 0.25), (0.42, 0.42, 0.25), segments=14),
        P("cyl", "stone_light", (0, 0, 0.51), (0.45, 0.45, 0.02), segments=14),
        P("cyl", "water", (0, 0, 0.46), (0.36, 0.36, 0.01), segments=14),
        P("cube", "wood", (0, 0, 1.12), (0.42, 0.03, 0.03), smooth=False),
        P("cyl", "wood_dark", (0, 0, 1.12), (0.05, 0.05, 0.3), rot=(0, 90, 0), segments=8),
        P("cube", "wood_dark", (0, 0, 0.85), (0.005, 0.005, 0.27), smooth=False),
        P("cyl", "wood", (0, 0, 0.6), (0.07, 0.07, 0.07), segments=8)]
for x in (-0.4, 0.4):
    well.append(P("cube", "wood", (x, 0, 0.85), (0.035, 0.035, 0.45), smooth=False))
for s in (-1, 1):
    well.append(P("cube", "roof" if s < 0 else "roof_dark", (0, s * 0.18, 1.42), (0.55, 0.26, 0.03), rot=(-s * 35, 0, 0), smooth=False))
render([make("Well", well, 0.016, smooth=False)], "well.png")

sign = [P("cyl", "wood_dark", (0, 0, 0.6), (0.04, 0.04, 0.6), segments=8),
        P("cube", "wood", (0.22, -0.02, 1.05), (0.26, 0.02, 0.08), rot=(0, 0, 8)),
        P("cone", "wood", (0.5, -0.02, 1.05), (0.08, 0.08, 0.04), rot=(0, 90, 8), segments=3),
        P("cube", "wood", (-0.2, -0.02, 0.82), (0.24, 0.02, 0.08), rot=(0, 0, -6)),
        P("cone", "wood", (-0.46, -0.02, 0.82), (0.08, 0.08, 0.04), rot=(0, -90, -6), segments=3),
        P("cube", "cloth_w", (0.22, -0.045, 1.05), (0.16, 0.005, 0.015), smooth=False),
        P("cube", "cloth_w", (-0.2, -0.045, 0.82), (0.14, 0.005, 0.015), smooth=False)]
render([make("Signpost", sign, 0.012)], "signpost.png")

cart = [P("cube", "wood", (0, 0, 0.42), (0.55, 0.3, 0.05)),
        P("cube", "wood_dark", (0, -0.3, 0.55), (0.55, 0.02, 0.12), smooth=False),
        P("cube", "wood_dark", (0, 0.3, 0.55), (0.55, 0.02, 0.12), smooth=False),
        P("cube", "wood_dark", (0.75, -0.12, 0.38), (0.25, 0.025, 0.025), smooth=False),
        P("cube", "wood_dark", (0.75, 0.12, 0.38), (0.25, 0.025, 0.025), smooth=False)]
for y in (-0.33, 0.33):
    cart += [P("torus", "wood_dark", (-0.15, y, 0.28), (0.26, 0.26, 0.26), rot=(90, 0, 0), segments=14),
             P("cyl", "metal", (-0.15, y, 0.28), (0.05, 0.05, 0.03), rot=(90, 0, 0), segments=8)]
cart += [P("cyl", "hay", (0.0, 0.0, 0.62), (0.18, 0.18, 0.3), rot=(0, 90, 0), segments=10),
         P("sphere", "cabbage", (-0.35, -0.1, 0.55), (0.1, 0.1, 0.09), segments=10),
         P("sphere", "apple", (0.35, -0.12, 0.52), (0.06, 0.06, 0.06), segments=8)]
render([make("Cart", cart)], "cart.png")

scene.render.resolution_x = scene.render.resolution_y = 128
pivot = L.foot_pivot(scene)

# ================================================================ Bodenkacheln (64 px, senkrecht von oben)
tiles = bpy.data.scenes.new("TownTiles")
L.setup_scene(tiles, 64)
tiles.render.film_transparent = False
tcd = bpy.data.cameras.new("TownTileCamera")
tcd.type = "ORTHO"
tcd.ortho_scale = 1.0
tcam = bpy.data.objects.new("TownTileCamera", tcd)
tiles.collection.objects.link(tcam)
tcam.location = (0.5, 0.5, 10)
tiles.camera = tcam
tsun = bpy.data.lights.new("TownTileSun", "SUN")
tsun.energy = 3.0
tso = bpy.data.objects.new("TownTileSun", tsun)
tiles.collection.objects.link(tso)
tso.rotation_euler = (math.radians(35), math.radians(-20), math.radians(-30))
T = {"mortar": L.cel("PflasterFuge", (0.38, 0.35, 0.33), shadow=0.9, mid=0.95),
     "cob": L.cel("Pflaster", (0.68, 0.64, 0.6)),
     "cob2": L.cel("Pflaster2", (0.6, 0.57, 0.55)),
     "cob3": L.cel("Pflaster3", (0.74, 0.68, 0.58)),
     "ground": L.cel("BodenStadt", (0.26, 0.49, 0.19), shadow=0.85, mid=0.95),
     "blade": L.cel("HalmStadt", (0.42, 0.72, 0.28)),
     "blade_dark": L.cel("HalmStadtDunkel", (0.17, 0.36, 0.13)),
     "water": L.cel("WasserKachel", (0.22, 0.5, 0.82), shadow=0.9, mid=0.96, emission=0.15),
     "water_light": L.cel("WasserKachelHell", (0.55, 0.82, 1.0), shadow=0.95, emission=0.4),
     "plank": L.cel("Planke", (0.62, 0.42, 0.24)),
     "plank2": L.cel("Planke2", (0.54, 0.35, 0.19)),
     "nail": L.cel("Nagel", (0.3, 0.3, 0.34))}
tobjs = []


def TP(kind, mat, loc, sc, rot=(0, 0, 0), **kw):
    o = L.shape(kind, loc, sc, rot, T[mat], None, collection=tiles.collection, **kw)
    tobjs.append(o)
    return o


def wrapped(kind, mat, x, y, z, sc, rot=(0, 0, 0), margin=0.15, **kw):
    """Teil an (x, y) plus Kopien jenseits der Kanten, damit die Kachel nahtlos ist"""
    for dx in (-1, 0, 1):
        for dy in (-1, 0, 1):
            px, py = x + dx, y + dy
            if -margin <= px <= 1 + margin and -margin <= py <= 1 + margin:
                TP(kind, mat, (px, py, z), sc, rot, **kw)


def tshot(name):
    global count
    L.render_still(tiles, os.path.join(OUT, name))
    for o in tobjs:
        bpy.data.objects.remove(o, do_unlink=True)
    tobjs.clear()
    count += 1


def cobbles(seed, rows=(0, 4)):
    """Pflastersteine in versetzten Reihen (4 Reihen x 4 Steine), Reihen rows[0]..rows[1]-1"""
    random.seed(seed)
    for row in range(*rows):
        off = 0.125 if row % 2 else 0.0
        for col in range(4):
            x = (col * 0.25 + off + 0.125) % 1.0
            y = row * 0.25 + 0.125
            mat = random.choice(["cob", "cob", "cob2", "cob3"])
            wrapped("cube", mat, x + random.uniform(-0.01, 0.01), y + random.uniform(-0.01, 0.01), 0.02,
                    (0.105, 0.1, 0.03), rot=(0, 0, random.uniform(-6, 6)), margin=0.2)


for v in range(3):
    TP("cube", "mortar", (0.5, 0.5, -0.05), (1.5, 1.5, 0.05), smooth=False)
    cobbles(500 + v)
    tshot(f"Ground_Cobble_{v}.png")

# Pflaster mit Grasrand oben (in Unity gedreht fuer die anderen Seiten)
TP("cube", "mortar", (0.5, 0.5, -0.05), (1.5, 1.5, 0.05), smooth=False)
cobbles(510, rows=(0, 3))
TP("cube", "ground", (0.5, 0.95, 0.0), (1.5, 0.2, 0.04), smooth=False)
random.seed(511)
for _ in range(16):
    x = random.random()
    wrapped("cone", random.choice(["blade", "blade", "blade_dark"]), x, random.uniform(0.74, 0.98), 0.05,
            (0.028, 0.028, 0.08), rot=(random.uniform(-25, 25), random.uniform(-25, 25), 0))
tshot("Ground_CobbleEdge.png")

for f in range(2):
    random.seed(520)
    TP("cube", "water", (0.5, 0.5, -0.05), (1.5, 1.5, 0.05), smooth=False)
    for k in range(7):
        x, y = random.random(), random.random()
        wrapped("cube", "water_light", (x + 0.12 * f) % 1.0, y, 0.01, (random.uniform(0.06, 0.12), 0.012, 0.005), smooth=False)
    tshot(f"Ground_Water_{f}.png")

random.seed(530)
TP("cube", "water", (0.5, 0.5, -0.1), (1.5, 1.5, 0.05), smooth=False)
for k in range(5):                                         # Planken quer, nahtlos nach links/rechts
    y = 0.1 + k * 0.2
    TP("cube", "plank" if k % 2 == 0 else "plank2", (0.5, y, 0.02), (0.75, 0.09, 0.03), smooth=False)
    for x in (0.08, 0.92):
        TP("sphere", "nail", (x, y, 0.06), (0.015, 0.015, 0.01), segments=6)
tshot("Ground_Bridge.png")

for attempt in range(5):
    try:
        bpy.ops.wm.save_as_mainfile(filepath=BLEND_PATH)
        break
    except RuntimeError:
        import time
        time.sleep(1.0)
print(f"RESULT rendered={count} randwarnungen={warnings} pivot={pivot}")
