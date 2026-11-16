"""RE:BORN - Hauptcharakter (v3, Meilenstein 5, Comic-Look)

Baut die Grundfigur (ohne Waffe) mit Skelett (Ellbogen, Knie, Umhang), legt 7 Animationen
aus Schluesselposen an und rendert sie in 4 Richtungen als 128x128-Sprites mit weichen Zwischenbildern:
  idle 4, walk 8, attack 5, dodge 4, cast 5, hurt 2, death 4
Die Ausruestung baut build_equipment.py am selben Skelett.

Aufruf (im Hintergrund):
  blender.exe -b --factory-startup --python build_player.py -- <projektordner>
"""
import bpy
import math
import os
import sys

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import reborn_lib as L  # noqa: E402

argv = sys.argv[sys.argv.index("--") + 1:] if "--" in sys.argv else []
PROJECT = argv[0] if argv else os.path.abspath(os.path.join(os.path.dirname(__file__), "..", "..", ".."))
OUT_DIR = os.path.join(PROJECT, "Assets", "_Project", "Sprites", "Player")
BLEND_PATH = os.path.join(PROJECT, "Art", "Blender", "Reborn_Characters.blend")
os.makedirs(OUT_DIR, exist_ok=True)
for f in os.listdir(OUT_DIR):
    if f.endswith(".png"):
        os.remove(os.path.join(OUT_DIR, f))

bpy.ops.wm.read_factory_settings(use_empty=True)
scene = bpy.context.scene
scene.name = "Characters"
L.setup_scene(scene)
L.camera(scene, target=(0, 0, 0.85), ortho=2.1)
L.sun(scene)

M = {
    "skin": L.cel("Haut", (0.96, 0.76, 0.62)),
    "skin_dark": L.cel("HautSchatten", (0.84, 0.6, 0.48)),
    "blush": L.cel("Wangen", (0.95, 0.55, 0.5)),
    "hair": L.cel("Haare", (0.36, 0.2, 0.11)),
    "hair_light": L.cel("HaareHell", (0.55, 0.33, 0.16)),
    "tunic": L.cel("Tunika", (0.2, 0.4, 0.72)),
    "tunic_dark": L.cel("TunikaDunkel", (0.12, 0.25, 0.5)),
    "trim": L.cel("Borte", (0.93, 0.78, 0.36)),
    "pants": L.cel("Hose", (0.32, 0.26, 0.2)),
    "shoes": L.cel("Schuhe", (0.24, 0.15, 0.09)),
    "belt": L.cel("Guertel", (0.5, 0.32, 0.13)),
    "gold": L.cel("Gold", (1.0, 0.8, 0.3), light=1.15),
    "cloak": L.cel("Umhang", (0.62, 0.14, 0.15)),
    "cloak_in": L.cel("UmhangInnen", (0.35, 0.08, 0.1)),
    "eye_white": L.cel("Augenweiss", (0.98, 0.98, 0.96), shadow=0.9, mid=0.95),
    "iris": L.cel("Iris", (0.15, 0.45, 0.85), emission=0.2),
    "pupil": L.cel("Pupille", (0.03, 0.04, 0.08)),
    "shine": L.cel("Glanz", (1, 1, 1), shadow=1, mid=1, emission=0.5),
    "mouth": L.cel("Mund", (0.55, 0.2, 0.2)),
}

rig = L.build_rig(scene, "PlayerRig", L.PLAYER_BONES)
P = []


def add(kind, bone, mat, loc, scale, rot=(0, 0, 0), **kw):
    P.append(L.shape(kind, loc, scale, rot, M[mat], bone, collection=scene.collection, **kw))


# ---- Beine: Oberschenkel, Knie, Unterschenkel, Schuh mit Spitze
for side, x in (("L", -0.12), ("R", 0.12)):
    add("cyl", f"thigh.{side}", "pants", (x, 0, 0.66), (0.095, 0.095, 0.19), taper=0.85)
    add("sphere", f"shin.{side}", "pants", (x, 0, 0.47), (0.085, 0.085, 0.07))
    add("cyl", f"shin.{side}", "pants", (x, 0, 0.29), (0.08, 0.08, 0.18), taper=0.82)
    add("sphere", f"shin.{side}", "shoes", (x, -0.03, 0.07), (0.085, 0.13, 0.065))
    add("cube", f"shin.{side}", "shoes", (x, -0.01, 0.02), (0.085, 0.13, 0.025))
