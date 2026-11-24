"""RE:BORN - Eisdrache (Meilenstein 9a, Comic-Look)

Vierbeiniger Drache mit Fluegeln, blassblauen/weissen Schuppen, leuchtenden Cyan-Kristallzacken und Augen.
Rendert idle 4, walk 6, breath 6 (Kopf vor, Maul auf, Atem), wing 6 (grosser Fluegelschlag), fly 4 (schwebt hoeher, Fluegel
gespreizt, Schleife), roar 4, hurt 2, death 6 in 4 Richtungen als 256x256-Sprites nach Sprites/Dragon
(gleicher Massstab und Fusspunkt wie Sprites/Boss: ortho 4,2 bei 256 px, Kamera-Ziel z=1,7, Fusspunkt-Pivot 0.5, 0.2138).
Ein flacher Schatten liegt immer am Boden (auch beim Schweben). Eigene Datei: Art/Blender/Reborn_Dragon.blend

Aufruf:  blender.exe -b --factory-startup --python build_dragon.py -- <projektordner>
Test:    Umgebungsvariable DRAGON_TEST=<ordner> rendert nur wenige Bilder dorthin (ohne Speichern).
"""
import bmesh
import bpy
import math
import os
import sys
from mathutils import Vector

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import reborn_lib as L  # noqa: E402

argv = sys.argv[sys.argv.index("--") + 1:] if "--" in sys.argv else []
PROJECT = argv[0] if argv else os.path.abspath(os.path.join(os.path.dirname(__file__), "..", "..", ".."))
TEST = os.environ.get("DRAGON_TEST")
OUT = TEST or os.path.join(PROJECT, "Assets", "_Project", "Sprites", "Dragon")
BLEND_PATH = os.path.join(PROJECT, "Art", "Blender", "Reborn_Dragon.blend")
os.makedirs(OUT, exist_ok=True)

bpy.ops.wm.read_factory_settings(use_empty=True)
scene = bpy.context.scene
scene.name = "Dragon"
L.setup_scene(scene, 256)
L.camera(scene, target=(0, 0, 1.7), ortho=4.2)       # wie build_boss.py
sun_obj = L.sun(scene, energy=3.4)
sun_obj.data.use_shadow = False                  # ruhigere Cel-Flaechen ohne Schatten-Rauschen


def fcel(name, light, shadow, emission=0.0, flat=False):
    m = L.cel(name, light, emission=emission)
    ramp = [n for n in m.node_tree.nodes if n.type == "VALTORGB"][0]
    els = ramp.color_ramp.elements
    if flat:
        els[0].color = els[1].color = els[2].color = (*light, 1.0)
    else:
        els[0].color = (*shadow, 1.0)
        els[1].color = (*[shadow[i] + (light[i] - shadow[i]) * 0.5 for i in range(3)], 1.0)
        els[2].color = (*light, 1.0)
    return m


M = {"scale": fcel("DrScale", (0.55, 0.78, 1.0), (0.12, 0.22, 0.58)),
     "scale_d": fcel("DrScaleDunkel", (0.32, 0.55, 0.9), (0.1, 0.19, 0.48)),
     "belly": fcel("DrBauch", (0.9, 0.97, 1.0), (0.45, 0.62, 0.92)),
     "bone": fcel("DrHorn", (0.88, 0.96, 1.0), (0.42, 0.58, 0.88)),
     "crystal": fcel("DrKristall", (0.3, 0.92, 1.0), (0.12, 0.6, 0.95), emission=0.7),
     "crystal_l": fcel("DrKristallHell", (0.7, 1.0, 1.0), (0.35, 0.85, 1.0), emission=0.6),
     "eye": fcel("DrAugen", (0.2, 1.0, 1.0), (0.2, 0.9, 1.0), emission=1.2),
     "mouth": fcel("DrMaul", (0.2, 0.14, 0.34), (0.1, 0.07, 0.22)),
     "tooth": fcel("DrZahn", (1.0, 1.0, 1.0), (0.7, 0.8, 0.95)),
     "membrane": fcel("DrFlughaut", (0.42, 0.72, 1.0), (0.12, 0.28, 0.68), emission=0.1),
     "membrane_l": fcel("DrFlughautHell", (0.75, 0.93, 1.0), (0.35, 0.6, 0.92), emission=0.1),
     "wingbone": fcel("DrFluegelKnochen", (0.45, 0.62, 0.9), (0.16, 0.26, 0.56)),
     "shadow": fcel("DrSchatten", (0.28, 0.36, 0.6), (0.28, 0.36, 0.6), flat=True)}

