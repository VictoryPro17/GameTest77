"""RE:BORN - Frostwelt (Meilenstein 9a, Comic-Look)

Ausgabe: Assets/_Project/Sprites/Frost (und drei Icons in Sprites/Items). Eigene Datei: Art/Blender/Reborn_Frost.blend

Kacheln (64 px, senkrecht von oben, nahtlos, PPU 64): Frost_Snow_0, Frost_Snow_1, Frost_Path, Frost_Wall.
Deko (Kamera wie build_props.py, Fusspunkt-Pivot 0.5, 0.2138): frozen_tree (256), ice_rock, snow_pile, ice_crystal,
ice_pillar (je 128; Saeule 0,7 breit, 1,4 hoch).
Siegel-Portal (256 px, Rahmung wie Base/portal_0.png, Fusspunkt-Pivot): sealed_portal, frost_portal_0..3.

Effekte (senkrecht von oben gerendert, 128 Pixel pro Welteinheit; Alpha nachtraeglich abgeschwaecht):
  breath_cone.png      512x384, Pivot (0, 0.5)     Kegel nach rechts (+x), Spitze links mittig, 4 m lang, 3 m breit am Ende
  breath_warn.png      512x384, Pivot (0, 0.5)     gleiche Form, rot-orange Warnflaeche
  ice_warn_circle.png  256x256, Pivot (0.5, 0.5)   Warnkreis 2 m Durchmesser
  icicle_0..2.png      128x128, Pivot (0.5, 0.2138) Eiszapfen: faellt, schlaegt ein, Scherben (Kamera 45 Grad, Fusspunkt)
  wing_wave_0..2.png   512x512, Pivot (0.5, 0.5)   Flügelschlag-Ring, 4 m Durchmesser (Ring wird groesser)
  blizzard.png         512x512, Pivot (0.5, 0.5)   Schneesturm-Wirbel, halbtransparent, kachelbar
  ice_shard.png        64x64,   Pivot (0.5, 0.5)   Eissplitter-Geschoss nach rechts
Icons (48 px): Item_frostschluessel, Item_drachenschuppe, Item_eiskristall.

Aufruf:  blender.exe -b --factory-startup --python build_frost.py -- <projektordner>
"""
import bpy
import math
import os
import random
import sys
import numpy as np
from mathutils import Vector

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import reborn_lib as L  # noqa: E402

argv = sys.argv[sys.argv.index("--") + 1:] if "--" in sys.argv else []
PROJECT = argv[0] if argv else os.path.abspath(os.path.join(os.path.dirname(__file__), "..", "..", ".."))
OUT = os.path.join(PROJECT, "Assets", "_Project", "Sprites", "Frost")
ICON_DIR = os.path.join(PROJECT, "Assets", "_Project", "Sprites", "Items")
BLEND_PATH = os.path.join(PROJECT, "Art", "Blender", "Reborn_Frost.blend")
os.makedirs(OUT, exist_ok=True)
os.makedirs(ICON_DIR, exist_ok=True)

bpy.ops.wm.read_factory_settings(use_empty=True)
scene = bpy.context.scene
scene.name = "Frost"
L.setup_scene(scene)
cam = L.camera(scene, target=(0, 0, 0.85), ortho=2.1)
L.sun(scene)
# senkrechte Kamera fuer Kacheln und Bodeneffekte
cd = bpy.data.cameras.new("TopCam")
cd.type = "ORTHO"
tcam = bpy.data.objects.new("TopCam", cd)
scene.collection.objects.link(tcam)
# Icon-Kamera
icd = bpy.data.cameras.new("IconCam")
icd.type = "ORTHO"
icam = bpy.data.objects.new("IconCam", icd)
scene.collection.objects.link(icam)


def fcel(name, light, shadow, emission=0.0, flat=False):
    """Comic-Material mit eigenem (blauem) Schattenton statt nur dunkler."""
    m = L.cel(name, light, emission=emission)
    ramp = [n for n in m.node_tree.nodes if n.type == "VALTORGB"][0]
    els = ramp.color_ramp.elements
    if flat:
        els[0].color = els[1].color = els[2].color = (*light, 1.0)
    else:
        els[0].color = (*shadow, 1.0)
        els[1].color = (*[shadow[i] + (light[i] - shadow[i]) * 0.55 for i in range(3)], 1.0)
        els[2].color = (*light, 1.0)
    return m


def fx(name, rgb, emission=0.0):
    return fcel(name, rgb, rgb, emission, flat=True)


M = {"snow": fcel("FSchnee", (0.95, 0.98, 1.0), (0.5, 0.64, 0.88)),
     "snow2": fcel("FSchnee2", (0.84, 0.92, 1.0), (0.42, 0.56, 0.84)),
     "snow_sh": fcel("FSchneeSchatten", (0.7, 0.82, 0.97), (0.4, 0.54, 0.82)),
     "white": fcel("FWeiss", (1.0, 1.0, 1.0), (0.72, 0.84, 0.99)),
     "ice": fcel("FEis", (0.56, 0.83, 0.98), (0.2, 0.4, 0.74)),
     "ice_l": fcel("FEisHell", (0.8, 0.94, 1.0), (0.38, 0.62, 0.9)),
     "ice_d": fcel("FEisDunkel", (0.3, 0.55, 0.86), (0.1, 0.22, 0.52)),
     "deep": fcel("FTiefblau", (0.18, 0.34, 0.62), (0.06, 0.12, 0.34)),
     "deep2": fcel("FTiefblau2", (0.24, 0.42, 0.72), (0.08, 0.16, 0.4)),
     "glow": fcel("FLeuchten", (0.45, 0.96, 1.0), (0.3, 0.8, 0.96), emission=0.9),
     "glow_d": fcel("FLeuchtenDunkel", (0.2, 0.7, 0.9), (0.12, 0.5, 0.8), emission=0.7),
     "spark": fcel("FFunkeln", (1.0, 1.0, 1.0), (1.0, 1.0, 1.0), emission=0.9, flat=True),
     "wood": fcel("FHolz", (0.42, 0.36, 0.42), (0.18, 0.16, 0.28)),
     "wood_d": fcel("FHolzDunkel", (0.3, 0.25, 0.32), (0.12, 0.1, 0.22)),
     "stone": fcel("FStein", (0.58, 0.6, 0.68), (0.28, 0.3, 0.44)),
     "stone_d": fcel("FSteinDunkel", (0.42, 0.44, 0.54), (0.2, 0.22, 0.36)),
     "iron": fcel("FEisen", (0.36, 0.38, 0.46), (0.14, 0.15, 0.26)),
     "seal": fcel("FSiegel", (0.3, 0.55, 0.62), (0.2, 0.4, 0.5), emission=0.25),
     "gold": fcel("FSilber", (0.86, 0.92, 1.0), (0.4, 0.52, 0.78)),
     "gem": fcel("FJuwel", (0.3, 0.95, 1.0), (0.2, 0.75, 0.95), emission=1.0),
     "portal": fcel("FPortal", (0.2, 0.62, 0.98), (0.14, 0.45, 0.9), emission=0.6),
     "portal_l": fcel("FPortalHell", (0.62, 0.95, 1.0), (0.4, 0.8, 0.98), emission=0.8),
     # Effekte (flach, ohne Schattierung)
     "x_mist": fx("XNebel", (0.12, 0.5, 1.0), 0.1),
     "x_mist2": fx("XNebel2", (0.45, 0.85, 1.0), 0.1),
     "x_core": fx("XKern", (1.0, 1.0, 1.0)),
     "x_cyan": fx("XCyan", (0.04, 0.8, 1.0), 0.3),
     "x_red": fx("XRot", (0.45, 0.02, 0.02)),
     "x_red_d": fx("XRotDunkel", (0.35, 0.03, 0.05)),
     "x_orange": fx("XOrange", (1.0, 0.33, 0.03), 0.3),
     "x_white": fx("XWeiss", (0.95, 0.99, 1.0))}

