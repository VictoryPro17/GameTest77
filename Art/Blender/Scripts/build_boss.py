"""RE:BORN - Boss Waldwaechter (Meilenstein 5, Comic-Look)

Baum-Golem mit Rinde, Moos, leuchtenden Augen, Wurzel-Haenden und gruenem Kern.
Rendert idle 4, walk 6, attack 6 (Schlag), rootattack 6 (Wurzelangriff), roar 4 (Bruellen, Phase 2),
hurt 2, death 6 in 4 Richtungen als 256x256-Sprites (gleicher Massstab wie der Spieler: 4,2 m Bildhoehe).
Eigene Datei: Art/Blender/Reborn_Boss.blend

Aufruf:  blender.exe -b --factory-startup --python build_boss.py -- <projektordner>
"""
import bpy
import math
import os
import sys

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import reborn_lib as L  # noqa: E402

argv = sys.argv[sys.argv.index("--") + 1:] if "--" in sys.argv else []
PROJECT = argv[0] if argv else os.path.abspath(os.path.join(os.path.dirname(__file__), "..", "..", ".."))
OUT = os.path.join(PROJECT, "Assets", "_Project", "Sprites", "Boss")
BLEND_PATH = os.path.join(PROJECT, "Art", "Blender", "Reborn_Boss.blend")

bpy.ops.wm.read_factory_settings(use_empty=True)
scene = bpy.context.scene
scene.name = "Boss"
L.setup_scene(scene, 256)
# Gleicher Massstab und Fusspunkt wie der Spieler: doppelte Bildgroesse, doppelte Kamerahoehe
L.camera(scene, target=(0, 0, 1.7), ortho=4.2)
L.sun(scene, energy=3.4)

M = {"bark": L.cel("Rinde", (0.42, 0.28, 0.17)),
     "bark_dark": L.cel("RindeDunkel", (0.24, 0.15, 0.09)),
     "bark_light": L.cel("RindeHell", (0.58, 0.42, 0.26)),
     "moss": L.cel("Moos", (0.32, 0.55, 0.2)),
     "moss_light": L.cel("MoosHell", (0.5, 0.75, 0.3)),
     "leaf": L.cel("Laub", (0.22, 0.5, 0.18)),
     "eye": L.cel("WaechterAugen", (0.55, 1.0, 0.35), shadow=0.9, emission=0.5),
     "core": L.cel("WaechterKern", (0.3, 0.9, 0.4), shadow=0.85, emission=0.4),
     "stone": L.cel("Stein", (0.5, 0.5, 0.52))}

bones = {"root": ((0, 0, 0), (0, 0, 0.3), None),
         "hips": ((0, 0, 1.0), (0, 0, 1.5), "root"),
         "spine": ((0, 0, 1.5), (0, 0, 2.3), "hips"),
         "head": ((0, 0, 2.3), (0, 0, 2.95), "spine")}
for s, sg in (("L", -1), ("R", 1)):
    bones[f"upperarm.{s}"] = ((0.62 * sg, 0, 2.15), (0.78 * sg, 0, 1.45), "spine")
    bones[f"forearm.{s}"] = ((0.78 * sg, 0, 1.45), (0.84 * sg, 0, 0.75), f"upperarm.{s}")
    bones[f"thigh.{s}"] = ((0.3 * sg, 0, 1.0), (0.32 * sg, 0, 0.52), "hips")
    bones[f"shin.{s}"] = ((0.32 * sg, 0, 0.52), (0.33 * sg, 0, 0.05), f"thigh.{s}")
rig = L.build_rig(scene, "BossRig", bones)
B = lambda k, b, m, loc, sc, rot=(0, 0, 0), **kw: L.shape(k, loc, sc, rot, M[m], b, collection=scene.collection, **kw)