# ---------------------------------------------------------------- Skelett (Kopf nach -Y, Ruecken oben)
bones = {"root": ((0, 0, 0), (0, 0, 0.3), None),
         "ground": ((0, 0, 0), (0, 0, 0.05), None),
         "body": ((0, 0.45, 1.15), (0, -0.45, 1.22), "root"),
         "tail1": ((0, 0.45, 1.15), (0, 0.95, 1.0), "body"),
         "tail2": ((0, 0.95, 1.0), (0, 1.4, 0.85), "tail1"),
         "tail3": ((0, 1.4, 0.85), (0, 1.8, 0.8), "tail2"),
         "neck1": ((0, -0.45, 1.22), (0, -0.68, 1.6), "body"),
         "neck2": ((0, -0.68, 1.6), (0, -0.85, 1.98), "neck1"),
         "head": ((0, -0.85, 1.98), (0, -1.4, 1.9), "neck2"),
         "jaw": ((0, -0.98, 1.84), (0, -1.4, 1.76), "head")}
for sd, sg in (("L", -1), ("R", 1)):
    bones[f"thigh.F{sd}"] = ((0.42 * sg, -0.4, 1.15), (0.47 * sg, -0.32, 0.62), "body")
    bones[f"shin.F{sd}"] = ((0.47 * sg, -0.32, 0.62), (0.47 * sg, -0.5, 0.08), f"thigh.F{sd}")
    bones[f"thigh.B{sd}"] = ((0.45 * sg, 0.5, 1.15), (0.5 * sg, 0.7, 0.62), "body")
    bones[f"shin.B{sd}"] = ((0.5 * sg, 0.7, 0.62), (0.5 * sg, 0.52, 0.08), f"thigh.B{sd}")
    bones[f"wingA.{sd}"] = ((0.3 * sg, -0.15, 1.45), (1.05 * sg, -0.1, 2.3), "body")
    bones[f"wingB.{sd}"] = ((1.05 * sg, -0.1, 2.3), (1.85 * sg, 0.15, 2.65), f"wingA.{sd}")
rig = L.build_rig(scene, "DragonRig", bones)

SM, SH = [], []                       # glatte Teile (Unterteilung) und scharfe Teile (Zacken, Haut, Krallen)


def D(kind, bone, mat, loc, sc, rot=(0, 0, 0), sharp=False, **kw):
    if sharp:
        kw["smooth"] = False
    o = L.shape(kind, loc, sc, rot, M[mat], bone, collection=scene.collection, **kw)
    (SH if sharp else SM).append(o)
    return o


def seg(a, b, r0, r1, mat, bone, segs=12, sharp=False):
    a, b = Vector(a), Vector(b)
    d = b - a
    rot = d.to_track_quat("Z", "Y").to_euler()
    return D("cyl", bone, mat, (a + b) / 2, (r0, r0, d.length / 2 * 1.04), tuple(math.degrees(x) for x in rot),
             sharp=sharp, taper=r1 / r0, segments=segs)


def spike(a, b, r, mat, bone, segs=5):
    """Kegel (Kristall/Horn/Kralle) mit Fuss bei a und Spitze bei b"""
    a, b = Vector(a), Vector(b)
    d = b - a
    rot = d.to_track_quat("Z", "Y").to_euler()
    return D("cone", bone, mat, (a + b) / 2, (r, r, d.length / 2), tuple(math.degrees(x) for x in rot), sharp=True, segments=segs)


