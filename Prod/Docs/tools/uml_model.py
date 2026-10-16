# -*- coding: utf-8 -*-
"""
Модель UML проекта: пакеты, вид каждого класса, его назначение, паттерн и связи между классами.
Поля и методы в диаграммах берутся прямо из кода (csharp_api.py), здесь их писать не нужно.

Добавили класс в код → добавьте C(...) и связи R(...) сюда и запустите (из папки Prod):
    python3 Docs/tools/generate_uml.py
Генератор сам проверит, что все классы из кода описаны здесь.

Стереотипы: interface, abstract, static, struct, enum, MonoBehaviour, ScriptableObject, Editor, class
Связи R(from, to, kind, label) — label пока используется только в API.md:
    inherit   — наследование (from extends to)
    implement — реализация интерфейса
    compose   — владеет/создаёт (from содержит to)
    assoc     — хранит ссылку / использует постоянно
    dep       — зависимость (вызывает, создаёт, публикует)
"""

PACKAGES = [
    # (имя пакета, namespace, описание)
    ("Core", "Game.Core", "Инфраструктура: сервисы, события, конечный автомат, пулы"),
    ("Inputs", "Game.Inputs", "Абстракция ввода поверх Unity Input System"),
    ("Character", "Game.Character", "Игрок: фасад, мотор, бой, конфиг движения"),
    ("Character.States", "Game.Character.States", "Состояния движения игрока (State)"),
    ("Cameras", "Game.Cameras", "Камера от третьего лица"),
    ("Animations", "Game.Animations", "Связка геймплея с Animator и клипами Mixamo"),
    ("Combat", "Game.Combat", "Урон, здоровье, снаряды, эффекты"),
    ("Combat.Weapons", "Game.Combat.Weapons", "Оружие: данные, стратегии, фабрика, инвентарь"),
    ("AI", "Game.AI", "Враги и мишени"),
    ("Interaction", "Game.Interaction", "Подбираемые предметы и объекты уровня"),
    ("Gameplay", "Game.Gameplay", "Правила игры: появление игрока"),
    ("UI", "Game.UI", "HUD (MVP) и пауза"),
    ("Demo", "Game.Demo", "Демо-контент из примитивов: фабрики, уровень, сцена Playground (рантайм)"),
    ("EditorTools", "Game.EditorTools", "Генератор демо: манекен, аниматор, префабы, сцена (только редактор)"),
]

CLASSES = []
RELATIONS = []

# Зачем нужен каждый паттерн в этом проекте (для таблицы в UML.md).
PATTERN_NOTES = {
    "Service Locator": "Компоненты находят сервисы (камера, ввод, пул, игрок) по интерфейсу, а не через FindObjectOfType и жёсткие ссылки.",
    "Observer": "События (урон, смерть, появление игрока, пауза) — подписчики не связаны с издателем: HUD не знает про турель.",
    "State": "Каждое состояние движения/ИИ — отдельный класс, переходы описаны таблицей условий в одном месте.",
    "Object Pool": "Пули, трассеры, эффекты переиспользуются вместо Instantiate/Destroy — меньше мусора и фризов.",
    "Composition Root": "Глобальные сервисы создаются в одном месте до загрузки сцены.",
    "Adapter": "Внешние API (Input System, Animator) спрятаны за своими интерфейсами — их можно заменить, не трогая геймплей.",
    "Data-driven config": "Цифры баланса живут в ассетах — правятся в инспекторе без перекомпиляции.",
    "Facade": "Один простой вход в сложную подсистему (игрок для других систем, меню для генератора).",
    "Template Method": "Базовый класс фиксирует порядок шагов, наследник меняет только свой шаг.",
    "Proxy": "Хитбокс подставляется вместо Health и меняет урон по пути.",
    "Flyweight": "Общие неизменяемые данные оружия в одном ассете на всех владельцев.",
    "Strategy": "Поведение оружия собирается из взаимозаменяемых частей: режим огня × тип выстрела.",
    "Factory": "Создание сложного объекта (оружие = визуал + стратегии) спрятано в одном месте.",
    "MVP": "Логика HUD (Presenter) отделена от отрисовки (View) — IMGUI можно заменить на uGUI.",
    "Builder": "Пошаговая сборка сложных объектов: Animator Controller, манекен со скелетом, уровень.",
}


def C(name, package, stereotype, description, pattern=None):
    CLASSES.append(dict(name=name, package=package, stereotype=stereotype, description=description, pattern=pattern))


