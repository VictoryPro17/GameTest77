"""RE:BORN - Hauptcharakter (v2, Meilenstein 4)

Baut die detaillierte Grundfigur (ohne Waffe) + Skelett, legt die Animationen
idle/walk/attack/dodge/cast an und rendert jede Animation in 4 Richtungen als 96x96-Pixel-Sprites.
Die Ausruestung (sichtbare Ebenen) baut build_equipment.py am selben Skelett.

Aufruf (im Hintergrund):
  blender.exe -b --factory-startup --python build_player.py -- <projektordner>
"""
import bpy
import math
import os
import sys
from mathutils import Vector

# ---------------------------------------------------------------- Pfade
argv = sys.argv[sys.argv.index("--") + 1:] if "--" in sys.argv else []
PROJECT = argv[0] if argv else os.path.abspath(os.path.join(os.path.dirname(__file__), "..", "..", ".."))
OUT_DIR = os.path.join(PROJECT, "Assets", "_Project", "Sprites", "Player")
BLEND_PATH = os.path.join(PROJECT, "Art", "Blender", "Reborn_Characters.blend")
os.makedirs(OUT_DIR, exist_ok=True)

SPRITE_SIZE = 96
DIRECTIONS = {"down": 0.0, "right": 90.0, "up": 180.0, "left": -90.0}

# ---------------------------------------------------------------- Szene leeren
bpy.ops.wm.read_factory_settings(use_empty=True)
scene = bpy.context.scene
scene.name = "Characters"

# ---------------------------------------------------------------- Render
scene.render.engine = "BLENDER_EEVEE"
scene.render.resolution_x = SPRITE_SIZE
scene.render.resolution_y = SPRITE_SIZE
scene.render.resolution_percentage = 100
scene.render.film_transparent = True
scene.render.filter_size = 0.0
scene.render.image_settings.file_format = "PNG"
scene.render.image_settings.color_mode = "RGBA"
try:
    scene.eevee.taa_render_samples = 1
except AttributeError:
    pass
scene.view_settings.view_transform = "Standard"
scene.render.use_freestyle = False

world = bpy.data.worlds.new("World")
scene.world = world
world.use_nodes = True
bg = world.node_tree.nodes.get("Background")
bg.inputs["Color"].default_value = (0.55, 0.6, 0.7, 1.0)
bg.inputs["Strength"].default_value = 0.6

# ---------------------------------------------------------------- Kamera (60 Grad nach unten)
TARGET = Vector((0.0, 0.0, 0.85))
pitch = math.radians(30.0)  # 0 = senkrecht nach unten -> 30 = 60 Grad unter dem Horizont
forward = Vector((0.0, math.sin(pitch), -math.cos(pitch)))
cam_data = bpy.data.cameras.new("SpriteCamera")
cam_data.type = "ORTHO"
cam_data.ortho_scale = 2.1
cam = bpy.data.objects.new("SpriteCamera", cam_data)
scene.collection.objects.link(cam)
cam.location = TARGET - forward * 10.0
cam.rotation_euler = (pitch, 0.0, 0.0)
scene.camera = cam

sun_data = bpy.data.lights.new("Sun", "SUN")
sun_data.energy = 3.5
sun = bpy.data.objects.new("Sun", sun_data)
scene.collection.objects.link(sun)
sun.rotation_euler = (math.radians(40), math.radians(-25), math.radians(-30))

# ---------------------------------------------------------------- Materialien
def mat(name, rgb):
    m = bpy.data.materials.new(name)
    m.use_nodes = True
    p = m.node_tree.nodes.get("Principled BSDF")
    p.inputs["Base Color"].default_value = (*rgb, 1.0)
    p.inputs["Roughness"].default_value = 1.0
    if "Specular IOR Level" in p.inputs:
        p.inputs["Specular IOR Level"].default_value = 0.1
    return m

M = {
    "skin": mat("Haut", (0.93, 0.72, 0.58)),
    "skin_dark": mat("HautSchatten", (0.80, 0.56, 0.44)),
    "hair": mat("Haare", (0.30, 0.17, 0.09)),
    "hair_light": mat("HaareHell", (0.45, 0.27, 0.13)),
    "tunic": mat("Tunika", (0.20, 0.36, 0.62)),
    "tunic_dark": mat("TunikaDunkel", (0.13, 0.24, 0.45)),
    "pants": mat("Hose", (0.30, 0.24, 0.18)),
    "boots": mat("Schuhe", (0.20, 0.13, 0.08)),
    "belt": mat("Guertel", (0.45, 0.30, 0.12)),
    "gold": mat("Gold", (0.95, 0.72, 0.25)),
    "cloak": mat("Umhang", (0.50, 0.13, 0.13)),
    "eye_white": mat("Augenweiss", (0.96, 0.96, 0.94)),
    "eyes": mat("Augen", (0.10, 0.22, 0.45)),
    "mouth": mat("Mund", (0.45, 0.18, 0.16)),
}