parts = [
    # Rumpf: Stamm, Brust, Rindenfurchen, gruener Kern, Moos
    B("cyl", "hips", "bark", (0, 0, 1.25), (0.48, 0.4, 0.28), taper=1.08, segments=20),
    B("cyl", "spine", "bark", (0, 0, 1.9), (0.55, 0.45, 0.42), taper=1.12, segments=20),
    B("sphere", "spine", "bark", (0, 0, 2.25), (0.66, 0.5, 0.2)),
    B("sphere", "spine", "core", (0, -0.42, 1.9), (0.14, 0.06, 0.16)),
    B("torus", "spine", "bark_dark", (0, -0.42, 1.9), (0.17, 0.1, 0.19), rot=(90, 0, 0)),
    B("sphere", "spine", "moss", (0.25, -0.3, 2.3), (0.25, 0.2, 0.08)),
    B("sphere", "spine", "moss_light", (-0.35, -0.25, 2.28), (0.18, 0.15, 0.07)),
    B("sphere", "hips", "moss", (-0.2, -0.36, 1.1), (0.15, 0.08, 0.1)),
    # Kopf: Block mit Augen, Stirn, Kinnbart aus Wurzeln, Krone aus Laub und Aesten
    B("sphere", "head", "bark", (0, -0.05, 2.62), (0.34, 0.32, 0.3), segments=20),
    B("cube", "head", "bark_dark", (0, -0.3, 2.72), (0.28, 0.06, 0.05)),
    B("sphere", "head", "leaf", (0, 0.05, 2.92), (0.42, 0.38, 0.18)),
    B("sphere", "head", "moss_light", (0.18, -0.05, 3.0), (0.2, 0.18, 0.1)),
    B("sphere", "head", "leaf", (-0.22, 0.1, 2.98), (0.22, 0.2, 0.12)),
]
for sg in (-1, 1):
    parts += [B("sphere", "head", "eye", (0.13 * sg, -0.32, 2.62), (0.06, 0.03, 0.045)),
              B("cone", "head", "bark_dark", (0.25 * sg, 0.0, 3.05), (0.05, 0.05, 0.28), rot=(-15, 30 * sg, 0)),
              B("cone", "head", "bark_dark", (0.4 * sg, 0.0, 3.0), (0.035, 0.035, 0.16), rot=(0, 70 * sg, 0)),
              B("cone", "head", "bark_light", (0.08 * sg, -0.28, 2.38), (0.04, 0.04, 0.12), rot=(180, 0, 10 * sg))]
for i, (x, z) in enumerate(((-0.3, 1.7), (0.25, 1.6), (-0.15, 2.05), (0.35, 2.0), (0.0, 1.45))):
    parts.append(B("cube", "spine" if z > 1.5 else "hips", "bark_dark", (x, -0.43, z), (0.025, 0.02, 0.18), rot=(0, 0, 8 * (i % 3 - 1)), smooth=False))
# Arme: Schulter aus Stein, Ober- und Unterarm aus Holz, Hand mit Wurzel-Fingern
for s, sg in (("L", -1), ("R", 1)):
    parts += [B("sphere", f"upperarm.{s}", "stone", (0.62 * sg, 0, 2.2), (0.24, 0.24, 0.2)),
              B("sphere", f"upperarm.{s}", "moss", (0.62 * sg, -0.05, 2.38), (0.18, 0.16, 0.06)),
              B("cyl", f"upperarm.{s}", "bark", (0.7 * sg, 0, 1.8), (0.16, 0.16, 0.36), taper=0.85),
              B("sphere", f"forearm.{s}", "bark_light", (0.78 * sg, 0, 1.45), (0.15, 0.15, 0.14)),
              B("cyl", f"forearm.{s}", "bark", (0.81 * sg, 0, 1.1), (0.15, 0.15, 0.33), taper=1.15),
              B("sphere", f"forearm.{s}", "bark_dark", (0.84 * sg, 0, 0.72), (0.2, 0.18, 0.14))]
    for f in range(4):
        a = math.radians(-45 + 30 * f)
        parts.append(B("cone", f"forearm.{s}", "bark_dark", (0.84 * sg + 0.12 * math.sin(a), -0.08 * math.cos(a), 0.55),
                       (0.04, 0.04, 0.15), rot=(180 - 15 * math.cos(a), 20 * math.sin(a) * sg, 0)))
    # Beine: Stumpf mit Wurzel-Fuessen
    parts += [B("cyl", f"thigh.{s}", "bark", (0.31 * sg, 0, 0.77), (0.19, 0.19, 0.26), taper=0.9),
              B("cyl", f"shin.{s}", "bark", (0.33 * sg, 0, 0.3), (0.2, 0.2, 0.26), taper=1.2),
              B("sphere", f"shin.{s}", "moss", (0.33 * sg, -0.12, 0.5), (0.12, 0.08, 0.07))]
    for f in range(3):
        a = math.radians(-50 + 50 * f)
        parts.append(B("cone", f"shin.{s}", "bark_dark", (0.33 * sg + 0.22 * math.sin(a), -0.22 * math.cos(a), 0.05),
                       (0.06, 0.06, 0.18), rot=(-80 * math.cos(a), 80 * math.sin(a), 0)))

boss = L.join(parts, "Waldwaechter")
L.attach_to_rig(boss, rig)
L.subdivide(boss, 1)
L.add_outline(boss, 0.03)
poser = L.Poser(rig)

