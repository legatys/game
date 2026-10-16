# UML-схема проекта

> Файл сгенерирован `Docs/uml/generate_uml.py` из `Docs/uml/uml_model.py`. Не правьте вручную — правьте модель и перезапустите `python3 Docs/uml/generate_uml.py` (из папки `Prod`). Та же схема для draw.io лежит в `Prod/Prod_di.drawio` (страницы: обзор, все классы, по пакетам).

**Обозначения.** Стереотип в `<<...>>`: `MonoBehaviour` — компонент Unity, `ScriptableObject` — ассет с данными, `static` — статический класс, `interface`, `abstract`, `struct`, `enum`. Связи: `--|>` наследование, `..|>` реализация интерфейса, `*--` владеет, `-->` ссылается, `..>` использует. `$` — статический член, `*` — абстрактный, `#` — protected, `-` — private.

## Паттерны проектирования

| Паттерн | Где | Зачем |
|---|---|---|
| **Service Locator** | `ServiceLocator` | Компоненты находят сервисы (камера, ввод, пул, игрок) по интерфейсу, а не через FindObjectOfType и жёсткие ссылки. |
| **Observer** | `EventBus`, `Health` | События (урон, смерть, появление игрока, пауза) — подписчики не связаны с издателем: HUD не знает про турель. |
| **State** | `StateMachine` | Каждое состояние движения/ИИ — отдельный класс, переходы описаны таблицей условий в одном месте. |
| **Object Pool** | `GameObjectPool` | Пули, трассеры, эффекты переиспользуются вместо Instantiate/Destroy — меньше мусора и фризов. |
| **Composition Root** | `CoreServices` | Глобальные сервисы создаются в одном месте до загрузки сцены. |
| **Adapter** | `PlayerInputReader`, `CharacterAnimator` | Внешние API (Input System, Animator) спрятаны за своими интерфейсами — их можно заменить, не трогая геймплей. |
| **Data-driven config** | `MovementConfig` | Цифры баланса живут в ассетах — правятся в инспекторе без перекомпиляции. |
| **Facade** | `PlayerCharacter`, `TpsSkeletonMenu` | Один простой вход в сложную подсистему (игрок для других систем, меню для генератора). |
| **Template Method** | `CharacterState`, `Pickup` | Базовый класс фиксирует порядок шагов, наследник меняет только свой шаг. |
| **Proxy** | `Hitbox` | Хитбокс подставляется вместо Health и меняет урон по пути. |
| **Flyweight** | `WeaponDefinition` | Общие неизменяемые данные оружия в одном ассете на всех владельцев. |
| **Strategy** | `IFireMode`, `IShotStrategy` | Поведение оружия собирается из взаимозаменяемых частей: режим огня × тип выстрела. |
| **Factory** | `WeaponFactory` | Создание сложного объекта (оружие = визуал + стратегии) спрятано в одном месте. |
| **MVP** | `HudPresenter` | Логика HUD (Presenter) отделена от отрисовки (View) — IMGUI можно заменить на uGUI. |
| **Builder** | `AnimatorControllerBuilder`, `MannequinBuilder`, `Serialized` | Пошаговая сборка сложных объектов: Animator Controller, манекен со скелетом, настройка полей. |

## Обзор пакетов

```mermaid
flowchart LR
    Core["Core<br/><small>Инфраструктура: сервисы, события, конечный автомат, пулы</small>"]
    Inputs["Inputs<br/><small>Абстракция ввода поверх Unity Input System</small>"]
    Character["Character<br/><small>Игрок: фасад, мотор, бой, конфиг движения</small>"]
    Character_States["Character.States<br/><small>Состояния движения игрока (State)</small>"]
    Cameras["Cameras<br/><small>Камера от третьего лица</small>"]
    Animations["Animations<br/><small>Связка геймплея с Animator и клипами Mixamo</small>"]
    Combat["Combat<br/><small>Урон, здоровье, снаряды, эффекты</small>"]
    Combat_Weapons["Combat.Weapons<br/><small>Оружие: данные, стратегии, фабрика, инвентарь</small>"]
    AI["AI<br/><small>Враги и мишени</small>"]
    Interaction["Interaction<br/><small>Подбираемые предметы и объекты уровня</small>"]
    Gameplay["Gameplay<br/><small>Правила игры: появление игрока</small>"]
    UI["UI<br/><small>HUD (MVP) и пауза</small>"]
    EditorTools["EditorTools<br/><small>Генератор демо: манекен, аниматор, префабы, сцена (только редактор)</small>"]
    Inputs -->|1| Core
    Character -->|1| Core
    Character -->|1| Inputs
    Character -->|1| Combat
    Character -->|3| Cameras
    Character -->|3| Combat_Weapons
    Character_States -->|1| Core
    Character_States -->|1| Character
    Cameras -->|1| Inputs
    Cameras -->|1| Core
    Animations -->|1| Character
    Animations -->|1| Combat_Weapons
    Combat -->|4| Core
    Combat_Weapons -->|3| Combat
    AI -->|4| Core
    AI -->|2| Combat_Weapons
    AI -->|2| Combat
    AI -->|1| Character
    Interaction -->|2| Character
    Gameplay -->|1| Character
    Gameplay -->|1| Core
    Gameplay -->|1| Cameras
    UI -->|1| Character
    UI -->|2| Core
    UI -->|1| Inputs
    EditorTools -->|1| Animations
    EditorTools -->|1| Combat_Weapons
    EditorTools -->|1| Gameplay
```

