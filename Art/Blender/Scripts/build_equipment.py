"""RE:BORN - Sichtbare Ausruestung und Item-Icons (Meilenstein 4)

Oeffnet Reborn_Characters.blend (nach build_player.py), baut 14 Ausruestungsteile am Spieler-Skelett
und rendert jedes Teil in allen Animationen und 4 Richtungen als eigene Bildebene (96x96).
Der Koerper ist dabei "Holdout": unsichtbar, verdeckt aber das Teil, damit vorne/hinten stimmt.
Danach Item-Icons (32x32) fuer alle Gegenstaende.

Aufruf:  blender.exe -b <Reborn_Characters.blend> --python build_equipment.py -- <projektordner>
"""
import bpy
import bmesh
import math
import os
import sys
from mathutils import Vector

argv = sys.argv[sys.argv.index("--") + 1:] if "--" in sys.argv else []
PROJECT = argv[0] if argv else os.path.abspath(os.path.join(os.path.dirname(__file__), "..", "..", ".."))
EQ_DIR = os.path.join(PROJECT, "Assets", "_Project", "Sprites", "Equipment")
ICON_DIR = os.path.join(PROJECT, "Assets", "_Project", "Sprites", "Items")
BLEND_PATH = os.path.join(PROJECT, "Art", "Blender", "Reborn_Characters.blend")
DIRECTIONS = {"down": 0.0, "right": 90.0, "up": 180.0, "left": -90.0}

scene = bpy.data.scenes["Characters"]
if bpy.context.window:
    bpy.context.window.scene = scene
rig = bpy.data.objects["PlayerRig"]
body = bpy.data.objects["Player"]
ANIM_FRAMES = dict(scene["anim_frames"])
r = scene.render

# Alte Ausruestung und Icon-Modelle entfernen
for o in list(bpy.data.objects):
    if o.name.startswith(("EQ_", "ICON_")) or o.name == "IconCamera":
        bpy.data.objects.remove(o, do_unlink=True)


def mat(name, rgb, metallic=0.0, rough=1.0, emission=0.0):
    m = bpy.data.materials.get(name) or bpy.data.materials.new(name)
    m.use_nodes = True
    p = m.node_tree.nodes.get("Principled BSDF")
    p.inputs["Base Color"].default_value = (*rgb, 1.0)
    p.inputs["Metallic"].default_value = metallic
    p.inputs["Roughness"].default_value = rough
    if "Emission Color" in p.inputs:
        p.inputs["Emission Color"].default_value = (*rgb, 1.0)
        p.inputs["Emission Strength"].default_value = emission
    return m


M = {
    "leather": mat("EQ_Leder", (0.47, 0.30, 0.15)),
    "leather_dark": mat("EQ_LederDunkel", (0.30, 0.18, 0.09)),
    "iron": mat("EQ_Eisen", (0.60, 0.62, 0.67), 0.7, 0.45),
    "iron_dark": mat("EQ_EisenDunkel", (0.36, 0.37, 0.41), 0.6, 0.5),
    "iron_light": mat("EQ_EisenHell", (0.86, 0.88, 0.92), 0.8, 0.3),
    "gold": mat("EQ_Gold", (0.95, 0.72, 0.25), 0.8, 0.35),
    "red": mat("EQ_Rot", (0.62, 0.10, 0.10)),
    "wood": mat("EQ_Holz", (0.58, 0.38, 0.18)),
    "wood_dark": mat("EQ_HolzDunkel", (0.34, 0.21, 0.10)),
    "orb": mat("EQ_Kristall", (0.35, 0.65, 1.0), 0.0, 0.3, 2.0),
    "gel": mat("ICON_Gel", (0.25, 0.75, 0.25), 0.0, 0.3, 0.3),
    "foam": mat("ICON_Glanz", (0.95, 1.0, 0.95), 0.0, 0.2, 0.5),
    "potion_red": mat("ICON_TrankRot", (0.85, 0.12, 0.2), 0.0, 0.2, 0.6),
    "potion_blue": mat("ICON_TrankBlau", (0.15, 0.4, 0.95), 0.0, 0.2, 0.6),
    "glass": mat("ICON_Glas", (0.8, 0.9, 0.95), 0.0, 0.1, 0.2),
    "cork": mat("ICON_Korken", (0.6, 0.42, 0.25)),
    "copper": mat("ICON_Kupfer", (0.8, 0.45, 0.22), 0.8, 0.35),
    "gem_teal": mat("ICON_Tuerkis", (0.2, 0.75, 0.8), 0.0, 0.2, 0.8),
    "gem_blue": mat("ICON_Saphir", (0.15, 0.3, 0.95), 0.0, 0.2, 0.8),
    "tooth": mat("ICON_Zahn", (0.93, 0.9, 0.82)),
}
R60 = math.radians(-60)
R90 = math.radians(90)


