"""RE:BORN - Gildeneinrichtung (Meilenstein 8, Comic-Look)

Moebel fuer die Gildenhallen, Kamera wie build_interiors.py (shot_prop): 128 px = ortho 2,1, 256 px = ortho 4,2 (grosse Teile),
Fusspunkt (0|0|0) bei 0,2138 von unten.
Krieger: carpet_red, fireplace, armor_stand, weapon_wall, feast_table
Magier: carpet_blue, cauldron, globe, telescope, bookwall, star_map
Assassinen: carpet_dark, map_table, dagger_wall, cabinet, lantern_red
Eigene Datei: Art/Blender/Reborn_Interiors2.blend. Ausgabe: Assets/_Project/Sprites/Interiors (nur diese neuen Namen).

Aufruf:  blender.exe -b --factory-startup --python build_interiors2.py -- <projektordner>
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
BLEND_PATH = os.path.join(PROJECT, "Art", "Blender", "Reborn_Interiors2.blend")
os.makedirs(OUT, exist_ok=True)

bpy.ops.wm.read_factory_settings(use_empty=True)
scene = bpy.context.scene
scene.name = "Interiors2"
L.setup_scene(scene)
cam = L.camera(scene, target=(0, 0, 0.85), ortho=2.1)
L.sun(scene)

C = L.cel
M = {
    "wood": C("Holz", (0.58, 0.37, 0.2)), "wood_l": C("HolzHell", (0.72, 0.5, 0.28)), "wood_d": C("HolzDunkel", (0.34, 0.2, 0.1)),
    "wood_dd": C("HolzSehrDunkel", (0.2, 0.13, 0.12)), "dark": C("Dunkel", (0.22, 0.17, 0.2)), "dark_l": C("DunkelHell", (0.34, 0.27, 0.31)),
    "black": C("Schwarz", (0.07, 0.06, 0.08), shadow=1.0, mid=1.0),
    "steel": C("Stahl", (0.66, 0.68, 0.74), light=1.05), "steel_d": C("StahlDunkel", (0.42, 0.44, 0.52)),
    "steel_l": C("StahlHell", (0.85, 0.88, 0.94), shadow=0.7), "iron": C("Eisen", (0.3, 0.31, 0.36)),
    "gold": C("Gold", (1.0, 0.8, 0.28), light=1.1), "gold_d": C("GoldDunkel", (0.8, 0.58, 0.18)),
    "brass": C("Messing", (0.85, 0.62, 0.22), light=1.1),
    "red": C("Rot", (0.75, 0.15, 0.15)), "red_d": C("RotDunkel", (0.5, 0.08, 0.1)), "red_dd": C("RotSehrDunkel", (0.3, 0.05, 0.08)),
    "blue": C("Blau", (0.18, 0.28, 0.65)), "blue_d": C("BlauDunkel", (0.1, 0.14, 0.4)), "navy": C("Nachtblau", (0.06, 0.09, 0.24)),
    "stone": C("Stein", (0.55, 0.55, 0.62)), "stone2": C("Stein2", (0.46, 0.46, 0.54)), "stone_d": C("SteinDunkel", (0.3, 0.3, 0.37)),
    "cream": C("Creme", (0.95, 0.92, 0.8)), "parch": C("Pergament", (0.9, 0.82, 0.6)), "parch_d": C("PergamentDunkel", (0.7, 0.58, 0.36)),
    "flame": C("Flamme", (1.0, 0.55, 0.15), shadow=1.0, mid=1.0, emission=1.0), "flame_y": C("FlammeGelb", (1.0, 0.9, 0.35), shadow=1.0, mid=1.0, emission=1.2),
    "ember": C("Glut", (1.0, 0.3, 0.1), shadow=1.0, mid=1.0, emission=0.8),
    "potion": C("TrankGruen", (0.3, 1.0, 0.35), shadow=1.0, mid=1.0, emission=1.2), "bubble": C("Blase", (0.7, 1.0, 0.7), shadow=1.0, mid=1.0, emission=1.0),
    "star": C("Stern", (1.0, 0.92, 0.5), shadow=1.0, mid=1.0, emission=0.8), "moon": C("Mond", (0.95, 0.95, 0.85), shadow=1.0, mid=1.0, emission=0.4),
    "ocean": C("Meer", (0.2, 0.45, 0.85)), "land": C("Land", (0.35, 0.65, 0.3)), "sand": C("Sand", (0.85, 0.75, 0.45)),
    "lamp_r": C("LampeRot", (1.0, 0.1, 0.08), shadow=1.0, mid=1.0, emission=1.8), "lamp_c": C("LampeKern", (1.0, 0.55, 0.45), shadow=1.0, mid=1.0, emission=1.5),
    "glass": C("Glas", (0.7, 0.9, 0.95), emission=0.2), "plume": C("Helmbusch", (0.8, 0.12, 0.12)),
    "bread": C("Brot", (0.85, 0.58, 0.28)), "roast": C("Braten", (0.62, 0.32, 0.14)), "roast_l": C("BratenHell", (0.8, 0.48, 0.2)),
    "apple": C("Apfel", (0.8, 0.15, 0.12)), "bone": C("Knochen", (0.95, 0.92, 0.85)), "cheese": C("Kaese", (0.95, 0.8, 0.25)),
    "mug": C("Krug", (0.62, 0.45, 0.25)), "foam": C("Schaum", (0.98, 0.97, 0.9)),
    "river": C("Fluss", (0.3, 0.55, 0.85)), "pin": C("Nadel", (0.9, 0.2, 0.2)),
    "leather": C("Leder", (0.45, 0.28, 0.14)), "fur": C("Fell", (0.45, 0.45, 0.5)),
    "bk1": C("BuchRot", (0.7, 0.2, 0.2)), "bk2": C("BuchBlau", (0.2, 0.35, 0.7)), "bk3": C("BuchGruen", (0.2, 0.55, 0.3)),
    "bk4": C("BuchLila", (0.5, 0.25, 0.65)), "bk5": C("BuchOrange", (0.85, 0.5, 0.18)), "bk6": C("BuchTuerkis", (0.18, 0.6, 0.62)),
    "bk7": C("BuchBraun", (0.45, 0.28, 0.16)), "bk8": C("BuchGelb", (0.85, 0.75, 0.25)),
}
BOOKS = ["bk1", "bk2", "bk3", "bk4", "bk5", "bk6", "bk7", "bk8"]
objs = []
count = 0
warnings = 0


def P(kind, mat, loc, sc, rot=(0, 0, 0), **kw):
    o = L.shape(kind, loc, sc, rot, M[mat], None, collection=scene.collection, **kw)
    objs.append(o)
    return o


def blk(mat, x, y, z, hx, hy, hz, rot=(0, 0, 0), smooth=False):
    return P("cube", mat, (x, y, z), (hx, hy, hz), rot, smooth=smooth)


def slab(mat, x, y, hx, hy, top, rotz=0):
    """flache Schicht vom Boden bis `top` (fuer Teppiche; hoehere Schichten verdecken tiefere)"""
    return P("cube", mat, (x, y, top / 2), (hx, hy, top / 2), (0, 0, rotz), smooth=False)


def rod(mat, a, b, rx, ry=None, kind="cyl", **kw):
    """Stab von a nach b. cyl/cube/cone (Spitze bei b); rx = Breite (lokal X), ry = Dicke (lokal Y)."""
    a, b = Vector(a), Vector(b)
    v = b - a
    e = v.to_track_quat("Z", "Y").to_euler()
    ry = rx if ry is None else ry
    return P(kind, mat, (a + b) / 2, (rx, ry, v.length / 2), tuple(math.degrees(t) for t in e), **kw)


def patch(mat, center, radius, dirv, sx, sy, th=0.02):
    """flacher Fleck auf einer Kugel (Richtung dirv)"""
    d = Vector(dirv).normalized()
    e = d.to_track_quat("Z", "Y").to_euler()
    return P("sphere", mat, Vector(center) + d * radius, (sx, sy, th), tuple(math.degrees(t) for t in e), segments=10)


def shot(name, size=128, outline=0.016, subdiv=True, k=1.0):
    """Teile verbinden, rendern (Kamera wie shot_prop in build_interiors.py), danach loeschen"""
    global count, warnings
    o = L.join(list(objs), name)
    o.scale = (k, k, k)
    if subdiv:
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
    e = L.edge_pixels(path)
    if e:
        warnings += 1
        print("WARNUNG Randpixel:", name, e)


def sword(hilt, tip, blade="steel_l", guard="gold", grip="leather", w=0.05, glen=0.15, y_th=0.014):
    a, b = Vector(hilt), Vector(tip)
    d = (b - a).normalized()
    perp = Vector((-d.z, 0, d.x))
    g = a + d * glen
    rod(grip, a, g, 0.026)
    P("sphere", guard, a - d * 0.02, (0.04, 0.04, 0.04), segments=8)
    rod(guard, g - perp * 0.15, g + perp * 0.15, 0.022, 0.022)
    rod(blade, g, b - d * 0.1, w, y_th, kind="cube", smooth=False)
    rod(blade, b - d * 0.1, b, w, y_th, kind="cone", segments=4)
    rod("steel_d", g + d * 0.02 + Vector((0, -0.01, 0)), b - d * 0.15 + Vector((0, -0.01, 0)), 0.01, 0.006, kind="cube", smooth=False)


def dagger(cx, cz, ang, y=-0.1, length=0.5):
    d = Vector((math.sin(math.radians(ang)), 0, math.cos(math.radians(ang))))
    a = Vector((cx, y, cz)) - d * length / 2
    sword(a, a + d * length, blade="steel_l", guard="red_d", grip="black", w=0.032, glen=0.13, y_th=0.012)


def star4(cx, cz, r, y=-0.1, rot=0.0, mat="steel_l"):
    """Wurfstern: 4 Spitzen"""
    for k in range(4):
        t = math.radians(rot + 90 * k)
        rod(mat, (cx, y, cz), (cx + r * math.sin(t), y, cz + r * math.cos(t)), r * 0.32, 0.012, kind="cone", segments=4)
    P("sphere", "black", (cx, y - 0.012, cz), (r * 0.22, 0.02, r * 0.22), segments=8)


def carpet_frame(base, border, hx, hy, inner, top=0.015):
    slab(border, 0, 0, hx, hy, top)
    slab(base, 0, 0, hx - 0.12, hy - 0.12, top + 0.006)
    slab(border, 0, 0, hx - 0.2, hy - 0.2, top + 0.012)
    slab(inner, 0, 0, hx - 0.25, hy - 0.25, top + 0.018)
    for sgn in (-1, 1):   # Fransen
        for k in range(-7, 8):
            P("cube", "cream", (sgn * (hx + 0.04), k * hy / 7.6, 0.006), (0.06, 0.018, 0.006), smooth=False)


# ===================================================== KRIEGER
# Teppich rot (3 x 2 m)
carpet_frame("red", "gold", 1.5, 1.0, "red_d")
slab("gold", 0, 0, 0.5, 0.5, 0.05, 45)
slab("red", 0, 0, 0.42, 0.42, 0.058, 45)
slab("gold", 0, 0, 0.26, 0.26, 0.066, 45)
slab("red_d", 0, 0, 0.2, 0.2, 0.074, 45)
for sx in (-1, 1):
    for k in range(3):
        slab("gold", sx * (0.8 + 0.16 * k), 0, 0.07, 0.07, 0.05, 45)
        slab("gold", sx * (0.8 + 0.16 * k), 0, 0.07, 0.07, 0.05, 45)
for sx in (-1, 1):
    for sy in (-1, 1):
        slab("gold", sx * 1.18, sy * 0.66, 0.09, 0.09, 0.05, 45)
shot("carpet_red", 256, 0.006, subdiv=False)

# Kamin (2 m breit)
blk("stone2", 0, -0.12, 0.05, 1.1, 0.55, 0.05)
blk("stone_d", 0, 0.3, 0.7, 0.6, 0.06, 0.65)                         # Rueckwand (russig)
blk("black", 0, 0.24, 0.55, 0.55, 0.02, 0.5)
for sx in (-1, 1):
    blk("stone_d", sx * 0.58, 0.1, 0.7, 0.04, 0.22, 0.65)           # Innenwand
    for i in range(6):                                              # Pfeiler aus Bloecken
        blk("stone" if (i + (sx > 0)) % 2 else "stone2", sx * 0.82 + 0.015 * (i % 2), 0.0, 0.2 + i * 0.26, 0.22, 0.42, 0.125)
    blk("stone2", sx * 0.82, -0.02, 0.1, 0.27, 0.46, 0.06)          # Sockel
blk("stone2", 0, 0.0, 1.62, 1.12, 0.47, 0.12)                       # Sturz
for i in range(7):
    blk("stone" if i % 2 else "stone2", -0.9 + i * 0.3, -0.45, 1.62, 0.145, 0.03, 0.1)
blk("stone", 0, -0.03, 1.82, 1.2, 0.5, 0.06)                        # Sims
blk("stone2", 0, 0.1, 2.12, 0.78, 0.3, 0.26)                        # Rauchfang
blk("stone", 0, 0.14, 2.52, 0.5, 0.22, 0.17)
blk("stone2", 0, 0.18, 2.88, 0.3, 0.18, 0.2)
blk("stone", 0, 0.18, 3.1, 0.38, 0.24, 0.05)
for k in range(-3, 4):
    P("cube", "stone_d", (k * 0.22, -0.2, 2.12 + 0.12 * (k % 2)), (0.09, 0.02, 0.1), smooth=False)
P("sphere", "red", (0, -0.52, 1.64), (0.07, 0.03, 0.07), segments=8)  # Wappen
P("sphere", "gold", (0, -0.5, 1.64), (0.1, 0.03, 0.1), segments=8)
for sx in (-1, 1):                                                  # Kerzen auf dem Sims
    P("cyl", "gold", (sx * 0.8, -0.08, 1.94), (0.06, 0.06, 0.06), segments=10)
    P("cyl", "cream", (sx * 0.8, -0.08, 2.08), (0.035, 0.035, 0.1), segments=10)
    P("cone", "flame_y", (sx * 0.8, -0.08, 2.22), (0.03, 0.03, 0.06), segments=8)
rod("wood_d", (-0.42, -0.05, 0.14), (0.42, 0.12, 0.2), 0.075)       # Holzscheite
rod("wood", (-0.34, 0.2, 0.13), (0.36, -0.12, 0.2), 0.07)
rod("wood_d", (-0.3, -0.2, 0.1), (0.3, -0.2, 0.12), 0.06)
P("sphere", "ember", (0, 0.05, 0.12), (0.4, 0.22, 0.05), segments=10)
for sx in (-1, 1):                                                  # Gitter
    rod("iron", (sx * 0.5, -0.4, 0.09), (sx * 0.5, -0.4, 0.45), 0.02)
rod("iron", (-0.5, -0.4, 0.4), (0.5, -0.4, 0.4), 0.02)
P("cone", "flame", (0, 0.05, 0.5), (0.24, 0.17, 0.4), segments=8)
P("cone", "flame", (-0.22, -0.02, 0.4), (0.13, 0.1, 0.27), (0, -12, 0), segments=8)
P("cone", "flame", (0.24, 0.0, 0.38), (0.12, 0.1, 0.25), (0, 12, 0), segments=8)
P("cone", "flame_y", (0, 0.0, 0.42), (0.14, 0.1, 0.28), segments=8)
P("cone", "flame_y", (0.02, -0.06, 0.32), (0.07, 0.06, 0.14), segments=8)
shot("fireplace", 256, 0.012)

# Ruestungsstaender
blk("wood_d", 0, 0, 0.04, 0.38, 0.07, 0.04)
blk("wood_d", 0, 0, 0.04, 0.07, 0.38, 0.04)
P("cyl", "wood", (0, 0, 0.6), (0.045, 0.045, 0.6), segments=10)
rod("wood", (-0.4, 0, 1.28), (0.4, 0, 1.28), 0.03)
P("cyl", "steel_d", (0, 0, 0.82), (0.29, 0.2, 0.12), taper=1.1, segments=16)   # Schurz
P("cyl", "steel", (0, 0, 1.08), (0.27, 0.19, 0.2), taper=1.16, segments=16)     # Brustplatte
P("sphere", "steel", (0, 0, 1.26), (0.29, 0.2, 0.1), segments=16)
P("cyl", "gold", (0, 0, 0.93), (0.285, 0.2, 0.03), segments=16)                 # Guertel
P("sphere", "gold", (0, -0.2, 1.1), (0.07, 0.025, 0.09), segments=8)
P("sphere", "red", (0, -0.215, 1.1), (0.035, 0.012, 0.05), segments=8)
for sx in (-1, 1):
    P("sphere", "steel", (sx * 0.36, 0, 1.28), (0.14, 0.13, 0.1), segments=12)
    P("sphere", "gold", (sx * 0.42, -0.06, 1.3), (0.025, 0.025, 0.025), segments=6)
P("sphere", "steel", (0, 0, 1.55), (0.19, 0.2, 0.2), segments=16)               # Helm
P("cyl", "steel_d", (0, 0, 1.4), (0.2, 0.21, 0.025), segments=16)
P("cube", "black", (0, -0.19, 1.52), (0.13, 0.03, 0.035), smooth=False)         # Visier
P("cube", "black", (0, -0.2, 1.43), (0.025, 0.03, 0.07), smooth=False)
P("cube", "plume", (0, 0.0, 1.8), (0.03, 0.2, 0.12), smooth=False)             # Helmbusch
P("sphere", "plume", (0, 0.12, 1.76), (0.05, 0.1, 0.08), segments=8)
shot("armor_stand", 128, 0.016)

# Waffenwand (2,4 m breit, senkrecht)
blk("wood", 0, 0, 1.1, 1.2, 0.06, 1.0)
for i in range(-3, 4):
    blk("wood_d", i * 0.34, -0.062, 1.1, 0.012, 0.004, 1.0)
blk("wood_d", 0, -0.075, 2.12, 1.25, 0.09, 0.075)
blk("wood_d", 0, -0.075, 0.1, 1.25, 0.09, 0.075)
for sx in (-1, 1):
    blk("wood_d", sx * 1.2, -0.075, 1.1, 0.075, 0.09, 1.0)
blk("red", 0, -0.065, 1.86, 1.1, 0.01, 0.12)                                     # roter Fries
for i in range(-5, 6):
    P("sphere", "gold", (i * 0.2, -0.085, 1.86), (0.025, 0.02, 0.025), segments=6)
P("cyl", "red_d", (0, -0.09, 1.2), (0.36, 0.36, 0.02), (90, 0, 0), segments=24)  # kleiner Rundschild
P("torus", "gold", (0, -0.1, 1.2), (0.36, 0.36, 0.36), (90, 0, 0), segments=24)
P("sphere", "gold", (0, -0.13, 1.2), (0.09, 0.05, 0.09), segments=10)
sword((-0.62, -0.15, 0.62), (0.62, -0.15, 1.88))                                  # gekreuzte Schwerter
sword((0.62, -0.15, 0.62), (-0.62, -0.15, 1.88), blade="steel", guard="red_d")
P("cyl", "steel", (-0.88, -0.1, 1.15), (0.4, 0.4, 0.03), (90, 0, 0), segments=28)  # grosser Schild
P("torus", "steel_d", (-0.88, -0.11, 1.15), (0.4, 0.4, 0.4), (90, 0, 0), segments=28)
P("cube", "red", (-0.88, -0.14, 1.15), (0.055, 0.015, 0.36), smooth=False)
P("cube", "red", (-0.88, -0.14, 1.15), (0.36, 0.015, 0.055), smooth=False)
P("sphere", "gold", (-0.88, -0.15, 1.15), (0.11, 0.06, 0.11), segments=12)
rod("wood_d", (0.9, -0.12, 0.45), (0.9, -0.12, 1.75), 0.035)                      # Axt
P("cube", "steel_l", (0.9, -0.13, 1.55), (0.28, 0.02, 0.2), smooth=True)
P("cube", "steel", (1.0, -0.13, 1.55), (0.09, 0.025, 0.24), smooth=True)
P("cube", "steel", (0.78, -0.13, 1.55), (0.07, 0.025, 0.2), smooth=True)
P("sphere", "gold", (0.9, -0.15, 1.75), (0.05, 0.05, 0.05), segments=8)
P("cone", "steel_l", (0.9, -0.12, 1.86), (0.04, 0.03, 0.08), segments=6)
for sx in (-1, 1):
    P("sphere", "gold", (sx * 1.12, -0.12, 0.2), (0.05, 0.03, 0.05), segments=6)
shot("weapon_wall", 256, 0.012)

# Festtafel (3 m, mit Baenken)
blk("wood_l", 0, 0, 0.8, 1.5, 0.52, 0.045, smooth=True)
blk("wood_d", 0, -0.46, 0.7, 1.46, 0.03, 0.06)
blk("wood_d", 0, 0.46, 0.7, 1.46, 0.03, 0.06)
for sx in (-1, 1):
    for sy in (-1, 1):
        blk("wood_d", sx * 1.35, sy * 0.4, 0.38, 0.06, 0.06, 0.38)
    blk("wood_d", sx * 1.35, 0, 0.2, 0.05, 0.4, 0.04)
    for sy in (-1, 1):
        blk("wood_d", sx * 0.95, sy * 0.78, 0.2, 0.04, 0.12, 0.2)
        blk("wood_d", sx * -0.95, sy * 0.78, 0.2, 0.04, 0.12, 0.2)
for sy in (-1, 1):
    blk("wood", 0, sy * 0.8, 0.42, 1.4, 0.17, 0.03, smooth=True)
blk("red", 0, 0, 0.85, 1.1, 0.12, 0.006)                                          # Laeufer
blk("gold", 0, 0, 0.853, 1.1, 0.014, 0.006)
P("sphere", "roast", (0, 0.02, 0.98), (0.3, 0.2, 0.15), segments=14)             # Braten auf Platte
P("sphere", "roast_l", (0.05, -0.04, 1.05), (0.2, 0.12, 0.08), segments=12)
P("cyl", "steel", (0, 0.02, 0.87), (0.36, 0.28, 0.02), segments=20)
for sx in (-1, 1):
    rod("bone", (sx * 0.28, 0.02, 0.98), (sx * 0.5, 0.0, 1.06), 0.03)
    P("sphere", "bone", (sx * 0.52, 0.0, 1.07), (0.04, 0.04, 0.04), segments=8)
for x, y in ((-0.85, 0.1), (-0.5, -0.12), (0.62, -0.1), (0.95, 0.12)):             # Brote / Kaese / Aepfel
    P("sphere", "bread", (x, y, 0.92), (0.14, 0.09, 0.07), segments=10)
    P("cube", "parch_d", (x, y - 0.03, 0.98), (0.07, 0.015, 0.008), (0, 0, 20), smooth=False)
P("cyl", "cheese", (0.35, 0.3, 0.9), (0.12, 0.12, 0.045), segments=3)
for x, y in ((-0.25, 0.32), (-0.15, -0.3), (0.2, 0.32)):
    P("sphere", "apple", (x, y, 0.92), (0.055, 0.055, 0.05), segments=8)
for x, y in ((-1.1, -0.28), (-0.7, 0.3), (0.5, 0.33), (1.05, -0.3), (1.15, 0.28), (-0.35, -0.34)):   # Kruege
    P("cyl", "mug", (x, y, 0.95), (0.065, 0.065, 0.09), segments=10)
    P("cyl", "foam", (x, y, 1.05), (0.07, 0.07, 0.022), segments=10)
    P("torus", "mug", (x + 0.07, y, 0.95), (0.05, 0.05, 0.05), (90, 0, 0), segments=8)
for x in (-0.6, 0.6):
    P("cyl", "gold", (x, 0.0, 0.89), (0.05, 0.05, 0.04), segments=10)
    P("cyl", "cream", (x, 0.0, 1.02), (0.03, 0.03, 0.1), segments=10)
    P("cone", "flame_y", (x, 0.0, 1.14), (0.025, 0.025, 0.05), segments=8)
shot("feast_table", 256, 0.012)

# ===================================================== MAGIER
# Teppich blau mit Sternen
carpet_frame("blue", "gold", 1.5, 1.0, "blue_d")


def stern(x, y, r, top, rot=0):
    slab("gold", x, y, r, r, top, rot)
    slab("gold", x, y, r, r, top, rot + 45)


stern(0, 0, 0.5, 0.05)
slab("blue_d", 0, 0, 0.16, 0.16, 0.06, 22)
slab("gold", 0, 0, 0.08, 0.08, 0.068, 22)
for sx in (-1, 1):
    stern(sx * 0.95, 0, 0.2, 0.05)
    for sy in (-1, 1):
        stern(sx * 0.5, sy * 0.52, 0.11, 0.045)
        stern(sx * 1.18, sy * 0.62, 0.1, 0.045)
shot("carpet_blue", 256, 0.006, subdiv=False)

# Kessel
for k in range(3):
    t = math.radians(90 + 120 * k)
    P("cone", "iron", (0.3 * math.cos(t), 0.3 * math.sin(t), 0.13), (0.08, 0.08, 0.13), (180, 0, 0), segments=8)
P("sphere", "iron", (0, 0, 0.5), (0.46, 0.46, 0.38), segments=18)
P("sphere", "dark_l", (-0.2, -0.3, 0.55), (0.1, 0.05, 0.14), segments=8)
P("torus", "iron", (0, 0, 0.76), (0.4, 0.4, 0.4), segments=20)
P("cyl", "potion", (0, 0, 0.76), (0.36, 0.36, 0.025), segments=20)
for x, y, z, r in ((-0.1, 0.05, 0.84, 0.07), (0.12, -0.08, 0.86, 0.055), (0.05, 0.12, 0.92, 0.04), (-0.14, -0.12, 0.98, 0.035), (0.14, 0.1, 1.06, 0.03)):
    P("sphere", "bubble", (x, y, z), (r, r, r), segments=8)
for sx in (-1, 1):
    P("torus", "iron", (sx * 0.46, 0, 0.62), (0.1, 0.1, 0.1), (0, 90, 0), segments=10)
for k in range(8):
    t = math.radians(k * 45)
    P("sphere", "steel_d", (0.4 * math.cos(t) * 0.97, 0.4 * math.sin(t) * 0.97 - 0.02, 0.77), (0.018, 0.018, 0.018), segments=6)
shot("cauldron", 128, 0.016)

# Globus auf Holzstaender
ring_z = 0.62
P("torus", "wood_d", (0, 0, ring_z), (0.5, 0.5, 0.5), segments=24)
for k in range(3):
    t = math.radians(90 + 120 * k)
    rod("wood", (0.5 * math.cos(t), 0.5 * math.sin(t), ring_z), (0.62 * math.cos(t), 0.62 * math.sin(t), 0.02), 0.035)
    P("sphere", "gold", (0.5 * math.cos(t), 0.5 * math.sin(t), ring_z + 0.03), (0.03, 0.03, 0.03), segments=6)
GC, GR = Vector((0, 0, 1.0)), 0.4
P("sphere", "ocean", GC, (GR, GR, GR), segments=20)
for d, sx, sy in (((0.1, -1, 0.3), 0.17, 0.12), ((-0.6, -0.8, 0.0), 0.13, 0.17), ((0.7, -0.7, -0.2), 0.14, 0.1),
                  ((0.4, -0.5, 0.75), 0.12, 0.08), ((-0.2, -0.6, -0.8), 0.12, 0.1), ((-0.9, 0.2, 0.3), 0.1, 0.13)):
    patch("land", GC, GR - 0.01, d, sx, sy, 0.03)
tilt = math.radians(23)
ax = Vector((math.sin(tilt), 0, math.cos(tilt)))
P("torus", "brass", GC, (GR + 0.06, GR + 0.06, GR + 0.06), (90, 23, 0), segments=24)
rod("brass", GC - ax * (GR + 0.1), GC + ax * (GR + 0.12), 0.02)
P("sphere", "gold", GC + ax * (GR + 0.13), (0.04, 0.04, 0.04), segments=8)
P("torus", "gold", GC, (GR + 0.015, GR + 0.015, GR + 0.015), (0, 0, 0), segments=24)
shot("globe", 128, 0.016)

# Messing-Teleskop auf Dreibein
pv = Vector((0, 0, 1.08))
for k in range(3):
    t = math.radians(90 + 120 * k)
    rod("wood_d", pv, (0.52 * math.cos(t), 0.52 * math.sin(t), 0.0), 0.035)
    P("sphere", "brass", (0.52 * math.cos(t), 0.52 * math.sin(t), 0.02), (0.045, 0.045, 0.03), segments=6)
P("cyl", "brass", pv - Vector((0, 0, 0.08)), (0.09, 0.09, 0.06), segments=10)
P("sphere", "brass", pv, (0.1, 0.1, 0.1), segments=12)
d = Vector((-0.63, -0.52, 0.58)).normalized()
p0 = pv - d * 0.5
segs = ((0.0, 0.42, 0.075, "brass"), (0.42, 0.78, 0.062, "gold"), (0.78, 1.1, 0.05, "brass"))
for s0, s1, r, m in segs:
    rod(m, p0 + d * s0, p0 + d * s1, r)
for s in (0.0, 0.42, 0.78, 1.1):
    rod("gold_d", p0 + d * (s - 0.012), p0 + d * (s + 0.012), 0.09 - 0.012 * (s > 0.5) - 0.012 * (s > 1.0))
rod("steel_d", p0 - d * 0.1, p0, 0.035)                                          # Okular
P("sphere", "glass", p0 + d * 1.1, (0.045, 0.045, 0.02), tuple(math.degrees(t) for t in d.to_track_quat("Z", "Y").to_euler()), segments=8)
rod("brass", pv + Vector((0, 0.05, 0.1)), pv + d * 0.2 + Vector((0, 0.05, -0.1)), 0.02)
shot("telescope", 128, 0.016)

# Buecherwand (2,4 x 2,2 m)
random.seed(11)
blk("wood_dd", 0, 0.1, 1.1, 1.2, 0.1, 1.1)
for sx in (-1, 1):
    blk("wood_d", sx * 1.16, -0.02, 1.1, 0.07, 0.24, 1.1)
SH = (0.12, 0.62, 1.12, 1.62, 2.12)
for z in SH:
    blk("wood", 0, -0.02, z, 1.2, 0.24, 0.04)
blk("wood_d", 0, -0.02, 2.2, 1.26, 0.28, 0.04)
blk("wood_d", 0, -0.2, 0.06, 1.2, 0.05, 0.08)
for r, z in enumerate(SH[:-1]):
    x = -1.06
    while x < 1.04:
        mode = random.random()
        if mode < 0.08 and x < 0.6:    # liegender Stapel
            for k in range(3):
                P("cube", random.choice(BOOKS), (x + 0.2, -0.06, z + 0.04 + 0.05 * (k * 2 + 1) / 2), (0.19 - 0.02 * k, 0.12, 0.04), (0, 0, random.uniform(-6, 6)), smooth=False)
            x += 0.42
            continue
        w = random.uniform(0.04, 0.075)
        h = random.uniform(0.3, 0.44)
        m = random.choice(BOOKS)
        tilt = 0
        if random.random() < 0.06:
            tilt = random.uniform(10, 18)
        P("cube", m, (x + w, -0.08, z + 0.04 + h / 2), (w, 0.13, h / 2), (0, tilt, 0), smooth=False)
        if random.random() < 0.5:
            P("cube", "gold", (x + w, -0.215, z + 0.04 + h * 0.8), (w * 1.02, 0.004, 0.018), (0, tilt, 0), smooth=False)
            P("cube", "gold", (x + w, -0.215, z + 0.04 + h * 0.2), (w * 1.02, 0.004, 0.012), (0, tilt, 0), smooth=False)
        x += 2 * w + 0.008 + (0.05 * tilt / 18)
P("sphere", "moon", (0.9, -0.05, 2.34), (0.1, 0.1, 0.1), segments=8)                  # Kleinkram obendrauf
P("cyl", "potion", (-0.9, -0.05, 2.32), (0.06, 0.06, 0.1), segments=8)
P("cyl", "glass", (-0.9, -0.05, 2.45), (0.025, 0.025, 0.04), segments=8)
P("cube", "bk2", (0.0, -0.05, 2.30), (0.22, 0.12, 0.05), smooth=False)
P("cube", "bk1", (0.0, -0.05, 2.38), (0.18, 0.1, 0.03), (0, 0, 8), smooth=False)
shot("bookwall", 256, 0.01, subdiv=False)

# Sternenkarte (Wandkarte in Holzrahmen, senkrecht)
ZC = 1.35
blk("wood_d", 0, 0, ZC, 1.0, 0.05, 0.72)
blk("navy", 0, -0.055, ZC, 0.9, 0.008, 0.62)
for sx in (-1, 1):
    blk("gold", sx * 0.91, -0.07, ZC, 0.012, 0.01, 0.63)
blk("gold", 0, -0.07, ZC + 0.63, 0.92, 0.01, 0.012)
blk("gold", 0, -0.07, ZC - 0.63, 0.92, 0.01, 0.012)
P("torus", "gold", (0, -0.07, ZC), (0.42, 0.42, 0.42), (90, 0, 0), segments=32)
P("torus", "gold_d", (0, -0.07, ZC), (0.3, 0.3, 0.3), (90, 0, 0), segments=32)
for t in range(0, 360, 45):                                                       # Tierkreis-Marken
    a = math.radians(t)
    P("sphere", "gold", (0.36 * math.sin(a), -0.075, ZC + 0.36 * math.cos(a)), (0.03, 0.012, 0.03), segments=6)
CON = (((-0.7, 0.35), (-0.5, 0.45), (-0.3, 0.38), (-0.18, 0.5)),
       ((0.25, 0.5), (0.45, 0.42), (0.65, 0.5), (0.7, 0.3), (0.5, 0.22)),
       ((-0.7, -0.35), (-0.5, -0.45), (-0.55, -0.2), (-0.3, -0.3)),
       ((0.2, -0.3), (0.4, -0.48), (0.62, -0.38), (0.72, -0.5)))
for line in CON:
    for p, q in zip(line, line[1:]):
        rod("gold_d", (p[0], -0.068, ZC + p[1]), (q[0], -0.068, ZC + q[1]), 0.008, 0.004, kind="cube", smooth=False)
    for p in line:
        rod("star", (p[0], -0.07, ZC + p[1] - 0.06), (p[0], -0.07, ZC + p[1] + 0.06), 0.022, 0.014, kind="cube", smooth=False)
        rod("star", (p[0] - 0.06, -0.07, ZC + p[1]), (p[0] + 0.06, -0.07, ZC + p[1]), 0.022, 0.014, kind="cube", smooth=False)
        P("sphere", "star", (p[0], -0.075, ZC + p[1]), (0.035, 0.014, 0.035), segments=8)
random.seed(5)
for _ in range(22):
    x, z = random.uniform(-0.8, 0.8), random.uniform(-0.55, 0.55)
    if math.hypot(x, z) > 0.46:
        P("sphere", "star", (x, -0.068, ZC + z), (0.014, 0.01, 0.014), segments=6)
P("sphere", "moon", (-0.05, -0.075, ZC + 0.0), (0.11, 0.014, 0.11), segments=12)
P("sphere", "navy", (0.0, -0.085, ZC + 0.02), (0.1, 0.014, 0.1), segments=12)
blk("wood_d", 0, 0, 0.3, 0.8, 0.05, 0.03)
for sx in (-1, 1):
    blk("wood_d", sx * 0.8, 0.0, 0.3, 0.04, 0.05, 0.3)
shot("star_map", 256, 0.01, subdiv=False)

# ===================================================== ASSASSINEN
# dunkler Teppich
carpet_frame("red_dd", "dark_l", 1.5, 1.0, "black")
slab("red_d", 0, 0, 0.55, 0.55, 0.05, 45)
slab("black", 0, 0, 0.45, 0.45, 0.058, 45)
slab("red_d", 0, 0, 0.3, 0.3, 0.066, 45)
slab("black", 0, 0, 0.2, 0.2, 0.074, 45)
slab("red", 0, 0, 0.06, 0.06, 0.082, 45)
for sx in (-1, 1):
    for k in (-1, 0, 1):
        slab("red_d", sx * 0.95, k * 0.3, 0.1, 0.1, 0.05, 45)
        slab("black", sx * 0.95, k * 0.3, 0.05, 0.05, 0.058, 45)
shot("carpet_dark", 256, 0.006, subdiv=False)

# Kartentisch
blk("wood_dd", 0, 0, 0.8, 1.0, 0.62, 0.05)
blk("dark_l", 0, -0.58, 0.7, 0.96, 0.03, 0.06)
blk("dark_l", 0, 0.58, 0.7, 0.96, 0.03, 0.06)
for sx in (-1, 1):
    for sy in (-1, 1):
        blk("wood_dd", sx * 0.9, sy * 0.52, 0.38, 0.06, 0.06, 0.38)
blk("dark_l", 0, 0, 0.25, 0.9, 0.5, 0.025)
blk("parch_d", 0, 0, 0.858, 0.82, 0.5, 0.008)
blk("parch", 0, 0, 0.868, 0.78, 0.46, 0.008)
random.seed(21)
for k in range(9):                                                                # Land
    P("cube", random.choice(["sand", "land"]), (random.uniform(-0.55, 0.55), random.uniform(-0.28, 0.28), 0.88),
      (random.uniform(0.07, 0.17), random.uniform(0.05, 0.11), 0.004), (0, 0, random.uniform(0, 90)), smooth=False)
rod("river", (-0.7, 0.3, 0.885), (-0.2, 0.05, 0.885), 0.02, 0.006, kind="cube", smooth=False)
rod("river", (-0.2, 0.05, 0.885), (0.3, 0.2, 0.885), 0.02, 0.006, kind="cube", smooth=False)
for k in range(8):                                                                # Route
    P("cube", "red_d", (-0.6 + k * 0.15, -0.3 + 0.04 * math.sin(k), 0.89), (0.04, 0.012, 0.004), (0, 0, 10 * k), smooth=False)
rod("red", (0.42, -0.1, 0.9), (0.62, 0.1, 0.9), 0.025, 0.006, kind="cube", smooth=False)
rod("red", (0.42, 0.1, 0.9), (0.62, -0.1, 0.9), 0.025, 0.006, kind="cube", smooth=False)
for x, y in ((-0.5, 0.0), (0.0, -0.1), (0.25, 0.28)):                             # Stecknadeln
    rod("steel_d", (x, y, 0.88), (x, y, 0.98), 0.008)
    P("sphere", "pin", (x, y, 1.0), (0.04, 0.04, 0.04), segments=8)
sword((-0.55, -0.15, 0.89), (-0.12, -0.35, 0.89), guard="red_d", grip="black", w=0.014, glen=0.1, y_th=0.04)
P("cyl", "parch", (0.7, 0.34, 0.9), (0.04, 0.04, 0.12), (0, 90, 25), segments=10)                   # Schriftrolle
P("cyl", "red", (0.7, 0.34, 0.9), (0.044, 0.044, 0.025), (0, 90, 25), segments=10)
P("cyl", "gold", (-0.75, 0.38, 0.9), (0.07, 0.07, 0.025), segments=12)                              # Kerze
P("cyl", "cream", (-0.75, 0.38, 1.0), (0.04, 0.04, 0.13), segments=10)
P("cone", "flame_y", (-0.75, 0.38, 1.16), (0.032, 0.032, 0.07), segments=8)
P("sphere", "flame", (-0.75, 0.38, 1.16), (0.06, 0.06, 0.1), segments=8)
shot("map_table", 256, 0.012)

# Dolchwand (dunkles Paneel)
blk("dark", 0, 0, 1.1, 1.2, 0.06, 1.0)
for i in range(-3, 4):
    blk("black", i * 0.34, -0.062, 1.1, 0.01, 0.004, 1.0)
blk("black", 0, -0.075, 2.12, 1.25, 0.09, 0.075)
blk("black", 0, -0.075, 0.1, 1.25, 0.09, 0.075)
for sx in (-1, 1):
    blk("black", sx * 1.2, -0.075, 1.1, 0.075, 0.09, 1.0)
blk("red_d", 0, -0.066, 1.9, 1.1, 0.008, 0.07)
blk("red_d", 0, -0.066, 0.32, 1.1, 0.008, 0.05)
random.seed(4)
for row, z in enumerate((1.5, 0.95)):
    for i, x in enumerate((-0.95, -0.63, -0.32, 0.0, 0.32, 0.63, 0.95)):
        flip = 180 if (i + row) % 2 else 0
        dagger(x, z, flip + random.uniform(-14, 14), length=0.52)
for x, z, r in ((-0.48, 0.66, 0.16), (0.48, 0.66, 0.16), (0.0, 0.62, 0.2), (-0.8, 0.5, 0.11), (0.8, 0.5, 0.11), (-0.48, 1.22, 0.1), (0.48, 1.22, 0.1), (0.0, 1.22, 0.1)):
    star4(x, z, r, rot=random.uniform(0, 45))
for sx in (-1, 1):                                                                    # Wurfmesser-Guertel unten
    for k in range(3):
        dagger(sx * (0.35 + 0.12 * k), 0.28, 180 + 8 * k, length=0.3)
shot("dagger_wall", 256, 0.012)

# Schrank
blk("wood_dd", 0, 0, 0.09, 0.47, 0.22, 0.09)
blk("wood_dd", 0, 0, 0.92, 0.42, 0.19, 0.75)
blk("dark_l", 0, -0.2, 0.92, 0.395, 0.015, 0.71)
for sx in (-1, 1):
    blk("wood_dd", sx * 0.2, -0.218, 0.92, 0.17, 0.012, 0.63)
    blk("dark_l", sx * 0.2, -0.228, 0.92, 0.12, 0.008, 0.52)
    blk("red_d", sx * 0.2, -0.236, 1.26, 0.08, 0.006, 0.12)
    P("sphere", "gold", (sx * 0.05, -0.24, 0.9), (0.025, 0.025, 0.025), segments=8)
    blk("wood_dd", sx * 0.2, -0.28, 0.09, 0.04, 0.04, 0.09)
blk("black", 0, -0.222, 0.92, 0.005, 0.01, 0.66)
blk("wood_dd", 0, 0, 1.72, 0.5, 0.26, 0.05)
blk("dark_l", 0, -0.22, 1.66, 0.5, 0.06, 0.03)
slab_d = P("cube", "red_d", (0, -0.28, 1.77), (0.5, 0.02, 0.02), smooth=False)
P("sphere", "gold", (0, -0.25, 1.4), (0.045, 0.01, 0.045), segments=8)
P("sphere", "black", (0, -0.26, 1.4), (0.012, 0.01, 0.018), segments=6)
shot("cabinet", 128, 0.016)

# Stehlaterne mit rotem Licht
P("cyl", "iron", (0, 0, 0.05), (0.2, 0.2, 0.05), segments=12)
P("cyl", "iron", (0, 0, 0.18), (0.1, 0.1, 0.08), taper=0.6, segments=10)
P("cyl", "dark_l", (0, 0, 0.9), (0.04, 0.04, 0.75), segments=8)
P("cyl", "gold_d", (0, 0, 0.5), (0.065, 0.065, 0.03), segments=8)
P("cyl", "iron", (0, 0, 1.68), (0.17, 0.17, 0.035), taper=1.1, segments=8)
for sx in (-1, 1):
    for sy in (-1, 1):
        rod("iron", (sx * 0.15, sy * 0.15, 1.7), (sx * 0.15, sy * 0.15, 2.08), 0.022)
P("cube", "lamp_r", (0, 0, 1.89), (0.14, 0.14, 0.18), smooth=False)
P("sphere", "lamp_c", (0, 0, 1.89), (0.1, 0.1, 0.12), segments=10)
P("cyl", "iron", (0, 0, 2.1), (0.2, 0.2, 0.025), taper=1.0, segments=8)
P("cone", "iron", (0, 0, 2.2), (0.19, 0.19, 0.1), segments=8)
P("torus", "iron", (0, 0, 2.34), (0.05, 0.05, 0.05), (90, 0, 0), segments=8)
shot("lantern_red", 128, 0.016, subdiv=False, k=0.8)

bpy.ops.wm.save_as_mainfile(filepath=BLEND_PATH)
print(f"RESULT rendered={count} randwarnungen={warnings}")
