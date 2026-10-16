# -*- coding: utf-8 -*-
"""
Лёгкий парсер C#-исходников проекта: типы, их члены и XML-комментарии (///).

Не полноценный компилятор — рассчитан на стиль кода проекта (одно объявление на строку,
фигурные скобки на отдельных строках). Используется генераторами документации и UML,
чтобы они всегда совпадали с кодом.
"""
import os
import re
from dataclasses import dataclass, field
from typing import List, Optional

SCRIPTS_ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", "..", "Assets", "Game", "Scripts"))

TYPE_RE = re.compile(
    r"^(?P<mods>(?:(?:public|internal|private|protected|static|sealed|abstract|readonly|partial)\s+)*)"
    r"(?P<kind>class|interface|struct|enum)\s+(?P<name>\w+)(?P<generic><[^>]*>)?\s*(?::\s*(?P<bases>[^{]+?))?\s*(?:where .*)?$")
MODIFIERS = {"public", "private", "protected", "internal", "static", "readonly", "const", "sealed", "abstract",
             "virtual", "override", "new", "event", "extern", "unsafe", "volatile", "async", "partial"}
UNITY_MESSAGES = {"Awake", "Start", "Update", "LateUpdate", "FixedUpdate", "OnEnable", "OnDisable", "OnDestroy",
                  "OnValidate", "OnGUI", "OnTriggerEnter", "OnAnimatorIK", "Reset"}


@dataclass
class Member:
    kind: str            # field, property, event, method, constructor, const, enum-value
    name: str
    type: str            # тип поля/свойства или возвращаемый тип метода
    params: str          # параметры метода как в коде
    visibility: str      # public, protected, internal, private
    is_static: bool
    is_abstract: bool
    serialized: bool     # [SerializeField] private
    summary: str
    signature: str       # объявление как в коде (без тела)
    param_docs: dict = field(default_factory=dict)
    line: int = -1       # индекс строки объявления в файле (0-based)


@dataclass
class TypeInfo:
    name: str
    kind: str            # class, interface, struct, enum
    namespace: str
    modifiers: List[str]
    bases: List[str]
    summary: str
    attributes: List[str]
    file: str
    members: List[Member] = field(default_factory=list)

    @property
    def is_static(self): return "static" in self.modifiers

    @property
    def is_abstract(self): return "abstract" in self.modifiers

    @property
    def visibility(self): return "internal" if "internal" in self.modifiers else "public"


# ---------------------------------------------------------------------------

def _strip_code(line: str) -> str:
    """Убирает строки, символы и комментарии — чтобы правильно считать фигурные скобки."""
    line = re.sub(r'@"(?:[^"]|"")*"', '""', line)
    line = re.sub(r'\$?"(?:\\.|[^"\\])*"', '""', line)
    line = re.sub(r"'(?:\\.|[^'\\])'", "''", line)
    return re.sub(r"//.*", "", line)


INHERIT = "@inheritdoc"


def _clean_xml_doc(lines: List[str]):
    text = " ".join(l.strip() for l in lines)
    if "<inheritdoc" in text:
        return INHERIT, {}
    params = {m.group(1): _xml_to_text(m.group(2)) for m in re.finditer(r'<param name="(\w+)">(.*?)</param>', text)}
    summary = re.search(r"<summary>(.*?)</summary>", text)
    summary = summary.group(1) if summary else re.sub(r"<param.*?</param>", "", text)
    return _xml_to_text(summary), params


def _xml_to_text(text: str) -> str:
    text = re.sub(r'<see cref="([^"]+)"\s*/>', lambda m: "`" + m.group(1).split(".")[-1] + "`", text)
    text = re.sub(r"<c>(.*?)</c>", r"`\1`", text)
    text = re.sub(r"<[^>]+>", "", text)
    text = text.replace("&lt;", "<").replace("&gt;", ">").replace("&amp;", "&")
    return re.sub(r"\s+", " ", text).strip()


def _split_top_level(text: str, sep: str = ",") -> List[str]:
    parts, depth, current = [], 0, ""
    for ch in text:
        if ch in "<([{":
            depth += 1
        elif ch in ">)]}":
            depth -= 1
        if ch == sep and depth == 0:
            parts.append(current.strip())
            current = ""
        else:
            current += ch
    if current.strip():
        parts.append(current.strip())
    return parts


