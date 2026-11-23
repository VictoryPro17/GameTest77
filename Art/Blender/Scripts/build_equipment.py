"""RE:BORN - Sichtbare Ausruestung und Item-Icons (v2, Meilenstein 5, Comic-Look)

Oeffnet Reborn_Characters.blend (nach build_player.py), baut 14 Ausruestungsteile am Spieler-Skelett
und rendert jedes Teil in allen Animationen (gleiche Bildpositionen wie der Koerper) und 4 Richtungen
als eigene Bildebene (128x128). Der Koerper ist dabei "Holdout": unsichtbar, verdeckt aber das Teil.
Danach Item-Icons (48x48) fuer alle 22 Gegenstaende.

Aufruf:  blender.exe -b <Reborn_Characters.blend> --python build_equipment.py -- <projektordner> [animation ...]
         (mit Animationsnamen werden nur diese Ebenen neu gerendert, z. B. "death")
"""
import bpy
import math
import os
import shutil
import sys
from mathutils import Vector

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import reborn_lib as L  # noqa: E402

argv = sys.argv[sys.argv.index("--") + 1:] if "--" in sys.argv else []
PROJECT = argv[0] if argv else os.path.abspath(os.path.join(os.path.dirname(__file__), "..", "..", ".."))
EQ_DIR = os.path.join(PROJECT, "Assets", "_Project", "Sprites", "Equipment")
ICON_DIR = os.path.join(PROJECT, "Assets", "_Project", "Sprites", "Items")
BLEND_PATH = os.path.join(PROJECT, "Art", "Blender", "Reborn_Characters.blend")
ICON_SIZE = 48

scene = bpy.data.scenes["Characters"]
rig = bpy.data.objects["PlayerRig"]
body = bpy.data.objects["Player"]
ANIM = {k: list(v) for k, v in scene["anim_frames"].items()}
# Animationen mit eigener Aktion pro Richtung ("Player_<anim>_<richtung>", von build_player.py)
DIR_ACTIONS = set(scene.get("dir_actions", []))
# Optional nur einzelne Animationen neu rendern:  -- <projektordner> death walk
ONLY_ANIMS = set(a for a in argv[1:] if not a.startswith("item:"))
# Optional nur einzelne Teile neu rendern (Meilenstein 7c):  -- <projektordner> item:gildenschwert item:gildenstab
ONLY_ITEMS = set(a[5:] for a in argv[1:] if a.startswith("item:"))
r = scene.render

for o in list(bpy.data.objects):
    if o.name.startswith(("EQ_", "ICON_")) or o.name == "IconCamera":
        bpy.data.objects.remove(o, do_unlink=True)

M = {
    "leather": L.cel("EQ_Leder", (0.55, 0.35, 0.18)),
    "leather_dark": L.cel("EQ_LederDunkel", (0.34, 0.2, 0.1)),
    "stitch": L.cel("EQ_Naht", (0.9, 0.78, 0.55)),
    "iron": L.cel("EQ_Eisen", (0.5, 0.53, 0.6), shadow=0.45, mid=0.75, light=1.05),
    "iron_dark": L.cel("EQ_EisenDunkel", (0.3, 0.32, 0.38), shadow=0.5),
    "iron_light": L.cel("EQ_EisenHell", (0.72, 0.76, 0.84), shadow=0.5, mid=0.8),
    "gold": L.cel("EQ_Gold", (1.0, 0.78, 0.28), light=1.15),
    "red": L.cel("EQ_Rot", (0.75, 0.13, 0.13)),
    "wood": L.cel("EQ_Holz", (0.66, 0.44, 0.22)),
    "wood_dark": L.cel("EQ_HolzDunkel", (0.4, 0.25, 0.12)),
    "orb": L.cel("EQ_Kristall", (0.4, 0.75, 1.0), shadow=0.8, emission=1.2),
    "gel": L.cel("ICON_Gel", (0.3, 0.82, 0.3), emission=0.2),
    "shine": L.cel("ICON_Glanz", (1, 1, 1), shadow=1, mid=1, emission=0.5),
    "potion_red": L.cel("ICON_TrankRot", (0.92, 0.15, 0.25), emission=0.3),
    "potion_blue": L.cel("ICON_TrankBlau", (0.2, 0.45, 1.0), emission=0.3),
    "glass": L.cel("ICON_Glas", (0.82, 0.92, 0.97)),
    "cork": L.cel("ICON_Korken", (0.66, 0.46, 0.27)),
    "copper": L.cel("ICON_Kupfer", (0.86, 0.5, 0.25), light=1.15),
    "gem_teal": L.cel("ICON_Tuerkis", (0.2, 0.85, 0.85), emission=0.6),
    "gem_blue": L.cel("ICON_Saphir", (0.15, 0.35, 1.0), emission=0.6),
    "tooth": L.cel("ICON_Zahn", (0.95, 0.92, 0.84)),
    # Meilenstein 7c: Gildenwaffen und neue Beute
    "arcane": L.cel("EQ_Arkan", (0.7, 0.35, 1.0), shadow=0.8, emission=1.2),
    "shadow": L.cel("EQ_Schatten", (0.22, 0.2, 0.28)),
    "fur": L.cel("ICON_Fell", (0.38, 0.38, 0.44)),
    "fur_dark": L.cel("ICON_FellDunkel", (0.22, 0.22, 0.28)),
    "silk": L.cel("ICON_Seide", (0.95, 0.95, 0.92), emission=0.1),
}