# ---------------------------------------------------------------- Skelett
arm_data = bpy.data.armatures.new("PlayerRig")
rig = bpy.data.objects.new("PlayerRig", arm_data)
scene.collection.objects.link(rig)
bpy.context.view_layer.objects.active = rig
bpy.ops.object.mode_set(mode="EDIT")
BONES = {
    # name: (head, tail, parent)
    "root": ((0, 0, 0), (0, 0, 0.25), None),
    "hips": ((0, 0, 0.82), (0, 0, 1.0), "root"),
    "spine": ((0, 0, 1.0), (0, 0, 1.33), "hips"),
    "head": ((0, 0, 1.33), (0, 0, 1.78), "spine"),
    "arm.L": ((-0.30, 0, 1.30), (-0.32, 0, 0.92), "spine"),
    "arm.R": ((0.30, 0, 1.30), (0.32, 0, 0.92), "spine"),
    "leg.L": ((-0.12, 0, 0.82), (-0.12, 0, 0.08), "hips"),
    "leg.R": ((0.12, 0, 0.82), (0.12, 0, 0.08), "hips"),
}
for name, (h, t, parent) in BONES.items():
    b = arm_data.edit_bones.new(name)
    b.head, b.tail, b.roll = Vector(h), Vector(t), 0.0
for name, (_, _, parent) in BONES.items():
    if parent:
        arm_data.edit_bones[name].parent = arm_data.edit_bones[parent]
        arm_data.edit_bones[name].use_connect = False
bpy.ops.object.mode_set(mode="OBJECT")

# ---------------------------------------------------------------- Modell (Einzelteile -> ein Mesh)
parts = []

def add(kind, bone, material, loc, scale=(1, 1, 1), rot=(0, 0, 0), **kw):
    if kind == "sphere":
        bpy.ops.mesh.primitive_uv_sphere_add(segments=16, ring_count=8, location=loc, rotation=rot, **kw)
    elif kind == "cyl":
        bpy.ops.mesh.primitive_cylinder_add(vertices=12, location=loc, rotation=rot, **kw)
    else:
        bpy.ops.mesh.primitive_cube_add(location=loc, rotation=rot, **kw)
    o = bpy.context.active_object
    o.scale = scale
    bpy.ops.object.transform_apply(location=False, rotation=True, scale=True)
    o.data.materials.append(material)
    vg = o.vertex_groups.new(name=bone)
    vg.add(list(range(len(o.data.vertices))), 1.0, "REPLACE")
    parts.append(o)
    return o

# Beine: Oberschenkel, Unterschenkel, einfache Schuhe (Stiefel kommen als Ausruestung)
for side, x in (("L", -0.12), ("R", 0.12)):
    add("cyl", f"leg.{side}", M["pants"], (x, 0, 0.62), (0.092, 0.092, 0.2))
    add("cyl", f"leg.{side}", M["pants"], (x, 0, 0.3), (0.078, 0.078, 0.2))
    add("cube", f"leg.{side}", M["boots"], (x, -0.035, 0.06), (0.085, 0.13, 0.06))
# Huefte, Oberkoerper mit runden Schultern, Kragen, Guertel mit Schnalle und Tasche, Umhang
add("cyl", "hips", M["pants"], (0, 0, 0.86), (0.24, 0.17, 0.09))
add("cyl", "spine", M["tunic"], (0, 0, 1.11), (0.23, 0.16, 0.21))
add("sphere", "spine", M["tunic"], (0, 0, 1.25), (0.255, 0.17, 0.12))
add("cyl", "spine", M["tunic_dark"], (0, 0, 1.335), (0.115, 0.095, 0.03))
add("cube", "spine", M["tunic_dark"], (0, -0.165, 1.12), (0.012, 0.005, 0.15))
add("cyl", "hips", M["belt"], (0, 0, 0.965), (0.245, 0.175, 0.035))
add("cube", "hips", M["gold"], (0, -0.18, 0.965), (0.045, 0.012, 0.032))
add("cube", "hips", M["belt"], (0.17, -0.12, 0.9), (0.05, 0.03, 0.055))
add("cube", "spine", M["cloak"], (0, 0.175, 1.02), (0.25, 0.022, 0.32))
add("cube", "spine", M["cloak"], (0, 0.14, 1.33), (0.2, 0.06, 0.03))
# Hals und Kopf: Ohren, Augen mit Pupille, Brauen, Nase, Mund
add("cyl", "head", M["skin_dark"], (0, 0, 1.37), (0.07, 0.07, 0.05))
add("sphere", "head", M["skin"], (0, 0, 1.56), (0.23, 0.22, 0.23))
for x in (-0.225, 0.225):
    add("sphere", "head", M["skin_dark"], (x, 0.01, 1.55), (0.03, 0.04, 0.055))
