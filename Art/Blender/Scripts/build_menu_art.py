"""RE:BORN - Hauptmenue-Grafiken (Meilenstein 10a, Comic-Look)

Ausgabe: Assets/_Project/Sprites/Menu. Eigene Datei: Art/Blender/Reborn_Menu.blend

  title_background.png  1920x1080  Daemmerungs-Landschaft: Stadt mit Mauer in der Ferne, Waldrand, Frostgipfel,
                                   violettes Portal auf einem Huegel im Vordergrund. Obere Mitte (Logo) und linke Seite
                                   (Menuetasten) bleiben ruhig.
  logo_frame.png        1200x360   leerer Titel-Rahmen (Gold/Bronze, dunkle Innenflaeche, aussen durchsichtig)
  target_arrow.png      128x128    goldener Pfeil nach rechts (+x), Pivot Mitte

Aufruf:  blender.exe -b --factory-startup --python build_menu_art.py -- <projektordner>
"""
import bpy
import bmesh
import math
import os
import random
import sys
import numpy as np
from mathutils import Vector, Matrix, Euler

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import reborn_lib as L  # noqa: E402

argv = sys.argv[sys.argv.index("--") + 1:] if "--" in sys.argv else []
PROJECT = argv[0] if argv else os.path.abspath(os.path.join(os.path.dirname(__file__), "..", "..", ".."))
OUT = os.path.join(PROJECT, "Assets", "_Project", "Sprites", "Menu")
BLEND_PATH = os.path.join(PROJECT, "Art", "Blender", "Reborn_Menu.blend")
os.makedirs(OUT, exist_ok=True)

bpy.ops.wm.read_factory_settings(use_empty=True)
rnd = random.Random(77)


# ------------------------------------------------------------------ Hilfen
def mix(a, b, t):
    return tuple(a[i] + (b[i] - a[i]) * t for i in range(3))


def smooth(e0, e1, x):
    t = max(0.0, min(1.0, (x - e0) / (e1 - e0)))
    return t * t * (3 - 2 * t)


def tcel(name, light, shadow=None, emission=0.0, flat=False):
    """Comic-Material mit eigenem (blauviolettem) Schattenton."""
    m = L.cel(name, light, emission=emission)
    shadow = shadow or tuple(c * 0.45 + 0.05 for c in light)
    ramp = [n for n in m.node_tree.nodes if n.type == "VALTORGB"][0]
    els = ramp.color_ramp.elements
    if flat:
        els[0].color = els[1].color = els[2].color = (*light, 1.0)
    else:
        els[0].color = (*shadow, 1.0)
        els[1].color = (*mix(shadow, light, 0.55), 1.0)
        els[2].color = (*light, 1.0)
    return m


def hazed(name, light, shadow, t, haze=(0.62, 0.45, 0.78), emission=0.0):
    return tcel(name, mix(light, haze, t), mix(shadow, mix(haze, (0.3, 0.2, 0.55), 0.4), t), emission)


def glow(name, rgb, strength=1.0):
    m = bpy.data.materials.get(name) or bpy.data.materials.new(name)
    m.use_nodes = True
    nt = m.node_tree
    nt.nodes.clear()
    out = nt.nodes.new("ShaderNodeOutputMaterial")
    em = nt.nodes.new("ShaderNodeEmission")
    em.inputs["Color"].default_value = (*rgb, 1.0)
    em.inputs["Strength"].default_value = strength
    nt.links.new(em.outputs["Emission"], out.inputs["Surface"])
    return m


def halo(name, rgb, strength=1.0, power=2.0, intensity=1.0):
    """Weicher runder Lichtschein (durchsichtig, Kreisverlauf), fuer Ebenen mit UV."""
    m = bpy.data.materials.get(name) or bpy.data.materials.new(name)
    m.use_nodes = True
    try:
        m.surface_render_method = "BLENDED"
    except Exception:
        pass
    nt = m.node_tree
    nt.nodes.clear()
    out = nt.nodes.new("ShaderNodeOutputMaterial")
    tc = nt.nodes.new("ShaderNodeTexCoord")
    sub = nt.nodes.new("ShaderNodeVectorMath")
    sub.operation = "SUBTRACT"
    sub.inputs[1].default_value = (0.5, 0.5, 0.0)
    ln = nt.nodes.new("ShaderNodeVectorMath")
    ln.operation = "LENGTH"
    dbl = nt.nodes.new("ShaderNodeMath")
    dbl.operation = "MULTIPLY"
    dbl.inputs[1].default_value = 2.0
    inv = nt.nodes.new("ShaderNodeMath")
    inv.operation = "SUBTRACT"
    inv.inputs[0].default_value = 1.0
    inv.use_clamp = True
    pw = nt.nodes.new("ShaderNodeMath")
    pw.operation = "POWER"
    pw.inputs[1].default_value = power
    sc = nt.nodes.new("ShaderNodeMath")
    sc.operation = "MULTIPLY"
    sc.inputs[1].default_value = intensity
    tr = nt.nodes.new("ShaderNodeBsdfTransparent")
    em = nt.nodes.new("ShaderNodeEmission")
    em.inputs["Color"].default_value = (*rgb, 1.0)
    em.inputs["Strength"].default_value = strength
    mx = nt.nodes.new("ShaderNodeMixShader")
    nt.links.new(tc.outputs["UV"], sub.inputs[0])
    nt.links.new(sub.outputs["Vector"], ln.inputs[0])
    nt.links.new(ln.outputs["Value"], dbl.inputs[0])
    nt.links.new(dbl.outputs["Value"], inv.inputs[1])
    nt.links.new(inv.outputs["Value"], pw.inputs[0])
    nt.links.new(pw.outputs["Value"], sc.inputs[0])
    nt.links.new(sc.outputs["Value"], mx.inputs["Fac"])
    nt.links.new(tr.outputs["BSDF"], mx.inputs[1])
    nt.links.new(em.outputs["Emission"], mx.inputs[2])
    nt.links.new(mx.outputs["Shader"], out.inputs["Surface"])
    return m