def part(kind, bone, mat, loc, scale, rot=(0, 0, 0), **kw):
    return L.shape(kind, loc, scale, rot, M[mat], bone, collection=scene.collection, **kw)


def pair(fn):
    """fn(seite, x, vorzeichen) -> Liste von Teilen; fuer links und rechts"""
    return fn("L", -1) + fn("R", 1)


def boots(mat_foot, mat_shaft, mat_cuff):
    return pair(lambda s, sg: [
        part("sphere", f"shin.{s}", mat_foot, (0.12 * sg, -0.035, 0.075), (0.093, 0.14, 0.072)),
        part("cube", f"shin.{s}", mat_foot, (0.12 * sg, -0.015, 0.025), (0.092, 0.14, 0.028)),
        part("cyl", f"shin.{s}", mat_shaft, (0.12 * sg, 0, 0.2), (0.09, 0.09, 0.1)),
        part("cyl", f"shin.{s}", mat_cuff, (0.12 * sg, 0, 0.31), (0.097, 0.097, 0.022))])


def gloves(mat, cuff):
    return pair(lambda s, sg: [
        part("sphere", f"forearm.{s}", mat, (0.32 * sg, -0.005, 0.89), (0.074, 0.068, 0.08)),
        part("cube", f"forearm.{s}", mat, (0.32 * sg, -0.03, 0.835), (0.058, 0.042, 0.035)),
        part("cyl", f"forearm.{s}", cuff, (0.315 * sg, 0, 0.975), (0.072, 0.072, 0.04), taper=1.15)])


def pants(mat, extra):
    return [part("cyl", "hips", mat, (0, 0, 0.875), (0.245, 0.178, 0.09))] + pair(lambda s, sg: [
        part("cyl", f"thigh.{s}", mat, (0.12 * sg, 0, 0.66), (0.102, 0.102, 0.19), taper=0.85),
        part("cyl", f"shin.{s}", mat, (0.12 * sg, 0, 0.36), (0.088, 0.088, 0.12), taper=0.9),
        part("sphere", f"shin.{s}", extra, (0.12 * sg, -0.07, 0.47), (0.07, 0.04, 0.06))])


def sword(blade, guard, grip, length):
    rot = (-60, 0, 0)
    return [part("cyl", "forearm.R", grip, (0.32, -0.02, 0.89), (0.022, 0.022, 0.07), rot=(30, 0, 0)),
            part("sphere", "forearm.R", guard, (0.32, 0.03, 0.86), (0.03, 0.03, 0.03)),
            part("cube", "forearm.R", guard, (0.32, -0.075, 0.935), (0.09, 0.02, 0.022), rot=rot),
            part("cube", "forearm.R", blade, (0.32, -0.08 - length * 0.85, 0.93 + length * 0.5), (0.03, 0.012, length), rot=rot, smooth=False),
            part("cube", "forearm.R", "iron_dark" if blade != "wood" else "wood_dark",
                 (0.32, -0.085 - length * 0.85, 0.935 + length * 0.5), (0.008, 0.014, length * 0.85), rot=rot, smooth=False),
            part("cone", "forearm.R", blade, (0.32, -0.075 - length * 1.72, 0.93 + length * 1.0), (0.03, 0.012, 0.05), rot=(-150, 0, 0))]