## Core — `Game.Core`

Инфраструктура: сервисы, события, конечный автомат, пулы.

```mermaid
classDiagram
    direction LR
    class ServiceLocator {
        <<static>>
        +Register~T~(service)
        +Unregister~T~(service)
        +Get~T~() T
        +TryGet~T~(out service) bool
        +Clear()
    }
    class EventBus {
        <<static>>
        +Subscribe~T~(handler)
        +Unsubscribe~T~(handler)
        +Publish~T~(event)
    }
    class IGameEvent {
        <<interface>>
    }
    class DamageDealtEvent {
        <<struct>>
        +Info : DamageInfo
        +Target : Health
        +Killed : bool
    }
    class PlayerSpawnedEvent {
        <<struct>>
        +Player : PlayerCharacter
    }
    class PlayerDiedEvent {
        <<struct>>
        +Player : PlayerCharacter
        +RespawnDelay : float
    }
    class GamePausedEvent {
        <<struct>>
        +IsPaused : bool
    }
    class ExplosionEvent {
        <<struct>>
        +Position : Vector3
        +Radius : float
    }
    class IState {
        <<interface>>
        +Name : string
        +Enter()
        +Tick(dt)
        +Exit()
    }
    class StateMachine {
        <<class>>
        +Current : IState
        +Previous : IState
        +StateChanged : event
        +AddTransition(from, to, condition)
        +AddAnyTransition(to, condition)
        +SetState(state)
        +Tick(dt)
    }
    class IPoolable {
        <<interface>>
        +OnSpawned()
        +OnDespawned()
    }
    class GameObjectPool {
        <<class>>
        +Get(position, rotation) PooledObject
        +Release(item)
        +Prewarm(count)
    }
    class PooledObject {
        <<MonoBehaviour>>
        +Release()
        +ReleaseAfter(seconds)
    }
    class PoolService {
        <<class>>
        +Spawn(prefab, pos, rot) GameObject
        +Spawn~T~(prefab, pos, rot) T
        +Prewarm(prefab, count)
        +Release(instance)$
    }
    class CoreServices {
        <<static>>
        -Install() BeforeSceneLoad
        +Spawn(prefab, pos, rot) GameObject
    }
    class PhysicsQuery {
        <<static>>
        +Raycast(ray, dist, mask, ignoreRoot, out hit) bool
        +SphereCast(...) bool
        +RandomInCone(dir, angle) Vector3
    }
    DamageDealtEvent ..|> IGameEvent
    PlayerSpawnedEvent ..|> IGameEvent
    PlayerDiedEvent ..|> IGameEvent
    GamePausedEvent ..|> IGameEvent
    ExplosionEvent ..|> IGameEvent
    EventBus ..> IGameEvent : публикует
    StateMachine --> IState : current / transitions
    PoolService *-- GameObjectPool : по пулу на префаб
    GameObjectPool *-- PooledObject : хранит
    PooledObject ..> IPoolable : уведомляет
    CoreServices ..> PoolService : создаёт
    CoreServices ..> ServiceLocator : регистрирует
```

| Класс | Тип | Что делает |
|---|---|---|
| `ServiceLocator` | static | Регистрация и поиск глобальных сервисов по типу (обычно интерфейсу). **[Service Locator]** |
| `EventBus` | static | Глобальная типизированная шина событий: издатель не знает подписчиков. **[Observer]** |
| `IGameEvent` | interface | Маркер событий для EventBus. |
| `DamageDealtEvent` | struct | Кто-то получил урон (для хит-маркера, статистики). |
| `PlayerSpawnedEvent` | struct | Игрок появился/возродился. |
| `PlayerDiedEvent` | struct | Игрок погиб, возрождение через RespawnDelay. |
| `GamePausedEvent` | struct | Пауза включена/выключена. |
| `ExplosionEvent` | struct | Взрыв (для тряски камеры, звука). |
| `IState` | interface | Состояние конечного автомата. |
| `StateMachine` | class | Универсальный конечный автомат с таблицей переходов-условий. Используется игроком и ИИ. **[State]** |
| `IPoolable` | interface | Уведомления о выдаче/возврате объекта в пул. |
| `GameObjectPool` | class | Пул экземпляров одного префаба (Stack неактивных объектов). **[Object Pool]** |
| `PooledObject` | MonoBehaviour | Ссылка экземпляра на свой пул; возврат в пул сразу или по таймеру. |
| `PoolService` | class | Пулы по префабам. Глобальный сервис, живёт между сценами. |
| `CoreServices` | static | Composition Root: до загрузки сцены создаёт и регистрирует глобальные сервисы. **[Composition Root]** |
| `PhysicsQuery` | static | Рейкасты/сферокасты, игнорирующие коллайдеры владельца; разброс в конусе. |