objs = []
count = 0
warnings = 0


def P(kind, mat, loc, sc, rot=(0, 0, 0), **kw):
    o = L.shape(kind, loc, sc, rot, M[mat], None, collection=scene.collection, **kw)
    objs.append(o)
    return o


def rod(a, b, r0, r1, mat, seg=8, **kw):
    """Stab von a nach b, Radius r0 -> r1"""
    a, b = Vector(a), Vector(b)
    d = b - a
    rot = d.to_track_quat("Z", "Y").to_euler()
    return P("cyl", mat, (a + b) / 2, (r0, r0, d.length / 2), tuple(math.degrees(x) for x in rot), taper=r1 / r0, segments=seg, **kw)


def poly(mat, pts, z=0.0):
    """flache Flaeche (Punkte x,y) in Welthoehe z, Normale nach oben"""
    n = len(pts)
    area = sum(pts[i][0] * pts[(i + 1) % n][1] - pts[(i + 1) % n][0] * pts[i][1] for i in range(n))
    if area < 0:
        pts = pts[::-1]
    me = bpy.data.meshes.new("poly")
    me.from_pydata([(x, y, z) for x, y in pts], [], [list(range(n))])
    me.update()
    o = bpy.data.objects.new("poly", me)
    scene.collection.objects.link(o)
    me.materials.append(M[mat])
    objs.append(o)
    return o


def clear():
    for o in objs:
        bpy.data.objects.remove(o, do_unlink=True)
    objs.clear()


def check(path, name):
    global count, warnings
    count += 1
    if L.edge_pixels(path):
        warnings += 1
        print("WARNUNG Randpixel:", name)


def use_foot_camera(size):
    scene.camera = cam
    scene.render.resolution_x = scene.render.resolution_y = size
    cam.data.ortho_scale = 2.1 * size / 128
    pitch = math.radians(45)
    cam.location = Vector((0, 0, 0.85 * size / 128)) - Vector((0, math.sin(pitch), -math.cos(pitch))) * 10


def shot_prop(name, parts, size=128, outline=0.016, smooth=True, out_dir=None, edge=True):
    o = L.join(parts, name)
    if smooth:
        L.subdivide(o, 1)
    if outline:
        L.add_outline(o, outline)
    use_foot_camera(size)
    path = os.path.join(out_dir or OUT, name + ".png")
    L.render_still(scene, path)
    bpy.data.objects.remove(o, do_unlink=True)
    objs.clear()
    if edge:
        check(path, name)
    return path


def shot_top(name, w, h, center=(0, 0), outline=None, alpha=None, smooth=False, ppu=128):
    """senkrecht von oben; w x h Pixel bei `ppu` Pixeln je Meter, Bildmitte = center"""
    if len(objs) > 1:
        o = L.join(list(objs), name)
    else:
        o = objs[0]
    if outline:
        L.add_outline(o, outline)
    scene.camera = tcam
    scene.render.resolution_x, scene.render.resolution_y = w, h
    tcam.data.ortho_scale = max(w, h) / ppu
    tcam.location = (center[0], center[1], 10)
    tcam.rotation_euler = (0, 0, 0)
    path = os.path.join(OUT, name + ".png")
    L.render_still(scene, path)
    bpy.data.objects.remove(o, do_unlink=True)
    objs.clear()
    if alpha:
        post_alpha(path, alpha)
    check(path, name)


def post_alpha(path, fn):
    """Alpha nachtraeglich veraendern: fn(u, v, rgba) -> Faktor (u nach rechts, v nach oben, 0..1)"""
    img = bpy.data.images.load(path, check_existing=False)
    try:
        img.alpha_mode = "STRAIGHT"
        w, h = img.size
        a = np.empty(w * h * 4, dtype=np.float32)
        img.pixels.foreach_get(a)
        a = a.reshape(h, w, 4)
        yy, xx = np.mgrid[0:h, 0:w]
        u, v = (xx + 0.5) / w, (yy + 0.5) / h
        a[..., 3] = np.clip(a[..., 3] * fn(u, v, a), 0, 1)
        img.pixels.foreach_set(a.ravel())
        img.filepath_raw = path
        img.file_format = "PNG"
        img.save()
    finally:
        bpy.data.images.remove(img)


# ======================================================================== Kacheln (64 px)
def tile_setup():
    for o in list(scene.objects):
        if o.type == "MESH":
            bpy.data.objects.remove(o, do_unlink=True)
    scene.render.resolution_x = scene.render.resolution_y = 64
    scene.render.film_transparent = False
    cd.ortho_scale = 1.0
    tcam.location = (0.5, 0.5, 10)
    tcam.rotation_euler = (0, 0, 0)
    scene.camera = tcam