EQUIP = {
    "lederkappe": lambda: [part("sphere", "head", "leather", (0, 0.02, 1.69), (0.27, 0.26, 0.17), segments=24),
                           part("cyl", "head", "leather_dark", (0, 0.02, 1.61), (0.275, 0.265, 0.03)),
                           part("cyl", "head", "stitch", (0, 0.02, 1.64), (0.272, 0.262, 0.006)),
                           part("cube", "head", "leather_dark", (0, -0.245, 1.6), (0.14, 0.05, 0.012), rot=(-15, 0, 0))],
    "eisenhelm": lambda: ([part("sphere", "head", "iron", (0, 0.01, 1.68), (0.28, 0.27, 0.2), segments=24),
                           part("cyl", "head", "iron_dark", (0, 0.01, 1.6), (0.285, 0.275, 0.025)),
                           part("cube", "head", "iron", (0, -0.272, 1.57), (0.018, 0.014, 0.075)),
                           part("cube", "head", "red", (0, 0.03, 1.87), (0.025, 0.21, 0.05))]
                          + [part("sphere", "head", "gold", (0.27 * math.sin(a), 0.01 - 0.26 * math.cos(a), 1.6), (0.015, 0.015, 0.015))
                             for a in [math.radians(d) for d in (-60, -30, 30, 60)]]),
    "lederweste": lambda: ([part("cyl", "spine", "leather", (0, 0, 1.12), (0.235, 0.172, 0.2), taper=1.12),
                            part("sphere", "spine", "leather", (0, 0, 1.26), (0.265, 0.18, 0.11)),
                            part("cyl", "hips", "leather_dark", (0, 0, 0.93), (0.262, 0.19, 0.06), taper=0.97),
                            part("cyl", "spine", "leather_dark", (0, 0, 1.33), (0.13, 0.11, 0.03))]
                           + [part("cube", "spine", "stitch", (0, -0.175, 1.03 + 0.05 * i), (0.035, 0.006, 0.006), smooth=False) for i in range(5)]
                           + [part("sphere", f"upperarm.{s}", "leather_dark", (0.31 * sg, 0, 1.28), (0.095, 0.095, 0.06)) for s, sg in (("L", -1), ("R", 1))]),
    "eisenbrust": lambda: ([part("cyl", "spine", "iron", (0, 0, 1.12), (0.24, 0.178, 0.205), taper=1.12),
                            part("sphere", "spine", "iron", (0, 0, 1.265), (0.27, 0.185, 0.115)),
                            part("sphere", "spine", "gold", (0, -0.185, 1.19), (0.05, 0.012, 0.05)),
                            part("cyl", "hips", "iron_dark", (0, 0, 0.92), (0.265, 0.192, 0.07), taper=0.97),
                            part("cyl", "spine", "iron_dark", (0, 0, 1.34), (0.13, 0.11, 0.03))]
                           + [part("sphere", f"upperarm.{s}", "iron", (0.32 * sg, 0, 1.29), (0.11, 0.11, 0.08)) for s, sg in (("L", -1), ("R", 1))]
                           + [part("sphere", f"upperarm.{s}", "gold", (0.36 * sg, -0.06, 1.3), (0.018, 0.018, 0.018)) for s, sg in (("L", -1), ("R", 1))]),
    "lederhose": lambda: pants("leather", "leather_dark"),
    "eisenhose": lambda: pants("iron", "iron_dark"),
    "lederstiefel": lambda: boots("leather_dark", "leather", "stitch"),
    "eisenstiefel": lambda: boots("iron_dark", "iron", "gold"),
    "lederhandschuhe": lambda: gloves("leather", "leather_dark"),
    "eisenhandschuhe": lambda: gloves("iron", "iron_dark"),
    "holzschwert": lambda: sword("wood", "wood_dark", "leather_dark", 0.25),
    "eisenschwert": lambda: sword("iron_light", "gold", "leather_dark", 0.32),
    "eichenstab": lambda: [part("cyl", "forearm.R", "wood", (0.32, -0.02, 1.02), (0.026, 0.026, 0.6), taper=0.8),
                           part("sphere", "forearm.R", "wood_dark", (0.32, -0.02, 1.2), (0.035, 0.035, 0.03)),
                           part("sphere", "forearm.R", "wood_dark", (0.32, -0.02, 0.7), (0.035, 0.035, 0.03)),
                           part("sphere", "forearm.R", "orb", (0.32, -0.02, 1.68), (0.07, 0.07, 0.07)),
                           part("cone", "forearm.R", "gold", (0.29, -0.02, 1.63), (0.018, 0.018, 0.07), rot=(0, -20, 0)),
                           part("cone", "forearm.R", "gold", (0.35, -0.02, 1.63), (0.018, 0.018, 0.07), rot=(0, 20, 0))],
    "holzschild": lambda: ([part("cyl", "forearm.L", "wood", (-0.4, -0.02, 1.0), (0.21, 0.21, 0.025), rot=(0, 90, 0), segments=24),
                            part("torus", "forearm.L", "iron_dark", (-0.41, -0.02, 1.0), (0.21, 0.21, 0.13), rot=(0, 90, 0)),
                            part("sphere", "forearm.L", "iron", (-0.43, -0.02, 1.0), (0.05, 0.05, 0.05))]
                           + [part("cube", "forearm.L", "wood_dark", (-0.427, -0.02, 1.0 + dz), (0.003, 0.2, 0.006), smooth=False) for dz in (-0.1, 0.1)]),
    # ---- Meilenstein 7c: Gildenwaffen
    "gildenschwert": lambda: sword("iron_light", "gold", "red", 0.4) + [part("sphere", "forearm.R", "red", (0.32, 0.06, 0.84), (0.035, 0.035, 0.035))],
    "gildenstab": lambda: [part("cyl", "forearm.R", "shadow", (0.32, -0.02, 1.04), (0.028, 0.028, 0.64), taper=0.85),
                           part("torus", "forearm.R", "gold", (0.32, -0.02, 1.25), (0.045, 0.045, 0.045)),
                           part("torus", "forearm.R", "gold", (0.32, -0.02, 0.72), (0.045, 0.045, 0.045)),
                           part("sphere", "forearm.R", "arcane", (0.32, -0.02, 1.74), (0.085, 0.085, 0.085)),
                           part("cone", "forearm.R", "gold", (0.27, -0.02, 1.68), (0.02, 0.02, 0.09), rot=(0, -25, 0)),
                           part("cone", "forearm.R", "gold", (0.37, -0.02, 1.68), (0.02, 0.02, 0.09), rot=(0, 25, 0))],
    "schattendolche": lambda: pair(lambda s, sg: [
        part("cyl", f"forearm.{s}", "shadow", (0.32 * sg, -0.02, 0.89), (0.02, 0.02, 0.06), rot=(30, 0, 0)),
        part("cube", f"forearm.{s}", "red", (0.32 * sg, -0.06, 0.925), (0.05, 0.015, 0.015), rot=(-60, 0, 0)),
        part("cube", f"forearm.{s}", "iron_light", (0.32 * sg, -0.16, 0.99), (0.022, 0.01, 0.12), rot=(-60, 0, 0), smooth=False),
        part("cone", f"forearm.{s}", "iron_light", (0.32 * sg, -0.27, 1.05), (0.022, 0.01, 0.04), rot=(-150, 0, 0))]),
}

