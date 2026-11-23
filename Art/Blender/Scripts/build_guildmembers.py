"""RE:BORN - Gildenmitglieder (Meilenstein 8, Comic-Look)

Sechs Figuren auf Basis der Spielfigur (gleiches Skelett, gleiche Animationen "idle" und "walk"), umgefaerbt und mit
eigenem Zubehoer: krieger_1, krieger_2, magier_1, magier_2, assassine_1, assassine_2.
Rendert idle (4 Bilder) und walk (8 Bilder) in 4 Richtungen, 128 px, nach
Sprites/Interiors/Member_<name>/<anim>_<richtung>_<i>.png (Aufbau wie build_villagers.py).
Die Datei Reborn_Characters.blend wird NICHT gespeichert.

Aufruf:  blender.exe -b Art/Blender/Reborn_Characters.blend --python build_guildmembers.py -- <projektordner> [<ausgabeordner>]
"""
import bpy
import math
import os
import sys

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import reborn_lib as L  # noqa: E402

argv = sys.argv[sys.argv.index("--") + 1:] if "--" in sys.argv else []
PROJECT = argv[0] if argv else os.path.abspath(os.path.join(os.path.dirname(__file__), "..", "..", ".."))
OUT = argv[1] if len(argv) > 1 else os.path.join(PROJECT, "Assets", "_Project", "Sprites", "Interiors")

scene = bpy.data.scenes["Characters"]
rig = bpy.data.objects["PlayerRig"]
body = bpy.data.objects["Player"]
ANIM_FRAMES = scene["anim_frames"]
for o in scene.objects:
    if o.name.startswith(("EQ_", "ICON_")) or o.name == "IconCamera":
        o.hide_render = True
body.hide_render = False
body.is_holdout = False

