# -*- coding: utf-8 -*-
"""
Генератор UML-диаграмм из Docs/uml/uml_model.py.

    python3 Docs/uml/generate_uml.py        (запускать из папки Prod)

Создаёт:
    Prod_di.drawio  — диаграмма для draw.io / diagrams.net (страницы: обзор, все классы, по пакетам)
    Docs/UML.md     — те же диаграммы в Mermaid (GitHub рисует их прямо в браузере) + таблицы описаний
"""
import math
import os
import sys
from collections import OrderedDict
from html import escape
from xml.sax.saxutils import quoteattr

HERE = os.path.dirname(os.path.abspath(__file__))
sys.path.insert(0, HERE)
import uml_model as model  # noqa: E402

PROD = os.path.abspath(os.path.join(HERE, "..", ".."))
DRAWIO_PATH = os.path.join(PROD, "Prod_di.drawio")
MARKDOWN_PATH = os.path.join(PROD, "Docs", "UML.md")

BOX_WIDTH = 280
CHARS_PER_LINE = 44
LINE_HEIGHT = 15

FILL = {
    "interface": "#d5e8d4", "abstract": "#fff2cc", "ScriptableObject": "#e1d5e7", "MonoBehaviour": "#dae8fc",
    "static": "#eeeeee", "struct": "#ffe6cc", "enum": "#ffe6cc", "Editor": "#f8cecc", "class": "#ffffff",
}
STROKE = {
    "interface": "#82b366", "abstract": "#d6b656", "ScriptableObject": "#9673a6", "MonoBehaviour": "#6c8ebf",
    "static": "#999999", "struct": "#d79b00", "enum": "#d79b00", "Editor": "#b85450", "class": "#666666",
}
EDGE_STYLE = {
    "inherit": "endArrow=block;endFill=0;endSize=12;",
    "implement": "endArrow=block;endFill=0;endSize=12;dashed=1;",
    "compose": "endArrow=open;startArrow=diamondThin;startFill=1;startSize=14;",
    "assoc": "endArrow=open;endFill=0;",
    "dep": "endArrow=open;endFill=0;dashed=1;",
}
MERMAID_ARROW = {"inherit": "--|>", "implement": "..|>", "compose": "*--", "assoc": "-->", "dep": "..>"}
KIND_RU = {"inherit": "наследует", "implement": "реализует", "compose": "владеет", "assoc": "ссылается", "dep": "использует"}

CLASSES = OrderedDict((c["name"], c) for c in model.CLASSES)
PACKAGES = OrderedDict((p[0], {"name": p[0], "namespace": p[1], "description": p[2]}) for p in model.PACKAGES)


def validate():
    errors = []
    for c in CLASSES.values():
        if c["package"] not in PACKAGES:
            errors.append(f"класс {c['name']}: неизвестный пакет {c['package']}")
    for r in model.RELATIONS:
        for end in ("source", "target"):
            if r[end] not in CLASSES:
                errors.append(f"связь {r['source']} -> {r['target']}: нет класса {r[end]}")
        if r["kind"] not in EDGE_STYLE:
            errors.append(f"связь {r['source']} -> {r['target']}: неизвестный тип {r['kind']}")
    if errors:
        raise SystemExit("Ошибки в uml_model.py:\n  " + "\n  ".join(errors))


# ============================== draw.io ==============================

def wrapped_lines(text):
    return max(1, math.ceil(len(text) / CHARS_PER_LINE))


def section_height(lines):
    return sum(wrapped_lines(line) for line in lines) * LINE_HEIGHT + 10


def drawio_text(text):
    return escape(text.replace("~", "<", 1).replace("~", ">", 1) if text.count("~") == 2 else text.replace("~", ""))


class DrawioPage:
    def __init__(self, name):
        self.name = name
        self.cells = []
        self.ids = {}
        self.counter = 0

    def new_id(self, prefix):
        self.counter += 1
        return f"{prefix}-{self.counter}"

    def vertex(self, cell_id, parent, value, style, x, y, w, h):
        self.cells.append(
            f'<mxCell id="{cell_id}" parent="{parent}" value={quoteattr(value)} style="{style}" vertex="1">'
            f'<mxGeometry x="{x:.0f}" y="{y:.0f}" width="{w:.0f}" height="{h:.0f}" as="geometry"/></mxCell>')

    def edge(self, source, target, kind, label):
        style = "edgeStyle=orthogonalEdgeStyle;rounded=1;html=1;fontSize=10;jettySize=auto;" + EDGE_STYLE[kind]
        self.cells.append(
            f'<mxCell id="{self.new_id("e")}" parent="1" source="{source}" target="{target}" value={quoteattr(label)} '
            f'style="{style}" edge="1"><mxGeometry relative="1" as="geometry"/></mxCell>')

    def xml(self, page_index):
        body = "".join(self.cells)
        return (f'<diagram id="page-{page_index}" name={quoteattr(self.name)}>'
                f'<mxGraphModel dx="1600" dy="900" grid="1" gridSize="10" guides="1" tooltips="1" connect="1" arrows="1" '
                f'fold="1" page="0" pageScale="1" math="0" shadow="0"><root><mxCell id="0"/><mxCell id="1" parent="0"/>'
                f'{body}</root></mxGraphModel></diagram>')


