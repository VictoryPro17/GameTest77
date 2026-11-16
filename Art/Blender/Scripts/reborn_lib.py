"""RE:BORN - gemeinsame Blender-Werkzeuge (Meilenstein 5, Comic-Look)

- Render-Setup (128 px, durchsichtig, Kamera 60 Grad von oben)
- Comic-Material (Cel-Shading mit 3 Helligkeitsstufen) und dunkle Kontur
- Formen per bmesh (ohne bpy.ops), Teile mit Knochen-Gewicht, Zusammenfuegen
- Skelett der Spielfigur mit Ellbogen, Knien und Umhang
- Animationen aus Schluesselposen mit weichen Zwischenbildern

Wird von den build_*.py-Skripten importiert (sys.path zeigt auf diesen Ordner).
"""
import bpy
import bmesh
import math
import os
from mathutils import Vector, Matrix

SIZE = 128
DIRECTIONS = {"down": 0.0, "right": 90.0, "up": 180.0, "left": -90.0}
OUTLINE = (0.07, 0.05, 0.06)


# ---------------------------------------------------------------- Szene, Kamera, Licht
def setup_scene(scene, size=SIZE):
    r = scene.render
    r.engine = "BLENDER_EEVEE"
    r.resolution_x = r.resolution_y = size
    r.resolution_percentage = 100
    r.film_transparent = True
    r.filter_size = 0.0
    r.image_settings.file_format = "PNG"
    r.image_settings.color_mode = "RGBA"
    r.use_freestyle = False
    try:
        scene.eevee.taa_render_samples = 4
    except AttributeError:
        pass
    scene.view_settings.view_transform = "Standard"
    if scene.world is None:
        world = bpy.data.worlds.get("RebornWorld") or bpy.data.worlds.new("RebornWorld")
        world.use_nodes = True
        bg = world.node_tree.nodes.get("Background")
        bg.inputs["Color"].default_value = (0.6, 0.62, 0.7, 1.0)
        bg.inputs["Strength"].default_value = 0.5
        scene.world = world


def new_scene(name, size=SIZE):
    if name in bpy.data.scenes:
        bpy.data.scenes.remove(bpy.data.scenes[name])
    scene = bpy.data.scenes.new(name)
    if bpy.context.window:
        bpy.context.window.scene = scene
    setup_scene(scene, size)
    return scene


def camera(scene, target=(0.0, 0.0, 0.85), ortho=2.1, pitch_deg=45.0, name=None):
    pitch = math.radians(pitch_deg)
    fwd = Vector((0.0, math.sin(pitch), -math.cos(pitch)))
    cd = bpy.data.cameras.new(name or scene.name + "Camera")
    cd.type = "ORTHO"
    cd.ortho_scale = ortho
    cam = bpy.data.objects.new(cd.name, cd)
    scene.collection.objects.link(cam)
    cam.location = Vector(target) - fwd * 10.0
    cam.rotation_euler = (pitch, 0.0, 0.0)
    scene.camera = cam
    return cam


def sun(scene, energy=3.2, rot=(40, -25, -30)):
    ld = bpy.data.lights.new(scene.name + "Sun", "SUN")
    ld.energy = energy
    s = bpy.data.objects.new(ld.name, ld)
    scene.collection.objects.link(s)
    s.rotation_euler = tuple(math.radians(a) for a in rot)
    return s


# ---------------------------------------------------------------- Materialien
def cel(name, rgb, shadow=0.55, mid=0.82, light=1.0, emission=0.0):
    """Comic-Material: Licht -> 3 harte Stufen der Grundfarbe (+ optionales Leuchten)."""
    m = bpy.data.materials.get(name) or bpy.data.materials.new(name)
    m.use_nodes = True
    nt = m.node_tree
    nt.nodes.clear()
    out = nt.nodes.new("ShaderNodeOutputMaterial")
    diff = nt.nodes.new("ShaderNodeBsdfDiffuse")
    diff.inputs["Color"].default_value = (1, 1, 1, 1)
    s2r = nt.nodes.new("ShaderNodeShaderToRGB")
    ramp = nt.nodes.new("ShaderNodeValToRGB")
    ramp.color_ramp.interpolation = "CONSTANT"
    els = ramp.color_ramp.elements
    els[0].position = 0.0
    els[0].color = (*[c * shadow for c in rgb], 1.0)
    els[1].position = 0.12
    els[1].color = (*[c * mid for c in rgb], 1.0)
    e3 = els.new(0.45)
    e3.color = (*[min(1.0, c * light) for c in rgb], 1.0)
    emit = nt.nodes.new("ShaderNodeEmission")
    emit.inputs["Strength"].default_value = 1.0 + emission
    nt.links.new(diff.outputs["BSDF"], s2r.inputs["Shader"])
    nt.links.new(s2r.outputs["Color"], ramp.inputs["Fac"])
    nt.links.new(ramp.outputs["Color"], emit.inputs["Color"])
    nt.links.new(emit.outputs["Emission"], out.inputs["Surface"])
    return m


