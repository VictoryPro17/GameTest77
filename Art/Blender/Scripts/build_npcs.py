"""RE:BORN - NPCs der Basis (Meilenstein 6, Comic-Look)

Haendler und Schmied auf Basis der Spielfigur (gleiches Skelett, gleiche Warte-Animation),
mit eigenen Farben und Zubehoer (Haendler: Hut, Tasche; Schmied: Schuerze, Bart, Stirnband).
Rendert idle nach unten (4 Bilder, 128 px) nach Sprites/Base/npc_<name>_<i>.png.
Die Datei Reborn_Characters.blend wird NICHT gespeichert (Spielfigur bleibt unveraendert).

Aufruf:  blender.exe -b Art/Blender/Reborn_Characters.blend --python build_npcs.py -- <projektordner>
"""
import bpy
import os
import sys

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import reborn_lib as L  # noqa: E402

argv = sys.argv[sys.argv.index("--") + 1:] if "--" in sys.argv else []
PROJECT = argv[0] if argv else os.path.abspath(os.path.join(os.path.dirname(__file__), "..", "..", ".."))
OUT = os.path.join(PROJECT, "Assets", "_Project", "Sprites", "Base")
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

NPCS = {
    "haendler": {
        "colors": {"Tunika": (0.2, 0.55, 0.3), "TunikaDunkel": (0.1, 0.32, 0.17), "Umhang": (0.45, 0.3, 0.15),
                   "UmhangInnen": (0.28, 0.18, 0.08), "Haare": (0.2, 0.2, 0.22), "HaareHell": (0.35, 0.35, 0.38),
                   "Hose": (0.4, 0.33, 0.22), "Iris": (0.35, 0.25, 0.1)},
        "parts": [("cyl", "head", "hat", (0, 0.02, 1.78), (0.36, 0.34, 0.025)),          # Hutkrempe
                  ("cyl", "head", "hat", (0, 0.02, 1.87), (0.2, 0.19, 0.09)),             # Hutkopf
                  ("cyl", "head", "band", (0, 0.02, 1.81), (0.205, 0.195, 0.02)),        # Hutband
                  ("sphere", "head", "feather", (0.17, 0.05, 1.93), (0.03, 0.03, 0.1)),  # Feder
                  ("cube", "hips", "bag", (-0.22, -0.08, 0.9), (0.08, 0.05, 0.09)),      # Geldbeutel
                  ("sphere", "hips", "coin", (-0.22, -0.135, 0.9), (0.03, 0.01, 0.03))],
    },
    "schmied": {
        "colors": {"Tunika": (0.32, 0.3, 0.3), "TunikaDunkel": (0.2, 0.19, 0.19), "Umhang": (0.25, 0.22, 0.2),
                   "UmhangInnen": (0.15, 0.13, 0.12), "Haare": (0.55, 0.22, 0.1), "HaareHell": (0.7, 0.32, 0.14),
                   "Haut": (0.86, 0.62, 0.48), "Hose": (0.25, 0.2, 0.17), "Iris": (0.2, 0.45, 0.3)},
        "parts": [("cube", "spine", "apron", (0, -0.17, 1.08), (0.2, 0.02, 0.26)),         # Schuerze Brust
                  ("cube", "hips", "apron", (0, -0.2, 0.78), (0.23, 0.02, 0.2)),          # Schuerze unten
                  ("cyl", "hips", "strap", (0, 0, 1.0), (0.245, 0.18, 0.025)),            # Schuerzenband
                  ("sphere", "head", "beard", (0, -0.17, 1.43), (0.17, 0.09, 0.12)),      # Bart
                  ("cyl", "head", "band", (0, 0, 1.69), (0.25, 0.24, 0.025))],            # Stirnband
    },
}
ACC = {"hat": L.cel("NPC_Hut", (0.45, 0.16, 0.18)), "band": L.cel("NPC_Band", (0.95, 0.78, 0.3), light=1.1),
       "feather": L.cel("NPC_Feder", (0.95, 0.95, 0.9)), "bag": L.cel("NPC_Beutel", (0.5, 0.32, 0.14)),
       "coin": L.cel("NPC_Muenze", (1.0, 0.82, 0.3), emission=0.2), "apron": L.cel("NPC_Schuerze", (0.42, 0.26, 0.13)),
       "strap": L.cel("NPC_Band2", (0.3, 0.18, 0.09)), "beard": L.cel("NPC_Bart", (0.55, 0.22, 0.1))}

count = 0
warnings = 0
for name, npc in NPCS.items():
    for mat, rgb in npc["colors"].items():
        L.cel(mat, rgb)   # gleicher Name -> Material der Figur wird umgefaerbt
    parts = [L.shape(kind, loc, sc, (0, 0, 0), ACC[m], bone, collection=scene.collection) for kind, bone, m, loc, sc in npc["parts"]]
    acc = L.join(parts, "NPC_" + name)
    L.attach_to_rig(acc, rig)
    L.subdivide(acc, 1)
    L.add_outline(acc, 0.016)
    for i, f in enumerate(FRAMES):
        L.set_frame(scene, f)
        path = os.path.join(OUT, f"npc_{name}_{i}.png")
        L.render_still(scene, path)
        count += 1
        if L.edge_pixels(path):
            warnings += 1
            print("WARNUNG Randpixel:", os.path.basename(path))
    bpy.data.objects.remove(acc, do_unlink=True)

print(f"RESULT rendered={count} randwarnungen={warnings}")
# absichtlich kein save_as_mainfile: die Spielfigur in Reborn_Characters.blend bleibt wie sie ist