# ---- Rumpf: Huefte, Tunika (unten weiter), Brust, Kragen, Borte, Guertel, Schnalle, Tasche
add("cyl", "hips", "pants", (0, 0, 0.88), (0.235, 0.17, 0.08))
add("cyl", "hips", "tunic", (0, 0, 0.92), (0.26, 0.19, 0.08), taper=0.95)
add("cyl", "spine", "tunic", (0, 0, 1.12), (0.215, 0.155, 0.2), taper=1.15)
add("sphere", "spine", "tunic", (0, 0, 1.26), (0.25, 0.17, 0.11))
add("cyl", "spine", "trim", (0, 0, 1.34), (0.12, 0.1, 0.025))
add("cube", "spine", "tunic_dark", (0, -0.16, 1.15), (0.06, 0.012, 0.16), smooth=False)
add("cube", "spine", "trim", (0, -0.168, 1.15), (0.012, 0.006, 0.16), smooth=False)
add("cyl", "hips", "belt", (0, 0, 0.995), (0.24, 0.175, 0.035))
add("cube", "hips", "gold", (0, -0.185, 0.995), (0.045, 0.015, 0.035))
add("cube", "hips", "belt", (0.18, -0.12, 0.93), (0.05, 0.035, 0.06))
# ---- Umhang (eigener Knochen, schwingt mit), Schulterspange
add("cube", "cloak", "cloak", (0, 0.19, 1.06), (0.26, 0.025, 0.3), taper=1.0)
add("cube", "cloak", "cloak_in", (0, 0.165, 1.06), (0.24, 0.006, 0.28))
add("cube", "cloak", "cloak", (0, 0.205, 0.76), (0.28, 0.022, 0.035))
for x in (-0.15, 0.15):
    add("sphere", "spine", "gold", (x, -0.12, 1.3), (0.035, 0.02, 0.035))
# ---- Hals, Kopf, Ohren
add("cyl", "head", "skin_dark", (0, 0, 1.37), (0.07, 0.07, 0.05))
add("sphere", "head", "skin", (0, 0, 1.57), (0.235, 0.225, 0.235), segments=24)
add("sphere", "head", "skin", (0, -0.08, 1.48), (0.17, 0.14, 0.11))
for x in (-0.23, 0.23):
    add("sphere", "head", "skin_dark", (x, 0.01, 1.56), (0.03, 0.045, 0.06))
# ---- Gesicht: grosse Augen mit Iris, Pupille, Glanzpunkt; Brauen, Nase, Mund, Wangen
for s in (-1, 1):
    x = 0.088 * s
    add("sphere", "head", "eye_white", (x, -0.205, 1.56), (0.055, 0.022, 0.065))
    add("sphere", "head", "iris", (x, -0.222, 1.552), (0.038, 0.012, 0.048))
    add("sphere", "head", "pupil", (x, -0.229, 1.55), (0.02, 0.008, 0.026))
    add("sphere", "head", "shine", (x - 0.015 * s, -0.233, 1.572), (0.011, 0.005, 0.011))
    add("cube", "head", "hair", (x, -0.212, 1.645), (0.05, 0.012, 0.012), rot=(0, 0, 12 * -s))
    add("sphere", "head", "blush", (0.135 * s, -0.19, 1.49), (0.035, 0.01, 0.02))
add("sphere", "head", "skin_dark", (0, -0.232, 1.51), (0.02, 0.018, 0.024))
add("cube", "head", "mouth", (0, -0.22, 1.455), (0.032, 0.006, 0.008), smooth=False)
# ---- Haare: Haube, Hinterkopf, Straehnen ueber der Stirn, Seiten, Wirbel
add("sphere", "head", "hair", (0, 0.03, 1.69), (0.255, 0.245, 0.16), segments=24)
add("sphere", "head", "hair", (0, 0.13, 1.57), (0.23, 0.13, 0.21))
for x, z, s, rz in ((-0.14, 1.69, 0.085, 25), (-0.05, 1.71, 0.095, 8), (0.05, 1.71, 0.09, -8), (0.14, 1.68, 0.08, -25)):
    add("cone", "head", "hair_light" if abs(x) < 0.1 else "hair", (x, -0.18, z - 0.03), (s, 0.06, 0.09), rot=(-20, 0, rz))