def wrapped(kind, mat, x, y, z, sc, rot=(0, 0, 0), margin=0.2, **kw):
    for dx in (-1, 0, 1):
        for dy in (-1, 0, 1):
            if -margin <= x + dx <= 1 + margin and -margin <= y + dy <= 1 + margin:
                P(kind, mat, (x + dx, y + dy, z), sc, rot, **kw)


def tile(name, seed, base, scatter):
    random.seed(seed)
    P("cube", base, (0.5, 0.5, -0.05), (1.5, 1.5, 0.04), smooth=False)
    zc = 0.0
    for mats, n, size, flat in scatter:
        for _ in range(n):
            zc += 0.0004
            wrapped("sphere", random.choice(mats), random.random(), random.random(), zc,
                    (random.uniform(*size), random.uniform(*size) * flat, 0.012), rot=(0, 0, random.uniform(0, 180)), segments=8)
    path = os.path.join(OUT, name + ".png")
    scene.render.filepath = path
    L.render_still(scene, path)
    clear()
    global count
    count += 1


tile_setup()
tile("Frost_Snow_0", 21, "snow", [(["snow_sh"], 5, (0.12, 0.2), 0.55), (["snow2"], 8, (0.06, 0.12), 0.6),
                                  (["white"], 7, (0.05, 0.1), 0.5), (["ice_l"], 6, (0.02, 0.035), 1.0), (["spark"], 7, (0.012, 0.02), 1.0)])
tile("Frost_Snow_1", 22, "snow2", [(["snow_sh"], 6, (0.1, 0.18), 0.5), (["snow"], 9, (0.07, 0.14), 0.6),
                                   (["white"], 6, (0.04, 0.09), 0.5), (["ice_l"], 8, (0.02, 0.035), 1.0), (["spark"], 5, (0.012, 0.02), 1.0)])
# Weg: festgetretener Schnee / blaues Eis, Laeufer-Streifen und Risse
random.seed(23)
P("cube", "ice", (0.5, 0.5, -0.05), (1.5, 1.5, 0.04), smooth=False)
zc = 0.0
for _ in range(9):
    zc += 0.0004
    wrapped("sphere", "ice_l", random.random(), random.random(), zc, (random.uniform(0.1, 0.2), random.uniform(0.03, 0.06), 0.012),
            rot=(0, 0, random.uniform(-20, 20)), segments=8)
for _ in range(8):
    zc += 0.0004
    wrapped("sphere", "ice_d", random.random(), random.random(), zc, (random.uniform(0.08, 0.18), random.uniform(0.012, 0.025), 0.012),
            rot=(0, 0, random.uniform(-70, 70)), segments=8)
for _ in range(6):
    zc += 0.0004
    wrapped("sphere", "snow", random.random(), random.random(), zc, (random.uniform(0.03, 0.07), random.uniform(0.02, 0.05), 0.012), segments=8)
for _ in range(5):
    zc += 0.0004
    wrapped("sphere", "spark", random.random(), random.random(), zc, (0.015, 0.015, 0.012), segments=6)
L.render_still(scene, os.path.join(OUT, "Frost_Path.png"))
clear()
count += 1

# Wand: Eisbloecke / Eisklippe, dunkle Fugen, helle Kanten, Kristallspitzen
random.seed(24)
P("cube", "deep", (0.5, 0.5, -0.05), (1.5, 1.5, 0.04), smooth=False)
N = 4
for i in range(N):
    for j in range(N):
        cx, cy = (i + 0.5) / N + random.uniform(-0.012, 0.012), (j + 0.5) / N + random.uniform(-0.012, 0.012)
        hz = random.uniform(0.05, 0.16)
        mat = random.choice(["ice_d", "ice_d", "deep2", "ice"])
        hs = 0.105 + random.uniform(-0.01, 0.012)
        P("cube", mat, (cx, cy, hz * 0.5), (hs, hs, hz * 0.5 + 0.02), smooth=True)
        P("cube", "ice_l", (cx - hs * 0.45, cy + hs * 0.55, hz + 0.022), (hs * 0.45, 0.012, 0.012), smooth=False)
        P("cube", "ice_l", (cx - hs * 0.8, cy + hs * 0.2, hz + 0.022), (0.012, hs * 0.5, 0.012), smooth=False)
        if random.random() < 0.55:
            P("cone", random.choice(["ice_l", "white", "glow_d"]), (cx + random.uniform(-0.04, 0.04), cy + random.uniform(-0.04, 0.04), hz + 0.07),
              (0.035, 0.035, 0.08), rot=(random.uniform(-10, 10), random.uniform(-10, 10), 0), segments=5, smooth=False)
L.render_still(scene, os.path.join(OUT, "Frost_Wall.png"))
clear()
count += 1

scene.render.film_transparent = True

# ======================================================================== Deko
# ---- gefrorene Tanne (256 px): kahler Stamm, Aeste mit Schnee, Eiszapfen
random.seed(31)
tree = [rod((0, 0, 0), (0, 0, 2.75), 0.2, 0.06, "wood_d", 10),
        P("cone", "snow", (0, 0, 0.08), (0.34, 0.3, 0.16), segments=14),
        P("cone", "wood_d", (0.18, 0.1, 0.07), (0.07, 0.06, 0.12), rot=(0, 70, 30)),
        P("cone", "wood_d", (-0.16, 0.12, 0.07), (0.07, 0.06, 0.12), rot=(0, -70, -30)),
        P("cone", "white", (0, 0, 2.86), (0.1, 0.1, 0.2), segments=8),
        P("sphere", "snow", (0.05, -0.1, 1.05), (0.12, 0.08, 0.07))]