def R(source, target, kind, label=""):
    RELATIONS.append(dict(source=source, target=target, kind=kind, label=label))


# ============================== Core ==============================
C("ServiceLocator", "Core", "static", "Регистрация и поиск глобальных сервисов по типу (обычно интерфейсу).", pattern="Service Locator")
C("EventBus", "Core", "static", "Глобальная типизированная шина событий: издатель не знает подписчиков.", pattern="Observer")
C("IGameEvent", "Core", "interface", "Маркер событий для EventBus.")
C("DamageDealtEvent", "Core", "struct", "Кто-то получил урон (для хит-маркера, статистики).")
C("PlayerSpawnedEvent", "Core", "struct", "Игрок появился/возродился.")
C("PlayerDiedEvent", "Core", "struct", "Игрок погиб, возрождение через RespawnDelay.")
C("GamePausedEvent", "Core", "struct", "Пауза включена/выключена.")
C("ExplosionEvent", "Core", "struct", "Взрыв (для тряски камеры, звука).")
C("IState", "Core", "interface", "Состояние конечного автомата.")
C("StateMachine", "Core", "class", "Универсальный конечный автомат с таблицей переходов-условий. Используется игроком и ИИ.", pattern="State")
C("IPoolable", "Core", "interface", "Уведомления о выдаче/возврате объекта в пул.")
C("GameObjectPool", "Core", "class", "Пул экземпляров одного префаба (Stack неактивных объектов).", pattern="Object Pool")
C("PooledObject", "Core", "MonoBehaviour", "Ссылка экземпляра на свой пул; возврат в пул сразу или по таймеру.")
C("PoolService", "Core", "class", "Пулы по префабам. Глобальный сервис, живёт между сценами.")
C("CoreServices", "Core", "static", "Composition Root: до загрузки сцены создаёт и регистрирует глобальные сервисы.", pattern="Composition Root")
C("PhysicsQuery", "Core", "static", "Рейкасты/сферокасты, игнорирующие коллайдеры владельца; разброс в конусе.")

R("DamageDealtEvent", "IGameEvent", "implement")
R("PlayerSpawnedEvent", "IGameEvent", "implement")
R("PlayerDiedEvent", "IGameEvent", "implement")
R("GamePausedEvent", "IGameEvent", "implement")
R("ExplosionEvent", "IGameEvent", "implement")
R("EventBus", "IGameEvent", "dep", "публикует")
R("StateMachine", "IState", "assoc", "current / transitions")
R("PoolService", "GameObjectPool", "compose", "по пулу на префаб")
R("GameObjectPool", "PooledObject", "compose", "хранит")
R("PooledObject", "IPoolable", "dep", "уведомляет")
R("CoreServices", "PoolService", "dep", "создаёт")
R("CoreServices", "ServiceLocator", "dep", "регистрирует")

# ============================== Inputs ==============================
C("IPlayerInput", "Inputs", "interface", "Абстракция ввода: персонаж, камера и оружие не зависят от Input System.")
C("PlayerInputReader", "Inputs", "MonoBehaviour", "Адаптер Unity Input System (InputActionAsset) к IPlayerInput.", pattern="Adapter")
C("InputActionNames", "Inputs", "static", "Имена карты и действий из InputSystem_Actions — единственное место со строками.")
R("PlayerInputReader", "IPlayerInput", "implement")
R("PlayerInputReader", "InputActionNames", "dep")
R("PlayerInputReader", "ServiceLocator", "dep", "регистрирует себя")

# ============================== Character ==============================
C("MovementConfig", "Character", "ScriptableObject", "Настройки передвижения: скорости, прыжок, присед, подкат.", pattern="Data-driven config")
C("CharacterMotor", "Character", "MonoBehaviour", "Перемещение поверх CharacterController: скорость, гравитация, прыжок, рост капсулы.")
C("PlayerCharacter", "Character", "MonoBehaviour", "Фасад игрока: объединяет мотор, здоровье, бой, ввод; владеет автоматом состояний.", pattern="Facade")
C("PlayerCombat", "Character", "MonoBehaviour", "Прицеливание, стрельба, перезарядка, смена оружия, отдача камеры.")
R("PlayerCharacter", "CharacterMotor", "assoc", "Motor")
R("PlayerCharacter", "MovementConfig", "assoc", "config")
R("PlayerCharacter", "StateMachine", "compose", "автомат движения")
R("PlayerCharacter", "IPlayerInput", "assoc", "Input")
R("PlayerCharacter", "Health", "assoc")
R("PlayerCharacter", "PlayerCombat", "assoc", "Combat")
R("PlayerCharacter", "ICameraTarget", "implement")
R("PlayerCharacter", "ICameraRig", "dep", "движение от камеры")
R("PlayerCombat", "IAimProvider", "implement")
R("PlayerCombat", "WeaponInventory", "assoc")
R("PlayerCombat", "ICameraRig", "dep", "AimPoint, отдача")
R("PlayerCombat", "AimData", "dep")