## Inputs — `Game.Inputs`

Абстракция ввода поверх Unity Input System.

```mermaid
classDiagram
    direction LR
    class IPlayerInput {
        <<interface>>
        +Move, Look : Vector2
        +LookIsDelta : bool
        +JumpPressed, SprintHeld : bool
        +CrouchPressed, CrouchHeld : bool
        +FirePressed, FireHeld, AimHeld : bool
        +ReloadPressed, InteractPressed : bool
        +Next/PreviousWeaponPressed : bool
        +WeaponSlotPressed : int
        +SwitchShoulderPressed, PausePressed : bool
        +GameplayEnabled : bool
    }
    class PlayerInputReader {
        <<MonoBehaviour>>
        -actions : InputActionAsset
        -Awake() находит действия по InputActionNames
    }
    class InputActionNames {
        <<static>>
        +PlayerMap, Move, Look, Jump, ... : const string
        +WeaponSlots : string
    }
    PlayerInputReader ..|> IPlayerInput
    PlayerInputReader ..> InputActionNames
    PlayerInputReader ..> ServiceLocator : регистрирует себя
```

| Класс | Тип | Что делает |
|---|---|---|
| `IPlayerInput` | interface | Абстракция ввода: персонаж, камера и оружие не зависят от Input System. |
| `PlayerInputReader` | MonoBehaviour | Адаптер Unity Input System (InputActionAsset) к IPlayerInput. **[Adapter]** |
| `InputActionNames` | static | Имена карты и действий из InputSystem_Actions — единственное место со строками. |

## Character — `Game.Character`

Игрок: фасад, мотор, бой, конфиг движения.

```mermaid
classDiagram
    direction LR
    class MovementConfig {
        <<ScriptableObject>>
        +walk/run/sprint/crouchSpeed : float
        +acceleration, deceleration, airAcceleration
        +rotationSpeed
        +gravity, jumpHeight, maxAirJumps
        +coyoteTime, jumpBufferTime
        +standHeight, crouchHeight
        +slideStartSpeed, slideFriction, slideMaxDuration, slideCooldown
    }
    class CharacterMotor {
        <<MonoBehaviour>>
        +Velocity, ActualVelocity : Vector3
        +IsGrounded : bool
        +AirTime, Height : float
        +Landed : event
        +AccelerateTowards(target, accel, dt)
        +SetPlanarVelocity(v)
        +Jump(height, gravity)
        +Launch(v)
        +AddImpulse(v)
        +Simulate(gravity, dt)
        +SetTargetHeight(h, speed)
        +CanStandUp(h) bool
        +RotateTowards(dir, degPerSec, dt)
        +Teleport(pos, rot)
    }
    class PlayerCharacter {
        <<MonoBehaviour>>
        +Config : MovementConfig
        +Motor, Health, Combat, Inventory, Input
        +CurrentStateName : string
        +IsSprinting, IsCrouching, IsSliding, IsDead, IsAiming : bool
        +AirJumpsRemaining : int
        +Jumped, Landed, Respawned, StateChanged : event
        -BuildStateMachine()
        +GetMoveDirection() Vector3
        +GetGroundSpeed() float
        +UpdateFacing(dir, dt)
        +TryConsumeJumpRequest() bool
        +PerformJump()
        +Launch(velocity)
        +Respawn(pos, rot)
    }
    class PlayerCombat {
        <<MonoBehaviour>>
        +IsAiming, WantsCombatFacing : bool
        +AimPoint : Vector3
        +CurrentWeapon : Weapon
        +WeaponFired, ReloadStarted : event
        -Update() тикает текущее оружие
        -ComputeSpreadMultiplier() float
    }
    PlayerCharacter --> CharacterMotor : Motor
    PlayerCharacter --> MovementConfig : config
    PlayerCharacter *-- StateMachine : автомат движения
    PlayerCharacter --> IPlayerInput : Input
    PlayerCharacter --> Health
    PlayerCharacter --> PlayerCombat : Combat
    PlayerCharacter ..|> ICameraTarget
    PlayerCharacter ..> ICameraRig : движение от камеры
    PlayerCombat ..|> IAimProvider
    PlayerCombat --> WeaponInventory
    PlayerCombat ..> ICameraRig : AimPoint, отдача
    PlayerCombat ..> AimData
```

