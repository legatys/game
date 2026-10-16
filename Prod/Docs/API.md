# Справочник API

> Сгенерировано `Docs/tools/generate_api_docs.py` из исходников и XML-комментариев (`///`). Не правьте вручную: допишите комментарий в коде и перезапустите генератор.

Здесь описан каждый публичный тип и член, а также поля, настраиваемые в инспекторе (`[SerializeField]`). Как всё связано и работает вместе — в [ARCHITECTURE.md](ARCHITECTURE.md), диаграммы — в [UML.md](UML.md).

## Содержание

- **[Core](#пакет-core)** — Инфраструктура: сервисы, события, конечный автомат, пулы: [ServiceLocator](#servicelocator), [EventBus](#eventbus), [IGameEvent](#igameevent), [DamageDealtEvent](#damagedealtevent), [PlayerSpawnedEvent](#playerspawnedevent), [PlayerDiedEvent](#playerdiedevent), [GamePausedEvent](#gamepausedevent), [ExplosionEvent](#explosionevent), [IState](#istate), [StateMachine](#statemachine), [IPoolable](#ipoolable), [GameObjectPool](#gameobjectpool), [PooledObject](#pooledobject), [PoolService](#poolservice), [CoreServices](#coreservices), [PhysicsQuery](#physicsquery)
- **[Inputs](#пакет-inputs)** — Абстракция ввода поверх Unity Input System: [IPlayerInput](#iplayerinput), [PlayerInputReader](#playerinputreader), [InputActionNames](#inputactionnames)
- **[Character](#пакет-character)** — Игрок: фасад, мотор, бой, конфиг движения: [MovementConfig](#movementconfig), [CharacterMotor](#charactermotor), [PlayerCharacter](#playercharacter), [PlayerCombat](#playercombat)
- **[Character.States](#пакет-characterstates)** — Состояния движения игрока (State): [CharacterState](#characterstate), [GroundedState](#groundedstate), [CrouchState](#crouchstate), [SlideState](#slidestate), [AirborneState](#airbornestate), [DeadState](#deadstate)
- **[Cameras](#пакет-cameras)** — Камера от третьего лица: [ICameraTarget](#icameratarget), [ICameraRig](#icamerarig), [CameraConfig](#cameraconfig), [ThirdPersonCamera](#thirdpersoncamera)
- **[Animations](#пакет-animations)** — Связка геймплея с Animator и клипами Mixamo: [AnimatorParams](#animatorparams), [AnimationSlot](#animationslot), [AnimationSet](#animationset), [CharacterAnimator](#characteranimator), [AimIKHandler](#aimikhandler)
- **[Combat](#пакет-combat)** — Урон, здоровье, снаряды, эффекты: [DamageInfo](#damageinfo), [IDamageable](#idamageable), [Health](#health), [Hitbox](#hitbox), [Projectile](#projectile), [Explosion](#explosion), [PooledEffect](#pooledeffect), [Tracer](#tracer)
- **[Combat.Weapons](#пакет-combatweapons)** — Оружие: данные, стратегии, фабрика, инвентарь: [FireModeType](#firemodetype), [ShotType](#shottype), [WeaponDefinition](#weapondefinition), [AimData](#aimdata), [ShotContext](#shotcontext), [IFireMode](#ifiremode), [SemiAutoFireMode](#semiautofiremode), [FullAutoFireMode](#fullautofiremode), [BurstFireMode](#burstfiremode), [IShotStrategy](#ishotstrategy), [HitscanShot](#hitscanshot), [SpreadShot](#spreadshot), [ProjectileShot](#projectileshot), [ShotUtility](#shotutility), [Weapon](#weapon), [WeaponView](#weaponview), [WeaponFactory](#weaponfactory), [WeaponInventory](#weaponinventory), [IAimProvider](#iaimprovider), [WeaponSocket](#weaponsocket)
- **[AI](#пакет-ai)** — Враги и мишени: [EnemyTurret](#enemyturret), [TurretIdleState](#turretidlestate), [TurretAttackState](#turretattackstate), [TurretDeadState](#turretdeadstate), [TargetDummy](#targetdummy)
- **[Interaction](#пакет-interaction)** — Подбираемые предметы и объекты уровня: [Pickup](#pickup), [HealthPickup](#healthpickup), [AmmoPickup](#ammopickup), [JumpPad](#jumppad)
- **[Gameplay](#пакет-gameplay)** — Правила игры: появление игрока: [PlayerSpawner](#playerspawner)
- **[UI](#пакет-ui)** — HUD (MVP) и пауза: [IHudView](#ihudview), [HudPresenter](#hudpresenter), [ImGuiHudView](#imguihudview), [PauseMenu](#pausemenu)
- **[Demo](#пакет-demo)** — Демо-контент из примитивов: фабрики, уровень, сцена Playground (рантайм): [IMaterialProvider](#imaterialprovider), [MaterialFactory](#materialfactory), [RuntimeMaterialProvider](#runtimematerialprovider), [Primitives](#primitives), [MannequinBuilder](#mannequinbuilder), [DemoWeapons](#demoweapons), [DemoWeaponCatalog](#demoweaponcatalog), [DemoPrefabs](#demoprefabs), [DemoContentFactory](#democontentfactory), [DemoLevelBuilder](#demolevelbuilder), [PlaygroundBootstrap](#playgroundbootstrap)
- **[EditorTools](#пакет-editortools)** — Генератор демо: манекен, аниматор, префабы, сцена (только редактор): [TpsSkeletonMenu](#tpsskeletonmenu), [CoreAssetsBuilder](#coreassetsbuilder), [AnimatorControllerBuilder](#animatorcontrollerbuilder), [WeaponContentBuilder](#weaponcontentbuilder), [PrefabContentBuilder](#prefabcontentbuilder), [DemoSceneBuilder](#demoscenebuilder), [AnimationSetEditor](#animationseteditor), [AssetMaterialProvider](#assetmaterialprovider), [PlaygroundSceneOpener](#playgroundsceneopener), [EditorAssetUtility](#editorassetutility), [SkeletonPaths](#skeletonpaths)

## Пакет Core

`Game.Core` — Инфраструктура: сервисы, события, конечный автомат, пулы.

### ServiceLocator

`Game.Core` · static класс · [ServiceLocator.cs](../Assets/Game/Scripts/Runtime/Core/ServiceLocator.cs) · паттерн **Service Locator**

Паттерн Service Locator: единая точка регистрации и поиска глобальных сервисов (пул объектов, камера, игрок и т.д.), чтобы компоненты не искали друг друга через FindObjectOfType. Сервис регистрируется по типу ключа (обычно интерфейс).

**Используется в:** [CoreServices](#coreservices), [PlayerInputReader](#playerinputreader).

**Методы**

| Член | Описание |
|---|---|
| `public static void Register<T>(T service) where T : class` | Зарегистрировать сервис под типом T (заменяет предыдущий). |
| `public static void Unregister<T>(T service) where T : class` | Снимает регистрацию, только если зарегистрирован именно этот экземпляр. |
| `public static bool TryGet<T>(out T service) where T : class` | Найти сервис типа T; false, если его нет или он уничтожен. |
| `public static T Get<T>() where T : class` | Найти сервис типа T или выбросить исключение. |
| `public static void Clear()` | Удалить все сервисы. |

### EventBus

`Game.Core` · static класс · [EventBus.cs](../Assets/Game/Scripts/Runtime/Core/EventBus.cs) · паттерн **Observer**

Паттерн Observer (Publish/Subscribe): глобальная типизированная шина событий. Издатель не знает подписчиков: Health публикует DamageDealtEvent, а HUD показывает хит-маркер. Подписывайтесь в OnEnable и отписывайтесь в OnDisable.

**Связи:** использует [IGameEvent](#igameevent) (публикует).

**Используется в:** [Health](#health), [HudPresenter](#hudpresenter), [PauseMenu](#pausemenu), [PlayerSpawner](#playerspawner).

**Методы**

| Член | Описание |
|---|---|
| `public static void Subscribe<T>(Action<T> handler) where T : struct, IGameEvent` | Подписаться на события типа T. |
| `public static void Unsubscribe<T>(Action<T> handler) where T : struct, IGameEvent` | Отписаться от событий типа T. |
| `public static void Publish<T>(T gameEvent) where T : struct, IGameEvent` | Разослать событие всем подписчикам типа T. |

### IGameEvent

`Game.Core` · интерфейс · [EventBus.cs](../Assets/Game/Scripts/Runtime/Core/EventBus.cs)

Маркер для событий, которые можно публиковать через `EventBus`.

**Используется в:** [DamageDealtEvent](#damagedealtevent), [EventBus](#eventbus), [ExplosionEvent](#explosionevent), [GamePausedEvent](#gamepausedevent), [PlayerDiedEvent](#playerdiedevent), [PlayerSpawnedEvent](#playerspawnedevent).

### DamageDealtEvent

`Game.Core` · структура · [GameEvents.cs](../Assets/Game/Scripts/Runtime/Core/GameEvents.cs)

Базовые типы: [IGameEvent](#igameevent)

Кто-то получил урон (публикует `Health`).

**Связи:** реализует [IGameEvent](#igameevent).

**Поля**

| Член | Описание |
|---|---|
| `public readonly DamageInfo Info` | Описание попадания. |
| `public readonly Health Target` | Кто получил урон. |
| `public readonly bool Killed` | Попадание было смертельным. |

**Конструкторы**

| Член | Описание |
|---|---|
| `public DamageDealtEvent(DamageInfo info, Health target, bool killed)` | Создаёт событие. |

### PlayerSpawnedEvent

`Game.Core` · структура · [GameEvents.cs](../Assets/Game/Scripts/Runtime/Core/GameEvents.cs)

Базовые типы: [IGameEvent](#igameevent)

Игрок появился или возродился (публикует PlayerSpawner).

**Связи:** реализует [IGameEvent](#igameevent).

**Поля**

| Член | Описание |
|---|---|
| `public readonly PlayerCharacter Player` | Появившийся игрок. |

**Конструкторы**

| Член | Описание |
|---|---|
| `public PlayerSpawnedEvent(PlayerCharacter player)` | Создаёт событие. |

### PlayerDiedEvent

`Game.Core` · структура · [GameEvents.cs](../Assets/Game/Scripts/Runtime/Core/GameEvents.cs)

Базовые типы: [IGameEvent](#igameevent)

Игрок погиб, возрождение через RespawnDelay секунд.

**Связи:** реализует [IGameEvent](#igameevent).

**Поля**

| Член | Описание |
|---|---|
| `public readonly PlayerCharacter Player` | Погибший игрок. |
| `public readonly float RespawnDelay` | Через сколько секунд возрождение. |

**Конструкторы**

| Член | Описание |
|---|---|
| `public PlayerDiedEvent(PlayerCharacter player, float respawnDelay)` | Создаёт событие. |

### GamePausedEvent

`Game.Core` · структура · [GameEvents.cs](../Assets/Game/Scripts/Runtime/Core/GameEvents.cs)

Базовые типы: [IGameEvent](#igameevent)

Пауза включена/выключена (публикует PauseMenu).

**Связи:** реализует [IGameEvent](#igameevent).

**Поля**

| Член | Описание |
|---|---|
| `public readonly bool IsPaused` | true — пауза включена. |

**Конструкторы**

| Член | Описание |
|---|---|
| `public GamePausedEvent(bool isPaused)` | Создаёт событие. |

### ExplosionEvent

`Game.Core` · структура · [GameEvents.cs](../Assets/Game/Scripts/Runtime/Core/GameEvents.cs)

Базовые типы: [IGameEvent](#igameevent)

Что-то взорвалось (для тряски камеры, звука и т.д.).

**Связи:** реализует [IGameEvent](#igameevent).

**Поля**

| Член | Описание |
|---|---|
| `public readonly Vector3 Position` | Центр взрыва. |
| `public readonly float Radius` | Радиус взрыва. |

**Конструкторы**

| Член | Описание |
|---|---|
| `public ExplosionEvent(Vector3 position, float radius)` | Создаёт событие. |

### IState

`Game.Core` · интерфейс · [StateMachine.cs](../Assets/Game/Scripts/Runtime/Core/StateMachine.cs)

Состояние для `StateMachine` (паттерн State).

**Используется в:** [CharacterState](#characterstate), [StateMachine](#statemachine), [TurretAttackState](#turretattackstate), [TurretDeadState](#turretdeadstate), [TurretIdleState](#turretidlestate).

**Свойства**

| Член | Описание |
|---|---|
| `string Name { get; }` | Имя для отладки и HUD. |

**Методы**

| Член | Описание |
|---|---|
| `void Enter()` | Вход в состояние. |
| `void Tick(float deltaTime)` | Кадр в состоянии. |
| `void Exit()` | Выход из состояния. |

### StateMachine

`Game.Core` · sealed класс · [StateMachine.cs](../Assets/Game/Scripts/Runtime/Core/StateMachine.cs) · паттерн **State**

Паттерн State: универсальный конечный автомат с таблицей переходов. Переходы описываются условиями (Func<bool>) в одном месте, а состояния содержат только своё поведение. Используется и игроком, и ИИ (турелью). Чтобы добавить новое состояние: создайте класс IState и добавьте ему переходы.

**Связи:** ссылается на [IState](#istate) (current / transitions).

**Используется в:** [EnemyTurret](#enemyturret), [PlayerCharacter](#playercharacter).

**Свойства**

| Член | Описание |
|---|---|
| `public IState Current { get; }` | Текущее состояние. |
| `public IState Previous { get; }` | Предыдущее состояние. |

**События**

| Член | Описание |
|---|---|
| `public event Action<IState, IState> StateChanged` | (предыдущее, новое) — вызывается после смены состояния. |

**Методы**

| Член | Описание |
|---|---|
| `public void AddTransition(IState from, IState to, Func<bool> condition)` | Переход from → to, когда condition() == true. Проверяются в порядке добавления. |
| `public void AddAnyTransition(IState to, Func<bool> condition)` | Переход из любого состояния (имеет приоритет над обычными). |
| `public void SetState(IState state)` | Немедленно перейти в состояние (Exit старого → Enter нового). |
| `public void Tick(float deltaTime)` | Сначала проверяет переходы, затем тикает текущее состояние. |

### IPoolable

`Game.Core` · интерфейс · [ObjectPool.cs](../Assets/Game/Scripts/Runtime/Core/ObjectPool.cs)

Компонент на пулируемом объекте получает уведомления о выдаче/возврате в пул.

**Используется в:** [PooledEffect](#pooledeffect), [PooledObject](#pooledobject), [Projectile](#projectile), [Tracer](#tracer).

**Методы**

| Член | Описание |
|---|---|
| `void OnSpawned()` | Объект выдан из пула (сброс состояния). |
| `void OnDespawned()` | Объект возвращён в пул. |

### GameObjectPool

`Game.Core` · sealed класс · [ObjectPool.cs](../Assets/Game/Scripts/Runtime/Core/ObjectPool.cs) · паттерн **Object Pool**

Паттерн Object Pool: пул экземпляров одного префаба. Вместо Instantiate/Destroy для пуль, трассеров и эффектов объекты переиспользуются — меньше мусора и лагов.

**Связи:** владеет [PooledObject](#pooledobject) (хранит).

**Используется в:** [PoolService](#poolservice).

**Конструкторы**

| Член | Описание |
|---|---|
| `public GameObjectPool(GameObject prefab, Transform root)` | Создаёт пул префаба ; экземпляры хранятся внутри . |

**Методы**

| Член | Описание |
|---|---|
| `public PooledObject Get(Vector3 position, Quaternion rotation)` | Выдать объект (новый или из запаса) в позиции и повороте. |
| `public void Release(PooledObject item)` | Вернуть объект в пул. |
| `public void Prewarm(int count)` | Заранее создать неактивных экземпляров. |

### PooledObject

`Game.Core` · sealed класс · [PooledObject.cs](../Assets/Game/Scripts/Runtime/Core/PooledObject.cs)

Базовые типы: `MonoBehaviour`

Ссылка экземпляра на свой пул. Вызовите Release(), чтобы вернуть объект.

**Связи:** использует [IPoolable](#ipoolable) (уведомляет).

**Используется в:** [GameObjectPool](#gameobjectpool).

**Свойства**

| Член | Описание |
|---|---|
| `internal GameObjectPool Pool { get; set; }` | Пул-владелец (null — объект создан не пулом). |

**Методы**

| Член | Описание |
|---|---|
| `public void Release()` | Вернуть объект в пул (или уничтожить, если пула нет). |
| `public void ReleaseAfter(float seconds)` | Вернуть в пул через seconds секунд. |
| `internal void NotifySpawned()` | Вызывает IPoolable.OnSpawned у всех компонентов. |
| `internal void NotifyDespawned()` | Вызывает IPoolable.OnDespawned у всех компонентов. |

### PoolService

`Game.Core` · sealed класс · [ObjectPool.cs](../Assets/Game/Scripts/Runtime/Core/ObjectPool.cs)

Сервис пулов: хранит по пулу на каждый префаб. Регистрируется в ServiceLocator при старте игры (`CoreServices`) и живёт между сценами.

**Связи:** владеет [GameObjectPool](#gameobjectpool) (по пулу на префаб).

**Используется в:** [CoreServices](#coreservices).

**Конструкторы**

| Член | Описание |
|---|---|
| `public PoolService(Transform root)` | Создаёт сервис; пулы хранят объекты внутри . |

**Методы**

| Член | Описание |
|---|---|
| `public GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation)` | Выдать экземпляр префаба из его пула. |
| `public T Spawn<T>(T prefab, Vector3 position, Quaternion rotation) where T : Component` | Выдать экземпляр и вернуть его компонент T. |
| `public void Prewarm(GameObject prefab, int count)` | Заранее создать экземпляры префаба. |
| `public static void Release(GameObject instance)` | Вернуть экземпляр в пул (или уничтожить, если он не из пула). |

### CoreServices

`Game.Core` · static класс · [CoreServices.cs](../Assets/Game/Scripts/Runtime/Core/CoreServices.cs) · паттерн **Composition Root**

Composition Root для глобальных сервисов: создаётся автоматически до загрузки первой сцены, регистрирует сервисы в `ServiceLocator` и живёт всю игру (DontDestroyOnLoad). Новый глобальный сервис добавляйте сюда.

**Связи:** использует [PoolService](#poolservice) (создаёт); использует [ServiceLocator](#servicelocator) (регистрирует).

**Методы**

| Член | Описание |
|---|---|
| `public static GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation)` | Заспавнить из пула, а если сервиса нет (например, в тестовой сцене) — обычный Instantiate. |

### PhysicsQuery

`Game.Core` · static класс · [PhysicsQuery.cs](../Assets/Game/Scripts/Runtime/Core/PhysicsQuery.cs)

Физические запросы, которые игнорируют коллайдеры "владельца" (самого стрелка). Благодаря этому не нужно настраивать отдельные слои для игрока: луч камеры и пули не попадают в собственную капсулу персонажа.

**Используется в:** [ThirdPersonCamera](#thirdpersoncamera).

**Методы**

| Член | Описание |
|---|---|
| `public static bool Raycast(Ray ray, float maxDistance, LayerMask mask, Transform ignoreRoot, out RaycastHit hit)` | Ближайшее попадание луча, кроме коллайдеров внутри . |
| `public static bool SphereCast(Ray ray, float radius, float maxDistance, LayerMask mask, Transform ignoreRoot, out RaycastHit hit)` | Ближайшее попадание сферокаста, кроме коллайдеров внутри . |
| `public static Vector3 RandomInCone(Vector3 direction, float angleDegrees)` | Случайное отклонение направления внутри конуса angleDegrees (для разброса). |

## Пакет Inputs

`Game.Inputs` — Абстракция ввода поверх Unity Input System.

### IPlayerInput

`Game.Inputs` · интерфейс · [IPlayerInput.cs](../Assets/Game/Scripts/Runtime/Input/IPlayerInput.cs)

Абстракция ввода. Персонаж, камера и оружие зависят только от этого интерфейса, а не от Input System — поэтому управление можно подменить (ИИ, реплей, тесты, сеть). "Pressed" = нажато в этом кадре, "Held" = удерживается.

**Используется в:** [PauseMenu](#pausemenu), [PlayerCharacter](#playercharacter), [PlayerInputReader](#playerinputreader), [ThirdPersonCamera](#thirdpersoncamera).

**Свойства**

| Член | Описание |
|---|---|
| `Vector2 Move { get; }` | Движение: x — вбок, y — вперёд (-1..1). |
| `Vector2 Look { get; }` | Поворот камеры: дельта мыши или отклонение стика. |
| `bool LookIsDelta { get; }` | true — Look это дельта мыши (пиксели за кадр), false — стик (умножать на deltaTime). |
| `bool JumpPressed { get; }` | Прыжок нажат в этом кадре. |
| `bool SprintHeld { get; }` | Бег удерживается. |
| `bool CrouchPressed { get; }` | Присед нажат в этом кадре. |
| `bool CrouchHeld { get; }` | Присед удерживается. |
| `bool FirePressed { get; }` | Огонь нажат в этом кадре. |
| `bool FireHeld { get; }` | Огонь удерживается. |
| `bool AimHeld { get; }` | Прицел удерживается. |
| `bool ReloadPressed { get; }` | Перезарядка нажата в этом кадре. |
| `bool InteractPressed { get; }` | Взаимодействие нажато в этом кадре. |
| `bool NextWeaponPressed { get; }` | Следующее оружие. |
| `bool PreviousWeaponPressed { get; }` | Предыдущее оружие. |
| `int WeaponSlotPressed { get; }` | Индекс слота оружия (0..3), нажатого в этом кадре, или -1. |
| `bool SwitchShoulderPressed { get; }` | Сменить плечо камеры. |
| `bool PausePressed { get; }` | Пауза (работает и при выключенном геймплейном вводе). |
| `bool GameplayEnabled { get; set; }` | Выключает геймплейный ввод (пауза, смерть, катсцена). PausePressed продолжает работать. |

### PlayerInputReader

`Game.Inputs` · sealed класс · [PlayerInputReader.cs](../Assets/Game/Scripts/Runtime/Input/PlayerInputReader.cs) · паттерн **Adapter**

Базовые типы: `MonoBehaviour`, [IPlayerInput](#iplayerinput)

Атрибуты: `[DefaultExecutionOrder(-100)]`

Паттерн Adapter: адаптирует Unity Input System (InputActionAsset) к интерфейсу `IPlayerInput`. Имена действий берутся из `InputActionNames`. Если ассет не задан — используется project-wide InputSystem_Actions (Project Settings → Input System Package).

**Связи:** реализует [IPlayerInput](#iplayerinput); использует [InputActionNames](#inputactionnames); использует [ServiceLocator](#servicelocator) (регистрирует себя).

**Настройки инспектора**

| Член | Описание |
|---|---|
| `private InputActionAsset actions` | Ассет действий. Пусто — project-wide InputSystem.actions. |

**Свойства**

| Член | Описание |
|---|---|
| `public bool GameplayEnabled { get; }` | Выключает геймплейный ввод (пауза, смерть, катсцена). PausePressed продолжает работать. |
| `public Vector2 Move { get; }` | Движение: x — вбок, y — вперёд (-1..1). |
| `public Vector2 Look { get; }` | Поворот камеры: дельта мыши или отклонение стика. |
| `public bool LookIsDelta { get; }` | true — Look это дельта мыши (пиксели за кадр), false — стик (умножать на deltaTime). |
| `public bool JumpPressed { get; }` | Прыжок нажат в этом кадре. |
| `public bool SprintHeld { get; }` | Бег удерживается. |
| `public bool CrouchPressed { get; }` | Присед нажат в этом кадре. |
| `public bool CrouchHeld { get; }` | Присед удерживается. |
| `public bool FirePressed { get; }` | Огонь нажат в этом кадре. |
| `public bool FireHeld { get; }` | Огонь удерживается. |
| `public bool AimHeld { get; }` | Прицел удерживается. |
| `public bool ReloadPressed { get; }` | Перезарядка нажата в этом кадре. |
| `public bool InteractPressed { get; }` | Взаимодействие нажато в этом кадре. |
| `public bool NextWeaponPressed { get; }` | Следующее оружие. |
| `public bool PreviousWeaponPressed { get; }` | Предыдущее оружие. |
| `public bool SwitchShoulderPressed { get; }` | Сменить плечо камеры. |
| `public bool PausePressed { get; }` | Пауза (работает и при выключенном геймплейном вводе). |
| `public int WeaponSlotPressed { get; }` | Индекс слота оружия (0..3), нажатого в этом кадре, или -1. |

### InputActionNames

`Game.Inputs` · static класс · [InputActionNames.cs](../Assets/Game/Scripts/Runtime/Input/InputActionNames.cs)

Имена карты и действий из InputSystem_Actions.inputactions — единственное место, где они записаны строками.

**Используется в:** [PlayerInputReader](#playerinputreader).

**Константы**

| Член | Описание |
|---|---|
| `public const string PlayerMap` | Карта действий игрока. |
| `public const string Move` | Действие движения. |
| `public const string Look` | Действие обзора. |
| `public const string Jump` | Действие прыжка. |
| `public const string Sprint` | Действие бега. |
| `public const string Crouch` | Действие приседа/подката. |
| `public const string Attack` | Действие огня. |
| `public const string Aim` | Действие прицеливания. |
| `public const string Reload` | Действие перезарядки. |
| `public const string Interact` | Действие взаимодействия. |
| `public const string NextWeapon` | Действие "следующее оружие". |
| `public const string PreviousWeapon` | Действие "предыдущее оружие". |
| `public const string SwitchShoulder` | Действие смены плеча. |
| `public const string Pause` | Действие паузы. |

**Поля**

| Член | Описание |
|---|---|
| `public static readonly string[] WeaponSlots` | Действия выбора слотов оружия 1–4. |

## Пакет Character

`Game.Character` — Игрок: фасад, мотор, бой, конфиг движения.

### MovementConfig

`Game.Character` · sealed класс · [MovementConfig.cs](../Assets/Game/Scripts/Runtime/Character/MovementConfig.cs) · паттерн **Data-driven config**

Базовые типы: `ScriptableObject`

Атрибуты: `[CreateAssetMenu(fileName = "MovementConfig", menuName = "Game/Movement Config")]`

Data-driven настройки передвижения (ScriptableObject). Разным персонажам — разные ассеты, код при этом один. Создать: ПКМ в Project → Create → Game/Movement Config.

**Используется в:** [PlayerCharacter](#playercharacter).

**Поля**

| Член | Описание |
|---|---|
| `public float walkSpeed` | Скорость шага (при прицеливании), м/с. |
| `public float runSpeed` | Скорость бега по умолчанию, м/с. |
| `public float sprintSpeed` | Скорость спринта, м/с. |
| `public float crouchSpeed` | Скорость в приседе, м/с. |
| `public float acceleration` | Разгон на земле, м/с². |
| `public float deceleration` | Торможение на земле без ввода, м/с². |
| `public float airAcceleration` | Управление в воздухе, м/с². |
| `public float rotationSpeed` | Скорость поворота персонажа, градусов в секунду |
| `public float gravity` | Гравитация (отрицательная), м/с². |
| `public float jumpHeight` | Высота прыжка, м. |
| `public int maxAirJumps` | Сколько дополнительных прыжков в воздухе (1 = двойной прыжок) |
| `public float coyoteTime` | Время после схода с края, когда прыжок ещё засчитывается как с земли |
| `public float jumpBufferTime` | Нажатие прыжка чуть раньше приземления запоминается на это время |
| `public float standHeight` | Рост капсулы стоя, м. |
| `public float crouchHeight` | Рост капсулы в приседе и подкате, м. |
| `public float heightChangeSpeed` | Скорость изменения роста капсулы, м/с |
| `public float slideStartSpeed` | Скорость в начале подката, м/с. |
| `public float slideMinStartSpeed` | Минимальная скорость бега для начала подката, м/с. |
| `public float slideFriction` | Торможение во время подката, м/с². |
| `public float slideMaxDuration` | Максимальная длительность подката, сек. |
| `public float slideCooldown` | Пауза между подкатами, сек. |
| `public float slideSteering` | Насколько можно поворачивать во время подката (0..1). |

### CharacterMotor

`Game.Character` · sealed класс · [CharacterMotor.cs](../Assets/Game/Scripts/Runtime/Character/CharacterMotor.cs)

Базовые типы: `MonoBehaviour`

Атрибуты: `[RequireComponent(typeof(CharacterController))]`

Низкоуровневое перемещение поверх CharacterController: скорость, гравитация, прыжок, рост капсулы, проверка потолка. Ничего не знает про ввод и состояния — ими управляет `PlayerCharacter` через состояния. Этот же мотор можно использовать для NPC.

**Используется в:** [PlayerCharacter](#playercharacter).

**Настройки инспектора**

| Член | Описание |
|---|---|
| `private float groundStickVelocity` | Скорость прижатия к земле на склонах и ступеньках, м/с. |
| `private float maxFallSpeed` | Предельная скорость падения, м/с. |
| `private LayerMask collisionMask` | Слои для проверки потолка. |

**Свойства**

| Член | Описание |
|---|---|
| `public CharacterController Controller { get; }` | Используемый CharacterController. |
| `public Vector3 Velocity { get; }` | Желаемая скорость (то, что мы задаём). |
| `public Vector3 ActualVelocity { get; }` | Фактическая скорость после столкновений — для анимаций. |
| `public Vector3 PlanarVelocity { get; }` | Горизонтальная составляющая скорости. |
| `public bool IsGrounded { get; }` | Стоит ли на земле. |
| `public float AirTime { get; }` | Сколько секунд персонаж в воздухе (0 на земле). |
| `public float Height { get; }` | Текущий рост капсулы. |
| `public float Radius { get; }` | Радиус капсулы. |

**События**

| Член | Описание |
|---|---|
| `public event Action<float> Landed` | Приземлился; параметр — вертикальная скорость в момент удара (отрицательная). |

**Методы**

| Член | Описание |
|---|---|
| `public void AccelerateTowards(Vector3 targetPlanarVelocity, float acceleration, float deltaTime)` | Плавно разгоняет горизонтальную скорость к target. |
| `public void SetPlanarVelocity(Vector3 planarVelocity)` | Мгновенно задать горизонтальную скорость. |
| `public void Jump(float height, float gravity)` | Прыжок на высоту height при гравитации gravity (отрицательной). |
| `public void Launch(Vector3 velocity)` | Мгновенно задать скорость (батут, отдача взрыва). |
| `public void AddImpulse(Vector3 impulse)` | Добавить скорость (толчок). |
| `public void Simulate(float gravity, float deltaTime)` | Двигает контроллер с учётом гравитации. Вызывается раз в кадр текущим состоянием. |
| `public void SetTargetHeight(float height, float changeSpeed)` | Плавно меняет рост капсулы (присед/подкат). Пол остаётся на месте. |
| `public bool CanStandUp(float height)` | Есть ли над головой место, чтобы вырасти до height. |
| `public void RotateTowards(Vector3 direction, float degreesPerSecond, float deltaTime)` | Повернуть персонажа к горизонтальному направлению с ограничением скорости. |
| `public void Teleport(Vector3 position, Quaternion rotation)` | Телепорт (CharacterController перезаписывает transform, поэтому временно выключаем его). |

### PlayerCharacter

`Game.Character` · sealed класс · [PlayerCharacter.cs](../Assets/Game/Scripts/Runtime/Character/PlayerCharacter.cs) · паттерн **Facade**

Базовые типы: `MonoBehaviour`, [ICameraTarget](#icameratarget)

Атрибуты: `[DefaultExecutionOrder(0)]`, `[RequireComponent(typeof(CharacterMotor), typeof(Health))]`

Главный компонент игрока. Паттерн Facade: собирает мотор, здоровье, бой и ввод в один понятный API для других систем (камера, HUD, подбираемые предметы, спавнер). Также владеет конечным автоматом состояний движения — все переходы описаны в BuildStateMachine().

**Связи:** ссылается на [CharacterMotor](#charactermotor) (Motor); ссылается на [MovementConfig](#movementconfig) (config); владеет [StateMachine](#statemachine) (автомат движения); ссылается на [IPlayerInput](#iplayerinput) (Input); ссылается на [Health](#health); ссылается на [PlayerCombat](#playercombat) (Combat); реализует [ICameraTarget](#icameratarget); использует [ICameraRig](#icamerarig) (движение от камеры).

**Используется в:** [CharacterAnimator](#characteranimator), [CharacterState](#characterstate), [DemoContentFactory](#democontentfactory), [EnemyTurret](#enemyturret), [HudPresenter](#hudpresenter), [JumpPad](#jumppad), [Pickup](#pickup), [PlayerSpawner](#playerspawner).

**Настройки инспектора**

| Член | Описание |
|---|---|
| `private MovementConfig config` | Настройки передвижения. |
| `private float cameraPivotHeight` | Высота точки вращения камеры в долях роста капсулы |
| `private float combatTurnMultiplier` | Множитель скорости поворота к прицелу (быстрее, чем к направлению бега) |

**Свойства**

| Член | Описание |
|---|---|
| `public MovementConfig Config { get; }` | Настройки передвижения. |
| `public CharacterMotor Motor { get; }` | Мотор перемещения. |
| `public Health Health { get; }` | Здоровье. |
| `public PlayerCombat Combat { get; }` | Боевая часть (может отсутствовать у NPC). |
| `public WeaponInventory Inventory { get; }` | Оружие игрока. |
| `public IPlayerInput Input { get; }` | Источник ввода. |
| `public IState CurrentState { get; }` | Текущее состояние движения. |
| `public string CurrentStateName { get; }` | Имя текущего состояния (для HUD). |
| `public bool IsSprinting { get; }` | Спринт в этом кадре. |
| `public bool IsSliding { get; }` | Подкат. |
| `public bool IsCrouching { get; }` | Присед или подкат. |
| `public bool IsDead { get; }` | Персонаж мёртв. |
| `public bool IsAiming { get; }` | Прицеливается. |
| `public bool WantsCombatFacing { get; }` | Должен смотреть в сторону камеры (прицел или недавний выстрел). |
| `public int AirJumpsRemaining { get; set; }` | Сколько прыжков в воздухе осталось. |
| `public bool HasJumpRequest { get; }` | Есть запомненное нажатие прыжка (буфер). |
| `public Transform Root { get; }` | Корень цели (её коллайдеры игнорируются камерой). |
| `public Vector3 PivotPosition { get; }` | Точка, вокруг которой вращается камера (обычно уровень плеч). |

**События**

| Член | Описание |
|---|---|
| `public event Action Jumped` | Персонаж прыгнул (с земли или в воздухе). |
| `public event Action<float> Landed` | Приземлился; параметр — вертикальная скорость удара. |
| `public event Action Respawned` | Персонаж возрождён. |
| `public event Action<IState, IState> StateChanged` | (предыдущее, новое) состояние движения. |

**Методы**

| Член | Описание |
|---|---|
| `public void Configure(MovementConfig movementConfig)` | Настройка из кода. Вызывать до активации объекта (до Awake). |
| `public Vector3 GetMoveDirection()` | Направление ввода в мире относительно камеры (длина 0..1). |
| `public float GetGroundSpeed()` | Целевая скорость на земле: шаг при прицеливании, спринт или бег. |
| `public void UpdateFacing(Vector3 moveDirection, float deltaTime)` | В бою — лицом к камере (стрейф), иначе — по направлению движения. |
| `public bool TryConsumeJumpRequest()` | Использовать запомненное нажатие прыжка. |
| `public void PerformJump()` | Прыгнуть на MovementConfig.jumpHeight и вызвать Jumped. |
| `public void RefillAirJumps()` | Восстановить прыжки в воздухе. |
| `public void StartSlideCooldown()` | Запустить паузу между подкатами. |
| `public void Launch(Vector3 velocity, bool refillAirJumps = true)` | Подбросить персонажа (батут, взрыв). |
| `public void Respawn(Vector3 position, Quaternion rotation)` | Телепортировать, восстановить здоровье и вернуть в состояние Grounded. |

### PlayerCombat

`Game.Character` · sealed класс · [PlayerCombat.cs](../Assets/Game/Scripts/Runtime/Character/PlayerCombat.cs)

Базовые типы: `MonoBehaviour`, [IAimProvider](#iaimprovider)

Атрибуты: `[DefaultExecutionOrder(10)]`, `[RequireComponent(typeof(PlayerCharacter), typeof(WeaponInventory))]`

Боевая часть игрока: прицеливание, стрельба, перезарядка, смена оружия, отдача камеры. Берёт точку прицела из камеры (центр экрана) и передаёт её текущему оружию. Пробрасывает события оружия наружу, чтобы подписчикам не нужно было переподписываться при смене ствола.

**Связи:** реализует [IAimProvider](#iaimprovider); ссылается на [WeaponInventory](#weaponinventory); использует [ICameraRig](#icamerarig) (AimPoint, отдача); использует [AimData](#aimdata).

**Используется в:** [PlayerCharacter](#playercharacter).

**Настройки инспектора**

| Член | Описание |
|---|---|
| `private float aimRange` | Дальность луча прицеливания, м. |
| `private float combatFacingTime` | Сколько секунд после выстрела персонаж продолжает смотреть в прицел |
| `private float movingSpreadMultiplier` | Множитель разброса на скорости бега. |
| `private float airborneSpreadMultiplier` | Множитель разброса в воздухе. |
| `private float crouchSpreadMultiplier` | Множитель разброса в приседе. |

**Свойства**

| Член | Описание |
|---|---|
| `public WeaponInventory Inventory { get; }` | Инвентарь оружия. |
| `public Weapon CurrentWeapon { get; }` | Оружие в руках. |
| `public bool IsAiming { get; }` | Прицеливается (ПКМ и не спринт). |
| `public Vector3 AimPoint { get; }` | Точка в мире под прицелом. |
| `public bool WantsCombatFacing { get; }` | Прицел или выстрел недавно — персонаж смотрит туда же, куда камера. |
| `public bool IsCombatReady { get; }` | Персонаж целится или стреляет — оружие и голова смотрят в точку прицела. |

**События**

| Член | Описание |
|---|---|
| `public event Action<Weapon> WeaponFired` | Текущее оружие выстрелило. |
| `public event Action<Weapon> ReloadStarted` | Текущее оружие начало перезарядку. |

## Пакет Character.States

`Game.Character.States` — Состояния движения игрока (State).

### CharacterState

`Game.Character.States` · abstract класс · [CharacterState.cs](../Assets/Game/Scripts/Runtime/Character/States/CharacterState.cs) · паттерн **Template Method**

Базовые типы: [IState](#istate)

Базовое состояние персонажа. Паттерн Template Method: Tick() фиксирует порядок "логика состояния → физика мотора", а наследники переопределяют только OnTick().

**Связи:** реализует [IState](#istate); ссылается на [PlayerCharacter](#playercharacter) (Player).

**Используется в:** [AirborneState](#airbornestate), [CrouchState](#crouchstate), [DeadState](#deadstate), [GroundedState](#groundedstate), [SlideState](#slidestate).

**Поля**

| Член | Описание |
|---|---|
| `protected readonly PlayerCharacter Player` | Игрок-владелец. |

**Свойства**

| Член | Описание |
|---|---|
| `public abstract string Name { get; }` | Имя для отладки и HUD. |
| `protected CharacterMotor Motor { get; }` | Мотор игрока. |
| `protected MovementConfig Config { get; }` | Настройки передвижения. |
| `protected IPlayerInput Input { get; }` | Ввод игрока. |

**Конструкторы**

| Член | Описание |
|---|---|
| `protected CharacterState(PlayerCharacter player)` | Создаёт состояние для игрока . |

**Методы**

| Член | Описание |
|---|---|
| `public virtual void Enter()` | Вход в состояние. |
| `public virtual void Exit()` | Выход из состояния. |
| `public void Tick(float deltaTime)` | Шаблонный метод: OnTick() состояния, затем Motor.Simulate(). |
| `protected abstract void OnTick(float deltaTime)` | Логика состояния в кадре (до физики мотора). |

### GroundedState

`Game.Character.States` · sealed класс · [GroundedState.cs](../Assets/Game/Scripts/Runtime/Character/States/GroundedState.cs)

Базовые типы: [CharacterState](#characterstate)

На земле: стоим, идём (при прицеливании), бежим или спринтуем.

**Связи:** наследует [CharacterState](#characterstate).

**Свойства**

| Член | Описание |
|---|---|
| `public override string Name { get; }` | Имя для отладки и HUD. |

**Конструкторы**

| Член | Описание |
|---|---|
| `public GroundedState(PlayerCharacter player) : base(player)` | Создаёт состояние для игрока . |

**Методы**

| Член | Описание |
|---|---|
| `public override void Enter()` | Восстанавливает прыжки в воздухе и рост стоя. |
| `protected override void OnTick(float deltaTime)` | Разгон к целевой скорости и поворот. |

### CrouchState

`Game.Character.States` · sealed класс · [CrouchState.cs](../Assets/Game/Scripts/Runtime/Character/States/CrouchState.cs)

Базовые типы: [CharacterState](#characterstate)

Присед: капсула ниже, скорость меньше. Включается/выключается нажатием Crouch.

**Связи:** наследует [CharacterState](#characterstate).

**Свойства**

| Член | Описание |
|---|---|
| `public override string Name { get; }` | Имя для отладки и HUD. |

**Конструкторы**

| Член | Описание |
|---|---|
| `public CrouchState(PlayerCharacter player) : base(player)` | Создаёт состояние для игрока . |

**Методы**

| Член | Описание |
|---|---|
| `public override void Enter()` | Опускает капсулу до роста приседа. |
| `protected override void OnTick(float deltaTime)` | Медленное движение и поворот. |

### SlideState

`Game.Character.States` · sealed класс · [SlideState.cs](../Assets/Game/Scripts/Runtime/Character/States/SlideState.cs)

Базовые типы: [CharacterState](#characterstate)

Подкат: рывок в направлении бега с низкой капсулой и постепенным торможением. Заканчивается по времени или когда скорость упала до скорости приседа.

**Связи:** наследует [CharacterState](#characterstate).

**Свойства**

| Член | Описание |
|---|---|
| `public override string Name { get; }` | Имя для отладки и HUD. |
| `public bool IsFinished { get; }` | Подкат закончился (время вышло или скорость упала). |

**Конструкторы**

| Член | Описание |
|---|---|
| `public SlideState(PlayerCharacter player) : base(player)` | Создаёт состояние для игрока . |

**Методы**

| Член | Описание |
|---|---|
| `public override void Enter()` | Рывок по направлению бега, капсула опускается. |
| `public override void Exit()` | Запускает паузу между подкатами. |
| `protected override void OnTick(float deltaTime)` | Торможение и лёгкое подруливание. |

### AirborneState

`Game.Character.States` · sealed класс · [AirborneState.cs](../Assets/Game/Scripts/Runtime/Character/States/AirborneState.cs)

Базовые типы: [CharacterState](#characterstate)

В воздухе: прыжок (если вошли по нажатию), падение, прыжки в воздухе (двойной прыжок) и слабое управление в полёте с сохранением набранной скорости.

**Связи:** наследует [CharacterState](#characterstate).

**Свойства**

| Член | Описание |
|---|---|
| `public override string Name { get; }` | Имя для отладки и HUD. |

**Конструкторы**

| Член | Описание |
|---|---|
| `public AirborneState(PlayerCharacter player) : base(player)` | Создаёт состояние для игрока . |

**Методы**

| Член | Описание |
|---|---|
| `public override void Enter()` | Выполняет прыжок, если вход был по нажатию, и запоминает скорость для управления в воздухе. |
| `protected override void OnTick(float deltaTime)` | Прыжки в воздухе и слабое управление направлением. |

### DeadState

`Game.Character.States` · sealed класс · [DeadState.cs](../Assets/Game/Scripts/Runtime/Character/States/DeadState.cs)

Базовые типы: [CharacterState](#characterstate)

Смерть: ввод игнорируется, персонаж останавливается. Выход — через PlayerCharacter.Respawn().

**Связи:** наследует [CharacterState](#characterstate).

**Свойства**

| Член | Описание |
|---|---|
| `public override string Name { get; }` | Имя для отладки и HUD. |

**Конструкторы**

| Член | Описание |
|---|---|
| `public DeadState(PlayerCharacter player) : base(player)` | Создаёт состояние для игрока . |

**Методы**

| Член | Описание |
|---|---|
| `protected override void OnTick(float deltaTime)` | Плавная остановка. |

## Пакет Cameras

`Game.Cameras` — Камера от третьего лица.

### ICameraTarget

`Game.Cameras` · интерфейс · [CameraInterfaces.cs](../Assets/Game/Scripts/Runtime/Camera/CameraInterfaces.cs)

То, за чем следует камера (игрок). Камера не знает конкретного класса персонажа.

**Используется в:** [PlayerCharacter](#playercharacter), [ThirdPersonCamera](#thirdpersoncamera).

**Свойства**

| Член | Описание |
|---|---|
| `Transform Root { get; }` | Корень цели (её коллайдеры игнорируются камерой). |
| `Vector3 PivotPosition { get; }` | Точка, вокруг которой вращается камера (обычно уровень плеч). |
| `bool IsAiming { get; }` | Цель целится — камера приближается. |

### ICameraRig

`Game.Cameras` · интерфейс · [CameraInterfaces.cs](../Assets/Game/Scripts/Runtime/Camera/CameraInterfaces.cs)

Сервис камеры для геймплея: направление взгляда, точка прицеливания, отдача.

**Используется в:** [PlayerCharacter](#playercharacter), [PlayerCombat](#playercombat), [PlayerSpawner](#playerspawner), [ThirdPersonCamera](#thirdpersoncamera).

**Свойства**

| Член | Описание |
|---|---|
| `UnityEngine.Camera Camera { get; }` | Unity-камера. |
| `Quaternion YawRotation { get; }` | Поворот камеры только по горизонтали — для движения относительно камеры. |
| `Vector3 Forward { get; }` | Направление взгляда камеры. |

**Методы**

| Член | Описание |
|---|---|
| `Vector3 GetAimPoint(float maxDistance, Transform ignoreRoot)` | Точка в мире под прицелом (центр экрана), игнорируя коллайдеры ignoreRoot. |
| `void AddRecoil(float pitchDegrees, float yawDegrees)` | Добавить отдачу: вверх на , вбок на . |
| `void SetTarget(ICameraTarget target)` | Следовать за целью. |

### CameraConfig

`Game.Cameras` · sealed класс · [CameraConfig.cs](../Assets/Game/Scripts/Runtime/Camera/CameraConfig.cs)

Базовые типы: `ScriptableObject`

Атрибуты: `[CreateAssetMenu(fileName = "CameraConfig", menuName = "Game/Camera Config")]`

Настройки камеры от третьего лица (ScriptableObject).

**Используется в:** [ThirdPersonCamera](#thirdpersoncamera).

**Поля**

| Член | Описание |
|---|---|
| `public float mouseSensitivity` | Градусов на пиксель движения мыши |
| `public float gamepadSensitivity` | Градусов в секунду при полном отклонении стика |
| `public float aimSensitivityMultiplier` | Множитель чувствительности при прицеливании. |
| `public bool invertY` | Инвертировать вертикальную ось. |
| `public float minPitch` | Минимальный наклон камеры (взгляд вверх), градусы. |
| `public float maxPitch` | Максимальный наклон камеры (взгляд вниз), градусы. |
| `public float distance` | Дистанция до персонажа, м. |
| `public float shoulderOffset` | Смещение вбок (через плечо). Знак меняется кнопкой SwitchShoulder |
| `public float heightOffset` | Смещение камеры вверх, м. |
| `public float fieldOfView` | Угол обзора, градусы. |
| `public float aimDistance` | Дистанция при прицеливании, м. |
| `public float aimShoulderOffset` | Смещение через плечо при прицеливании, м. |
| `public float aimFieldOfView` | Угол обзора при прицеливании, градусы. |
| `public float aimTransitionSpeed` | Скорость перехода в режим прицела. |
| `public float shoulderSwitchSpeed` | Скорость смены плеча. |
| `public float followSmoothTime` | Сглаживание следования за персонажем, сек. |
| `public float collisionRadius` | Радиус сферы для отталкивания от стен, м. |
| `public LayerMask collisionMask` | Слои, от которых отталкивается камера. |
| `public float distanceRecoverSpeed` | Скорость возврата камеры на дистанцию после столкновения со стеной, м/с |
| `public float recoilRecovery` | Скорость возврата прицела после отдачи, градусов в секунду |
| `public float recoilKickSpeed` | Скорость "подброса" отдачи, градусов в секунду. |

### ThirdPersonCamera

`Game.Cameras` · sealed класс · [ThirdPersonCamera.cs](../Assets/Game/Scripts/Runtime/Camera/ThirdPersonCamera.cs)

Базовые типы: `MonoBehaviour`, [ICameraRig](#icamerarig)

Атрибуты: `[DefaultExecutionOrder(100)]`, `[RequireComponent(typeof(UnityEngine.Camera))]`

Камера от третьего лица "через плечо": орбита вокруг цели, приближение при прицеливании, смена плеча, отдача, отталкивание от стен (SphereCast). Вешается на Main Camera. Регистрируется в ServiceLocator как `ICameraRig`.

**Связи:** реализует [ICameraRig](#icamerarig); ссылается на [CameraConfig](#cameraconfig) (config); ссылается на [ICameraTarget](#icameratarget) (target); использует [IPlayerInput](#iplayerinput) (Look, SwitchShoulder); использует [PhysicsQuery](#physicsquery).

**Используется в:** [DemoLevelBuilder](#demolevelbuilder).

**Настройки инспектора**

| Член | Описание |
|---|---|
| `private CameraConfig config` | Настройки камеры. Пусто — значения по умолчанию. |

**Свойства**

| Член | Описание |
|---|---|
| `public Quaternion YawRotation { get; }` | Поворот камеры только по горизонтали — для движения относительно камеры. |
| `public Vector3 Forward { get; }` | Направление взгляда камеры. |

**Методы**

| Член | Описание |
|---|---|
| `public void Configure(CameraConfig cameraConfig)` | Настройка из кода. Вызывать до активации объекта (до Awake). |
| `public void SetTarget(ICameraTarget target)` | Следовать за целью. |
| `public void AddRecoil(float pitchDegrees, float yawDegrees)` | Добавить отдачу: вверх на , вбок на . |
| `public Vector3 GetAimPoint(float maxDistance, Transform ignoreRoot)` | Точка в мире под прицелом (центр экрана), игнорируя коллайдеры ignoreRoot. |

## Пакет Animations

`Game.Animations` — Связка геймплея с Animator и клипами Mixamo.

### AnimatorParams

`Game.Animations` · static класс · [AnimatorParams.cs](../Assets/Game/Scripts/Runtime/Animation/AnimatorParams.cs)

Имена параметров, состояний и слоёв Animator Controller. Генератор контроллера и `CharacterAnimator` берут имена отсюда — переименовывать только здесь.

**Используется в:** [CharacterAnimator](#characteranimator).

**Константы**

| Член | Описание |
|---|---|
| `public const string MoveX` | Float: скорость вбок в локальных осях персонажа, м/с. |
| `public const string MoveZ` | Float: скорость вперёд/назад в локальных осях, м/с. |
| `public const string Speed` | Float: горизонтальная скорость, м/с. |
| `public const string VerticalSpeed` | Float: вертикальная скорость, м/с. |
| `public const string IsGrounded` | Bool: персонаж на земле. |
| `public const string IsCrouching` | Bool: присед или подкат. |
| `public const string IsSliding` | Bool: подкат. |
| `public const string IsAiming` | Bool: оружие поднято (прицел или стрельба). |
| `public const string IsDead` | Bool: персонаж мёртв. |
| `public const string Jump` | Trigger: прыжок. |
| `public const string Fire` | Trigger: выстрел. |
| `public const string Reload` | Trigger: начало перезарядки. |
| `public const string Hit` | Trigger: получен урон. |
| `public const string UpperBodyLayer` | Имя слоя верхней части тела. |

**Поля**

| Член | Описание |
|---|---|
| `public static readonly int MoveXHash` | Хэш параметра MoveX. |
| `public static readonly int MoveZHash` | Хэш параметра MoveZ. |
| `public static readonly int SpeedHash` | Хэш параметра Speed. |
| `public static readonly int VerticalSpeedHash` | Хэш параметра VerticalSpeed. |
| `public static readonly int IsGroundedHash` | Хэш параметра IsGrounded. |
| `public static readonly int IsCrouchingHash` | Хэш параметра IsCrouching. |
| `public static readonly int IsSlidingHash` | Хэш параметра IsSliding. |
| `public static readonly int IsAimingHash` | Хэш параметра IsAiming. |
| `public static readonly int IsDeadHash` | Хэш параметра IsDead. |
| `public static readonly int JumpHash` | Хэш триггера Jump. |
| `public static readonly int FireHash` | Хэш триггера Fire. |
| `public static readonly int ReloadHash` | Хэш триггера Reload. |
| `public static readonly int HitHash` | Хэш триггера Hit. |

### AnimationSlot

`Game.Animations` · перечисление · [AnimationSet.cs](../Assets/Game/Scripts/Runtime/Animation/AnimationSet.cs)

Слоты анимаций персонажа.

**Используется в:** [AnimationSet](#animationset).

**Значения**

| Значение | Описание |
|---|---|
| `Idle` | — |
| `WalkForward, WalkBackward, WalkLeft, WalkRight` | — |
| `RunForward, RunBackward, RunLeft, RunRight` | — |
| `Sprint` | — |
| `CrouchIdle, CrouchForward, CrouchBackward, CrouchLeft, CrouchRight` | — |
| `Slide, JumpStart, Fall, Death` | — |
| `AimIdle, Fire, Reload, HitReact` | — |

### AnimationSet

`Game.Animations` · sealed класс · [AnimationSet.cs](../Assets/Game/Scripts/Runtime/Animation/AnimationSet.cs)

Базовые типы: `ScriptableObject`

Атрибуты: `[CreateAssetMenu(fileName = "AnimationSet", menuName = "Game/Animation Set")]`

Набор анимаций персонажа (ScriptableObject). Сюда перетаскиваются клипы из Mixamo (Rig = Humanoid), затем кнопка "Rebuild Animator Controller" в инспекторе собирает контроллер. Пустые слоты заменяются ближайшим запасным клипом (см. Fallbacks), поэтому персонаж работает даже с одними Idle и Run. Ссылка на собранный контроллер хранится в поле controller, и `CharacterAnimator` подставляет его в Animator при старте.

**Связи:** использует [AnimationSlot](#animationslot).

**Используется в:** [AnimatorControllerBuilder](#animatorcontrollerbuilder), [CharacterAnimator](#characteranimator), [PlaygroundBootstrap](#playgroundbootstrap).

**Поля**

| Член | Описание |
|---|---|
| `public RuntimeAnimatorController controller` | Сгенерированный Animator Controller; CharacterAnimator подставляет его в Animator при старте. |
| `public float walkSpeed` | Скорость ходьбы (м/с) — точка клипов Walk в blend tree. |
| `public float runSpeed` | Скорость бега (м/с) — точка клипов Run в blend tree. |
| `public float sprintSpeed` | Скорость спринта (м/с) — точка клипа Sprint в blend tree. |
| `public float crouchSpeed` | Скорость шага в приседе (м/с) — точка клипов Crouch в blend tree. |
| `public AnimationClip idle` | Стойка на месте. |
| `public AnimationClip walkForward` | Шаг вперёд. |
| `public AnimationClip walkBackward` | Шаг назад. |
| `public AnimationClip walkLeft` | Шаг влево (стрейф). |
| `public AnimationClip walkRight` | Шаг вправо (стрейф). |
| `public AnimationClip runForward` | Бег вперёд. |
| `public AnimationClip runBackward` | Бег назад. |
| `public AnimationClip runLeft` | Бег влево. |
| `public AnimationClip runRight` | Бег вправо. |
| `public AnimationClip sprint` | Mixamo: Running / Fast Run |
| `public AnimationClip crouchIdle` | Стойка в приседе. |
| `public AnimationClip crouchForward` | Гусиный шаг вперёд. |
| `public AnimationClip crouchBackward` | Гусиный шаг назад. |
| `public AnimationClip crouchLeft` | Гусиный шаг влево. |
| `public AnimationClip crouchRight` | Гусиный шаг вправо. |
| `public AnimationClip slide` | Подкат. |
| `public AnimationClip jumpStart` | Начало прыжка. |
| `public AnimationClip fall` | Падение / полёт (цикл). |
| `public AnimationClip death` | Смерть (не цикл). |
| `public AnimationClip aimIdle` | Оружие у плеча, прицеливание (слой UpperBody). |
| `public AnimationClip fire` | Выстрел (слой UpperBody). |
| `public AnimationClip reload` | Перезарядка (слой UpperBody). |
| `public AnimationClip hitReact` | Реакция на попадание (слой UpperBody). |
| `public static readonly IReadOnlyDictionary<AnimationSlot, AnimationSlot[]> Fallbacks` | Цепочки замен для пустых слотов. |

**Методы**

| Член | Описание |
|---|---|
| `public AnimationClip GetClip(AnimationSlot slot)` | Клип, назначенный в слот (без замен), или null. |
| `public bool HasClip(AnimationSlot slot)` | Назначен ли клип в слот (без учёта замен). |
| `public AnimationClip Resolve(AnimationSlot slot, out AnimationSlot resolvedFrom)` | Клип слота или первый доступный запасной. resolvedFrom — откуда реально взят клип. |

### CharacterAnimator

`Game.Animations` · sealed класс · [CharacterAnimator.cs](../Assets/Game/Scripts/Runtime/Animation/CharacterAnimator.cs) · паттерн **Adapter**

Базовые типы: `MonoBehaviour`

Атрибуты: `[DefaultExecutionOrder(50)]`, `[RequireComponent(typeof(PlayerCharacter))]`

Паттерн Adapter: переводит состояние геймплея (скорость, присед, прицел, события выстрела/урона) в параметры Animator. Геймплей ничего не знает об Animator — анимации можно менять независимо. Animator ищется в дочерних объектах (модель персонажа), поэтому модель легко заменить на любую Humanoid-модель Mixamo, оставив этот компонент на корне.

**Связи:** ссылается на [AnimationSet](#animationset); ссылается на [PlayerCharacter](#playercharacter) (читает состояние); использует [AnimatorParams](#animatorparams).

**Настройки инспектора**

| Член | Описание |
|---|---|
| `private Animator animator` | Animator модели. Пусто — ищется в дочерних объектах. |
| `private AnimationSet animationSet` | Набор анимаций; его контроллер подставляется в Animator. |
| `private float locomotionDamping` | Сглаживание параметров движения, сек. |
| `private float upperBodyBlendSpeed` | Скорость изменения веса слоя UpperBody в секунду. |
| `private float actionHoldTime` | Сколько держать слой верха тела после выстрела/попадания |
| `private float groundedGraceTime` | Короткие отрывы от земли (ступеньки) не считаются падением |
| `private bool squashVisualWithoutCrouchClip` | Нет клипа приседа — сплющивать модель по высоте капсулы, чтобы присед был виден |
| `private bool tipOverWithoutDeathClip` | Нет клипа смерти — модель падает на спину |

**Свойства**

| Член | Описание |
|---|---|
| `public Animator Animator { get; }` | Используемый Animator. |

**Методы**

| Член | Описание |
|---|---|
| `public void Configure(Animator targetAnimator, AnimationSet set)` | Настройка из кода. Вызывать до активации объекта (до Awake). |

### AimIKHandler

`Game.Animations` · sealed класс · [AimIKHandler.cs](../Assets/Game/Scripts/Runtime/Animation/AimIKHandler.cs)

Базовые типы: `MonoBehaviour`

Атрибуты: `[RequireComponent(typeof(Animator))]`

Humanoid IK взгляда: голова и корпус поворачиваются к точке прицела. Должен висеть на том же объекте, что и Animator (OnAnimatorIK вызывается только там), а в контроллере у базового слоя должен быть включён IK Pass (генератор делает это сам).

**Связи:** ссылается на [IAimProvider](#iaimprovider).

**Настройки инспектора**

| Член | Описание |
|---|---|
| `private float combatWeight` | Вес взгляда в прицел, когда персонаж целится или стреляет. |
| `private float idleWeight` | Вес взгляда вне боя (голова следит за камерой). |
| `private float bodyWeight` | Насколько поворачивается корпус. |
| `private float headWeight` | Насколько поворачивается голова. |
| `private float clampWeight` | Ограничение угла поворота (0 — без ограничений, 1 — не поворачивать). |
| `private float blendSpeed` | Скорость изменения веса IK в секунду. |

## Пакет Combat

`Game.Combat` — Урон, здоровье, снаряды, эффекты.

### DamageInfo

`Game.Combat` · структура · [DamageInfo.cs](../Assets/Game/Scripts/Runtime/Combat/Damage/DamageInfo.cs)

Неизменяемое описание одного попадания: сколько, куда, откуда и кто стрелял.

**Используется в:** [Health](#health).

**Поля**

| Член | Описание |
|---|---|
| `public readonly float Amount` | Количество урона. |
| `public readonly Vector3 Point` | Точка попадания в мире. |
| `public readonly Vector3 Direction` | Направление удара. |
| `public readonly GameObject Instigator` | Кто нанёс урон (стрелок). |
| `public readonly bool IsCritical` | Критическое попадание (например, в голову). |

**Конструкторы**

| Член | Описание |
|---|---|
| `public DamageInfo(float amount, Vector3 point, Vector3 direction, GameObject instigator, bool isCritical = false)` | Создаёт описание попадания. |

**Методы**

| Член | Описание |
|---|---|
| `public DamageInfo Scaled(float multiplier, bool critical)` | Копия с уроном, умноженным на ; при помечается как критическая. |

### IDamageable

`Game.Combat` · интерфейс · [DamageInfo.cs](../Assets/Game/Scripts/Runtime/Combat/Damage/DamageInfo.cs)

Всё, что можно повредить: здоровье, хитбоксы, разрушаемые объекты.

**Используется в:** [Explosion](#explosion), [Health](#health), [Hitbox](#hitbox), [ShotUtility](#shotutility).

**Свойства**

| Член | Описание |
|---|---|
| `bool IsAlive { get; }` | Жив ли объект. |
| `Transform DamageRoot { get; }` | Корневой объект "жертвы" — чтобы взрыв не задел один объект дважды через разные хитбоксы. |

**Методы**

| Член | Описание |
|---|---|
| `void ApplyDamage(in DamageInfo info)` | Нанести урон. |

### Health

`Game.Combat` · sealed класс · [Health.cs](../Assets/Game/Scripts/Runtime/Combat/Damage/Health.cs) · паттерн **Observer**

Базовые типы: `MonoBehaviour`, [IDamageable](#idamageable)

Здоровье с событиями (паттерн Observer): Damaged, Died, Changed, Revived. HUD, аниматор, спавнер и ИИ подписываются на события вместо опроса каждый кадр. Дополнительно публикует глобальные события в `EventBus`.

**Связи:** реализует [IDamageable](#idamageable); использует [DamageInfo](#damageinfo); использует [EventBus](#eventbus) (DamageDealtEvent).

**Используется в:** [EnemyTurret](#enemyturret), [Hitbox](#hitbox), [PlayerCharacter](#playercharacter), [TargetDummy](#targetdummy).

**Настройки инспектора**

| Член | Описание |
|---|---|
| `private float maxHealth` | Максимальное здоровье. |
| `private bool invulnerable` | Не получать урон (для отладки). |
| `private float regenDelay` | Через сколько секунд после урона начинается регенерация. |
| `private float regenPerSecond` | Восстановление здоровья в секунду (0 — без регенерации). |

**Свойства**

| Член | Описание |
|---|---|
| `public float Current { get; }` | Текущее здоровье. |
| `public float Max { get; }` | Максимальное здоровье. |
| `public float Normalized { get; }` | Здоровье в долях от максимума (0..1). |
| `public bool IsAlive { get; }` | Жив ли объект. |
| `public Transform DamageRoot { get; }` | Корневой объект "жертвы" — чтобы взрыв не задел один объект дважды через разные хитбоксы. |

**События**

| Член | Описание |
|---|---|
| `public event Action<DamageInfo> Damaged` | Получен урон (вызывается и на смертельном попадании). |
| `public event Action<DamageInfo> Died` | Здоровье дошло до нуля. |
| `public event Action<float, float> Changed` | (текущее, максимальное) |
| `public event Action Revived` | Здоровье восстановлено через Revive(). |

**Методы**

| Член | Описание |
|---|---|
| `public void Configure(float max, float regenDelaySeconds = 4f, float regenPerSecondAmount = 0f)` | Настройка из кода (генератор, рантайм-сборка уровня). Вызывать до активации объекта. |
| `public void ApplyDamage(in DamageInfo info)` | Уменьшает здоровье, вызывает события и публикует DamageDealtEvent. |
| `public float Heal(float amount)` | Лечит и возвращает, сколько реально восстановлено. |
| `public void Revive()` | Полное восстановление (респаун). |

### Hitbox

`Game.Combat` · sealed класс · [Hitbox.cs](../Assets/Game/Scripts/Runtime/Combat/Damage/Hitbox.cs) · паттерн **Proxy**

Базовые типы: `MonoBehaviour`, [IDamageable](#idamageable)

Атрибуты: `[RequireComponent(typeof(Collider))]`

Хитбокс на отдельном коллайдере (голова, корпус): умножает урон и передаёт его в `Health` родителя (паттерн Proxy). Пример: голова ×2 и IsCritical.

**Связи:** реализует [IDamageable](#idamageable); ссылается на [Health](#health) (передаёт урон).

**Настройки инспектора**

| Член | Описание |
|---|---|
| `private float damageMultiplier` | Множитель урона (голова — 2). |
| `private bool isCritical` | Отмечать попадание как критическое (для хит-маркера). |

**Свойства**

| Член | Описание |
|---|---|
| `public bool IsAlive { get; }` | Жив ли объект. |
| `public Transform DamageRoot { get; }` | Корневой объект "жертвы" — чтобы взрыв не задел один объект дважды через разные хитбоксы. |

**Методы**

| Член | Описание |
|---|---|
| `public void Configure(float multiplier, bool critical)` | Задаёт множитель урона и критичность. Вызывать до активации объекта. |
| `public void ApplyDamage(in DamageInfo info)` | Умножает урон и передаёт его в Health родителя. |

### Projectile

`Game.Combat` · sealed класс · [Projectile.cs](../Assets/Game/Scripts/Runtime/Combat/Projectiles/Projectile.cs)

Базовые типы: `MonoBehaviour`, [IPoolable](#ipoolable)

Снаряд из пула (граната/ракета). Двигается сам (без Rigidbody), каждый шаг проверяет попадание лучом — быстрые снаряды не пролетают сквозь стены. При попадании взрывается.

**Связи:** реализует [IPoolable](#ipoolable); использует [Explosion](#explosion).

**Используется в:** [ProjectileShot](#projectileshot).

**Настройки инспектора**

| Член | Описание |
|---|---|
| `private float lifetime` | Через сколько секунд снаряд взрывается сам. |
| `private GameObject explosionEffectPrefab` | Эффект взрыва. |
| `private float explosionForce` | Сила толчка физических объектов взрывом. |

**Методы**

| Член | Описание |
|---|---|
| `public void Configure(GameObject explosionEffect)` | Задаёт эффект взрыва. Вызывать до активации объекта. |
| `public void Launch(Vector3 velocity, WeaponDefinition definition, GameObject owner)` | Запускает снаряд со скоростью ; урон и радиус берутся из . |
| `public void OnSpawned()` | Объект выдан из пула (сброс состояния). |
| `public void OnDespawned()` | Объект возвращён в пул. |

### Explosion

`Game.Combat` · static класс · [Explosion.cs](../Assets/Game/Scripts/Runtime/Combat/Projectiles/Explosion.cs)

Урон по площади с затуханием к краю и толчком физических объектов.

**Связи:** использует [IDamageable](#idamageable).

**Используется в:** [Projectile](#projectile).

**Методы**

| Член | Описание |
|---|---|
| `public static void Detonate(Vector3 center, float radius, float damage, float force, GameObject instigator)` | Наносит урон всем IDamageable в радиусе (с затуханием к краю), толкает физические тела и публикует ExplosionEvent. |

### PooledEffect

`Game.Combat` · sealed класс · [PooledEffect.cs](../Assets/Game/Scripts/Runtime/Combat/Effects/PooledEffect.cs)

Базовые типы: `MonoBehaviour`, [IPoolable](#ipoolable)

Простой эффект из пула (искры попадания, взрыв): живёт lifetime секунд, меняет масштаб от startScale до endScale и гасит свет, затем возвращается в пул. Замените на ParticleSystem при желании.

**Связи:** реализует [IPoolable](#ipoolable).

**Настройки инспектора**

| Член | Описание |
|---|---|
| `private float lifetime` | Время жизни эффекта, сек. |
| `private float startScale` | Множитель масштаба в начале. |
| `private float endScale` | Множитель масштаба в конце. |
| `private Light effectLight` | Свет, который гаснет вместе с эффектом (необязательно). |

**Методы**

| Член | Описание |
|---|---|
| `public void Configure(float lifetimeSeconds, float fromScale, float toScale, Light fadingLight = null)` | Задаёт параметры эффекта. Вызывать до активации объекта. |
| `public void OnSpawned()` | Объект выдан из пула (сброс состояния). |
| `public void OnDespawned()` | Объект возвращён в пул. |

### Tracer

`Game.Combat` · sealed класс · [Tracer.cs](../Assets/Game/Scripts/Runtime/Combat/Effects/Tracer.cs)

Базовые типы: `MonoBehaviour`, [IPoolable](#ipoolable)

Атрибуты: `[RequireComponent(typeof(LineRenderer))]`

Трассер пули: линия от дула до точки попадания, быстро сужается и возвращается в пул.

**Связи:** реализует [IPoolable](#ipoolable).

**Используется в:** [ShotUtility](#shotutility).

**Настройки инспектора**

| Член | Описание |
|---|---|
| `private float lifetime` | Сколько секунд виден трассер. |
| `private float width` | Начальная толщина линии. |

**Методы**

| Член | Описание |
|---|---|
| `public void Show(Vector3 from, Vector3 to)` | Рисует линию от до . |
| `public void OnSpawned()` | Объект выдан из пула (сброс состояния). |
| `public void OnDespawned()` | Объект возвращён в пул. |

## Пакет Combat.Weapons

`Game.Combat.Weapons` — Оружие: данные, стратегии, фабрика, инвентарь.

### FireModeType

`Game.Combat.Weapons` · перечисление · [WeaponDefinition.cs](../Assets/Game/Scripts/Runtime/Combat/Weapons/WeaponDefinition.cs)

SemiAuto, FullAuto, Burst

**Используется в:** [WeaponDefinition](#weapondefinition).

**Значения**

| Значение | Описание |
|---|---|
| `SemiAuto, FullAuto, Burst` | — |

### ShotType

`Game.Combat.Weapons` · перечисление · [WeaponDefinition.cs](../Assets/Game/Scripts/Runtime/Combat/Weapons/WeaponDefinition.cs)

Hitscan, Spread, Projectile

**Используется в:** [WeaponDefinition](#weapondefinition).

**Значения**

| Значение | Описание |
|---|---|
| `Hitscan, Spread, Projectile` | — |

### WeaponDefinition

`Game.Combat.Weapons` · sealed класс · [WeaponDefinition.cs](../Assets/Game/Scripts/Runtime/Combat/Weapons/WeaponDefinition.cs) · паттерн **Flyweight**

Базовые типы: `ScriptableObject`

Атрибуты: `[CreateAssetMenu(fileName = "Weapon", menuName = "Game/Weapon Definition")]`

Описание оружия (ScriptableObject, паттерн Flyweight): общие неизменяемые данные, которые разделяют все экземпляры оружия. Новое оружие = новый ассет, без кода. Поведение выбирается полями fireMode / shotType (их превращает в стратегии `WeaponFactory`). Create → Game/Weapon Definition.

**Связи:** использует [FireModeType](#firemodetype); использует [ShotType](#shottype).

**Используется в:** [DemoWeaponCatalog](#demoweaponcatalog), [Weapon](#weapon), [WeaponContentBuilder](#weaponcontentbuilder).

**Поля**

| Член | Описание |
|---|---|
| `public string displayName` | Название для HUD. |
| `public FireModeType fireMode` | Режим огня → IFireMode (см. WeaponFactory). |
| `public ShotType shotType` | Тип выстрела → IShotStrategy (см. WeaponFactory). |
| `public WeaponView viewPrefab` | Префаб модели оружия с компонентом WeaponView. Пусто — сгенерируется кубик |
| `public float damage` | Урон за попадание (у дробовика — за дробинку). |
| `public float roundsPerMinute` | Выстрелов в минуту |
| `public float range` | Дальность луча, м. |
| `public float impactForce` | Импульс, передаваемый физическим объектам при попадании. |
| `public LayerMask hitMask` | Слои, в которые можно попасть. |
| `public int burstCount` | Выстрелов в очереди (режим Burst). |
| `public float burstCooldown` | Пауза после очереди, сек. |
| `public int magazineSize` | Ёмкость магазина. |
| `public int startingReserve` | Запас патронов при получении оружия. |
| `public int maxReserve` | Максимальный запас патронов. |
| `public float reloadTime` | Время перезарядки, сек. |
| `public bool autoReloadWhenEmpty` | Перезаряжать автоматически при попытке выстрела с пустым магазином. |
| `public float hipSpread` | Базовый разброс от бедра, градусы. |
| `public float aimSpread` | Базовый разброс при прицеливании, градусы. |
| `public float spreadPerShot` | Насколько раскрывается разброс за выстрел, градусы. |
| `public float maxBloom` | Максимальное раскрытие разброса, градусы. |
| `public float spreadRecovery` | Скорость уменьшения разброса, градусов в секунду |
| `public int pelletCount` | Дробинок за выстрел (тип Spread). |
| `public float pelletSpread` | Разлёт дробинок, градусы. |
| `public Projectile projectilePrefab` | Префаб снаряда (тип Projectile). |
| `public float projectileSpeed` | Начальная скорость снаряда, м/с. |
| `public float projectileGravity` | Гравитация для снаряда, м/с². |
| `public float explosionRadius` | Радиус взрыва снаряда, м. |
| `public float recoilPitch` | Подброс камеры вверх за выстрел, градусы. |
| `public float recoilYaw` | Максимальный случайный увод камеры вбок, градусы. |
| `public GameObject tracerPrefab` | Трассер (необязательно). |
| `public GameObject impactPrefab` | Эффект попадания (необязательно). |
| `public AudioClip fireSound` | Звук выстрела. |
| `public AudioClip reloadSound` | Звук перезарядки. |
| `public AudioClip emptySound` | Щелчок пустого магазина. |

**Свойства**

| Член | Описание |
|---|---|
| `public float FireInterval { get; }` | Интервал между выстрелами, сек (из roundsPerMinute). |

### AimData

`Game.Combat.Weapons` · структура · [AimData.cs](../Assets/Game/Scripts/Runtime/Combat/Weapons/AimData.cs)

Куда и как стрелять в этом кадре. Заполняется владельцем (игрок/ИИ) и передаётся в оружие.

**Используется в:** [PlayerCombat](#playercombat).

**Поля**

| Член | Описание |
|---|---|
| `public readonly Vector3 AimPoint` | Точка в мире, куда целится стрелок. |
| `public readonly GameObject Owner` | Стрелок (его коллайдеры игнорируются выстрелом). |
| `public readonly bool IsAiming` | Прицеливание (меньший разброс). |
| `public readonly float SpreadMultiplier` | Множитель разброса от движения, прыжка, приседа. |

**Конструкторы**

| Член | Описание |
|---|---|
| `public AimData(Vector3 aimPoint, GameObject owner, bool isAiming, float spreadMultiplier = 1f)` | Создаёт данные прицеливания. |

### ShotContext

`Game.Combat.Weapons` · структура · [AimData.cs](../Assets/Game/Scripts/Runtime/Combat/Weapons/AimData.cs)

Контекст одного выстрела для `IShotStrategy`.

**Используется в:** [Weapon](#weapon).

**Поля**

| Член | Описание |
|---|---|
| `public readonly WeaponDefinition Definition` | Определение оружия. |
| `public readonly Vector3 Origin` | Откуда летит выстрел (дуло). |
| `public readonly Vector3 AimPoint` | Куда целились. |
| `public readonly GameObject Owner` | Стрелок. |
| `public readonly float SpreadDegrees` | Итоговый разброс в градусах. |

**Свойства**

| Член | Описание |
|---|---|
| `public Vector3 BaseDirection { get; }` | Направление от дула к точке прицела без разброса. |
| `public Transform OwnerRoot { get; }` | Transform стрелка (или null). |

**Конструкторы**

| Член | Описание |
|---|---|
| `public ShotContext(WeaponDefinition definition, Vector3 origin, Vector3 aimPoint, GameObject owner, float spreadDegrees)` | Создаёт контекст выстрела. |

### IFireMode

`Game.Combat.Weapons` · интерфейс · [FireModes.cs](../Assets/Game/Scripts/Runtime/Combat/Weapons/FireModes/FireModes.cs) · паттерн **Strategy**

Паттерн Strategy: КОГДА оружие стреляет (полуавтомат, автомат, очередь). Получает состояние спускового крючка и просит оружие выстрелить через Weapon.TryFire().

**Используется в:** [BurstFireMode](#burstfiremode), [FullAutoFireMode](#fullautofiremode), [SemiAutoFireMode](#semiautofiremode), [Weapon](#weapon).

**Методы**

| Член | Описание |
|---|---|
| `void Tick(Weapon weapon, bool triggerPressed, bool triggerHeld, in AimData aim, float deltaTime)` | Решает, стрелять ли в этом кадре, и вызывает weapon.TryFire(). |

### SemiAutoFireMode

`Game.Combat.Weapons` · sealed класс · [FireModes.cs](../Assets/Game/Scripts/Runtime/Combat/Weapons/FireModes/FireModes.cs)

Базовые типы: [IFireMode](#ifiremode)

Один выстрел на одно нажатие. Нажатие во время кулдауна запоминается ненадолго.

**Связи:** реализует [IFireMode](#ifiremode).

**Методы**

| Член | Описание |
|---|---|
| `public void Tick(Weapon weapon, bool triggerPressed, bool triggerHeld, in AimData aim, float deltaTime)` | Решает, стрелять ли в этом кадре, и вызывает weapon.TryFire(). |

### FullAutoFireMode

`Game.Combat.Weapons` · sealed класс · [FireModes.cs](../Assets/Game/Scripts/Runtime/Combat/Weapons/FireModes/FireModes.cs)

Базовые типы: [IFireMode](#ifiremode)

Стреляет, пока зажат курок.

**Связи:** реализует [IFireMode](#ifiremode).

**Методы**

| Член | Описание |
|---|---|
| `public void Tick(Weapon weapon, bool triggerPressed, bool triggerHeld, in AimData aim, float deltaTime)` | Решает, стрелять ли в этом кадре, и вызывает weapon.TryFire(). |

### BurstFireMode

`Game.Combat.Weapons` · sealed класс · [FireModes.cs](../Assets/Game/Scripts/Runtime/Combat/Weapons/FireModes/FireModes.cs)

Базовые типы: [IFireMode](#ifiremode)

Очередь из N выстрелов на одно нажатие, затем пауза.

**Связи:** реализует [IFireMode](#ifiremode).

**Конструкторы**

| Член | Описание |
|---|---|
| `public BurstFireMode(int burstCount, float burstCooldown)` | Создаёт режим очереди из выстрелов с паузой . |

**Методы**

| Член | Описание |
|---|---|
| `public void Tick(Weapon weapon, bool triggerPressed, bool triggerHeld, in AimData aim, float deltaTime)` | Решает, стрелять ли в этом кадре, и вызывает weapon.TryFire(). |

### IShotStrategy

`Game.Combat.Weapons` · интерфейс · [ShotStrategies.cs](../Assets/Game/Scripts/Runtime/Combat/Weapons/Shots/ShotStrategies.cs) · паттерн **Strategy**

Паттерн Strategy: ЧТО происходит при выстреле (луч, дробь, снаряд). Новый тип выстрела (лазер, огнемёт) = новый класс + строка в WeaponFactory.

**Используется в:** [HitscanShot](#hitscanshot), [ProjectileShot](#projectileshot), [SpreadShot](#spreadshot), [Weapon](#weapon).

**Методы**

| Член | Описание |
|---|---|
| `void Fire(in ShotContext context)` | Выполнить выстрел. |

### HitscanShot

`Game.Combat.Weapons` · sealed класс · [ShotStrategies.cs](../Assets/Game/Scripts/Runtime/Combat/Weapons/Shots/ShotStrategies.cs)

Базовые типы: [IShotStrategy](#ishotstrategy)

Мгновенный луч (пистолет, винтовка).

**Связи:** реализует [IShotStrategy](#ishotstrategy); использует [ShotUtility](#shotutility).

**Методы**

| Член | Описание |
|---|---|
| `public void Fire(in ShotContext context)` | Выполнить выстрел. |

### SpreadShot

`Game.Combat.Weapons` · sealed класс · [ShotStrategies.cs](../Assets/Game/Scripts/Runtime/Combat/Weapons/Shots/ShotStrategies.cs)

Базовые типы: [IShotStrategy](#ishotstrategy)

Несколько лучей-дробинок за выстрел (дробовик).

**Связи:** реализует [IShotStrategy](#ishotstrategy); использует [ShotUtility](#shotutility).

**Методы**

| Член | Описание |
|---|---|
| `public void Fire(in ShotContext context)` | Выполнить выстрел. |

### ProjectileShot

`Game.Combat.Weapons` · sealed класс · [ShotStrategies.cs](../Assets/Game/Scripts/Runtime/Combat/Weapons/Shots/ShotStrategies.cs)

Базовые типы: [IShotStrategy](#ishotstrategy)

Физический снаряд из пула (гранатомёт, ракета).

**Связи:** реализует [IShotStrategy](#ishotstrategy); использует [Projectile](#projectile) (спавнит).

**Методы**

| Член | Описание |
|---|---|
| `public void Fire(in ShotContext context)` | Выполнить выстрел. |

### ShotUtility

`Game.Combat.Weapons` · static класс · [ShotStrategies.cs](../Assets/Game/Scripts/Runtime/Combat/Weapons/Shots/ShotStrategies.cs)

Общий код лучевых выстрелов: рейкаст, урон, сила удара, эффекты.

**Связи:** использует [IDamageable](#idamageable); использует [Tracer](#tracer).

**Используется в:** [HitscanShot](#hitscanshot), [SpreadShot](#spreadshot).

**Методы**

| Член | Описание |
|---|---|
| `public static void FireRay(in ShotContext context, Vector3 direction, float damage)` | Луч по направлению : урон, импульс, эффект попадания, трассер. |

### Weapon

`Game.Combat.Weapons` · sealed класс · [Weapon.cs](../Assets/Game/Scripts/Runtime/Combat/Weapons/Weapon.cs)

Экземпляр оружия в руках (модель): патроны, перезарядка, кулдаун, разброс. Обычный C#-класс, не MonoBehaviour — его тикает владелец (WeaponInventory или турель). Поведение делегируется стратегиям `IFireMode` и `IShotStrategy`.

**Связи:** ссылается на [WeaponDefinition](#weapondefinition) (Definition); ссылается на [WeaponView](#weaponview) (View); владеет [IFireMode](#ifiremode); ссылается на [IShotStrategy](#ishotstrategy); использует [ShotContext](#shotcontext).

**Используется в:** [EnemyTurret](#enemyturret), [WeaponFactory](#weaponfactory), [WeaponInventory](#weaponinventory).

**Свойства**

| Член | Описание |
|---|---|
| `public WeaponDefinition Definition { get; }` | Данные оружия. |
| `public WeaponView View { get; }` | Модель оружия (может быть null у ИИ без визуала). |
| `public int AmmoInMagazine { get; }` | Патронов в магазине. |
| `public int ReserveAmmo { get; }` | Патронов в запасе. |
| `public bool InfiniteReserve { get; set; }` | Бесконечный запас (для ИИ). |
| `public bool IsReloading { get; }` | Идёт перезарядка. |
| `public float ReloadProgress { get; }` | Прогресс перезарядки 0..1. |
| `public bool IsEmpty { get; }` | Магазин пуст и перезарядка не идёт. |
| `public bool IsReadyToFire { get; }` | Можно выстрелить прямо сейчас. |
| `public float CurrentSpread { get; }` | Текущий разброс в градусах (база от прицеливания + "раскрытие" от стрельбы). |

**События**

| Член | Описание |
|---|---|
| `public event Action<Weapon> Fired` | Произведён выстрел. |
| `public event Action<Weapon> DryFired` | Попытка выстрела с пустым магазином. |
| `public event Action<Weapon> AmmoChanged` | Изменились патроны в магазине или запасе. |
| `public event Action<Weapon> ReloadStarted` | Началась перезарядка. |
| `public event Action<Weapon> ReloadFinished` | Перезарядка завершена. |

**Конструкторы**

| Член | Описание |
|---|---|
| `public Weapon(WeaponDefinition definition, WeaponView view, IFireMode fireMode, IShotStrategy shot)` | Создаёт оружие с полным магазином и стартовым запасом из определения. |

**Методы**

| Член | Описание |
|---|---|
| `public void Tick(float deltaTime, bool triggerPressed, bool triggerHeld, in AimData aim)` | Вызывается владельцем каждый кадр, пока оружие в руках. |
| `public bool TryFire(in AimData aim)` | Попытка одного выстрела. Вызывается режимом огня. |
| `public bool TryReload()` | Начать перезарядку, если магазин не полон и есть запас. |
| `public void CancelReload()` | Прервать перезарядку (патроны не переносятся). |
| `public void Holster()` | Убрать оружие: сброс режима огня и перезарядки. |
| `public void Draw()` | Достать оружие: показать модель. |
| `public int AddReserveAmmo(int amount)` | Добавить патроны в запас. Возвращает, сколько реально добавлено. |

### WeaponView

`Game.Combat.Weapons` · sealed класс · [WeaponView.cs](../Assets/Game/Scripts/Runtime/Combat/Weapons/WeaponView.cs)

Базовые типы: `MonoBehaviour`

Визуал оружия (View в связке Weapon-модель / WeaponView-представление): модель, дуло, вспышка. Ничего не знает о патронах и уроне. Ось +Z модели = направление ствола.

**Используется в:** [Weapon](#weapon).

**Настройки инспектора**

| Член | Описание |
|---|---|
| `private Transform muzzle` | Точка дула (откуда летят пули). |
| `private GameObject muzzleFlash` | Объект вспышки (включается на время выстрела). |
| `private float muzzleFlashDuration` | Длительность вспышки, сек. |
| `private AudioSource audioSource` | Источник звука. Пусто — AudioSource.PlayClipAtPoint. |

**Свойства**

| Член | Описание |
|---|---|
| `public Transform Muzzle { get; }` | Точка дула (или сам объект, если не задана). |

**Методы**

| Член | Описание |
|---|---|
| `public void Configure(Transform muzzlePoint, GameObject flash)` | Задаёт дуло и вспышку. Вызывать до активации объекта. |
| `public void SetVisible(bool visible)` | Показать/спрятать модель. |
| `public void PlayFireEffects(AudioClip clip)` | Вспышка и звук выстрела. |
| `public void PlaySound(AudioClip clip)` | Проиграть звук, если он задан. |
| `public static WeaponView CreatePlaceholder(Transform parent, string name)` | Запасной визуал, если у оружия нет префаба: кубик-ствол из примитивов. |

### WeaponFactory

`Game.Combat.Weapons` · static класс · [WeaponFactory.cs](../Assets/Game/Scripts/Runtime/Combat/Weapons/WeaponFactory.cs) · паттерн **Factory**

Паттерн Factory: собирает рабочее оружие из данных `WeaponDefinition` — создаёт визуал и подбирает стратегии режима огня и выстрела по enum-ам.

**Связи:** использует [Weapon](#weapon) (создаёт).

**Используется в:** [EnemyTurret](#enemyturret), [WeaponInventory](#weaponinventory).

**Методы**

| Член | Описание |
|---|---|
| `public static Weapon Create(WeaponDefinition definition, Transform parent)` | Создаёт модель оружия внутри и оружие с подходящими стратегиями. |
| `public static IFireMode CreateFireMode(WeaponDefinition definition)` | Стратегия режима огня по полю fireMode. |
| `public static IShotStrategy CreateShot(WeaponDefinition definition)` | Стратегия выстрела по полю shotType (общие экземпляры). |

### WeaponInventory

`Game.Combat.Weapons` · sealed класс · [WeaponInventory.cs](../Assets/Game/Scripts/Runtime/Combat/Weapons/WeaponInventory.cs)

Базовые типы: `MonoBehaviour`

Слоты оружия персонажа: выдача стартового оружия, переключение, подбор, патроны. Само оружие тикает владелец (PlayerCombat), инвентарь только хранит и переключает.

**Связи:** владеет [Weapon](#weapon) (0..4); использует [WeaponFactory](#weaponfactory).

**Используется в:** [PlayerCombat](#playercombat).

**Настройки инспектора**

| Член | Описание |
|---|---|
| `private WeaponDefinition[] startingWeapons` | Оружие, выдаваемое при появлении. |
| `private Transform weaponSocket` | Куда крепится модель оружия (обычно WeaponSocket на груди/руке) |
| `private int maxWeapons` | Максимум слотов. |

**Свойства**

| Член | Описание |
|---|---|
| `public IReadOnlyList<Weapon> Weapons { get; }` | Всё оружие в слотах. |
| `public int CurrentIndex { get; }` | Индекс оружия в руках (-1 — нет). |
| `public Weapon Current { get; }` | Оружие в руках (или null). |

**События**

| Член | Описание |
|---|---|
| `public event Action<Weapon> WeaponEquipped` | Сменилось оружие в руках. |
| `public event Action<Weapon> WeaponAdded` | Добавлено новое оружие. |

**Методы**

| Член | Описание |
|---|---|
| `public void Configure(WeaponDefinition[] weapons, Transform socket)` | Настройка из кода. Вызывать до активации объекта (до Awake). |
| `public Weapon AddWeapon(WeaponDefinition definition)` | Добавить оружие. Если такое уже есть — добавляются патроны. |
| `public bool Equip(int index)` | Взять оружие из слота . false — слот пуст или уже в руках. |
| `public void EquipNext()` | Следующее оружие по кругу. |
| `public void EquipPrevious()` | Предыдущее оружие по кругу. |
| `public int AddAmmoToAll(int magazines)` | Добавить всему оружию по magazines магазинов. Возвращает суммарно добавленное. |

### IAimProvider

`Game.Combat.Weapons` · интерфейс · [WeaponSocket.cs](../Assets/Game/Scripts/Runtime/Combat/Weapons/WeaponSocket.cs)

Источник точки прицеливания для визуала (сокет оружия, IK головы).

**Используется в:** [AimIKHandler](#aimikhandler), [PlayerCombat](#playercombat), [WeaponSocket](#weaponsocket).

**Свойства**

| Член | Описание |
|---|---|
| `bool IsCombatReady { get; }` | Персонаж целится или стреляет — оружие и голова смотрят в точку прицела. |
| `Vector3 AimPoint { get; }` | Точка в мире под прицелом. |

### WeaponSocket

`Game.Combat.Weapons` · sealed класс · [WeaponSocket.cs](../Assets/Game/Scripts/Runtime/Combat/Weapons/WeaponSocket.cs)

Базовые типы: `MonoBehaviour`

Атрибуты: `[DefaultExecutionOrder(200)]`

Точка крепления оружия. В бою поворачивает ствол в точку прицела, вне боя — держит оружие в "походном" положении. Работает после анимации (LateUpdate), поэтому можно вешать сокет на кость скелета (грудь/правая рука).

**Связи:** ссылается на [IAimProvider](#iaimprovider).

**Настройки инспектора**

| Член | Описание |
|---|---|
| `private Vector3 restEulerAngles` | Поворот оружия вне боя относительно персонажа. |
| `private float rotationSharpness` | Резкость поворота к цели (больше — быстрее). |

## Пакет AI

`Game.AI` — Враги и мишени.

### EnemyTurret

`Game.AI` · sealed класс · [EnemyTurret.cs](../Assets/Game/Scripts/Runtime/AI/EnemyTurret.cs)

Базовые типы: `MonoBehaviour`

Атрибуты: `[RequireComponent(typeof(Health))]`

Простой враг-турель: ищет игрока, поворачивается к нему и стреляет очередями. Показывает переиспользование систем: тот же `StateMachine`, что у игрока, то же оружие через `WeaponFactory`, тот же `Health`.

**Связи:** владеет [StateMachine](#statemachine); владеет [Weapon](#weapon); использует [WeaponFactory](#weaponfactory); ссылается на [Health](#health); использует [PlayerCharacter](#playercharacter) (цель).

**Используется в:** [DemoContentFactory](#democontentfactory), [TurretAttackState](#turretattackstate), [TurretDeadState](#turretdeadstate), [TurretIdleState](#turretidlestate).

**Настройки инспектора**

| Член | Описание |
|---|---|
| `private Transform yawPivot` | Часть, поворачивающаяся по горизонтали. |
| `private Transform pitchPivot` | Часть, наклоняющаяся по вертикали (внутри yawPivot). |
| `private Transform weaponSocket` | Куда крепится модель оружия. |
| `private WeaponDefinition weapon` | Оружие турели. |
| `private float extraSpread` | Дополнительный разброс ИИ, чтобы он не был снайпером |
| `private float detectionRange` | Дальность обнаружения игрока, м. |
| `private float turnSpeed` | Скорость поворота башни, градусов в секунду. |
| `private float fireAngleTolerance` | Стрелять, только если ствол отклонён от цели меньше чем на столько градусов. |
| `private float idleScanSpeed` | Скорость вращения при поиске цели, градусов в секунду. |
| `private float respawnDelay` | Через сколько секунд уничтоженная турель восстанавливается. |
| `private LayerMask sightMask` | Слои, которые перекрывают обзор. |

**Свойства**

| Член | Описание |
|---|---|
| `public Health Health { get; }` | Здоровье турели. |
| `public Weapon Weapon { get; }` | Экземпляр оружия турели (бесконечные патроны). |
| `public float IdleScanSpeed { get; }` | Скорость вращения при поиске цели. |
| `public float RespawnDelay { get; }` | Задержка восстановления после уничтожения. |
| `public string CurrentStateName { get; }` | Имя текущего состояния ИИ (для отладки). |
| `public Vector3 TargetPoint { get; }` | Точка, в которую целиться (уровень плеч игрока). |

**Методы**

| Член | Описание |
|---|---|
| `public void Configure(Transform yaw, Transform pitch, Transform socket, WeaponDefinition weaponDefinition)` | Настройка из кода. Вызывать до активации объекта (до Awake). |
| `public bool CanSeeTarget()` | Игрок жив, в радиусе и не закрыт препятствием. |
| `public float AimAt(Vector3 point, float deltaTime)` | Повернуть башню к точке. Возвращает угол между стволом и целью. |
| `public void Scan(float deltaTime)` | Медленно вращает башню в поиске цели. |
| `public void TickWeapon(bool trigger, float deltaTime, float aimError)` | Тик оружия: trigger — держать ли курок. |
| `public void SetVisible(bool visible)` | Показать/скрыть турель вместе с коллайдерами (мёртвую нельзя "добивать"). |

### TurretIdleState

`Game.AI.States` · sealed класс · [TurretStates.cs](../Assets/Game/Scripts/Runtime/AI/States/TurretStates.cs)

Базовые типы: [IState](#istate)

Нет цели: башня медленно вращается.

**Связи:** реализует [IState](#istate); ссылается на [EnemyTurret](#enemyturret).

**Свойства**

| Член | Описание |
|---|---|
| `public string Name { get; }` | Имя для отладки и HUD. |

**Конструкторы**

| Член | Описание |
|---|---|
| `public TurretIdleState(EnemyTurret turret)` | Создаёт состояние для турели . |

**Методы**

| Член | Описание |
|---|---|
| `public void Enter()` | Вход в состояние. |
| `public void Exit()` | Выход из состояния. |
| `public void Tick(float deltaTime)` | Вращает башню и держит курок отпущенным. |

### TurretAttackState

`Game.AI.States` · sealed класс · [TurretStates.cs](../Assets/Game/Scripts/Runtime/AI/States/TurretStates.cs)

Базовые типы: [IState](#istate)

Цель видна: доворачиваемся и стреляем очередями с паузами.

**Связи:** реализует [IState](#istate); ссылается на [EnemyTurret](#enemyturret).

**Свойства**

| Член | Описание |
|---|---|
| `public string Name { get; }` | Имя для отладки и HUD. |

**Конструкторы**

| Член | Описание |
|---|---|
| `public TurretAttackState(EnemyTurret turret)` | Создаёт состояние для турели . |

**Методы**

| Член | Описание |
|---|---|
| `public void Enter()` | Сбрасывает таймер: перед первой очередью есть время реакции. |
| `public void Exit()` | Выход из состояния. |
| `public void Tick(float deltaTime)` | Наводится на цель и стреляет очередями с паузами. |

### TurretDeadState

`Game.AI.States` · sealed класс · [TurretStates.cs](../Assets/Game/Scripts/Runtime/AI/States/TurretStates.cs)

Базовые типы: [IState](#istate)

Уничтожена: скрыта, через RespawnDelay восстанавливается.

**Связи:** реализует [IState](#istate); ссылается на [EnemyTurret](#enemyturret).

**Свойства**

| Член | Описание |
|---|---|
| `public string Name { get; }` | Имя для отладки и HUD. |

**Конструкторы**

| Член | Описание |
|---|---|
| `public TurretDeadState(EnemyTurret turret)` | Создаёт состояние для турели . |

**Методы**

| Член | Описание |
|---|---|
| `public void Enter()` | Скрывает турель и запускает таймер восстановления. |
| `public void Exit()` | Показывает турель снова. |
| `public void Tick(float deltaTime)` | Отсчитывает время и восстанавливает здоровье. |

### TargetDummy

`Game.AI` · sealed класс · [TargetDummy.cs](../Assets/Game/Scripts/Runtime/AI/TargetDummy.cs)

Базовые типы: `MonoBehaviour`

Атрибуты: `[RequireComponent(typeof(Health))]`

Мишень для тренировки: вспыхивает при попадании, "умирает" (скрывается) и возрождается. Голова — отдельный коллайдер с `Hitbox` (×2 урона).

**Связи:** ссылается на [Health](#health).

**Настройки инспектора**

| Член | Описание |
|---|---|
| `private float respawnDelay` | Через сколько секунд мишень появляется снова. |
| `private Color flashColor` | Цвет вспышки при попадании. |
| `private float flashDuration` | Длительность вспышки, сек. |

## Пакет Interaction

`Game.Interaction` — Подбираемые предметы и объекты уровня.

### Pickup

`Game.Interaction` · abstract класс · [Pickup.cs](../Assets/Game/Scripts/Runtime/Interaction/Pickup.cs) · паттерн **Template Method**

Базовые типы: `MonoBehaviour`

Атрибуты: `[RequireComponent(typeof(Collider))]`

Базовый подбираемый предмет. Паттерн Template Method: общий алгоритм (вошёл игрок → TryApply → спрятать → появиться снова) в базе, а эффект — в наследнике. Нужен Collider с IsTrigger.

**Связи:** использует [PlayerCharacter](#playercharacter).

**Используется в:** [AmmoPickup](#ammopickup), [HealthPickup](#healthpickup).

**Настройки инспектора**

| Член | Описание |
|---|---|
| `private float respawnTime` | Через сколько секунд предмет появляется снова. |
| `private Transform visual` | Визуал, который вращается и прячется после подбора. |
| `private float spinSpeed` | Скорость вращения, градусов в секунду. |
| `private float bobHeight` | Амплитуда покачивания вверх-вниз, м. |

**Методы**

| Член | Описание |
|---|---|
| `public void Configure(Transform visualRoot)` | Объект, который крутится и прячется после подбора. |
| `protected abstract bool TryApply(PlayerCharacter player)` | Применить эффект. false — предмет не нужен (полное здоровье и т.п.) и остаётся лежать. |

### HealthPickup

`Game.Interaction` · sealed класс · [HealthPickup.cs](../Assets/Game/Scripts/Runtime/Interaction/HealthPickup.cs)

Базовые типы: [Pickup](#pickup)

Аптечка: лечит, если здоровье не полное.

**Связи:** наследует [Pickup](#pickup).

**Настройки инспектора**

| Член | Описание |
|---|---|
| `private float amount` | Сколько здоровья восстанавливает. |

**Методы**

| Член | Описание |
|---|---|
| `protected override bool TryApply(PlayerCharacter player)` | Лечит; false — здоровье уже полное. |

### AmmoPickup

`Game.Interaction` · sealed класс · [AmmoPickup.cs](../Assets/Game/Scripts/Runtime/Interaction/AmmoPickup.cs)

Базовые типы: [Pickup](#pickup)

Патроны: добавляет магазины всему оружию игрока.

**Связи:** наследует [Pickup](#pickup).

**Настройки инспектора**

| Член | Описание |
|---|---|
| `private int magazines` | Сколько магазинов добавить каждому оружию. |

**Методы**

| Член | Описание |
|---|---|
| `protected override bool TryApply(PlayerCharacter player)` | Добавляет патроны; false — всё оружие уже заполнено. |

### JumpPad

`Game.Interaction` · sealed класс · [JumpPad.cs](../Assets/Game/Scripts/Runtime/Interaction/JumpPad.cs)

Базовые типы: `MonoBehaviour`

Атрибуты: `[RequireComponent(typeof(Collider))]`

Батут: подбрасывает игрока и восстанавливает прыжки в воздухе (развитие идеи JumpableSurface из старого наброска). Нужен Collider с IsTrigger.

**Связи:** использует [PlayerCharacter](#playercharacter) (Launch()).

**Настройки инспектора**

| Член | Описание |
|---|---|
| `private Vector3 launchVelocity` | Скорость запуска в локальных координатах батута |

**Методы**

| Член | Описание |
|---|---|
| `public void Configure(Vector3 localLaunchVelocity)` | Задаёт скорость запуска. Вызывать до активации объекта. |

## Пакет Gameplay

`Game.Gameplay` — Правила игры: появление игрока.

### PlayerSpawner

`Game.Gameplay` · sealed класс · [PlayerSpawner.cs](../Assets/Game/Scripts/Runtime/Gameplay/PlayerSpawner.cs)

Базовые типы: `MonoBehaviour`

Появление и возрождение игрока. Если игрок уже стоит в сцене — использует его, иначе создаёт из префаба. Сообщает о появлении/смерти через EventBus.

**Связи:** ссылается на [PlayerCharacter](#playercharacter); использует [EventBus](#eventbus) (публикует); использует [ICameraRig](#icamerarig) (SetTarget).

**Используется в:** [DemoLevelBuilder](#demolevelbuilder).

**Настройки инспектора**

| Член | Описание |
|---|---|
| `private PlayerCharacter playerPrefab` | Префаб игрока (если игрока нет в сцене). |
| `private Transform spawnPoint` | Точка появления. Пусто — позиция спавнера. |
| `private float respawnDelay` | Задержка возрождения после смерти, сек. |

**Свойства**

| Член | Описание |
|---|---|
| `public PlayerCharacter Player { get; }` | Текущий игрок. |

**Методы**

| Член | Описание |
|---|---|
| `public void Configure(PlayerCharacter prefab, Transform spawn)` | Настройка из кода. Вызывать до активации объекта. |

## Пакет UI

`Game.UI` — HUD (MVP) и пауза.

### IHudView

`Game.UI` · интерфейс · [IHudView.cs](../Assets/Game/Scripts/Runtime/UI/IHudView.cs)

View в паттерне MVP: только отображение, никакой логики. Сейчас реализован на IMGUI (`ImGuiHudView`), можно сделать реализацию на uGUI/TextMeshPro/UI Toolkit, не трогая `HudPresenter`.

**Используется в:** [HudPresenter](#hudpresenter), [ImGuiHudView](#imguihudview).

**Методы**

| Член | Описание |
|---|---|
| `void SetHealth(float current, float max)` | Показать здоровье. |
| `void SetWeapon(string weaponName, int magazine, int magazineSize, int reserve)` | weaponName == null — оружия нет. |
| `void SetReload(bool reloading, float progress01)` | Показать/скрыть полосу перезарядки. |
| `void SetCrosshair(float spreadPixels, bool aiming)` | Размер прицела (радиус разброса в пикселях) и режим прицеливания. |
| `void ShowHitMarker(bool killed, bool critical)` | Хит-маркер попадания (красный — убийство). |
| `void ShowDamageFlash(float strength01)` | Красная вспышка экрана при получении урона. |
| `void SetRespawnCountdown(float seconds)` | Отсчёт до возрождения (0 — скрыть). |
| `void SetDebugText(string text)` | Отладочный текст в углу экрана. |

### HudPresenter

`Game.UI` · sealed класс · [HudPresenter.cs](../Assets/Game/Scripts/Runtime/UI/HudPresenter.cs) · паттерн **MVP**

Базовые типы: `MonoBehaviour`

Presenter в паттерне MVP: слушает модель (Health, Weapon, EventBus) и обновляет `IHudView`. Дискретные вещи (урон, патроны, хит-маркер) — через события, непрерывные (разброс, перезарядка) — в Update.

**Связи:** ссылается на [IHudView](#ihudview) (view); ссылается на [PlayerCharacter](#playercharacter) (model); использует [EventBus](#eventbus) (подписка).

**Настройки инспектора**

| Член | Описание |
|---|---|
| `private float fullFlashDamage` | Урон, при котором красная вспышка экрана максимальна |
| `private bool showDebugInfo` | Показывать состояние и скорость игрока. |

### ImGuiHudView

`Game.UI` · sealed класс · [ImGuiHudView.cs](../Assets/Game/Scripts/Runtime/UI/ImGuiHudView.cs)

Базовые типы: `MonoBehaviour`, [IHudView](#ihudview)

Реализация HUD на IMGUI (OnGUI): не требует Canvas и префабов, удобно для прототипа. Для релиза замените на uGUI/TextMeshPro-реализацию `IHudView`.

**Связи:** реализует [IHudView](#ihudview).

**Настройки инспектора**

| Член | Описание |
|---|---|
| `private bool showControlsHelp` | Показывать подсказку по управлению. |
| `private Color crosshairColor` | Цвет прицела. |
| `private Color hitColor` | Цвет хит-маркера. |
| `private Color killColor` | Цвет хит-маркера при убийстве. |

**Методы**

| Член | Описание |
|---|---|
| `public void SetHealth(float current, float max)` | Показать здоровье. |
| `public void SetWeapon(string weaponName, int magazine, int magazineSize, int reserve)` | weaponName == null — оружия нет. |
| `public void SetReload(bool reloading, float progress01)` | Показать/скрыть полосу перезарядки. |
| `public void SetCrosshair(float spreadPixels, bool aiming)` | Размер прицела (радиус разброса в пикселях) и режим прицеливания. |
| `public void ShowHitMarker(bool killed, bool critical)` | Хит-маркер попадания (красный — убийство). |
| `public void ShowDamageFlash(float strength01)` | Красная вспышка экрана при получении урона. |
| `public void SetRespawnCountdown(float seconds)` | Отсчёт до возрождения (0 — скрыть). |
| `public void SetDebugText(string text)` | Отладочный текст в углу экрана. |

### PauseMenu

`Game.UI` · sealed класс · [PauseMenu.cs](../Assets/Game/Scripts/Runtime/UI/PauseMenu.cs)

Базовые типы: `MonoBehaviour`

Пауза по кнопке Pause (Esc): останавливает время, отключает геймплейный ввод, освобождает курсор и публикует `GamePausedEvent`. Также управляет захватом курсора.

**Связи:** использует [IPlayerInput](#iplayerinput); использует [EventBus](#eventbus) (публикует).

**Свойства**

| Член | Описание |
|---|---|
| `public bool IsPaused { get; }` | Включена ли пауза. |

**Методы**

| Член | Описание |
|---|---|
| `public void SetPaused(bool paused)` | Включить/выключить паузу. |

## Пакет Demo

`Game.Demo` — Демо-контент из примитивов: фабрики, уровень, сцена Playground (рантайм).

### IMaterialProvider

`Game.Demo` · интерфейс · [MaterialProviders.cs](../Assets/Game/Scripts/Runtime/Demo/MaterialProviders.cs) · паттерн **Strategy**

Откуда брать материалы для примитивов (паттерн Strategy): в редакторе — ассеты .mat (AssetMaterialProvider в Game.Editor), в рантайме — материалы в памяти (`RuntimeMaterialProvider`). Фабрики контента работают с любым из них одинаково.

**Используется в:** [AssetMaterialProvider](#assetmaterialprovider), [DemoContentFactory](#democontentfactory), [DemoLevelBuilder](#demolevelbuilder), [RuntimeMaterialProvider](#runtimematerialprovider).

**Методы**

| Член | Описание |
|---|---|
| `Material Lit(string name, Color color, float smoothness = 0.25f)` | Материал с освещением. |
| `Material Emissive(string name, Color color, float intensity = 2f)` | Светящийся материал. |
| `Material Unlit(string name, Color color)` | Материал без освещения. |
| `Material Checker(string name, Color a, Color b, float tiling)` | Материал с текстурой-шахматкой. |

### MaterialFactory

`Game.Demo` · static класс · [MaterialProviders.cs](../Assets/Game/Scripts/Runtime/Demo/MaterialProviders.cs)

Создание материалов URP (Lit/Unlit) с запасным вариантом на встроенные шейдеры.

**Используется в:** [RuntimeMaterialProvider](#runtimematerialprovider).

**Методы**

| Член | Описание |
|---|---|
| `public static Material CreateLit(string name, Color color, float smoothness)` | Новый материал URP Lit (или Standard). |
| `public static Material CreateEmissive(string name, Color color, float intensity)` | Новый светящийся Lit-материал. |
| `public static Material CreateUnlit(string name, Color color)` | Новый материал URP Unlit (или Unlit/Color). |
| `public static Material CreateChecker(string name, Texture2D texture, float tiling)` | Новый Lit-материал с текстурой и тайлингом. |
| `public static Texture2D CreateCheckerTexture(Color a, Color b)` | Текстура-"шахматка" с тонкой сеткой: на ней хорошо видно скорость движения. |
| `public static void SetColor(Material material, Color color)` | Задаёт цвет и для URP (_BaseColor), и для встроенных шейдеров (_Color). |

### RuntimeMaterialProvider

`Game.Demo` · sealed класс · [MaterialProviders.cs](../Assets/Game/Scripts/Runtime/Demo/MaterialProviders.cs)

Базовые типы: [IMaterialProvider](#imaterialprovider)

Материалы в памяти с кэшем по имени. Используется сценой Playground (без генерации ассетов).

**Связи:** реализует [IMaterialProvider](#imaterialprovider); использует [MaterialFactory](#materialfactory).

**Используется в:** [PlaygroundBootstrap](#playgroundbootstrap).

**Конструкторы**

| Член | Описание |
|---|---|
| `public RuntimeMaterialProvider(HideFlags hideFlags = HideFlags.None)` | Создаёт провайдер; назначаются всем созданным материалам.<br>Параметры: `hideFlags` — Для превью в редакторе — DontSave, чтобы материалы не попали в сцену. |

**Методы**

| Член | Описание |
|---|---|
| `public Material Lit(string name, Color color, float smoothness = 0.25f)` | Материал с освещением. |
| `public Material Emissive(string name, Color color, float intensity = 2f)` | Светящийся материал. |
| `public Material Unlit(string name, Color color)` | Материал без освещения. |
| `public Material Checker(string name, Color a, Color b, float tiling)` | Материал с текстурой-шахматкой. |
| `public void DestroyAll()` | Уничтожить созданные материалы (при пересборке превью). |

### Primitives

`Game.Demo` · static класс · [Primitives.cs](../Assets/Game/Scripts/Runtime/Demo/Primitives.cs)

Создание примитивов (куб, сфера, капсула) для визуала и геометрии уровня. Работает и в редакторе, и в игре.

**Используется в:** [DemoContentFactory](#democontentfactory).

**Методы**

| Член | Описание |
|---|---|
| `public static GameObject Visual(PrimitiveType type, string name, Transform parent, Vector3 localPosition, Vector3 localScale, Material material)` | Примитив без коллайдера (визуал). |
| `public static GameObject Solid(PrimitiveType type, string name, Transform parent, Vector3 localPosition, Vector3 localScale, Material material)` | Примитив с коллайдером. |
| `public static Transform Child(string name, Transform parent, Vector3 localPosition)` | Пустой дочерний объект в локальной позиции. |
| `public static GameObject Inactive(string name, Transform parent = null)` | Неактивный объект: компоненты можно добавить и настроить через Configure() до их Awake. |

### MannequinBuilder

`Game.Demo` · sealed класс · [MannequinBuilder.cs](../Assets/Game/Scripts/Runtime/Demo/MannequinBuilder.cs) · паттерн **Builder**

Паттерн Builder: строит "манекен" из примитивов на скелете с именами костей Mixamo (mixamorig:Hips, mixamorig:Spine...) и создаёт для него Humanoid Avatar. Благодаря Humanoid-ретаргетингу на манекене проигрывается ЛЮБАЯ Humanoid-анимация Mixamo, а сам манекен можно заменить на настоящую модель с тем же Animator Controller.

**Используется в:** [DemoContentFactory](#democontentfactory).

**Константы**

| Член | Описание |
|---|---|
| `public const string BonePrefix` | Префикс имён костей Mixamo. |

**Методы**

| Член | Описание |
|---|---|
| `public MannequinBuilder WithMaterials(Material body, Material joints, Material accent)` | Задаёт материалы тела, суставов и акцентов. |
| `public GameObject Build(string name)` | Создаёт объект-модель (корень + скелет + меши). Animator вешается снаружи. |
| `public Transform GetBone(string shortName)` | Кость по короткому имени без префикса (например, "Spine2"). |
| `public static Avatar CreateAvatar(GameObject root)` | Humanoid Avatar по скелету манекена (AvatarBuilder — рантайм-API, работает и в игре). root — объект, на котором будет Animator. В редакторе Avatar дополнительно сохраняется ассетом. |

### DemoWeapons

`Game.Demo` · sealed класс · [DemoWeaponCatalog.cs](../Assets/Game/Scripts/Runtime/Demo/DemoWeaponCatalog.cs)

Набор оружия демо: стартовое оружие игрока и оружие турели.

**Используется в:** [DemoWeaponCatalog](#demoweaponcatalog).

**Поля**

| Член | Описание |
|---|---|
| `public WeaponDefinition Pistol, Rifle, Shotgun, Launcher, TurretRifle` | Определения оружия демо. |

**Свойства**

| Член | Описание |
|---|---|
| `public WeaponDefinition[] PlayerLoadout { get; }` | Стартовое оружие игрока (4 слота). |
| `public WeaponDefinition[] All { get; }` | Всё оружие, включая турельное. |

### DemoWeaponCatalog

`Game.Demo` · static класс · [DemoWeaponCatalog.cs](../Assets/Game/Scripts/Runtime/Demo/DemoWeaponCatalog.cs)

Каталог параметров демо-оружия — одно место с цифрами баланса для генератора ассетов (редактор) и для сцены Playground (рантайм). Модели оружия — из примитивов, размеры тоже здесь.

**Связи:** использует [DemoWeapons](#demoweapons) (создаёт); использует [WeaponDefinition](#weapondefinition) (настраивает).

**Используется в:** [DemoContentFactory](#democontentfactory).

**Константы**

| Член | Описание |
|---|---|
| `public const string Pistol` | Имя пистолета. |
| `public const string Rifle` | Имя автомата. |
| `public const string Shotgun` | Имя дробовика. |
| `public const string Launcher` | Имя гранатомёта. |
| `public const string TurretRifle` | Имя оружия турели. |

**Методы**

| Член | Описание |
|---|---|
| `public static Action<WeaponDefinition> Setup(string weapon)` | Функция, заполняющая WeaponDefinition параметрами оружия . |
| `public static DemoWeapons CreateInMemory()` | Создать определения в памяти (для Playground). Генератор создаёт их ассетами через Setup(). |
| `public static void ConfigurePistol(WeaponDefinition d)` | Пистолет: полуавтомат, луч. |
| `public static void ConfigureRifle(WeaponDefinition d)` | Автомат: автоматический огонь, луч. |
| `public static void ConfigureShotgun(WeaponDefinition d)` | Дробовик: полуавтомат, 9 дробинок. |
| `public static void ConfigureLauncher(WeaponDefinition d)` | Гранатомёт: снаряд с взрывом. |
| `public static void ConfigureTurretRifle(WeaponDefinition d)` | Оружие турели: очереди по 4 выстрела. |

### DemoPrefabs

`Game.Demo` · sealed класс · [DemoContentFactory.cs](../Assets/Game/Scripts/Runtime/Demo/DemoContentFactory.cs)

Шаблоны (префабы или неактивные объекты-образцы) для расстановки по уровню.

**Используется в:** [DemoLevelBuilder](#demolevelbuilder).

**Поля**

| Член | Описание |
|---|---|
| `public GameObject Player, Dummy, Turret, HealthPickup, AmmoPickup, JumpPad` | Шаблоны объектов демо. |

### DemoContentFactory

`Game.Demo` · static класс · [DemoContentFactory.cs](../Assets/Game/Scripts/Runtime/Demo/DemoContentFactory.cs) · паттерн **Factory**

Паттерн Factory: собирает игровые объекты демо из примитивов — игрока на скелете Mixamo, мишень, турель, предметы, модели оружия и эффекты. Один код для двух потребителей: генератор в редакторе сохраняет результат в префабы, сцена Playground создаёт их прямо в игре. Объекты создаются внутри parent; если parent неактивен — Awake компонентов ждёт активации, поэтому все Configure() успевают выполниться до него.

**Связи:** использует [IMaterialProvider](#imaterialprovider); использует [MannequinBuilder](#mannequinbuilder); использует [Primitives](#primitives); использует [DemoWeaponCatalog](#demoweaponcatalog); использует [PlayerCharacter](#playercharacter) (собирает); использует [EnemyTurret](#enemyturret) (собирает).

**Используется в:** [PlaygroundBootstrap](#playgroundbootstrap), [PrefabContentBuilder](#prefabcontentbuilder), [WeaponContentBuilder](#weaponcontentbuilder).

**Методы**

| Член | Описание |
|---|---|
| `public static GameObject BuildTracer(IMaterialProvider materials, Transform parent = null)` | Трассер на LineRenderer. |
| `public static GameObject BuildImpact(IMaterialProvider materials, Transform parent = null)` | Искра попадания (куб, уменьшающийся до нуля). |
| `public static GameObject BuildExplosion(IMaterialProvider materials, Transform parent = null)` | Взрыв: растущая сфера и гаснущий свет. |
| `public static GameObject BuildGrenade(IMaterialProvider materials, GameObject explosion, Transform parent = null)` | Граната со следом; при попадании создаёт . |
| `public static GameObject BuildWeaponView(string weapon, IMaterialProvider materials, Transform parent = null)` | Модель оружия из примитивов: корпус, рукоять, ствол, дуло со вспышкой и светом. |
| `public static void LinkWeapons(DemoWeapons weapons, Func<string, WeaponView> view, GameObject tracer, GameObject impact, GameObject grenade)` | Связать определения оружия с моделями и эффектами. |
| `public static GameObject BuildPlayer(MovementConfig movement, AnimationSet animationSet, WeaponDefinition[] loadout,` | Игрок: CharacterController, все компоненты, манекен с Animator и сокетом оружия.<br>Параметры: `movement` — Настройки передвижения (рост капсулы и т.д.); null — значения по умолчанию.; `animationSet` — Набор анимаций; его контроллер ставится в Animator.; `loadout` — Стартовое оружие.; `materials` — Источник материалов.; `avatarProvider` — Как получить Avatar для модели (в редакторе — ассет, в игре — построить на лету).; `parent` — Родитель; неактивный родитель откладывает Awake компонентов. |
| `public static GameObject BuildDummy(IMaterialProvider materials, Transform parent = null)` | Мишень с хитбоксом головы (×2 урона). |
| `public static GameObject BuildTurret(IMaterialProvider materials, WeaponDefinition weapon, Transform parent = null)` | Турель с оружием . |
| `public static GameObject BuildHealthPickup(IMaterialProvider materials, Transform parent = null)` | Аптечка. |
| `public static GameObject BuildAmmoPickup(IMaterialProvider materials, Transform parent = null)` | Патроны. |
| `public static GameObject BuildJumpPad(IMaterialProvider materials, Transform parent = null)` | Батут. |

### DemoLevelBuilder

`Game.Demo` · sealed класс · [DemoLevelBuilder.cs](../Assets/Game/Scripts/Runtime/Demo/DemoLevelBuilder.cs) · паттерн **Builder**

Паттерн Builder: строит тестовый полигон из примитивов — тир с мишенями, укрытия, тоннель для приседа и подката, лестница, рампа, блоки для прыжков, батут, турель, аптечка, патроны, ящики. Как ставить объекты, решает вызывающий (place): редактор ставит экземпляры префабов, сцена Playground — копии шаблонов. Плюс общие помощники для камеры и систем сцены.

**Связи:** ссылается на [DemoPrefabs](#demoprefabs); ссылается на [IMaterialProvider](#imaterialprovider); использует [PlayerSpawner](#playerspawner) (BuildSystems); использует [ThirdPersonCamera](#thirdpersoncamera) (SetupCamera).

**Используется в:** [DemoSceneBuilder](#demoscenebuilder), [PlaygroundBootstrap](#playgroundbootstrap).

**Свойства**

| Член | Описание |
|---|---|
| `public Transform SpawnPoint { get; }` | Точка появления игрока (создаётся вместе с уровнем). |

**Конструкторы**

| Член | Описание |
|---|---|
| `public DemoLevelBuilder(IMaterialProvider materials, DemoPrefabs prefabs, PlaceFunc place)` | Создаёт строитель уровня с материалами, шаблонами и способом расстановки. |

**Методы**

| Член | Описание |
|---|---|
| `public delegate GameObject PlaceFunc(GameObject template, Vector3 position, Quaternion rotation, Transform parent)` | Поставить шаблон/префаб в точку: (шаблон, позиция, поворот, родитель) → экземпляр. |
| `public Transform Build(Transform parent)` | Строит уровень внутри и возвращает его корень. |
| `public static ThirdPersonCamera SetupCamera(CameraConfig config, Vector3 nearPosition)` | Main Camera + ThirdPersonCamera, рядом с игроком. |
| `public static GameObject BuildSystems(PlayerCharacter playerPrefab, Transform spawnPoint, Transform parent = null)` | Спавнер игрока, HUD и пауза в объекте Systems. |

### PlaygroundBootstrap

`Game.Demo` · sealed класс · [PlaygroundBootstrap.cs](../Assets/Game/Scripts/Runtime/Demo/PlaygroundBootstrap.cs) · паттерн **Composition Root**

Базовые типы: `MonoBehaviour`

Атрибуты: `[ExecuteAlways]`, `[DefaultExecutionOrder(-500)]`

Сцена Playground "из коробки": собирает весь полигон прямо в игре через `DemoContentFactory` и `DemoLevelBuilder` — без генерации ассетов в редакторе. Composition Root сцены. В режиме редактирования строит несохраняемое превью уровня, чтобы сцена не выглядела пустой. Для настоящих префабов и своей сцены используйте Tools → TPS Skeleton → Build Demo Scene.

**Связи:** использует [DemoContentFactory](#democontentfactory); использует [DemoLevelBuilder](#demolevelbuilder); владеет [RuntimeMaterialProvider](#runtimematerialprovider); ссылается на [AnimationSet](#animationset).

**Используется в:** [PlaygroundSceneOpener](#playgroundsceneopener).

**Настройки инспектора**

| Член | Описание |
|---|---|
| `private AnimationSet animationSet` | Клипы Mixamo и Animator Controller игрока |
| `private MovementConfig movementConfig` | Пусто — значения по умолчанию |
| `private CameraConfig cameraConfig` | Пусто — значения по умолчанию |
| `private bool showPreviewInEditor` | Показывать превью полигона в режиме редактирования. |

## Пакет EditorTools

`Game.EditorTools` — Генератор демо: манекен, аниматор, префабы, сцена (только редактор).

### TpsSkeletonMenu

`Game.EditorTools` · static класс · [TpsSkeletonMenu.cs](../Assets/Game/Scripts/Editor/TpsSkeletonMenu.cs) · паттерн **Facade**

Паттерн Facade: одно меню Tools → TPS Skeleton скрывает всю цепочку генерации (конфиги → анимации → оружие → префабы → сцена). Существующие ScriptableObject-ы (настройки, оружие, AnimationSet) НЕ перезаписываются никогда.

**Связи:** использует [CoreAssetsBuilder](#coreassetsbuilder); использует [WeaponContentBuilder](#weaponcontentbuilder); использует [PrefabContentBuilder](#prefabcontentbuilder); использует [DemoSceneBuilder](#demoscenebuilder).

### CoreAssetsBuilder

`Game.EditorTools` · static класс · [CoreAssetsBuilder.cs](../Assets/Game/Scripts/Editor/CoreAssetsBuilder.cs)

Конфиги, AnimationSet (с клипами Mixamo из Assets/Models), Avatar манекена и Animator Controller.

**Связи:** использует [AnimatorControllerBuilder](#animatorcontrollerbuilder).

**Используется в:** [TpsSkeletonMenu](#tpsskeletonmenu).

**Методы**

| Член | Описание |
|---|---|
| `public static MovementConfig LoadMovementConfig()` | Загружает или создаёт Generated/Configs/MovementConfig.asset. |
| `public static CameraConfig LoadCameraConfig()` | Загружает или создаёт Generated/Configs/CameraConfig.asset. |
| `public static AnimationSet LoadAnimationSet(MovementConfig movement)` | Создаёт AnimationSet. При первом создании подставляет Idle и Jogging из исходного проекта. |
| `public static AnimatorController BuildAnimator(AnimationSet set)` | Пересобирает Animator Controller из набора и обновляет ссылку в префабе игрока. |

### AnimatorControllerBuilder

`Game.EditorTools` · sealed класс · [AnimatorControllerBuilder.cs](../Assets/Game/Scripts/Editor/AnimatorControllerBuilder.cs) · паттерн **Builder**

Паттерн Builder: собирает Animator Controller из `AnimationSet`. Base Layer — локомоция (2D blend tree), присед, подкат, прыжок/падение, смерть (+ IK Pass для взгляда). UpperBody — прицел, выстрел, перезарядка, реакция на попадание (маска верхней части тела). Меняете клипы в AnimationSet → жмёте Rebuild → контроллер пересобирается, код не трогается.

**Связи:** использует [AnimationSet](#animationset) (читает клипы).

**Используется в:** [AnimationSetEditor](#animationseteditor), [CoreAssetsBuilder](#coreassetsbuilder).

**Конструкторы**

| Член | Описание |
|---|---|
| `public AnimatorControllerBuilder(AnimationSet set)` | Создаёт строитель для набора анимаций . |

**Методы**

| Член | Описание |
|---|---|
| `public AnimatorController Build(string path, AvatarMask upperBodyMask)` | Пересоздаёт контроллер по пути , записывает ссылку в AnimationSet.controller и возвращает его. |
| `public static AvatarMask LoadOrCreateUpperBodyMask(string path)` | Маска "верх тела" для слоя UpperBody. |

### WeaponContentBuilder

`Game.EditorTools` · static класс · [WeaponContentBuilder.cs](../Assets/Game/Scripts/Editor/WeaponContentBuilder.cs)

Сохраняет в ассеты то, что строит `DemoContentFactory`: эффекты, модели оружия, WeaponDefinition-ы (цифры — из `DemoWeaponCatalog`).

**Связи:** использует [DemoContentFactory](#democontentfactory); использует [WeaponDefinition](#weapondefinition) (создаёт ассеты).

**Используется в:** [TpsSkeletonMenu](#tpsskeletonmenu).

**Методы**

| Член | Описание |
|---|---|
| `public static DemoWeapons Build(bool overwritePrefabs)` | Создаёт эффекты, модели и определения оружия; возвращает набор оружия демо. |
| `public static GameObject Prefab(string path, bool overwrite, Func<GameObject> build)` | Загрузить префаб или (если нет / overwrite) построить и сохранить. |

### PrefabContentBuilder

`Game.EditorTools` · static класс · [PrefabContentBuilder.cs](../Assets/Game/Scripts/Editor/PrefabContentBuilder.cs)

Сохраняет в префабы объекты из `DemoContentFactory`: игрок (манекен на скелете Mixamo), мишень, турель, аптечка, патроны, батут. Avatar манекена сохраняется отдельным ассетом.

**Связи:** использует [DemoContentFactory](#democontentfactory); использует [AssetMaterialProvider](#assetmaterialprovider).

**Используется в:** [TpsSkeletonMenu](#tpsskeletonmenu).

**Методы**

| Член | Описание |
|---|---|
| `public static DemoPrefabs Build(MovementConfig movement, AnimationSet animationSet, DemoWeapons weapons, bool overwrite)` | Загружает или создаёт префабы демо (при — пересоздаёт). |

### DemoSceneBuilder

`Game.EditorTools` · static класс · [DemoSceneBuilder.cs](../Assets/Game/Scripts/Editor/DemoSceneBuilder.cs)

Собирает сцену Demo из сгенерированных префабов. Раскладку уровня строит тот же `DemoLevelBuilder`, что и сцена Playground, — отличие только в том, что здесь ставятся экземпляры префабов, которые можно редактировать.

**Связи:** использует [DemoLevelBuilder](#demolevelbuilder).

**Используется в:** [TpsSkeletonMenu](#tpsskeletonmenu).

**Методы**

| Член | Описание |
|---|---|
| `public static Scene Build(DemoPrefabs prefabs, CameraConfig cameraConfig)` | Создаёт новую сцену Demo из префабов, сохраняет её и ставит первой в Build Settings. |

### AnimationSetEditor

`Game.EditorTools` · sealed класс · [AnimationSetEditor.cs](../Assets/Game/Scripts/Editor/AnimationSetEditor.cs)

Базовые типы: `UnityEditor.Editor`

Атрибуты: `[CustomEditor(typeof(AnimationSet))]`

Инспектор AnimationSet: подсказки по Mixamo, список подменённых слотов и кнопка пересборки контроллера.

**Связи:** использует [AnimatorControllerBuilder](#animatorcontrollerbuilder).

**Методы**

| Член | Описание |
|---|---|
| `public override void OnInspectorGUI()` | Рисует инспектор: подсказку по Mixamo, поля набора, отчёт о заменах и кнопку пересборки. |

### AssetMaterialProvider

`Game.EditorTools` · sealed класс · [AssetMaterialProvider.cs](../Assets/Game/Scripts/Editor/AssetMaterialProvider.cs)

Базовые типы: [IMaterialProvider](#imaterialprovider)

`IMaterialProvider` для генератора: материалы сохраняются ассетами в Generated/Materials (иначе префабы потеряли бы ссылки на них). Существующие .mat не перезаписываются — их можно править.

**Связи:** реализует [IMaterialProvider](#imaterialprovider).

**Используется в:** [PrefabContentBuilder](#prefabcontentbuilder).

**Поля**

| Член | Описание |
|---|---|
| `public static readonly AssetMaterialProvider Instance` | Единственный экземпляр (провайдер без состояния). |

**Методы**

| Член | Описание |
|---|---|
| `public Material Lit(string name, Color color, float smoothness = 0.25f)` | Материал с освещением. |
| `public Material Emissive(string name, Color color, float intensity = 2f)` | Светящийся материал. |
| `public Material Unlit(string name, Color color)` | Материал без освещения. |
| `public Material Checker(string name, Color a, Color b, float tiling)` | Материал с текстурой-шахматкой. |

### PlaygroundSceneOpener

`Game.EditorTools` · static класс · [PlaygroundSceneOpener.cs](../Assets/Game/Scripts/Editor/PlaygroundSceneOpener.cs)

Атрибуты: `[InitializeOnLoad]`

При запуске редактора открывает сцену Playground, если открыта пустая безымянная сцена (так Unity делает при первом открытии проекта). Один раз за сессию, ваши сцены не трогает.

**Связи:** использует [PlaygroundBootstrap](#playgroundbootstrap) (открывает сцену).

**Константы**

| Член | Описание |
|---|---|
| `public const string ScenePath` | Путь к сцене Playground. |

**Методы**

| Член | Описание |
|---|---|
| `public static void Open()` | Пункт меню Tools → TPS Skeleton → Open Playground Scene. |

### EditorAssetUtility

`Game.EditorTools` · static класс · [EditorAssetUtility.cs](../Assets/Game/Scripts/Editor/EditorAssetUtility.cs)

Общие операции генератора: папки, ScriptableObject-ы, префабы.

**Методы**

| Член | Описание |
|---|---|
| `public static void EnsureFolder(string path)` | Создаёт папку проекта (и все родительские), если её нет. |
| `public static T LoadOrCreate<T>(string path, Action<T> setup = null) where T : ScriptableObject` | Загружает ассет, а если его нет — создаёт и вызывает setup. Существующие ассеты не перезаписываются. |
| `public static bool Exists(string path)` | Есть ли ассет по пути. |
| `public static GameObject SavePrefab(GameObject instance, string path)` | Сохраняет объект сцены как префаб (перезаписывая, GUID сохраняется) и удаляет временный объект. |

### SkeletonPaths

`Game.EditorTools` · static класс · [SkeletonPaths.cs](../Assets/Game/Scripts/Editor/SkeletonPaths.cs)

Пути к генерируемым ассетам. Всё сгенерированное лежит в Assets/Game/Generated.

**Константы**

| Член | Описание |
|---|---|
| `public const string Root` | Корень проекта игры. |
| `public const string Generated` | Папка для всего сгенерированного. |
| `public const string Configs` | Папка конфигов (ScriptableObject). |
| `public const string Materials` | Папка материалов. |
| `public const string Animation` | Папка анимационных ассетов (контроллер, маска, Avatar). |
| `public const string Weapons` | Папка WeaponDefinition-ов. |
| `public const string WeaponViews` | Папка префабов моделей оружия. |
| `public const string Effects` | Папка префабов эффектов. |
| `public const string Level` | Папка префабов объектов уровня. |
| `public const string Characters` | Папка префабов персонажей. |
| `public const string Scenes` | Папка сцен. |
| `public const string MovementConfig` | Ассет настроек передвижения. |
| `public const string CameraConfig` | Ассет настроек камеры. |
| `public const string AnimationSet` | Набор анимаций игрока для генератора. |
| `public const string AnimatorController` | Сгенерированный Animator Controller игрока. |
| `public const string UpperBodyMask` | Маска верхней части тела для слоя UpperBody. |
| `public const string MannequinAvatar` | Humanoid Avatar манекена. |
| `public const string PlayerPrefab` | Префаб игрока. |
| `public const string DemoScene` | Сцена Demo, собранная генератором. |
| `public const string MixamoModels` | Папка с Mixamo-клипами из исходного проекта (используются по умолчанию). |

---
Типов: 102, задокументированных членов: 702.