def outline_material():
    m = bpy.data.materials.get("Kontur")
    if m is None:
        m = bpy.data.materials.new("Kontur")
        m.use_nodes = True
        nt = m.node_tree
        nt.nodes.clear()
        out = nt.nodes.new("ShaderNodeOutputMaterial")
        emit = nt.nodes.new("ShaderNodeEmission")
        emit.inputs["Color"].default_value = (*OUTLINE, 1.0)
        nt.links.new(emit.outputs["Emission"], out.inputs["Surface"])
    m.use_backface_culling = True
    return m


def add_outline(obj, thickness=0.016):
    """Dunkle Kontur: Huelle mit umgedrehten Flaechen, von hinten unsichtbar."""
    mat = outline_material()
    if mat.name not in [s.material.name for s in obj.material_slots if s.material]:
        obj.data.materials.append(mat)
    idx = [s.material.name if s.material else "" for s in obj.material_slots].index(mat.name)
    mod = obj.modifiers.new("Kontur", "SOLIDIFY")
    mod.thickness = thickness
    mod.offset = 1.0
    mod.use_flip_normals = True
    mod.use_rim = False
    mod.material_offset = idx
    for slot in obj.material_slots:
        if slot.material and slot.material is not mat:
            slot.material.use_backface_culling = False
    return mod


# ---------------------------------------------------------------- Formen (bmesh)
def _mesh_object(name, bm, collection):
    me = bpy.data.meshes.new(name)
    bm.to_mesh(me)
    bm.free()
    o = bpy.data.objects.new(name, me)
    collection.objects.link(o)
    return o