| Класс | Тип | Что делает |
|---|---|---|
| `MovementConfig` | ScriptableObject | Настройки передвижения: скорости, прыжок, присед, подкат. **[Data-driven config]** |
| `CharacterMotor` | MonoBehaviour | Перемещение поверх CharacterController: скорость, гравитация, прыжок, рост капсулы. |
| `PlayerCharacter` | MonoBehaviour | Фасад игрока: объединяет мотор, здоровье, бой, ввод; владеет автоматом состояний. **[Facade]** |
| `PlayerCombat` | MonoBehaviour | Прицеливание, стрельба, перезарядка, смена оружия, отдача камеры. |

## Character.States — `Game.Character.States`

Состояния движения игрока (State).

```mermaid
classDiagram
    direction LR
    class CharacterState {
        <<abstract>>
        #Player : PlayerCharacter
        #Motor, Config, Input
        +Tick(dt) (final)
        #OnTick(dt)*
        +Enter()
        +Exit()
    }
    class GroundedState {
        <<class>>
    }
    class CrouchState {
        <<class>>
    }
    class SlideState {
        <<class>>
        +IsFinished : bool
    }
    class AirborneState {
        <<class>>
    }
    class DeadState {
        <<class>>
    }
    CharacterState ..|> IState
    GroundedState --|> CharacterState
    CrouchState --|> CharacterState
    SlideState --|> CharacterState
    AirborneState --|> CharacterState
    DeadState --|> CharacterState
    CharacterState --> PlayerCharacter : Player
```

| Класс | Тип | Что делает |
|---|---|---|
| `CharacterState` | abstract | База состояний: Tick() = OnTick() + Motor.Simulate(). **[Template Method]** |
| `GroundedState` | class | На земле: стоим, идём при прицеливании, бежим, спринтуем. |
| `CrouchState` | class | Присед: низкая капсула, медленное движение. |
| `SlideState` | class | Подкат: рывок с затуханием скорости и низкой капсулой. |
| `AirborneState` | class | Прыжок/падение, двойной прыжок, управление в воздухе. |
| `DeadState` | class | Смерть: персонаж останавливается, ввод игнорируется. |

## Cameras — `Game.Cameras`

Камера от третьего лица.

```mermaid
classDiagram
    direction LR
    class ICameraTarget {
        <<interface>>
        +Root : Transform
        +PivotPosition : Vector3
        +IsAiming : bool
    }
    class ICameraRig {
        <<interface>>
        +Camera : Camera
        +YawRotation : Quaternion
        +Forward : Vector3
        +GetAimPoint(maxDist, ignoreRoot) Vector3
        +AddRecoil(pitch, yaw)
        +SetTarget(target)
    }
    class CameraConfig {
        <<ScriptableObject>>
        +mouse/gamepadSensitivity
        +distance, shoulderOffset, fieldOfView
        +aimDistance, aimShoulderOffset, aimFieldOfView
        +collisionRadius, collisionMask
        +recoilRecovery
    }
    class ThirdPersonCamera {
        <<MonoBehaviour>>
        -LateUpdate()
        -ReadInput(dt)
        -UpdateRecoil(dt)
        -ResolveCollision(pivot, offset, dt) Vector3
    }
    ThirdPersonCamera ..|> ICameraRig
    ThirdPersonCamera --> CameraConfig : config
    ThirdPersonCamera --> ICameraTarget : target
    ThirdPersonCamera ..> IPlayerInput : Look, SwitchShoulder
    ThirdPersonCamera ..> PhysicsQuery
```

| Класс | Тип | Что делает |
|---|---|---|
| `ICameraTarget` | interface | То, за чем следует камера. |
| `ICameraRig` | interface | Сервис камеры для геймплея. |
| `CameraConfig` | ScriptableObject | Чувствительность, дистанции, плечо, FOV, коллизии, отдача. |
| `ThirdPersonCamera` | MonoBehaviour | Камера через плечо: орбита, прицел-зум, смена плеча, отдача, коллизии. |

## Animations — `Game.Animations`

Связка геймплея с Animator и клипами Mixamo.

