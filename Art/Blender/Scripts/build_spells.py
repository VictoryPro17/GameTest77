"""RE:BORN - Zauber-Grafik (v2, Meilenstein 5, Comic-Look)

Cel-Shading + Kontur wie Figuren und Gegner. Seitenansicht, alles zeigt nach rechts (+X);
Unity dreht die Bilder in Flugrichtung. Eigene Datei: Art/Blender/Reborn_Spells.blend
  spell_dragon_head.png, spell_dragon_body.png, spell_waterball.png,
  spell_fireball_0.png, spell_fireball_1.png                      (128 x 128 = 1 Einheit bei PPU 128)
  icon_wasserkugel.png, icon_wasserdrache.png, icon_feuerball.png, icon_flammenring.png (48 x 48)

Aufruf:  blender.exe -b --factory-startup --python build_spells.py -- <projektordner>
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
OUT = os.path.join(PROJECT, "Assets", "_Project", "Sprites", "Spells")
BLEND_PATH = os.path.join(PROJECT, "Art", "Blender", "Reborn_Spells.blend")
os.makedirs(OUT, exist_ok=True)

bpy.ops.wm.read_factory_settings(use_empty=True)
scene = bpy.context.scene
scene.name = "Spells"
L.setup_scene(scene)
cd = bpy.data.cameras.new("SpellCamera"); cd.type = "ORTHO"; cd.ortho_scale = 1.6
cam = bpy.data.objects.new("SpellCamera", cd); scene.collection.objects.link(cam)
cam.location = (0, -10, 0); cam.rotation_euler = (math.radians(90), 0, 0); scene.camera = cam
L.sun(scene, energy=3.2, rot=(50, -20, -35))

M = {"water": L.cel("ZWasser", (0.2, 0.58, 0.95), shadow=0.6, emission=0.25),
     "water_dark": L.cel("ZWasserDunkel", (0.1, 0.33, 0.72), shadow=0.6, emission=0.15),
     "water_light": L.cel("ZWasserHell", (0.55, 0.85, 1.0), shadow=0.7, emission=0.3),
     "foam": L.cel("ZSchaum", (0.88, 0.97, 1.0), shadow=0.75, emission=0.35),
     "eye": L.cel("ZAuge", (1.0, 0.96, 0.7), shadow=0.9, emission=0.6),
     "pupil": L.cel("ZPupille", (0.03, 0.06, 0.15), shadow=1.0),
     "tooth": L.cel("ZZahn", (0.95, 0.97, 1.0), shadow=0.8),
     "fire_core": L.cel("ZGlut", (1.0, 0.9, 0.5), shadow=0.95, emission=0.2),
     "fire": L.cel("ZFeuer", (0.98, 0.45, 0.08), shadow=0.7, emission=0.05),
     "fire_dark": L.cel("ZFeuerRot", (0.8, 0.16, 0.05), shadow=0.7),
     "gold": L.cel("ZGold", (0.95, 0.75, 0.25), light=1.1),
     "bg_water": L.cel("ZGrundWasser", (0.08, 0.16, 0.3), shadow=0.8, mid=0.9),
     "bg_fire": L.cel("ZGrundFeuer", (0.3, 0.09, 0.04), shadow=0.8, mid=0.9)}


def P(kind, mat, loc, sc, rot=(0, 0, 0), **kw):
    return L.shape(kind, loc, sc, rot, M[mat], None, collection=scene.collection, **kw)


def make(name, parts, outline=0.02, smooth=True):
    o = L.join(parts, name)
    if smooth:
        L.subdivide(o, 1)
    L.add_outline(o, outline)
    o.hide_render = True
    return o


def flame(mat, x, z, length, radius, tilt, rz=0.0):
    """Flammenzunge: Kegel, Spitze zeigt nach hinten (-X), leicht nach oben gebogen"""
    return P("cone", mat, (x - length * 0.5, 0, z), (radius, radius, length * 0.5), rot=(0, -90 + tilt, rz), segments=10)


# ---------------- Wasserdrache: Kopf (Schnauze nach +X, Maul leicht offen, Hoerner, Maehne)
head = [P("sphere", "water", (-0.08, 0, 0.04), (0.3, 0.24, 0.26)),
        P("sphere", "water", (0.22, 0, 0.02), (0.28, 0.17, 0.13)),                     # Oberkiefer
        P("sphere", "water_light", (0.2, 0, -0.13), (0.24, 0.13, 0.06), rot=(0, 10, 0)),  # Unterkiefer
        P("sphere", "water_dark", (0.0, 0, -0.08), (0.2, 0.18, 0.08)),                  # Kehle
        P("sphere", "water_dark", (0.47, 0, 0.06), (0.05, 0.1, 0.05)),                  # Nasenspitze
        P("sphere", "pupil", (0.46, -0.08, 0.08), (0.02, 0.012, 0.015)),                # Nasenloch
        P("sphere", "water_dark", (0.06, -0.16, 0.17), (0.12, 0.06, 0.04), rot=(0, -12, 0)),  # Braue
        P("sphere", "eye", (0.05, -0.19, 0.1), (0.075, 0.03, 0.06)),
        P("sphere", "pupil", (0.075, -0.215, 0.1), (0.025, 0.012, 0.045)),
        P("sphere", "foam", (0.06, -0.225, 0.13), (0.012, 0.006, 0.012))]
head += [P("cone", "tooth", (x, -0.1, -0.05), (0.02, 0.02, 0.035), rot=(180, 0, 0), segments=6) for x in (0.18, 0.28, 0.38)]
head += [P("cone", "foam", (-0.24, y, 0.3), (0.045, 0.045, 0.2), rot=(0, -55, 0), segments=8) for y in (-0.09, 0.09)]  # Hoerner
for i in range(5):                                                                      # Maehne aus Gischt
    a = -20 + i * 22
    head.append(P("cone", "foam" if i % 2 == 0 else "water_light", (-0.32 - 0.03 * i, 0, 0.18 - 0.09 * i),
                  (0.06, 0.06, 0.16 - 0.01 * i), rot=(0, -100 + a * 0.5, 0), segments=8))

dragon_head = make("DragonHead", head)

# ---------------- Koerperteil (Rueckenflosse, heller Bauch, Schuppenpunkte)
body = [P("sphere", "water", (0, 0, 0), (0.27, 0.2, 0.2)),
        P("sphere", "water_light", (0, -0.03, -0.1), (0.2, 0.17, 0.1)),
        P("cone", "foam", (-0.06, 0, 0.25), (0.09, 0.04, 0.13), rot=(0, -35, 0), segments=8),
        P("sphere", "water_light", (0.02, -0.08, 0.12), (0.14, 0.08, 0.05))]

dragon_body = make("DragonBody", body)

# ---------------- Wasserkugel (Kern, Wirbel, Tropfenschweif)
wb = [P("sphere", "water", (0, 0, 0), (0.25, 0.25, 0.25)),
      P("sphere", "water_light", (0.03, -0.06, 0.03), (0.17, 0.17, 0.17)),
      P("sphere", "foam", (0.06, -0.2, 0.1), (0.06, 0.03, 0.05)),
      P("torus", "water_light", (0, 0, 0), (0.3, 0.3, 0.12), rot=(78, 0, 25), segments=14)]
for i in range(4):
    wb.append(P("sphere", "water" if i % 2 else "water_light", (-0.33 - 0.11 * i, 0, 0.08 * math.sin(i * 1.7)), (0.065 - 0.012 * i,) * 3, segments=10))
waterball = make("Waterball", wb)

# ---------------- Feuerball (2 Bilder: Flammenzungen tanzen)
fireballs = []
for frame in range(2):
    random.seed(40 + frame)
    fb = [P("sphere", "fire_core", (0.03, -0.04, 0), (0.15, 0.15, 0.15)),
          P("sphere", "fire", (0, 0, 0), (0.22, 0.22, 0.22))]
    for i in range(7):
        a = 2 * math.pi * i / 7 + frame * 0.45
        z = 0.15 * math.sin(a)
        length = (0.42 if i % 2 == 0 else 0.3) + random.uniform(-0.05, 0.05)
        fb.append(flame("fire" if i % 2 == 0 else "fire_dark", -0.08, z, length + 0.1, 0.11, 20 * math.sin(a) + random.uniform(-8, 8)))
    fb.append(flame("fire_core", -0.05, 0.0, 0.3, 0.08, 0))
    fireballs.append(make(f"Fireball{frame}", fb, 0.018))

# ---------------- Flammenring (nur Icon): gekippter Ring mit Flammen
ring = [P("torus", "fire_dark", (0, 0, -0.1), (0.42, 0.42, 0.42), rot=(72, 0, 0), segments=18)]
for i in range(10):
    a = 2 * math.pi * i / 10
    x, y = 0.42 * math.cos(a), 0.42 * math.sin(a)
    ring.append(P("cone", "fire_core" if i % 2 else "fire", (x, y * math.cos(math.radians(72)), y * math.sin(math.radians(72)) + 0.06),
                  (0.09, 0.09, 0.22), segments=8))
flame_ring = make("FlameRing", ring)

# ---------------- Icon-Hintergruende: Scheibe mit Goldrand, zur Kamera gedreht
backs = {}
for key in ("bg_water", "bg_fire"):
    backs[key] = make("Icon_" + key, [P("cyl", key, (0, 0.6, 0), (0.64, 0.64, 0.03), rot=(90, 0, 0), segments=32),
                                      P("cyl", "gold", (0, 0.66, 0), (0.74, 0.74, 0.03), rot=(90, 0, 0), segments=32)], 0.02, smooth=False)


def render(objs, file, size, ortho):
    for o in objs:
        o.hide_render = False
    scene.render.resolution_x = scene.render.resolution_y = size
    cd.ortho_scale = ortho
    L.render_still(scene, os.path.join(OUT, file))
    for o in objs:
        o.hide_render = True


count = 0
for obj, file in ((dragon_head, "spell_dragon_head.png"), (dragon_body, "spell_dragon_body.png"), (waterball, "spell_waterball.png")):
    render([obj], file, 128, 1.6); count += 1
for i, fb in enumerate(fireballs):
    render([fb], f"spell_fireball_{i}.png", 128, 1.6); count += 1
for obj, bg, file, ortho in ((waterball, "bg_water", "icon_wasserkugel.png", 1.6),
                             (dragon_head, "bg_water", "icon_wasserdrache.png", 1.6),
                             (fireballs[0], "bg_fire", "icon_feuerball.png", 1.6),
                             (flame_ring, "bg_fire", "icon_flammenring.png", 1.6)):
    render([obj, backs[bg]], file, 48, ortho); count += 1

bpy.ops.wm.save_as_mainfile(filepath=BLEND_PATH)
print(f"RESULT rendered={count}")