MEMBERS = {
    "krieger_1": {   # rote Tunika, Lederruestung, Kurzschwert
        "colors": {"Tunika": (0.78, 0.18, 0.15), "TunikaDunkel": (0.5, 0.1, 0.1), "Umhang": (0.55, 0.12, 0.1),
                   "UmhangInnen": (0.32, 0.07, 0.07), "Haare": (0.3, 0.18, 0.1), "HaareHell": (0.45, 0.3, 0.16),
                   "Hose": (0.4, 0.28, 0.18), "Iris": (0.3, 0.2, 0.1)},
        "parts": [("cyl", "spine", "leather", (0, 0, 1.12), (0.235, 0.172, 0.2)),               # Lederharnisch
                  ("sphere", "spine", "leather", (0, 0, 1.26), (0.265, 0.18, 0.11)),
                  ("cyl", "hips", "leather_d", (0, 0, 0.93), (0.262, 0.19, 0.06)),             # Guertel
                  ("sphere", "upperarm.L", "leather_d", (-0.31, 0, 1.28), (0.095, 0.095, 0.06)),
                  ("sphere", "upperarm.R", "leather_d", (0.31, 0, 1.28), (0.095, 0.095, 0.06)),
                  ("sphere", "spine", "gold", (0, -0.185, 1.12), (0.035, 0.012, 0.035)),
                  ("cube", "hips", "metal", (-0.28, 0.0, 0.72), (0.02, 0.02, 0.2)),            # Kurzschwert an der Hueft
                  ("cube", "hips", "gold", (-0.28, 0.0, 0.93), (0.06, 0.02, 0.015))],
    },
    "krieger_2": {   # Plattenruestung, Helm
        "colors": {"Tunika": (0.5, 0.52, 0.58), "TunikaDunkel": (0.32, 0.34, 0.4), "Umhang": (0.55, 0.1, 0.1),
                   "UmhangInnen": (0.3, 0.06, 0.06), "Haare": (0.2, 0.15, 0.1), "HaareHell": (0.3, 0.24, 0.18),
                   "Hose": (0.35, 0.36, 0.42), "Iris": (0.25, 0.3, 0.4)},
        "parts": [("cyl", "spine", "plate", (0, 0, 1.12), (0.24, 0.178, 0.205)),
                  ("sphere", "spine", "plate", (0, 0, 1.265), (0.27, 0.185, 0.115)),
                  ("sphere", "spine", "gold", (0, -0.185, 1.19), (0.05, 0.012, 0.05)),
                  ("cyl", "hips", "plate_d", (0, 0, 0.92), (0.265, 0.192, 0.07)),
                  ("sphere", "upperarm.L", "plate", (-0.32, 0, 1.29), (0.12, 0.12, 0.09)),     # Schulterplatten
                  ("sphere", "upperarm.R", "plate", (0.32, 0, 1.29), (0.12, 0.12, 0.09)),
                  ("sphere", "head", "plate", (0, 0.01, 1.68), (0.28, 0.27, 0.2)),             # Helm
                  ("cyl", "head", "plate_d", (0, 0.01, 1.6), (0.285, 0.275, 0.025)),
                  ("cube", "head", "plate_d", (0, -0.272, 1.57), (0.018, 0.014, 0.075)),
                  ("cube", "head", "red", (0, 0.03, 1.87), (0.025, 0.21, 0.05)),               # Helmkamm
                  ("cyl", "forearm.L", "plate", (-0.32, 0, 1.0), (0.075, 0.075, 0.08)),
                  ("cyl", "forearm.R", "plate", (0.32, 0, 1.0), (0.075, 0.075, 0.08))],
    },
    "magier_1": {    # gruene Robe, Kapuze, Buch
        "colors": {"Tunika": (0.2, 0.55, 0.3), "TunikaDunkel": (0.12, 0.38, 0.2), "Umhang": (0.15, 0.45, 0.25),
                   "UmhangInnen": (0.08, 0.28, 0.15), "Haare": (0.6, 0.5, 0.3), "HaareHell": (0.75, 0.65, 0.4),
                   "Hose": (0.15, 0.4, 0.22), "Iris": (0.3, 0.7, 0.4)},
        "parts": [("cube", "hips", "robe_g", (0, 0, 0.55), (0.25, 0.21, 0.32)),
                  ("sphere", "head", "robe_g", (0, 0.09, 1.66), (0.29, 0.27, 0.21)),            # Kapuze
                  ("cone", "head", "robe_g", (0, 0.2, 1.62), (0.2, 0.12, 0.22), (150, 0, 0)),   # Kapuzenzipfel hinten
                  ("cyl", "spine", "gold", (0, 0, 1.31), (0.2, 0.15, 0.03)),                    # Kragen
                  ("cube", "forearm.L", "book", (-0.36, -0.12, 0.92), (0.1, 0.03, 0.13)),       # Zauberbuch
                  ("cube", "forearm.L", "gold", (-0.36, -0.155, 0.92), (0.06, 0.005, 0.02)),
                  ("sphere", "forearm.L", "gem", (-0.36, -0.16, 0.99), (0.025, 0.01, 0.025))],
    },
    "magier_2": {    # lila Robe, Spitzhut
        "colors": {"Tunika": (0.5, 0.25, 0.7), "TunikaDunkel": (0.33, 0.15, 0.48), "Umhang": (0.4, 0.2, 0.6),
                   "UmhangInnen": (0.22, 0.1, 0.35), "Haare": (0.85, 0.85, 0.9), "HaareHell": (0.95, 0.95, 0.98),
                   "Hose": (0.38, 0.18, 0.55), "Iris": (0.7, 0.3, 0.8)},
        "parts": [("cube", "hips", "robe_p", (0, 0, 0.55), (0.25, 0.21, 0.32)),
                  ("cyl", "head", "hat", (0, 0.03, 1.77), (0.37, 0.35, 0.02)),                  # Hutkrempe
                  ("cone", "head", "hat", (0, 0.04, 2.0), (0.22, 0.21, 0.27)),                  # Hutspitze
                  ("cyl", "head", "gold", (0, 0.03, 1.81), (0.225, 0.215, 0.03)),               # Hutband
                  ("sphere", "head", "gem", (0, -0.2, 1.82), (0.04, 0.015, 0.04)),
                  ("sphere", "head", "beard", (0, -0.17, 1.39), (0.1, 0.06, 0.09)),
                  ("sphere", "spine", "gold", (0, -0.17, 1.3), (0.03, 0.02, 0.03))],
    },
    "assassine_1": { # schwarzer Mantel, Kapuze, Maske
        "colors": {"Tunika": (0.12, 0.11, 0.15), "TunikaDunkel": (0.06, 0.05, 0.08), "Umhang": (0.1, 0.09, 0.13),
                   "UmhangInnen": (0.45, 0.08, 0.1), "Haare": (0.1, 0.08, 0.1), "HaareHell": (0.2, 0.17, 0.2),
                   "Hose": (0.1, 0.09, 0.12), "Iris": (0.9, 0.2, 0.2)},
        "parts": [("cube", "hips", "coat", (0, 0, 0.58), (0.26, 0.22, 0.3)),                   # Mantel unten
                  ("sphere", "head", "coat", (0, 0.07, 1.67), (0.29, 0.27, 0.21)),              # Kapuze
                  ("cone", "head", "coat", (0, 0.2, 1.62), (0.2, 0.12, 0.22), (150, 0, 0)),
                  ("cube", "head", "mask", (0, -0.19, 1.51), (0.15, 0.03, 0.075)),              # Maske
                  ("cube", "spine", "red_cloth", (0, -0.17, 1.0), (0.2, 0.02, 0.04)),           # Schaerpe
                  ("sphere", "spine", "coat", (0, 0, 1.3), (0.26, 0.19, 0.1))],
    },
    "assassine_2": { # dunkelgrau, Halstuch, Dolche
        "colors": {"Tunika": (0.32, 0.33, 0.37), "TunikaDunkel": (0.2, 0.21, 0.25), "Umhang": (0.25, 0.26, 0.3),
                   "UmhangInnen": (0.14, 0.15, 0.18), "Haare": (0.55, 0.12, 0.15), "HaareHell": (0.75, 0.2, 0.22),
                   "Hose": (0.22, 0.23, 0.27), "Iris": (0.6, 0.8, 0.3)},
        "parts": [("cyl", "spine", "scarf_r", (0, -0.01, 1.35), (0.17, 0.15, 0.06)),            # Halstuch
                  ("cube", "spine", "scarf_r", (0.1, -0.12, 1.22), (0.05, 0.02, 0.1), (0, 0, 15)),
                  ("cyl", "hips", "leather_d", (0, 0, 0.93), (0.262, 0.19, 0.05)),
                  ("cube", "hips", "metal", (-0.27, -0.04, 0.78), (0.015, 0.015, 0.12), (0, 0, 20)),   # Dolche an den Hueften
                  ("cube", "hips", "red_cloth", (-0.27, -0.04, 0.93), (0.04, 0.02, 0.015)),
                  ("cube", "hips", "metal", (0.27, -0.04, 0.78), (0.015, 0.015, 0.12), (0, 0, -20)),
                  ("cube", "hips", "red_cloth", (0.27, -0.04, 0.93), (0.04, 0.02, 0.015)),
                  ("cube", "spine", "leather_d", (0, -0.17, 1.15), (0.01, 0.03, 0.2), (0, 0, 40)),   # Brustriemen
                  ("sphere", "spine", "gold", (0, -0.2, 1.13), (0.03, 0.015, 0.03))],
    },
}
ACC = {"leather": L.cel("GM_Leder", (0.55, 0.35, 0.18)), "leather_d": L.cel("GM_LederDunkel", (0.34, 0.2, 0.1)),
       "gold": L.cel("GM_Gold", (1.0, 0.82, 0.3), emission=0.2), "metal": L.cel("GM_Metall", (0.7, 0.72, 0.8)),
       "plate": L.cel("GM_Platte", (0.62, 0.64, 0.7), light=1.05), "plate_d": L.cel("GM_PlatteDunkel", (0.4, 0.42, 0.5)),
       "red": L.cel("GM_Rot", (0.8, 0.14, 0.14)), "robe_g": L.cel("GM_RobeGruen", (0.2, 0.52, 0.3)),
       "robe_p": L.cel("GM_RobeLila", (0.48, 0.24, 0.68)), "hat": L.cel("GM_Hut", (0.36, 0.18, 0.55)),
       "book": L.cel("GM_Buch", (0.6, 0.18, 0.18)), "gem": L.cel("GM_Stein", (0.4, 0.9, 1.0), emission=0.6),
       "beard": L.cel("GM_Bart", (0.92, 0.92, 0.95)), "coat": L.cel("GM_Mantel", (0.1, 0.09, 0.13)),
       "mask": L.cel("GM_Maske", (0.05, 0.04, 0.06), shadow=1.0, mid=1.0), "red_cloth": L.cel("GM_RotStoff", (0.7, 0.1, 0.12)),
       "scarf_r": L.cel("GM_Halstuch", (0.75, 0.15, 0.2))}