# ---- Rumpf
D("sphere", "body", "scale", (0, 0.05, 1.2), (0.44, 0.85, 0.43), segments=24)
D("sphere", "body", "scale", (0, 0.5, 1.2), (0.43, 0.4, 0.42), segments=20)
D("sphere", "body", "scale", (0, -0.45, 1.25), (0.42, 0.4, 0.42), segments=20)
D("sphere", "body", "belly", (0, -0.1, 1.0), (0.32, 0.75, 0.26), segments=20)
D("sphere", "body", "scale_d", (0, 0.1, 1.55), (0.22, 0.7, 0.1))
for k in range(5):                                                       # Bauchplatten
    D("sphere", "body", "belly", (0, -0.42 + k * 0.22, 0.86), (0.2, 0.04, 0.08), (0, 0, 0))
# ---- Rueckenkristalle (Koerper, Hals, Schwanz)
for k, (y, h) in enumerate(((-0.45, 0.3), (-0.22, 0.34), (0.02, 0.34), (0.26, 0.3), (0.5, 0.26))):
    spike((0, y, 1.58), (0, y + 0.1, 1.58 + h), 0.075, "crystal" if k % 2 == 0 else "crystal_l", "body", 6)
for (a, b, bone) in (((0, -0.58, 1.55), (0, -0.52, 1.88), "neck1"), ((0, -0.78, 1.85), (0, -0.7, 2.18), "neck2")):
    spike(a, b, 0.06, "crystal", bone, 6)
# ---- Hals
seg((0, -0.45, 1.25), (0, -0.68, 1.6), 0.27, 0.22, "scale", "neck1")
D("sphere", "neck1", "scale", (0, -0.68, 1.6), (0.23, 0.23, 0.23))
seg((0, -0.68, 1.6), (0, -0.85, 1.98), 0.22, 0.2, "scale", "neck2")
D("sphere", "neck2", "belly", (0, -0.7, 1.77), (0.16, 0.12, 0.2))
# ---- Kopf
D("sphere", "head", "scale", (0, -1.0, 2.0), (0.27, 0.3, 0.25), segments=18)
D("sphere", "head", "scale", (0, -1.22, 1.95), (0.17, 0.27, 0.13), segments=16)
D("sphere", "head", "scale_d", (0, -1.1, 2.12), (0.2, 0.25, 0.08))
D("sphere", "head", "mouth", (0, -1.2, 1.87), (0.13, 0.23, 0.03))
D("sphere", "jaw", "scale", (0, -1.2, 1.82), (0.15, 0.27, 0.07))
D("sphere", "jaw", "belly", (0, -1.15, 1.77), (0.1, 0.2, 0.04))
D("sphere", "jaw", "mouth", (0, -1.2, 1.87), (0.1, 0.2, 0.025))
for sg in (-1, 1):
    D("sphere", "head", "eye", (0.15 * sg, -1.07, 2.08), (0.055, 0.04, 0.05), (0, 0, 0), sharp=True)
    D("sphere", "head", "scale_d", (0.17 * sg, -1.06, 2.15), (0.09, 0.09, 0.03), (0, 0, 20 * sg), sharp=True)
    D("sphere", "head", "mouth", (0.06 * sg, -1.43, 1.98), (0.025, 0.02, 0.02), sharp=True)          # Nasenloecher
    spike((0.15 * sg, -0.95, 2.2), (0.36 * sg, -0.62, 2.55), 0.075, "bone", "head")               # Hoerner
    spike((0.22 * sg, -0.9, 2.05), (0.42 * sg, -0.78, 2.15), 0.05, "crystal", "head")             # Wangenkristalle
    spike((0.2 * sg, -0.95, 1.95), (0.38 * sg, -0.85, 1.9), 0.045, "crystal_l", "head")
    for k in range(4):                                                                              # Zaehne
        spike((0.08 * sg, -1.1 - k * 0.1, 1.86), (0.08 * sg, -1.1 - k * 0.1, 1.76), 0.022, "tooth", "head", 4)
        spike((0.07 * sg, -1.12 - k * 0.1, 1.79), (0.07 * sg, -1.12 - k * 0.1, 1.88), 0.02, "tooth", "jaw", 4)
