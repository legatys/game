# Правила проекта (Unity TPS Skeleton)

Проект Unity лежит в `Prod/`. Код — `Prod/Assets/Game/Scripts/{Runtime,Editor}` (asmdef `Game.Runtime` / `Game.Editor`).
Документация: `Prod/Docs/README.md` (старт), `Prod/Docs/ARCHITECTURE.md` (устройство), `Prod/Docs/API.md` (справочник, генерируется),
UML: `Prod/Docs/UML.md` + `Prod/Docs/uml/*.drawio.svg` + `Prod/Prod_di.drawio` (генерируются).

## Обязательно
- **После добавления/изменения класса обновить документацию и UML**:
  1. у каждого нового публичного члена и `[SerializeField]`-поля — `/// <summary>` (или `[Tooltip]`), у реализаций интерфейса — `/// <inheritdoc />`;
  2. новый класс — строка `C(...)` и связи `R(...)` в `Prod/Docs/tools/uml_model.py`;
  3. `cd Prod && python3 Docs/tools/generate_api_docs.py && python3 Docs/tools/generate_uml.py` (нужен graphviz `dot`);
  4. если меняется поведение/потоки — поправить `Prod/Docs/ARCHITECTURE.md`.
  `API.md`, `UML.md`, `uml/*.drawio.svg`, `Prod_di.drawio` руками не править.
- Писать по паттернам, уже принятым в проекте (State, Strategy, Factory, Observer/EventBus, Service Locator, Adapter, MVP, Builder),
  и указывать паттерн в `<summary>` класса и в поле `pattern` модели UML.
- Один MonoBehaviour/ScriptableObject = один файл с тем же именем (иначе Unity не сериализует компонент).
- Для каждого нового файла/папки в `Assets` создавать `.meta` со случайным GUID.
- Пространства имён `Game.*`; не называть namespace как типы Unity (`Game.Cameras`, `Game.Inputs`, `Game.Animations` — уже так).
- Геймплей зависит от интерфейсов (`IPlayerInput`, `ICameraRig`, `IDamageable`, `IHudView`), а не от Input System/Animator напрямую.
- Числа баланса — в ScriptableObject-конфигах, не хардкодом.
- Комментарии и документация — на русском, идентификаторы — на английском.
- Модели — только примитивы; анимации — Humanoid-клипы Mixamo через `AnimationSet`.