```mermaid
classDiagram
    direction LR
    class AnimatorParams {
        <<static>>
        +MoveX, MoveZ, Speed, VerticalSpeed
        +IsGrounded, IsCrouching, IsSliding, IsAiming, IsDead
        +Jump, Fire, Reload, Hit
        +UpperBodyLayer
    }
    class AnimationSlot {
        <<enum>>
    }
    class AnimationSet {
        <<ScriptableObject>>
        +controller : RuntimeAnimatorController
        +walk/run/sprint/crouchSpeed
        +idle, walk*, run*, sprint ... : AnimationClip
        +Fallbacks$ : Dictionary
        +GetClip(slot) AnimationClip
        +HasClip(slot) bool
        +Resolve(slot, out from) AnimationClip
    }
    class CharacterAnimator {
        <<MonoBehaviour>>
        -animator : Animator
        -animationSet : AnimationSet
        -Update() выставляет параметры
        -UpdateUpperBodyLayer(dt)
        -OnJumped/OnWeaponFired/OnDamaged() триггеры
    }
    class AimIKHandler {
        <<MonoBehaviour>>
        -OnAnimatorIK(layer)
    }
    AnimationSet ..> AnimationSlot
    CharacterAnimator --> AnimationSet
    CharacterAnimator --> PlayerCharacter : читает состояние
    CharacterAnimator ..> AnimatorParams
    AimIKHandler --> IAimProvider
```

| Класс | Тип | Что делает |
|---|---|---|
| `AnimatorParams` | static | Имена/хэши параметров и слоёв Animator — общие для генератора и рантайма. |
| `AnimationSlot` | enum | Слоты анимаций: Idle, Walk*, Run*, Sprint, Crouch*, Slide, JumpStart, Fall, Death, AimIdle, Fire, Reload, HitReact. |
| `AnimationSet` | ScriptableObject | Набор клипов Mixamo + ссылка на сгенерированный контроллер. Пустые слоты берут запасной клип. |
| `CharacterAnimator` | MonoBehaviour | Переводит состояние игрока и его события в параметры Animator; запасные процедурные позы. **[Adapter]** |
| `AimIKHandler` | MonoBehaviour | Humanoid IK взгляда: голова и корпус смотрят в точку прицела. |

## Combat — `Game.Combat`

Урон, здоровье, снаряды, эффекты.

```mermaid
classDiagram
    direction LR
    class DamageInfo {
        <<struct>>
        +Amount : float
        +Point, Direction : Vector3
        +Instigator : GameObject
        +IsCritical : bool
        +Scaled(multiplier, critical) DamageInfo
    }
    class IDamageable {
        <<interface>>
        +IsAlive : bool
        +DamageRoot : Transform
        +ApplyDamage(in DamageInfo)
    }
    class Health {
        <<MonoBehaviour>>
        +Current, Max, Normalized : float
        +IsAlive : bool
        +Damaged, Died, Changed, Revived : event
        +ApplyDamage(in info)
        +Heal(amount) float
        +Revive()
    }
    class Hitbox {
        <<MonoBehaviour>>
        -damageMultiplier : float
        -isCritical : bool
    }
    class Projectile {
        <<MonoBehaviour>>
        +Launch(velocity, definition, owner)
        -Explode(position)
    }
    class Explosion {
        <<static>>
        +Detonate(center, radius, damage, force, instigator)
    }
    class PooledEffect {
        <<MonoBehaviour>>
    }
    class Tracer {
        <<MonoBehaviour>>
        +Show(from, to)
    }
    Health ..|> IDamageable
    Hitbox ..|> IDamageable
    Hitbox --> Health : передаёт урон
    Health ..> DamageInfo
    Health ..> EventBus : DamageDealtEvent
    Projectile ..|> IPoolable
    PooledEffect ..|> IPoolable
    Tracer ..|> IPoolable
    Projectile ..> Explosion
    Explosion ..> IDamageable
```

| Класс | Тип | Что делает |
|---|---|---|
| `DamageInfo` | struct | Описание попадания. |
| `IDamageable` | interface | Всё, что можно повредить. |
| `Health` | MonoBehaviour | Здоровье с событиями и регенерацией; публикует DamageDealtEvent. **[Observer]** |
| `Hitbox` | MonoBehaviour | Хитбокс (голова ×2): умножает урон и передаёт его в Health родителя. **[Proxy]** |
| `Projectile` | MonoBehaviour | Снаряд из пула: летит по дуге, проверяет попадание лучом, взрывается. |
| `Explosion` | static | Урон по площади с затуханием и толчок физики. |
| `PooledEffect` | MonoBehaviour | Простой эффект из пула: масштаб и свет по времени жизни. |
| `Tracer` | MonoBehaviour | Трассер пули на LineRenderer. |

## Combat.Weapons — `Game.Combat.Weapons`

Оружие: данные, стратегии, фабрика, инвентарь.

