# -*- coding: utf-8 -*-
"""
Генератор UML-диаграмм классов в классической нотации (как в draw.io).

    cd Prod && python3 Docs/tools/generate_uml.py

Откуда берутся данные:
    * классы, поля, свойства, методы — из исходников C# (Docs/tools/csharp_api.py);
    * пакеты, стереотипы, описания и связи — из Docs/tools/uml_model.py.
Раскладку считает graphviz (`dot`, нужен в PATH): наследники под родителями, ортогональные линии.

Результат:
    Prod_di.drawio                       — все страницы для draw.io (редактируемые)
    Docs/uml/ClassDiagram.drawio.svg     — классы игры (рантайм); открывается и в draw.io, и в браузере
    Docs/uml/EditorDiagram.drawio.svg    — классы редактора (генератор демо)
    Docs/uml/Packages.drawio.svg         — пакеты и зависимости между ними
    Docs/UML.md                          — страница с диаграммами и таблицей классов
"""
import json
import os
import re
import shutil
import subprocess
import sys
from collections import OrderedDict, defaultdict
from html import escape

HERE = os.path.dirname(os.path.abspath(__file__))
sys.path.insert(0, HERE)
import csharp_api  # noqa: E402
import uml_model as model  # noqa: E402

PROD = os.path.abspath(os.path.join(HERE, "..", ".."))
UML_DIR = os.path.join(PROD, "Docs", "uml")
DRAWIO_PATH = os.path.join(PROD, "Prod_di.drawio")
MARKDOWN_PATH = os.path.join(PROD, "Docs", "UML.md")

FONT_SIZE = 11
NAME_SIZE = 12
LINE_HEIGHT = 15
PAD_X = 8
MIN_WIDTH = 120
MAX_LINES = 16          # длиннее — сворачиваем в "… ещё N" (полный список в Docs/API.md)
EDITOR_PACKAGE = "EditorTools"

STEREOTYPE_LABEL = {
    "interface": "«interface»", "enum": "«enumeration»", "struct": "«struct»", "static": "«static»",
    "MonoBehaviour": "«MonoBehaviour»", "ScriptableObject": "«ScriptableObject»", "Editor": "«Editor»",
    "abstract": None, "class": None,
}
EDGE_SVG = {  # dasharray, marker-start, marker-end
    "inherit": ("", None, "triangle"),
    "implement": ("6,4", None, "triangle"),
    "compose": ("", "diamond", None),
    "assoc": ("", None, "open"),
    "dep": ("6,4", None, "open"),
}
EDGE_DRAWIO = {
    "inherit": "endArrow=block;endFill=0;endSize=12;",
    "implement": "endArrow=block;endFill=0;endSize=12;dashed=1;",
    "compose": "startArrow=diamondThin;startFill=1;startSize=14;endArrow=none;",
    "assoc": "endArrow=open;endSize=10;",
    "dep": "endArrow=open;endSize=10;dashed=1;",
}


# ============================== Измерение текста ==============================

class TextMeter:
    """Ширина текста по шрифту, совместимому с Helvetica/Arial (как в draw.io)."""

    def __init__(self):
        self._fonts = {}
        try:
            from PIL import ImageFont
            self._image_font = ImageFont
        except ImportError:
            self._image_font = None

    def width(self, text, size, bold=False):
        font = self._font(size, bold)
        if font is None:
            return len(text) * size * (0.62 if bold else 0.56)
        return font.getlength(text)

    def _font(self, size, bold):
        key = (size, bold)
        if key not in self._fonts:
            self._fonts[key] = None
            if self._image_font:
                names = (["LiberationSans-Bold.ttf", "arialbd.ttf", "Arial Bold.ttf", "DejaVuSans-Bold.ttf"] if bold
                         else ["LiberationSans-Regular.ttf", "arial.ttf", "Arial.ttf", "DejaVuSans.ttf"])
                for name in names:
                    for folder in ("/usr/share/fonts/truetype/liberation", "/usr/share/fonts/truetype/dejavu",
                                   "C:/Windows/Fonts", "/Library/Fonts", "/System/Library/Fonts/Supplemental", ""):
                        try:
                            self._fonts[key] = self._image_font.truetype(os.path.join(folder, name), size)
                            break
                        except OSError:
                            continue
                    if self._fonts[key] is not None:
                        break
        return self._fonts[key]


METER = TextMeter()


# ============================== Классы → блоки ==============================

VISIBILITY = {"public": "+", "private": "-", "protected": "#", "internal": "~"}


def simplify_type(t):
    t = t.replace("UnityEngine.", "").replace("System.", "")
    return re.sub(r"\s+", " ", t).strip()


def format_params(params, with_types=True):
    result = []
    for p in csharp_api._split_top_level(params):
        p = p.split("=")[0].strip()
        tokens = csharp_api._tokens(p)
        if not tokens:
            continue
        mods = [t for t in tokens[:-1] if t in ("in", "out", "ref", "params", "this")]
        rest = [t for t in tokens[:-1] if t not in ("in", "out", "ref", "params", "this")]
        name = tokens[-1]
        prefix = (" ".join(mods) + " ") if mods else ""
        result.append(f"{prefix}{name}: {simplify_type(' '.join(rest))}" if with_types and rest else f"{prefix}{name}")
    return ", ".join(result)