def prim(kind, loc, scale, rot):
    if kind == "sphere":
        bpy.ops.mesh.primitive_uv_sphere_add(segments=16, ring_count=8, location=loc, rotation=rot)
    elif kind == "cyl":
        bpy.ops.mesh.primitive_cylinder_add(vertices=16, location=loc, rotation=rot)
    elif kind == "cone":
        bpy.ops.mesh.primitive_cone_add(vertices=12, location=loc, rotation=rot)
    elif kind == "torus":
        bpy.ops.mesh.primitive_torus_add(major_segments=24, minor_segments=8, location=loc, rotation=rot, major_radius=1.0, minor_radius=0.18)
    else:
        bpy.ops.mesh.primitive_cube_add(location=loc, rotation=rot)
    o = bpy.context.active_object
    o.scale = scale
    bpy.ops.object.transform_apply(location=False, rotation=True, scale=True)
    return o


def build(name, parts, rigged=True):
    """parts: (kind, bone, material, loc, scale[, rot]) -> ein Objekt (am Skelett, wenn rigged)"""
    bpy.ops.object.mode_set(mode="OBJECT") if bpy.context.object and bpy.context.object.mode != "OBJECT" else None
    objs = []
    for p in parts:
        kind, bone, material, loc, scale = p[:5]
        rot = p[5] if len(p) > 5 else (0, 0, 0)
        o = prim(kind, loc, scale, rot)
        o.data.materials.append(M[material])
        if rigged:
            vg = o.vertex_groups.new(name=bone)
            vg.add(list(range(len(o.data.vertices))), 1.0, "REPLACE")
        objs.append(o)
    bpy.ops.object.select_all(action="DESELECT")
    for o in objs:
        o.select_set(True)
    bpy.context.view_layer.objects.active = objs[0]
    if len(objs) > 1:
        bpy.ops.object.join()
    o = bpy.context.active_object
    o.name = name
    bpy.ops.object.shade_flat()
    if rigged:
        o.parent = rig
        mod = o.modifiers.new("Armature", "ARMATURE")
        mod.object = rig
    o.hide_render = True
    return o


def legs(kind_parts):
    out = []
    for side, x in (("L", -0.12), ("R", 0.12)):
        for p in kind_parts:
            kind, material, loc, scale = p
            out.append((kind, f"leg.{side}", material, (x + loc[0], loc[1], loc[2]), scale))
    return out


def arms(kind_parts):
    out = []
    for side, x in (("L", -0.31), ("R", 0.31)):
        for p in kind_parts:
            kind, material, loc, scale = p
            out.append((kind, f"arm.{side}", material, (x + loc[0], loc[1], loc[2]), scale))
    return out