```mermaid
classDiagram
    direction LR
    class FireModeType {
        <<enum>>
    }
    class ShotType {
        <<enum>>
    }
    class WeaponDefinition {
        <<ScriptableObject>>
        +displayName : string
        +fireMode : FireModeType
        +shotType : ShotType
        +viewPrefab : WeaponView
        +damage, roundsPerMinute, range
        +magazineSize, maxReserve, reloadTime
        +hip/aimSpread, spreadPerShot
        +pelletCount, projectilePrefab
        +recoilPitch, recoilYaw
        +tracer/impactPrefab, sounds
    }
    class AimData {
        <<struct>>
        +AimPoint : Vector3
        +Owner : GameObject
        +IsAiming : bool
        +SpreadMultiplier : float
    }
    class ShotContext {
        <<struct>>
        +Definition
        +Origin, AimPoint : Vector3
        +Owner : GameObject
        +SpreadDegrees : float
        +BaseDirection : Vector3
    }
    class IFireMode {
        <<interface>>
        +Tick(weapon, pressed, held, aim, dt)
        +Reset()
    }
    class SemiAutoFireMode {
        <<class>>
    }
    class FullAutoFireMode {
        <<class>>
    }
    class BurstFireMode {
        <<class>>
    }
    class IShotStrategy {
        <<interface>>
        +Fire(in ShotContext)
    }
    class HitscanShot {
        <<class>>
    }
    class SpreadShot {
        <<class>>
    }
    class ProjectileShot {
        <<class>>
    }
    class ShotUtility {
        <<static>>
        +FireRay(ctx, direction, damage)
    }
    class Weapon {
        <<class>>
        +Definition : WeaponDefinition
        +View : WeaponView
        +AmmoInMagazine, ReserveAmmo : int
        +IsReloading, IsReadyToFire, IsEmpty
        +CurrentSpread : float
        +Fired, DryFired, AmmoChanged, ReloadStarted, ReloadFinished : event
        +Tick(dt, pressed, held, aim)
        +TryFire(aim) bool
        +TryReload() bool
        +Holster()
        +Draw()
        +AddReserveAmmo(n) int
    }
    class WeaponView {
        <<MonoBehaviour>>
        +Muzzle : Transform
        +PlayFireEffects(clip)
        +PlaySound(clip)
        +SetVisible(bool)
        +CreatePlaceholder(parent, name)$
    }
    class WeaponFactory {
        <<static>>
        +Create(definition, parent) Weapon
        +CreateFireMode(def) IFireMode
        +CreateShot(def) IShotStrategy
    }
    class WeaponInventory {
        <<MonoBehaviour>>
        -startingWeapons : WeaponDefinition
        +Weapons : IReadOnlyList~Weapon~
        +Current : Weapon
        +WeaponEquipped, WeaponAdded : event
        +AddWeapon(def) Weapon
        +Equip(index) bool
        +EquipNext()
        +EquipPrevious()
        +AddAmmoToAll(mags) int
    }
    class IAimProvider {
        <<interface>>
        +IsCombatReady : bool
        +AimPoint : Vector3
    }
    class WeaponSocket {
        <<MonoBehaviour>>
    }
    WeaponDefinition ..> FireModeType
    WeaponDefinition ..> ShotType
    SemiAutoFireMode ..|> IFireMode
    FullAutoFireMode ..|> IFireMode
    BurstFireMode ..|> IFireMode
    HitscanShot ..|> IShotStrategy
    SpreadShot ..|> IShotStrategy
    ProjectileShot ..|> IShotStrategy
    HitscanShot ..> ShotUtility
    SpreadShot ..> ShotUtility
    ProjectileShot ..> Projectile : спавнит
    ShotUtility ..> IDamageable
    ShotUtility ..> Tracer
    Weapon --> WeaponDefinition : Definition
    Weapon --> WeaponView : View
    Weapon *-- IFireMode
    Weapon --> IShotStrategy
    Weapon ..> ShotContext
    WeaponFactory ..> Weapon : создаёт
    WeaponInventory *-- Weapon : 0..4
    WeaponInventory ..> WeaponFactory
    WeaponSocket --> IAimProvider
```

| Класс | Тип | Что делает |
|---|---|---|
| `FireModeType` | enum | SemiAuto, FullAuto, Burst |
| `ShotType` | enum | Hitscan, Spread, Projectile |
| `WeaponDefinition` | ScriptableObject | Данные оружия: урон, темп, патроны, разброс, отдача, эффекты. Новое оружие = новый ассет. **[Flyweight]** |
| `AimData` | struct | Куда стрелять в этом кадре. |
| `ShotContext` | struct | Контекст одного выстрела для стратегии. |
| `IFireMode` | interface | КОГДА стрелять. **[Strategy]** |
| `SemiAutoFireMode` | class | Выстрел на нажатие (с буфером нажатия). |
| `FullAutoFireMode` | class | Стрельба, пока зажат курок. |
| `BurstFireMode` | class | Очередь из N выстрелов на нажатие. |
| `IShotStrategy` | interface | ЧТО происходит при выстреле. **[Strategy]** |
| `HitscanShot` | class | Мгновенный луч. |
| `SpreadShot` | class | Дробь: несколько лучей. |
| `ProjectileShot` | class | Снаряд из пула. |
| `ShotUtility` | static | Общий код луча: урон, сила удара, искры, трассер. |
| `Weapon` | class | Экземпляр оружия: патроны, перезарядка, кулдаун, разброс. Поведение — в стратегиях. |
| `WeaponView` | MonoBehaviour | Визуал оружия: модель, дуло, вспышка, звук. |
| `WeaponFactory` | static | Собирает Weapon из WeaponDefinition: визуал + стратегии по enum-ам. **[Factory]** |
| `WeaponInventory` | MonoBehaviour | Слоты оружия: стартовое оружие, переключение, патроны. |
| `IAimProvider` | interface | Источник точки прицела для визуала. |
| `WeaponSocket` | MonoBehaviour | Крепление оружия: в бою ствол смотрит в прицел, вне боя — походное положение. |