def shape(kind, loc=(0, 0, 0), scale=(1, 1, 1), rot=(0, 0, 0), material=None, bone=None, name="Teil",
          collection=None, segments=16, taper=1.0, smooth=True):
    """kind: sphere | cyl | cone | cube | torus. taper: Zylinder oben schmaler (1 = gerade)."""
    collection = collection or bpy.context.scene.collection
    bm = bmesh.new()
    if kind == "sphere":
        bmesh.ops.create_uvsphere(bm, u_segments=segments, v_segments=max(6, segments // 2), radius=1.0)
    elif kind == "cyl":
        bmesh.ops.create_cone(bm, cap_ends=True, segments=segments, radius1=1.0, radius2=taper, depth=2.0)
    elif kind == "cone":
        bmesh.ops.create_cone(bm, cap_ends=True, segments=segments, radius1=1.0, radius2=0.0, depth=2.0)
    elif kind == "torus":
        ring, tube, minor = segments * 2, 8, 0.25
        verts = []
        for i in range(ring):
            a = 2 * math.pi * i / ring
            row = []
            for j in range(tube):
                b = 2 * math.pi * j / tube
                row.append(bm.verts.new(((1 + minor * math.cos(b)) * math.cos(a), (1 + minor * math.cos(b)) * math.sin(a), minor * math.sin(b))))
            verts.append(row)
        for i in range(ring):
            for j in range(tube):
                bm.faces.new((verts[i][j], verts[(i + 1) % ring][j], verts[(i + 1) % ring][(j + 1) % tube], verts[i][(j + 1) % tube]))
    else:
        bmesh.ops.create_cube(bm, size=2.0)
        if smooth:
            bmesh.ops.bevel(bm, geom=list(bm.edges), offset=0.25, segments=2, affect="EDGES", profile=0.5)
    m = Matrix.LocRotScale(Vector(loc), _euler(rot).to_quaternion(), Vector(scale))
    bmesh.ops.transform(bm, matrix=m, verts=bm.verts)
    o = _mesh_object(name, bm, collection)
    for p in o.data.polygons:
        p.use_smooth = smooth
    if material is not None:
        o.data.materials.append(material)
    if bone:
        vg = o.vertex_groups.new(name=bone)
        vg.add(list(range(len(o.data.vertices))), 1.0, "REPLACE")
    return o


def _euler(rot):
    from mathutils import Euler
    return Euler(tuple(math.radians(a) for a in rot), "XYZ")


def join(objs, name, collection=None):
    """Teile zu einem Objekt zusammenfuegen (Materialien und Knochen-Gewichte bleiben)."""
    if bpy.context.object and bpy.context.object.mode != "OBJECT":
        bpy.ops.object.mode_set(mode="OBJECT")
    bpy.ops.object.select_all(action="DESELECT")
    for o in objs:
        o.select_set(True)
    bpy.context.view_layer.objects.active = objs[0]
    if len(objs) > 1:
        bpy.ops.object.join()
    out = bpy.context.view_layer.objects.active
    out.name = name
    out.data.name = name
    return out

def subdivide(obj, levels=1):
    mod = obj.modifiers.new("Glatt", "SUBSURF")
    mod.levels = levels
    mod.render_levels = levels
    return mod


def attach_to_rig(obj, rig):
    obj.parent = rig
    mod = obj.modifiers.new("Skelett", "ARMATURE")
    mod.object = rig
    # Skelett zuerst, damit Glaetten und Kontur der Bewegung folgen
    while obj.modifiers.find("Skelett") > 0:
        bpy.context.view_layer.objects.active = obj
        obj.modifiers.move(obj.modifiers.find("Skelett"), 0)
    return mod


# ---------------------------------------------------------------- Skelett der Spielfigur
PLAYER_BONES = {
    # name: (kopf, schwanz, eltern)
    "root": ((0, 0, 0), (0, 0, 0.2), None),
    "hips": ((0, 0, 0.85), (0, 0, 1.0), "root"),
    "spine": ((0, 0, 1.0), (0, 0, 1.33), "hips"),
    "head": ((0, 0, 1.33), (0, 0, 1.82), "spine"),
    "upperarm.L": ((-0.30, 0, 1.30), (-0.31, 0, 1.10), "spine"),
    "forearm.L": ((-0.31, 0, 1.10), (-0.32, 0, 0.90), "upperarm.L"),
    "upperarm.R": ((0.30, 0, 1.30), (0.31, 0, 1.10), "spine"),
    "forearm.R": ((0.31, 0, 1.10), (0.32, 0, 0.90), "upperarm.R"),
    "thigh.L": ((-0.12, 0, 0.85), (-0.12, 0, 0.47), "hips"),
    "shin.L": ((-0.12, 0, 0.47), (-0.12, 0, 0.08), "thigh.L"),
    "thigh.R": ((0.12, 0, 0.85), (0.12, 0, 0.47), "hips"),
    "shin.R": ((0.12, 0, 0.47), (0.12, 0, 0.08), "thigh.R"),
    "cloak": ((0, 0.16, 1.30), (0, 0.2, 0.86), "spine"),
}


def build_rig(scene, name, bones):
    data = bpy.data.armatures.new(name)
    rig = bpy.data.objects.new(name, data)
    scene.collection.objects.link(rig)
    bpy.context.view_layer.objects.active = rig
    bpy.ops.object.mode_set(mode="EDIT")
    for b, (h, t, _) in bones.items():
        eb = data.edit_bones.new(b)
        eb.head, eb.tail, eb.roll = Vector(h), Vector(t), 0.0
    for b, (_, _, parent) in bones.items():
        if parent:
            data.edit_bones[b].parent = data.edit_bones[parent]
            data.edit_bones[b].use_connect = False
    bpy.ops.object.mode_set(mode="OBJECT")
    for pb in rig.pose.bones:
        pb.rotation_mode = "XYZ"
    return rig


class Poser:
    """Posen mit einheitlicher Richtung: rx > 0 kippt jeden Knochen nach vorne (zur Kamera, -Y)."""

    def __init__(self, rig):
        self.rig = rig
        self.sign = {}
        for pb in rig.pose.bones:
            self.sign[pb.name] = self._forward_sign(pb)
        self.reset()

    def _tail_y(self, pb, deg):
        self.reset()
        pb.rotation_euler.x = math.radians(deg)
        bpy.context.view_layer.update()
        return (self.rig.matrix_world @ pb.tail).y

    def _forward_sign(self, pb):
        return 1.0 if self._tail_y(pb, 15) < self._tail_y(pb, 0) else -1.0

    def reset(self):
        for pb in self.rig.pose.bones:
            pb.location = (0, 0, 0)
            pb.rotation_euler = (0, 0, 0)
            pb.scale = (1, 1, 1)

    def apply(self, pose):
        """pose: {knochen: {rx, ry, rz (Grad), z (hoch), y (vor/zurueck)}}"""
        self.reset()
        for bone, v in pose.items():
            pb = self.rig.pose.bones.get(bone)
            if pb is None:
                continue
            s = self.sign.get(bone, 1.0)
            pb.rotation_euler = (math.radians(v.get("rx", 0)) * s, math.radians(v.get("ry", 0)), math.radians(v.get("rz", 0)))
            # Knochen-Y zeigt entlang des Knochens; fuer root (zeigt nach oben) ist Y = hoch
            pb.location = (v.get("x", 0), v.get("z", 0), v.get("y", 0))
            # sz: entlang des Knochens strecken/stauchen (Volumen bleibt ungefaehr gleich), s: gleichmaessig
            sz, s_all = v.get("sz", 1.0), v.get("s", 1.0)
            pb.scale = (s_all / math.sqrt(sz), s_all * sz, s_all / math.sqrt(sz))

    def key(self, frame):
        for pb in self.rig.pose.bones:
            pb.keyframe_insert("location", frame=frame)
            pb.keyframe_insert("rotation_euler", frame=frame)
            pb.keyframe_insert("scale", frame=frame)


def make_action(rig, poser, name, keys, loop, step=10):
    """Schluesselposen alle `step` Bilder; bei Schleifen kommt die erste Pose am Ende noch einmal."""
    rig.animation_data_clear()
    seq = list(keys) + ([keys[0]] if loop else [])
    for i, pose in enumerate(seq):
        poser.apply(pose)
        poser.key(1 + i * step)
    act = rig.animation_data.action
    act.name = name
    act.use_fake_user = True
    end = 1 + (len(seq) - 1) * step
    return act, end


def sample_frames(end, count, loop):
    """Bildpositionen fuer `count` gerenderte Bilder (Schleife: ohne das Doppelbild am Ende)."""
    if count == 1:
        return [1.0]
    if loop:
        return [1 + (end - 1) * i / count for i in range(count)]
    return [1 + (end - 1) * i / (count - 1) for i in range(count)]


def set_frame(scene, f):
    scene.frame_set(int(math.floor(f)), subframe=f - math.floor(f))


def use_action(rig, act):
    ad = rig.animation_data or rig.animation_data_create()
    ad.action = act
    if hasattr(ad, "action_slot") and ad.action_slot is None and len(getattr(act, "slots", [])) > 0:
        ad.action_slot = act.slots[0]


def foot_pivot(scene, point=(0.0, 0.0, 0.0)):
    """Wo liegt ein Weltpunkt (die Fuesse) im Bild? (x, y) von 0 bis 1, fuer den Sprite-Pivot in Unity."""
    from bpy_extras.object_utils import world_to_camera_view
    co = world_to_camera_view(scene, scene.camera, Vector(point))
    return round(co.x, 4), round(co.y, 4)


def render_actions(scene, rig, poser, prefix, anims, out_dir, file_prefix=""):
    """anims: {name: (schluesselposen, bilder, schleife)} -> PNGs <file_prefix><name>_<richtung>_<i>.png; gibt Bildzahl zurueck."""
    os.makedirs(out_dir, exist_ok=True)
    n = 0
    for anim, (keys, count, loop) in anims.items():
        act, end = make_action(rig, poser, prefix + anim, keys, loop)
        for direction, angle in DIRECTIONS.items():
            rig.rotation_euler = (0, 0, math.radians(angle))
            for i, f in enumerate(sample_frames(end, count, loop)):
                set_frame(scene, f)
                render_still(scene, os.path.join(out_dir, f"{file_prefix}{anim}_{direction}_{i}.png"))
                n += 1
    rig.rotation_euler = (0, 0, 0)
    return n


def edge_pixels(path, alpha=0.1):
    """Sichtbare Pixel am aeusseren Bildrand zaehlen (0 = Figur liegt ganz im Bild)."""
    img = bpy.data.images.load(path, check_existing=False)
    try:
        w, h = img.size
        px = list(img.pixels)
        n = 0
        for x in range(w):
            for y in (0, h - 1):
                n += px[(y * w + x) * 4 + 3] > alpha
        for y in range(1, h - 1):
            for x in (0, w - 1):
                n += px[(y * w + x) * 4 + 3] > alpha
        return n
    finally:
        bpy.data.images.remove(img)


def render_still(scene, path, tries=6):
    """Ein Bild rendern und speichern. Ist die Datei kurz gesperrt (z. B. Unity importiert sie gerade),
    wird nach einer kurzen Pause erneut versucht."""
    import time
    scene.render.filepath = path
    for attempt in range(tries):
        try:
            bpy.ops.render.render(write_still=True, scene=scene.name)
            return
        except RuntimeError:
            if attempt == tries - 1:
                raise
            time.sleep(0.5 * (attempt + 1))