for x in (-0.085, 0.085):
    add("sphere", "head", M["eye_white"], (x, -0.2, 1.55), (0.05, 0.02, 0.055))
    add("sphere", "head", M["eyes"], (x, -0.218, 1.545), (0.03, 0.012, 0.038))
    add("cube", "head", M["hair"], (x, -0.212, 1.625), (0.045, 0.01, 0.012), rot=(0, 0, math.radians(10 if x < 0 else -10)))
add("sphere", "head", M["skin_dark"], (0, -0.226, 1.505), (0.022, 0.02, 0.026))
add("cube", "head", M["mouth"], (0, -0.214, 1.455), (0.035, 0.006, 0.007))
# Haare: Kappe, Hinterkopf, Strähnen in der Stirn, Seiten
add("sphere", "head", M["hair"], (0, 0.03, 1.68), (0.248, 0.238, 0.15))
add("sphere", "head", M["hair"], (0, 0.12, 1.56), (0.225, 0.13, 0.2))
for x, y, z, s in ((-0.11, -0.17, 1.68, 0.09), (0.01, -0.195, 1.7, 0.1), (0.12, -0.165, 1.67, 0.08)):
    add("sphere", "head", M["hair_light"] if x == 0.01 else M["hair"], (x, y, z), (s, 0.06, 0.065))
for x in (-0.205, 0.205):
    add("sphere", "head", M["hair"], (x, -0.03, 1.6), (0.05, 0.08, 0.1))
# Arme: Aermel, Unterarm, Hand mit Daumen
for side, x in (("L", -0.31), ("R", 0.31)):
    s = -1 if side == "L" else 1
    add("cyl", f"arm.{side}", M["tunic"], (x, 0, 1.2), (0.072, 0.072, 0.1))
    add("cyl", f"arm.{side}", M["skin"], (x, 0, 1.02), (0.06, 0.06, 0.1))
    add("sphere", f"arm.{side}", M["skin"], (x, 0, 0.9), (0.066, 0.06, 0.072))
    add("sphere", f"arm.{side}", M["skin_dark"], (x - 0.045 * s, -0.04, 0.915), (0.026, 0.026, 0.036))

bpy.ops.object.select_all(action="DESELECT")
for o in parts:
    o.select_set(True)
bpy.context.view_layer.objects.active = parts[0]
bpy.ops.object.join()
body = bpy.context.active_object
body.name = "Player"
body.data.name = "PlayerMesh"
bpy.ops.object.shade_flat()
body.parent = rig
mod = body.modifiers.new("Armature", "ARMATURE")
mod.object = rig

# ---------------------------------------------------------------- Posen-Hilfen
bpy.context.view_layer.objects.active = rig
bpy.ops.object.mode_set(mode="POSE")
PB = rig.pose.bones
for pb in PB:
    pb.rotation_mode = "XYZ"

def reset_pose():
    for pb in PB:
        pb.location = (0, 0, 0)
        pb.rotation_euler = (0, 0, 0)
        pb.scale = (1, 1, 1)

def hand_y(angle):
    """Wohin zeigt eine Arm-Drehung um X? (negativ = nach vorne, zur Kamera)"""
    reset_pose()
    PB["arm.R"].rotation_euler.x = math.radians(angle)
    bpy.context.view_layer.update()
    return (rig.matrix_world @ PB["arm.R"].tail).y

# Vorzeichen so bestimmen, dass "+" immer "nach vorne" (Richtung -Y) bedeutet
ARM_FWD = 1.0 if hand_y(45) < hand_y(0) else -1.0

def spine_tip_y(angle):
    reset_pose()
    PB["spine"].rotation_euler.x = math.radians(angle)
    bpy.context.view_layer.update()
    return (rig.matrix_world @ PB["head"].tail).y

SPINE_FWD = 1.0 if spine_tip_y(20) < spine_tip_y(0) else -1.0
reset_pose()

def deg(a):
    return math.radians(a)

