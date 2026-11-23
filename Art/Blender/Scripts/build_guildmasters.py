"""RE:BORN - Gildenmeister (Meilenstein 7b, Comic-Look)

Drei Figuren auf Basis der Spielfigur (gleiches Skelett, Warte-Animation), umgefaerbt und mit eigenem Zubehoer:
Kriegerin (Ruestung, Helm mit Federbusch, Zweihaender), Erzmagier (Robe, Spitzhut, Bart, Stab mit Kristall),
Schattenmeisterin (Kapuze, Maske, zwei Dolche). Rendert idle nach unten (4 Bilder, 128 px) nach
Sprites/Interiors/master_<name>_<i>.png. Reborn_Characters.blend wird NICHT gespeichert.

Aufruf:  blender.exe -b Art/Blender/Reborn_Characters.blend --python build_guildmasters.py -- <projektordner>
"""
import bpy
import os
import sys

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import reborn_lib as L  # noqa: E402

argv = sys.argv[sys.argv.index("--") + 1:] if "--" in sys.argv else []
PROJECT = argv[0] if argv else os.path.abspath(os.path.join(os.path.dirname(__file__), "..", "..", ".."))
OUT = os.path.join(PROJECT, "Assets", "_Project", "Sprites", "Interiors")
os.makedirs(OUT, exist_ok=True)

scene = bpy.data.scenes["Characters"]
rig = bpy.data.objects["PlayerRig"]
body = bpy.data.objects["Player"]
FRAMES = list(scene["anim_frames"]["idle"])
for o in scene.objects:
    if o.name.startswith(("EQ_", "ICON_")) or o.name == "IconCamera":
        o.hide_render = True
body.hide_render = False
body.is_holdout = False
rig.rotation_euler = (0, 0, 0)
L.use_action(rig, bpy.data.actions["Player_idle"])