for lvl, (z, rl, n) in enumerate(((0.85, 0.95, 5), (1.3, 0.82, 5), (1.75, 0.66, 4), (2.2, 0.46, 4))):
    for k in range(n):
        a = 2 * math.pi * (k + 0.5 * (lvl % 2)) / n + lvl * 0.5
        end = (rl * math.cos(a), rl * math.sin(a) * 0.85, z - 0.28)
        tree.append(rod((0, 0, z), end, 0.055, 0.02, "wood", 6))
        mid = (end[0] * 0.55, end[1] * 0.55, z - 0.1)
        tree.append(P("sphere", "snow", (mid[0], mid[1], mid[2] + 0.06), (0.2, 0.085, 0.05), rot=(0, 0, math.degrees(a)), segments=8))
        tree.append(P("sphere", "white", (end[0], end[1], end[2] + 0.04), (0.14, 0.07, 0.05), rot=(0, 0, math.degrees(a)), segments=8))
        tree.append(P("cone", "ice", (end[0], end[1], end[2] - 0.11), (0.025, 0.025, 0.11), rot=(180, 0, 0), segments=5, smooth=False))
        tree.append(P("cone", "ice_l", (end[0] * 0.8, end[1] * 0.8, end[2] + 0.05 - 0.14), (0.02, 0.02, 0.08), rot=(180, 0, 0), segments=5, smooth=False))
shot_prop("frozen_tree", tree, 256, 0.024)

# ---- Eisfels
rock = [P("sphere", "ice", (0, 0, 0.2), (0.42, 0.34, 0.26), segments=10),
        P("cube", "ice_l", (0.2, 0.06, 0.2), (0.19, 0.15, 0.22), rot=(0, 0, 25)),
        P("cube", "ice_d", (-0.22, 0.05, 0.16), (0.17, 0.15, 0.16), rot=(0, 0, -30)),
        P("cone", "ice_l", (0.05, -0.04, 0.5), (0.1, 0.09, 0.2), rot=(8, -8, 20), segments=6, smooth=False),
        P("sphere", "snow", (-0.02, 0.02, 0.4), (0.3, 0.22, 0.07)),
        P("sphere", "white", (0.24, -0.02, 0.37), (0.14, 0.1, 0.05)),
        P("sphere", "snow", (0, 0, 0.04), (0.5, 0.4, 0.06)),
        P("sphere", "glow_d", (-0.1, -0.3, 0.2), (0.05, 0.02, 0.07))]
shot_prop("ice_rock", rock)

# ---- Schneehaufen
pile = [P("sphere", "snow", (0, 0, 0.12), (0.52, 0.4, 0.22), segments=14),
        P("sphere", "white", (-0.12, -0.06, 0.2), (0.3, 0.22, 0.17)),
        P("sphere", "snow2", (0.28, 0.1, 0.08), (0.22, 0.16, 0.1)),
        P("sphere", "snow_sh", (0.0, -0.3, 0.03), (0.4, 0.1, 0.04)),
        P("sphere", "ice_l", (0.3, -0.2, 0.05), (0.045, 0.04, 0.04)),
        P("sphere", "spark", (-0.2, -0.17, 0.3), (0.025, 0.02, 0.02)),
        P("cone", "ice", (-0.34, 0.05, 0.1), (0.05, 0.05, 0.1), rot=(0, -25, 0), segments=5, smooth=False)]
shot_prop("snow_pile", pile)


# ---- Eiskristall (leuchtend, Gruppe)
def crystal(x, y, h, r, tilt_x, tilt_y, mat="glow"):
    return [P("cyl", mat, (x, y, h * 0.4), (r, r, h * 0.4), rot=(tilt_x, tilt_y, 0), taper=0.8, segments=6, smooth=False),
            P("cone", "ice_l" if mat == "glow" else mat, (x + math.sin(math.radians(tilt_y)) * h * 0.75, y - math.sin(math.radians(tilt_x)) * h * 0.75, h * 0.8 + h * 0.15),
              (r * 0.82, r * 0.82, h * 0.2), rot=(tilt_x, tilt_y, 0), segments=6, smooth=False)]


cl = [P("sphere", "snow", (0, 0, 0.03), (0.42, 0.34, 0.06))]
cl += crystal(0, 0, 1.05, 0.15, -4, 3)
cl += crystal(-0.24, 0.05, 0.68, 0.11, 6, -18, "glow_d")
cl += crystal(0.24, -0.02, 0.78, 0.12, -8, 16)
cl += crystal(0.08, 0.17, 0.52, 0.085, 20, 8, "glow_d")
cl += crystal(-0.1, -0.17, 0.4, 0.08, -22, -10)
cl.append(P("sphere", "spark", (-0.03, -0.12, 0.62), (0.02, 0.012, 0.045)))
shot_prop("ice_crystal", cl, outline=0.014, smooth=False)

# ---- Eissaeule: hohes dickes Prisma (0,7 breit, 1,4 hoch)
pil = [P("cone", "snow", (0, 0, 0.05), (0.5, 0.42, 0.1), segments=10),
       P("cyl", "ice", (0, 0, 0.7), (0.35, 0.35, 0.65), taper=0.86, segments=8, smooth=False),
       P("cyl", "ice_l", (0.0, 0.0, 1.2), (0.3, 0.3, 0.12), taper=0.7, segments=8, smooth=False),
       P("cone", "ice_l", (0.04, 0.02, 1.48), (0.22, 0.22, 0.14), rot=(0, 0, 10), segments=8, smooth=False),
       P("cone", "ice", (-0.2, 0.05, 1.25), (0.09, 0.09, 0.2), rot=(0, -18, 0), segments=6, smooth=False),
       P("cube", "white", (-0.17, -0.3, 0.8), (0.03, 0.02, 0.5), rot=(0, 0, 14), smooth=False),
       P("cube", "ice_l", (0.1, -0.31, 0.55), (0.025, 0.02, 0.3), rot=(0, 0, -8), smooth=False),
       P("cube", "glow_d", (0.0, -0.34, 0.95), (0.05, 0.012, 0.08), rot=(0, 0, 0), smooth=False),
       P("sphere", "snow", (0.12, -0.28, 0.1), (0.2, 0.1, 0.08))]
shot_prop("ice_pillar", pil, outline=0.014, smooth=False)


