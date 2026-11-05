"""RE:BORN - Waldschleim (Platzhalter v1)

Fuegt der Datei Reborn_Characters.blend eine Szene "Slime" hinzu und rendert
idle (4), hurt (1), death (3) in 4 Richtungen als 64x64-Pixel-Sprites.

Aufruf:  blender.exe -b <Reborn_Characters.blend> --python build_slime.py -- <projektordner>
"""
import bpy
import math
import os
import sys
from mathutils import Vector

argv = sys.argv[sys.argv.index("--") + 1:] if "--" in sys.argv else []
PROJECT = argv[0] if argv else os.path.abspath(os.path.join(os.path.dirname(__file__), "..", "..", ".."))
OUT_DIR = os.path.join(PROJECT, "Assets", "_Project", "Sprites", "Slime")
BLEND_PATH = os.path.join(PROJECT, "Art", "Blender", "Reborn_Characters.blend")
os.makedirs(OUT_DIR, exist_ok=True)
DIRECTIONS = {"down": 0.0, "right": 90.0, "up": 180.0, "left": -90.0}

# Alte Slime-Szene entfernen, neue Szene mit gleichen Render-Einstellungen
if "Slime" in bpy.data.scenes:
    bpy.data.scenes.remove(bpy.data.scenes["Slime"])
src = bpy.data.scenes["Characters"]
scene = bpy.data.scenes.new("Slime")
bpy.context.window.scene = scene if bpy.context.window else None
r, s = scene.render, src.render
for attr in ("engine", "resolution_x", "resolution_y", "resolution_percentage", "film_transparent",
             "filter_size", "use_freestyle", "line_thickness_mode", "line_thickness"):
    setattr(r, attr, getattr(s, attr))
r.image_settings.file_format = "PNG"
r.image_settings.color_mode = "RGBA"
try:
    scene.eevee.taa_render_samples = 1
except AttributeError:
    pass
scene.view_settings.view_transform = "Standard"
scene.world = src.world
ls = scene.view_layers[0].freestyle_settings.linesets.new("Outline")
ls.select_by_visibility = True
ls.linestyle.color = (0.05, 0.12, 0.05)

# Kamera + Licht (gleicher Winkel wie beim Spieler, etwas naeher)
TARGET = Vector((0, 0, 0.3))
pitch = math.radians(30.0)
fwd = Vector((0, math.sin(pitch), -math.cos(pitch)))
cd = bpy.data.cameras.new("SlimeCamera"); cd.type = "ORTHO"; cd.ortho_scale = 1.6
cam = bpy.data.objects.new("SlimeCamera", cd); scene.collection.objects.link(cam)
cam.location = TARGET - fwd * 10.0; cam.rotation_euler = (pitch, 0, 0); scene.camera = cam
ld = bpy.data.lights.new("SlimeSun", "SUN"); ld.energy = 3.5
sun = bpy.data.objects.new("SlimeSun", ld); scene.collection.objects.link(sun)
sun.rotation_euler = (math.radians(40), math.radians(-25), math.radians(-30))

def mat(name, rgb, alpha=1.0):
    m = bpy.data.materials.get(name) or bpy.data.materials.new(name)
    m.use_nodes = True
    p = m.node_tree.nodes.get("Principled BSDF")
    p.inputs["Base Color"].default_value = (*rgb, 1.0)
    p.inputs["Roughness"].default_value = 0.6
    return m

goo = mat("Schleim", (0.12, 0.42, 0.10))
eye = mat("SchleimAuge", (0.03, 0.05, 0.03))
shine = mat("SchleimGlanz", (0.85, 1.0, 0.8))

def ctx_link(o):
    for c in o.users_collection:
        c.objects.unlink(o)
    scene.collection.objects.link(o)

# Koerper: Kugel, unten abgeflacht
bm_mesh = bpy.data.meshes.new("SlimeMesh")
import bmesh
bm = bmesh.new()
bmesh.ops.create_uvsphere(bm, u_segments=20, v_segments=10, radius=0.42)
for v in bm.verts:
    if v.co.z < -0.1:
        v.co.z = -0.1 + (v.co.z + 0.1) * 0.15
    v.co.z *= 0.8
bm.to_mesh(bm_mesh); bm.free()
body = bpy.data.objects.new("Slime", bm_mesh)
scene.collection.objects.link(body)
body.data.materials.append(goo)
for p in body.data.polygons:
    p.use_smooth = True

root = bpy.data.objects.new("SlimeRoot", None)
scene.collection.objects.link(root)
body.parent = root
body.location = (0, 0, 0.1)

def sphere(name, material, loc, scale):
    me = bpy.data.meshes.new(name)
    bm = bmesh.new(); bmesh.ops.create_uvsphere(bm, u_segments=10, v_segments=6, radius=1.0); bm.to_mesh(me); bm.free()
    o = bpy.data.objects.new(name, me); scene.collection.objects.link(o)
    o.data.materials.append(material); o.parent = body; o.location = loc; o.scale = scale
    return o

for x in (-0.13, 0.13):
    sphere("SlimeEye", eye, (x, -0.33, 0.12), (0.06, 0.03, 0.08))
sphere("SlimeShine", shine, (-0.15, -0.12, 0.27), (0.08, 0.05, 0.04))

def key(o, frame, sz, z=0.0, rx=0.0, sx=None):
    if sx is None:
        sx = 1.0 / math.sqrt(sz)
    o.scale = (sx, sx, sz)
    o.location = (0, 0, 0.1 + z)
    o.rotation_euler = (math.radians(rx), 0, 0)
    for path in ("scale", "location", "rotation_euler"):
        o.keyframe_insert(path, frame=frame)

ANIMS = {
    "idle": [(1.0, 0.0), (0.88, 0.0), (1.12, 0.06), (1.0, 0.02)],
    "hurt": [(1.25, 0.0, -12.0)],
    # (hoehe, z, kippen, breite): zerfliesst flach und wird kleiner
    "death": [(0.7, 0.0, 0.0, 1.15), (0.4, -0.03, 0.0, 1.2), (0.15, -0.06, 0.0, 0.9)],
}

rendered = 0
for anim, frames in ANIMS.items():
    body.animation_data_clear()
    for i, f in enumerate(frames):
        key(body, i + 1, f[0], f[1], f[2] if len(f) > 2 else 0.0, f[3] if len(f) > 3 else None)
    act = body.animation_data.action
    act.name = f"Slime_{anim}"
    act.use_fake_user = True
    for direction, angle in DIRECTIONS.items():
        root.rotation_euler = (0, 0, math.radians(angle))
        for i in range(len(frames)):
            scene.frame_set(i + 1)
            r.filepath = os.path.join(OUT_DIR, f"{anim}_{direction}_{i}.png")
            bpy.ops.render.render(write_still=True, scene=scene.name)
            rendered += 1

root.rotation_euler = (0, 0, 0)
body.animation_data.action = bpy.data.actions["Slime_idle"]
bpy.ops.wm.save_as_mainfile(filepath=BLEND_PATH)
print(f"RESULT rendered={rendered}")