spike((0, -1.0, 2.24), (0, -0.85, 2.6), 0.07, "crystal", "head", 6)
# ---- Beine
for sd, sg in (("L", -1), ("R", 1)):
    for tag, (hip, knee, foot, r) in (("F", ((0.42, -0.4, 1.15), (0.47, -0.32, 0.62), (0.47, -0.5, 0.08), 0.2)),
                                      ("B", ((0.45, 0.5, 1.15), (0.5, 0.7, 0.62), (0.5, 0.52, 0.08), 0.23))):
        t, s_ = f"thigh.{tag}{sd}", f"shin.{tag}{sd}"
        hp, kn, ft = [(p[0] * sg, p[1], p[2]) for p in (hip, knee, foot)]
        D("sphere", t, "scale", hp, (r * 1.15, r * 1.2, r * 1.15))
        seg(hp, kn, r, r * 0.8, "scale", t)
        D("sphere", s_, "scale_d", kn, (r * 0.85, r * 0.85, r * 0.85))
        seg(kn, (ft[0], ft[1], ft[2] + 0.1), r * 0.7, r * 0.55, "scale", s_)
        D("sphere", s_, "scale", (ft[0], ft[1] - 0.06, 0.07), (0.17, 0.24, 0.09))
        for c in (-1, 0, 1):
            spike((ft[0] + 0.07 * c, ft[1] - 0.2, 0.07), (ft[0] + 0.08 * c, ft[1] - 0.36, 0.03), 0.035, "bone", s_, 4)
        spike((kn[0] + 0.12 * sg, kn[1], kn[2]), (kn[0] + 0.3 * sg, kn[1] + 0.02, kn[2] + 0.1), 0.05, "crystal", t, 5)
# ---- Schwanz
seg((0, 0.45, 1.15), (0, 0.95, 1.0), 0.36, 0.26, "scale", "tail1")
D("sphere", "tail1", "scale", (0, 0.95, 1.0), (0.26, 0.26, 0.26))
seg((0, 0.95, 1.0), (0, 1.4, 0.85), 0.26, 0.17, "scale", "tail2")
D("sphere", "tail2", "scale", (0, 1.4, 0.85), (0.17, 0.17, 0.17))
seg((0, 1.4, 0.85), (0, 1.8, 0.8), 0.17, 0.05, "scale", "tail3")
for (a, b, bone, r) in (((0, 0.75, 1.2), (0, 0.85, 1.52), "tail1", 0.07), ((0, 1.15, 1.0), (0, 1.23, 1.3), "tail2", 0.06), ((0, 1.55, 0.88), (0, 1.62, 1.12), "tail3", 0.05)):
    spike(a, b, r, "crystal", bone, 6)
spike((0, 1.74, 0.82), (0, 2.0, 0.7), 0.1, "crystal", "tail3", 6)
spike((0.06, 1.72, 0.82), (0.26, 1.88, 0.78), 0.06, "crystal_l", "tail3", 5)
spike((-0.06, 1.72, 0.82), (-0.26, 1.88, 0.78), 0.06, "crystal_l", "tail3", 5)