for s in (-1, 1):
    add("cone", "head", "hair", (0.21 * s, -0.05, 1.56), (0.055, 0.06, 0.12), rot=(0, 0, 0))
add("cone", "head", "hair_light", (0.03, 0.05, 1.86), (0.05, 0.05, 0.08), rot=(-25, 15, 0))
# ---- Arme: Aermel mit Borte, Unterarm, Hand mit Daumen und Fingern
for side, x in (("L", -0.31), ("R", 0.31)):
    s = -1 if side == "L" else 1
    add("sphere", f"upperarm.{side}", "tunic", (x, 0, 1.27), (0.085, 0.085, 0.07))
    add("cyl", f"upperarm.{side}", "tunic", (x, 0, 1.19), (0.075, 0.075, 0.09), taper=1.1)
    add("cyl", f"upperarm.{side}", "trim", (x, 0, 1.11), (0.083, 0.083, 0.015))
    add("cyl", f"forearm.{side}", "skin", (x + 0.005 * s, 0, 1.01), (0.058, 0.058, 0.09), taper=0.9)
    add("sphere", f"forearm.{side}", "skin", (x + 0.01 * s, -0.005, 0.89), (0.064, 0.058, 0.07))
    add("sphere", f"forearm.{side}", "skin_dark", (x - 0.045 * s, -0.045, 0.905), (0.025, 0.025, 0.035))
    add("cube", f"forearm.{side}", "skin", (x + 0.01 * s, -0.03, 0.835), (0.05, 0.035, 0.03))

body = L.join(P, "Player")
L.attach_to_rig(body, rig)
L.subdivide(body, 1)
L.add_outline(body, 0.016)