class Line:
    def __init__(self, text, static=False, abstract=False):
        self.text, self.static, self.abstract = text, static, abstract


def member_lines(t):
    attributes, operations = [], []
    for m in csharp_api.visible_members(t):
        vis = VISIBILITY[m.visibility]
        if m.kind == "enum-value":
            attributes += [Line(v.strip()) for v in m.name.split(",") if v.strip()]
        elif m.kind in ("field", "property", "const"):
            suffix = " {readOnly}" if m.kind == "const" else ""
            attributes.append(Line(f"{vis} {m.name}: {simplify_type(m.type)}{suffix}", static=m.is_static))
        elif m.kind == "event":
            attributes.append(Line(f"{vis} «event» {m.name}: {simplify_type(m.type)}", static=m.is_static))
        elif m.kind in ("method", "constructor"):
            ret = "" if m.kind == "constructor" or m.type == "void" else f": {simplify_type(m.type)}"
            text = f"{vis} {m.name}({format_params(m.params)}){ret}"
            if len(text) > 64:
                text = f"{vis} {m.name}({format_params(m.params, with_types=False)}){ret}"
            operations.append(Line(text, static=m.is_static, abstract=m.is_abstract and t.kind != "interface"))
    return clamp(attributes), clamp(operations)


def clamp(lines):
    if len(lines) <= MAX_LINES:
        return lines
    hidden = len(lines) - (MAX_LINES - 1)
    return lines[:MAX_LINES - 1] + [Line(f"… ещё {hidden} (см. Docs/API.md)")]


class Box:
    def __init__(self, name, stereotype, type_info, ghost=False):
        self.name = name
        self.stereotype = stereotype
        self.type_info = type_info
        self.ghost = ghost
        self.label = STEREOTYPE_LABEL.get(stereotype)
        self.italic = stereotype in ("abstract", "interface")
        if ghost:
            self.attributes, self.operations = [], []
        else:
            self.attributes, self.operations = member_lines(type_info)
        self.header = 40 if self.label else 26
        self.attr_h = max(1, len(self.attributes)) * LINE_HEIGHT + 8 if not ghost else 0
        self.ops_h = max(1, len(self.operations)) * LINE_HEIGHT + 8 if not ghost else 0
        self.height = self.header + (0 if ghost else 8 + self.attr_h + 8 + self.ops_h)
        if ghost:
            self.height = self.header
        widths = [METER.width(name, NAME_SIZE, True) + 2 * PAD_X]
        if self.label:
            widths.append(METER.width(self.label, FONT_SIZE) + 2 * PAD_X)
        widths += [METER.width(l.text, FONT_SIZE) + 2 * PAD_X for l in self.attributes + self.operations]
        self.width = max(MIN_WIDTH, int(max(widths)) + 6)
        self.x = self.y = 0.0


# ============================== Раскладка graphviz ==============================

FRAME_PAD = 24          # отступ блоков от рамки пакета
FRAME_TITLE = 30        # место под название пакета
PACKAGE_GAP = 70        # расстояние между пакетами


def run_graphviz(engine, source):
    """Запуск graphviz. Ортогональная трассировка в graphviz иногда падает на конкретной геометрии
    (assert в maze.c) — тогда пробуем чуть другие отступы, в крайнем случае — ломаные линии."""
    attempts = [source]
    for k in (1.1, 0.9, 1.25, 0.8, 1.4):
        attempts.append(re.sub(r"(nodesep|ranksep)=([\d.]+)", lambda m: f"{m.group(1)}={float(m.group(2)) * k:.2f}", source))
    attempts.append(source.replace("splines=ortho", "splines=polyline"))
    error = ""
    for attempt in attempts:
        result = subprocess.run([engine, "-Tjson"], input=attempt, capture_output=True, text=True)
        if result.returncode == 0:
            return json.loads(result.stdout)
        error = result.stderr
    raise SystemExit(f"graphviz ({engine}) ошибка:\n{error}")


def edge_attrs(kind, constrain_deps=False):
    if kind == "dep" and constrain_deps:
        return "weight=1"
    if kind in ("inherit", "implement"):
        return "weight=8"
    if kind == "compose":
        return "weight=2"
    if kind == "assoc":
        return "weight=1"
    return "weight=0, constraint=false"


def parse_route(edge, height, dx=0.0, dy=0.0):
    pos = edge.get("pos", "")
    numbers = [tuple(map(float, p.split(","))) for p in pos.split() if not p.startswith(("s,", "e,"))]
    points = [numbers[0]] + numbers[3::3] if numbers else []
    return simplify_polyline([(x + dx, height - y + dy) for x, y in points])