equip = {}
for item, build in EQUIP.items():
    o = L.join(build(), "EQ_" + item)
    L.attach_to_rig(o, rig)
    L.subdivide(o, 1)
    L.add_outline(o, 0.014)
    o.hide_render = True
    equip[item] = o

# ---------------- Ebenen rendern (Koerper = Holdout, gleiche Bildpositionen wie der Koerper)
body.is_holdout = True
rendered = 0
for item, obj in equip.items():
    if ONLY_ITEMS and item not in ONLY_ITEMS:
        continue
    out = os.path.join(EQ_DIR, item)
    os.makedirs(out, exist_ok=True)
    obj.hide_render = False
    for anim, frames in ANIM.items():
        if ONLY_ANIMS and anim not in ONLY_ANIMS:
            continue
        L.use_action(rig, bpy.data.actions["Player_" + anim] if anim not in DIR_ACTIONS else None)
        for direction, angle in L.DIRECTIONS.items():
            if anim in DIR_ACTIONS:   # eigene Fassung pro Richtung (z. B. Tod, siehe build_player.py)
                L.use_action(rig, bpy.data.actions[f"Player_{anim}_{direction}"])
            rig.rotation_euler = (0, 0, math.radians(angle))
            for i, f in enumerate(frames):
                L.set_frame(scene, f)
                L.render_still(scene, os.path.join(out, f"{anim}_{direction}_{i}.png"))
                rendered += 1
    obj.hide_render = True