# ============================== Character.States ==============================
C("CharacterState", "Character.States", "abstract", "База состояний: Tick() = OnTick() + Motor.Simulate().", pattern="Template Method")
C("GroundedState", "Character.States", "class", "На земле: стоим, идём при прицеливании, бежим, спринтуем.")
C("CrouchState", "Character.States", "class", "Присед: низкая капсула, медленное движение.")
C("SlideState", "Character.States", "class", "Подкат: рывок с затуханием скорости и низкой капсулой.")
C("AirborneState", "Character.States", "class", "Прыжок/падение, двойной прыжок, управление в воздухе.")
C("DeadState", "Character.States", "class", "Смерть: персонаж останавливается, ввод игнорируется.")
R("CharacterState", "IState", "implement")
for s in ("GroundedState", "CrouchState", "SlideState", "AirborneState", "DeadState"):
    R(s, "CharacterState", "inherit")
R("CharacterState", "PlayerCharacter", "assoc", "Player")

# ============================== Cameras ==============================
C("ICameraTarget", "Cameras", "interface", "То, за чем следует камера.")
C("ICameraRig", "Cameras", "interface", "Сервис камеры для геймплея.")
C("CameraConfig", "Cameras", "ScriptableObject", "Чувствительность, дистанции, плечо, FOV, коллизии, отдача.")
C("ThirdPersonCamera", "Cameras", "MonoBehaviour", "Камера через плечо: орбита, прицел-зум, смена плеча, отдача, коллизии.")
R("ThirdPersonCamera", "ICameraRig", "implement")
R("ThirdPersonCamera", "CameraConfig", "assoc", "config")
R("ThirdPersonCamera", "ICameraTarget", "assoc", "target")
R("ThirdPersonCamera", "IPlayerInput", "dep", "Look, SwitchShoulder")
R("ThirdPersonCamera", "PhysicsQuery", "dep")

# ============================== Animations ==============================
C("AnimatorParams", "Animations", "static", "Имена/хэши параметров и слоёв Animator — общие для генератора и рантайма.")
C("AnimationSlot", "Animations", "enum", "Слоты анимаций: Idle, Walk*, Run*, Sprint, Crouch*, Slide, JumpStart, Fall, Death, AimIdle, Fire, Reload, HitReact.")
C("AnimationSet", "Animations", "ScriptableObject", "Набор клипов Mixamo + ссылка на сгенерированный контроллер. Пустые слоты берут запасной клип.")
C("CharacterAnimator", "Animations", "MonoBehaviour", "Переводит состояние игрока и его события в параметры Animator; запасные процедурные позы.", pattern="Adapter")
C("AimIKHandler", "Animations", "MonoBehaviour", "Humanoid IK взгляда: голова и корпус смотрят в точку прицела.")
R("AnimationSet", "AnimationSlot", "dep")
R("CharacterAnimator", "AnimationSet", "assoc")
R("CharacterAnimator", "PlayerCharacter", "assoc", "читает состояние")
R("CharacterAnimator", "AnimatorParams", "dep")
R("AimIKHandler", "IAimProvider", "assoc")

# ============================== Combat ==============================
C("DamageInfo", "Combat", "struct", "Описание попадания.")
C("IDamageable", "Combat", "interface", "Всё, что можно повредить.")
C("Health", "Combat", "MonoBehaviour", "Здоровье с событиями и регенерацией; публикует DamageDealtEvent.", pattern="Observer")
C("Hitbox", "Combat", "MonoBehaviour", "Хитбокс (голова ×2): умножает урон и передаёт его в Health родителя.", pattern="Proxy")
C("Projectile", "Combat", "MonoBehaviour", "Снаряд из пула: летит по дуге, проверяет попадание лучом, взрывается.")
C("Explosion", "Combat", "static", "Урон по площади с затуханием и толчок физики.")
C("PooledEffect", "Combat", "MonoBehaviour", "Простой эффект из пула: масштаб и свет по времени жизни.")
C("Tracer", "Combat", "MonoBehaviour", "Трассер пули на LineRenderer.")
R("Health", "IDamageable", "implement")
R("Hitbox", "IDamageable", "implement")
R("Hitbox", "Health", "assoc", "передаёт урон")
R("Health", "DamageInfo", "dep")
R("Health", "EventBus", "dep", "DamageDealtEvent")
R("Projectile", "IPoolable", "implement")
R("PooledEffect", "IPoolable", "implement")
R("Tracer", "IPoolable", "implement")
R("Projectile", "Explosion", "dep")
R("Explosion", "IDamageable", "dep")