def dot_layout(boxes, relations, rankdir="BT", nodesep=0.5, ranksep=0.7, constrain_deps=False):
    """Раскладка набора блоков: позиции (локальные), маршруты связей, размер."""
    lines = ["digraph G {", f"  graph [rankdir={rankdir}, splines=ortho, nodesep={nodesep}, ranksep={ranksep}, margin=0];",
             '  node [shape=box, fixedsize=true, label=""];', "  edge [arrowhead=none, arrowtail=none];"]
    for box in boxes:
        lines.append(f'  "{box.name}" [width={box.width / 72:.3f}, height={box.height / 72:.3f}];')
    for i, r in relations:
        lines.append(f'  "{r["source"]}" -> "{r["target"]}" [{edge_attrs(r["kind"], constrain_deps)}, id="e{i}"];')
    lines.append("}")
    graph = run_graphviz("dot", "\n".join(lines))
    _, _, width, height = map(float, graph["bb"].split(","))
    positions = {}
    for obj in graph.get("objects", []):
        if "pos" in obj:
            cx, cy = map(float, obj["pos"].split(","))
            positions[obj["name"]] = (cx, height - cy)
    routes = {int(e["id"][1:]): parse_route(e, height) for e in graph.get("edges", [])}
    return positions, routes, width, height


def layout(boxes, relations, groups):
    """
    Двухуровневая раскладка: сначала каждый пакет отдельно (наследники под родителями),
    затем пакеты относительно друг друга по зависимостям, затем межпакетные связи
    прокладываются ортогонально в обход всех блоков (neato -n2).
    Возвращает (ширина, высота, рамки пакетов, маршруты). Координаты в пикселях, y вниз.
    """
    indexed = list(enumerate(relations))
    by_group = OrderedDict()
    for box in boxes.values():
        by_group.setdefault(groups.get(box.name, ""), []).append(box)

    # 1. Каждый пакет.
    blocks, routes = {}, {}
    for group, members in by_group.items():
        names = {b.name for b in members}
        inner = [(i, r) for i, r in indexed if r["source"] in names and r["target"] in names]
        # В небольших пакетах зависимости тоже задают уровни — иначе всё вытягивается в одну строку.
        positions, inner_routes, w, h = dot_layout(members, inner, constrain_deps=len(members) <= 20)
        top = FRAME_TITLE if group else 0
        pad = FRAME_PAD if group else 0
        blocks[group] = dict(members=members, positions=positions, routes=inner_routes,
                             width=w + 2 * pad, height=h + top + pad + (pad if not group else 0), pad=pad, top=top)

    # 2. Пакеты целиком: зависимость A → B ставит B выше A.
    group_of = {b.name: g for g, block in blocks.items() for b in block["members"]}
    weights = defaultdict(int)
    for _, r in indexed:
        a, b = group_of[r["source"]], group_of[r["target"]]
        if a != b:
            weights[(a, b)] += 1
    lines = ["digraph P {", f"  graph [rankdir=BT, nodesep={PACKAGE_GAP / 72:.2f}, ranksep={PACKAGE_GAP / 72:.2f}, margin=0];",
             '  node [shape=box, fixedsize=true, label=""];']
    for index, (group, block) in enumerate(blocks.items()):
        lines.append(f'  "g{index}" [width={block["width"] / 72:.3f}, height={block["height"] / 72:.3f}];')
    keys = list(blocks)
    for (a, b), count in weights.items():
        # Двусторонние зависимости не должны ломать ранги — оставляем более "тяжёлое" направление.
        if weights.get((b, a), 0) > count or (weights.get((b, a), 0) == count and keys.index(a) < keys.index(b)):
            continue
        lines.append(f'  "g{keys.index(a)}" -> "g{keys.index(b)}" [weight={count}];')
    lines.append("}")
    graph = run_graphviz("dot", "\n".join(lines))
    _, _, total_w, total_h = map(float, graph["bb"].split(","))
    frames = []
    for obj in graph.get("objects", []):
        if obj.get("name", "").startswith("g") and "pos" in obj:
            group = keys[int(obj["name"][1:])]
            block = blocks[group]
            cx, cy = map(float, obj["pos"].split(","))
            bx, by = cx - block["width"] / 2, total_h - cy - block["height"] / 2
            ox, oy = bx + block["pad"], by + block["top"]
            for box in block["members"]:
                px, py = block["positions"][box.name]
                box.x, box.y = ox + px - box.width / 2, oy + py - box.height / 2
            for i, points in block["routes"].items():
                routes[i] = [(x + ox, y + oy) for x, y in points]
            if group:
                frames.append((group, bx, by, block["width"], block["height"]))

    # 3. Межпакетные связи — ортогонально в обход блоков, позиции блоков фиксированы.
    cross = [(i, r) for i, r in indexed if i not in routes]
    if cross:
        routes.update(route_fixed(boxes, cross, total_h))

    # Ортогональная трассировка иногда останавливается в паре пикселей от блока — доводим концы до границы.
    for i, r in indexed:
        if i in routes and len(routes[i]) >= 2:
            routes[i] = snap_route(routes[i], boxes[r["source"]], boxes[r["target"]])
    return total_w, total_h, frames, routes


