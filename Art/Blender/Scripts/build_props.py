"""RE:BORN - Gegenstaende in der Welt (Platzhalter v1): Schatztruhe

Fuegt der Datei Reborn_Characters.blend eine Szene "Props" hinzu und rendert die Truhe
geschlossen und offen als 64x64-Pixel-Sprites (gleicher Kamerawinkel wie die Figuren).

Aufruf:  blender.exe -b <Reborn_Characters.blend> --python build_props.py -- <projektordner>
"""
import bpy
import bmesh
import math
import os
import sys
from mathutils import Vector

argv = sys.argv[sys.argv.index("--") + 1:] if "--" in sys.argv else []
PROJECT = argv[0] if argv else os.path.abspath(os.path.join(os.path.dirname(__file__), "..", "..", ".."))
OUT_DIR = os.path.join(PROJECT, "Assets", "_Project", "Sprites", "Props")
BLEND_PATH = os.path.join(PROJECT, "Art", "Blender", "Reborn_Characters.blend")
os.makedirs(OUT_DIR, exist_ok=True)

# Alte Props-Szene entfernen, neue Szene mit gleichen Render-Einstellungen wie "Characters"
if "Props" in bpy.data.scenes:
    bpy.data.scenes.remove(bpy.data.scenes["Props"])
src = bpy.data.scenes["Characters"]
scene = bpy.data.scenes.new("Props")
if bpy.context.window:
    bpy.context.window.scene = scene
r, s = scene.render, src.render
for attr in ("engine", "resolution_x", "resolution_y", "resolution_percentage", "film_transparent",
             "filter_size", "use_freestyle"):
    setattr(r, attr, getattr(s, attr))
r.image_settings.file_format = "PNG"
r.image_settings.color_mode = "RGBA"
try:
    scene.eevee.taa_render_samples = 1
except AttributeError:
    pass
scene.view_settings.view_transform = "Standard"
scene.world = src.world

# Kamera + Licht (gleicher Winkel wie beim Spieler)
TARGET = Vector((0, 0, 0.42))
pitch = math.radians(30.0)
fwd = Vector((0, math.sin(pitch), -math.cos(pitch)))
cd = bpy.data.cameras.new("PropsCamera"); cd.type = "ORTHO"; cd.ortho_scale = 1.6
cam = bpy.data.objects.new("PropsCamera", cd); scene.collection.objects.link(cam)
cam.location = TARGET - fwd * 10.0; cam.rotation_euler = (pitch, 0, 0); scene.camera = cam
ld = bpy.data.lights.new("PropsSun", "SUN"); ld.energy = 3.5
sun = bpy.data.objects.new("PropsSun", ld); scene.collection.objects.link(sun)
sun.rotation_euler = (math.radians(40), math.radians(-25), math.radians(-30))


def mat(name, rgb, metallic=0.0, roughness=0.6):
    m = bpy.data.materials.get(name) or bpy.data.materials.new(name)
    m.use_nodes = True
    p = m.node_tree.nodes.get("Principled BSDF")
    p.inputs["Base Color"].default_value = (*rgb, 1.0)
    p.inputs["Metallic"].default_value = metallic
    p.inputs["Roughness"].default_value = roughness
    return m


wood = mat("TruheHolz", (0.36, 0.19, 0.08))
wood_dark = mat("TruheHolzDunkel", (0.22, 0.11, 0.05))
metal = mat("TruheMetall", (0.32, 0.32, 0.36), metallic=0.8, roughness=0.4)
gold = mat("TruheGold", (0.95, 0.68, 0.15), metallic=0.9, roughness=0.3)
inside = mat("TruheInnen", (0.05, 0.03, 0.02))


def box(name, material, loc, size, parent=None):
    me = bpy.data.meshes.new(name)
    bm = bmesh.new()
    bmesh.ops.create_cube(bm, size=1.0)
    for v in bm.verts:
        v.co.x *= size[0]; v.co.y *= size[1]; v.co.z *= size[2]
    bm.to_mesh(me); bm.free()
    o = bpy.data.objects.new(name, me)
    scene.collection.objects.link(o)
    o.data.materials.append(material)
    o.parent = parent
    o.location = loc
    return o


def sphere(name, material, loc, scale, parent=None):
    me = bpy.data.meshes.new(name)
    bm = bmesh.new(); bmesh.ops.create_uvsphere(bm, u_segments=10, v_segments=6, radius=1.0); bm.to_mesh(me); bm.free()
    o = bpy.data.objects.new(name, me); scene.collection.objects.link(o)
    o.data.materials.append(material); o.parent = parent; o.location = loc; o.scale = scale
    return o


W, D, H, LID = 0.8, 0.5, 0.38, 0.16

root = bpy.data.objects.new("ChestRoot", None)
scene.collection.objects.link(root)

# Koerper mit dunklem Innenraum und Gold
box("ChestBody", wood, (0, 0, H / 2), (W, D, H), root)
box("ChestBottomTrim", wood_dark, (0, 0, 0.03), (W + 0.02, D + 0.02, 0.06), root)
box("ChestInside", inside, (0, 0, H - 0.01), (W - 0.08, D - 0.08, 0.03), root)
for i, (x, y) in enumerate([(-0.15, 0.02), (0.05, -0.05), (0.18, 0.06), (-0.02, 0.1)]):
    sphere(f"ChestCoin{i}", gold, (x, y, H + 0.01), (0.09, 0.07, 0.04), root)
for x in (-0.25, 0.25):
    box("ChestBand", metal, (x, 0, H / 2), (0.07, D + 0.02, H + 0.01), root)
box("ChestLock", gold, (0, -D / 2 - 0.02, H - 0.05), (0.1, 0.04, 0.12), root)

# Deckel: Drehpunkt an der hinteren oberen Kante
hinge = bpy.data.objects.new("ChestHinge", None)
scene.collection.objects.link(hinge)
hinge.parent = root
hinge.location = (0, D / 2, H)
box("ChestLid", wood, (0, -D / 2, LID / 2), (W + 0.02, D + 0.02, LID), hinge)
box("ChestLidTop", wood_dark, (0, -D / 2, LID + 0.01), (W - 0.06, D - 0.1, 0.02), hinge)
for x in (-0.25, 0.25):
    box("ChestLidBand", metal, (x, -D / 2, LID / 2), (0.07, D + 0.04, LID + 0.03), hinge)

STATES = {"closed": 0.0, "open": -105.0}
rendered = 0
for state, angle in STATES.items():
    hinge.rotation_euler = (math.radians(angle), 0, 0)
    r.filepath = os.path.join(OUT_DIR, f"chest_{state}.png")
    bpy.ops.render.render(write_still=True, scene=scene.name)
    rendered += 1

hinge.rotation_euler = (0, 0, 0)
bpy.ops.wm.save_as_mainfile(filepath=BLEND_PATH)
print(f"RESULT rendered={rendered}")