# ---- Fluegel: Arm- und Fingerknochen plus Flughaut als duenne Platte
def membrane(sg):
    Sd = (0.3, -0.15, 1.45)
    E = (1.05, -0.1, 2.3)
    W = (1.85, 0.15, 2.65)
    F = [(1.9, 0.72, 2.12), (1.5, 1.0, 1.78), (1.0, 0.95, 1.5)]
    Hh = (0.35, 0.55, 1.35)
    pts = [W, F[0], None, F[1], None, F[2], None, Hh, Sd, E]            # Umriss; None = Einbuchtung zwischen den Fingern
    outline = []
    for i, p in enumerate(pts):
        if p is None:
            a, b = pts[i - 1], pts[i + 1]
            m = [(a[j] + b[j]) / 2 for j in range(3)]
            outline.append(tuple(m[j] + (E[j] - m[j]) * 0.32 for j in range(3)))
        else:
            outline.append(p)
    n = len(outline)
    iE = outline.index(E)
    wts = []
    for i, p in enumerate(outline):
        if p == Sd:
            wts.append({f"wingA.{sd_}": 1.0})
        elif p == E:
            wts.append({f"wingA.{sd_}": 0.5, f"wingB.{sd_}": 0.5})
        elif p == Hh:  # Hinterkante haengt am Fluegelarm
            wts.append({f"wingA.{sd_}": 1.0})
        elif i == n - 3 or i == 6:
            wts.append({f"wingB.{sd_}": 0.5, f"wingA.{sd_}": 0.5})
        else:
            wts.append({f"wingB.{sd_}": 1.0})
    return outline, iE, wts


for sd_, sg in (("L", -1), ("R", 1)):
    outline, iE, wts = membrane(sg)
    n = len(outline)
    bm = bmesh.new()
    th = 0.022
    top = [bm.verts.new((p[0] * sg, p[1], p[2] + th)) for p in outline]
    bot = [bm.verts.new((p[0] * sg, p[1], p[2] - th)) for p in outline]
    # Faecher von E aus (E ist Teil des Umrisses)
    order = [(iE + 1 + k) % n for k in range(n - 1)]                   # alle Umrisspunkte ausser E, im Umlaufsinn
    for a, b in zip(order[:-1], order[1:]):
        bm.faces.new((top[iE], top[a], top[b]))
        bm.faces.new((bot[iE], bot[b], bot[a]))
    for i in range(n):                                                  # Rand
        j = (i + 1) % n
        bm.faces.new((top[i], bot[i], bot[j], top[j]))
    bmesh.ops.recalc_face_normals(bm, faces=bm.faces[:])
    me = bpy.data.meshes.new(f"Haut{sd_}")
    bm.to_mesh(me)
    bm.free()
    ob = bpy.data.objects.new(f"Haut{sd_}", me)
    scene.collection.objects.link(ob)
    me.materials.append(M["membrane"])
    for p in me.polygons:
        p.use_smooth = False
    groups = {}
    for i, w in enumerate(wts):
        for bn, wv in w.items():
            vg = groups.get(bn) or ob.vertex_groups.new(name=bn)
            groups[bn] = vg
            vg.add([i, i + n], wv, "REPLACE")
    SH.append(ob)
    # Knochen: Arm (Schulter-Ellbogen-Handgelenk), Finger, Krallen
    A, E_, W_ = (0.3 * sg, -0.15, 1.45), (1.05 * sg, -0.1, 2.3), (1.85 * sg, 0.15, 2.65)
    seg(A, E_, 0.1, 0.075, "wingbone", f"wingA.{sd_}", 8, sharp=True)
    D("sphere", f"wingA.{sd_}", "wingbone", E_, (0.1, 0.1, 0.1), sharp=True)
    seg(E_, W_, 0.075, 0.045, "wingbone", f"wingB.{sd_}", 8, sharp=True)
    for p in ((1.9, 0.72, 2.12), (1.5, 1.0, 1.78), (1.0, 0.95, 1.5)):
        seg(W_, (p[0] * sg, p[1], p[2]), 0.04, 0.02, "wingbone", f"wingB.{sd_}", 6, sharp=True)
    spike(W_, (1.98 * sg, 0.1, 2.78), 0.06, "bone", f"wingB.{sd_}")
    spike((0.7 * sg, -0.13, 1.95), (0.78 * sg, -0.13, 2.28), 0.06, "crystal", f"wingA.{sd_}", 5)
    spike((1.4 * sg, 0.0, 2.5), (1.44 * sg, 0.0, 2.78), 0.05, "crystal_l", f"wingB.{sd_}", 5)