EQUIP = {
    "lederkappe": [("sphere", "head", "leather", (0, 0.02, 1.67), (0.262, 0.252, 0.16)),
                   ("cyl", "head", "leather_dark", (0, 0.02, 1.6), (0.262, 0.252, 0.028))],
    "eisenhelm": [("sphere", "head", "iron", (0, 0.01, 1.66), (0.268, 0.258, 0.19)),
                  ("cyl", "head", "iron_dark", (0, 0.01, 1.585), (0.272, 0.262, 0.022)),
                  ("cube", "head", "iron", (0, -0.262, 1.56), (0.016, 0.012, 0.07)),
                  ("cube", "head", "red", (0, 0.02, 1.84), (0.022, 0.2, 0.035))],
    "lederweste": [("cyl", "spine", "leather", (0, 0, 1.12), (0.242, 0.172, 0.205)),
                   ("sphere", "spine", "leather", (0, 0, 1.255), (0.262, 0.178, 0.118)),
                   ("cube", "spine", "leather_dark", (0, -0.178, 1.13), (0.01, 0.006, 0.15)),
                   ("cyl", "hips", "leather_dark", (0, 0, 0.9), (0.25, 0.18, 0.06))],
    "eisenbrust": [("cyl", "spine", "iron", (0, 0, 1.12), (0.248, 0.178, 0.21)),
                   ("sphere", "spine", "iron", (0, 0, 1.26), (0.268, 0.182, 0.12)),
                   ("sphere", "spine", "gold", (0, -0.182, 1.18), (0.05, 0.012, 0.05)),
                   ("sphere", "arm.L", "iron", (-0.31, 0, 1.28), (0.1, 0.1, 0.075)),
                   ("sphere", "arm.R", "iron", (0.31, 0, 1.28), (0.1, 0.1, 0.075)),
                   ("cyl", "hips", "iron_dark", (0, 0, 0.9), (0.255, 0.185, 0.065))],
    "lederhose": [("cyl", "hips", "leather", (0, 0, 0.86), (0.248, 0.178, 0.095))]
                 + legs([("cyl", "leather", (0, 0, 0.62), (0.098, 0.098, 0.2)), ("cyl", "leather", (0, 0, 0.33), (0.084, 0.084, 0.17))]),
    "eisenhose": [("cyl", "hips", "iron_dark", (0, 0, 0.86), (0.25, 0.18, 0.095))]
                 + legs([("cyl", "iron", (0, 0, 0.62), (0.1, 0.1, 0.2)), ("cyl", "iron", (0, 0, 0.33), (0.086, 0.086, 0.17)),
                         ("sphere", "iron_dark", (0, -0.07, 0.46), (0.07, 0.04, 0.06))]),
    "lederstiefel": legs([("cube", "leather_dark", (0, -0.035, 0.065), (0.092, 0.138, 0.068)), ("cyl", "leather", (0, 0, 0.19), (0.088, 0.088, 0.1))]),
    "eisenstiefel": legs([("cube", "iron_dark", (0, -0.035, 0.065), (0.094, 0.14, 0.07)), ("cyl", "iron", (0, 0, 0.19), (0.09, 0.09, 0.1))]),
    "lederhandschuhe": arms([("sphere", "leather", (0, 0, 0.9), (0.074, 0.068, 0.08)), ("cyl", "leather_dark", (0, 0, 0.98), (0.07, 0.07, 0.035))]),
    "eisenhandschuhe": arms([("sphere", "iron", (0, 0, 0.9), (0.076, 0.07, 0.082)), ("cyl", "iron_dark", (0, 0, 0.98), (0.072, 0.072, 0.04))]),
    "holzschwert": [("cube", "arm.R", "wood_dark", (0.31, -0.02, 0.9), (0.03, 0.1, 0.03)),
                    ("cube", "arm.R", "wood_dark", (0.31, -0.12, 0.95), (0.08, 0.02, 0.02), (R60, 0, 0)),
                    ("cube", "arm.R", "wood", (0.31, -0.33, 1.08), (0.028, 0.022, 0.28), (R60, 0, 0))],
    "eisenschwert": [("cube", "arm.R", "wood_dark", (0.31, -0.02, 0.9), (0.03, 0.1, 0.03)),
                     ("cube", "arm.R", "gold", (0.31, -0.12, 0.95), (0.09, 0.02, 0.022), (R60, 0, 0)),
                     ("cube", "arm.R", "iron_light", (0.31, -0.36, 1.1), (0.026, 0.02, 0.33), (R60, 0, 0))],
    "eichenstab": [("cyl", "arm.R", "wood", (0.31, -0.02, 1.02), (0.025, 0.025, 0.6)),
                   ("sphere", "arm.R", "orb", (0.31, -0.02, 1.66), (0.065, 0.065, 0.065)),
                   ("cone", "arm.R", "wood_dark", (0.31, -0.02, 1.58), (0.06, 0.06, 0.06))],
    "holzschild": [("cyl", "arm.L", "wood", (-0.39, -0.02, 1.0), (0.2, 0.2, 0.025), (0, R90, 0)),
                   ("cyl", "arm.L", "iron_dark", (-0.385, -0.02, 1.0), (0.215, 0.215, 0.018), (0, R90, 0)),
                   ("sphere", "arm.L", "iron", (-0.42, -0.02, 1.0), (0.05, 0.05, 0.05))],
}