class Builder:
    """Viele Formen in EIN Objekt sammeln (schnell). Material je Form, Kontur optional."""

    def __init__(self, name):
        self.name = name
        self.bm = bmesh.new()
        self.mats = []

    def _mi(self, mat):
        if mat not in self.mats:
            self.mats.append(mat)
        return self.mats.index(mat)

    def add(self, mat, kind, loc=(0, 0, 0), scale=(1, 1, 1), rot=(0, 0, 0), seg=10, taper=0.0, smooth_=False):
        bm = self.bm
        if kind == "cube":
            r = bmesh.ops.create_cube(bm, size=2.0)
        elif kind == "sphere":
            r = bmesh.ops.create_uvsphere(bm, u_segments=seg, v_segments=max(4, seg // 2), radius=1.0)
        else:  # cyl (taper = Radius oben) oder cone (taper 0)
            r = bmesh.ops.create_cone(bm, cap_ends=True, segments=seg, radius1=1.0, radius2=taper, depth=2.0)
        verts = r["verts"]
        m = Matrix.LocRotScale(Vector(loc), Euler(tuple(math.radians(a) for a in rot), "XYZ").to_quaternion(), Vector(scale))
        bmesh.ops.transform(bm, matrix=m, verts=verts)
        mi = self._mi(mat)
        faces = set()
        for v in verts:
            for f in v.link_faces:
                faces.add(f)
        for f in faces:
            f.material_index = mi
            f.smooth = smooth_
        return self

    def finish(self, collection, outline=0.0):
        me = bpy.data.meshes.new(self.name)
        self.bm.normal_update()
        self.bm.to_mesh(me)
        self.bm.free()
        for m in self.mats:
            me.materials.append(m)
        o = bpy.data.objects.new(self.name, me)
        collection.objects.link(o)
        if outline:
            L.add_outline(o, outline)
        return o


# ================================================================== 1) Titelbild
scene = L.new_scene("Menu", 1920)
scene.render.resolution_x, scene.render.resolution_y = 1920, 1080
scene.render.film_transparent = False
scene.render.filter_size = 1.0
try:
    scene.eevee.taa_render_samples = 24
except AttributeError:
    pass
col = scene.collection

CAM_LOC = Vector((0.0, 0.0, 2.6))
cd = bpy.data.cameras.new("MenuCam")
cd.lens = 28.0
cd.sensor_fit = "HORIZONTAL"
cd.sensor_width = 36.0
cd.clip_start = 0.1
cd.clip_end = 3000.0
cam = bpy.data.objects.new("MenuCam", cd)
col.objects.link(cam)
cam.location = CAM_LOC
cam.rotation_euler = (math.radians(94.0), 0.0, 0.0)
scene.camera = cam

# ---- Himmel (nur fuer Kamerastrahlen), Umgebungslicht violett
world = bpy.data.worlds.new("MenuWorld")
world.use_nodes = True
nt = world.node_tree
nt.nodes.clear()
out = nt.nodes.new("ShaderNodeOutputWorld")
tc = nt.nodes.new("ShaderNodeTexCoord")
sep = nt.nodes.new("ShaderNodeSeparateXYZ")
ramp = nt.nodes.new("ShaderNodeValToRGB")
sky_stops = [(0.0, (0.5, 0.3, 0.55)), (0.34, (1.0, 0.62, 0.42)), (0.42, (1.0, 0.5, 0.45)), (0.52, (0.88, 0.36, 0.58)),
             (0.64, (0.5, 0.22, 0.6)), (0.8, (0.22, 0.13, 0.46)), (1.0, (0.09, 0.07, 0.3))]
els = ramp.color_ramp.elements
els[0].position, els[0].color = sky_stops[0][0], (*sky_stops[0][1], 1)
els[1].position, els[1].color = sky_stops[1][0] + 0.0001, (*sky_stops[1][1], 1)
for pos, c in sky_stops[2:]:
    e = els.new(pos)
    e.color = (*c, 1)
ramp.color_ramp.interpolation = "EASE"
bg_sky = nt.nodes.new("ShaderNodeBackground")
bg_amb = nt.nodes.new("ShaderNodeBackground")
bg_amb.inputs["Color"].default_value = (0.5, 0.4, 0.8, 1.0)
bg_amb.inputs["Strength"].default_value = 0.55
lp = nt.nodes.new("ShaderNodeLightPath")
mx = nt.nodes.new("ShaderNodeMixShader")
nt.links.new(tc.outputs["Window"], sep.inputs[0])
nt.links.new(sep.outputs["Y"], ramp.inputs["Fac"])
nt.links.new(ramp.outputs["Color"], bg_sky.inputs["Color"])
nt.links.new(lp.outputs["Is Camera Ray"], mx.inputs["Fac"])
nt.links.new(bg_amb.outputs["Background"], mx.inputs[1])
nt.links.new(bg_sky.outputs["Background"], mx.inputs[2])
nt.links.new(mx.outputs["Shader"], out.inputs["Surface"])
scene.world = world

# ---- Licht: rosa-goldenes Abendlicht von links vorne
ld = bpy.data.lights.new("MenuSun", "SUN")
ld.energy = 3.6
ld.color = (1.0, 0.78, 0.72)
sun_o = bpy.data.objects.new("MenuSun", ld)
col.objects.link(sun_o)
sun_o.rotation_euler = tuple(math.radians(a) for a in (58, 0, -32))

# ---- Gelaende (Hoehenfeld): sanfte Huegel, ferne Huegelketten, flacher Platz fuer die Stadt, Portalhuegel
TOWN = (7.0, 100.0)
PORTAL = (6.4, 17.0)


def terrain(x, y):
    h = 0.45 * math.sin(x * 0.11 + 1.3) * math.cos(y * 0.09) + 0.3 * math.sin(x * 0.23 + y * 0.17)
    far = smooth(55, 170, y)
    h += far * (7.5 * (0.5 + 0.5 * math.sin(x * 0.03 + 2.0)) + 4.5 * math.sin(y * 0.045 + x * 0.05) + 2.0 * math.sin(x * 0.11 + y * 0.07))
    h += 14.0 * smooth(170, 330, y) * (0.5 + 0.5 * math.sin(x * 0.02 + 0.5))
    d = math.hypot(x - TOWN[0], (y - TOWN[1]) * 1.4)
    m = smooth(34, 16, d)
    h = h * (1 - m) + 3.2 * m
    dp = math.hypot(x - PORTAL[0], y - PORTAL[1])
    h *= 1 - 0.8 * math.exp(-(dp / 7.0) ** 2)
    h += 2.0 * math.exp(-(dp / 4.2) ** 2)
    return h


def path_x(y):
    return -3.0 + (TOWN[0] + 3.0) * smooth(0, 88, y) + 3.0 * math.sin(y * 0.07) * (1 - smooth(60, 88, y))


xs = list(np.arange(-420, -30, 3.0)) + list(np.arange(-30, 42, 0.6)) + list(np.arange(42, 421, 3.0))
ys = [1.0]
while ys[-1] < 520:
    ys.append(ys[-1] + max(0.3, ys[-1] * 0.028))
nx, ny = len(xs), len(ys)
G = {
    "near": hazed("G_Nah", (0.2, 0.42, 0.34), (0.08, 0.12, 0.3), 0.05),
    "mid": hazed("G_Mitte", (0.2, 0.42, 0.34), (0.08, 0.12, 0.3), 0.35),
    "far": hazed("G_Fern", (0.2, 0.42, 0.34), (0.08, 0.12, 0.3), 0.62),
    "path": hazed("G_Pfad", (0.82, 0.66, 0.5), (0.4, 0.28, 0.4), 0.0),
    "path_m": hazed("G_PfadMitte", (0.82, 0.66, 0.5), (0.4, 0.28, 0.4), 0.38),
}
gmats = [G["near"], G["mid"], G["far"], G["path"], G["path_m"]]
bm = bmesh.new()
vg = [[bm.verts.new((xs[i], ys[j], terrain(xs[i], ys[j]))) for i in range(nx)] for j in range(ny)]
for j in range(ny - 1):
    for i in range(nx - 1):
        f = bm.faces.new((vg[j][i], vg[j][i + 1], vg[j + 1][i + 1], vg[j + 1][i]))
        cx, cy = (xs[i] + xs[i + 1]) / 2, (ys[j] + ys[j + 1]) / 2
        if cy < 55:
            mi = 0
        elif cy < 120:
            mi = 1
        else:
            mi = 2
        f.material_index = mi
        f.smooth = True
me = bpy.data.meshes.new("Gelaende")
bm.to_mesh(me)
bm.free()
for m in gmats:
    me.materials.append(m)
ground = bpy.data.objects.new("Gelaende", me)
col.objects.link(ground)

# Pfad als glattes Band (folgt dem Gelaende)
rb = bmesh.new()
rows = []
yy_ = 2.0
while yy_ < 90:
    cxp = path_x(yy_)
    wd = 1.1 + yy_ * 0.04
    rows.append([rb.verts.new((cxp + s * wd, yy_, terrain(cxp + s * wd, yy_) + 0.05)) for s in (-1, -0.5, 0, 0.5, 1)])
    yy_ += max(0.4, yy_ * 0.03)
for r0, r1 in zip(rows[:-1], rows[1:]):
    for k in range(4):
        fq = rb.faces.new((r0[k], r0[k + 1], r1[k + 1], r1[k]))
        fq.smooth = True
        fq.material_index = 0 if r0[0].co.y < 45 else 1
mr = bpy.data.meshes.new("Pfad")
rb.to_mesh(mr)
rb.free()
mr.materials.append(G["path"])
mr.materials.append(G["path_m"])
path_o = bpy.data.objects.new("Pfad", mr)
col.objects.link(path_o)


def gz(x, y):
    return terrain(x, y)


# ---- Frostgipfel (Gebirgskette, weit hinten)
rock = hazed("Fels", (0.42, 0.42, 0.7), (0.18, 0.16, 0.44), 0.55)
snow = hazed("Gipfelschnee", (0.97, 0.95, 1.0), (0.58, 0.55, 0.9), 0.4)
ice = hazed("Gipfeleis", (0.8, 0.9, 1.0), (0.45, 0.5, 0.85), 0.45)


def peak(bmesh_, cx, cy, h, r, seed):
    rr = random.Random(seed)
    segs = 10
    levels = [(0.0, 1.0), (0.3, 0.72), (0.55, 0.5), (0.78, 0.26), (1.0, 0.0)]
    base = -0.2 * h
    rings = []
    for k, (zf, rf) in enumerate(levels):
        ring = []
        for s in range(segs):
            a = 2 * math.pi * s / segs
            jr = 1 + rr.uniform(-0.14, 0.14) * (1 if 0 < k < 4 else 0.6)
            jz = rr.uniform(-0.05, 0.05) * h if 0 < k < 4 else 0.0
            if k == 4:
                ring.append(bmesh_.verts.new((cx + rr.uniform(-0.04, 0.04) * r, cy + rr.uniform(-0.04, 0.04) * r, base + (h - base) * zf)))
                break
            z = base + (h - base) * zf + jz
            ring.append(bmesh_.verts.new((cx + math.cos(a) * r * rf * jr, cy + math.sin(a) * r * rf * jr, z)))
        rings.append(ring)
    snowline = [rr.uniform(0.5, 0.68) for _ in range(segs)]
    for k in range(4):
        for s in range(segs):
            s2 = (s + 1) % segs
            if k == 3:
                vs = (rings[3][s], rings[3][s2], rings[4][0])
            else:
                vs = (rings[k][s], rings[k][s2], rings[k + 1][s2], rings[k + 1][s])
            f = bmesh_.faces.new(vs)
            zc = sum(v.co.z for v in vs) / len(vs)
            f.material_index = 1 if (zc - base) / (h - base) > snowline[s] else (2 if rr.random() < 0.08 else 0)
            f.smooth = False


bm = bmesh.new()
PEAKS = [(-250, 310, 46, 85), (-170, 330, 36, 70), (-95, 320, 44, 80), (-20, 350, 50, 95), (60, 330, 58, 90),
         (135, 340, 64, 100), (210, 320, 52, 85), (275, 340, 46, 90), (-310, 300, 40, 80), (20, 300, 38, 70),
         (95, 295, 40, 65), (-60, 290, 30, 60), (170, 290, 42, 70)]
for i, (px, py, ph, pr) in enumerate(PEAKS):
    peak(bm, px, py, ph * 1.2, pr, i + 11)
me = bpy.data.meshes.new("Frostgipfel")
bm.to_mesh(me)
bm.free()
for m in (rock, snow, ice):
    me.materials.append(m)
peaks_o = bpy.data.objects.new("Frostgipfel", me)
col.objects.link(peaks_o)
L.add_outline(peaks_o, 0.9)

# ---- Stadt mit Mauer (in der Ferne)
T_STONE = hazed("S_Stein", (0.68, 0.62, 0.78), (0.3, 0.25, 0.55), 0.06)
T_STONE_D = hazed("S_SteinDunkel", (0.6, 0.55, 0.7), (0.3, 0.25, 0.5), 0.1)
T_WALL = hazed("S_Wand", (0.92, 0.78, 0.66), (0.42, 0.3, 0.5), 0.06)
T_ROOF_R = hazed("S_DachRot", (0.82, 0.3, 0.3), (0.4, 0.14, 0.34), 0.1)
T_ROOF_B = hazed("S_DachBlau", (0.3, 0.42, 0.85), (0.16, 0.2, 0.5), 0.1)
T_ROOF_G = hazed("S_DachGold", (0.95, 0.7, 0.28), (0.5, 0.3, 0.35), 0.1)
T_WIN = glow("S_Fenster", (1.0, 0.78, 0.32), 3.0)
T_DARK = tcel("S_Tor", (0.12, 0.08, 0.2), (0.06, 0.04, 0.12), flat=True)
tz = 3.2
tb = Builder("Stadt")
N = 18
RX, RY = 13.0, 7.0
pts = [(TOWN[0] + RX * math.cos(2 * math.pi * i / N - math.pi / 2), TOWN[1] + RY * math.sin(2 * math.pi * i / N - math.pi / 2)) for i in range(N)]
gate_i = 0
for i in range(N):
    a, b = pts[i], pts[(i + 1) % N]
    mx_, my_ = (a[0] + b[0]) / 2, (a[1] + b[1]) / 2
    ln_ = math.hypot(b[0] - a[0], b[1] - a[1])
    ang = math.degrees(math.atan2(b[1] - a[1], b[0] - a[0]))
    tb.add(T_STONE, "cube", (mx_, my_, tz + 1.5), (ln_ / 2 + 0.2, 0.55, 1.6), (0, 0, ang))
    tb.add(T_STONE_D, "cube", (mx_, my_, tz + 3.2), (ln_ / 2 + 0.25, 0.7, 0.16), (0, 0, ang))
    for c in range(3):  # Zinnen
        t = (c + 0.5) / 3 - 0.5
        tb.add(T_STONE, "cube", (mx_ + t * ln_ * math.cos(math.radians(ang)), my_ + t * ln_ * math.sin(math.radians(ang)), tz + 3.65),
               (0.4, 0.45, 0.3), (0, 0, ang))
for i in range(0, N, 3):
    x, y = pts[i]
    tb.add(T_STONE, "cyl", (x, y, tz + 2.6), (1.35, 1.35, 3.0), seg=10, smooth_=True)
    tb.add(T_STONE_D, "cyl", (x, y, tz + 5.7), (1.6, 1.6, 0.2), seg=10, smooth_=True)
    tb.add(T_ROOF_R if i % 6 == 0 else T_ROOF_B, "cone", (x, y, tz + 7.4), (1.7, 1.7, 1.7), seg=10)
    tb.add(T_WIN, "cube", (x, y - 1.3, tz + 4.4), (0.18, 0.05, 0.3))
# Tor
gx, gy = pts[gate_i]
tb.add(T_DARK, "cube", (gx, gy - 0.5, tz + 1.1), (1.1, 0.4, 1.1))
tb.add(T_DARK, "cyl", (gx, gy - 0.5, tz + 2.1), (1.1, 1.1, 0.4), (90, 0, 0), seg=12)
tb.add(T_WIN, "sphere", (gx - 1.5, gy - 0.9, tz + 2.2), (0.18, 0.18, 0.18), seg=6)
tb.add(T_WIN, "sphere", (gx + 1.5, gy - 0.9, tz + 2.2), (0.18, 0.18, 0.18), seg=6)
# Haeuser
HOUSES = [(-7, 0.5, 3.4, 2.4, 3.0, "r"), (-3.5, -1.5, 3.0, 2.2, 2.6, "b"), (3.5, -1.5, 3.2, 2.4, 3.2, "r"), (7.5, 0.5, 3.4, 2.5, 2.8, "g"),
          (-6, 3.5, 3.0, 2.2, 3.4, "b"), (5.5, 3.5, 3.4, 2.4, 3.0, "r"), (-9, 2.5, 2.6, 2.0, 2.4, "g"), (9.5, 2.0, 2.6, 2.0, 2.6, "b"),
          (0.0, 4.5, 3.4, 2.4, 3.6, "r"), (-2.5, 2.5, 2.6, 2.0, 3.0, "g"), (2.8, 2.2, 2.6, 2.0, 3.2, "b")]
ROOFS = {"r": T_ROOF_R, "b": T_ROOF_B, "g": T_ROOF_G}
for hx, hy, w, d, hh, rc in HOUSES:
    x, y = TOWN[0] + hx, TOWN[1] + hy
    tb.add(T_WALL, "cube", (x, y, tz + hh / 2), (w / 2, d / 2, hh / 2))
    tb.add(ROOFS[rc], "cone", (x, y, tz + hh + w * 0.3), (w * 0.78, d * 0.95, w * 0.34), (0, 0, 45), seg=4)
    tb.add(T_WIN, "cube", (x - w * 0.22, y - d / 2 - 0.02, tz + hh * 0.6), (0.2, 0.04, 0.28))
    tb.add(T_WIN, "cube", (x + w * 0.22, y - d / 2 - 0.02, tz + hh * 0.6), (0.2, 0.04, 0.28))
# Bergfried / Schloss in der Mitte
kx, ky = TOWN[0] + 0.5, TOWN[1] + 6.2
tb.add(T_STONE, "cube", (kx, ky, tz + 5.5), (3.0, 2.6, 5.5))
tb.add(T_STONE_D, "cube", (kx, ky, tz + 11.2), (3.3, 2.9, 0.3))
for (ox, oy) in ((-3.0, -2.6), (3.0, -2.6), (-3.0, 2.6), (3.0, 2.6)):
    tb.add(T_STONE, "cyl", (kx + ox, ky + oy, tz + 6.5), (1.0, 1.0, 6.5), seg=8, smooth_=True)
    tb.add(T_ROOF_B, "cone", (kx + ox, ky + oy, tz + 14.4), (1.35, 1.35, 1.8), seg=8)
tb.add(T_ROOF_R, "cone", (kx, ky, tz + 14.2), (2.6, 2.3, 3.0), (0, 0, 45), seg=4)
tb.add(T_ROOF_G, "cyl", (kx, ky, tz + 18.6), (0.07, 0.07, 1.6), seg=6)
tb.add(T_ROOF_R, "cube", (kx + 0.7, ky, tz + 19.7), (0.7, 0.05, 0.4))
for wx in (-1.4, 0.0, 1.4):
    tb.add(T_WIN, "cube", (kx + wx, ky - 2.62, tz + 7.5), (0.25, 0.05, 0.5))
    tb.add(T_WIN, "cube", (kx + wx, ky - 2.62, tz + 4.2), (0.22, 0.05, 0.4))
town_o = tb.finish(col, outline=0.0)
SC = Matrix.Translation((TOWN[0], TOWN[1], tz)) @ Matrix.Diagonal((1.15, 1.15, 1.15, 1.0)) @ Matrix.Translation((-TOWN[0], -TOWN[1], -tz))
town_o.data.transform(SC)
L.add_outline(town_o, 0.17)
# warmer Lichtschein ueber der Stadt
glow_amb = halo("H_Amber", (1.0, 0.55, 0.3), 1.4, 2.2, 0.16)


def billboard(name, mat, loc, size, col_=None, rot_x=94.0):
    bmq = bmesh.new()
    bmesh.ops.create_grid(bmq, x_segments=1, y_segments=1, size=size / 2)
    uv = bmq.loops.layers.uv.new("UVMap")
    for f in bmq.faces:
        for lp_ in f.loops:
            lp_[uv].uv = (lp_.vert.co.x / size + 0.5, lp_.vert.co.y / size + 0.5)
    mq = bpy.data.meshes.new(name)
    bmq.to_mesh(mq)
    bmq.free()
    mq.materials.append(mat)
    o = bpy.data.objects.new(name, mq)
    (col_ or col).objects.link(o)
    o.location = loc
    o.rotation_euler = (math.radians(rot_x), 0, 0)
    try:
        o.visible_shadow = False
    except AttributeError:
        pass
    return o


billboard("Stadtlicht", glow_amb, (TOWN[0], TOWN[1] - 4, tz + 10), 60)

# ---- Waldrand rechts (Fichten) und ein paar Baeume links weit hinten
spr_a = hazed("B_Fichte", (0.18, 0.4, 0.38), (0.07, 0.14, 0.3), 0.1)
spr_b = hazed("B_FichteHell", (0.26, 0.52, 0.42), (0.1, 0.2, 0.34), 0.1)
trunk = hazed("B_Stamm", (0.4, 0.26, 0.2), (0.16, 0.1, 0.22), 0.1)


def spruce(b, x, y, s, light=False):
    z = gz(x, y)
    mat = spr_b if light else spr_a
    b.add(trunk, "cyl", (x, y, z + 0.5 * s), (0.22 * s, 0.22 * s, 0.6 * s), seg=6)
    for k, (zz, rr_, hh) in enumerate(((1.0, 1.7, 1.5), (2.2, 1.35, 1.35), (3.3, 0.95, 1.2))):
        b.add(mat, "cone", (x, y, z + zz * s + hh * s * 0.5), (rr_ * s, rr_ * s, hh * s), seg=8)


forest_near = Builder("Wald_Nah")
forest_far = Builder("Wald_Fern")
for _ in range(90):
    x = rnd.uniform(22, 62)
    y = rnd.uniform(42, 72)
    if math.hypot(x - PORTAL[0], y - PORTAL[1]) < 9:
        continue
    if abs(x - path_x(y)) < 3.5:
        continue
    spruce(forest_near, x, y, rnd.uniform(1.8, 2.6), rnd.random() < 0.3)
for _ in range(60):
    x = rnd.uniform(40, 120)
    y = rnd.uniform(46, 90)
    if math.hypot(x - TOWN[0], (y - TOWN[1]) * 1.4) < 22:
        continue
    spruce(forest_far, x, y, rnd.uniform(2.0, 3.0), rnd.random() < 0.3)
for _ in range(22):
    x = rnd.uniform(-110, -45)
    y = rnd.uniform(60, 120)
    spruce(forest_far, x, y, rnd.uniform(2.0, 3.0), rnd.random() < 0.3)
forest_near.finish(col, outline=0.07)
forest_far.finish(col, outline=0.16)

# ---- Portal auf dem Huegel
px, py = PORTAL
pz = gz(px, py)
st = hazed("P_Stein", (0.62, 0.58, 0.75), (0.26, 0.22, 0.5), 0.0)
st_d = hazed("P_SteinDunkel", (0.45, 0.4, 0.6), (0.2, 0.16, 0.42), 0.0)
p_light = glow("P_Licht", (0.72, 0.4, 1.0), 1.5)
p_dark = glow("P_Dunkel", (0.4, 0.12, 0.8), 1.3)
p_core = glow("P_Kern", (0.95, 0.8, 1.0), 2.0)
rune = glow("P_Rune", (0.62, 0.3, 1.0), 1.6)
pb = Builder("Portal")
cz = pz + 2.35
# Plattform
pb.add(st, "cyl", (px, py, pz + 0.12), (2.9, 2.9, 0.22), seg=18, smooth_=False)
pb.add(st_d, "cyl", (px, py, pz + 0.34), (2.4, 2.4, 0.16), seg=18)
# Bogen: Ring aus Steinbloecken + Saeulen
for k in range(22):
    a = math.pi * 2 * k / 22
    if math.sin(a) < -0.45:
        continue
    cx_, cz_ = px + 1.55 * math.cos(a), cz + 1.55 * math.sin(a)
    pb.add(st if k % 2 == 0 else st_d, "cube", (cx_, py, cz_), (0.28, 0.32, 0.3), (0, -math.degrees(a), 0))
for sx in (-1, 1):
    pb.add(st, "cube", (px + sx * 1.55, py, pz + 0.95), (0.3, 0.33, 0.8))
    pb.add(st_d, "cube", (px + sx * 1.55, py, pz + 0.55), (0.37, 0.4, 0.14))
pb.add(rune, "sphere", (px, py - 0.34, cz + 1.88), (0.17, 0.08, 0.17), seg=8)
for sx in (-1, 1):
    for dz in (-0.2, 0.2):
        pb.add(rune, "cube", (px + sx * 1.55, py - 0.34, pz + 1.1 + dz * 2.0), (0.06, 0.03, 0.1))
portal_o = pb.finish(col, outline=0.055)
# Leuchtende Innenflaeche
pi = Builder("PortalInnen")
pi.add(p_dark, "cyl", (px, py + 0.05, cz), (1.35, 1.35, 0.03), (90, 0, 0), seg=28)
pi.add(p_light, "cyl", (px, py - 0.02, cz), (1.05, 1.05, 0.03), (90, 0, 0), seg=28)
pi.add(p_dark, "cyl", (px, py - 0.05, cz), (0.75, 0.75, 0.03), (90, 0, 0), seg=28)
pi.add(p_light, "cyl", (px, py - 0.08, cz), (0.5, 0.5, 0.03), (90, 0, 0), seg=28)
pi.add(p_core, "cyl", (px, py - 0.11, cz), (0.26, 0.26, 0.03), (90, 0, 0), seg=28)
pi.finish(col)
# Lichtschein, Bodenlicht, Funken
billboard("PortalSchein", halo("H_Violett", (0.6, 0.25, 1.0), 1.4, 1.8, 0.5), (px, py + 0.4, cz), 11)
billboard("PortalSchein2", halo("H_VioletHell", (0.8, 0.6, 1.0), 1.6, 3.0, 0.5), (px, py - 0.2, cz), 5)
floor_g = billboard("PortalBoden", halo("H_Boden", (0.6, 0.25, 1.0), 1.3, 2.0, 0.4), (px, py - 0.6, pz + 0.5), 10, rot_x=18.0)
sp = Builder("Funken")
sp_m = glow("P_Funke", (0.9, 0.75, 1.0), 2.5)
for _ in range(16):
    a = rnd.uniform(0, math.tau)
    r = rnd.uniform(0.8, 3.6)
    sp.add(sp_m, "sphere", (px + r * math.cos(a) * 1.2, py - 0.5 + rnd.uniform(-1.5, 1), cz + rnd.uniform(-1.8, 2.2)),
           (0.04, 0.04, 0.04) if rnd.random() < 0.7 else (0.07, 0.07, 0.07), seg=6)
sp.finish(col)
# Steine und Gras auf dem Huegel
rk = Builder("Huegelsteine")
for _ in range(9):
    a = rnd.uniform(0, math.tau)
    r = rnd.uniform(3.6, 5.2)
    x, y = px + r * math.cos(a) * 1.2, py + r * math.sin(a) * 0.8
    s = rnd.uniform(0.15, 0.3)
    rk.add(st, "sphere", (x, y, gz(x, y) + s * 0.4), (s * 1.4, s, s * 0.8), (0, 0, rnd.uniform(0, 90)), seg=7)
rk.finish(col, outline=0.04)

# ---- Grasbueschel (dezent, nur im Mittelfeld und rechts)
tuft_m = hazed("B_Gras", (0.16, 0.36, 0.3), (0.07, 0.11, 0.26), 0.08)
tb2 = Builder("Gras")
for _ in range(120):
    x = rnd.uniform(-35, 45)
    y = rnd.uniform(8, 60)
    if x < -4 + y * 0.05 and rnd.random() < 0.7:
        continue
    if abs(x - path_x(y)) < 2.5 or math.hypot(x - PORTAL[0], y - PORTAL[1]) < 5:
        continue
    z0 = gz(x, y)
    for k in range(3):
        a = rnd.uniform(0, math.tau)
        tb2.add(tuft_m, "cone", (x + 0.12 * math.cos(a), y + 0.12 * math.sin(a), z0 + 0.22), (0.06, 0.06, 0.28), (rnd.uniform(-15, 15), rnd.uniform(-15, 15), 0), seg=4)
tb2.finish(col, outline=0.0)

# ---- Wolken (tief am Horizont, links und rechts) und Mond, Sterne
cl_l = tcel("W_Wolke", (0.95, 0.62, 0.72), (0.45, 0.28, 0.6))
cl_h = tcel("W_WolkeHell", (1.0, 0.8, 0.7), (0.6, 0.4, 0.65))
cb = Builder("Wolken")
for cx_, cz_, w in ((-200, 52, 70), (-95, 36, 50), (170, 60, 60), (260, 40, 80), (40, 28, 45), (-290, 30, 70)):
    for k in range(5):
        ox = (k - 2) * w * 0.35
        cb.add(cl_l if k % 2 else cl_h, "sphere", (cx_ + ox, 520, cz_ + rnd.uniform(-2, 3)),
               (w * 0.28, w * 0.1, w * 0.09 * (1.0 - abs(k - 2) * 0.2)), seg=10, smooth_=False)
cb.finish(col)
moon_m = glow("M_Mond", (1.0, 0.95, 0.82), 1.3)
moon_c = glow("M_Krater", (0.85, 0.78, 0.78), 1.2)
mp = Vector((190.0, 520.0, 168.0))
mb = Builder("Mond")
mb.add(moon_m, "sphere", tuple(mp), (15, 15, 15), seg=24, smooth_=True)
mb.finish(col)
billboard("MondSchein", halo("H_Mond", (0.8, 0.75, 1.0), 1.2, 2.5, 0.45), (mp.x, mp.y + 5, mp.z), 110)
stars = Builder("Sterne")
star_m = glow("M_Stern", (1.0, 0.95, 0.9), 5.0)
for _ in range(220):
    az = rnd.uniform(-48, 48)
    el = rnd.uniform(14, 52)
    d = 900.0
    x = d * math.sin(math.radians(az)) * math.cos(math.radians(el))
    y = d * math.cos(math.radians(az)) * math.cos(math.radians(el))
    z = d * math.sin(math.radians(el)) + 2.6
    r = rnd.choice((0.7, 0.9, 1.1, 1.5, 2.2))
    stars.add(star_m, "sphere", (x, y, z), (r, r, r), seg=4)
stars.finish(col)

# ---- Rendern + leichte Vignette
tmp = os.path.join(OUT, "title_background.png")
L.render_still(scene, tmp)
img = bpy.data.images.load(tmp, check_existing=False)
w, h = img.size
a = np.empty(w * h * 4, dtype=np.float32)
img.pixels.foreach_get(a)
a = a.reshape(h, w, 4)
yy, xx = np.mgrid[0:h, 0:w]
nxx, nyy = (xx / (w - 1) - 0.5) * 2, (yy / (h - 1) - 0.5) * 2
vig = 1.0 - 0.22 * np.clip((nxx ** 2 * 0.7 + nyy ** 2 * 0.9) - 0.25, 0, 1)
left = 1.0 - 0.14 * np.clip(1.0 - (xx / (w - 1)) / 0.3, 0, 1)   # linke Seite etwas ruhiger/dunkler
a[..., :3] *= (vig * left)[..., None]
img.pixels.foreach_set(a.ravel())
img.filepath_raw = tmp
img.file_format = "PNG"
img.save()
bpy.data.images.remove(img)
bg_count = 1

# ================================================================== 2) Logo-Rahmen (1200x360, durchsichtig)
lscene = L.new_scene("Logo", 1200)
lscene.render.resolution_x, lscene.render.resolution_y = 1200, 360
lscene.render.filter_size = 1.0
lscene.render.film_transparent = True
lscene.world = bpy.data.worlds.new("LogoWorld")
lscene.world.use_nodes = True
lscene.world.node_tree.nodes["Background"].inputs["Color"].default_value = (0.75, 0.7, 0.85, 1)
lscene.world.node_tree.nodes["Background"].inputs["Strength"].default_value = 0.7
lcol = lscene.collection
lcd = bpy.data.cameras.new("LogoCam")
lcd.type = "ORTHO"
lcd.ortho_scale = 6.0
lcam = bpy.data.objects.new("LogoCam", lcd)
lcol.objects.link(lcam)
lcam.location = (0, -10, 0)
lcam.rotation_euler = (math.radians(90), 0, 0)
lscene.camera = lcam
lsun = bpy.data.lights.new("LogoSun", "SUN")
lsun.energy = 3.0
lso = bpy.data.objects.new("LogoSun", lsun)
lcol.objects.link(lso)
lso.rotation_euler = tuple(math.radians(a_) for a_ in (68, 0, -28))

bronze = tcel("L_Bronze", (0.6, 0.38, 0.2), (0.22, 0.12, 0.1))
bronze_d = tcel("L_BronzeDunkel", (0.42, 0.25, 0.14), (0.16, 0.08, 0.08))
gold = tcel("L_Gold", (1.0, 0.8, 0.3), (0.52, 0.34, 0.15), emission=0.15)
gold_l = tcel("L_GoldHell", (1.0, 0.92, 0.55), (0.7, 0.5, 0.22), emission=0.25)
inner = tcel("L_Innen", (0.11, 0.08, 0.2), (0.05, 0.03, 0.1))
inner_l = tcel("L_InnenRand", (0.2, 0.14, 0.32), (0.08, 0.05, 0.16))
gem = tcel("L_Edelstein", (0.7, 0.3, 0.95), (0.35, 0.12, 0.6), emission=0.7)
lb = Builder("Logo")
T = 0.07


def lcube(mat, cx_, cz_, hx, hz, y=0.0, hy=T):
    lb.add(mat, "cube", (cx_, y, cz_), (hx, hy, hz), smooth_=True)


# Platten (von hinten nach vorn), nur Bevel ueber smooth cube-Look
for mat, hx, hz, y in ((bronze_d, 2.88, 0.80, 0.0), (bronze, 2.80, 0.72, -0.04), (gold, 2.70, 0.63, -0.08),
                       (bronze_d, 2.58, 0.52, -0.12), (inner_l, 2.50, 0.45, -0.14), (inner, 2.42, 0.38, -0.17)):
    lb.add(mat, "cube", (0, y, 0), (hx, T, hz), smooth_=True)
# Eckzier
for sx in (-1, 1):
    for sz in (-1, 1):
        lb.add(gold, "sphere", (sx * 2.62, -0.14, sz * 0.6), (0.17, 0.1, 0.17), seg=12, smooth_=True)
        lb.add(gold_l, "sphere", (sx * 2.62, -0.2, sz * 0.6), (0.075, 0.05, 0.075), seg=8, smooth_=True)
# Seitenzipfel
for sx in (-1, 1):
    lb.add(gold, "cone", (sx * 2.88, -0.06, 0), (0.2, 0.08, 0.32), (0, sx * 90, 0), seg=4, smooth_=False)
    lb.add(gem, "sphere", (sx * 2.58, -0.2, 0), (0.13, 0.07, 0.13), seg=10, smooth_=True)
# Nieten
for i in range(15):
    x = -2.45 + i * (4.9 / 14)
    for sz in (-1, 1):
        lb.add(gold_l, "sphere", (x, -0.14, sz * 0.67), (0.045, 0.035, 0.045), seg=6, smooth_=True)
# Krone oben und Fuss unten
lb.add(gold, "cube", (0, -0.1, 0.72), (0.34, 0.07, 0.34), (0, 45, 0) if False else (0, 0, 45), smooth_=False)
lb.add(bronze_d, "cube", (0, -0.14, 0.72), (0.22, 0.06, 0.22), (0, 0, 45), smooth_=False)
lb.add(gem, "sphere", (0, -0.24, 0.72), (0.14, 0.07, 0.14), seg=10, smooth_=True)
for sx in (-1, 1):
    lb.add(gold, "cone", (sx * 0.62, -0.08, 0.7), (0.17, 0.07, 0.27), (0, 0, -sx * 80), seg=4)
    lb.add(gold, "cone", (sx * 1.0, -0.08, 0.68), (0.1, 0.06, 0.17), (0, 0, -sx * 85), seg=4)
lb.add(gold, "cube", (0, -0.1, -0.72), (0.26, 0.07, 0.17), (0, 0, 45), smooth_=False)
lb.add(gem, "sphere", (0, -0.2, -0.72), (0.09, 0.05, 0.09), seg=8, smooth_=True)
logo_o = lb.finish(lcol, outline=0.03)
L.render_still(lscene, os.path.join(OUT, "logo_frame.png"))

# ================================================================== 3) Zielpfeil (128x128, nach +x)
ascene = L.new_scene("Pfeil", 128)
ascene.render.filter_size = 1.0
ascene.world = bpy.data.worlds.new("PfeilWorld")
ascene.world.use_nodes = True
ascene.world.node_tree.nodes["Background"].inputs["Color"].default_value = (0.8, 0.78, 0.85, 1)
ascene.world.node_tree.nodes["Background"].inputs["Strength"].default_value = 0.8
acol = ascene.collection
acd = bpy.data.cameras.new("PfeilCam")
acd.type = "ORTHO"
acd.ortho_scale = 1.5
acam = bpy.data.objects.new("PfeilCam", acd)
acol.objects.link(acam)
acam.location = (0, -10, 0)
acam.rotation_euler = (math.radians(90), 0, 0)
ascene.camera = acam
asun = bpy.data.lights.new("PfeilSun", "SUN")
asun.energy = 3.0
aso = bpy.data.objects.new("PfeilSun", asun)
acol.objects.link(aso)
aso.rotation_euler = tuple(math.radians(a_) for a_ in (68, 0, -28))
a_gold = tcel("A_Gold", (1.0, 0.82, 0.18), (0.75, 0.45, 0.08), emission=0.35)
a_hi = tcel("A_GoldHell", (1.0, 0.95, 0.55), (1.0, 0.82, 0.3), emission=0.6)


def arrow_poly(sc, ty):
    # Pfeil nach +x, Mitte bei x = 0, z = 0
    return [(-0.6 * sc, 0.17 * sc), (0.02 * sc, 0.17 * sc), (0.02 * sc, 0.42 * sc), (0.6 * sc, 0.0), (0.02 * sc, -0.42 * sc),
            (0.02 * sc, -0.17 * sc), (-0.6 * sc, -0.17 * sc)]


def arrow_mesh(name, mat, sc, y0, thick, bev):
    bma = bmesh.new()
    front = [bma.verts.new((x, y0 - thick, z)) for x, z in arrow_poly(sc, 0)]
    back = [bma.verts.new((x, y0 + thick, z)) for x, z in arrow_poly(sc, 0)]
    bma.faces.new(front)
    bma.faces.new(list(reversed(back)))
    n = len(front)
    for i in range(n):
        j = (i + 1) % n
        bma.faces.new((front[j], front[i], back[i], back[j]))
    bmesh.ops.recalc_face_normals(bma, faces=list(bma.faces))
    try:
        bmesh.ops.bevel(bma, geom=list(bma.edges), offset=bev, segments=2, affect="EDGES")
    except Exception as ex:  # noqa: BLE001
        print("Bevel uebersprungen:", ex)
    me_ = bpy.data.meshes.new(name)
    bma.to_mesh(me_)
    bma.free()
    for p in me_.polygons:
        p.use_smooth = True
    me_.materials.append(mat)
    o = bpy.data.objects.new(name, me_)
    acol.objects.link(o)
    return o


a1 = arrow_mesh("PfeilKoerper", a_gold, 1.0, 0.0, 0.07, 0.035)
L.add_outline(a1, 0.045)
a2 = arrow_mesh("PfeilGlanz", a_hi, 0.66, -0.09, 0.03, 0.015)
L.render_still(ascene, os.path.join(OUT, "target_arrow.png"))

bpy.ops.wm.save_as_mainfile(filepath=BLEND_PATH)
files = ["title_background.png", "logo_frame.png", "target_arrow.png"]
ok = all(os.path.exists(os.path.join(OUT, f)) for f in files)
print(f"RESULT files={len(files)} ok={ok}")
