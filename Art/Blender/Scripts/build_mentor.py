"""RE:BORN - Mentor Aldric (Meilenstein 10a, Comic-Look)

Aelterer, weiser Abenteurer-Mentor auf Basis der Spielfigur (gleiches Skelett, Animationen "idle" und "walk"):
grauer Bart und Haar, tiefblauer Reiseumhang mit Goldborte, braune Stiefel, Wanderstab mit leuchtendem blauem Kristall.
Rendert idle (4 Bilder) und walk (8 Bilder) in 4 Richtungen, 128 px, nach Sprites/Menu/Mentor/<anim>_<richtung>_<i>.png
(Aufbau wie Sprites/Town/Villager_*) und ein Portraet (256x256, Kopf und Schultern, von vorn) nach Sprites/Menu/mentor_portrait.png.
Die Datei Reborn_Characters.blend wird NICHT gespeichert.

Aufruf:  blender.exe -b Art/Blender/Reborn_Characters.blend --python build_mentor.py -- <projektordner>
"""
import bpy
import math
import os
import sys

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import reborn_lib as L  # noqa: E402

argv = sys.argv[sys.argv.index("--") + 1:] if "--" in sys.argv else []
PROJECT = argv[0] if argv else os.path.abspath(os.path.join(os.path.dirname(__file__), "..", "..", ".."))
MENU = os.path.join(PROJECT, "Assets", "_Project", "Sprites", "Menu")
OUT = os.path.join(MENU, "Mentor")
os.makedirs(OUT, exist_ok=True)

scene = bpy.data.scenes["Characters"]
rig = bpy.data.objects["PlayerRig"]
body = bpy.data.objects["Player"]
ANIM_FRAMES = scene["anim_frames"]
for o in scene.objects:
    if o.name.startswith(("EQ_", "ICON_")) or o.name == "IconCamera":
        o.hide_render = True
body.hide_render = False
body.is_holdout = False

COLORS = {"Tunika": (0.14, 0.22, 0.52), "TunikaDunkel": (0.08, 0.12, 0.34), "Umhang": (0.1, 0.18, 0.55),
          "UmhangInnen": (0.06, 0.09, 0.3), "Haare": (0.7, 0.7, 0.75), "HaareHell": (0.9, 0.9, 0.94),
          "Hose": (0.36, 0.25, 0.16), "Iris": (0.3, 0.5, 0.75), "Schuhe": (0.42, 0.26, 0.13),
          "Borte": (0.95, 0.75, 0.25), "Guertel": (0.4, 0.25, 0.12)}
for mat, rgb in COLORS.items():
    L.cel(mat, rgb)   # gleicher Name -> Material der Figur wird umgefaerbt

ACC = {"grey": L.cel("MENT_Grau", (0.78, 0.78, 0.83)), "gold": L.cel("MENT_Gold", (1.0, 0.8, 0.28), emission=0.2),
       "blue": L.cel("MENT_Blau", (0.1, 0.18, 0.55)), "blue_d": L.cel("MENT_BlauDunkel", (0.07, 0.12, 0.4)),
       "boot": L.cel("MENT_Stiefel", (0.42, 0.26, 0.13)), "wood": L.cel("MENT_Holz", (0.5, 0.32, 0.16)),
       "crystal": L.cel("MENT_Kristall", (0.35, 0.7, 1.0), emission=1.0),
       "belt": L.cel("MENT_Guertel", (0.36, 0.22, 0.11))}
PARTS = [
    ("sphere", "head", "grey", (0, -0.17, 1.38), (0.14, 0.075, 0.16)),            # Bart
    ("sphere", "head", "grey", (0, -0.19, 1.5), (0.1, 0.04, 0.03)),               # Schnurrbart
    ("sphere", "head", "grey", (-0.1, -0.15, 1.69), (0.06, 0.03, 0.02)),          # Augenbrauen
    ("sphere", "head", "grey", (0.1, -0.15, 1.69), (0.06, 0.03, 0.02)),
    ("sphere", "spine", "blue", (0, 0.1, 1.34), (0.24, 0.13, 0.1)),               # Kapuzenfalte im Nacken
    ("torus", "spine", "gold", (0, 0, 1.3), (0.2, 0.2, 0.2)),                     # Goldkragen
    ("cube", "hips", "belt", (0, 0, 0.93), (0.2, 0.17, 0.03)),                    # Guertel
    ("cube", "hips", "gold", (0, -0.17, 0.93), (0.04, 0.015, 0.035)),             # Schnalle
    ("cube", "hips", "blue", (0, 0, 0.62), (0.25, 0.21, 0.26)),                   # langer Rock
    ("cube", "hips", "gold", (0, 0, 0.4), (0.255, 0.215, 0.02)),                  # Goldsaum
    ("cube", "cloak", "gold", (0, 0.02, -0.02), (0.17, 0.02, 0.03)),              # Borte am Umhang
    ("cyl", "shin.L", "boot", (-0.12, 0, 0.15), (0.09, 0.09, 0.13)),              # Stiefel
    ("cyl", "shin.R", "boot", (0.12, 0, 0.15), (0.09, 0.09, 0.13)),
    ("cyl", "forearm.R", "wood", (0.38, -0.06, 0.93), (0.022, 0.022, 0.77)),      # Wanderstab
    ("cyl", "forearm.R", "gold", (0.38, -0.06, 1.68), (0.04, 0.04, 0.02)),
    ("sphere", "forearm.R", "crystal", (0.38, -0.06, 1.77), (0.055, 0.055, 0.08)),  # Kristall
]
parts = [L.shape(kind, loc, sc, (0, 0, 0), ACC[m], bone, collection=scene.collection) for kind, bone, m, loc, sc in PARTS]
acc = L.join(parts, "MENTOR_Zubehoer")
L.attach_to_rig(acc, rig)
L.subdivide(acc, 1)
L.add_outline(acc, 0.016)

# ---- Portraet: Kopf und Schultern von vorn (leichte Aufsicht), 256 px (vor den Laufbildern, damit die Pose sauber ist)
old_cam, old_res = scene.camera, scene.render.resolution_x
L.use_action(rig, bpy.data.actions["Player_idle"])
L.set_frame(scene, list(ANIM_FRAMES["idle"])[0])
scene.render.resolution_x = scene.render.resolution_y = 256
pcam = L.camera(scene, target=(0, 0, 1.52), ortho=1.0, pitch_deg=80.0, name="MentorPortraitCam")
L.render_still(scene, os.path.join(MENU, "mentor_portrait.png"))
scene.camera = old_cam
scene.render.resolution_x = scene.render.resolution_y = old_res
bpy.data.objects.remove(pcam, do_unlink=True)

count = 0
warnings = 0
for anim, action in (("idle", "Player_idle"), ("walk", "Player_walk")):
    L.use_action(rig, bpy.data.actions[action])
    frames = list(ANIM_FRAMES[anim])
    for direction, angle in L.DIRECTIONS.items():
        rig.rotation_euler = (0, 0, math.radians(angle))
        for i, f in enumerate(frames):
            L.set_frame(scene, f)
            path = os.path.join(OUT, f"{anim}_{direction}_{i}.png")
            L.render_still(scene, path)
            count += 1
            if L.edge_pixels(path):
                warnings += 1
                print("WARNUNG Randpixel:", os.path.basename(path))
rig.rotation_euler = (0, 0, 0)
bpy.data.objects.remove(acc, do_unlink=True)

print(f"RESULT rendered={count} randwarnungen={warnings}")
# absichtlich kein save_as_mainfile
