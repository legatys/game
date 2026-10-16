# Правила проекта (Unity TPS Skeleton)

Проект Unity лежит в `Prod/`. Код — `Prod/Assets/Game/Scripts/{Runtime,Editor}` (asmdef `Game.Runtime` / `Game.Editor`).
Документация: `Prod/Docs/README.md`, UML: `Prod/Docs/UML.md` + `Prod/Prod_di.drawio`.

## Обязательно
- **После добавления/изменения класса обновить UML**: добавить `C(...)` и связи `R(...)` в `Prod/Docs/uml/uml_model.py`,
  затем `cd Prod && python3 Docs/uml/generate_uml.py`. `UML.md` и `Prod_di.drawio` руками не править.
- Писать по паттернам, уже принятым в проекте (State, Strategy, Factory, Observer/EventBus, Service Locator, Adapter, MVP, Builder),
  и указывать паттерн в `<summary>` класса и в поле `pattern` модели UML.
- Один MonoBehaviour/ScriptableObject = один файл с тем же именем (иначе Unity не сериализует компонент).
- Для каждого нового файла/папки в `Assets` создавать `.meta` со случайным GUID.
- Пространства имён `Game.*`; не называть namespace как типы Unity (`Game.Cameras`, `Game.Inputs`, `Game.Animations` — уже так).
- Геймплей зависит от интерфейсов (`IPlayerInput`, `ICameraRig`, `IDamageable`, `IHudView`), а не от Input System/Animator напрямую.
- Числа баланса — в ScriptableObject-конфигах, не хардкодом.
- Комментарии и документация — на русском, идентификаторы — на английском.
- Модели — только примитивы; анимации — Humanoid-клипы Mixamo через `AnimationSet`.