def _tokens(text: str) -> List[str]:
    """Разбивает 'public static Dictionary<A, B> Name' на токены, не разрывая generic-и."""
    tokens, depth, current = [], 0, ""
    for ch in text:
        if ch == "<":
            depth += 1
        elif ch == ">":
            depth -= 1
        if ch.isspace() and depth == 0:
            if current:
                tokens.append(current)
            current = ""
        else:
            current += ch
    if current:
        tokens.append(current)
    return tokens


def _parse_member(decl: str, type_kind: str, summary: str, param_docs: dict, tooltip: Optional[str], type_name: str) -> Optional[Member]:
    serialized = "[SerializeField" in decl
    decl = re.sub(r"^\s*(\[[^\]]*\]\s*)+", "", decl).strip()
    if not decl or decl.startswith(("[", "#")):
        return None

    if type_kind == "enum":
        name = decl.split("=")[0].strip().rstrip(",")
        values = [v.strip() for v in name.split(",") if v.strip()]
        return Member("enum-value", ", ".join(values), "", "", "public", True, False, False, summary, decl.rstrip(","))

    # Первый значимый символ определяет вид объявления.
    positions = {ch: decl.find(ch) for ch in ("(", "=", "{", ";")}
    arrow = decl.find("=>")
    first = min((p, ch) for ch, p in positions.items() if p >= 0) if any(p >= 0 for p in positions.values()) else (len(decl), "{")
    head_end = first[0]
    kind_char = first[1]
    if kind_char == "=" and arrow == head_end:
        kind_char = "=>"

    head = decl[:head_end].strip()
    tokens = _tokens(head)
    mods = [t for t in tokens if t in MODIFIERS]
    rest = [t for t in tokens if t not in MODIFIERS]
    if not rest:
        return None

    visibility = next((m for m in ("public", "protected", "internal", "private") if m in mods), None)
    if visibility is None:
        visibility = "public" if type_kind == "interface" else "private"
    is_static = "static" in mods or "const" in mods
    is_abstract = "abstract" in mods or type_kind == "interface"

    if kind_char == "(":
        name = rest[-1]
        ret = " ".join(rest[:-1])
        depth, end = 0, head_end
        for i in range(head_end, len(decl)):
            if decl[i] == "(":
                depth += 1
            elif decl[i] == ")":
                depth -= 1
                if depth == 0:
                    end = i
                    break
        params = decl[head_end + 1:end].strip()
        if name.split("<")[0] == type_name and not ret:
            kind, ret = "constructor", ""
        elif "event" in mods:
            return None
        else:
            kind = "method"
        signature = decl[:end + 1] + decl[end + 1:].split("{")[0].split("=>")[0].rstrip().rstrip(";")
        return Member(kind, name, ret, params, visibility, is_static, is_abstract, serialized, summary,
                      re.sub(r"\s+", " ", signature).strip(), param_docs)

    if "," in head and "<" not in head.split(",")[0].split()[-1]:
        # public GameObject Player, Dummy, Turret;  → одно объявление на несколько полей
        first_part, *others = [p.strip() for p in head.split(",")]
        first_tokens = [t for t in _tokens(first_part) if t not in MODIFIERS]
        rest = [first_tokens[0], ", ".join([first_tokens[-1]] + others)]
    name = rest[-1]
    typ = " ".join(rest[:-1])
    if "event" in mods:
        kind = "event"
    elif "const" in mods:
        kind = "const"
    elif kind_char in ("{", "=>"):
        kind = "property"
    else:
        kind = "field"
    if kind == "field" and not summary and tooltip:
        summary = tooltip
    signature = re.sub(r"\s+", " ", decl.split("{")[0].split("=>")[0].split("=")[0] if kind != "property" else head).strip().rstrip(";")
    if kind == "property":
        accessor = "{ get; set; }" if re.search(r"\bset\b", decl) and "private set" not in decl else "{ get; }"
        signature = f"{head} {accessor}"
    return Member(kind, name, typ, "", visibility, is_static, is_abstract, serialized, summary, signature, param_docs)