dragon_smooth = L.join(SM, "DrachenGlatt")
L.attach_to_rig(dragon_smooth, rig)
L.subdivide(dragon_smooth, 1)
L.add_outline(dragon_smooth, 0.03)
dragon_sharp = L.join(SH, "DrachenScharf")
L.attach_to_rig(dragon_sharp, rig)
L.add_outline(dragon_sharp, 0.024)
sh = L.shape("cyl", (0, 0.1, 0.004), (0.7, 1.05, 0.004), (0, 0, 0), M["shadow"], "ground", collection=scene.collection, segments=28)
L.attach_to_rig(sh, rig)


# ---------------------------------------------------------------- Posen: Fluegel ueber gemessene Achsen
class DPoser(L.Poser):
    """Poser mit 'fl' (Fluegel-Spitze hoch) und 'sw' (Spitze nach vorne) fuer seitlich zeigende Fluegelknochen"""
    WING = [f"wing{p}.{s}" for p in "AB" for s in "LR"]

    def __init__(self, rig_):
        super().__init__(rig_)
        self.map = {}
        for b in self.WING:
            self.sign[b] = 1.0
            self.map[b] = self._calibrate(b)
        self.reset()

    def _tip(self, pb, ch, deg):
        self.reset()
        e = [0.0, 0.0, 0.0]
        e[ch] = math.radians(deg)
        pb.rotation_euler = tuple(e)
        bpy.context.view_layer.update()
        return self.rig.matrix_world @ pb.tail

    def _calibrate(self, name):
        pb = self.rig.pose.bones[name]
        base = self._tip(pb, 0, 0)
        d = [self._tip(pb, ch, 15) - base for ch in range(3)]
        up = max(range(3), key=lambda c: abs(d[c].z))
        fwd = max((c for c in range(3) if c != up), key=lambda c: abs(d[c].y))
        return {"fl": (up, 1 if d[up].z > 0 else -1), "sw": (fwd, 1 if d[fwd].y < 0 else -1)}

    def apply(self, pose):
        p = {}
        for bone, v in pose.items():
            v = dict(v)
            if bone in self.map:
                for k in ("fl", "sw"):
                    if k in v:
                        ch, sgn = self.map[bone][k]
                        key = ("rx", "ry", "rz")[ch]
                        v[key] = v.get(key, 0) + v.pop(k) * sgn
            p[bone] = v
        super().apply(p)


rig.scale = (0.88, 0.88, 0.88)                   # etwas kleiner, damit auch Schwebe- und Fluegelposen ins Bild passen
poser = DPoser(rig)
print("WINGMAP", poser.map["wingA.R"], poser.map["wingB.R"], poser.map["wingA.L"])


def wings(a, b, sa=0, sb=0, s=1.0, la=None, lb=None, lsa=None, lsb=None):
    """a/b: Fluegel-Spitze hoch (Grad) fuer Arm/Hand; sa/sb: nach vorne. Optional andere Werte fuer die linke Seite"""
    return {"wingA.R": {"fl": a, "sw": sa, "s": s}, "wingB.R": {"fl": b, "sw": sb},
            "wingA.L": {"fl": a if la is None else la, "sw": sa if lsa is None else lsa, "s": s},
            "wingB.L": {"fl": b if lb is None else lb, "sw": sb if lsb is None else lsb}}


def legs(fl, fr, bl, br, kf=0, kb=0):
    return {"thigh.FL": {"rx": fl}, "thigh.FR": {"rx": fr}, "thigh.BL": {"rx": bl}, "thigh.BR": {"rx": br},
            "shin.FL": {"rx": kf}, "shin.FR": {"rx": kf}, "shin.BL": {"rx": kb}, "shin.BR": {"rx": kb}}


def tail(a, b=0, c=0, lift=0):
    return {"tail1": {"rz": a, "rx": -lift}, "tail2": {"rz": b}, "tail3": {"rz": c}}