## AI — `Game.AI`

Враги и мишени.

```mermaid
classDiagram
    direction LR
    class EnemyTurret {
        <<MonoBehaviour>>
        -weapon : WeaponDefinition
        -detectionRange, turnSpeed
        +Health, Weapon
        +CanSeeTarget() bool
        +AimAt(point, dt) float
        +Scan(dt)
        +TickWeapon(trigger, dt, aimError)
        +SetVisible(bool)
    }
    class TurretIdleState {
        <<class>>
    }
    class TurretAttackState {
        <<class>>
    }
    class TurretDeadState {
        <<class>>
    }
    class TargetDummy {
        <<MonoBehaviour>>
    }
    EnemyTurret *-- StateMachine
    EnemyTurret *-- Weapon
    EnemyTurret ..> WeaponFactory
    EnemyTurret --> Health
    EnemyTurret ..> PlayerCharacter : цель
    TurretIdleState ..|> IState
    TurretIdleState --> EnemyTurret
    TurretAttackState ..|> IState
    TurretAttackState --> EnemyTurret
    TurretDeadState ..|> IState
    TurretDeadState --> EnemyTurret
    TargetDummy --> Health
```

| Класс | Тип | Что делает |
|---|---|---|
| `EnemyTurret` | MonoBehaviour | Турель: ищет игрока, доворачивается и стреляет очередями. Переиспользует StateMachine и Weapon. |
| `TurretIdleState` | class | Нет цели — сканирование. |
| `TurretAttackState` | class | Цель видна — наведение и очереди. |
| `TurretDeadState` | class | Уничтожена — скрыта, затем восстанавливается. |
| `TargetDummy` | MonoBehaviour | Мишень: вспышка при попадании, исчезает и возрождается. |

## Interaction — `Game.Interaction`

Подбираемые предметы и объекты уровня.

```mermaid
classDiagram
    direction LR
    class Pickup {
        <<abstract>>
        -OnTriggerEnter(other)
        #TryApply(player) bool*
    }
    class HealthPickup {
        <<MonoBehaviour>>
        -amount : float
    }
    class AmmoPickup {
        <<MonoBehaviour>>
        -magazines : int
    }
    class JumpPad {
        <<MonoBehaviour>>
        -launchVelocity : Vector3
    }
    HealthPickup --|> Pickup
    AmmoPickup --|> Pickup
    Pickup ..> PlayerCharacter
    JumpPad ..> PlayerCharacter : Launch()
```

| Класс | Тип | Что делает |
|---|---|---|
| `Pickup` | abstract | Подбираемое: триггер → TryApply → скрыть → появиться снова. **[Template Method]** |
| `HealthPickup` | MonoBehaviour | Аптечка. |
| `AmmoPickup` | MonoBehaviour | Патроны всему оружию. |
| `JumpPad` | MonoBehaviour | Батут: подбрасывает игрока и восстанавливает прыжки. |

## Gameplay — `Game.Gameplay`

Правила игры: появление игрока.

```mermaid
classDiagram
    direction LR
    class PlayerSpawner {
        <<MonoBehaviour>>
        -playerPrefab : PlayerCharacter
        -spawnPoint : Transform
        -respawnDelay : float
        -Start()
        -RespawnRoutine()
    }
    PlayerSpawner --> PlayerCharacter
    PlayerSpawner ..> EventBus : публикует
    PlayerSpawner ..> ICameraRig : SetTarget
```

| Класс | Тип | Что делает |
|---|---|---|
| `PlayerSpawner` | MonoBehaviour | Появление и возрождение игрока, события PlayerSpawned/PlayerDied. |

## UI — `Game.UI`

HUD (MVP) и пауза.