# ---------------------------------------------------------------- Animationen
poser = L.Poser(rig)
A = {
    "idle": ([{"cloak": {"rx": -4}},
              {"root": {"z": -0.018}, "spine": {"rx": 2}, "upperarm.L": {"rz": -3}, "upperarm.R": {"rz": 3}, "forearm.L": {"rx": 6}, "forearm.R": {"rx": 6}, "cloak": {"rx": -8}}],
             4, True),
    "walk": ([{"thigh.L": {"rx": 28}, "shin.L": {"rx": -6}, "thigh.R": {"rx": -22}, "shin.R": {"rx": -28},
               "upperarm.L": {"rx": -24}, "upperarm.R": {"rx": 24}, "forearm.L": {"rx": 10}, "forearm.R": {"rx": 22},
               "spine": {"rx": 5, "rz": 4}, "cloak": {"rx": -12}, "root": {"z": -0.01}},
              {"thigh.L": {"rx": 2}, "shin.L": {"rx": -8}, "thigh.R": {"rx": 12}, "shin.R": {"rx": -55},
               "spine": {"rx": 4}, "cloak": {"rx": -20}, "root": {"z": 0.035}, "forearm.L": {"rx": 12}, "forearm.R": {"rx": 12}},
              {"thigh.R": {"rx": 28}, "shin.R": {"rx": -6}, "thigh.L": {"rx": -22}, "shin.L": {"rx": -28},
               "upperarm.R": {"rx": -24}, "upperarm.L": {"rx": 24}, "forearm.R": {"rx": 10}, "forearm.L": {"rx": 22},
               "spine": {"rx": 5, "rz": -4}, "cloak": {"rx": -12}, "root": {"z": -0.01}},
              {"thigh.R": {"rx": 2}, "shin.R": {"rx": -8}, "thigh.L": {"rx": 12}, "shin.L": {"rx": -55},
               "spine": {"rx": 4}, "cloak": {"rx": -20}, "root": {"z": 0.035}, "forearm.L": {"rx": 12}, "forearm.R": {"rx": 12}}],
             8, True),
    "attack": ([{"upperarm.R": {"rx": -70, "rz": 25}, "forearm.R": {"rx": 40}, "spine": {"rz": 22, "rx": -4}, "upperarm.L": {"rx": 20}, "cloak": {"rx": -6}},
                {"upperarm.R": {"rx": 40, "rz": 10}, "forearm.R": {"rx": 15}, "spine": {"rz": 0, "rx": 8}, "thigh.L": {"rx": 10}, "cloak": {"rx": -18}},
                {"upperarm.R": {"rx": 100, "rz": -5}, "forearm.R": {"rx": 0}, "spine": {"rx": 14, "rz": -22}, "upperarm.L": {"rx": -20},
                 "thigh.L": {"rx": 20}, "shin.L": {"rx": -10}, "thigh.R": {"rx": -12}, "cloak": {"rx": -26, "rz": 10}},
                {"upperarm.R": {"rx": 60}, "forearm.R": {"rx": 20}, "spine": {"rx": 6, "rz": -8}, "cloak": {"rx": -12}}],
               5, False),
    "dodge": ([{"root": {"z": -0.1}, "spine": {"rx": 22}, "thigh.L": {"rx": 30}, "shin.L": {"rx": -30}, "thigh.R": {"rx": -10}, "shin.R": {"rx": -30},
                "upperarm.L": {"rx": -35}, "upperarm.R": {"rx": -35}, "cloak": {"rx": -10}},
               {"root": {"z": -0.28}, "hips": {"rx": 10}, "spine": {"rx": 34}, "head": {"rx": 18}, "thigh.L": {"rx": 60}, "shin.L": {"rx": -80},
                "thigh.R": {"rx": 40}, "shin.R": {"rx": -70}, "upperarm.L": {"rx": 55}, "upperarm.R": {"rx": 55}, "forearm.L": {"rx": 30}, "forearm.R": {"rx": 30}, "cloak": {"rx": 15}},
               {"root": {"z": -0.06}, "spine": {"rx": 12}, "thigh.L": {"rx": -10}, "thigh.R": {"rx": 18}, "shin.R": {"rx": -20}, "cloak": {"rx": -6}}],
              4, False),
    "cast": ([{"spine": {"rx": -8}, "head": {"rx": -6}, "upperarm.L": {"rx": 40, "rz": 22}, "upperarm.R": {"rx": 40, "rz": -22}, "forearm.L": {"rx": 50}, "forearm.R": {"rx": 50}, "cloak": {"rx": -4}},
              {"spine": {"rx": 10}, "upperarm.L": {"rx": 90, "rz": 8}, "upperarm.R": {"rx": 90, "rz": -8}, "forearm.L": {"rx": 0}, "forearm.R": {"rx": 0},
               "thigh.L": {"rx": 12}, "thigh.R": {"rx": -8}, "cloak": {"rx": -28}},
              {"spine": {"rx": 6}, "upperarm.L": {"rx": 82, "rz": 5}, "upperarm.R": {"rx": 82, "rz": -5}, "forearm.L": {"rx": 5}, "forearm.R": {"rx": 5}, "cloak": {"rx": -16}}],
             5, False),
    "hurt": ([{"spine": {"rx": -16}, "head": {"rx": -18}, "upperarm.L": {"rx": -20, "rz": 30}, "upperarm.R": {"rx": -20, "rz": -30}, "root": {"z": -0.02}, "cloak": {"rx": 10}},
              {"spine": {"rx": -8}, "head": {"rx": -8}, "upperarm.L": {"rx": -8, "rz": 14}, "upperarm.R": {"rx": -8, "rz": -14}, "cloak": {"rx": 4}}],
             2, False),
}


