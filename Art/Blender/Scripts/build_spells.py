"""RE:BORN - Zauber-Grafik (Platzhalter v1)

Fuegt der Datei Reborn_Characters.blend eine Szene "Spells" hinzu und rendert:
  spell_dragon_head.png, spell_dragon_body.png, spell_waterball.png,
  spell_fireball_0.png, spell_fireball_1.png   (64 x 64, Seitenansicht, zeigt nach rechts)
  icon_wasserkugel.png, icon_wasserdrache.png, icon_feuerball.png, icon_flammenring.png (32 x 32)

Aufruf:  blender.exe -b <Reborn_Characters.blend> --python build_spells.py -- <projektordner>
"""
import bpy
import bmesh
import math
import os
import sys
from mathutils import Vector

argv = sys.argv[sys.argv.index("--") + 1:] if "--" in sys.argv else []
PROJECT = argv[0] if argv else os.path.abspath(os.path.join(os.path.dirname(__file__), "..", "..", ".."))
OUT_DIR = os.path.join(PROJECT, "Assets", "_Project", "Sprites", "Spells")
BLEND_PATH = os.path.join(PROJECT, "Art", "Blender", "Reborn_Characters.blend")
os.makedirs(OUT_DIR, exist_ok=True)

if "Spells" in bpy.data.scenes:
    bpy.data.scenes.remove(bpy.data.scenes["Spells"])
src = bpy.data.scenes["Characters"]
scene = bpy.data.scenes.new("Spells")
if bpy.context.window:
    bpy.context.window.scene = scene
r, s = scene.render, src.render
for attr in ("engine", "resolution_percentage", "film_transparent", "filter_size", "use_freestyle"):
    setattr(r, attr, getattr(s, attr))
r.image_settings.file_format = "PNG"
r.image_settings.color_mode = "RGBA"
try:
    scene.eevee.taa_render_samples = 1
except AttributeError:
    pass
scene.view_settings.view_transform = "Standard"
scene.world = src.world

# Kamera von vorne (schaut in +Y), Objekte zeigen nach +X = im Bild nach rechts
cd = bpy.data.cameras.new("SpellCamera"); cd.type = "ORTHO"; cd.ortho_scale = 1.6
cam = bpy.data.objects.new("SpellCamera", cd); scene.collection.objects.link(cam)
cam.location = (0, -10, 0); cam.rotation_euler = (math.radians(90), 0, 0); scene.camera = cam
ld = bpy.data.lights.new("SpellSun", "SUN"); ld.energy = 3.0
sun = bpy.data.objects.new("SpellSun", ld); scene.collection.objects.link(sun)
sun.rotation_euler = (math.radians(60), 0, math.radians(-30))


def mat(name, rgb, emission=0.0, emit_rgb=None, rough=0.4):
    m = bpy.data.materials.get(name) or bpy.data.materials.new(name)
    m.use_nodes = True
    p = m.node_tree.nodes.get("Principled BSDF")
    p.inputs["Base Color"].default_value = (*rgb, 1.0)
    p.inputs["Roughness"].default_value = rough
    if "Emission Color" in p.inputs:
        p.inputs["Emission Color"].default_value = (*(emit_rgb or rgb), 1.0)
        p.inputs["Emission Strength"].default_value = emission
    return m


M = {
    "water": mat("ZauberWasser", (0.18, 0.55, 0.95), 0.9, (0.25, 0.65, 1.0)),
    "water_dark": mat("ZauberWasserDunkel", (0.08, 0.3, 0.7), 0.4, (0.1, 0.35, 0.8)),
    "foam": mat("ZauberSchaum", (0.85, 0.95, 1.0), 1.2),
    "eye": mat("ZauberAuge", (1.0, 0.95, 0.6), 2.0),
    "pupil": mat("ZauberPupille", (0.02, 0.05, 0.12)),
    "fire_core": mat("ZauberGlut", (1.0, 0.95, 0.6), 3.0),
    "fire": mat("ZauberFeuer", (1.0, 0.5, 0.08), 2.2),
    "fire_dark": mat("ZauberFeuerRot", (0.85, 0.18, 0.05), 1.5),
    "bg_water": mat("IconGrundWasser", (0.05, 0.12, 0.25), 0.3),
    "bg_fire": mat("IconGrundFeuer", (0.25, 0.07, 0.03), 0.3),
}