# ============================== Combat.Weapons ==============================
C("FireModeType", "Combat.Weapons", "enum", "SemiAuto, FullAuto, Burst")
C("ShotType", "Combat.Weapons", "enum", "Hitscan, Spread, Projectile")
C("WeaponDefinition", "Combat.Weapons", "ScriptableObject", "Данные оружия: урон, темп, патроны, разброс, отдача, эффекты. Новое оружие = новый ассет.", pattern="Flyweight")
C("AimData", "Combat.Weapons", "struct", "Куда стрелять в этом кадре.")
C("ShotContext", "Combat.Weapons", "struct", "Контекст одного выстрела для стратегии.")
C("IFireMode", "Combat.Weapons", "interface", "КОГДА стрелять.", pattern="Strategy")
C("SemiAutoFireMode", "Combat.Weapons", "class", "Выстрел на нажатие (с буфером нажатия).")
C("FullAutoFireMode", "Combat.Weapons", "class", "Стрельба, пока зажат курок.")
C("BurstFireMode", "Combat.Weapons", "class", "Очередь из N выстрелов на нажатие.")
C("IShotStrategy", "Combat.Weapons", "interface", "ЧТО происходит при выстреле.", pattern="Strategy")
C("HitscanShot", "Combat.Weapons", "class", "Мгновенный луч.")
C("SpreadShot", "Combat.Weapons", "class", "Дробь: несколько лучей.")
C("ProjectileShot", "Combat.Weapons", "class", "Снаряд из пула.")
C("ShotUtility", "Combat.Weapons", "static", "Общий код луча: урон, сила удара, искры, трассер.")
C("Weapon", "Combat.Weapons", "class", "Экземпляр оружия: патроны, перезарядка, кулдаун, разброс. Поведение — в стратегиях.")
C("WeaponView", "Combat.Weapons", "MonoBehaviour", "Визуал оружия: модель, дуло, вспышка, звук.")
C("WeaponFactory", "Combat.Weapons", "static", "Собирает Weapon из WeaponDefinition: визуал + стратегии по enum-ам.", pattern="Factory")
C("WeaponInventory", "Combat.Weapons", "MonoBehaviour", "Слоты оружия: стартовое оружие, переключение, патроны.")
C("IAimProvider", "Combat.Weapons", "interface", "Источник точки прицела для визуала.")
C("WeaponSocket", "Combat.Weapons", "MonoBehaviour", "Крепление оружия: в бою ствол смотрит в прицел, вне боя — походное положение.")
R("WeaponDefinition", "FireModeType", "dep")
R("WeaponDefinition", "ShotType", "dep")
for s in ("SemiAutoFireMode", "FullAutoFireMode", "BurstFireMode"):
    R(s, "IFireMode", "implement")
for s in ("HitscanShot", "SpreadShot", "ProjectileShot"):
    R(s, "IShotStrategy", "implement")
R("HitscanShot", "ShotUtility", "dep")
R("SpreadShot", "ShotUtility", "dep")
R("ProjectileShot", "Projectile", "dep", "спавнит")
R("ShotUtility", "IDamageable", "dep")
R("ShotUtility", "Tracer", "dep")
R("Weapon", "WeaponDefinition", "assoc", "Definition")
R("Weapon", "WeaponView", "assoc", "View")
R("Weapon", "IFireMode", "compose")
R("Weapon", "IShotStrategy", "assoc")
R("Weapon", "ShotContext", "dep")
R("WeaponFactory", "Weapon", "dep", "создаёт")
R("WeaponInventory", "Weapon", "compose", "0..4")
R("WeaponInventory", "WeaponFactory", "dep")
R("WeaponSocket", "IAimProvider", "assoc")