def neck(n1, n2, hd, jw=0):
    return {"neck1": {"rx": n1}, "neck2": {"rx": n2}, "head": {"rx": hd}, "jaw": {"rx": -jw}}


FOLD = wings(-25, -30, -55, -95, 0.58)    # Fluegel angelegt (verkleinert, nach hinten geklappt)
SPREAD = wings(0, 0)
anims = {
    "idle": ([{**FOLD, **neck(0, 0, 0), **tail(6, 10, 14), "body": {"z": 0.0}},
              {**FOLD, **neck(4, 2, -2), **tail(0, 0, 0), "body": {"z": 0.03, "sz": 1.01}},
              {**FOLD, **neck(6, 4, -4), **tail(-6, -10, -14), "body": {"z": 0.0}},
              {**FOLD, **neck(3, 1, -1), **tail(0, 0, 0), "body": {"z": -0.02}}], 4, True),
    "walk": ([{**FOLD, **legs(22, -18, -18, 22), **neck(2, 2, 0), **tail(10, 14, 18), "body": {"z": 0.0, "rz": 3}},
              {**FOLD, **legs(4, 4, 4, 4, 0, 0), **neck(4, 3, 0), **tail(0, 0, 0), "body": {"z": 0.06}},
              {**FOLD, **legs(-18, 22, 22, -18), **neck(2, 2, 0), **tail(-10, -14, -18), "body": {"z": 0.0, "rz": -3}},
              {**FOLD, **legs(4, 4, 4, 4), **neck(4, 3, 0), **tail(0, 0, 0), "body": {"z": 0.06}}], 6, True),
    "breath": ([{**FOLD, **neck(0, 0, 0), **tail(0, 0, 0)},
                {**wings(-25, -20, -40, -70), **neck(-28, -22, 10, 10), "body": {"rx": 8, "z": 0.04}, **tail(0, 0, 0)},
                {**wings(-22, -25, -40, -80, 0.75), **neck(35, 30, -12, 40), "body": {"rx": -6, "z": -0.02}, **tail(0, 4, 8, 6)},
                {**wings(-22, -25, -40, -80, 0.75), **neck(40, 34, -14, 55), "body": {"rx": -8, "z": -0.04}, **tail(0, 6, 10, 8)},
                {**wings(-22, -25, -40, -80, 0.75), **neck(38, 30, -12, 45), "body": {"rx": -6, "z": -0.03}, **tail(0, 4, 8, 6)},
                {**FOLD, **neck(10, 8, -4, 10), "body": {"rx": 0}, **tail(0, 0, 0)}], 6, False),
    "wing": ([{**FOLD, **neck(0, 0, 0)},
              {**wings(35, 20, 10, 0), **neck(-6, -4, 0), "body": {"rx": 6, "z": 0.05}, "thigh.FL": {"rx": 10}, "thigh.FR": {"rx": 10}},
              {**wings(60, 35, 5, 0), **neck(-12, -8, 0, 6), "body": {"rx": 10, "z": 0.1}, "thigh.FL": {"rx": 14}, "thigh.FR": {"rx": 14}},
              {**wings(-25, -10, 5, 0), **neck(6, 4, 0), "body": {"rx": -4, "z": 0.02}, **legs(6, 6, 0, 0)},
              {**wings(-45, -25, 0, 0), **neck(10, 8, 0), "body": {"rx": -8, "z": -0.04}},
              {**FOLD, **neck(2, 1, 0)}], 6, False),
    "fly": ([{**wings(12, 4, 0, 0), **legs(35, 35, 30, 30, -50, -50), **neck(8, 4, 0), **tail(0, 0, 0, 8), "root": {"z": 0.42}, "body": {"rx": -4}},
             {**wings(-2, -15, 0, 0), **legs(32, 32, 28, 28, -45, -45), **neck(10, 6, 0), **tail(5, 5, 5, 4), "root": {"z": 0.65}, "body": {"rx": -6}},
             {**wings(-30, -35, 0, 0), **legs(35, 35, 30, 30, -50, -50), **neck(8, 4, 0), **tail(0, 0, 0, 6), "root": {"z": 0.55}, "body": {"rx": -4}},
             {**wings(0, 6, 0, 0), **legs(32, 32, 28, 28, -45, -45), **neck(6, 2, 0), **tail(-5, -5, -5, 8), "root": {"z": 0.45}, "body": {"rx": -2}}], 4, True),
    "roar": ([{**FOLD, **neck(0, 0, 0)},
              {**wings(-5, -5, -20, -40), **neck(-30, -30, 15, 50), "body": {"rx": 10, "z": 0.08}, **legs(8, 8, 0, 0)},
              {**wings(25, 10, 10, 0), **neck(-36, -34, 18, 60), "body": {"rx": 12, "z": 0.1}, **legs(10, 10, 0, 0), **tail(0, 0, 0, 6)},
              {**wings(-10, -10, -20, -40), **neck(-18, -16, 10, 25), "body": {"rx": 5, "z": 0.04}}], 4, False),
    "hurt": ([{**wings(4, 0, 10, -20, 0.85), **neck(-24, -16, 20, 22), "body": {"rx": 10, "z": 0.03}, **tail(0, 0, 0, 8), **legs(10, 10, -6, -6)},
              {**wings(-15, -10, -30, -50), **neck(-8, -6, 8, 6), "body": {"rx": 4}}], 2, False),
    "death": ([{**wings(0, 0, -10, -30), **neck(-20, -18, 14, 20), "body": {"rx": 8, "z": 0.0}},
               {**wings(5, -10, 0, 0), **neck(-10, -6, 10, 30), "body": {"rx": 10, "z": -0.1}, **legs(14, 14, 10, 10, -20, -20), **tail(0, 0, 0, 12)},
               {**wings(-10, -25, 0, 0), **neck(10, 8, 12, 25), "body": {"rx": 6, "z": -0.25}, **legs(30, 30, 24, 24, -45, -45), **tail(4, 6, 6, 18)},
               {**wings(-30, -40, 5, 0), **neck(35, 28, 10, 30), "body": {"rx": 4, "z": -0.4}, **legs(40, 40, 34, 34, -60, -60), **tail(8, 10, 12, 22)},
               {**wings(-45, -45, 10, 0), **neck(50, 40, 8, 25), "body": {"z": -0.5}, **legs(50, 50, 40, 40, -70, -70), **tail(18, 24, 28, 25)},
               {**wings(-55, -50, 10, 0), **neck(65, 55, 10, 20), "body": {"z": -0.55}, **legs(56, 56, 44, 44, -76, -76), **tail(24, 32, 36, 25)}], 6, False)}

total = warnings = 0
for anim, (keys, count, loop) in anims.items():
    act, end = L.make_action(rig, poser, "Dragon_" + anim, keys, loop)
    for direction, angle in L.DIRECTIONS.items():
        if TEST and direction not in ("down", "left"):
            continue
        rig.rotation_euler = (0, 0, math.radians(angle))
        for i, f in enumerate(L.sample_frames(end, count, loop)):
            if TEST and i not in (0, count // 2, count - 1):
                continue
            L.set_frame(scene, f)
            path = os.path.join(OUT, f"{anim}_{direction}_{i}.png")
            L.render_still(scene, path)
            total += 1
            e = L.edge_pixels(path)
            if e:
                warnings += 1
                print("WARNUNG Randpixel:", anim, direction, i, e)
rig.rotation_euler = (0, 0, 0)
scene["foot_pivot"] = L.foot_pivot(scene)
if not TEST:
    bpy.ops.wm.save_as_mainfile(filepath=BLEND_PATH)
print(f"RESULT rendered={total} randwarnungen={warnings} pivot={scene['foot_pivot'][0]},{scene['foot_pivot'][1]}")