# ======================================================================== Portale (256 px wie Base/portal_0.png)
def arch(ice_look):
    """Steinbogen wie in build_base.py; Siegel: grau und vereist, offen: eisblau"""
    st, sd = ("stone", "stone_d") if not ice_look else ("ice_l", "ice")
    parts = [P("cyl", sd, (0, 0, 0.06), (0.95, 0.5, 0.06), segments=24),
             P("cyl", st, (0, 0, 0.14), (0.85, 0.42, 0.04), segments=24)]
    for x in (-0.62, 0.62):
        parts += [P("cube", st, (x, 0, 0.75), (0.13, 0.15, 0.6)),
                  P("cube", sd, (x, 0, 0.2), (0.17, 0.19, 0.08)),
                  P("cube", "glow" if ice_look else "seal", (x, -0.152, 0.75), (0.04, 0.01, 0.12), smooth=False)]
    parts.append(P("torus", st, (0, 0, 1.35), (0.62, 0.62, 0.62), rot=(90, 0, 0), segments=16))
    parts.append(P("cube", "glow" if ice_look else "seal", (0, -0.17, 1.98), (0.07, 0.015, 0.07), rot=(0, 45, 0), smooth=False))
    # Schnee auf dem Bogen und den Saeulen, Eiszapfen
    parts += [P("sphere", "snow", (0, 0.0, 1.98), (0.3, 0.17, 0.09)), P("sphere", "white", (-0.45, 0.0, 1.8), (0.2, 0.15, 0.07)),
              P("sphere", "snow", (-0.62, 0, 1.38), (0.2, 0.2, 0.06)), P("sphere", "snow", (0.62, 0, 1.38), (0.2, 0.2, 0.06)),
              P("sphere", "snow", (0.5, 0.0, 1.85), (0.17, 0.14, 0.06)), P("sphere", "snow2", (0.74, 0.05, 0.3), (0.2, 0.2, 0.08)),
              P("sphere", "snow2", (-0.76, 0.0, 0.28), (0.2, 0.2, 0.08))]
    for k, (x, z, h) in enumerate(((-0.42, 1.62, 0.2), (-0.2, 1.7, 0.14), (0.15, 1.72, 0.17), (0.38, 1.63, 0.22), (0.0, 1.74, 0.1))):
        parts.append(P("cone", "ice" if k % 2 else "ice_l", (x, -0.02, z - h * 0.5), (0.03, 0.03, h * 0.5), rot=(180, 0, 0), segments=5, smooth=False))
    return parts


def portal_shot(name, parts, extra=0.0):
    o = L.join(parts, name)
    L.subdivide(o, 1)
    L.add_outline(o, 0.022)
    o.scale = (1, 1, 1)
    use_foot_camera(256)
    path = os.path.join(OUT, name + ".png")
    L.render_still(scene, path)
    bpy.data.objects.remove(o, do_unlink=True)
    objs.clear()
    check(path, name)


# versiegelt: grauer Stein, Eisplatte, Ketten und mattes Runensiegel
sealed = arch(False)
sealed += [P("cyl", "ice", (0, 0.02, 1.0), (0.5, 0.5, 0.05), rot=(90, 0, 0), segments=24),
           P("cyl", "ice_l", (0, -0.04, 1.0), (0.44, 0.44, 0.02), rot=(90, 0, 0), segments=24),
           P("torus", "seal", (0, -0.1, 1.05), (0.26, 0.26, 0.26), rot=(90, 0, 0), segments=20),
           P("torus", "seal", (0, -0.1, 1.05), (0.16, 0.16, 0.16), rot=(90, 0, 0), segments=16),
           P("cube", "seal", (0, -0.1, 1.05), (0.05, 0.015, 0.22), smooth=False),
           P("cube", "seal", (0, -0.1, 1.05), (0.22, 0.015, 0.05), smooth=False),
           P("cube", "seal", (0, -0.1, 1.05), (0.07, 0.016, 0.07), rot=(0, 45, 0), smooth=False)]
for sgn in (-1, 1):                                              # Ketten: gekreuzt von Saeule zu Saeule
    pts = [(-0.5 * sgn + (1.0 * sgn) * t, -0.2, 1.55 - 1.0 * t * 1.0 if sgn > 0 else 0.55 + 1.0 * t) for t in [i / 8 for i in range(9)]]
    for i, (x, y, z) in enumerate(pts):
        sag = -0.08 * math.sin(math.pi * i / 8)
        sealed.append(P("torus", "iron", (x, y, z + sag), (0.075, 0.075, 0.075), rot=(90 if i % 2 else 0, 0, 0), segments=8))
sealed.append(P("cube", "iron", (0, -0.24, 1.07), (0.1, 0.03, 0.12)))                  # Schloss in der Mitte
sealed.append(P("sphere", "seal", (0, -0.28, 1.07), (0.035, 0.015, 0.035)))
portal_shot("sealed_portal", sealed)

# offen: eisblauer Bogen mit wirbelndem Portal (4 Bilder)
for frame in range(4):
    a0 = frame * 90 / 4
    parts = arch(True)
    parts.append(P("cyl", "portal", (0, 0.02, 1.05), (0.5, 0.5, 0.02), rot=(90, 0, 0), segments=24))
    for k in range(3):
        parts.append(P("torus", "portal_l" if k % 2 == 0 else "portal", (0, -0.01 - 0.01 * k, 1.05),
                       (0.42 - k * 0.12, 0.42 - k * 0.12, 0.42 - k * 0.12), rot=(90, a0 * (k + 1), 0), segments=12))
    for k in range(6):
        ang = math.radians(a0 * 2 + k * 60)
        r = 0.18 + 0.07 * (k % 2)
        parts.append(P("sphere", "spark" if k % 2 else "portal_l", (r * math.cos(ang), -0.05, 1.05 + r * math.sin(ang)), (0.035, 0.02, 0.035)))
    portal_shot(f"frost_portal_{frame}", parts)

# ======================================================================== Effekte (senkrecht von oben)
# ---- Atemkegel: Spitze links mittig, 4 m lang, 3 m breit
CONE_L, CONE_HW = 3.96, 1.46


def cone_pts(scale, bulge=0.1, wscale=None):
    pts = [(0.02, 0.0)]
    n = 16
    for i in range(n + 1):
        t = -1 + 2 * i / n
        x = 0.02 + (CONE_L - 0.02) * scale - bulge * scale * t * t
        pts.append((x, CONE_HW * (wscale or scale) * t))
    return pts