def class_height(c):
    height = 46 + (16 if c["pattern"] else 0) + section_height([c["description"]])
    if c["fields"]:
        height += 8 + section_height(c["fields"])
    if c["methods"]:
        height += 8 + section_height(c["methods"])
    return height


def add_class(page, c, parent, x, y, ghost=False):
    """Класс как swimlane со стеком: описание / поля / методы."""
    cell_id = page.new_id("c")
    page.ids[c["name"]] = cell_id
    stereotype = c["stereotype"]

    if ghost:
        value = f'<i>{escape(c["package"])}</i><br><b>{escape(c["name"])}</b>'
        style = ("rounded=0;whiteSpace=wrap;html=1;dashed=1;fillColor=#fafafa;strokeColor=#bbbbbb;fontColor=#777777;"
                 "fontSize=11;")
        page.vertex(cell_id, parent, value, style, x, y, BOX_WIDTH * 0.7, 44)
        return 44

    header = f"«{escape(stereotype)}»<br><b>{escape(c['name'])}</b>"
    if c["pattern"]:
        header += f'<br><font color="#b00000">[{escape(c["pattern"])}]</font>'
    start = 46 + (16 if c["pattern"] else 0)
    height = class_height(c)
    style = (f"swimlane;fontStyle=0;align=center;verticalAlign=top;childLayout=stackLayout;horizontal=1;startSize={start};"
             f"horizontalStack=0;resizeParent=1;resizeParentMax=0;resizeLast=0;collapsible=1;marginBottom=0;html=1;"
             f"whiteSpace=wrap;fillColor={FILL.get(stereotype, '#ffffff')};strokeColor={STROKE.get(stereotype, '#666666')};"
             f"fontSize=12;swimlaneFillColor=#ffffff;")
    page.vertex(cell_id, parent, header, style, x, y, BOX_WIDTH, height)

    text_style = ("text;strokeColor=none;fillColor=none;align=left;verticalAlign=top;spacingLeft=4;spacingRight=4;"
                  "overflow=hidden;rotatable=0;points=[[0,0.5],[1,0.5]];portConstraint=eastwest;whiteSpace=wrap;html=1;fontSize=11;")
    line_style = "line;strokeWidth=1;fillColor=none;align=left;verticalAlign=middle;spacingTop=-1;rotatable=0;points=[];"

    offset = start
    desc_height = section_height([c["description"]])
    page.vertex(page.new_id("t"), cell_id, f"<i>{escape(c['description'])}</i>", text_style + "fontColor=#555555;",
                0, offset, BOX_WIDTH, desc_height)
    offset += desc_height
    for lines in (c["fields"], c["methods"]):
        if not lines:
            continue
        page.vertex(page.new_id("l"), cell_id, "", line_style, 0, offset, BOX_WIDTH, 8)
        offset += 8
        h = section_height(lines)
        page.vertex(page.new_id("t"), cell_id, "<br>".join(drawio_text(l) for l in lines), text_style, 0, offset, BOX_WIDTH, h)
        offset += h
    return height


def layout_columns(items, heights, columns):
    cols = [[] for _ in range(columns)]
    col_heights = [0] * columns
    for item, h in zip(items, heights):
        index = col_heights.index(min(col_heights))
        cols[index].append(item)
        col_heights[index] += h + 30
    return cols, max(col_heights) if items else 0


def columns_for(count):
    return 1 if count <= 2 else 2 if count <= 6 else 3 if count <= 12 else 4


