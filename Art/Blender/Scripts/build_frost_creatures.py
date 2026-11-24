"""RE:BORN - Frost-Gegner (Meilenstein 9a, Comic-Look)

Eiswolf: oeffnet Art/Blender/Reborn_Enemies.blend (Wolf-Rig + Aktionen "Wolf_<anim>"), faerbt die Wolf-Materialien
eisig um (weiss-hellblaues Fell, cyanfarbene Augen), setzt Frost- und Eiskristall-Zacken auf den Ruecken und
rendert die Aktionen wie build_enemies.py nach Sprites/IceWolf (idle 4, walk 6, attack 5, hurt 2, death 4).
Frostgeist: neue schwebende Eisgestalt mit Eissplittern, die um den Koerper kreisen (idle 4, walk 6, attack 5 = Zauber-Geste,
hurt 2, death 4 = zerspringt) nach Sprites/FrostSpirit, 128 px, Fusspunkt-Pivot wie alle Figuren (0.5, 0.2138).
Die Datei wird als Art/Blender/Reborn_FrostCreatures.blend gespeichert (Reborn_Enemies.blend bleibt unveraendert).

Aufruf:  blender.exe -b Art/Blender/Reborn_Enemies.blend --python build_frost_creatures.py -- <projektordner>
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
SPRITES = os.path.join(PROJECT, "Assets", "_Project", "Sprites")
BLEND_PATH = os.path.join(PROJECT, "Art", "Blender", "Reborn_FrostCreatures.blend")
bpy.ops.wm.open_mainfile(filepath=os.path.join(PROJECT, "Art", "Blender", "Reborn_Enemies.blend"))
scene = bpy.data.scenes["Enemies"]
if bpy.context.window:
    bpy.context.window.scene = scene


def fcel(name, light, shadow, emission=0.0):
    """Comic-Material mit blauem Schattenton"""
    m = L.cel(name, light, emission=emission)
    ramp = [n for n in m.node_tree.nodes if n.type == "VALTORGB"][0]
    els = ramp.color_ramp.elements
    els[0].color = (*shadow, 1.0)
    els[1].color = (*[shadow[i] + (light[i] - shadow[i]) * 0.55 for i in range(3)], 1.0)
    els[2].color = (*light, 1.0)
    return m


total = 0
warnings = 0


def render_clip(rig, scene_, anim, direction, angle, frames, out_dir, file_prefix="", scales=None):
    global total, warnings
    rig.rotation_euler = (0, 0, math.radians(angle))
    for i, f in frames:
        if scales:
            rig.scale = (scales[i],) * 3
        L.set_frame(scene_, f)
        path = os.path.join(out_dir, f"{file_prefix}{anim}_{direction}_{i}.png")
        L.render_still(scene_, path)
        total += 1
        e = L.edge_pixels(path)
        if e:
            warnings += 1
            print("WARNUNG Randpixel:", out_dir, anim, direction, i, e)


# ======================================================================== Eiswolf
OUT = os.path.join(SPRITES, "IceWolf")
os.makedirs(OUT, exist_ok=True)
M = {"fur": fcel("WolfFell", (0.82, 0.92, 1.0), (0.32, 0.48, 0.8)),
     "fur_light": fcel("WolfFellHell", (0.98, 1.0, 1.0), (0.6, 0.76, 0.96)),
     "fur_dark": fcel("WolfFellDunkel", (0.45, 0.66, 0.92), (0.18, 0.28, 0.58)),
     "eye": fcel("WolfAugen", (0.15, 0.95, 1.0), (0.1, 0.8, 0.95), emission=1.5),
     "nose": fcel("WolfNase", (0.12, 0.2, 0.4), (0.05, 0.08, 0.2)),
     "claw": fcel("WolfKralle", (0.82, 0.96, 1.0), (0.4, 0.6, 0.9)),
     "mouth": fcel("WolfMaul", (0.35, 0.6, 0.85), (0.15, 0.3, 0.6))}
ice = fcel("EisKristall", (0.25, 0.88, 1.0), (0.1, 0.55, 0.95), emission=0.6)
snow = fcel("EisSchnee", (1.0, 1.0, 1.0), (0.68, 0.82, 0.98))
wolf_rig = bpy.data.objects["WolfRig"]
wolf = bpy.data.objects["Wolf"]
parts = []
for i, y in enumerate((-0.2, -0.04, 0.12, 0.27)):                    # Eiskristall-Zacken auf dem Ruecken
    parts.append(L.shape("cone", (0, y, 0.8 - 0.02 * i), (0.06, 0.06, 0.2 - 0.02 * i), (-25, 0, 0), ice, "body",
                         collection=scene.collection, segments=5, smooth=False))
for y, sc in ((-0.12, 0.7), (0.2, 0.7)):                                # kleine Schneehauben am Rand
    parts.append(L.shape("sphere", (0, y, 0.67), (0.1 * sc, 0.09, 0.04), (0, 0, 0), snow, "body", collection=scene.collection, segments=10))
parts.append(L.shape("sphere", (0, -0.3, 0.74), (0.1, 0.1, 0.045), (0, 0, 0), snow, "neck", collection=scene.collection, segments=10))
parts.append(L.shape("cone", (0, 0.66, 0.78), (0.03, 0.03, 0.08), (-60, 0, 0), ice, "tail", collection=scene.collection, segments=5, smooth=False))
frost = L.join(parts, "Frostzacken")
L.attach_to_rig(frost, wolf_rig)
L.add_outline(frost, 0.012)
wolf.hide_render = False
frost.hide_render = False
for name, (count, loop) in {"idle": (4, True), "walk": (6, True), "attack": (5, False), "hurt": (2, False), "death": (4, False)}.items():
    act = bpy.data.actions["Wolf_" + name]
    L.use_action(wolf_rig, act)
    end = act.frame_range[1]
    frames = list(enumerate(L.sample_frames(end, count, loop)))
    for direction, angle in L.DIRECTIONS.items():
        # liegender Wolf waere sonst zu lang fuer das Bild: im Tod langsam etwas kleiner
        render_clip(wolf_rig, scene, name, direction, angle, frames, OUT, scales=(1.3, 1.22, 1.1, 0.95) if name == "death" else None)
wolf_rig.rotation_euler = (0, 0, 0)
wolf_rig.scale = (1.3, 1.3, 1.3)
wolf.hide_render = True
frost.hide_render = True

# ======================================================================== Frostgeist
OUT = os.path.join(SPRITES, "FrostSpirit")
os.makedirs(OUT, exist_ok=True)
C = {"body": fcel("GeistKoerper", (0.72, 0.9, 1.0), (0.32, 0.52, 0.88)),
     "body_l": fcel("GeistHell", (0.92, 0.98, 1.0), (0.55, 0.74, 0.97)),
     "wisp": fcel("GeistNebel", (0.6, 0.84, 1.0), (0.28, 0.5, 0.9)),
     "dark": fcel("GeistDunkel", (0.12, 0.2, 0.46), (0.05, 0.1, 0.3)),
     "eye": fcel("GeistAugen", (0.05, 0.85, 1.0), (0.05, 0.75, 1.0), emission=0.9),
     "core": fcel("GeistKern", (0.3, 0.95, 1.0), (0.2, 0.8, 1.0), emission=1.2),
     "ice": fcel("GeistEis", (0.55, 0.9, 1.0), (0.2, 0.5, 0.9), emission=0.3),
     "shard": fcel("GeistSplitter", (0.1, 0.8, 1.0), (0.04, 0.5, 0.92), emission=0.5),
     "shadow": fcel("GeistSchatten", (0.3, 0.38, 0.62), (0.3, 0.38, 0.62))}
bones = {"root": ((0, 0, 0), (0, 0, 0.1), None),
         "body": ((0, 0, 0.6), (0, 0, 1.05), "root"),
         "head": ((0, 0, 1.05), (0, 0, 1.5), "body"),
         "tail1": ((0, 0, 0.62), (0, 0.04, 0.4), "body"),
         "tail2": ((0, 0.04, 0.4), (0, 0.1, 0.22), "tail1"),
         "orbit": ((0, 0, 0.95), (0, 0, 1.05), "root")}
for s, sg in (("L", -1), ("R", 1)):
    bones[f"upperarm.{s}"] = ((0.27 * sg, 0, 1.12), (0.4 * sg, 0, 0.92), "body")
    bones[f"forearm.{s}"] = ((0.4 * sg, 0, 0.92), (0.47 * sg, 0, 0.7), f"upperarm.{s}")
rig = L.build_rig(scene, "SpiritRig", bones)
G = lambda k, b, m, loc, sc, rot=(0, 0, 0), **kw: L.shape(k, loc, sc, rot, C[m], b, collection=scene.collection, **kw)


def seg(a, b, r0, r1, mat, bone, segs=10):
    a, b = Vector(a), Vector(b)
    d = b - a
    rot = d.to_track_quat("Z", "Y").to_euler()
    return L.shape("cyl", (a + b) / 2, (r0, r0, d.length / 2 * 1.05), tuple(math.degrees(x) for x in rot), C[mat], bone,
                   collection=scene.collection, taper=r1 / r0, segments=segs)


sp = [G("sphere", "body", "body", (0, 0, 1.0), (0.3, 0.24, 0.3), segments=18),               # Brust
      G("sphere", "body", "body_l", (0, -0.07, 1.06), (0.2, 0.15, 0.2)),
      G("cone", "body", "core", (0, -0.2, 1.02), (0.07, 0.07, 0.09), (90, 0, 0), segments=6, smooth=False),   # Eiskern vorne
      seg((0, 0, 0.88), (0, 0.03, 0.46), 0.27, 0.13, "wisp", "tail1"),                          # wallender Unterleib
      seg((0, 0.03, 0.46), (0, 0.08, 0.2), 0.13, 0.015, "wisp", "tail2"),
      G("sphere", "tail1", "body_l", (0, -0.07, 0.62), (0.13, 0.09, 0.15)),
      G("cone", "tail1", "ice", (0.2, 0.08, 0.56), (0.05, 0.05, 0.12), (180, 0, -35), segments=5, smooth=False),
      G("cone", "tail1", "ice", (-0.2, 0.08, 0.56), (0.05, 0.05, 0.12), (180, 0, 35), segments=5, smooth=False),
      # Kopf
      G("sphere", "head", "body", (0, 0, 1.36), (0.25, 0.23, 0.26), segments=18),
      G("sphere", "head", "body_l", (0, -0.05, 1.42), (0.17, 0.13, 0.15)),
      G("sphere", "head", "dark", (0.095, -0.18, 1.38), (0.075, 0.05, 0.09)),
      G("sphere", "head", "dark", (-0.095, -0.18, 1.38), (0.075, 0.05, 0.09)),
      G("sphere", "head", "eye", (0.095, -0.215, 1.385), (0.048, 0.022, 0.06)),
      G("sphere", "head", "eye", (-0.095, -0.215, 1.385), (0.048, 0.022, 0.06)),
      G("sphere", "head", "dark", (0, -0.225, 1.26), (0.045, 0.014, 0.028)),
      # Eiskrone und Schulterzacken
      G("cone", "head", "ice", (0, 0.0, 1.68), (0.06, 0.06, 0.2), (0, 0, 0), segments=5, smooth=False),
      G("cone", "head", "ice", (0.14, 0.0, 1.6), (0.05, 0.05, 0.15), (0, 28, 0), segments=5, smooth=False),
      G("cone", "head", "ice", (-0.14, 0.0, 1.6), (0.05, 0.05, 0.15), (0, -28, 0), segments=5, smooth=False),
      G("cone", "head", "body_l", (0.07, 0.1, 1.64), (0.04, 0.04, 0.12), (-18, 12, 0), segments=5, smooth=False),
      G("cone", "head", "body_l", (-0.07, 0.1, 1.64), (0.04, 0.04, 0.12), (-18, -12, 0), segments=5, smooth=False)]
for s, sg in (("L", -1), ("R", 1)):
    sp += [G("cone", "body", "ice", (0.3 * sg, 0.02, 1.26), (0.06, 0.06, 0.17), (0, 35 * sg, 0), segments=5, smooth=False),
           G("sphere", f"upperarm.{s}", "body", (0.27 * sg, 0, 1.1), (0.11, 0.11, 0.11)),
           seg((0.27 * sg, 0, 1.1), (0.4 * sg, 0, 0.92), 0.075, 0.062, "body", f"upperarm.{s}"),
           G("sphere", f"forearm.{s}", "body_l", (0.4 * sg, 0, 0.92), (0.075, 0.075, 0.075)),
           seg((0.4 * sg, 0, 0.92), (0.47 * sg, 0, 0.72), 0.062, 0.05, "body", f"forearm.{s}"),
           G("sphere", f"forearm.{s}", "body_l", (0.47 * sg, 0, 0.7), (0.075, 0.07, 0.08))]
    for f in range(3):                                           # Eisklauen
        sp.append(G("cone", f"forearm.{s}", "ice", (0.47 * sg + 0.035 * (f - 1), -0.01 * (f % 2), 0.62), (0.017, 0.017, 0.07), (180, 0, 0), segments=4, smooth=False))
# Splitter, die um den Koerper kreisen (4 gleiche Plaetze im Abstand von 90 Grad -> Schleife nach 90 Grad)
for k in range(4):
    a = math.pi / 2 * k
    r = 0.52
    z = (0.68, 1.12, 0.86, 1.3)[k]
    x, y = r * math.cos(a), r * math.sin(a)
    sp += [G("cone", "orbit", "shard", (x, y, z + 0.12), (0.085, 0.085, 0.18), (0, 0, 0), segments=4, smooth=False),
           G("cone", "orbit", "ice", (x, y, z - 0.09), (0.085, 0.085, 0.13), (180, 0, 0), segments=4, smooth=False)]
spirit = L.join(sp, "Frostgeist")
L.attach_to_rig(spirit, rig)
L.subdivide(spirit, 1)
L.add_outline(spirit, 0.02)
# Schatten am Boden (getrennt, ohne Kontur) - die Luecke zeigt, dass der Geist schwebt
sh_obj = L.shape("cyl", (0, 0.04, 0.004), (0.3, 0.22, 0.004), (0, 0, 0), C["shadow"], "root", collection=scene.collection, segments=20)
L.attach_to_rig(sh_obj, rig)
poser = L.Poser(rig)
arms = lambda l, r, fl=0, fr=0, zl=0, zr=0: {"upperarm.L": {"rx": l, "rz": zl}, "upperarm.R": {"rx": r, "rz": zr},
                                              "forearm.L": {"rx": fl}, "forearm.R": {"rx": fr}}
tails = lambda a, b=0: {"tail1": {"rz": a}, "tail2": {"rz": b}}
# Schleifen: die letzte Schluesselpose ist die erste mit 90 Grad weiter gedrehten Splittern (dann wie Pose 0)
anims = {
    "idle": ([{**arms(8, 8, 10, 10), **tails(8, 14), "orbit": {"ry": 0}, "body": {"z": 0.0}},
              {**arms(12, 12, 14, 14), **tails(0, 0), "orbit": {"ry": 22}, "body": {"z": 0.07}},
              {**arms(8, 8, 10, 10), **tails(-8, -14), "orbit": {"ry": 45}, "body": {"z": 0.0}},
              {**arms(4, 4, 6, 6), **tails(0, 0), "orbit": {"ry": 67}, "body": {"z": -0.04}},
              {**arms(8, 8, 10, 10), **tails(8, 14), "orbit": {"ry": 90}, "body": {"z": 0.0}}], 4, "loop"),
    "walk": ([{**arms(24, -24, 16, -6), **tails(12, 20), "body": {"rx": 10, "z": 0.03}, "head": {"rx": 5}, "orbit": {"ry": 0}},
              {**arms(10, -10, 10, 10), **tails(0, 0), "body": {"rx": 12, "z": 0.09}, "orbit": {"ry": 30}},
              {**arms(-24, 24, -6, 16), **tails(-12, -20), "body": {"rx": 10, "z": 0.03}, "head": {"rx": 5}, "orbit": {"ry": 60}},
              {**arms(10, -10, 10, 10), **tails(0, 0), "body": {"rx": 12, "z": 0.09}, "orbit": {"ry": 90}},
              {**arms(24, -24, 16, -6), **tails(12, 20), "body": {"rx": 10, "z": 0.03}, "head": {"rx": 5}, "orbit": {"ry": 120}}], 6, "loop"),
    "attack": ([{**arms(10, 10, 20, 20), "body": {"rx": -6, "z": 0.0}, "head": {"rx": -6}},
                {**arms(60, 60, 30, 30, 22, -22), "body": {"rx": -12, "z": 0.04}, "head": {"rx": -10}, "orbit": {"ry": 20}},
                {**arms(110, 110, 15, 15, 38, -38), "body": {"rx": -20, "z": 0.1}, "head": {"rx": -14}, "orbit": {"ry": 60}},
                {**arms(90, 90, -10, -10, 28, -28), "body": {"rx": 14, "z": 0.02}, "head": {"rx": 10}, "orbit": {"ry": 120}},
                {**arms(30, 30, 10, 10), "body": {"rx": 4}, "orbit": {"ry": 150}}], 5, "once"),
    "hurt": ([{**arms(-25, -25, 20, 20, 30, -30), "body": {"rx": -22, "z": 0.05}, "head": {"rx": -18}, **tails(0, 0), "orbit": {"ry": 20}},
              {**arms(-8, -8, 10, 10), "body": {"rx": -8}, "head": {"rx": -6}, "orbit": {"ry": 35}}], 2, "once"),
    "death": ([{**arms(-20, -20, 20, 20, 25, -25), "body": {"rx": -14, "z": 0.04}, "head": {"rx": -14}, "orbit": {"ry": 20, "s": 1.08}},
               {"body": {"rx": -10, "s": 0.85, "z": -0.05}, "head": {"rx": -20}, **arms(-30, -30, 10, 10, 40, -40), "orbit": {"ry": 60, "s": 1.2}},
               {"body": {"s": 0.55, "z": -0.1}, "head": {"rx": -10}, "root": {"z": -0.02}, "orbit": {"ry": 110, "s": 1.3}},
               {"body": {"s": 0.22, "z": -0.15}, "orbit": {"ry": 170, "s": 1.35}}], 4, "once")}
os.makedirs(OUT, exist_ok=True)
for anim, (keys, count, mode) in anims.items():
    act, end = L.make_action(rig, poser, "Geist_" + anim, keys, False)
    frames = list(enumerate(L.sample_frames(end, count, mode == "loop")))
    for direction, angle in L.DIRECTIONS.items():
        render_clip(rig, scene, anim, direction, angle, frames, OUT)
rig.rotation_euler = (0, 0, 0)
spirit.hide_render = True
sh_obj.hide_render = True
scene["foot_pivot"] = L.foot_pivot(scene)
bpy.ops.wm.save_as_mainfile(filepath=BLEND_PATH)
print(f"RESULT rendered={total} randwarnungen={warnings} pivot={scene['foot_pivot'][0]},{scene['foot_pivot'][1]}")