equip_objs = {k: build("EQ_" + k, v) for k, v in EQUIP.items()}

# Icon-Modelle fuer Gegenstaende ohne Ausruestungs-Modell (stehen am Ursprung, ohne Skelett)
ICON_ONLY = {
    "schleimgel": [("sphere", "", "gel", (0, 0, 0), (0.3, 0.3, 0.2)), ("sphere", "", "foam", (-0.1, -0.22, 0.08), (0.06, 0.03, 0.04))],
    "heiltrank_klein": [("sphere", "", "potion_red", (0, 0, 0), (0.22, 0.22, 0.22)), ("cyl", "", "glass", (0, 0, 0.24), (0.07, 0.07, 0.07)),
                        ("cyl", "", "cork", (0, 0, 0.33), (0.06, 0.06, 0.04))],
    "heiltrank": [("sphere", "", "potion_red", (0, 0, 0), (0.28, 0.28, 0.3)), ("cyl", "", "glass", (0, 0, 0.32), (0.08, 0.08, 0.1)),
                  ("cyl", "", "cork", (0, 0, 0.45), (0.07, 0.07, 0.05)), ("sphere", "", "foam", (-0.1, -0.24, 0.1), (0.05, 0.03, 0.07))],
    "manatrank_klein": [("sphere", "", "potion_blue", (0, 0, 0), (0.22, 0.22, 0.22)), ("cyl", "", "glass", (0, 0, 0.24), (0.07, 0.07, 0.07)),
                        ("cyl", "", "cork", (0, 0, 0.33), (0.06, 0.06, 0.04))],
    "manatrank": [("sphere", "", "potion_blue", (0, 0, 0), (0.28, 0.28, 0.3)), ("cyl", "", "glass", (0, 0, 0.32), (0.08, 0.08, 0.1)),
                  ("cyl", "", "cork", (0, 0, 0.45), (0.07, 0.07, 0.05)), ("sphere", "", "foam", (-0.1, -0.24, 0.1), (0.05, 0.03, 0.07))],
    "kupferring": [("torus", "", "copper", (0, 0, 0), (0.25, 0.25, 0.25), (R90, 0, 0)), ("sphere", "", "gem_teal", (0, -0.02, 0.29), (0.08, 0.08, 0.08))],
    "saphirring": [("torus", "", "gold", (0, 0, 0), (0.25, 0.25, 0.25), (R90, 0, 0)), ("sphere", "", "gem_blue", (0, -0.02, 0.3), (0.1, 0.1, 0.1))],
    "wolfszahnkette": [("torus", "", "iron_dark", (0, 0, 0.12), (0.3, 0.3, 0.3), (R90, 0, 0)),
                       ("cone", "", "tooth", (0, -0.02, -0.28), (0.07, 0.07, 0.14), (math.radians(180), 0, 0))],
}
icon_objs = {k: build("ICON_" + k, v, rigged=False) for k, v in ICON_ONLY.items()}


