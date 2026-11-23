"""RE:BORN - Boss Stufe 2 "Uralter Waldwaechter" (Meilenstein 8b, Comic-Look)

Oeffnet Art/Blender/Reborn_Boss.blend (Rig + Aktionen "Boss_<anim>"), faerbt die Materialien um
(fast schwarze Rinde, gluehende Risse/Augen/Kern, duesteres Moos), fuegt Hoerner, Kristalle und Risse
am Rig hinzu und rendert alle Animationen wie build_boss.py nach Sprites/Boss2. Die Datei wird NICHT gespeichert.

Aufruf:  blender.exe -b Art/Blender/Reborn_Boss.blend --python build_boss2.py -- <projektordner>
"""
import bpy
import math
import os
import sys
from mathutils import Vector

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import reborn_lib as L  # noqa: E402

argv = sys.argv[sys.argv.index("--") + 1:] if "--" in sys.argv else []
PROJECT = argv[0] if argv else os.path.abspath(os.path.join(os.path.dirname(__file__), "..", "..", ".."))
OUT = os.path.join(PROJECT, "Assets", "_Project", "Sprites", "Boss2")
bpy.ops.wm.open_mainfile(filepath=os.path.join(PROJECT, "Art", "Blender", "Reborn_Boss.blend"))
scene = bpy.data.scenes["Boss"]
if bpy.context.window:
    bpy.context.window.scene = scene
rig = bpy.data.objects["BossRig"]

M = {"bark": L.cel("Rinde", (0.2, 0.15, 0.18)),
     "bark_dark": L.cel("RindeDunkel", (0.1, 0.075, 0.1)),
     "bark_light": L.cel("RindeHell", (0.36, 0.26, 0.28)),
     "moss": L.cel("Moos", (0.26, 0.36, 0.26)),
     "moss_light": L.cel("MoosHell", (0.44, 0.3, 0.52)),
     "leaf": L.cel("Laub", (0.2, 0.3, 0.24)),
     "eye": L.cel("WaechterAugen", (1.0, 0.5, 0.1), shadow=1.0, mid=1.0, emission=0.3),
     "core": L.cel("WaechterKern", (1.0, 0.28, 0.06), shadow=0.95, mid=1.0, emission=0.3),
     "stone": L.cel("Stein", (0.3, 0.27, 0.34)),
     "crack": L.cel("Riss", (1.0, 0.4, 0.08), shadow=1.0, mid=1.0, emission=0.3),
     "crystal": L.cel("Kristall", (1.0, 0.3, 0.12), shadow=0.8, mid=0.95, emission=0.2),
     "horn": L.cel("Horn", (0.14, 0.1, 0.12), shadow=0.6)}
B = lambda k, b, m, loc, sc, rot=(0, 0, 0), **kw: L.shape(k, loc, sc, rot, M[m], b, collection=scene.collection, **kw)


def chain(bone, mat, pts, r0, r1, seg=8):
    """gebogenes, spitz zulaufendes Horn: Kette aus Kegelstumpf-Stuecken entlang der Punkte"""
    out = []
    n = len(pts) - 1
    for i in range(n):
        a, b = Vector(pts[i]), Vector(pts[i + 1])
        v = b - a
        ra, rb = r0 + (r1 - r0) * i / n, r0 + (r1 - r0) * (i + 1) / n
        e = v.to_track_quat("Z", "Y").to_euler()
        out.append(B("cyl", bone, mat, (a + b) / 2, (ra, ra, v.length / 2 * 1.1), rot=tuple(math.degrees(x) for x in e),
                     taper=rb / ra, segments=seg))
    return out