# ============================== AI ==============================
C("EnemyTurret", "AI", "MonoBehaviour", "Турель: ищет игрока, доворачивается и стреляет очередями. Переиспользует StateMachine и Weapon.")
C("TurretIdleState", "AI", "class", "Нет цели — сканирование.")
C("TurretAttackState", "AI", "class", "Цель видна — наведение и очереди.")
C("TurretDeadState", "AI", "class", "Уничтожена — скрыта, затем восстанавливается.")
C("TargetDummy", "AI", "MonoBehaviour", "Мишень: вспышка при попадании, исчезает и возрождается.")
R("EnemyTurret", "StateMachine", "compose")
R("EnemyTurret", "Weapon", "compose")
R("EnemyTurret", "WeaponFactory", "dep")
R("EnemyTurret", "Health", "assoc")
R("EnemyTurret", "PlayerCharacter", "dep", "цель")
for s in ("TurretIdleState", "TurretAttackState", "TurretDeadState"):
    R(s, "IState", "implement")
    R(s, "EnemyTurret", "assoc")
R("TargetDummy", "Health", "assoc")

# ============================== Interaction ==============================
C("Pickup", "Interaction", "abstract", "Подбираемое: триггер → TryApply → скрыть → появиться снова.", pattern="Template Method")
C("HealthPickup", "Interaction", "MonoBehaviour", "Аптечка.")
C("AmmoPickup", "Interaction", "MonoBehaviour", "Патроны всему оружию.")
C("JumpPad", "Interaction", "MonoBehaviour", "Батут: подбрасывает игрока и восстанавливает прыжки.")
R("HealthPickup", "Pickup", "inherit")
R("AmmoPickup", "Pickup", "inherit")
R("Pickup", "PlayerCharacter", "dep")
R("JumpPad", "PlayerCharacter", "dep", "Launch()")

# ============================== Gameplay ==============================
C("PlayerSpawner", "Gameplay", "MonoBehaviour", "Появление и возрождение игрока, события PlayerSpawned/PlayerDied.")
R("PlayerSpawner", "PlayerCharacter", "assoc")
R("PlayerSpawner", "EventBus", "dep", "публикует")
R("PlayerSpawner", "ICameraRig", "dep", "SetTarget")

# ============================== UI ==============================
C("IHudView", "UI", "interface", "View (MVP): только отображение.")
C("HudPresenter", "UI", "MonoBehaviour", "Presenter (MVP): слушает Health/Weapon/EventBus и обновляет IHudView.", pattern="MVP")
C("ImGuiHudView", "UI", "MonoBehaviour", "HUD на IMGUI: прицел, хит-маркер, HP, патроны, подсказки.")
C("PauseMenu", "UI", "MonoBehaviour", "Пауза: timeScale, курсор, отключение ввода, GamePausedEvent.")
R("ImGuiHudView", "IHudView", "implement")
R("HudPresenter", "IHudView", "assoc", "view")
R("HudPresenter", "PlayerCharacter", "assoc", "model")
R("HudPresenter", "EventBus", "dep", "подписка")
R("PauseMenu", "IPlayerInput", "dep")
R("PauseMenu", "EventBus", "dep", "публикует")