def breath_shape(seed):
    random.seed(seed)
    poly("x_mist", cone_pts(1.0), 0.00)
    poly("x_mist2", cone_pts(0.97, 0.1, 0.7), 0.01)
    poly("x_core", cone_pts(0.88, 0.1, 0.38), 0.02)
    # Eisscherben (flache Rauten) in Atemrichtung
    for k in range(34):
        x = random.uniform(0.35, 3.7)
        hw = CONE_HW * x / CONE_L * 0.8
        y = random.uniform(-hw, hw)
        ln = random.uniform(0.18, 0.42)
        w = ln * random.uniform(0.18, 0.3)
        mat = random.choice(["x_cyan", "x_white", "x_white", "x_core"])
        poly(mat, [(x, y), (x + ln * 0.35, y + w), (x + ln, y), (x + ln * 0.35, y - w)], 0.03 + k * 0.0004)
    # Nebelschwaden
    for k in range(18):
        x = random.uniform(0.3, 3.6)
        hw = CONE_HW * x / CONE_L * 0.75
        P("sphere", random.choice(["x_core", "x_mist2"]), (x, random.uniform(-hw, hw), 0.05 + k * 0.0005),
          (random.uniform(0.15, 0.3), random.uniform(0.08, 0.16), 0.005), rot=(0, 0, random.uniform(-25, 25)), segments=10)


breath_shape(41)


def cone_alpha(u, v, rgba):
    r = np.abs((v - 0.5) * 3.0) / np.maximum(CONE_HW * u / 0.99, 0.06)
    edge = 0.5 + 0.5 * np.clip((1 - r) / 0.4, 0, 1)
    fade = 0.85 - 0.3 * u
    mist = fade * edge
    shard = np.where((rgba[..., 0] > 0.9) | (rgba[..., 0] < 0.3), 0.92, mist)   # Scherben und Nebelkern kraeftiger
    return np.clip(np.where(rgba[..., 0] > 0.9, np.maximum(mist, 0.7), shard), 0, 1)


shot_top("breath_cone", 512, 384, (2.0, 0.0), alpha=cone_alpha)

# ---- Warnflaeche des Kegels: rot-orange, Rand heller
poly("x_orange", cone_pts(1.0), 0.00)
poly("x_red", cone_pts(0.955, 0.1), 0.01)
poly("x_red_d", cone_pts(0.62, 0.1), 0.02)
poly("x_red", cone_pts(0.585, 0.1), 0.03)
for i in range(5):                                           # Pfeilspitzen in Atemrichtung
    x = 0.55 + i * 0.7
    hw = CONE_HW * x / CONE_L * 0.55
    poly("x_orange", [(x, 0), (x - 0.22, hw), (x - 0.12, hw), (x + 0.08, 0), (x - 0.12, -hw), (x - 0.22, -hw)], 0.04)


def warn_alpha(u, v, rgba):
    border = (rgba[..., 1] > 0.25)               # orange (hell) -> kraeftig, rote Flaeche -> durchscheinend
    return np.where(border, 0.9, 0.38)


shot_top("breath_warn", 512, 384, (2.0, 0.0), alpha=warn_alpha)

# ---- Warnkreis (2 m Durchmesser)
P("cyl", "x_cyan", (0, 0, 0.0), (0.99, 0.99, 0.004), segments=48)
P("cyl", "x_red", (0, 0, 0.01), (0.92, 0.92, 0.004), segments=48)
P("cyl", "x_cyan", (0, 0, 0.02), (0.6, 0.6, 0.004), segments=40)
P("cyl", "x_red_d", (0, 0, 0.03), (0.56, 0.56, 0.004), segments=40)
for k in range(12):                                          # Eisspitzen am Rand
    a = 2 * math.pi * k / 12
    poly("x_cyan", [(0.62 * math.cos(a - 0.1), 0.62 * math.sin(a - 0.1)), (0.9 * math.cos(a), 0.9 * math.sin(a)),
                    (0.62 * math.cos(a + 0.1), 0.62 * math.sin(a + 0.1))], 0.04)
for k in range(6):                                           # Schneeflocke in der Mitte
    a = math.pi * k / 3
    c, s = math.cos(a), math.sin(a)
    poly("x_white", [(-0.5 * c - 0.03 * s, -0.5 * s + 0.03 * c), (0.5 * c - 0.03 * s, 0.5 * s + 0.03 * c),
                     (0.5 * c + 0.03 * s, 0.5 * s - 0.03 * c), (-0.5 * c + 0.03 * s, -0.5 * s - 0.03 * c)], 0.05)


def circle_alpha(u, v, rgba):
    cool = rgba[..., 2] > 0.5                    # cyan/weiss kraeftig, rote Flaeche durchscheinend
    return np.where(cool, 0.88, 0.36)


shot_top("ice_warn_circle", 256, 256, (0, 0), alpha=circle_alpha)

# ---- Eiszapfen: faellt (0), schlaegt ein (1), Scherben (2); 128 px, Fusspunkt-Kamera
use_foot_camera(128)
random.seed(51)
shadow0 = P("cyl", "snow_sh", (0, 0, 0.004), (0.28, 0.28, 0.004), segments=20)
ic0 = [shadow0,
       P("cone", "ice", (0, 0, 0.95), (0.19, 0.19, 0.55), rot=(180, 0, 0), segments=7, smooth=False),
       P("cone", "ice_l", (0, 0, 0.95), (0.11, 0.11, 0.5), rot=(180, 0, 0), segments=7, smooth=False),
       P("cyl", "ice_d", (0, 0, 1.52), (0.19, 0.19, 0.04), segments=7, smooth=False),
       P("sphere", "white", (-0.06, -0.12, 1.05), (0.03, 0.012, 0.22)),
       P("cone", "glow", (0.0, 0.0, 1.7), (0.05, 0.05, 0.1), segments=5, smooth=False)]
shot_prop("icicle_0", ic0, outline=0.014, smooth=False)
ic1 = [P("sphere", "snow", (0, 0, 0.03), (0.5, 0.4, 0.06)),
       P("cone", "ice_l", (0, 0, 0.42), (0.14, 0.14, 0.42), segments=7, smooth=False),
       P("sphere", "white", (-0.04, -0.1, 0.45), (0.022, 0.012, 0.22))]
for k in range(7):
    a = 2 * math.pi * k / 7 + 0.3
    ic1.append(P("cone", "ice", (0.3 * math.cos(a), 0.26 * math.sin(a), 0.1), (0.05, 0.05, 0.12), rot=(-math.sin(a) * 35, math.cos(a) * 35, 0), segments=5, smooth=False))
