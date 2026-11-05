"""RE:BORN - Hauptcharakter (Platzhalter v1)

Baut Modell + Skelett, legt die Animationen idle/walk/attack/dodge an
und rendert jede Animation in 4 Richtungen als 64x64-Pixel-Sprites.

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

SPRITE_SIZE = 64
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
    "hair": mat("Haare", (0.22, 0.13, 0.08)),
    "tunic": mat("Tunika", (0.20, 0.36, 0.62)),
    "pants": mat("Hose", (0.28, 0.22, 0.17)),
    "boots": mat("Stiefel", (0.15, 0.10, 0.07)),
    "belt": mat("Guertel", (0.45, 0.30, 0.12)),
    "eyes": mat("Augen", (0.05, 0.05, 0.08)),
    "blade": mat("Klinge", (0.80, 0.84, 0.90)),
    "hilt": mat("Griff", (0.55, 0.40, 0.10)),
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

# Beine + Stiefel
for side, x in (("L", -0.12), ("R", 0.12)):
    add("cyl", f"leg.{side}", M["pants"], (x, 0, 0.47), (0.085, 0.085, 0.36))
    add("cube", f"leg.{side}", M["boots"], (x, -0.03, 0.07), (0.09, 0.13, 0.07))
# Koerper
add("cyl", "spine", M["tunic"], (0, 0, 1.10), (0.24, 0.17, 0.26))
add("cyl", "hips", M["tunic"], (0, 0, 0.86), (0.25, 0.18, 0.08))
add("cyl", "hips", M["belt"], (0, 0, 0.95), (0.255, 0.185, 0.03))
# Kopf
add("sphere", "head", M["skin"], (0, 0, 1.55), (0.22, 0.21, 0.22))
add("sphere", "head", M["hair"], (0, 0.07, 1.66), (0.23, 0.19, 0.15))
for x in (-0.08, 0.08):
    add("sphere", "head", M["eyes"], (x, -0.2, 1.52), (0.04, 0.025, 0.05))
# Arme + Haende
for side, x in (("L", -0.32), ("R", 0.32)):
    add("cyl", f"arm.{side}", M["tunic"], (x, 0, 1.13), (0.07, 0.07, 0.18))
    add("sphere", f"arm.{side}", M["skin"], (x, 0, 0.92), (0.075, 0.075, 0.075))
# Schwert in der rechten Hand (zeigt nach vorne oben)
add("cube", "arm.R", M["hilt"], (0.32, -0.02, 0.92), (0.03, 0.10, 0.03))
add("cube", "arm.R", M["blade"], (0.32, -0.33, 1.08), (0.025, 0.02, 0.30), rot=(math.radians(-60), 0, 0))

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
bpy.ops.object.mode_set(mode="OBJECT")
scene.frame_set(1)
bpy.ops.wm.save_as_mainfile(filepath=BLEND_PATH)
print(f"RESULT rendered={rendered} armfwd={ARM_FWD} spinefwd={SPINE_FWD} blend={BLEND_PATH}")