```mermaid
classDiagram
    direction LR
    class IHudView {
        <<interface>>
        +SetHealth(cur, max)
        +SetWeapon(name, mag, size, reserve)
        +SetReload(on, progress)
        +SetCrosshair(px, aiming)
        +ShowHitMarker(killed, critical)
        +ShowDamageFlash(strength)
        +SetRespawnCountdown(s)
        +SetDebugText(text)
    }
    class HudPresenter {
        <<MonoBehaviour>>
        -Bind(player)
        -BindWeapon(weapon)
        -SpreadToPixels(deg) float$
    }
    class ImGuiHudView {
        <<MonoBehaviour>>
    }
    class PauseMenu {
        <<MonoBehaviour>>
        +SetPaused(bool)
    }
    ImGuiHudView ..|> IHudView
    HudPresenter --> IHudView : view
    HudPresenter --> PlayerCharacter : model
    HudPresenter ..> EventBus : подписка
    PauseMenu ..> IPlayerInput
    PauseMenu ..> EventBus : публикует
```

| Класс | Тип | Что делает |
|---|---|---|
| `IHudView` | interface | View (MVP): только отображение. |
| `HudPresenter` | MonoBehaviour | Presenter (MVP): слушает Health/Weapon/EventBus и обновляет IHudView. **[MVP]** |
| `ImGuiHudView` | MonoBehaviour | HUD на IMGUI: прицел, хит-маркер, HP, патроны, подсказки. |
| `PauseMenu` | MonoBehaviour | Пауза: timeScale, курсор, отключение ввода, GamePausedEvent. |

## EditorTools — `Game.EditorTools`

Генератор демо: манекен, аниматор, префабы, сцена (только редактор).

```mermaid
classDiagram
    direction LR
    class TpsSkeletonMenu {
        <<static>>
        -BuildDemo()
        -RebuildAll()
        -RebuildAnimator()
        -Generate(overwritePrefabs, rebuildScene)
    }
    class CoreAssetsBuilder {
        <<static>>
        +LoadMovementConfig()
        +LoadCameraConfig()
        +LoadAnimationSet(movement)
        +BuildAnimator(set)
    }
    class AnimatorControllerBuilder {
        <<class>>
        +Build(path, mask) AnimatorController
        +LoadOrCreateUpperBodyMask(path)$
    }
    class MannequinBuilder {
        <<class>>
        +WithMaterials(body, joints, accent)
        +Build(name) GameObject
        +GetBone(name) Transform
        +CreateAvatar(root, path)$
    }
    class WeaponContentBuilder {
        <<static>>
        +Build(overwrite) Result
    }
    class PrefabContentBuilder {
        <<static>>
        +Build(...) Result
    }
    class DemoSceneBuilder {
        <<static>>
        +Build(prefabs, camera) Scene
    }
    class AnimationSetEditor {
        <<Editor>>
    }
    class MaterialLibrary {
        <<static>>
    }
    class EditorAssetUtility {
        <<static>>
    }
    class Serialized {
        <<class>>
        +Of(target)$
        +Set(field, value)
        +Apply()
    }
    class SkeletonPaths {
        <<static>>
    }
    TpsSkeletonMenu ..> CoreAssetsBuilder
    TpsSkeletonMenu ..> WeaponContentBuilder
    TpsSkeletonMenu ..> PrefabContentBuilder
    TpsSkeletonMenu ..> DemoSceneBuilder
    CoreAssetsBuilder ..> AnimatorControllerBuilder
    AnimatorControllerBuilder ..> AnimationSet : читает клипы
    PrefabContentBuilder ..> MannequinBuilder
    AnimationSetEditor ..> AnimatorControllerBuilder
    WeaponContentBuilder ..> WeaponDefinition : создаёт
    DemoSceneBuilder ..> PlayerSpawner : размещает
```

| Класс | Тип | Что делает |
|---|---|---|
| `TpsSkeletonMenu` | static | Меню Tools → TPS Skeleton: вся генерация одной кнопкой. **[Facade]** |
| `CoreAssetsBuilder` | static | Конфиги, AnimationSet (с клипами из Assets/Models), Animator Controller. |
| `AnimatorControllerBuilder` | class | Собирает Animator Controller из AnimationSet: blend trees, слои, переходы. **[Builder]** |
| `MannequinBuilder` | class | Манекен из примитивов на скелете Mixamo + Humanoid Avatar. **[Builder]** |
| `WeaponContentBuilder` | static | Эффекты, модели оружия из примитивов, WeaponDefinition-ы. |
| `PrefabContentBuilder` | static | Префабы: игрок, мишень, турель, аптечка, патроны, батут. |
| `DemoSceneBuilder` | static | Сцена-полигон: тир, тоннель, лестница, рампа, паркур, турель. |
| `AnimationSetEditor` | Editor | Инспектор AnimationSet: подсказки, отчёт о заменах, кнопка Rebuild. |
| `MaterialLibrary` | static | Материалы URP Lit/Unlit и текстура-шахматка. |
| `EditorAssetUtility` | static | Папки, ScriptableObject, префабы, примитивы. |
| `Serialized` | class | Fluent-установка приватных [SerializeField] через SerializedObject. **[Builder]** |
| `SkeletonPaths` | static | Пути генерируемых ассетов (Assets/Game/Generated). |