def add_package(page, package, classes, x, y, ghosts=()):
    """Пакет-контейнер с классами внутри. Возвращает (ширина, высота)."""
    items = list(classes) + list(ghosts)
    heights = [class_height(c) if c in classes else 44 for c in items]
    columns = columns_for(len(items))
    cols, inner_height = layout_columns(items, heights, columns)
    width = columns * (BOX_WIDTH + 30) + 30
    height = inner_height + 50

    package_id = page.new_id("p")
    label = f"<b>{escape(package['name'])}</b>&nbsp;&nbsp;<font color=\"#666666\">{escape(package['namespace'])} — {escape(package['description'])}</font>"
    style = ("swimlane;startSize=28;html=1;whiteSpace=wrap;fillColor=#f7f7f7;strokeColor=#888888;fontSize=12;align=left;"
             "spacingLeft=8;collapsible=0;swimlaneFillColor=#fcfcfc;")
    page.vertex(package_id, "1", label, style, x, y, width, height)

    for col_index, col in enumerate(cols):
        cy = 44
        for c in col:
            h = add_class(page, c, package_id, 30 + col_index * (BOX_WIDTH + 30), cy, ghost=c not in classes)
            cy += h + 30
    return width, height


def package_classes(name):
    return [c for c in CLASSES.values() if c["package"] == name]


def build_full_page():
    page = DrawioPage("Все классы")
    x = y = 0
    row_height = 0
    max_width = 3400
    add_legend(page, -420, 0)
    for package in PACKAGES.values():
        classes = package_classes(package["name"])
        w = columns_for(len(classes)) * (BOX_WIDTH + 30) + 30
        if x > 0 and x + w > max_width:
            x = 0
            y += row_height + 60
            row_height = 0
        w, h = add_package(page, package, classes, x, y)
        x += w + 60
        row_height = max(row_height, h)
    for r in model.RELATIONS:
        page.edge(page.ids[r["source"]], page.ids[r["target"]], r["kind"], r["label"])
    return page