body.is_holdout = False
rig.rotation_euler = (0, 0, 0)
L.use_action(rig, bpy.data.actions["Player_idle"])
scene.frame_set(1)

# ---------------- Icon-Modelle ohne Skelett
ICON_ONLY = {
    "schleimgel": [("sphere", "gel", (0, 0, 0), (0.3, 0.3, 0.2)), ("sphere", "shine", (-0.1, -0.24, 0.08), (0.06, 0.03, 0.04))],
    "heiltrank_klein": [("sphere", "potion_red", (0, 0, 0), (0.22, 0.22, 0.22)), ("cyl", "glass", (0, 0, 0.26), (0.07, 0.07, 0.07)),
                        ("cyl", "cork", (0, 0, 0.35), (0.06, 0.06, 0.04)), ("sphere", "shine", (-0.08, -0.19, 0.08), (0.04, 0.02, 0.05))],
    "heiltrank": [("sphere", "potion_red", (0, 0, 0), (0.28, 0.28, 0.3)), ("cyl", "glass", (0, 0, 0.33), (0.08, 0.08, 0.1)),
                  ("cyl", "cork", (0, 0, 0.46), (0.07, 0.07, 0.05)), ("sphere", "shine", (-0.1, -0.24, 0.1), (0.05, 0.03, 0.07)),
                  ("torus", "gold", (0, 0, 0.25), (0.09, 0.09, 0.09))],
    "manatrank_klein": [("sphere", "potion_blue", (0, 0, 0), (0.22, 0.22, 0.22)), ("cyl", "glass", (0, 0, 0.26), (0.07, 0.07, 0.07)),
                        ("cyl", "cork", (0, 0, 0.35), (0.06, 0.06, 0.04)), ("sphere", "shine", (-0.08, -0.19, 0.08), (0.04, 0.02, 0.05))],
    "manatrank": [("sphere", "potion_blue", (0, 0, 0), (0.28, 0.28, 0.3)), ("cyl", "glass", (0, 0, 0.33), (0.08, 0.08, 0.1)),
                  ("cyl", "cork", (0, 0, 0.46), (0.07, 0.07, 0.05)), ("sphere", "shine", (-0.1, -0.24, 0.1), (0.05, 0.03, 0.07)),
                  ("torus", "gold", (0, 0, 0.25), (0.09, 0.09, 0.09))],
    "kupferring": [("torus", "copper", (0, 0, 0), (0.25, 0.25, 0.25), (90, 0, 0)), ("sphere", "gem_teal", (0, -0.02, 0.3), (0.08, 0.08, 0.08))],
    "saphirring": [("torus", "gold", (0, 0, 0), (0.25, 0.25, 0.25), (90, 0, 0)), ("sphere", "gem_blue", (0, -0.02, 0.31), (0.1, 0.1, 0.1))],
    "wolfszahnkette": [("torus", "iron_dark", (0, 0, 0.12), (0.3, 0.3, 0.3), (90, 0, 0)),
                       ("cone", "tooth", (0, -0.02, -0.28), (0.07, 0.07, 0.14), (180, 0, 0)),
                       ("cone", "tooth", (-0.12, -0.02, -0.2), (0.04, 0.04, 0.09), (180, 0, 30)),
                       ("cone", "tooth", (0.12, -0.02, -0.2), (0.04, 0.04, 0.09), (180, 0, -30))],
    # ---- Meilenstein 7c: Beute von Wolf und Spinne
    "wolfsfell": [("sphere", "fur", (0, 0, 0), (0.34, 0.26, 0.06)), ("sphere", "fur_dark", (0.12, -0.05, 0.04), (0.1, 0.08, 0.03)),
                  ("sphere", "fur_dark", (-0.14, 0.06, 0.04), (0.08, 0.07, 0.03)), ("cone", "fur", (-0.3, 0.18, 0), (0.08, 0.05, 0.06), (0, 0, 40)),
                  ("cone", "fur", (0.3, 0.18, 0), (0.08, 0.05, 0.06), (0, 0, -40))],
    "spinnenseide": [("cyl", "wood", (0, 0, 0), (0.09, 0.09, 0.3)), ("cyl", "silk", (0, 0, 0), (0.22, 0.22, 0.22)),
                     ("torus", "silk", (0, 0, 0.1), (0.23, 0.23, 0.23)), ("cyl", "wood_dark", (0, 0, 0.3), (0.16, 0.16, 0.03)),
                     ("cyl", "wood_dark", (0, 0, -0.3), (0.16, 0.16, 0.03))],
}
icon_only = {}
for item, parts in ICON_ONLY.items():
    objs = [L.shape(k, loc, sc, (rot[0] if rot else (0, 0, 0)), M[m], None, collection=scene.collection)
            for k, m, loc, sc, *rot in parts]
    o = L.join(objs, "ICON_" + item)
    L.subdivide(o, 1)
    L.add_outline(o, 0.012)
    o.hide_render = True
    icon_only[item] = o