# ============================== Demo ==============================
C("IMaterialProvider", "Demo", "interface", "Источник материалов для примитивов: ассеты (редактор) или память (игра).", pattern="Strategy")
C("MaterialFactory", "Demo", "static", "Создание материалов URP Lit/Unlit и текстуры-шахматки.")
C("RuntimeMaterialProvider", "Demo", "class", "Материалы в памяти с кэшем по имени (сцена Playground и её превью).")
C("Primitives", "Demo", "static", "Создание примитивов с коллайдером/без и неактивных объектов для настройки до Awake.")
C("MannequinBuilder", "Demo", "class", "Манекен из примитивов на скелете Mixamo (mixamorig:*) + Humanoid Avatar через AvatarBuilder.", pattern="Builder")
C("DemoWeapons", "Demo", "class", "Набор демо-оружия.")
C("DemoWeaponCatalog", "Demo", "static", "Цифры баланса и размеры моделей демо-оружия — общие для генератора и Playground.")
C("DemoPrefabs", "Demo", "class", "Шаблоны для расстановки: префабы (редактор) или неактивные образцы (игра).")
C("DemoContentFactory", "Demo", "static", "Строит из примитивов игрока, мишень, турель, предметы, модели оружия и эффекты.", pattern="Factory")
C("DemoLevelBuilder", "Demo", "class", "Строит полигон: тир, укрытия, тоннель, лестница, рампа, паркур, батут, турель, ящики; камера и системы сцены.", pattern="Builder")
C("PlaygroundBootstrap", "Demo", "MonoBehaviour", "Сцена Playground из коробки: собирает полигон и игрока в игре; в редакторе — несохраняемое превью.", pattern="Composition Root")
R("RuntimeMaterialProvider", "IMaterialProvider", "implement")
R("RuntimeMaterialProvider", "MaterialFactory", "dep")
R("DemoContentFactory", "IMaterialProvider", "dep")
R("DemoContentFactory", "MannequinBuilder", "dep")
R("DemoContentFactory", "Primitives", "dep")
R("DemoContentFactory", "DemoWeaponCatalog", "dep")
R("DemoContentFactory", "PlayerCharacter", "dep", "собирает")
R("DemoContentFactory", "EnemyTurret", "dep", "собирает")
R("DemoWeaponCatalog", "DemoWeapons", "dep", "создаёт")
R("DemoWeaponCatalog", "WeaponDefinition", "dep", "настраивает")
R("DemoLevelBuilder", "DemoPrefabs", "assoc")
R("DemoLevelBuilder", "IMaterialProvider", "assoc")
R("DemoLevelBuilder", "PlayerSpawner", "dep", "BuildSystems")
R("DemoLevelBuilder", "ThirdPersonCamera", "dep", "SetupCamera")
R("PlaygroundBootstrap", "DemoContentFactory", "dep")
R("PlaygroundBootstrap", "DemoLevelBuilder", "dep")
R("PlaygroundBootstrap", "RuntimeMaterialProvider", "compose")
R("PlaygroundBootstrap", "AnimationSet", "assoc")

# ============================== EditorTools ==============================
C("TpsSkeletonMenu", "EditorTools", "static", "Меню Tools → TPS Skeleton: вся генерация одной кнопкой.", pattern="Facade")
C("CoreAssetsBuilder", "EditorTools", "static", "Конфиги, AnimationSet (с клипами из Assets/Models), Animator Controller.")
C("AnimatorControllerBuilder", "EditorTools", "class", "Собирает Animator Controller из AnimationSet: blend trees, слои, переходы.", pattern="Builder")
C("WeaponContentBuilder", "EditorTools", "static", "Сохраняет в ассеты эффекты, модели оружия и WeaponDefinition-ы из DemoContentFactory.")
C("PrefabContentBuilder", "EditorTools", "static", "Сохраняет в префабы игрока, мишень, турель, предметы; Avatar манекена — ассетом.")
C("DemoSceneBuilder", "EditorTools", "static", "Сцена Demo из префабов; раскладка — тот же DemoLevelBuilder.")
C("AnimationSetEditor", "EditorTools", "Editor", "Инспектор AnimationSet: подсказки, отчёт о заменах, кнопка Rebuild.")
C("AssetMaterialProvider", "EditorTools", "class", "IMaterialProvider для генератора: материалы сохраняются ассетами .mat.")
C("PlaygroundSceneOpener", "EditorTools", "static", "При старте редактора открывает сцену Playground вместо пустой безымянной; меню Open Playground Scene.")
C("EditorAssetUtility", "EditorTools", "static", "Папки, ScriptableObject, префабы, примитивы.")
C("SkeletonPaths", "EditorTools", "static", "Пути генерируемых ассетов (Assets/Game/Generated).")
R("TpsSkeletonMenu", "CoreAssetsBuilder", "dep")
R("TpsSkeletonMenu", "WeaponContentBuilder", "dep")
R("TpsSkeletonMenu", "PrefabContentBuilder", "dep")
R("TpsSkeletonMenu", "DemoSceneBuilder", "dep")
R("CoreAssetsBuilder", "AnimatorControllerBuilder", "dep")
R("AnimatorControllerBuilder", "AnimationSet", "dep", "читает клипы")
R("PrefabContentBuilder", "DemoContentFactory", "dep")
R("WeaponContentBuilder", "DemoContentFactory", "dep")
R("DemoSceneBuilder", "DemoLevelBuilder", "dep")
R("AssetMaterialProvider", "IMaterialProvider", "implement")
R("PrefabContentBuilder", "AssetMaterialProvider", "dep")
R("AnimationSetEditor", "AnimatorControllerBuilder", "dep")
R("WeaponContentBuilder", "WeaponDefinition", "dep", "создаёт ассеты")
R("PlaygroundSceneOpener", "PlaygroundBootstrap", "dep", "открывает сцену")
