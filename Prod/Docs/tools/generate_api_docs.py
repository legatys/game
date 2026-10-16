# -*- coding: utf-8 -*-
"""
Генератор справочника API (Docs/API.md) из исходников C# и их XML-комментариев.

    cd Prod && python3 Docs/tools/generate_api_docs.py

Описания берутся из `/// <summary>` в коде (для полей инспектора — ещё и из [Tooltip]),
пакеты, паттерны и связи — из Docs/tools/uml_model.py. Не правьте API.md вручную.
"""
import os
import re
import sys
from collections import OrderedDict

HERE = os.path.dirname(os.path.abspath(__file__))
sys.path.insert(0, HERE)
import csharp_api  # noqa: E402
import uml_model as model  # noqa: E402

PROD = os.path.abspath(os.path.join(HERE, "..", ".."))
OUTPUT = os.path.join(PROD, "Docs", "API.md")

KIND_RU = {"class": "класс", "interface": "интерфейс", "struct": "структура", "enum": "перечисление"}
RELATION_RU = {"inherit": "наследует", "implement": "реализует", "compose": "владеет", "assoc": "ссылается на",
               "dep": "использует"}
SECTIONS = [
    ("Настройки инспектора", lambda m: m.serialized and m.visibility == "private"),
    ("Значения", lambda m: m.kind == "enum-value"),
    ("Константы", lambda m: m.kind == "const"),
    ("Поля", lambda m: m.kind == "field" and not (m.serialized and m.visibility == "private")),
    ("Свойства", lambda m: m.kind == "property"),
    ("События", lambda m: m.kind == "event"),
    ("Конструкторы", lambda m: m.kind == "constructor"),
    ("Методы", lambda m: m.kind == "method"),
]


def md(text):
    return text.replace("|", "\\|").replace("\n", " ")


def anchor(name):
    return name.lower()