for k in range(5):
    a = 2 * math.pi * k / 5 + 0.9
    ic1.append(P("sphere", "white", (0.42 * math.cos(a), 0.34 * math.sin(a), 0.12 + 0.06 * (k % 2)), (0.07, 0.06, 0.06)))
shot_prop("icicle_1", ic1, outline=0.014, smooth=False)
ic2 = [P("sphere", "snow", (0, 0, 0.02), (0.55, 0.42, 0.05)), P("sphere", "snow_sh", (0, 0.02, 0.01), (0.62, 0.46, 0.02))]
for k in range(10):
    a = 2 * math.pi * k / 10 + random.uniform(-0.2, 0.2)
    r = random.uniform(0.1, 0.4)
    ic2.append(P("cone", random.choice(["ice", "ice_l", "glow_d"]), (r * math.cos(a), r * 0.8 * math.sin(a), 0.05),
                 (random.uniform(0.04, 0.08), random.uniform(0.03, 0.06), random.uniform(0.06, 0.12)),
                 rot=(random.uniform(60, 90), random.uniform(-40, 40), random.uniform(0, 180)), segments=4, smooth=False))
shot_prop("icicle_2", ic2, outline=0.012, smooth=False)


# ---- Fluegelschlag-Ring (3 Bilder, 4 m Durchmesser) -- Ring waechst und verblasst
def ring_arc(mat, r0, r1, a0, a1, z, steps=10):
    outer = [(r1 * math.cos(a0 + (a1 - a0) * i / steps), r1 * math.sin(a0 + (a1 - a0) * i / steps)) for i in range(steps + 1)]
    inner = [(r0 * math.cos(a0 + (a1 - a0) * i / steps), r0 * math.sin(a0 + (a1 - a0) * i / steps)) for i in range(steps, -1, -1)]
    return poly(mat, outer + inner, z)


for frame, (rad, thick, amul) in enumerate(((0.85, 0.24, 0.95), (1.45, 0.26, 0.75), (1.6, 0.18, 0.45))):
    random.seed(60 + frame)
    ring_arc("x_mist", rad - thick, rad + thick * 0.4, 0, 2 * math.pi, 0.0, 48)
    ring_arc("x_mist2", rad - thick * 0.7, rad + thick * 0.1, 0, 2 * math.pi, 0.01, 48)
    z = 0.02
    for k in range(10):                                      # wehende Windboegen auf dem Ring
        a0 = 2 * math.pi * k / 10 + random.uniform(0, 0.2)
        span = random.uniform(0.3, 0.55)
        rr = rad + random.uniform(-0.45, 0.2) * thick
        ring_arc("x_core", rr - 0.035, rr + 0.035, a0, a0 + span, z, 8)
        z += 0.002
    for k in range(16):                                      # Eissplitter, nach aussen gerichtet
        a = 2 * math.pi * k / 16 + random.uniform(-0.1, 0.1)
        r = rad + thick * 0.35
        ln = random.uniform(0.12, 0.2)
        c, s = math.cos(a), math.sin(a)
        poly(random.choice(["x_cyan", "x_white"]), [(r * c - 0.035 * s, r * s + 0.035 * c), ((r + ln) * c, (r + ln) * s),
                                                    (r * c + 0.035 * s, r * s - 0.035 * c)], 0.03)

    def mk(amul=amul, rad=rad, thick=thick):
        def f(u, v, rgba):
            d = np.hypot((u - 0.5) * 4.0, (v - 0.5) * 4.0)
            soft = np.clip(1.0 - np.abs(d - rad + thick * 0.2) / (thick * 1.5), 0.3, 1.0)
            return amul * soft * (0.75 + 0.25 * rgba[..., 0])
        return f
    shot_top(f"wing_wave_{frame}", 512, 512, (0, 0), alpha=mk())

# ---- Schneesturm (512, halbtransparent, kachelbar: Elemente werden ueber den Rand umgebrochen)
random.seed(77)
W = 4.0


def wrap_blizzard(kind, mat, x, y, z, sc, rot):
    for dx in (-W, 0, W):
        for dy in (-W, 0, W):
            if -0.6 <= x + dx <= W + 0.6 and -0.6 <= y + dy <= W + 0.6:
                P(kind, mat, (x + dx - W / 2, y + dy - W / 2, z), sc, rot, segments=8)


for arm in range(3):                                         # breite, weiche Spiralarme
    for k in range(34):
        t = k / 33
        rr = 0.25 + t * 2.0
        a = 2 * math.pi * arm / 3 + t * 3.4
        x, y = rr * math.cos(a), rr * math.sin(a)
        tang = math.degrees(a) + 90 + 25
        wrap_blizzard("sphere", "x_mist2", x + W / 2, y + W / 2, 0.0005 * (arm * 34 + k),
                      (random.uniform(0.22, 0.38), random.uniform(0.05, 0.09), 0.004), (0, 0, tang))
for k in range(90):                                          # kuerzere Schneeschlieren
    x, y = random.uniform(0, W), random.uniform(0, W)
    ang = math.degrees(math.atan2(y - W / 2, x - W / 2)) + 90 + 25 + random.uniform(-20, 20)
    wrap_blizzard("sphere", random.choice(["x_core", "x_core", "x_mist2"]), x, y, 0.1 + 0.001 * k,
                  (random.uniform(0.12, 0.3), random.uniform(0.02, 0.04), 0.004), (0, 0, ang))
for k in range(260):                                         # Flocken
    x, y = random.uniform(0, W), random.uniform(0, W)
    wrap_blizzard("sphere", random.choice(["x_core", "x_core", "x_cyan"]), x, y, 0.3 + 0.0005 * k,
                  (random.uniform(0.015, 0.04),) * 2 + (0.004,), (0, 0, 0))


def blizzard_alpha(u, v, rgba):
    r = rgba[..., 0]
    return np.where(r > 0.9, 0.7, np.where(r < 0.3, 0.6, 0.24))


shot_top("blizzard", 512, 512, (0, 0), alpha=blizzard_alpha)

