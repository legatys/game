# TPS Skeleton — базовый скелет шутера от третьего лица

Unity 6 (6000.5), URP, Input System. Весь код лежит в `Assets/Game/Scripts`, UML-схема — в [`Docs/UML.md`](UML.md) и `Prod_di.drawio`.

## Быстрый старт

1. Откройте проект в Unity и дождитесь компиляции.
2. Откроется сцена **`Assets/Game/Scenes/Playground.unity`**. Если не открылась — выберите **Tools → TPS Skeleton → Open Playground Scene**.
   В редакторе виден полигон: это превью, оно не сохраняется в сцену.
3. Нажмите **Play**. Персонаж, оружие, мишени, турель и уровень собираются прямо в игре из примитивов (`PlaygroundBootstrap`).

Анимации берутся из `Assets/Game/Animation/PlaygroundAnimationSet.asset`. Сейчас там `Idle` и `Jogging` из `Assets/Models`, присед и смерть показаны процедурно. Добавьте клипы Mixamo в слоты и нажмите **Rebuild Animator Controller** в инспекторе.

### Сцена из редактируемых префабов (по желанию)
**Tools → TPS Skeleton → 1. Build Demo Scene** сохранит то же самое в префабы и ассеты (`Assets/Game/Generated`) и соберёт сцену `Assets/Game/Scenes/Demo.unity`. Её можно редактировать руками. Код построения общий: `Game.Demo.DemoContentFactory` и `DemoLevelBuilder`.

Старый набросок (ConCharapter, GameSettings, сцены MainGame/CharacterSelect) перенесён в `Assets/_Legacy` без изменений: GUID сохранены, поэтому ссылки не сломались. Когда он станет не нужен, папку можно удалить.

## Управление

| Действие | Клавиатура/мышь | Геймпад |
|---|---|---|
| Движение / камера | WASD / мышь | левый / правый стик |
| Бег | Shift (удерживать) | L3 |
| Прыжок (повторно в воздухе — двойной) | Space | A |
| Присесть (переключатель) | C / Ctrl | B |
| **Подкат** | C / Ctrl на бегу | B на бегу |
| Огонь / прицел | ЛКМ / ПКМ | RT / LT |
| Перезарядка | R | X |
| Оружие | 1–4, колесо мыши | D-pad ←/→ |
| Сменить плечо камеры | V | R3 |
| Пауза | Esc | Start |

Привязки настраиваются в `Assets/InputSystem_Actions.inputactions`, имена действий — в `InputActionNames.cs`.

## Что есть в скелете

- **Движение** (`PlayerCharacter` + состояния): ходьба, бег, спринт, присед, подкат, прыжок, двойной прыжок, coyote time, буфер прыжка, батут. Капсула меняет высоту, а встать под низким потолком нельзя.
- **Камера** (`ThirdPersonCamera`): вид через плечо, приближение при прицеливании, смена плеча, отдача и отталкивание от стен.
- **Стрельба**: оружие = данные (`WeaponDefinition`) × режим огня (полуавто / авто / очередь) × тип выстрела (луч / дробь / снаряд). Есть разброс, его раскрытие при стрельбе, перезарядка, запас патронов, попадания в голову ×2, взрывы с уроном по площади и толчок физических объектов.
- **Здоровье и урон**: `Health` с событиями и регенерацией, хитбоксы, смерть и возрождение.
- **Анимации Mixamo**: Humanoid-манекен из примитивов, генерируемый Animator Controller (blend trees, слой верхней части тела) и IK взгляда в прицел.
- **ИИ**: турель на том же `StateMachine` и той же системе оружия; мишени возрождаются.
- **HUD** (MVP): прицел, зависящий от разброса, хит-маркер, HP, патроны, полоса перезарядки, красная вспышка при уроне. Пауза.

## Как заменить анимации (Mixamo)

1. На [mixamo.com](https://www.mixamo.com) выберите анимацию → **Download**: Format *FBX for Unity*, *Without Skin*, для движения — галочка **In Place**.
2. Положите FBX в проект → в импорте: **Rig → Animation Type = Humanoid**, **Animation → Loop Time** для циклов (бег, idle).
3. Откройте `Assets/Game/Generated/Animation/PlayerAnimationSet.asset` (или **Tools → TPS Skeleton → Select Animation Set**), перетащите клип в нужный слот и нажмите **Rebuild Animator Controller**.

Пустые слоты заменяются запасными клипами (инспектор показывает, что чем заменено). Поэтому персонаж работает и с одними `Idle` + `Jogging` из исходного проекта. Если клипа приседа нет, модель сплющивается по высоте; если нет клипа смерти, она падает на спину.

Рекомендуемые клипы Mixamo: *Rifle Idle, Walking/Strafe, Rifle Run, Running (sprint), Crouch Idle, Crouched Walking, Running Slide, Jump, Falling Idle, Dying, Rifle Aiming Idle, Firing Rifle, Reloading, Hit Reaction*.

## Как заменить модель персонажа

Манекен — это объект `Visual` внутри префаба `Player`. Animator висит на нём, а вся логика — на корне.

1. Импортируйте модель Mixamo с **Rig = Humanoid**.
2. Положите её в префаб `Player` вместо `Visual` (или внутрь `Player`, удалив `Visual`) и добавьте ей `Animator` с её Avatar и компонент `AimIKHandler`.
3. Перенесите `WeaponSocket` на нужную кость (грудь или правая рука).
4. На корне в `CharacterAnimator` укажите новый Animator и `WeaponInventory → Weapon Socket`.

Контроллер подставится из AnimationSet автоматически.

## Как расширять

| Хочу | Что делать |
|---|---|
| Новое оружие | Create → Game/Weapon Definition, заполнить поля, добавить в `WeaponInventory.startingWeapons` |
| Новый режим огня / тип выстрела | Класс `IFireMode` / `IShotStrategy` + значение enum + строка в `WeaponFactory` |
| Новое состояние движения (wall-run, перекат) | Класс от `CharacterState` + переходы в `PlayerCharacter.BuildStateMachine()` |
| Новый враг | Компонент + `Health` + свои `IState` на `StateMachine`; оружие — через `WeaponFactory` |
| Новый подбираемый предмет | Наследник `Pickup` с методом `TryApply` |
| Глобальное событие | `struct : IGameEvent` в `GameEvents.cs`, затем `EventBus.Publish/Subscribe` |
| Другой HUD (uGUI/TMP) | Новая реализация `IHudView` вместо `ImGuiHudView` |
| Глобальный сервис | Создать и зарегистрировать в `CoreServices.Install()` |

**После добавления класса обновите UML**: опишите класс (`C(...)`) и его связи (`R(...)`) в `Docs/uml/uml_model.py` и запустите из папки `Prod`:

```bash
python3 Docs/uml/generate_uml.py
```

Скрипт перегенерирует `Prod_di.drawio` и `Docs/UML.md`.

## Генератор (Tools → TPS Skeleton)

- **1. Build Demo Scene** — догенерирует недостающее и (по вашему выбору) пересоздаст сцену.
- **2. Rebuild All Prefabs And Scene** — пересоздаёт префабы и сцену. Настройки и оружие (ScriptableObject) не трогает никогда.
- **3. Rebuild Animator Controller** — пересобирает контроллер из AnimationSet.

Всё сгенерированное лежит в `Assets/Game/Generated`. Если вы правили префабы или сцену вручную, не нажимайте «Rebuild All»: он их перезапишет.