def parse_file(path: str) -> List[TypeInfo]:
    with open(path, encoding="utf-8-sig") as f:
        lines = f.read().splitlines()

    types: List[TypeInfo] = []
    namespace = ""
    depth = 0
    doc: List[str] = []
    attributes: List[str] = []
    tooltip = None
    current: Optional[TypeInfo] = None
    type_depth = None
    pending_decl = None
    entered = False
    decl_line = -1
    i = 0

    while i < len(lines):
        raw = lines[i]
        stripped = raw.strip()
        i += 1

        if stripped.startswith("///"):
            doc.append(stripped[3:])
            continue

        code = _strip_code(raw)

        if pending_decl is None and stripped and not stripped.startswith("//"):
            ns = re.match(r"namespace\s+([\w.]+)", stripped)
            if ns:
                namespace = ns.group(1)
            elif re.match(r"^\[[^\]]*\]$", stripped) and not stripped.startswith("[SerializeField]"):
                attributes.append(stripped)
                tip = re.match(r'\[Tooltip\("(.*)"\)\]', stripped)
                if tip:
                    tooltip = tip.group(1)
            else:
                inline_body = None
                one_line = re.match(r"^(.*?\b(?:class|interface|struct|enum)\s+\w+[^{]*)\{(.*)\}\s*$", stripped)
                if one_line:
                    inline_body = one_line.group(2).strip()
                m = TYPE_RE.match(one_line.group(1).strip() if one_line else stripped)
                is_top_level = depth == 1 or (current is not None and depth == type_depth)
                if m and depth == 1:
                    mods = m.group("mods").split()
                    if "private" not in mods:
                        summary, _ = _clean_xml_doc(doc)
                        bases = _split_top_level(m.group("bases")) if m.group("bases") else []
                        current = TypeInfo(m.group("name"), m.group("kind"), namespace, mods,
                                           [b.strip() for b in bases], summary, attributes, path)
                        types.append(current)
                        type_depth = 2
                        entered = False
                        if inline_body is not None:
                            # Тип целиком на одной строке: enum A { X, Y } или interface I { }.
                            if current.kind == "enum" and inline_body:
                                current.members.append(Member("enum-value", inline_body.replace(" ", "").replace(",", ", "),
                                                              "", "", "public", True, False, False, "", inline_body))
                            current = None
                            type_depth = None
                    doc, attributes, tooltip = [], [], None
                elif current is not None and depth == type_depth and not m:
                    if stripped not in ("{", "}") and not stripped.startswith(("case ", "default:")):
                        pending_decl = stripped
                        decl_line = i - 1
                elif m:
                    doc, attributes, tooltip = [], [], None

        # Сбор многострочного объявления члена.
        if pending_decl is not None:
            decl = pending_decl if pending_decl == stripped else pending_decl + " " + stripped
            complete = (current.kind == "enum") or decl.rstrip().endswith((";", "{", ",", "}")) or "=>" in decl \
                or (i < len(lines) and lines[i].strip() == "{") or decl.rstrip().endswith(")")
            if not complete and i < len(lines):
                pending_decl = decl
                depth += code.count("{") - code.count("}")
                continue
            summary, params = _clean_xml_doc(doc)
            member = _parse_member(decl, current.kind, summary, params, tooltip, current.name)
            if member:
                member.line = decl_line
                current.members.append(member)
            doc, attributes, tooltip = [], [], None
            pending_decl = None

        depth += code.count("{") - code.count("}")
        if current is not None and depth >= type_depth:
            entered = True
        if current is not None and entered and depth < type_depth:
            current = None
            type_depth = None
        if stripped and not stripped.startswith("[") and pending_decl is None and depth > (type_depth or 1):
            doc = [] if not stripped.startswith("///") else doc

    return types


def load_all(root: str = SCRIPTS_ROOT) -> List[TypeInfo]:
    result = []
    for folder, _, files in os.walk(root):
        for name in sorted(files):
            if name.endswith(".cs"):
                result += parse_file(os.path.join(folder, name))
    return result


def is_documented_member(m: Member) -> bool:
    return m.visibility in ("public", "protected", "internal") or m.serialized


def visible_members(t: TypeInfo) -> List[Member]:
    """Члены, которые попадают в документацию и UML: публичные, protected и [SerializeField]."""
    return [m for m in t.members if is_documented_member(m) and m.name not in UNITY_MESSAGES]


if __name__ == "__main__":
    types = load_all()
    missing = 0
    for t in types:
        undocumented = [m for m in visible_members(t) if not m.summary and m.kind != "enum-value"]
        missing += len(undocumented)
        flag = "" if t.summary else "  [нет summary у типа]"
        print(f"{t.namespace}.{t.name} ({t.kind}) members={len(visible_members(t))} undocumented={len(undocumented)}{flag}")
        for m in undocumented:
            print(f"      - {m.kind} {m.name}")
    print(f"\nтипов: {len(types)}, без комментария: {missing}")