# ---- Eissplitter-Geschoss (64 px, nach rechts)
sh = [P("cone", "ice_l", (0.05, 0, 0.05), (0.055, 0.055, 0.17), rot=(0, 90, 0), segments=6, smooth=False),
      P("cone", "ice", (-0.12, 0, 0.05), (0.055, 0.055, 0.07), rot=(0, -90, 0), segments=6, smooth=False),
      P("cone", "glow", (-0.01, 0, 0.1), (0.028, 0.028, 0.11), rot=(0, 90, 0), segments=6, smooth=False),
      P("cone", "ice_l", (-0.08, 0.05, 0.05), (0.02, 0.02, 0.06), rot=(0, 90, 35), segments=4, smooth=False),
      P("cone", "ice_l", (-0.08, -0.05, 0.05), (0.02, 0.02, 0.06), rot=(0, 90, -35), segments=4, smooth=False)]
shot_top("ice_shard", 64, 64, (0, 0), outline=0.012, ppu=128)


# ======================================================================== Item-Icons (48 px)
def icon(name, parts, outline=0.012, smooth=True, span_mul=1.25):
    o = L.join(parts, "ICON_" + name)
    objs.clear()
    if smooth:
        L.subdivide(o, 1)
    L.add_outline(o, outline)
    bpy.context.view_layer.update()
    ev = o.evaluated_get(bpy.context.evaluated_depsgraph_get())
    me = ev.to_mesh()
    pts = [ev.matrix_world @ v.co for v in me.vertices]
    ev.to_mesh_clear()
    lo = Vector((min(p.x for p in pts), min(p.y for p in pts), min(p.z for p in pts)))
    hi = Vector((max(p.x for p in pts), max(p.y for p in pts), max(p.z for p in pts)))
    c, size = (lo + hi) / 2, hi - lo
    scene.camera = icam
    icam.location = (c.x, c.y - 10, c.z + 1.2)
    icam.rotation_euler = (math.radians(83), 0, 0)
    icd.ortho_scale = max(0.2, max(size.x, size.z) * span_mul)
    scene.render.resolution_x = scene.render.resolution_y = 48
    path = os.path.join(ICON_DIR, f"Item_{name}.png")
    L.render_still(scene, path)
    bpy.data.objects.remove(o, do_unlink=True)
    check(path, "Item_" + name)


def rot2(x, z, deg):
    a = math.radians(deg)
    return x * math.cos(a) - z * math.sin(a), x * math.sin(a) + z * math.cos(a)


# Frostschluessel: eisblauer Schluessel, schraeg, mit Kristall im Griff
K = 35        # Neigung in Grad
def kp(kind, mat, x, z, sc, rot=(0, 0, 0), **kw):
    rx, rz = rot2(x, z, K)
    return P(kind, mat, (rx, 0, rz), sc, (rot[0], rot[1] - K if kind != "torus" else rot[1] + 0, rot[2]), **kw)


key = [kp("torus", "ice_l", 0, 0.5, (0.2, 0.2, 0.2), rot=(90, -K, 0), segments=16),
       kp("sphere", "gem", 0, 0.5, (0.075, 0.05, 0.075)),
       kp("cyl", "gold", 0, 0.05, (0.045, 0.045, 0.4), segments=10),
       kp("torus", "ice", 0, 0.28, (0.08, 0.08, 0.08), rot=(90, -K, 0), segments=10),
       kp("cube", "ice_l", 0.09, -0.28, (0.09, 0.035, 0.05), smooth=False),
       kp("cube", "ice_l", 0.07, -0.14, (0.07, 0.035, 0.045), smooth=False),
       kp("cube", "ice_d", 0.09, -0.4, (0.11, 0.035, 0.05), smooth=False)]
for k_, (dx, dz) in enumerate(((-0.2, 0.62), (0.2, 0.62), (0, 0.78))):
    key.append(kp("cone", "white", dx, dz, (0.045, 0.045, 0.09), segments=4, smooth=False))
icon("frostschluessel", key, 0.012)

# Drachenschuppe: grosse blassblaue Schuppe mit Rippen
sc_parts = [P("sphere", "deep2", (0, 0.04, 0.05), (0.38, 0.05, 0.42), segments=18),
            P("cone", "deep2", (0, 0.04, -0.3), (0.34, 0.05, 0.2), rot=(180, 0, 0), segments=16),
            P("sphere", "ice", (0, -0.0, 0.07), (0.31, 0.05, 0.35), segments=18),
            P("cone", "ice", (0, -0.0, -0.23), (0.28, 0.05, 0.17), rot=(180, 0, 0), segments=16),
            P("sphere", "ice_l", (0, -0.05, 0.12), (0.2, 0.04, 0.25), segments=16),
            P("sphere", "white", (-0.1, -0.09, 0.22), (0.05, 0.02, 0.1), rot=(0, 20, 0)),
            P("cube", "ice_d", (0, -0.08, -0.04), (0.012, 0.015, 0.3), smooth=False),
            P("cube", "ice_d", (-0.12, -0.08, -0.02), (0.012, 0.015, 0.24), rot=(0, 20, 0), smooth=False),
            P("cube", "ice_d", (0.12, -0.08, -0.02), (0.012, 0.015, 0.24), rot=(0, -20, 0), smooth=False)]
icon("drachenschuppe", sc_parts, 0.014)

# Eiskristall: leuchtendes Kristallbuendel
ek = []
for (x, h, r, tl, mat) in ((0, 0.9, 0.085, 0, "glow"), (-0.27, 0.58, 0.065, 28, "glow_d"), (0.27, 0.64, 0.07, -26, "glow"), (-0.12, 0.3, 0.05, 42, "glow_d"), (0.14, 0.28, 0.05, -40, "glow_d")):
    d = Vector((math.sin(math.radians(tl)), 0, math.cos(math.radians(tl))))
    b = Vector((x, 0, 0))
    ek += [rod(b, b + d * h * 0.75, r, r * 0.9, mat, 6, smooth=False),
           rod(b + d * h * 0.75, b + d * (h * 0.75 + 0.2), r * 0.9, 0.012, "ice_l", 6, smooth=False)]
ek.append(P("sphere", "spark", (-0.02, -0.1, 0.4), (0.02, 0.012, 0.09)))
ek.append(P("sphere", "snow", (0, 0, 0.0), (0.34, 0.08, 0.025)))
icon("eiskristall", ek, 0.012, smooth=False)

bpy.ops.wm.save_as_mainfile(filepath=BLEND_PATH)
print(f"RESULT rendered={count} randwarnungen={warnings}")