# ---------------- Icons (48 x 48), jedes Teil einzeln, Koerper ausgeblendet
old_cam = scene.camera
old_res = (r.resolution_x, r.resolution_y)
cd = bpy.data.cameras.new("IconCamera"); cd.type = "ORTHO"
icam = bpy.data.objects.new("IconCamera", cd); scene.collection.objects.link(icam)
scene.camera = icam
r.resolution_x = r.resolution_y = ICON_SIZE
body.hide_render = True
VIEW = {"holzschwert": "right", "eisenschwert": "right", "eichenstab": "right", "holzschild": "left", "gildenschwert": "right", "gildenstab": "right"}
PAIRS = {"lederhandschuhe", "eisenhandschuhe", "lederstiefel", "eisenstiefel", "schattendolche"}
R90 = math.radians(90)
icons = 0
for item, obj in list(equip.items()) + list(icon_only.items()):
    bpy.context.view_layer.update()
    ev = obj.evaluated_get(bpy.context.evaluated_depsgraph_get())
    me = ev.to_mesh()
    pts = [ev.matrix_world @ v.co for v in me.vertices]
    ev.to_mesh_clear()
    if item in PAIRS:
        pts = [p for p in pts if p.x > 0] or pts
    lo = Vector((min(p.x for p in pts), min(p.y for p in pts), min(p.z for p in pts)))
    hi = Vector((max(p.x for p in pts), max(p.y for p in pts), max(p.z for p in pts)))
    c, size = (lo + hi) / 2, hi - lo
    view = VIEW.get(item, "front")
    if view == "front":
        icam.location = (c.x, c.y - 10, c.z + 1.2); icam.rotation_euler = (math.radians(83), 0, 0); span = max(size.x, size.z)
    elif view == "right":
        icam.location = (c.x + 10, c.y, c.z); icam.rotation_euler = (R90, 0, R90); span = max(size.y, size.z)
    else:
        icam.location = (c.x - 10, c.y, c.z); icam.rotation_euler = (R90, 0, -R90); span = max(size.y, size.z)
    cd.ortho_scale = max(0.2, span * (0.95 if view == "right" else 1.2))
    obj.hide_render = False
    L.render_still(scene, os.path.join(ICON_DIR, f"Item_{item}.png"))
    obj.hide_render = True
    icons += 1

body.hide_render = False
scene.camera = old_cam
r.resolution_x, r.resolution_y = old_res
bpy.ops.wm.save_as_mainfile(filepath=BLEND_PATH)
print(f"RESULT layers={rendered} icons={icons}")