def pose(p):
    """p = dict(bone -> dict(rx, ry, rz, z, sz)); rx > 0 = nach vorne"""
    reset_pose()
    for bone, v in p.items():
        pb = PB[bone]
        fwd = SPINE_FWD if bone in ("spine", "hips", "head") else ARM_FWD
        pb.rotation_euler = (deg(v.get("rx", 0)) * fwd, deg(v.get("ry", 0)), deg(v.get("rz", 0)))
        pb.location = (0, v.get("z", 0), 0)  # Bone-Y zeigt nach oben -> "z" = hoch/runter
        s = v.get("sz", 1.0)
        pb.scale = (1.0 / math.sqrt(s), s, 1.0 / math.sqrt(s))

ANIMS = {
    "idle": [
        {},
        {"root": {"z": -0.025}, "arm.L": {"rx": 3}, "arm.R": {"rx": 3}},
    ],
    "walk": [
        {"leg.L": {"rx": 20}, "leg.R": {"rx": -20}, "arm.L": {"rx": -18}, "arm.R": {"rx": 18}},
        {"root": {"z": 0.04}},
        {"leg.L": {"rx": -20}, "leg.R": {"rx": 20}, "arm.L": {"rx": 18}, "arm.R": {"rx": -18}},
        {"root": {"z": 0.04}},
    ],
    "attack": [
        {"arm.R": {"rx": -70}, "spine": {"rz": 18}, "arm.L": {"rx": 15}},
        {"arm.R": {"rx": 95}, "spine": {"rx": 12, "rz": -20}, "arm.L": {"rx": -15}, "leg.L": {"rx": 12}},
        {"arm.R": {"rx": 55}, "spine": {"rx": 6, "rz": -8}},
    ],
    "dodge": [
        {"root": {"z": -0.12}, "spine": {"rx": 20}, "leg.L": {"rx": 25}, "leg.R": {"rx": -10}, "arm.L": {"rx": -30}, "arm.R": {"rx": -30}},
        {"root": {"z": -0.28}, "hips": {"rx": 8}, "spine": {"rx": 32}, "head": {"rx": 18},
         "leg.L": {"rx": 45}, "leg.R": {"rx": 30}, "arm.L": {"rx": 50}, "arm.R": {"rx": 50}},
        {"root": {"z": -0.08}, "spine": {"rx": 12}, "leg.L": {"rx": -12}, "leg.R": {"rx": 15}},
    ],
    # Zaubern: Arme holen aus, stossen nach vorne, halten (Leuchten der Hand kommt als Partikel aus Unity)
    "cast": [
        {"spine": {"rx": -8}, "arm.L": {"rx": 35, "rz": 20}, "arm.R": {"rx": 35, "rz": -20}, "head": {"rx": -6}},
        {"spine": {"rx": 10}, "arm.L": {"rx": 88, "rz": 8}, "arm.R": {"rx": 88, "rz": -8}, "leg.L": {"rx": 10}, "leg.R": {"rx": -8}},
        {"spine": {"rx": 5}, "arm.L": {"rx": 75, "rz": 5}, "arm.R": {"rx": 75, "rz": -5}},
    ],
}

rendered = 0
for anim, frames in ANIMS.items():
    rig.animation_data_clear()
    for i, p in enumerate(frames):
        pose(p)
        for pb in PB:
            pb.keyframe_insert("location", frame=i + 1)
            pb.keyframe_insert("rotation_euler", frame=i + 1)
            pb.keyframe_insert("scale", frame=i + 1)
    act = rig.animation_data.action
    act.name = f"Player_{anim}"
    act.use_fake_user = True
    for direction, angle in DIRECTIONS.items():
        rig.rotation_euler = (0, 0, deg(angle))
        for i in range(len(frames)):
            scene.frame_set(i + 1)
            scene.render.filepath = os.path.join(OUT_DIR, f"{anim}_{direction}_{i}.png")
            bpy.ops.render.render(write_still=True)
            rendered += 1

rig.rotation_euler = (0, 0, 0)
rig.animation_data.action = bpy.data.actions["Player_idle"]
# Fuer build_equipment.py: Liste der Animationen und Bildzahlen im Blend speichern
scene["anim_frames"] = {k: len(v) for k, v in ANIMS.items()}
bpy.ops.object.mode_set(mode="OBJECT")
scene.frame_set(1)
bpy.ops.wm.save_as_mainfile(filepath=BLEND_PATH)
print(f"RESULT rendered={rendered} armfwd={ARM_FWD} spinefwd={SPINE_FWD} blend={BLEND_PATH}")