def death_keys(sideways):
    """Tod: in die Knie, dann umfallen - immer quer zum Bildschirm und mittig ueber dem Fusspunkt,
    damit die liegende Figur (1,8 m) ganz im Bild bleibt (2,1 m breit).
    sideways=True (Blick unten/oben): zur Seite kippen (rz), Wurzel zur Gegenseite schieben (x).
    sideways=False (Blick links/rechts): nach hinten kippen (rx), Wurzel nach vorne schieben (y)."""
    def fall(deg, shift, z):
        return {"z": z, "rz": deg, "x": shift} if sideways else {"z": z, "rx": -deg, "y": shift}
    return [{"spine": {"rx": -14}, "head": {"rx": -16}, "upperarm.L": {"rz": 30}, "upperarm.R": {"rz": -30}},
            {"root": {"z": -0.12}, "thigh.L": {"rx": 40}, "shin.L": {"rx": -70}, "thigh.R": {"rx": 35}, "shin.R": {"rx": -65},
             "spine": {"rx": -10}, "head": {"rx": -15}, "upperarm.L": {"rx": -20, "rz": 25}, "upperarm.R": {"rx": -20, "rz": -25}},
            {"root": fall(45, 0.45, -0.1), "thigh.L": {"rx": 30}, "shin.L": {"rx": -40}, "thigh.R": {"rx": 25}, "shin.R": {"rx": -35},
             "spine": {"rx": -5}, "upperarm.L": {"rx": -30, "rz": 35}, "upperarm.R": {"rx": -30, "rz": -35}},
            {"root": fall(84, 0.88, 0.06), "thigh.L": {"rx": 14}, "shin.L": {"rx": -14}, "thigh.R": {"rx": 8}, "shin.R": {"rx": -8},
             "head": {"rx": 16}, "upperarm.L": {"rx": -10, "rz": 28}, "upperarm.R": {"rx": -10, "rz": -28},
             "forearm.L": {"rx": 25}, "forearm.R": {"rx": 25}}]


# Animationen mit eigener Fassung pro Richtung (Aktion "Player_<anim>_<richtung>"); build_equipment.py liest diese Liste
DIR_ANIMS = {"death": ({d: death_keys(d in ("down", "up")) for d in L.DIRECTIONS}, 4, False)}

frames_info = {}
rendered = 0
for anim, (keys, count, loop) in A.items():
    act, end = L.make_action(rig, poser, "Player_" + anim, keys, loop)
    frames = L.sample_frames(end, count, loop)
    frames_info[anim] = {"frames": frames, "loop": loop}
    for direction, angle in L.DIRECTIONS.items():
        rig.rotation_euler = (0, 0, math.radians(angle))
        for i, f in enumerate(frames):
            L.set_frame(scene, f)
            L.render_still(scene, os.path.join(OUT_DIR, f"{anim}_{direction}_{i}.png"))
            rendered += 1

edge_warnings = 0
for anim, (per_dir, count, loop) in DIR_ANIMS.items():
    for direction, angle in L.DIRECTIONS.items():
        act, end = L.make_action(rig, poser, f"Player_{anim}_{direction}", per_dir[direction], loop)
        frames = L.sample_frames(end, count, loop)
        frames_info[anim] = {"frames": frames, "loop": loop}
        rig.rotation_euler = (0, 0, math.radians(angle))
        for i, f in enumerate(frames):
            L.set_frame(scene, f)
            path = os.path.join(OUT_DIR, f"{anim}_{direction}_{i}.png")
            L.render_still(scene, path)
            rendered += 1
            edge = L.edge_pixels(path)
            if edge:
                edge_warnings += 1
                print(f"WARNUNG Randpixel {edge}: {os.path.basename(path)}")

rig.rotation_euler = (0, 0, 0)
L.use_action(rig, bpy.data.actions["Player_idle"])
scene.frame_set(1)
# Fuer build_equipment.py: Animationen mit genau denselben Bildpositionen
scene["anim_frames"] = {k: v["frames"] for k, v in frames_info.items()}
scene["dir_actions"] = list(DIR_ANIMS.keys())
scene["foot_pivot"] = L.foot_pivot(scene)
bpy.ops.wm.save_as_mainfile(filepath=BLEND_PATH)
print(f"RESULT pivot={scene['foot_pivot'][0]},{scene['foot_pivot'][1]} rendered={rendered} randwarnungen={edge_warnings} "
      + " ".join(f"{k}={len(v['frames'])}" for k, v in frames_info.items()))