class Docs:
    def __init__(self):
        self.types = csharp_api.load_all()
        self.by_name = {t.name: t for t in self.types}
        self.classes = OrderedDict((c["name"], c) for c in model.CLASSES)

    def resolve_summary(self, t, member, depth=0):
        """<inheritdoc/> → описание того же члена у базового класса или интерфейса."""
        if member.summary != csharp_api.INHERIT or depth > 5:
            return member.summary
        for base in t.bases:
            base_type = self.by_name.get(base.split("<")[0])
            if not base_type:
                continue
            for m in base_type.members:
                if m.name == member.name and m.kind == member.kind:
                    return self.resolve_summary(base_type, m, depth + 1)
        return "Реализация члена базового типа."

    def link_types(self, text):
        """Имена типов проекта в сигнатурах превращаются в ссылки на их раздел."""
        def repl(match):
            word = match.group(0)
            return f"[{word}](#{anchor(word)})" if word in self.by_name else word
        return re.sub(r"\b[A-Z]\w+\b", repl, text)

    def member_rows(self, t, members):
        rows = []
        for m in members:
            summary = md(self.resolve_summary(t, m))
            if m.kind == "enum-value":
                rows.append(f"| `{md(m.name)}` | {summary or '—'} |")
                continue
            signature = md(re.sub(r"\s+", " ", m.signature))
            if m.param_docs:
                params = "; ".join(f"`{name}` — {md(text)}" for name, text in m.param_docs.items())
                summary = f"{summary}<br>Параметры: {params}"
            rows.append(f"| `{signature}` | {summary} |")
        return rows

    def type_section(self, t):
        info = self.classes.get(t.name, {})
        out = [f"### {t.name}", ""]
        rel_path = os.path.relpath(t.file, os.path.join(PROD, "Docs")).replace("\\", "/")
        kind = KIND_RU.get(t.kind, t.kind)
        mods = [m for m in t.modifiers if m in ("static", "abstract", "sealed")]
        head = f"`{t.namespace}` · {' '.join(mods + [kind])} · [{os.path.basename(t.file)}]({rel_path})"
        if info.get("pattern"):
            head += f" · паттерн **{info['pattern']}**"
        out.append(head)
        out.append("")
        if t.bases:
            out.append("Базовые типы: " + ", ".join(self.link_types(f"`{b}`").replace("`[", "[").replace(")`", ")")
                                                     if b in self.by_name else f"`{b}`" for b in t.bases))
            out.append("")
        attrs = [a for a in t.attributes if not a.startswith(("[Header", "[Tooltip"))]
        if attrs:
            out.append("Атрибуты: " + ", ".join(f"`{a}`" for a in attrs))
            out.append("")
        out.append(t.summary or info.get("description", ""))
        out.append("")

        relations = [r for r in model.RELATIONS if r["source"] == t.name]
        incoming = [r for r in model.RELATIONS if r["target"] == t.name]
        if relations:
            parts = [f"{RELATION_RU[r['kind']]} [{r['target']}](#{anchor(r['target'])})"
                     + (f" ({r['label']})" if r["label"] else "") for r in relations]
            out.append("**Связи:** " + "; ".join(parts) + ".")
            out.append("")
        if incoming:
            users = sorted({r["source"] for r in incoming})
            out.append("**Используется в:** " + ", ".join(f"[{u}](#{anchor(u)})" for u in users) + ".")
            out.append("")

        members = csharp_api.visible_members(t)
        for title, predicate in SECTIONS:
            selected = [m for m in members if predicate(m)]
            if not selected:
                continue
            out.append(f"**{title}**")
            out.append("")
            out.append("| Член | Описание |" if title != "Значения" else "| Значение | Описание |")
            out.append("|---|---|")
            out += self.member_rows(t, selected)
            out.append("")
        return out

    def render(self):
        out = ["# Справочник API", "",
               "> Сгенерировано `Docs/tools/generate_api_docs.py` из исходников и XML-комментариев (`///`). "
               "Не правьте вручную: допишите комментарий в коде и перезапустите генератор.", "",
               "Здесь описан каждый публичный тип и член, а также поля, настраиваемые в инспекторе (`[SerializeField]`). "
               "Как всё связано и работает вместе — в [ARCHITECTURE.md](ARCHITECTURE.md), диаграммы — в [UML.md](UML.md).", ""]

        packages = OrderedDict()
        for name, namespace, description in model.PACKAGES:
            packages[name] = dict(namespace=namespace, description=description, types=[])
        for c in self.classes.values():
            if c["name"] in self.by_name:
                packages[c["package"]]["types"].append(self.by_name[c["name"]])

        out.append("## Содержание")
        out.append("")
        for name, package in packages.items():
            links = ", ".join(f"[{t.name}](#{anchor(t.name)})" for t in package["types"])
            out.append(f"- **[{name}](#пакет-{anchor(name).replace('.', '')})** — {package['description']}: {links}")
        out.append("")

        total_members = 0
        for name, package in packages.items():
            out.append(f"## Пакет {name}")
            out.append("")
            out.append(f"`{package['namespace']}` — {package['description']}.")
            out.append("")
            for t in package["types"]:
                out += self.type_section(t)
                total_members += len(csharp_api.visible_members(t))
        out.append(f"---\nТипов: {len(self.types)}, задокументированных членов: {total_members}.\n")
        return "\n".join(out)


def main():
    docs = Docs()
    undocumented = [f"{t.name}.{m.name}" for t in docs.types for m in csharp_api.visible_members(t)
                    if not m.summary and m.kind != "enum-value"]
    with open(OUTPUT, "w", encoding="utf-8") as f:
        f.write(docs.render())
    print(f"OK -> {os.path.relpath(OUTPUT, PROD)}: {len(docs.types)} типов")
    if undocumented:
        print(f"Без XML-комментария ({len(undocumented)}): " + ", ".join(undocumented))


if __name__ == "__main__":
    main()