def build_package_page(package):
    page = DrawioPage(f"Пакет {package['name']}")
    classes = package_classes(package["name"])
    names = {c["name"] for c in classes}
    relations = [r for r in model.RELATIONS if r["source"] in names or r["target"] in names]
    external = OrderedDict()
    for r in relations:
        for end in (r["source"], r["target"]):
            if end not in names:
                external[end] = CLASSES[end]

    add_package(page, package, classes, 0, 0)
    # Внешние классы — "призраки" справа, чтобы были видны связи с другими пакетами.
    gx = columns_for(len(classes)) * (BOX_WIDTH + 30) + 120
    for i, c in enumerate(external.values()):
        add_class(page, c, "1", gx + (i % 2) * (BOX_WIDTH * 0.7 + 30), 40 + (i // 2) * 74, ghost=True)
    for r in relations:
        page.edge(page.ids[r["source"]], page.ids[r["target"]], r["kind"], r["label"])
    return page


def package_dependencies():
    deps = OrderedDict()
    for r in model.RELATIONS:
        a = CLASSES[r["source"]]["package"]
        b = CLASSES[r["target"]]["package"]
        if a != b:
            deps.setdefault((a, b), 0)
            deps[(a, b)] += 1
    return deps


def build_overview_page():
    page = DrawioPage("Обзор пакетов")
    add_legend(page, -420, 0)
    columns = 4
    for i, package in enumerate(PACKAGES.values()):
        x = (i % columns) * 380
        y = (i // columns) * 200
        cell_id = page.new_id("p")
        page.ids[package["name"]] = cell_id
        count = len(package_classes(package["name"]))
        value = (f"<b>{escape(package['name'])}</b><br><font color=\"#666666\">{escape(package['namespace'])}</font><br>"
                 f"<i>{escape(package['description'])}</i><br>классов: {count}")
        style = "shape=folder;tabWidth=80;tabHeight=16;tabPosition=left;html=1;whiteSpace=wrap;fillColor=#dae8fc;strokeColor=#6c8ebf;fontSize=12;"
        page.vertex(cell_id, "1", value, style, x, y, 300, 120)
    for (a, b), n in package_dependencies().items():
        page.edge(page.ids[a], page.ids[b], "dep", str(n))
    return page


def add_legend(page, x, y):
    lines = ["<b>Легенда</b>"]
    for stereotype, color in FILL.items():
        lines.append(f'<span style="background:{color};border:1px solid {STROKE[stereotype]}">&nbsp;&nbsp;&nbsp;&nbsp;</span> «{stereotype}»')
    lines += ["", "—▷ наследование", "- -▷ реализация интерфейса", "◆— владеет (композиция)", "—> ссылается (ассоциация)",
              "- -> использует (зависимость)", "", "<font color=\"#b00000\">[Pattern]</font> — паттерн проектирования",
              "Пунктирные серые блоки — классы из других пакетов"]
    style = "rounded=1;whiteSpace=wrap;html=1;align=left;verticalAlign=top;spacing=10;fillColor=#ffffff;strokeColor=#999999;fontSize=11;"
    page.vertex(page.new_id("legend"), "1", "<br>".join(lines), style, x, y, 360, 22 * len(lines))


def write_drawio():
    pages = [build_overview_page(), build_full_page()] + [build_package_page(p) for p in PACKAGES.values()]
    xml = '<mxfile host="generate_uml.py" type="device">' + "".join(p.xml(i) for i, p in enumerate(pages)) + "</mxfile>\n"
    with open(DRAWIO_PATH, "w", encoding="utf-8") as f:
        f.write(xml)


# ============================== Mermaid ==============================

def mermaid_member(text):
    return text.replace("[", "").replace("]", "")


def mermaid_class(c):
    lines = [f"    class {c['name']} {{", f"        <<{c['stereotype']}>>"]
    lines += [f"        {mermaid_member(f)}" for f in c["fields"]]
    lines += [f"        {mermaid_member(m)}" for m in c["methods"]]
    lines.append("    }")
    return lines


def mermaid_relation(r):
    label = f" : {r['label']}" if r["label"] else ""
    return f"    {r['source']} {MERMAID_ARROW[r['kind']]} {r['target']}{label}"


def write_markdown():
    out = []
    out.append("# UML-схема проекта\n")
    out.append("> Файл сгенерирован `Docs/uml/generate_uml.py` из `Docs/uml/uml_model.py`. Не правьте вручную — "
               "правьте модель и перезапустите `python3 Docs/uml/generate_uml.py` (из папки `Prod`). "
               "Та же схема для draw.io лежит в `Prod/Prod_di.drawio` (страницы: обзор, все классы, по пакетам).\n")
    out.append("**Обозначения.** Стереотип в `<<...>>`: `MonoBehaviour` — компонент Unity, `ScriptableObject` — ассет с данными, "
               "`static` — статический класс, `interface`, `abstract`, `struct`, `enum`. Связи: `--|>` наследование, `..|>` реализация "
               "интерфейса, `*--` владеет, `-->` ссылается, `..>` использует. `$` — статический член, `*` — абстрактный, "
               "`#` — protected, `-` — private.\n")

    out.append("## Паттерны проектирования\n")
    out.append("| Паттерн | Где | Зачем |")
    out.append("|---|---|---|")
    patterns = OrderedDict()
    for c in CLASSES.values():
        if c["pattern"]:
            patterns.setdefault(c["pattern"], []).append(c)
    for pattern, classes in patterns.items():
        why = model.PATTERN_NOTES.get(pattern, classes[0]["description"])
        out.append(f"| **{pattern}** | {', '.join('`' + c['name'] + '`' for c in classes)} | {why} |")
    out.append("")

    out.append("## Обзор пакетов\n")
    out.append("```mermaid\nflowchart LR")
    for package in PACKAGES.values():
        out.append(f'    {package["name"].replace(".", "_")}["{package["name"]}<br/><small>{package["description"]}</small>"]')
    for (a, b), n in package_dependencies().items():
        out.append(f"    {a.replace('.', '_')} -->|{n}| {b.replace('.', '_')}")
    out.append("```\n")

    for package in PACKAGES.values():
        classes = package_classes(package["name"])
        names = {c["name"] for c in classes}
        out.append(f"## {package['name']} — `{package['namespace']}`\n")
        out.append(f"{package['description']}.\n")
        out.append("```mermaid\nclassDiagram\n    direction LR")
        for c in classes:
            out += mermaid_class(c)
        for r in model.RELATIONS:
            if r["source"] in names:
                out.append(mermaid_relation(r))
        out.append("```\n")
        out.append("| Класс | Тип | Что делает |")
        out.append("|---|---|---|")
        for c in classes:
            pattern = f" **[{c['pattern']}]**" if c["pattern"] else ""
            out.append(f"| `{c['name']}` | {c['stereotype']} | {c['description']}{pattern} |")
        out.append("")

    with open(MARKDOWN_PATH, "w", encoding="utf-8") as f:
        f.write("\n".join(out))


if __name__ == "__main__":
    validate()
    write_drawio()
    write_markdown()
    print(f"OK: {len(CLASSES)} классов, {len(model.RELATIONS)} связей, {len(PACKAGES)} пакетов")
    print(f"  -> {os.path.relpath(DRAWIO_PATH, PROD)}")
    print(f"  -> {os.path.relpath(MARKDOWN_PATH, PROD)}")
