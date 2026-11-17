"""RE:BORN - Stadtbewohner (Meilenstein 6.1, Comic-Look)

Elf Figuren auf Basis der Spielfigur (gleiches Skelett, gleiche Animationen "idle" und "walk"),
umgefaerbt und mit eigenem Zubehoer: Baeuerin, Wache, Gelehrter, Soldat (Mauer), Bauer, Edeldame, Barde, Fischer
und die Haendler fuer Waffen, Ruestung und Traenke (Meilenstein 7a).
Rendert idle (4 Bilder) und walk (8 Bilder) in 4 Richtungen, 128 px, nach
Sprites/Town/Villager_<name>/<anim>_<richtung>_<i>.png (Aufbau wie Sprites/Player).
Die Datei Reborn_Characters.blend wird NICHT gespeichert (Spielfigur bleibt unveraendert).

Aufruf:  blender.exe -b Art/Blender/Reborn_Characters.blend --python build_villagers.py -- <projektordner> [<ausgabeordner>]
"""
import bpy
import math
import os
import sys

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import reborn_lib as L  # noqa: E402

argv = sys.argv[sys.argv.index("--") + 1:] if "--" in sys.argv else []
PROJECT = argv[0] if argv else os.path.abspath(os.path.join(os.path.dirname(__file__), "..", "..", ".."))
OUT = argv[1] if len(argv) > 1 else os.path.join(PROJECT, "Assets", "_Project", "Sprites", "Town")

scene = bpy.data.scenes["Characters"]
rig = bpy.data.objects["PlayerRig"]
body = bpy.data.objects["Player"]
ANIM_FRAMES = scene["anim_frames"]
for o in scene.objects:
    if o.name.startswith(("EQ_", "ICON_")) or o.name == "IconCamera":
        o.hide_render = True
body.hide_render = False
body.is_holdout = False