def snap_endpoint(points, box, at_end):
    """Доводит крайний отрезок маршрута до границы блока (продлевает, укорачивает или добавляет доворот)."""
    if not at_end:
        return list(reversed(snap_endpoint(list(reversed(points)), box, True)))
    end, prev = points[-1], points[-2]
    x0, y0, x1, y1 = box.x, box.y, box.x + box.width, box.y + box.height
    vertical = abs(end[0] - prev[0]) < 0.5
    if vertical and x0 <= end[0] <= x1:
        return points[:-1] + [(end[0], y0 if abs(prev[1] - y0) < abs(prev[1] - y1) else y1)]
    if not vertical and y0 <= end[1] <= y1:
        return points[:-1] + [(x0 if abs(prev[0] - x0) < abs(prev[0] - x1) else x1, end[1])]
    # Конец возле угла: доходим до края блока по своей оси и поворачиваем к границе.
    dx = max(x0 - end[0], 0, end[0] - x1)
    dy = max(y0 - end[1], 0, end[1] - y1)
    if max(dx, dy) > 60:
        return points
    if vertical:
        y = min(max(end[1], y0 + 8), y1 - 8)
        return points[:-1] + [(end[0], y), (x0 if end[0] < x0 else x1, y)]
    x = min(max(end[0], x0 + 8), x1 - 8)
    return points[:-1] + [(x, end[1]), (x, y0 if end[1] < y0 else y1)]


def snap_route(points, source, target):
    return snap_endpoint(snap_endpoint(points, source, at_end=False), target, at_end=True)


def route_fixed(boxes, edges, total_h):
    """Ортогональная трассировка связей между блоками с фиксированными позициями (neato -n2).
    Если graphviz падает на какой-то геометрии — делим связи пополам; одиночную связь
    в крайнем случае рисуем L-образной линией."""
    def source_for(batch, grow):
        lines = ["digraph X {", "  graph [splines=ortho, margin=0];",
                 '  node [shape=box, fixedsize=true, label=""];', "  edge [arrowhead=none, arrowtail=none];"]
        for box in boxes.values():
            cx, cy = box.x + box.width / 2, total_h - (box.y + box.height / 2)
            lines.append(f'  "{box.name}" [pos="{cx:.1f},{cy:.1f}!", width={(box.width + grow) / 72:.4f}, '
                         f'height={(box.height + grow) / 72:.4f}];')
        for i, r in batch:
            lines.append(f'  "{r["source"]}" -> "{r["target"]}" [id="e{i}"];')
        lines.append("}")
        return "\n".join(lines)

    for grow in (0, 1, 3, 5):
        result = subprocess.run(["neato", "-n2", "-Tjson"], input=source_for(edges, grow), capture_output=True, text=True)
        if result.returncode == 0:
            graph = json.loads(result.stdout)
            return {int(e["id"][1:]): parse_route(e, total_h) for e in graph.get("edges", [])}

    if len(edges) > 1:
        half = len(edges) // 2
        routes = route_fixed(boxes, edges[:half], total_h)
        routes.update(route_fixed(boxes, edges[half:], total_h))
        return routes

    (i, r), = edges
    a, b = boxes[r["source"]], boxes[r["target"]]
    start = (a.x + a.width / 2, a.y if b.y < a.y else a.y + a.height)
    end = (b.x if b.x > start[0] else b.x + b.width, b.y + b.height / 2)
    return {i: [start, (start[0], end[1]), end]}


def snap_endpoint(points, box, at_end):
    """Доводит крайний отрезок маршрута до границы блока (продлевает, укорачивает или добавляет доворот)."""
    if not at_end:
        return list(reversed(snap_endpoint(list(reversed(points)), box, True)))
    end, prev = points[-1], points[-2]
    x0, y0, x1, y1 = box.x, box.y, box.x + box.width, box.y + box.height
    vertical = abs(end[0] - prev[0]) < 0.5
    if vertical and x0 <= end[0] <= x1:
        return points[:-1] + [(end[0], y0 if abs(prev[1] - y0) < abs(prev[1] - y1) else y1)]
    if not vertical and y0 <= end[1] <= y1:
        return points[:-1] + [(x0 if abs(prev[0] - x0) < abs(prev[0] - x1) else x1, end[1])]
    # Конец возле угла: доходим до края блока по своей оси и поворачиваем к границе.
    dx = max(x0 - end[0], 0, end[0] - x1)
    dy = max(y0 - end[1], 0, end[1] - y1)
    if max(dx, dy) > 60:
        return points
    if vertical:
        y = min(max(end[1], y0 + 8), y1 - 8)
        return points[:-1] + [(end[0], y), (x0 if end[0] < x0 else x1, y)]
    x = min(max(end[0], x0 + 8), x1 - 8)
    return points[:-1] + [(x, end[1]), (x, y0 if end[1] < y0 else y1)]


def snap_route(points, source, target):
    return snap_endpoint(snap_endpoint(points, source, at_end=False), target, at_end=True)