MASTERS = {
    "krieger": {
        "colors": {"Tunika": (0.7, 0.18, 0.15), "TunikaDunkel": (0.45, 0.1, 0.08), "Umhang": (0.75, 0.2, 0.15),
                   "UmhangInnen": (0.45, 0.1, 0.08), "Haare": (0.85, 0.65, 0.3), "HaareHell": (0.95, 0.8, 0.45),
                   "Hose": (0.35, 0.33, 0.36), "Iris": (0.35, 0.55, 0.25)},
        "parts": [("sphere", "head", "steel", (0, 0.02, 1.7), (0.26, 0.26, 0.17)),             # Helm
                  ("cyl", "head", "steel", (0, 0.02, 1.62), (0.28, 0.28, 0.02)),
                  ("cone", "head", "plume", (0, 0.05, 1.93), (0.05, 0.14, 0.12)),              # Federbusch
                  ("cube", "spine", "steel", (0, -0.16, 1.15), (0.21, 0.035, 0.18)),           # Brustplatte
                  ("sphere", "spine", "steel", (-0.3, 0, 1.3), (0.11, 0.1, 0.07)),             # Schulterplatten
                  ("sphere", "spine", "steel", (0.3, 0, 1.3), (0.11, 0.1, 0.07)),
                  ("sphere", "spine", "gold", (0, -0.2, 1.2), (0.05, 0.02, 0.05)),
                  ("cyl", "spine", "steel", (0.18, 0.22, 1.25), (0.03, 0.03, 0.5), ),           # Zweihaender (Ruecken)
                  ("cube", "spine", "gold", (0.18, 0.22, 1.5), (0.13, 0.025, 0.02))],
    },
    "magier": {
        "colors": {"Tunika": (0.2, 0.3, 0.75), "TunikaDunkel": (0.12, 0.18, 0.5), "Umhang": (0.18, 0.22, 0.6),
                   "UmhangInnen": (0.5, 0.35, 0.8), "Haare": (0.88, 0.88, 0.9), "HaareHell": (0.97, 0.97, 0.98),
                   "Hose": (0.15, 0.2, 0.5), "Iris": (0.4, 0.6, 0.95)},
        "parts": [("cone", "head", "robe", (0, 0.02, 1.95), (0.2, 0.2, 0.22)),                 # Spitzhut
                  ("cyl", "head", "robe", (0, 0.02, 1.75), (0.32, 0.31, 0.02)),                # Hutkrempe
                  ("sphere", "head", "star", (0.08, -0.15, 1.88), (0.03, 0.01, 0.03)),
                  ("sphere", "head", "beard", (0, -0.17, 1.4), (0.15, 0.08, 0.16)),            # langer Bart
                  ("cube", "hips", "robe", (0, 0, 0.55), (0.26, 0.22, 0.33)),                  # Robe unten
                  ("cyl", "forearm.R", "wood", (0.38, -0.06, 0.95), (0.022, 0.022, 0.75)),     # Stab
                  ("sphere", "forearm.R", "crystal", (0.38, -0.06, 1.75), (0.06, 0.06, 0.08))],
    },
    "assassine": {
        "colors": {"Tunika": (0.15, 0.13, 0.18), "TunikaDunkel": (0.08, 0.07, 0.1), "Umhang": (0.2, 0.12, 0.25),
                   "UmhangInnen": (0.45, 0.1, 0.15), "Haare": (0.1, 0.08, 0.1), "HaareHell": (0.2, 0.15, 0.2),
                   "Hose": (0.12, 0.11, 0.14), "Iris": (0.75, 0.2, 0.25)},
        "parts": [("sphere", "head", "hood", (0, 0.05, 1.66), (0.29, 0.28, 0.25)),             # Kapuze
                  ("cone", "head", "hood", (0, 0.18, 1.78), (0.12, 0.1, 0.12)),
                  ("cube", "head", "mask", (0, -0.2, 1.47), (0.15, 0.02, 0.05)),               # Maske
                  ("cube", "hips", "belt_r", (0, -0.17, 0.97), (0.24, 0.02, 0.03)),            # roter Guertel
                  ("cube", "hips", "blade", (-0.22, -0.1, 0.82), (0.015, 0.02, 0.12)),         # Dolche
                  ("cube", "hips", "blade", (0.22, -0.1, 0.82), (0.015, 0.02, 0.12)),
                  ("cube", "hips", "wood", (-0.22, -0.1, 0.96), (0.025, 0.025, 0.04)),
                  ("cube", "hips", "wood", (0.22, -0.1, 0.96), (0.025, 0.025, 0.04))],
    },
}
ACC = {"steel": L.cel("GM_Stahl", (0.62, 0.64, 0.7), light=1.05), "gold": L.cel("GM_Gold", (1.0, 0.8, 0.3), light=1.1),
       "plume": L.cel("GM_Busch", (0.9, 0.15, 0.15)), "robe": L.cel("GM_Robe", (0.18, 0.22, 0.62)),
       "star": L.cel("GM_Stern", (1.0, 0.9, 0.4), emission=0.4), "beard": L.cel("GM_Bart", (0.94, 0.94, 0.95)),
       "wood": L.cel("GM_Holz", (0.45, 0.28, 0.14)), "crystal": L.cel("GM_Kristall", (0.4, 0.85, 1.0), emission=0.7),
       "hood": L.cel("GM_Kapuze", (0.16, 0.12, 0.2)), "mask": L.cel("GM_Maske", (0.08, 0.07, 0.09)),
       "belt_r": L.cel("GM_GuertelRot", (0.6, 0.1, 0.15)), "blade": L.cel("GM_Klinge", (0.75, 0.78, 0.85), light=1.1)}

count = 0
warnings = 0
for name, v in MASTERS.items():
    for mat, rgb in v["colors"].items():
        L.cel(mat, rgb)   # gleicher Name -> Material der Figur wird umgefaerbt
    parts = [L.shape(kind, loc, sc, (0, 0, 0), ACC[m], bone, collection=scene.collection) for kind, bone, m, loc, sc in v["parts"]]
    acc = L.join(parts, "GM_" + name)
    L.attach_to_rig(acc, rig)
    L.subdivide(acc, 1)
    L.add_outline(acc, 0.016)
    for i, f in enumerate(FRAMES):
        L.set_frame(scene, f)
        path = os.path.join(OUT, f"master_{name}_{i}.png")
        L.render_still(scene, path)
        count += 1
        if L.edge_pixels(path):
            warnings += 1
            print("WARNUNG Randpixel:", os.path.basename(path))
    bpy.data.objects.remove(acc, do_unlink=True)

print(f"RESULT rendered={count} randwarnungen={warnings}")
# absichtlich kein save_as_mainfile: die Spielfigur in Reborn_Characters.blend bleibt wie sie ist