VILLAGERS = {
    "baeuerin": {
        "colors": {"Tunika": (0.75, 0.42, 0.3), "TunikaDunkel": (0.5, 0.25, 0.18), "Umhang": (0.9, 0.85, 0.7),
                   "UmhangInnen": (0.7, 0.65, 0.5), "Haare": (0.6, 0.4, 0.18), "HaareHell": (0.78, 0.55, 0.26),
                   "Hose": (0.45, 0.3, 0.22), "Iris": (0.25, 0.45, 0.2)},
        "parts": [("sphere", "head", "scarf", (0, 0.07, 1.72), (0.23, 0.22, 0.15)),          # Kopftuch (Gesicht frei)
                  ("cube", "hips", "skirt", (0, 0, 0.62), (0.26, 0.22, 0.24)),              # Rock
                  ("cyl", "forearm.L", "basket", (-0.4, -0.08, 0.9), (0.13, 0.13, 0.08)),   # Korb
                  ("sphere", "forearm.L", "apple", (-0.4, -0.08, 0.99), (0.05, 0.05, 0.04)),
                  ("sphere", "forearm.L", "bread", (-0.36, -0.03, 0.98), (0.07, 0.04, 0.03))],
    },
    "wache": {
        "colors": {"Tunika": (0.25, 0.35, 0.65), "TunikaDunkel": (0.15, 0.2, 0.42), "Umhang": (0.2, 0.28, 0.55),
                   "UmhangInnen": (0.12, 0.16, 0.32), "Haare": (0.18, 0.15, 0.12), "HaareHell": (0.3, 0.25, 0.2),
                   "Hose": (0.3, 0.3, 0.34), "Iris": (0.3, 0.3, 0.35)},
        "parts": [("sphere", "head", "helmet", (0, 0.0, 1.7), (0.25, 0.25, 0.17)),           # Helm
                  ("cyl", "head", "helmet", (0, 0.0, 1.62), (0.27, 0.27, 0.02)),
                  ("cube", "spine", "plate", (0, -0.16, 1.15), (0.2, 0.03, 0.17)),          # Brustplatte
                  ("sphere", "spine", "gold", (0, -0.2, 1.17), (0.04, 0.02, 0.04)),
                  ("cyl", "forearm.R", "shaft", (0.36, -0.06, 1.0), (0.02, 0.02, 0.62)),    # Speer
                  ("cone", "forearm.R", "plate", (0.36, -0.06, 1.68), (0.045, 0.045, 0.08))],
    },
    "gelehrter": {
        "colors": {"Tunika": (0.45, 0.25, 0.55), "TunikaDunkel": (0.3, 0.15, 0.38), "Umhang": (0.38, 0.2, 0.48),
                   "UmhangInnen": (0.22, 0.1, 0.3), "Haare": (0.85, 0.85, 0.88), "HaareHell": (0.95, 0.95, 0.97),
                   "Hose": (0.35, 0.18, 0.42), "Iris": (0.3, 0.3, 0.6)},
        "parts": [("cube", "hips", "robe", (0, 0, 0.55), (0.25, 0.21, 0.32)),               # Robe unten
                  ("sphere", "head", "beard", (0, -0.17, 1.39), (0.11, 0.06, 0.1)),         # Bart
                  ("cube", "forearm.L", "book", (-0.36, -0.12, 0.92), (0.1, 0.03, 0.13)),   # Buch
                  ("cube", "forearm.L", "gold", (-0.36, -0.155, 0.92), (0.06, 0.005, 0.02)),
                  ("cone", "head", "hood", (0, 0.06, 1.92), (0.2, 0.2, 0.2))],             # Spitzhut
    },
    # ---- Meilenstein 7a: Soldaten auf der Mauer, weitere Bewohner, Haendler
    "soldat": {
        "colors": {"Tunika": (0.7, 0.18, 0.18), "TunikaDunkel": (0.45, 0.1, 0.1), "Umhang": (0.6, 0.12, 0.12),
                   "UmhangInnen": (0.35, 0.08, 0.08), "Haare": (0.25, 0.18, 0.12), "HaareHell": (0.38, 0.28, 0.18),
                   "Hose": (0.32, 0.32, 0.36), "Iris": (0.3, 0.3, 0.35)},
        "parts": [("sphere", "head", "helmet", (0, 0.0, 1.7), (0.25, 0.25, 0.17)),
                  ("cone", "head", "plate", (0, 0.0, 1.9), (0.06, 0.06, 0.08)),             # Helmspitze
                  ("cube", "spine", "plate", (0, -0.16, 1.15), (0.2, 0.03, 0.17)),
                  ("cyl", "forearm.L", "shield_r", (-0.4, -0.05, 0.98), (0.17, 0.03, 0.2)), # Schild
                  ("sphere", "forearm.L", "gold", (-0.4, -0.09, 0.98), (0.04, 0.02, 0.04)),
                  ("cyl", "forearm.R", "shaft", (0.36, -0.06, 1.0), (0.02, 0.02, 0.62)),
                  ("cone", "forearm.R", "plate", (0.36, -0.06, 1.68), (0.045, 0.045, 0.08))],
    },
    "bauer": {
        "colors": {"Tunika": (0.45, 0.6, 0.3), "TunikaDunkel": (0.3, 0.42, 0.2), "Umhang": (0.6, 0.5, 0.3),
                   "UmhangInnen": (0.42, 0.34, 0.2), "Haare": (0.4, 0.28, 0.15), "HaareHell": (0.55, 0.4, 0.22),
                   "Hose": (0.5, 0.4, 0.28), "Iris": (0.3, 0.25, 0.15)},
        "parts": [("cyl", "head", "straw", (0, 0.02, 1.77), (0.36, 0.34, 0.02)),            # Strohhut
                  ("cone", "head", "straw", (0, 0.02, 1.86), (0.2, 0.2, 0.1)),
                  ("cyl", "forearm.R", "shaft", (0.36, -0.04, 0.95), (0.02, 0.02, 0.55)),   # Mistgabel
                  ("cube", "forearm.R", "metal", (0.36, -0.04, 1.52), (0.08, 0.01, 0.06))],
    },
    "edeldame": {
        "colors": {"Tunika": (0.3, 0.45, 0.85), "TunikaDunkel": (0.2, 0.3, 0.6), "Umhang": (0.85, 0.85, 0.95),
                   "UmhangInnen": (0.6, 0.6, 0.75), "Haare": (0.95, 0.8, 0.4), "HaareHell": (1.0, 0.9, 0.55),
                   "Hose": (0.3, 0.45, 0.85), "Iris": (0.2, 0.4, 0.8)},
        "parts": [("cone", "hips", "dress", (0, 0, 0.55), (0.32, 0.28, 0.45)),               # Kleid
                  ("cyl", "head", "gold", (0, 0.0, 1.8), (0.15, 0.15, 0.03)),               # Diadem
                  ("sphere", "head", "gem", (0, -0.15, 1.81), (0.03, 0.02, 0.03))],
    },
    "barde": {
        "colors": {"Tunika": (0.85, 0.55, 0.2), "TunikaDunkel": (0.6, 0.35, 0.1), "Umhang": (0.3, 0.6, 0.55),
                   "UmhangInnen": (0.18, 0.4, 0.36), "Haare": (0.15, 0.12, 0.1), "HaareHell": (0.28, 0.22, 0.18),
                   "Hose": (0.3, 0.6, 0.55), "Iris": (0.35, 0.25, 0.1)},
        "parts": [("cone", "head", "cap", (0.03, 0.04, 1.8), (0.22, 0.2, 0.1)),              # Barettmuetze
                  ("sphere", "head", "feather", (0.18, 0.08, 1.9), (0.03, 0.03, 0.12)),
                  ("sphere", "spine", "lute", (0.05, -0.2, 1.0), (0.13, 0.05, 0.16)),        # Laute
                  ("cube", "spine", "wood_d", (0.18, -0.2, 1.22), (0.025, 0.02, 0.18))],
    },
    "fischer": {
        "colors": {"Tunika": (0.9, 0.78, 0.25), "TunikaDunkel": (0.65, 0.55, 0.15), "Umhang": (0.9, 0.78, 0.25),
                   "UmhangInnen": (0.6, 0.5, 0.15), "Haare": (0.6, 0.6, 0.62), "HaareHell": (0.75, 0.75, 0.78),
                   "Hose": (0.25, 0.3, 0.45), "Iris": (0.3, 0.45, 0.6)},
        "parts": [("sphere", "head", "rainhat", (0, 0.05, 1.72), (0.27, 0.27, 0.13)),        # Suedwester
                  ("sphere", "head", "beard", (0, -0.17, 1.39), (0.11, 0.06, 0.1)),
                  ("cyl", "forearm.R", "shaft", (0.36, -0.06, 1.1), (0.015, 0.015, 0.7)),   # Angel
                  ("sphere", "forearm.L", "fish", (-0.38, -0.06, 0.85), (0.04, 0.12, 0.05))],
    },
    "waffenhaendler": {
        "colors": {"Tunika": (0.35, 0.25, 0.2), "TunikaDunkel": (0.22, 0.15, 0.12), "Umhang": (0.4, 0.28, 0.18),
                   "UmhangInnen": (0.25, 0.17, 0.1), "Haare": (0.12, 0.1, 0.1), "HaareHell": (0.22, 0.18, 0.16),
                   "Hose": (0.25, 0.2, 0.17), "Iris": (0.25, 0.2, 0.15)},
        "parts": [("cube", "spine", "apron", (0, -0.17, 1.08), (0.2, 0.02, 0.26)),
                  ("cube", "hips", "apron", (0, -0.2, 0.78), (0.23, 0.02, 0.2)),
                  ("sphere", "head", "beard", (0, -0.17, 1.4), (0.15, 0.08, 0.12)),
                  ("cube", "hips", "metal", (-0.28, 0.0, 0.7), (0.02, 0.02, 0.28)),          # Schwert an der Hueft
                  ("cube", "hips", "gold", (-0.28, 0.0, 0.98), (0.06, 0.02, 0.015))],
    },
    "ruestungshaendler": {
        "colors": {"Tunika": (0.55, 0.57, 0.62), "TunikaDunkel": (0.38, 0.4, 0.45), "Umhang": (0.2, 0.3, 0.55),
                   "UmhangInnen": (0.12, 0.18, 0.35), "Haare": (0.85, 0.85, 0.88), "HaareHell": (0.95, 0.95, 0.97),
                   "Hose": (0.3, 0.3, 0.34), "Iris": (0.3, 0.35, 0.45)},
        "parts": [("cube", "spine", "plate", (0, -0.16, 1.15), (0.21, 0.03, 0.18)),
                  ("sphere", "upperarm.L", "plate", (-0.33, 0, 1.3), (0.1, 0.1, 0.08)),     # Schulterplatten
                  ("sphere", "upperarm.R", "plate", (0.33, 0, 1.3), (0.1, 0.1, 0.08)),
                  ("sphere", "head", "beard", (0, -0.17, 1.4), (0.13, 0.07, 0.1))],
    },
    "alchemistin": {
        "colors": {"Tunika": (0.25, 0.55, 0.4), "TunikaDunkel": (0.15, 0.38, 0.27), "Umhang": (0.2, 0.45, 0.35),
                   "UmhangInnen": (0.12, 0.28, 0.2), "Haare": (0.55, 0.2, 0.15), "HaareHell": (0.72, 0.3, 0.2),
                   "Hose": (0.25, 0.55, 0.4), "Iris": (0.3, 0.6, 0.3)},
        "parts": [("cube", "hips", "robe_g", (0, 0, 0.55), (0.25, 0.21, 0.32)),
                  ("cyl", "forearm.R", "potion_r", (0.36, -0.08, 0.92), (0.05, 0.05, 0.07)), # Trank
                  ("cyl", "forearm.R", "glass", (0.36, -0.08, 1.01), (0.02, 0.02, 0.03)),
                  ("cyl", "head", "band", (0, 0, 1.69), (0.25, 0.24, 0.025)),
                  ("sphere", "hips", "potion_b", (-0.24, -0.1, 0.88), (0.05, 0.04, 0.05))],
    },
}
ACC = {"scarf": L.cel("DORF_Kopftuch", (0.85, 0.3, 0.3)), "skirt": L.cel("DORF_Rock", (0.6, 0.3, 0.22)),
       "basket": L.cel("DORF_Korb", (0.7, 0.5, 0.25)), "apple": L.cel("DORF_Apfel", (0.85, 0.2, 0.15)),
       "bread": L.cel("DORF_Brot", (0.85, 0.6, 0.3)), "helmet": L.cel("DORF_Helm", (0.62, 0.64, 0.7)),
       "plate": L.cel("DORF_Platte", (0.62, 0.64, 0.7)), "gold": L.cel("DORF_Gold", (1.0, 0.82, 0.3), emission=0.2),
       "shaft": L.cel("DORF_Schaft", (0.45, 0.28, 0.14)), "robe": L.cel("DORF_Robe", (0.45, 0.25, 0.55)),
       "beard": L.cel("DORF_Bart", (0.9, 0.9, 0.92)), "book": L.cel("DORF_Buch", (0.55, 0.15, 0.15)),
       "hood": L.cel("DORF_Hut", (0.3, 0.15, 0.4)),
       "shield_r": L.cel("DORF_Schild", (0.75, 0.2, 0.2)), "straw": L.cel("DORF_Stroh", (0.92, 0.8, 0.4)),
       "metal": L.cel("DORF_Metall", (0.45, 0.46, 0.52)), "dress": L.cel("DORF_Kleid", (0.3, 0.45, 0.85)),
       "gem": L.cel("DORF_Edelstein", (0.9, 0.2, 0.4), emission=0.4), "cap": L.cel("DORF_Muetze", (0.6, 0.15, 0.3)),
       "feather": L.cel("DORF_Feder", (0.95, 0.95, 0.9)), "lute": L.cel("DORF_Laute", (0.7, 0.45, 0.2)),
       "wood_d": L.cel("DORF_HolzDunkel", (0.36, 0.2, 0.1)), "rainhat": L.cel("DORF_Suedwester", (0.95, 0.8, 0.2)),
       "fish": L.cel("DORF_Fisch", (0.6, 0.7, 0.8)), "apron": L.cel("DORF_Schuerze", (0.42, 0.26, 0.13)),
       "robe_g": L.cel("DORF_RobeGruen", (0.2, 0.45, 0.33)), "potion_r": L.cel("DORF_TrankRot", (0.9, 0.2, 0.25), emission=0.3),
       "potion_b": L.cel("DORF_TrankBlau", (0.25, 0.45, 0.95), emission=0.3), "glass": L.cel("DORF_Glas", (0.8, 0.9, 0.95)),
       "band": L.cel("DORF_Band", (0.95, 0.78, 0.3), light=1.1)}
ANIMS = (("idle", "Player_idle"), ("walk", "Player_walk"))

count = 0
warnings = 0
for name, v in VILLAGERS.items():
    for mat, rgb in v["colors"].items():
        L.cel(mat, rgb)   # gleicher Name -> Material der Figur wird umgefaerbt
    parts = [L.shape(kind, loc, sc, (0, 0, 0), ACC[m], bone, collection=scene.collection) for kind, bone, m, loc, sc in v["parts"]]
    acc = L.join(parts, "DORF_" + name)
    L.attach_to_rig(acc, rig)
    L.subdivide(acc, 1)
    L.add_outline(acc, 0.016)
    out = os.path.join(OUT, "Villager_" + name)
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