def run_graphviz_neato(source):
    result = subprocess.run(["neato", "-n2", "-Tjson"], input=source, capture_output=True, text=True)
    if result.returncode != 0:
        raise SystemExit(f"graphviz (neato) ошибка:\n{result.stderr}")
    return json.loads(result.stdout)


def simplify_polyline(points):
    result = []
    for p in points:
        if result and abs(result[-1][0] - p[0]) < 0.5 and abs(result[-1][1] - p[1]) < 0.5:
            continue
        if len(result) >= 2:
            a, b = result[-2], result[-1]
            if (abs(a[0] - b[0]) < 0.5 and abs(b[0] - p[0]) < 0.5) or (abs(a[1] - b[1]) < 0.5 and abs(b[1] - p[1]) < 0.5):
                result[-1] = p
                continue
        result.append(p)
    return result


# ============================== SVG ==============================

def svg_text(x, y, text, size=FONT_SIZE, bold=False, italic=False, underline=False, anchor="start", color="#000000"):
    style = []
    if bold:
        style.append('font-weight="bold"')
    if italic:
        style.append('font-style="italic"')
    if underline:
        style.append('text-decoration="underline"')
    return (f'<text x="{x:.1f}" y="{y:.1f}" font-size="{size}" text-anchor="{anchor}" fill="{color}" '
            f'{" ".join(style)}>{escape(text)}</text>')


def render_svg(boxes, relations, routes, frames, width, height, margin=20):
    out = [f'<g transform="translate({margin},{margin})">']
    for label, x, y, w, h in frames:
        out.append(f'<rect x="{x:.1f}" y="{y:.1f}" width="{w:.1f}" height="{h:.1f}" fill="none" stroke="#9a9a9a" '
                   f'stroke-dasharray="8,5"/>')
        out.append(svg_text(x + 8, y + 18, label, size=14, bold=True, color="#555555"))
    for i, r in enumerate(relations):
        points = routes.get(i)
        if not points or len(points) < 2:
            continue
        dash, start, end = EDGE_SVG[r["kind"]]
        attrs = f'stroke-dasharray="{dash}" ' if dash else ""
        if start:
            attrs += f'marker-start="url(#{start})" '
        if end:
            attrs += f'marker-end="url(#{end})" '
        path = " ".join(f"{x:.1f},{y:.1f}" for x, y in points)
        out.append(f'<polyline points="{path}" fill="none" stroke="#000000" stroke-width="1" {attrs}/>')
    for box in boxes.values():
        out.append(render_box_svg(box))
    out.append("</g>")

    defs = ('<defs>'
            '<marker id="triangle" viewBox="0 0 12 12" refX="12" refY="6" markerWidth="12" markerHeight="12" '
            'markerUnits="userSpaceOnUse" orient="auto"><path d="M0,0 L12,6 L0,12 z" fill="#ffffff" stroke="#000000"/></marker>'
            '<marker id="open" viewBox="0 0 10 10" refX="10" refY="5" markerWidth="10" markerHeight="10" '
            'markerUnits="userSpaceOnUse" orient="auto"><path d="M0,0 L10,5 L0,10" fill="none" stroke="#000000"/></marker>'
            '<marker id="diamond" viewBox="0 0 14 8" refX="0" refY="4" markerWidth="14" markerHeight="8" '
            'markerUnits="userSpaceOnUse" orient="auto"><path d="M0,4 L7,0 L14,4 L7,8 z" fill="#000000"/></marker>'
            '</defs>')
    total_w, total_h = width + 2 * margin, height + 2 * margin
    body = "".join(out)
    return total_w, total_h, (f'<rect x="0" y="0" width="{total_w:.0f}" height="{total_h:.0f}" fill="#ffffff"/>'
                              f'{defs}<g font-family="Helvetica, Arial, Liberation Sans, sans-serif">{body}</g>')


def render_box_svg(box):
    x, y, w = box.x, box.y, box.width
    dash = ' stroke-dasharray="4,3"' if box.ghost else ""
    out = [f'<rect x="{x:.1f}" y="{y:.1f}" width="{w}" height="{box.height}" fill="#ffffff" stroke="#000000"{dash}/>']
    cy = y + 17
    if box.label:
        out.append(svg_text(x + w / 2, cy, box.label, anchor="middle"))
        cy += 15
    out.append(svg_text(x + w / 2, cy + 1, box.name, size=NAME_SIZE, bold=True, italic=box.italic, anchor="middle"))
    if box.ghost:
        return "".join(out)
    line_y = y + box.header + 4
    out.append(f'<line x1="{x:.1f}" y1="{line_y:.1f}" x2="{x + w:.1f}" y2="{line_y:.1f}" stroke="#000000"/>')
    ty = line_y + 4 + 12
    for line in box.attributes:
        out.append(svg_text(x + PAD_X, ty, line.text, underline=line.static))
        ty += LINE_HEIGHT
    line_y = y + box.header + 8 + box.attr_h + 4
    out.append(f'<line x1="{x:.1f}" y1="{line_y:.1f}" x2="{x + w:.1f}" y2="{line_y:.1f}" stroke="#000000"/>')
    ty = line_y + 4 + 12
    for line in box.operations:
        out.append(svg_text(x + PAD_X, ty, line.text, underline=line.static, italic=line.abstract))
        ty += LINE_HEIGHT
    return "".join(out)