extra = []
for sg in (-1, 1):
    s = "L" if sg < 0 else "R"
    # grosses Horn, nach aussen und oben gebogen
    big = [(0.2 * sg, 0.0, 2.95), (0.34 * sg, 0.0, 3.05), (0.5 * sg, 0.0, 3.17), (0.62 * sg, 0.0, 3.35), (0.66 * sg, 0.0, 3.56)]
    extra += chain("head", "horn", big, 0.15, 0.02)
    extra.append(B("sphere", "head", "core", (0.66 * sg, 0.0, 3.54), (0.03, 0.03, 0.045)))
    # kleiner Ast schraeg nach aussen
    small = [(0.28 * sg, 0.08, 2.78), (0.42 * sg, 0.1, 2.84), (0.55 * sg, 0.1, 2.95), (0.6 * sg, 0.09, 3.1)]
    extra += chain("head", "horn", small, 0.09, 0.015, 6)
    # Kristalle auf den Schultern (Haupt + zwei kleine)
    for dx, dz, h, r, tilt in ((0.0, 2.4, 0.34, 0.09, 0), (0.14, 2.36, 0.2, 0.06, 25), (-0.12, 2.36, 0.22, 0.06, -25)):
        extra.append(B("cone", f"upperarm.{s}", "crystal", (0.66 * sg + dx, -0.03, dz + h * 0.4),
                       (r, r, h), rot=(0, tilt * sg, 0), segments=6, smooth=False))
zusatz = L.join(extra, "ZusatzTeile")
L.attach_to_rig(zusatz, rig)
L.subdivide(zusatz, 1)
L.add_outline(zusatz, 0.022)

# gluehende Risse auf Brust, Bauch, Armen und Beinen (duenne Streifen, ohne Glaetten)
cr = []
for i, (b, x, z, ang, h) in enumerate((("spine", -0.3, 1.75, 8, 0.2), ("spine", 0.28, 1.65, -10, 0.17), ("spine", -0.12, 2.1, 5, 0.14),
                                       ("spine", 0.3, 2.05, -6, 0.12), ("hips", 0.05, 1.3, 0, 0.16), ("hips", -0.22, 1.2, 10, 0.1))):
    cr.append(B("cube", b, "crack", (x, -0.445, z), (0.018, 0.012, h), rot=(0, 0, ang), smooth=False))
    cr.append(B("cube", b, "crack", (x + 0.05 * (1 if i % 2 else -1), -0.445, z - h * 0.8), (0.012, 0.01, h * 0.5), rot=(0, 0, -ang * 2 + 25), smooth=False))
for s, sg in (("L", -1), ("R", 1)):
    cr += [B("cube", f"upperarm.{s}", "crack", (0.7 * sg, -0.165, 1.85), (0.014, 0.01, 0.15), rot=(0, 0, 8), smooth=False),
           B("cube", f"forearm.{s}", "crack", (0.81 * sg, -0.155, 1.1), (0.014, 0.01, 0.14), rot=(0, 0, -6), smooth=False),
           B("cube", f"thigh.{s}", "crack", (0.31 * sg, -0.19, 0.78), (0.014, 0.01, 0.12), rot=(0, 0, 4), smooth=False)]
risse = L.join(cr, "Risse")
L.attach_to_rig(risse, rig)
L.add_outline(risse, 0.006)

# ---------------------------------------------------------------- Rendern wie build_boss.py (Aktionen aus der Datei)
os.makedirs(OUT, exist_ok=True)
anims = {"idle": (4, True), "walk": (6, True), "attack": (6, False), "rootattack": (6, False), "roar": (4, False),
         "hurt": (2, False), "death": (6, False)}
total = warnings = 0
for anim, (count, loop) in anims.items():
    act = bpy.data.actions["Boss_" + anim]
    L.use_action(rig, act)
    end = act.frame_range[1]
    for direction, angle in L.DIRECTIONS.items():
        rig.rotation_euler = (0, 0, math.radians(angle))
        for i, f in enumerate(L.sample_frames(end, count, loop)):
            L.set_frame(scene, f)
            path = os.path.join(OUT, f"{anim}_{direction}_{i}.png")
            L.render_still(scene, path)
            total += 1
            e = L.edge_pixels(path)
            if e:
                warnings += 1
                print("WARNUNG Randpixel:", anim, direction, i, e)
print(f"RESULT rendered={total} randwarnungen={warnings}")