arms = lambda l, r, fl=0, fr=0: {"upperarm.L": {"rx": l}, "upperarm.R": {"rx": r}, "forearm.L": {"rx": fl}, "forearm.R": {"rx": fr}}
anims = {
    "idle": ([{**arms(0, 0, 8, 8)}, {**arms(4, 4, 14, 14), "spine": {"rx": 3}, "root": {"z": -0.03}, "head": {"rx": -3}}], 4, True),
    "walk": ([{"thigh.L": {"rx": 22}, "thigh.R": {"rx": -18}, "shin.R": {"rx": -25}, **arms(-16, 16, 10, 20), "spine": {"rz": 5}},
              {"thigh.L": {"rx": 2}, "thigh.R": {"rx": 8}, "shin.R": {"rx": -40}, "root": {"z": 0.06}},
              {"thigh.R": {"rx": 22}, "thigh.L": {"rx": -18}, "shin.L": {"rx": -25}, **arms(16, -16, 20, 10), "spine": {"rz": -5}},
              {"thigh.R": {"rx": 2}, "thigh.L": {"rx": 8}, "shin.L": {"rx": -40}, "root": {"z": 0.06}}], 6, True),
    "attack": ([{**arms(0, -60, 0, 30), "spine": {"rz": 18, "rx": -6}},
                {**arms(0, -110, 0, 40), "spine": {"rz": 26, "rx": -10}, "head": {"rx": -10}},
                {**arms(10, 70, 0, 10), "spine": {"rz": -10, "rx": 18}, "thigh.L": {"rx": 15}},
                {**arms(10, 95, 0, 0), "spine": {"rz": -18, "rx": 24}, "root": {"z": -0.08}},
                {**arms(0, 50, 0, 15), "spine": {"rx": 8}}], 6, False),
    "rootattack": ([{**arms(-70, -70, 30, 30), "spine": {"rx": -12}, "head": {"rx": -12}},
                    {**arms(-130, -130, 20, 20), "spine": {"rx": -20}, "head": {"rx": -18}, "root": {"z": 0.08}},
                    {**arms(60, 60, 0, 0), "spine": {"rx": 30}, "head": {"rx": 10}, "root": {"z": -0.15}, "thigh.L": {"rx": 25}, "thigh.R": {"rx": 25}, "shin.L": {"rx": -40}, "shin.R": {"rx": -40}},
                    {**arms(80, 80, 0, 0), "spine": {"rx": 35}, "root": {"z": -0.2}, "thigh.L": {"rx": 30}, "thigh.R": {"rx": 30}, "shin.L": {"rx": -50}, "shin.R": {"rx": -50}},
                    {**arms(30, 30, 10, 10), "spine": {"rx": 10}}], 6, False),
    "roar": ([{"head": {"rx": -10}, **arms(-20, -20, 10, 10)},
              {"head": {"rx": -35}, "spine": {"rx": -18}, "upperarm.L": {"rx": -40, "rz": 50}, "upperarm.R": {"rx": -40, "rz": -50}, "root": {"z": 0.05}},
              {"head": {"rx": -32}, "spine": {"rx": -16}, "upperarm.L": {"rx": -35, "rz": 55}, "upperarm.R": {"rx": -35, "rz": -55}, "root": {"z": 0.04}},
              {"head": {"rx": -8}, **arms(-5, -5, 10, 10)}], 4, False),
    "hurt": ([{"spine": {"rx": -14}, "head": {"rx": -18}, **arms(-20, -20, 20, 20)}, {"spine": {"rx": -6}, "head": {"rx": -6}}], 2, False),
    "death": ([{"spine": {"rx": -10}, "head": {"rx": -20}, **arms(-30, -30)},
               {"root": {"z": -0.2}, "thigh.L": {"rx": 40}, "thigh.R": {"rx": 40}, "shin.L": {"rx": -70}, "shin.R": {"rx": -70}, "spine": {"rx": 10}, **arms(20, 20, 20, 20)},
               {"root": {"z": -0.45}, "thigh.L": {"rx": 80}, "thigh.R": {"rx": 80}, "shin.L": {"rx": -110}, "shin.R": {"rx": -110}, "spine": {"rx": 25}, "head": {"rx": 15}, **arms(50, 50, 30, 30)},
               {"root": {"z": -0.35, "rx": -35}, "thigh.L": {"rx": 60}, "thigh.R": {"rx": 60}, "shin.L": {"rx": -80}, "shin.R": {"rx": -80}, "spine": {"rx": -10}, "head": {"rx": -10}, **arms(-30, -30, 10, 10)},
               {"root": {"z": -0.1, "rx": -82}, "thigh.L": {"rx": 15}, "thigh.R": {"rx": 10}, "shin.L": {"rx": -15}, "shin.R": {"rx": -10}, "spine": {"rx": 0}, "head": {"rx": 5}, "upperarm.L": {"rx": -20, "rz": 40}, "upperarm.R": {"rx": -20, "rz": -40}}], 6, False),
}
total = L.render_actions(scene, rig, poser, "Boss_", anims, OUT)
scene["foot_pivot"] = L.foot_pivot(scene)
bpy.ops.wm.save_as_mainfile(filepath=BLEND_PATH)
print(f"RESULT rendered={total} pivot={scene['foot_pivot'][0]},{scene['foot_pivot'][1]}")
