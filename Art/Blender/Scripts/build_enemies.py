"""RE:BORN - Gegner (Meilenstein 5, Comic-Look): Waldschleim, Schattenwolf, Riesenspinne

Baut jede Figur mit eigenem Skelett und rendert idle 4, walk 6, attack 5, hurt 2, death 4
in 4 Richtungen als 128x128-Sprites (gleiche Kamera wie der Spieler, Fuesse am selben Bildpunkt).
Ersetzt build_slime.py. Eigene Datei: Art/Blender/Reborn_Enemies.blend

Aufruf:  blender.exe -b --factory-startup --python build_enemies.py -- <projektordner> [schleim wolf spinne]
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
ONLY = set(argv[1:]) or {"schleim", "wolf", "spinne"}
SPRITES = os.path.join(PROJECT, "Assets", "_Project", "Sprites")
BLEND_PATH = os.path.join(PROJECT, "Art", "Blender", "Reborn_Enemies.blend")

bpy.ops.wm.read_factory_settings(use_empty=True)
scene = bpy.context.scene
scene.name = "Enemies"
L.setup_scene(scene)
L.camera(scene, target=(0, 0, 0.85), ortho=2.1)
L.sun(scene)


def seg(a, b, radius, mat, bone, taper=1.0):
    """Zylinder von Punkt a nach Punkt b"""
    a, b = Vector(a), Vector(b)
    d = b - a
    rot = d.to_track_quat("Z", "Y").to_euler()
    return L.shape("cyl", (a + b) / 2, (radius, radius, d.length / 2), tuple(math.degrees(x) for x in rot), mat, bone,
                   collection=scene.collection, taper=taper)


def finish(parts, name, rig, outline=0.014, subdiv=1):
    o = L.join(parts, name)
    L.attach_to_rig(o, rig)
    if subdiv:
        L.subdivide(o, subdiv)
    L.add_outline(o, outline)
    return o


def hide(objs):
    for o in objs:
        o.hide_render = True


total = 0

# ======================================================================== Waldschleim
if "schleim" in ONLY:
    M = {"goo": L.cel("Schleim", (0.18, 0.55, 0.16), shadow=0.55),
         "goo_dark": L.cel("SchleimDunkel", (0.1, 0.36, 0.1)),
         "white": L.cel("SchleimAugenweiss", (0.97, 0.98, 0.95), shadow=0.9),
         "pupil": L.cel("SchleimPupille", (0.05, 0.08, 0.05)),
         "shine": L.cel("SchleimGlanz", (1, 1, 1), shadow=1, mid=1, emission=0.6),
         "mouth": L.cel("SchleimMund", (0.08, 0.25, 0.08))}
    rig = L.build_rig(scene, "SlimeRig", {"root": ((0, 0, 0), (0, 0, 0.1), None), "body": ((0, 0, 0.02), (0, 0, 0.6), "root")})
    S = lambda k, m, loc, sc, rot=(0, 0, 0), **kw: L.shape(k, loc, sc, rot, M[m], "body", collection=scene.collection, **kw)
    parts = [S("sphere", "goo", (0, 0, 0.26), (0.42, 0.4, 0.3), segments=24),
             S("sphere", "goo", (0, 0, 0.08), (0.45, 0.43, 0.09)),
             S("sphere", "goo_dark", (0.2, 0.25, 0.1), (0.09, 0.09, 0.06)),
             S("sphere", "goo", (-0.32, -0.15, 0.04), (0.08, 0.08, 0.04)),
             S("sphere", "goo", (0.35, 0.05, 0.03), (0.06, 0.06, 0.03))]
    for s in (-1, 1):
        parts += [S("sphere", "white", (0.13 * s, -0.33, 0.33), (0.085, 0.04, 0.1)),
                  S("sphere", "pupil", (0.13 * s, -0.365, 0.32), (0.045, 0.02, 0.06)),
                  S("sphere", "shine", (0.11 * s, -0.375, 0.35), (0.016, 0.008, 0.016))]
    parts += [S("cube", "mouth", (0, -0.36, 0.2), (0.06, 0.01, 0.012)),
              S("sphere", "shine", (-0.18, -0.2, 0.45), (0.09, 0.05, 0.05))]
    slime = finish(parts, "Slime", rig)
    poser = L.Poser(rig)
    anims = {
        "idle": ([{}, {"body": {"sz": 0.9}}, {"body": {"sz": 1.08}}, {"body": {"sz": 0.97}}], 4, True),
        "walk": ([{"body": {"sz": 0.78}}, {"body": {"sz": 1.2}, "root": {"z": 0.12}}, {"body": {"sz": 1.05}, "root": {"z": 0.18}}, {"body": {"sz": 0.85}, "root": {"z": 0.02}}], 6, True),
        "attack": ([{"body": {"sz": 0.7}}, {"body": {"sz": 0.65, "rx": -10}}, {"body": {"sz": 1.3, "rx": 30}, "root": {"z": 0.1}},
                    {"body": {"sz": 1.1, "rx": 20}}, {"body": {"sz": 0.95}}], 5, False),
        "hurt": ([{"body": {"sz": 1.25, "rx": -14}}, {"body": {"sz": 1.1, "rx": -6}}], 2, False),
        "death": ([{"body": {"sz": 0.75, "s": 0.95}}, {"body": {"sz": 0.5, "s": 0.82}}, {"body": {"sz": 0.33, "s": 0.66}}, {"body": {"sz": 0.22, "s": 0.5}}], 4, False),
    }
    total += L.render_actions(scene, rig, poser, "Slime_", anims, os.path.join(SPRITES, "Slime"))
    hide([slime])

# ======================================================================== Schattenwolf
if "wolf" in ONLY:
    M = {"fur": L.cel("WolfFell", (0.22, 0.21, 0.3)),
         "fur_light": L.cel("WolfFellHell", (0.4, 0.38, 0.5)),
         "fur_dark": L.cel("WolfFellDunkel", (0.11, 0.1, 0.16)),
         "eye": L.cel("WolfAugen", (0.85, 0.35, 1.0), shadow=0.9, emission=1.5),
         "nose": L.cel("WolfNase", (0.05, 0.05, 0.07)),
         "claw": L.cel("WolfKralle", (0.85, 0.83, 0.78)),
         "mouth": L.cel("WolfMaul", (0.55, 0.15, 0.2))}
    bones = {"root": ((0, 0, 0), (0, 0, 0.1), None),
             "body": ((0, 0.3, 0.5), (0, -0.22, 0.52), "root"),
             "neck": ((0, -0.22, 0.55), (0, -0.36, 0.7), "body"),
             "head": ((0, -0.36, 0.7), (0, -0.66, 0.66), "neck"),
             "tail": ((0, 0.3, 0.55), (0, 0.62, 0.75), "body")}
    for nm, y in (("F", -0.16), ("B", 0.22)):
        for side, x in (("L", -0.12), ("R", 0.12)):
            bones[f"leg.{nm}{side}"] = ((x, y, 0.46), (x, y, 0.04), "body")
    rig = L.build_rig(scene, "WolfRig", bones)
    W = lambda k, b, m, loc, sc, rot=(0, 0, 0), **kw: L.shape(k, loc, sc, rot, M[m], b, collection=scene.collection, **kw)
    parts = [W("sphere", "body", "fur", (0, 0.04, 0.52), (0.19, 0.35, 0.17), segments=20),
             W("sphere", "body", "fur_light", (0, -0.19, 0.48), (0.17, 0.15, 0.18)),
             W("sphere", "neck", "fur", (0, -0.3, 0.63), (0.13, 0.13, 0.13)),
             W("sphere", "head", "fur", (0, -0.47, 0.71), (0.135, 0.15, 0.12), segments=20),
             W("sphere", "head", "fur_light", (0, -0.62, 0.66), (0.075, 0.11, 0.06)),
             W("sphere", "head", "nose", (0, -0.73, 0.68), (0.03, 0.025, 0.025)),
             W("cube", "head", "mouth", (0, -0.64, 0.61), (0.05, 0.06, 0.01)),
             W("cone", "tail", "fur_dark", (0, 0.47, 0.66), (0.07, 0.07, 0.2), rot=(-60, 0, 0)),
             W("cone", "tail", "fur_light", (0, 0.62, 0.75), (0.035, 0.035, 0.07), rot=(-60, 0, 0))]
    for s in (-1, 1):
        parts += [W("cone", "head", "fur_dark", (0.075 * s, -0.42, 0.84), (0.045, 0.035, 0.08), rot=(10, 15 * s, 0)),
                  W("sphere", "head", "eye", (0.065 * s, -0.58, 0.74), (0.028, 0.015, 0.02)),
                  W("cone", "head", "claw", (0.035 * s, -0.7, 0.62), (0.01, 0.01, 0.025), rot=(180, 0, 0))]
    for i, y in enumerate((-0.12, 0.0, 0.12, 0.24)):
        parts.append(W("cone", "body", "fur_dark", (0, y, 0.68 - 0.02 * i), (0.05, 0.05, 0.08 - 0.01 * i), rot=(-35, 0, 0)))
    for nm, y in (("F", -0.16), ("B", 0.22)):
        for side, x in (("L", -0.12), ("R", 0.12)):
            b = f"leg.{nm}{side}"
            parts += [W("cyl", b, "fur", (x, y, 0.28), (0.05, 0.05, 0.19), taper=0.75),
                      W("sphere", b, "fur_dark", (x, y - 0.025, 0.04), (0.055, 0.075, 0.035)),
                      W("cone", b, "claw", (x, y - 0.09, 0.03), (0.012, 0.012, 0.02), rot=(-90, 0, 0))]
    wolf = finish(parts, "Wolf", rig)
    rig.scale = (1.3, 1.3, 1.3)  # Wolf etwas groesser als der Spieler breit ist
    poser = L.Poser(rig)
    gait = lambda a: {"leg.FL": {"rx": a}, "leg.BR": {"rx": a}, "leg.FR": {"rx": -a}, "leg.BL": {"rx": -a}}
    anims = {
        "idle": ([{"tail": {"rz": 14}}, {"tail": {"rz": -14}, "body": {"sz": 1.02}, "neck": {"rx": 4}}], 4, True),
        "walk": ([{**gait(26), "tail": {"rz": 10}, "root": {"z": 0.0}}, {**gait(0), "root": {"z": 0.03}},
                  {**gait(-26), "tail": {"rz": -10}}, {**gait(0), "root": {"z": 0.03}}], 6, True),
        "attack": ([{"root": {"z": -0.05}, "body": {"rx": -8}, "neck": {"rx": -15}, "leg.BL": {"rx": 15}, "leg.BR": {"rx": 15}},
                    {"root": {"z": -0.08}, "body": {"rx": -12}, "neck": {"rx": -25}, "head": {"rx": -10}},
                    {"root": {"z": 0.08}, "body": {"rx": 10}, "neck": {"rx": 25}, "head": {"rx": 20}, "leg.FL": {"rx": 45}, "leg.FR": {"rx": 45}, "tail": {"rx": 20}},
                    {"body": {"rx": 6}, "neck": {"rx": 10}, "head": {"rx": 5}}], 5, False),
        "hurt": ([{"neck": {"rx": -30}, "head": {"rx": -15}, "body": {"rx": -6}, "tail": {"rx": -20}}, {"neck": {"rx": -12}, "body": {"rx": -2}}], 2, False),
        "death": ([{"neck": {"rx": -20}, "root": {"z": -0.03}},
                   {"root": {"z": -0.12, "ry": 25}, "leg.FL": {"rx": 20}, "leg.BL": {"rx": -20}},
                   {"root": {"z": -0.2, "ry": 60}, "neck": {"rx": 15}, "tail": {"rx": 20}},
                   {"root": {"z": -0.25, "ry": 85}, "neck": {"rx": 25}, "head": {"rx": 10}, "leg.FL": {"rx": 30}, "leg.FR": {"rx": 30}}], 4, False),
    }
    total += L.render_actions(scene, rig, poser, "Wolf_", anims, os.path.join(SPRITES, "Wolf"))
    hide([wolf])

# ======================================================================== Riesenspinne
if "spinne" in ONLY:
    M = {"body": L.cel("SpinneKoerper", (0.26, 0.17, 0.14)),
         "leg": L.cel("SpinneBein", (0.2, 0.13, 0.11)),
         "joint": L.cel("SpinneGelenk", (0.42, 0.28, 0.2)),
         "mark": L.cel("SpinneZeichen", (0.85, 0.2, 0.12), emission=0.3),
         "eye": L.cel("SpinneAugen", (1.0, 0.2, 0.15), shadow=0.9, emission=1.4),
         "fang": L.cel("SpinneZaehne", (0.92, 0.88, 0.75))}
    bones = {"root": ((0, 0, 0), (0, 0, 0.1), None), "body": ((0, 0.35, 0.38), (0, -0.25, 0.36), "root")}
    LEGS = []
    for i, y in enumerate((-0.14, -0.05, 0.04, 0.13)):
        for side, sg in (("L", -1), ("R", 1)):
            spread = (-0.3, -0.1, 0.1, 0.3)[i]
            base = (0.12 * sg, y, 0.36)
            knee = (0.42 * sg, y + spread * 0.9, 0.58)
            foot = (0.62 * sg, y + spread * 1.9, 0.0)
            n = f"{i}{side}"
            bones[f"legA.{n}"] = (base, knee, "body")
            bones[f"legB.{n}"] = (knee, foot, f"legA.{n}")
            LEGS.append((n, base, knee, foot))
    rig = L.build_rig(scene, "SpiderRig", bones)
    SP = lambda k, b, m, loc, sc, rot=(0, 0, 0), **kw: L.shape(k, loc, sc, rot, M[m], b, collection=scene.collection, **kw)
    parts = [SP("sphere", "body", "body", (0, -0.1, 0.37), (0.17, 0.18, 0.13), segments=20),
             SP("sphere", "body", "body", (0, 0.27, 0.42), (0.27, 0.33, 0.25), segments=24),
             SP("sphere", "body", "mark", (0, 0.2, 0.66), (0.08, 0.14, 0.02)),
             SP("sphere", "body", "mark", (0, 0.4, 0.6), (0.05, 0.06, 0.02))]
    for s in (-1, 1):
        parts += [SP("sphere", "body", "eye", (0.05 * s, -0.27, 0.43), (0.03, 0.02, 0.03)),
                  SP("sphere", "body", "eye", (0.1 * s, -0.24, 0.45), (0.02, 0.015, 0.02)),
                  SP("cone", "body", "fang", (0.04 * s, -0.27, 0.3), (0.018, 0.018, 0.06), rot=(180, 0, 0))]
    for n, base, knee, foot in LEGS:
        parts += [seg(base, knee, 0.03, M["leg"], f"legA.{n}"),
                  L.shape("sphere", knee, (0.04, 0.04, 0.04), (0, 0, 0), M["joint"], f"legB.{n}", collection=scene.collection),
                  seg(knee, foot, 0.025, M["leg"], f"legB.{n}", taper=0.5)]
    spider = finish(parts, "Spider", rig, outline=0.012)
    poser = L.Poser(rig)

    def legs(a, b):
        """Gruppe A (0L,1R,2L,3R) und Gruppe B (gegenueber) heben sich abwechselnd"""
        p = {}
        for n, *_ in LEGS:
            grp = (int(n[0]) + (0 if n[1] == "L" else 1)) % 2
            p[f"legA.{n}"] = {"rx": a if grp == 0 else b}
        return p
    anims = {
        "idle": ([{**legs(0, 0)}, {**legs(4, -4), "root": {"z": 0.015}}], 4, True),
        "walk": ([{**legs(18, -6), "root": {"z": 0.02}}, {**legs(0, 0)}, {**legs(-6, 18), "root": {"z": 0.02}}, {**legs(0, 0)}], 6, True),
        "attack": ([{"body": {"rx": -10}, "legA.0L": {"rx": -40}, "legA.0R": {"rx": -40}},
                    {"body": {"rx": -22}, "root": {"z": 0.06}, "legA.0L": {"rx": -70}, "legA.0R": {"rx": -70}},
                    {"body": {"rx": 15}, "root": {"z": -0.02}, "legA.0L": {"rx": 30}, "legA.0R": {"rx": 30}},
                    {"body": {"rx": 4}}], 5, False),
        "hurt": ([{"body": {"rx": -12}, "root": {"z": 0.04}, **legs(-10, -10)}, {"body": {"rx": -4}}], 2, False),
        "death": ([{"body": {"rx": -10}, **legs(-15, -15)}, {"root": {"z": -0.1}, **legs(-35, -35)},
                   {"root": {"z": -0.2}, **legs(-60, -60)}, {"root": {"z": -0.26}, **legs(-80, -80)}], 4, False),
    }
    total += L.render_actions(scene, rig, poser, "Spider_", anims, os.path.join(SPRITES, "Spider"))
    hide([spider])

scene["foot_pivot"] = L.foot_pivot(scene)
bpy.ops.wm.save_as_mainfile(filepath=BLEND_PATH)
print(f"RESULT rendered={total} pivot={scene['foot_pivot'][0]},{scene['foot_pivot'][1]}")