# ============================== draw.io ==============================

def quote(value):
    return (value.replace("&", "&amp;").replace('"', "&quot;").replace("<", "&lt;").replace(">", "&gt;")
            .replace("\n", "&#10;"))


def html_lines(lines):
    parts = []
    for line in lines:
        text = escape(line.text)
        if line.static:
            text = f"<u>{text}</u>"
        if line.abstract:
            text = f"<i>{text}</i>"
        parts.append(text)
    return "<br>".join(parts)


def mx_page(page_id, name, boxes, relations, routes, frames, margin=20):
    cells = ['<mxCell id="0"/>', '<mxCell id="1" parent="0"/>']
    ids = {}
    for i, (label, x, y, w, h) in enumerate(frames):
        style = ("rounded=0;whiteSpace=wrap;html=1;fillColor=none;dashed=1;dashPattern=8 5;strokeColor=#9A9A9A;"
                 "align=left;verticalAlign=top;spacingLeft=8;spacingTop=2;fontStyle=1;fontSize=14;fontColor=#555555;")
        cells.append(f'<mxCell id="pkg{i}" value="{quote(escape(label))}" style="{style}" vertex="1" parent="1">'
                     f'<mxGeometry x="{x + margin:.0f}" y="{y + margin:.0f}" width="{w:.0f}" height="{h:.0f}" as="geometry"/></mxCell>')

    for n, box in enumerate(boxes.values()):
        cid = f"c{n}"
        ids[box.name] = cid
        x, y = box.x + margin, box.y + margin
        title = escape(box.name)
        if box.italic:
            title = f"<i>{title}</i>"
        header = f"{escape(box.label)}<br><b>{title}</b>" if box.label else f"<b>{title}</b>"
        if box.ghost:
            style = "rounded=0;whiteSpace=wrap;html=1;dashed=1;fontSize=12;fillColor=#FFFFFF;strokeColor=#000000;fontColor=#000000;"
            cells.append(f'<mxCell id="{cid}" value="{quote(header)}" style="{style}" vertex="1" parent="1">'
                         f'<mxGeometry x="{x:.0f}" y="{y:.0f}" width="{box.width}" height="{box.height}" as="geometry"/></mxCell>')
            continue
        style = (f"swimlane;fontStyle=0;align=center;verticalAlign=top;childLayout=stackLayout;horizontal=1;"
                 f"startSize={box.header};horizontalStack=0;resizeParent=1;resizeParentMax=0;resizeLast=0;collapsible=1;"
                 f"marginBottom=0;html=1;whiteSpace=wrap;fontSize=12;fillColor=#FFFFFF;strokeColor=#000000;fontColor=#000000;"
                 f"swimlaneFillColor=#FFFFFF;")
        cells.append(f'<mxCell id="{cid}" value="{quote(header)}" style="{style}" vertex="1" parent="1">'
                     f'<mxGeometry x="{x:.0f}" y="{y:.0f}" width="{box.width}" height="{box.height}" as="geometry"/></mxCell>')
        text_style = ("text;strokeColor=none;fillColor=none;align=left;verticalAlign=top;spacingLeft=4;spacingRight=4;"
                      "overflow=hidden;rotatable=0;points=[[0,0.5],[1,0.5]];portConstraint=eastwest;html=1;fontSize=11;"
                      "fontColor=#000000;")
        line_style = "line;strokeWidth=1;fillColor=none;strokeColor=#000000;align=left;verticalAlign=middle;spacingTop=-1;rotatable=0;points=[];"
        offset = box.header
        cells.append(f'<mxCell id="{cid}l1" value="" style="{line_style}" vertex="1" parent="{cid}">'
                     f'<mxGeometry y="{offset}" width="{box.width}" height="8" as="geometry"/></mxCell>')
        offset += 8
        cells.append(f'<mxCell id="{cid}a" value="{quote(html_lines(box.attributes))}" style="{text_style}" vertex="1" parent="{cid}">'
                     f'<mxGeometry y="{offset}" width="{box.width}" height="{box.attr_h}" as="geometry"/></mxCell>')
        offset += box.attr_h
        cells.append(f'<mxCell id="{cid}l2" value="" style="{line_style}" vertex="1" parent="{cid}">'
                     f'<mxGeometry y="{offset}" width="{box.width}" height="8" as="geometry"/></mxCell>')
        offset += 8
        cells.append(f'<mxCell id="{cid}o" value="{quote(html_lines(box.operations))}" style="{text_style}" vertex="1" parent="{cid}">'
                     f'<mxGeometry y="{offset}" width="{box.width}" height="{box.ops_h}" as="geometry"/></mxCell>')

    for i, r in enumerate(relations):
        points = routes.get(i)
        if not points or len(points) < 2:
            continue
        source, target = boxes[r["source"]], boxes[r["target"]]
        exit_x, exit_y = relative(points[0], source)
        entry_x, entry_y = relative(points[-1], target)
        style = (f"edgeStyle=none;rounded=0;html=1;endSize=10;strokeColor=#000000;exitX={exit_x:.3f};exitY={exit_y:.3f};exitDx=0;exitDy=0;"
                 f"entryX={entry_x:.3f};entryY={entry_y:.3f};entryDx=0;entryDy=0;" + EDGE_DRAWIO[r["kind"]])
        waypoints = "".join(f'<mxPoint x="{x + margin:.0f}" y="{y + margin:.0f}"/>' for x, y in points[1:-1])
        geometry = f'<mxGeometry relative="1" as="geometry"><Array as="points">{waypoints}</Array></mxGeometry>'
        cells.append(f'<mxCell id="e{i}" value="" style="{style}" edge="1" parent="1" source="{ids[r["source"]]}" '
                     f'target="{ids[r["target"]]}">{geometry}</mxCell>')

    return (f'<diagram id="{page_id}" name="{quote(name)}"><mxGraphModel dx="1600" dy="900" grid="1" gridSize="10" '
            f'guides="1" tooltips="1" connect="1" arrows="1" fold="1" page="0" pageScale="1" math="0" shadow="0">'
            f'<root>{"".join(cells)}</root></mxGraphModel></diagram>')