def set_action(anim):
    act = bpy.data.actions["Player_" + anim]
    ad = rig.animation_data or rig.animation_data_create()
    ad.action = act
    if hasattr(ad, "action_slot") and ad.action_slot is None and len(getattr(act, "slots", [])) > 0:
        ad.action_slot = act.slots[0]


# ---------------- Ebenen rendern (Koerper = Holdout)
body.is_holdout = True
rendered = 0
for item, obj in equip_objs.items():
    out = os.path.join(EQ_DIR, item)
    os.makedirs(out, exist_ok=True)
    obj.hide_render = False
    for anim, count in ANIM_FRAMES.items():
        set_action(anim)
        for direction, angle in DIRECTIONS.items():
            rig.rotation_euler = (0, 0, math.radians(angle))
            for i in range(count):
                scene.frame_set(i + 1)
                r.filepath = os.path.join(out, f"{anim}_{direction}_{i}.png")
                bpy.ops.render.render(write_still=True, scene=scene.name)
                rendered += 1
    obj.hide_render = True
body.is_holdout = False
rig.rotation_euler = (0, 0, 0)
set_action("idle")
scene.frame_set(1)

# ---------------- Icons (32 x 32), jedes Teil einzeln, Koerper ausgeblendet
old_cam = scene.camera
old_res = (r.resolution_x, r.resolution_y)
cam_data = bpy.data.cameras.new("IconCamera"); cam_data.type = "ORTHO"
icam = bpy.data.objects.new("IconCamera", cam_data); scene.collection.objects.link(icam)
scene.camera = icam
r.resolution_x = r.resolution_y = 32
body.hide_render = True
VIEW = {"holzschwert": "right", "eisenschwert": "right", "eichenstab": "right", "holzschild": "left"}
PAIRS = {"lederhandschuhe", "eisenhandschuhe", "lederstiefel", "eisenstiefel"}
# Lange, duenne Teile fuellen das Icon staerker aus
TIGHT = {"holzschwert": 0.9, "eisenschwert": 0.9, "eichenstab": 0.85}

icons = 0
for item, obj in list(equip_objs.items()) + list(icon_objs.items()):
    bpy.context.view_layer.update()
    deps = bpy.context.evaluated_depsgraph_get()
    ev = obj.evaluated_get(deps)
    if item in PAIRS:
        # Paarweise Teile (Handschuhe, Stiefel): nur das rechte Teil ins Bild nehmen
        me = ev.to_mesh()
        pts = [ev.matrix_world @ v.co for v in me.vertices]
        pts = [p for p in pts if p.x > 0] or pts
        ev.to_mesh_clear()
    else:
        pts = [ev.matrix_world @ Vector(c) for c in ev.bound_box]
    lo = Vector((min(p.x for p in pts), min(p.y for p in pts), min(p.z for p in pts)))
    hi = Vector((max(p.x for p in pts), max(p.y for p in pts), max(p.z for p in pts)))
    center = (lo + hi) / 2
    size = hi - lo
    view = VIEW.get(item, "front")
    if view == "front":
        icam.location = (center.x, center.y - 10, center.z); icam.rotation_euler = (R90, 0, 0); span = max(size.x, size.z)
    elif view == "right":
        icam.location = (center.x + 10, center.y, center.z); icam.rotation_euler = (R90, 0, R90); span = max(size.y, size.z)
    else:
        icam.location = (center.x - 10, center.y, center.z); icam.rotation_euler = (R90, 0, -R90); span = max(size.y, size.z)
    cam_data.ortho_scale = max(0.2, span * TIGHT.get(item, 1.2))
    obj.hide_render = False
    r.filepath = os.path.join(ICON_DIR, f"Item_{item}.png")
    bpy.ops.render.render(write_still=True, scene=scene.name)
    obj.hide_render = True
    icons += 1

body.hide_render = False
scene.camera = old_cam
r.resolution_x, r.resolution_y = old_res
bpy.ops.wm.save_as_mainfile(filepath=BLEND_PATH)
print(f"RESULT layers={rendered} icons={icons}")
