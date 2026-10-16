# -*- coding: utf-8 -*-
"""
Модель UML проекта — единственный источник правды для диаграмм.

Добавили класс в код → добавьте C(...) и связи R(...) сюда и запустите:
    python3 Docs/uml/generate_uml.py
Скрипт перегенерирует Prod_di.drawio (draw.io) и Docs/UML.md (Mermaid, рендерится на GitHub).

Стереотипы: interface, abstract, static, struct, enum, MonoBehaviour, ScriptableObject, Editor, class
Связи R(from, to, kind, label):
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
    "Builder": "Пошаговая сборка сложных объектов: Animator Controller, манекен со скелетом, настройка полей.",
}


def C(name, package, stereotype, description, fields=(), methods=(), pattern=None):
    CLASSES.append(dict(name=name, package=package, stereotype=stereotype, description=description,
                        fields=list(fields), methods=list(methods), pattern=pattern))


def R(source, target, kind, label=""):
    RELATIONS.append(dict(source=source, target=target, kind=kind, label=label))


# ============================== Core ==============================
C("ServiceLocator", "Core", "static", "Регистрация и поиск глобальных сервисов по типу (обычно интерфейсу).",
  methods=["+Register~T~(service)", "+Unregister~T~(service)", "+Get~T~() T", "+TryGet~T~(out service) bool", "+Clear()"],
  pattern="Service Locator")
C("EventBus", "Core", "static", "Глобальная типизированная шина событий: издатель не знает подписчиков.",
  methods=["+Subscribe~T~(handler)", "+Unsubscribe~T~(handler)", "+Publish~T~(event)"], pattern="Observer")
C("IGameEvent", "Core", "interface", "Маркер событий для EventBus.")
C("DamageDealtEvent", "Core", "struct", "Кто-то получил урон (для хит-маркера, статистики).",
  fields=["+Info : DamageInfo", "+Target : Health", "+Killed : bool"])
C("PlayerSpawnedEvent", "Core", "struct", "Игрок появился/возродился.", fields=["+Player : PlayerCharacter"])
C("PlayerDiedEvent", "Core", "struct", "Игрок погиб, возрождение через RespawnDelay.",
  fields=["+Player : PlayerCharacter", "+RespawnDelay : float"])
C("GamePausedEvent", "Core", "struct", "Пауза включена/выключена.", fields=["+IsPaused : bool"])
C("ExplosionEvent", "Core", "struct", "Взрыв (для тряски камеры, звука).", fields=["+Position : Vector3", "+Radius : float"])
C("IState", "Core", "interface", "Состояние конечного автомата.",
  fields=["+Name : string"], methods=["+Enter()", "+Tick(dt)", "+Exit()"])
C("StateMachine", "Core", "class", "Универсальный конечный автомат с таблицей переходов-условий. Используется игроком и ИИ.",
  fields=["+Current : IState", "+Previous : IState", "+StateChanged : event"],
  methods=["+AddTransition(from, to, condition)", "+AddAnyTransition(to, condition)", "+SetState(state)", "+Tick(dt)"],
  pattern="State")
C("IPoolable", "Core", "interface", "Уведомления о выдаче/возврате объекта в пул.", methods=["+OnSpawned()", "+OnDespawned()"])
C("GameObjectPool", "Core", "class", "Пул экземпляров одного префаба (Stack неактивных объектов).",
  methods=["+Get(position, rotation) PooledObject", "+Release(item)", "+Prewarm(count)"], pattern="Object Pool")
C("PooledObject", "Core", "MonoBehaviour", "Ссылка экземпляра на свой пул; возврат в пул сразу или по таймеру.",
  methods=["+Release()", "+ReleaseAfter(seconds)"])
C("PoolService", "Core", "class", "Пулы по префабам. Глобальный сервис, живёт между сценами.",
  methods=["+Spawn(prefab, pos, rot) GameObject", "+Spawn~T~(prefab, pos, rot) T", "+Prewarm(prefab, count)", "+Release(instance)$"])
C("CoreServices", "Core", "static", "Composition Root: до загрузки сцены создаёт и регистрирует глобальные сервисы.",
  methods=["-Install() [BeforeSceneLoad]", "+Spawn(prefab, pos, rot) GameObject"], pattern="Composition Root")
C("PhysicsQuery", "Core", "static", "Рейкасты/сферокасты, игнорирующие коллайдеры владельца; разброс в конусе.",
  methods=["+Raycast(ray, dist, mask, ignoreRoot, out hit) bool", "+SphereCast(...) bool", "+RandomInCone(dir, angle) Vector3"])

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
C("IPlayerInput", "Inputs", "interface", "Абстракция ввода: персонаж, камера и оружие не зависят от Input System.",
  fields=["+Move, Look : Vector2", "+LookIsDelta : bool", "+JumpPressed, SprintHeld : bool", "+CrouchPressed, CrouchHeld : bool",
          "+FirePressed, FireHeld, AimHeld : bool", "+ReloadPressed, InteractPressed : bool",
          "+Next/PreviousWeaponPressed : bool", "+WeaponSlotPressed : int", "+SwitchShoulderPressed, PausePressed : bool",
          "+GameplayEnabled : bool"])
C("PlayerInputReader", "Inputs", "MonoBehaviour", "Адаптер Unity Input System (InputActionAsset) к IPlayerInput.",
  fields=["-actions : InputActionAsset"], methods=["-Awake() находит действия по InputActionNames"], pattern="Adapter")
C("InputActionNames", "Inputs", "static", "Имена карты и действий из InputSystem_Actions — единственное место со строками.",
  fields=["+PlayerMap, Move, Look, Jump, ... : const string", "+WeaponSlots : string[]"])
R("PlayerInputReader", "IPlayerInput", "implement")
R("PlayerInputReader", "InputActionNames", "dep")
R("PlayerInputReader", "ServiceLocator", "dep", "регистрирует себя")

# ============================== Character ==============================
C("MovementConfig", "Character", "ScriptableObject", "Настройки передвижения: скорости, прыжок, присед, подкат.",
  fields=["+walk/run/sprint/crouchSpeed : float", "+acceleration, deceleration, airAcceleration", "+rotationSpeed",
          "+gravity, jumpHeight, maxAirJumps", "+coyoteTime, jumpBufferTime", "+standHeight, crouchHeight",
          "+slideStartSpeed, slideFriction, slideMaxDuration, slideCooldown"], pattern="Data-driven config")
C("CharacterMotor", "Character", "MonoBehaviour", "Перемещение поверх CharacterController: скорость, гравитация, прыжок, рост капсулы.",
  fields=["+Velocity, ActualVelocity : Vector3", "+IsGrounded : bool", "+AirTime, Height : float", "+Landed : event"],
  methods=["+AccelerateTowards(target, accel, dt)", "+SetPlanarVelocity(v)", "+Jump(height, gravity)", "+Launch(v)",
           "+AddImpulse(v)", "+Simulate(gravity, dt)", "+SetTargetHeight(h, speed)", "+CanStandUp(h) bool",
           "+RotateTowards(dir, degPerSec, dt)", "+Teleport(pos, rot)"])
C("PlayerCharacter", "Character", "MonoBehaviour", "Фасад игрока: объединяет мотор, здоровье, бой, ввод; владеет автоматом состояний.",
  fields=["+Config : MovementConfig", "+Motor, Health, Combat, Inventory, Input", "+CurrentStateName : string",
          "+IsSprinting, IsCrouching, IsSliding, IsDead, IsAiming : bool", "+AirJumpsRemaining : int",
          "+Jumped, Landed, Respawned, StateChanged : event"],
  methods=["-BuildStateMachine()", "+GetMoveDirection() Vector3", "+GetGroundSpeed() float", "+UpdateFacing(dir, dt)",
           "+TryConsumeJumpRequest() bool", "+PerformJump()", "+Launch(velocity)", "+Respawn(pos, rot)"],
  pattern="Facade")
C("PlayerCombat", "Character", "MonoBehaviour", "Прицеливание, стрельба, перезарядка, смена оружия, отдача камеры.",
  fields=["+IsAiming, WantsCombatFacing : bool", "+AimPoint : Vector3", "+CurrentWeapon : Weapon", "+WeaponFired, ReloadStarted : event"],
  methods=["-Update() тикает текущее оружие", "-ComputeSpreadMultiplier() float"])
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
C("CharacterState", "Character.States", "abstract", "База состояний: Tick() = OnTick() + Motor.Simulate().",
  fields=["#Player : PlayerCharacter", "#Motor, Config, Input"], methods=["+Tick(dt) (final)", "#OnTick(dt)*", "+Enter()", "+Exit()"],
  pattern="Template Method")
C("GroundedState", "Character.States", "class", "На земле: стоим, идём при прицеливании, бежим, спринтуем.")
C("CrouchState", "Character.States", "class", "Присед: низкая капсула, медленное движение.")
C("SlideState", "Character.States", "class", "Подкат: рывок с затуханием скорости и низкой капсулой.", fields=["+IsFinished : bool"])
C("AirborneState", "Character.States", "class", "Прыжок/падение, двойной прыжок, управление в воздухе.")
C("DeadState", "Character.States", "class", "Смерть: персонаж останавливается, ввод игнорируется.")
R("CharacterState", "IState", "implement")
for s in ("GroundedState", "CrouchState", "SlideState", "AirborneState", "DeadState"):
    R(s, "CharacterState", "inherit")
R("CharacterState", "PlayerCharacter", "assoc", "Player")

# ============================== Cameras ==============================
C("ICameraTarget", "Cameras", "interface", "То, за чем следует камера.", fields=["+Root : Transform", "+PivotPosition : Vector3", "+IsAiming : bool"])
C("ICameraRig", "Cameras", "interface", "Сервис камеры для геймплея.",
  fields=["+Camera : Camera", "+YawRotation : Quaternion", "+Forward : Vector3"],
  methods=["+GetAimPoint(maxDist, ignoreRoot) Vector3", "+AddRecoil(pitch, yaw)", "+SetTarget(target)"])
C("CameraConfig", "Cameras", "ScriptableObject", "Чувствительность, дистанции, плечо, FOV, коллизии, отдача.",
  fields=["+mouse/gamepadSensitivity", "+distance, shoulderOffset, fieldOfView", "+aimDistance, aimShoulderOffset, aimFieldOfView",
          "+collisionRadius, collisionMask", "+recoilRecovery"])
C("ThirdPersonCamera", "Cameras", "MonoBehaviour", "Камера через плечо: орбита, прицел-зум, смена плеча, отдача, коллизии.",
  methods=["-LateUpdate()", "-ReadInput(dt)", "-UpdateRecoil(dt)", "-ResolveCollision(pivot, offset, dt) Vector3"])
R("ThirdPersonCamera", "ICameraRig", "implement")
R("ThirdPersonCamera", "CameraConfig", "assoc", "config")
R("ThirdPersonCamera", "ICameraTarget", "assoc", "target")
R("ThirdPersonCamera", "IPlayerInput", "dep", "Look, SwitchShoulder")
R("ThirdPersonCamera", "PhysicsQuery", "dep")

# ============================== Animations ==============================
C("AnimatorParams", "Animations", "static", "Имена/хэши параметров и слоёв Animator — общие для генератора и рантайма.",
  fields=["+MoveX, MoveZ, Speed, VerticalSpeed", "+IsGrounded, IsCrouching, IsSliding, IsAiming, IsDead", "+Jump, Fire, Reload, Hit",
          "+UpperBodyLayer"])
C("AnimationSlot", "Animations", "enum", "Слоты анимаций: Idle, Walk*, Run*, Sprint, Crouch*, Slide, JumpStart, Fall, Death, AimIdle, Fire, Reload, HitReact.")
C("AnimationSet", "Animations", "ScriptableObject", "Набор клипов Mixamo + ссылка на сгенерированный контроллер. Пустые слоты берут запасной клип.",
  fields=["+controller : RuntimeAnimatorController", "+walk/run/sprint/crouchSpeed", "+idle, walk*, run*, sprint ... : AnimationClip",
          "+Fallbacks$ : Dictionary"], methods=["+GetClip(slot) AnimationClip", "+HasClip(slot) bool", "+Resolve(slot, out from) AnimationClip"])
C("CharacterAnimator", "Animations", "MonoBehaviour", "Переводит состояние игрока и его события в параметры Animator; запасные процедурные позы.",
  fields=["-animator : Animator", "-animationSet : AnimationSet"], methods=["-Update() выставляет параметры", "-UpdateUpperBodyLayer(dt)",
                                                                        "-OnJumped/OnWeaponFired/OnDamaged() триггеры"],
  pattern="Adapter")
C("AimIKHandler", "Animations", "MonoBehaviour", "Humanoid IK взгляда: голова и корпус смотрят в точку прицела.",
  methods=["-OnAnimatorIK(layer)"])
R("AnimationSet", "AnimationSlot", "dep")
R("CharacterAnimator", "AnimationSet", "assoc")
R("CharacterAnimator", "PlayerCharacter", "assoc", "читает состояние")
R("CharacterAnimator", "AnimatorParams", "dep")
R("AimIKHandler", "IAimProvider", "assoc")

# ============================== Combat ==============================
C("DamageInfo", "Combat", "struct", "Описание попадания.",
  fields=["+Amount : float", "+Point, Direction : Vector3", "+Instigator : GameObject", "+IsCritical : bool"],
  methods=["+Scaled(multiplier, critical) DamageInfo"])
C("IDamageable", "Combat", "interface", "Всё, что можно повредить.",
  fields=["+IsAlive : bool", "+DamageRoot : Transform"], methods=["+ApplyDamage(in DamageInfo)"])
C("Health", "Combat", "MonoBehaviour", "Здоровье с событиями и регенерацией; публикует DamageDealtEvent.",
  fields=["+Current, Max, Normalized : float", "+IsAlive : bool", "+Damaged, Died, Changed, Revived : event"],
  methods=["+ApplyDamage(in info)", "+Heal(amount) float", "+Revive()"], pattern="Observer")
C("Hitbox", "Combat", "MonoBehaviour", "Хитбокс (голова ×2): умножает урон и передаёт его в Health родителя.",
  fields=["-damageMultiplier : float", "-isCritical : bool"], pattern="Proxy")
C("Projectile", "Combat", "MonoBehaviour", "Снаряд из пула: летит по дуге, проверяет попадание лучом, взрывается.",
  methods=["+Launch(velocity, definition, owner)", "-Explode(position)"])
C("Explosion", "Combat", "static", "Урон по площади с затуханием и толчок физики.", methods=["+Detonate(center, radius, damage, force, instigator)"])
C("PooledEffect", "Combat", "MonoBehaviour", "Простой эффект из пула: масштаб и свет по времени жизни.")
C("Tracer", "Combat", "MonoBehaviour", "Трассер пули на LineRenderer.", methods=["+Show(from, to)"])
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
C("WeaponDefinition", "Combat.Weapons", "ScriptableObject", "Данные оружия: урон, темп, патроны, разброс, отдача, эффекты. Новое оружие = новый ассет.",
  fields=["+displayName : string", "+fireMode : FireModeType", "+shotType : ShotType", "+viewPrefab : WeaponView",
          "+damage, roundsPerMinute, range", "+magazineSize, maxReserve, reloadTime", "+hip/aimSpread, spreadPerShot",
          "+pelletCount, projectilePrefab", "+recoilPitch, recoilYaw", "+tracer/impactPrefab, sounds"], pattern="Flyweight")
C("AimData", "Combat.Weapons", "struct", "Куда стрелять в этом кадре.", fields=["+AimPoint : Vector3", "+Owner : GameObject", "+IsAiming : bool", "+SpreadMultiplier : float"])
C("ShotContext", "Combat.Weapons", "struct", "Контекст одного выстрела для стратегии.",
  fields=["+Definition", "+Origin, AimPoint : Vector3", "+Owner : GameObject", "+SpreadDegrees : float", "+BaseDirection : Vector3"])
C("IFireMode", "Combat.Weapons", "interface", "КОГДА стрелять.", methods=["+Tick(weapon, pressed, held, aim, dt)", "+Reset()"], pattern="Strategy")
C("SemiAutoFireMode", "Combat.Weapons", "class", "Выстрел на нажатие (с буфером нажатия).")
C("FullAutoFireMode", "Combat.Weapons", "class", "Стрельба, пока зажат курок.")
C("BurstFireMode", "Combat.Weapons", "class", "Очередь из N выстрелов на нажатие.")
C("IShotStrategy", "Combat.Weapons", "interface", "ЧТО происходит при выстреле.", methods=["+Fire(in ShotContext)"], pattern="Strategy")
C("HitscanShot", "Combat.Weapons", "class", "Мгновенный луч.")
C("SpreadShot", "Combat.Weapons", "class", "Дробь: несколько лучей.")
C("ProjectileShot", "Combat.Weapons", "class", "Снаряд из пула.")
C("ShotUtility", "Combat.Weapons", "static", "Общий код луча: урон, сила удара, искры, трассер.", methods=["+FireRay(ctx, direction, damage)"])
C("Weapon", "Combat.Weapons", "class", "Экземпляр оружия: патроны, перезарядка, кулдаун, разброс. Поведение — в стратегиях.",
  fields=["+Definition : WeaponDefinition", "+View : WeaponView", "+AmmoInMagazine, ReserveAmmo : int", "+IsReloading, IsReadyToFire, IsEmpty",
          "+CurrentSpread : float", "+Fired, DryFired, AmmoChanged, ReloadStarted, ReloadFinished : event"],
  methods=["+Tick(dt, pressed, held, aim)", "+TryFire(aim) bool", "+TryReload() bool", "+Holster()", "+Draw()", "+AddReserveAmmo(n) int"])
C("WeaponView", "Combat.Weapons", "MonoBehaviour", "Визуал оружия: модель, дуло, вспышка, звук.",
  fields=["+Muzzle : Transform"], methods=["+PlayFireEffects(clip)", "+PlaySound(clip)", "+SetVisible(bool)", "+CreatePlaceholder(parent, name)$"])
C("WeaponFactory", "Combat.Weapons", "static", "Собирает Weapon из WeaponDefinition: визуал + стратегии по enum-ам.",
  methods=["+Create(definition, parent) Weapon", "+CreateFireMode(def) IFireMode", "+CreateShot(def) IShotStrategy"], pattern="Factory")
C("WeaponInventory", "Combat.Weapons", "MonoBehaviour", "Слоты оружия: стартовое оружие, переключение, патроны.",
  fields=["-startingWeapons : WeaponDefinition[]", "+Weapons : IReadOnlyList~Weapon~", "+Current : Weapon", "+WeaponEquipped, WeaponAdded : event"],
  methods=["+AddWeapon(def) Weapon", "+Equip(index) bool", "+EquipNext()", "+EquipPrevious()", "+AddAmmoToAll(mags) int"])
C("IAimProvider", "Combat.Weapons", "interface", "Источник точки прицела для визуала.", fields=["+IsCombatReady : bool", "+AimPoint : Vector3"])
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
C("EnemyTurret", "AI", "MonoBehaviour", "Турель: ищет игрока, доворачивается и стреляет очередями. Переиспользует StateMachine и Weapon.",
  fields=["-weapon : WeaponDefinition", "-detectionRange, turnSpeed", "+Health, Weapon"],
  methods=["+CanSeeTarget() bool", "+AimAt(point, dt) float", "+Scan(dt)", "+TickWeapon(trigger, dt, aimError)", "+SetVisible(bool)"])
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
C("Pickup", "Interaction", "abstract", "Подбираемое: триггер → TryApply → скрыть → появиться снова.",
  methods=["-OnTriggerEnter(other)", "#TryApply(player) bool*"], pattern="Template Method")
C("HealthPickup", "Interaction", "MonoBehaviour", "Аптечка.", fields=["-amount : float"])
C("AmmoPickup", "Interaction", "MonoBehaviour", "Патроны всему оружию.", fields=["-magazines : int"])
C("JumpPad", "Interaction", "MonoBehaviour", "Батут: подбрасывает игрока и восстанавливает прыжки.", fields=["-launchVelocity : Vector3"])
R("HealthPickup", "Pickup", "inherit")
R("AmmoPickup", "Pickup", "inherit")
R("Pickup", "PlayerCharacter", "dep")
R("JumpPad", "PlayerCharacter", "dep", "Launch()")

# ============================== Gameplay ==============================
C("PlayerSpawner", "Gameplay", "MonoBehaviour", "Появление и возрождение игрока, события PlayerSpawned/PlayerDied.",
  fields=["-playerPrefab : PlayerCharacter", "-spawnPoint : Transform", "-respawnDelay : float"], methods=["-Start()", "-RespawnRoutine()"])
R("PlayerSpawner", "PlayerCharacter", "assoc")
R("PlayerSpawner", "EventBus", "dep", "публикует")
R("PlayerSpawner", "ICameraRig", "dep", "SetTarget")

# ============================== UI ==============================
C("IHudView", "UI", "interface", "View (MVP): только отображение.",
  methods=["+SetHealth(cur, max)", "+SetWeapon(name, mag, size, reserve)", "+SetReload(on, progress)", "+SetCrosshair(px, aiming)",
           "+ShowHitMarker(killed, critical)", "+ShowDamageFlash(strength)", "+SetRespawnCountdown(s)", "+SetDebugText(text)"])
C("HudPresenter", "UI", "MonoBehaviour", "Presenter (MVP): слушает Health/Weapon/EventBus и обновляет IHudView.",
  methods=["-Bind(player)", "-BindWeapon(weapon)", "-SpreadToPixels(deg) float$"], pattern="MVP")
C("ImGuiHudView", "UI", "MonoBehaviour", "HUD на IMGUI: прицел, хит-маркер, HP, патроны, подсказки.")
C("PauseMenu", "UI", "MonoBehaviour", "Пауза: timeScale, курсор, отключение ввода, GamePausedEvent.", methods=["+SetPaused(bool)"])
R("ImGuiHudView", "IHudView", "implement")
R("HudPresenter", "IHudView", "assoc", "view")
R("HudPresenter", "PlayerCharacter", "assoc", "model")
R("HudPresenter", "EventBus", "dep", "подписка")
R("PauseMenu", "IPlayerInput", "dep")
R("PauseMenu", "EventBus", "dep", "публикует")

# ============================== EditorTools ==============================
C("TpsSkeletonMenu", "EditorTools", "static", "Меню Tools → TPS Skeleton: вся генерация одной кнопкой.",
  methods=["-BuildDemo()", "-RebuildAll()", "-RebuildAnimator()", "-Generate(overwritePrefabs, rebuildScene)"], pattern="Facade")
C("CoreAssetsBuilder", "EditorTools", "static", "Конфиги, AnimationSet (с клипами из Assets/Models), Animator Controller.",
  methods=["+LoadMovementConfig()", "+LoadCameraConfig()", "+LoadAnimationSet(movement)", "+BuildAnimator(set)"])
C("AnimatorControllerBuilder", "EditorTools", "class", "Собирает Animator Controller из AnimationSet: blend trees, слои, переходы.",
  methods=["+Build(path, mask) AnimatorController", "+LoadOrCreateUpperBodyMask(path)$"], pattern="Builder")
C("MannequinBuilder", "EditorTools", "class", "Манекен из примитивов на скелете Mixamo + Humanoid Avatar.",
  methods=["+WithMaterials(body, joints, accent)", "+Build(name) GameObject", "+GetBone(name) Transform", "+CreateAvatar(root, path)$"],
  pattern="Builder")
C("WeaponContentBuilder", "EditorTools", "static", "Эффекты, модели оружия из примитивов, WeaponDefinition-ы.", methods=["+Build(overwrite) Result"])
C("PrefabContentBuilder", "EditorTools", "static", "Префабы: игрок, мишень, турель, аптечка, патроны, батут.", methods=["+Build(...) Result"])
C("DemoSceneBuilder", "EditorTools", "static", "Сцена-полигон: тир, тоннель, лестница, рампа, паркур, турель.", methods=["+Build(prefabs, camera) Scene"])
C("AnimationSetEditor", "EditorTools", "Editor", "Инспектор AnimationSet: подсказки, отчёт о заменах, кнопка Rebuild.")
C("MaterialLibrary", "EditorTools", "static", "Материалы URP Lit/Unlit и текстура-шахматка.")
C("EditorAssetUtility", "EditorTools", "static", "Папки, ScriptableObject, префабы, примитивы.")
C("Serialized", "EditorTools", "class", "Fluent-установка приватных [SerializeField] через SerializedObject.", methods=["+Of(target)$", "+Set(field, value)", "+Apply()"],
  pattern="Builder")
C("SkeletonPaths", "EditorTools", "static", "Пути генерируемых ассетов (Assets/Game/Generated).")
R("TpsSkeletonMenu", "CoreAssetsBuilder", "dep")
R("TpsSkeletonMenu", "WeaponContentBuilder", "dep")
R("TpsSkeletonMenu", "PrefabContentBuilder", "dep")
R("TpsSkeletonMenu", "DemoSceneBuilder", "dep")
R("CoreAssetsBuilder", "AnimatorControllerBuilder", "dep")
R("AnimatorControllerBuilder", "AnimationSet", "dep", "читает клипы")
R("PrefabContentBuilder", "MannequinBuilder", "dep")
R("AnimationSetEditor", "AnimatorControllerBuilder", "dep")
R("WeaponContentBuilder", "WeaponDefinition", "dep", "создаёт")
R("DemoSceneBuilder", "PlayerSpawner", "dep", "размещает")