def relative(point, box):
    x = min(1.0, max(0.0, (point[0] - box.x) / box.width))
    y = min(1.0, max(0.0, (point[1] - box.y) / box.height))
    return x, y


# ============================== Страницы ==============================

CLASSES = OrderedDict((c["name"], c) for c in model.CLASSES)
PACKAGES = OrderedDict((p[0], {"name": p[0], "namespace": p[1], "description": p[2]}) for p in model.PACKAGES)


class Page:
    def __init__(self, page_id, title, file_name, boxes, relations, groups):
        self.page_id, self.title, self.file_name = page_id, title, file_name
        self.boxes, self.relations = boxes, relations
        self.width, self.height, self.frames, self.routes = layout(boxes, relations, groups)

    def svg_body(self):
        return render_svg(self.boxes, self.relations, self.routes, self.frames, self.width, self.height)

    def mx(self):
        return mx_page(self.page_id, self.title, self.boxes, self.relations, self.routes, self.frames)


def class_page(page_id, title, file_name, packages):
    types = {t.name: t for t in csharp_api.load_all()}
    names = [n for n, c in CLASSES.items() if c["package"] in packages]
    boxes = OrderedDict((n, Box(n, CLASSES[n]["stereotype"], types[n])) for n in names)
    relations = [r for r in model.RELATIONS if r["source"] in boxes]
    # Классы из других пакетов, на которые есть ссылки, — пунктирные "призраки" без членов.
    for r in relations:
        if r["target"] not in boxes:
            boxes[r["target"]] = Box(r["target"], CLASSES[r["target"]]["stereotype"], types[r["target"]], ghost=True)
    groups = {n: f"{CLASSES[n]['package']}" for n in names}
    return Page(page_id, title, file_name, boxes, relations, groups)


class PackageBox(Box):
    def __init__(self, package):
        self.name = package["name"]
        self.stereotype = "package"
        self.label = "«namespace»"
        self.italic = False
        self.ghost = False
        count = sum(1 for c in CLASSES.values() if c["package"] == package["name"])
        self.attributes = [Line(package["namespace"]), Line(f"классов: {count}")]
        self.operations = [Line(package["description"])]
        self.header = 40
        self.attr_h = len(self.attributes) * LINE_HEIGHT + 8
        self.ops_h = LINE_HEIGHT + 8
        self.height = self.header + 8 + self.attr_h + 8 + self.ops_h
        self.width = int(max(METER.width(l.text, FONT_SIZE) for l in self.attributes + self.operations) + 2 * PAD_X + 6)
        self.x = self.y = 0.0


def package_page():
    boxes = OrderedDict((p["name"], PackageBox(p)) for p in PACKAGES.values())
    seen = set()
    relations = []
    for r in model.RELATIONS:
        a, b = CLASSES[r["source"]]["package"], CLASSES[r["target"]]["package"]
        if a != b and (a, b) not in seen:
            seen.add((a, b))
            relations.append({"source": a, "target": b, "kind": "dep", "label": ""})
    page = Page.__new__(Page)
    page.page_id, page.title, page.file_name = "packages", "Пакеты", "Packages.drawio.svg"
    page.boxes, page.relations = boxes, relations
    # Для пакетов зависимости тоже влияют на ранги, иначе всё выстраивается в одну строку.
    ranked = [dict(r, kind="assoc") for r in relations]
    page.width, page.height, page.frames, page.routes = layout(boxes, ranked, {})
    return page


# ============================== Вывод ==============================