groups = {}


def link(o, group, material, parent=None):
    scene.collection.objects.link(o)
    o.data.materials.append(material)
    o.parent = parent
    groups.setdefault(group, []).append(o)
    for poly in o.data.polygons:
        poly.use_smooth = True
    return o


def sphere(group, material, loc, scale, parent=None, seg=16):
    me = bpy.data.meshes.new(group)
    bm = bmesh.new(); bmesh.ops.create_uvsphere(bm, u_segments=seg, v_segments=max(6, seg // 2), radius=1.0); bm.to_mesh(me); bm.free()
    o = bpy.data.objects.new(group, me)
    o.location = loc; o.scale = scale
    return link(o, group, material, parent)


def cone(group, material, loc, radius, depth, rot, parent=None):
    me = bpy.data.meshes.new(group)
    bm = bmesh.new(); bmesh.ops.create_cone(bm, cap_ends=True, segments=10, radius1=radius, radius2=0.0, depth=depth); bm.to_mesh(me); bm.free()
    o = bpy.data.objects.new(group, me)
    o.location = loc; o.rotation_euler = rot
    return link(o, group, material, parent)


def cylinder(group, material, loc, radius, depth, rot, parent=None):
    me = bpy.data.meshes.new(group)
    bm = bmesh.new(); bmesh.ops.create_cone(bm, cap_ends=True, segments=24, radius1=radius, radius2=radius, depth=depth); bm.to_mesh(me); bm.free()
    o = bpy.data.objects.new(group, me)
    o.location = loc; o.rotation_euler = rot
    return link(o, group, material, parent)


def torus(group, material, loc, major, minor, rot, parent=None):
    me = bpy.data.meshes.new(group)
    bm = bmesh.new()
    ring, tube = 32, 10
    verts = []
    for i in range(ring):
        a = 2 * math.pi * i / ring
        row = []
        for j in range(tube):
            b = 2 * math.pi * j / tube
            x = (major + minor * math.cos(b)) * math.cos(a)
            y = (major + minor * math.cos(b)) * math.sin(a)
            z = minor * math.sin(b)
            row.append(bm.verts.new((x, y, z)))
        verts.append(row)
    for i in range(ring):
        for j in range(tube):
            bm.faces.new((verts[i][j], verts[(i + 1) % ring][j], verts[(i + 1) % ring][(j + 1) % tube], verts[i][(j + 1) % tube]))
    bm.to_mesh(me); bm.free()
    o = bpy.data.objects.new(group, me)
    o.location = loc; o.rotation_euler = rot
    return link(o, group, material, parent)


R90 = math.radians(90)

# ---------------- Drachenkopf (zeigt nach +X)
g = "head"
sphere(g, M["water"], (-0.05, 0, 0.02), (0.36, 0.26, 0.26))
sphere(g, M["water"], (0.32, 0, -0.03), (0.3, 0.17, 0.15))
sphere(g, M["water_dark"], (0.28, 0, -0.17), (0.26, 0.13, 0.07))
sphere(g, M["foam"], (0.58, -0.05, 0.0), (0.05, 0.04, 0.04))
sphere(g, M["eye"], (0.12, -0.2, 0.1), (0.07, 0.04, 0.05))
sphere(g, M["pupil"], (0.14, -0.235, 0.1), (0.025, 0.015, 0.035))
for y in (-0.08, 0.08):
    cone(g, M["foam"], (-0.25, y, 0.28), 0.06, 0.4, (0, math.radians(-60), 0))
cone(g, M["water_dark"], (-0.32, 0, 0.14), 0.12, 0.3, (0, math.radians(-100), 0))
for z in (-0.06, 0.02):
    cylinder(g, M["foam"], (0.25, -0.12, z - 0.08), 0.012, 0.5, (0, math.radians(100), math.radians(15)))

# ---------------- Koerperteil
g = "body"
sphere(g, M["water"], (0, 0, 0), (0.26, 0.24, 0.24))
sphere(g, M["foam"], (-0.06, -0.15, 0.12), (0.07, 0.04, 0.05))
cone(g, M["water_dark"], (0, 0, 0.27), 0.08, 0.2, (0, 0, 0))

# ---------------- Wasserkugel
g = "waterball"
sphere(g, M["water"], (0, 0, 0), (0.26, 0.26, 0.26))
sphere(g, M["foam"], (-0.08, -0.2, 0.1), (0.07, 0.04, 0.06))
for i in range(5):
    a = 2 * math.pi * i / 5
    sphere(g, M["water"], (-0.32 - 0.1 * i, 0, 0.12 * math.sin(a)), (0.07 - 0.01 * i,) * 3, seg=10)

# ---------------- Feuerball (2 Bilder: Flammen drehen sich)
g = "fireball"
fb_root = bpy.data.objects.new("FireballRoot", None); scene.collection.objects.link(fb_root)
groups.setdefault(g, []).append(fb_root)
sphere(g, M["fire_core"], (0, 0, 0), (0.17, 0.17, 0.17), fb_root)
sphere(g, M["fire"], (0, 0, 0), (0.23, 0.23, 0.23), fb_root)
flames = []
for i in range(7):
    a = 2 * math.pi * i / 7
    z = 0.14 * math.sin(a)
    length = 0.45 if i % 2 == 0 else 0.32
    flames.append(cone(g, M["fire"] if i % 2 == 0 else M["fire_dark"], (-0.22, 0, z), 0.11, length,
                       (0, math.radians(-90 + 18 * math.sin(a)), 0), fb_root))

# ---------------- Flammenring (nur fuer das Icon)
g = "ring"
torus(g, M["fire"], (0, 0, 0), 0.5, 0.08, (math.radians(70), 0, 0))
for i in range(12):
    a = 2 * math.pi * i / 12
    x, y = 0.5 * math.cos(a), 0.5 * math.sin(a)
    # Flammen sitzen auf dem um 70 Grad gekippten Ring
    cone(g, M["fire_dark"] if i % 2 else M["fire_core"], (x, y * math.cos(math.radians(70)), y * math.sin(math.radians(70)) + 0.12), 0.07, 0.26, (0, 0, 0))

# ---------------- Icon-Hintergruende (Scheibe zur Kamera)
cylinder("bg_water", M["bg_water"], (0, 0.6, 0), 0.75, 0.05, (R90, 0, 0))
cylinder("bg_fire", M["bg_fire"], (0, 0.6, 0), 0.75, 0.05, (R90, 0, 0))


def show(*names):
    for name, objs in groups.items():
        for o in objs:
            o.hide_render = name not in names


def render(path, size, ortho):
    r.resolution_x = r.resolution_y = size
    cd.ortho_scale = ortho
    r.filepath = os.path.join(OUT_DIR, path)
    bpy.ops.render.render(write_still=True, scene=scene.name)


rendered = 0
for name, file in (("head", "spell_dragon_head.png"), ("body", "spell_dragon_body.png"), ("waterball", "spell_waterball.png")):
    show(name); render(file, 64, 1.6); rendered += 1
for i, angle in enumerate((0, 25)):
    fb_root.rotation_euler = (math.radians(angle), 0, 0)
    show("fireball"); render(f"spell_fireball_{i}.png", 64, 1.6); rendered += 1
fb_root.rotation_euler = (0, 0, 0)

for name, bg, file, ortho in (("waterball", "bg_water", "icon_wasserkugel.png", 1.5),
                              ("head", "bg_water", "icon_wasserdrache.png", 1.6),
                              ("fireball", "bg_fire", "icon_feuerball.png", 1.5),
                              ("ring", "bg_fire", "icon_flammenring.png", 1.6)):
    show(name, bg); render(file, 32, ortho); rendered += 1

bpy.ops.wm.save_as_mainfile(filepath=BLEND_PATH)
print(f"RESULT rendered={rendered}")