ANIMS = (("idle", "Player_idle"), ("walk", "Player_walk"))

count = 0
warnings = 0
for name, v in MEMBERS.items():
    for mat, rgb in v["colors"].items():
        L.cel(mat, rgb)   # gleicher Name -> Material der Figur wird umgefaerbt
    parts = [L.shape(p[0], p[3], p[4], p[5] if len(p) > 5 else (0, 0, 0), ACC[p[2]], p[1], collection=scene.collection) for p in v["parts"]]
    acc = L.join(parts, "GM_" + name)
    L.attach_to_rig(acc, rig)
    L.subdivide(acc, 1)
    L.add_outline(acc, 0.016)
    out = os.path.join(OUT, "Member_" + name)
    os.makedirs(out, exist_ok=True)
    for anim, action in ANIMS:
        L.use_action(rig, bpy.data.actions[action])
        frames = list(ANIM_FRAMES[anim])
        for direction, angle in L.DIRECTIONS.items():
            rig.rotation_euler = (0, 0, math.radians(angle))
            for i, f in enumerate(frames):
                L.set_frame(scene, f)
                path = os.path.join(out, f"{anim}_{direction}_{i}.png")
                L.render_still(scene, path)
                count += 1
                if L.edge_pixels(path):
                    warnings += 1
                    print("WARNUNG Randpixel:", name, os.path.basename(path))
    rig.rotation_euler = (0, 0, 0)
    bpy.data.objects.remove(acc, do_unlink=True)

print(f"RESULT rendered={count} randwarnungen={warnings}")
# absichtlich kein save_as_mainfile: die Spielfigur in Reborn_Characters.blend bleibt wie sie ist