def validate():
    errors = []
    code = {t.name for t in csharp_api.load_all()}
    for name in code - set(CLASSES):
        errors.append(f"класс {name} есть в коде, но не описан в uml_model.py (добавьте C(...))")
    for name in set(CLASSES) - code:
        errors.append(f"класс {name} описан в uml_model.py, но не найден в коде")
    for c in CLASSES.values():
        if c["package"] not in PACKAGES:
            errors.append(f"класс {c['name']}: неизвестный пакет {c['package']}")
    for r in model.RELATIONS:
        for end in ("source", "target"):
            if r[end] not in CLASSES:
                errors.append(f"связь {r['source']} -> {r['target']}: нет класса {r[end]}")
        if r["kind"] not in EDGE_SVG:
            errors.append(f"связь {r['source']} -> {r['target']}: неизвестный тип {r['kind']}")
    if errors:
        raise SystemExit("Ошибки модели UML:\n  " + "\n  ".join(errors))


def write_drawio_svg(page, path):
    width, height, body = page.svg_body()
    mxfile = f'<mxfile host="generate_uml.py" type="device">{page.mx()}</mxfile>'
    svg = ('<?xml version="1.0" encoding="UTF-8"?>\n'
           '<!-- Сгенерировано Docs/tools/generate_uml.py. Открывается в draw.io (диаграмма внутри атрибута content). -->\n'
           f'<svg xmlns="http://www.w3.org/2000/svg" version="1.1" width="{width:.0f}px" height="{height:.0f}px" '
           f'viewBox="0 0 {width:.0f} {height:.0f}" content="{quote(mxfile)}">{body}</svg>\n')
    with open(path, "w", encoding="utf-8") as f:
        f.write(svg)


def write_markdown(pages):
    out = ["# UML-диаграммы классов\n",
           "> Сгенерировано `Docs/tools/generate_uml.py`: члены классов берутся из кода, связи и описания — из "
           "`Docs/tools/uml_model.py`. Не правьте вручную. Полное описание каждого члена — в [API.md](API.md), "
           "устройство системы — в [ARCHITECTURE.md](ARCHITECTURE.md).\n",
           "Файлы `*.drawio.svg` — это одновременно картинка (открывается в браузере/на GitHub) и редактируемая "
           "диаграмма draw.io. Все страницы вместе — в `Prod/Prod_di.drawio`.\n",
           "## Нотация\n",
           "| Обозначение | Значение |", "|---|---|",
           "| `+` `-` `#` `~` | public, private, protected, internal |",
           "| подчёркнуто | статический член |",
           "| *курсив* | абстрактный класс / метод, интерфейс |",
           "| `- поле` у MonoBehaviour/ScriptableObject | приватное поле `[SerializeField]` (настраивается в инспекторе) |",
           "| сплошная линия, пустой треугольник | наследование |",
           "| пунктир, пустой треугольник | реализация интерфейса |",
           "| линия с чёрным ромбом у владельца | композиция (владеет, создаёт) |",
           "| сплошная линия, открытая стрелка | ассоциация (хранит ссылку) |",
           "| пунктир, открытая стрелка | зависимость (использует, создаёт, публикует) |",
           "| пунктирный блок без членов | класс из другого пакета/диаграммы |",
           "| пунктирная рамка | пакет (namespace) |", ""]
    for page in pages:
        out.append(f"## {page.title}\n")
        out.append(f"![{page.title}](uml/{page.file_name})\n")

    out.append("## Классы\n")
    out.append("| Класс | Пакет | Вид | Паттерн | Назначение |")
    out.append("|---|---|---|---|---|")
    for c in CLASSES.values():
        out.append(f"| [`{c['name']}`](API.md#{c['name'].lower()}) | {c['package']} | {c['stereotype']} | "
                   f"{c['pattern'] or ''} | {c['description']} |")
    out.append("")
    with open(MARKDOWN_PATH, "w", encoding="utf-8") as f:
        f.write("\n".join(out))


def main():
    if shutil.which("dot") is None:
        raise SystemExit("Нужен graphviz (команда dot): https://graphviz.org/download/")
    validate()
    os.makedirs(UML_DIR, exist_ok=True)

    runtime_packages = [p for p in PACKAGES if p != EDITOR_PACKAGE]
    pages = [
        class_page("runtime", "Классы игры", "ClassDiagram.drawio.svg", runtime_packages),
        class_page("editor", "Редактор (генератор демо)", "EditorDiagram.drawio.svg", [EDITOR_PACKAGE]),
        package_page(),
    ]
    for page in pages:
        write_drawio_svg(page, os.path.join(UML_DIR, page.file_name))

    with open(DRAWIO_PATH, "w", encoding="utf-8") as f:
        f.write('<mxfile host="generate_uml.py" type="device">' + "".join(p.mx() for p in pages) + "</mxfile>\n")
    write_markdown(pages)

    for page in pages:
        print(f"{page.title}: {len(page.boxes)} блоков, {len(page.relations)} связей, {page.width:.0f}x{page.height:.0f}px")
    print("OK ->", os.path.relpath(DRAWIO_PATH, PROD), "и Docs/uml/*.drawio.svg, Docs/UML.md")


if __name__ == "__main__":
    main()
