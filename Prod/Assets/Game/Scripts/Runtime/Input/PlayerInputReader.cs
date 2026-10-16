using Game.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.Inputs
{
    /// <summary>
    /// Паттерн Adapter: адаптирует Unity Input System (InputActionAsset) к интерфейсу <see cref="IPlayerInput"/>.
    /// Имена действий берутся из <see cref="InputActionNames"/>. Если ассет не задан —
    /// используется project-wide InputSystem_Actions (Project Settings → Input System Package).
    /// </summary>
    [DefaultExecutionOrder(-100)]
    public sealed class PlayerInputReader : MonoBehaviour, IPlayerInput
    {
        [SerializeField] private InputActionAsset actions;

        private InputAction _move, _look, _jump, _sprint, _crouch, _attack, _aim, _reload, _interact;
        private InputAction _next, _previous, _switchShoulder, _pause;
        private InputAction[] _weaponSlots;
        private bool _gameplayEnabled = true;

        public bool GameplayEnabled
        {
            get => _gameplayEnabled;
            set => _gameplayEnabled = value;
        }

        public Vector2 Move => Gameplay ? Read(_move) : Vector2.zero;
        public Vector2 Look => Gameplay ? Read(_look) : Vector2.zero;
        public bool LookIsDelta => _look?.activeControl == null || _look.activeControl.device is Pointer;

        public bool JumpPressed => Gameplay && Pressed(_jump);
        public bool SprintHeld => Gameplay && Held(_sprint);
        public bool CrouchPressed => Gameplay && Pressed(_crouch);
        public bool CrouchHeld => Gameplay && Held(_crouch);

        public bool FirePressed => Gameplay && Pressed(_attack);
        public bool FireHeld => Gameplay && Held(_attack);
        public bool AimHeld => Gameplay && Held(_aim);
        public bool ReloadPressed => Gameplay && Pressed(_reload);
        public bool InteractPressed => Gameplay && Pressed(_interact);

        public bool NextWeaponPressed => Gameplay && Pressed(_next);
        public bool PreviousWeaponPressed => Gameplay && Pressed(_previous);
        public bool SwitchShoulderPressed => Gameplay && Pressed(_switchShoulder);
        public bool PausePressed => Pressed(_pause);

        public int WeaponSlotPressed
        {
            get
            {
                if (!Gameplay || _weaponSlots == null) return -1;
                for (var i = 0; i < _weaponSlots.Length; i++)
                    if (Pressed(_weaponSlots[i]))
                        return i;
                return -1;
            }
        }

        private bool Gameplay => _gameplayEnabled && enabled;

        private void Awake()
        {
            if (actions == null) actions = InputSystem.actions;
            if (actions == null)
            {
                Debug.LogError("[PlayerInputReader] Не найден InputActionAsset. Назначьте его в инспекторе.", this);
                enabled = false;
                return;
            }

            _move = Find(InputActionNames.Move);
            _look = Find(InputActionNames.Look);
            _jump = Find(InputActionNames.Jump);
            _sprint = Find(InputActionNames.Sprint);
            _crouch = Find(InputActionNames.Crouch);
            _attack = Find(InputActionNames.Attack);
            _aim = Find(InputActionNames.Aim);
            _reload = Find(InputActionNames.Reload);
            _interact = Find(InputActionNames.Interact);
            _next = Find(InputActionNames.NextWeapon);
            _previous = Find(InputActionNames.PreviousWeapon);
            _switchShoulder = Find(InputActionNames.SwitchShoulder);
            _pause = Find(InputActionNames.Pause);

            _weaponSlots = new InputAction[InputActionNames.WeaponSlots.Length];
            for (var i = 0; i < _weaponSlots.Length; i++)
                _weaponSlots[i] = Find(InputActionNames.WeaponSlots[i]);

            ServiceLocator.Register<IPlayerInput>(this);
        }

        private void OnEnable()
        {
            if (actions != null) actions.FindActionMap(InputActionNames.PlayerMap)?.Enable();
        }

        private void OnDestroy() => ServiceLocator.Unregister<IPlayerInput>(this);

        private InputAction Find(string actionName)
        {
            var action = actions.FindAction($"{InputActionNames.PlayerMap}/{actionName}");
            if (action == null)
                Debug.LogWarning($"[PlayerInputReader] Действие '{actionName}' не найдено в {actions.name}.", this);
            return action;
        }

        private static Vector2 Read(InputAction action) => action?.ReadValue<Vector2>() ?? Vector2.zero;
        private static bool Pressed(InputAction action) => action != null && action.WasPressedThisFrame();
        private static bool Held(InputAction action) => action != null && action.IsPressed();
    }
}
