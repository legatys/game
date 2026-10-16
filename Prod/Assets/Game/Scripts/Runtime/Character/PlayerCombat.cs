using System;
using Game.Cameras;
using Game.Combat.Weapons;
using Game.Core;
using UnityEngine;

namespace Game.Character
{
    /// <summary>
    /// Боевая часть игрока: прицеливание, стрельба, перезарядка, смена оружия, отдача камеры.
    /// Берёт точку прицела из камеры (центр экрана) и передаёт её текущему оружию.
    /// Пробрасывает события оружия наружу, чтобы подписчикам не нужно было переподписываться при смене ствола.
    /// </summary>
    [DefaultExecutionOrder(10)]
    [RequireComponent(typeof(PlayerCharacter), typeof(WeaponInventory))]
    public sealed class PlayerCombat : MonoBehaviour, IAimProvider
    {
        /// <summary>Дальность луча прицеливания, м.</summary>
        [SerializeField] private float aimRange = 250f;
        [Tooltip("Сколько секунд после выстрела персонаж продолжает смотреть в прицел")]
        [SerializeField] private float combatFacingTime = 0.6f;

        /// <summary>Множитель разброса на скорости бега.</summary>
        [Header("Множители разброса")]
        [SerializeField] private float movingSpreadMultiplier = 1.6f;
        /// <summary>Множитель разброса в воздухе.</summary>
        [SerializeField] private float airborneSpreadMultiplier = 2.5f;
        /// <summary>Множитель разброса в приседе.</summary>
        [SerializeField] private float crouchSpreadMultiplier = 0.7f;

        private PlayerCharacter _player;
        private WeaponInventory _inventory;
        private ICameraRig _camera;
        private Weapon _subscribedWeapon;
        private float _combatTimer;

        /// <summary>Инвентарь оружия.</summary>
        public WeaponInventory Inventory => _inventory;
        /// <summary>Оружие в руках.</summary>
        public Weapon CurrentWeapon => _inventory.Current;
        /// <summary>Прицеливается (ПКМ и не спринт).</summary>
        public bool IsAiming { get; private set; }
        /// <inheritdoc />
        public Vector3 AimPoint { get; private set; }
        /// <summary>Прицел или выстрел недавно — персонаж смотрит туда же, куда камера.</summary>
        public bool WantsCombatFacing => IsAiming || _combatTimer > 0f;
        /// <inheritdoc />
        public bool IsCombatReady => WantsCombatFacing && !_player.IsDead;

        /// <summary>Текущее оружие выстрелило.</summary>
        public event Action<Weapon> WeaponFired;
        /// <summary>Текущее оружие начало перезарядку.</summary>
        public event Action<Weapon> ReloadStarted;

        private void Awake()
        {
            _player = GetComponent<PlayerCharacter>();
            _inventory = GetComponent<WeaponInventory>();
            AimPoint = transform.position + transform.forward * 10f;
        }

        private void OnEnable()
        {
            _inventory.WeaponEquipped += SubscribeTo;
            SubscribeTo(_inventory.Current);
        }

        private void OnDisable()
        {
            _inventory.WeaponEquipped -= SubscribeTo;
            SubscribeTo(null);
        }

        private void Update()
        {
            var input = _player.Input;
            if (input == null) return;
            if (_camera == null || _camera is UnityEngine.Object unityObject && unityObject == null)
                ServiceLocator.TryGet(out _camera);

            var deltaTime = Time.deltaTime;
            var canUseWeapons = !_player.IsDead;

            IsAiming = canUseWeapons && input.AimHeld && !_player.IsSprinting;
            HandleWeaponSwitching(canUseWeapons);

            AimPoint = _camera != null
                ? _camera.GetAimPoint(aimRange, transform)
                : _player.PivotPosition + transform.forward * aimRange;

            var weapon = _inventory.Current;
            if (weapon == null) return;

            if (canUseWeapons && input.ReloadPressed) weapon.TryReload();

            var fireHeld = canUseWeapons && input.FireHeld;
            var firePressed = canUseWeapons && input.FirePressed;
            _combatTimer = fireHeld ? combatFacingTime : _combatTimer - deltaTime;

            var aim = new AimData(AimPoint, gameObject, IsAiming, ComputeSpreadMultiplier());
            weapon.Tick(deltaTime, firePressed, fireHeld, aim);
        }

        private void HandleWeaponSwitching(bool allowed)
        {
            if (!allowed) return;

            var input = _player.Input;
            var slot = input.WeaponSlotPressed;
            if (slot >= 0) _inventory.Equip(slot);
            else if (input.NextWeaponPressed) _inventory.EquipNext();
            else if (input.PreviousWeaponPressed) _inventory.EquipPrevious();
        }

        private float ComputeSpreadMultiplier()
        {
            var motor = _player.Motor;
            var speed01 = Mathf.Clamp01(motor.PlanarVelocity.magnitude / Mathf.Max(0.01f, _player.Config.runSpeed));
            var multiplier = Mathf.Lerp(1f, movingSpreadMultiplier, speed01);
            if (!motor.IsGrounded) multiplier *= airborneSpreadMultiplier;
            if (_player.IsCrouching) multiplier *= crouchSpreadMultiplier;
            return multiplier;
        }

        private void SubscribeTo(Weapon weapon)
        {
            if (_subscribedWeapon != null)
            {
                _subscribedWeapon.Fired -= OnWeaponFired;
                _subscribedWeapon.ReloadStarted -= OnReloadStarted;
            }

            _subscribedWeapon = weapon;
            if (weapon == null) return;

            weapon.Fired += OnWeaponFired;
            weapon.ReloadStarted += OnReloadStarted;
        }

        private void OnWeaponFired(Weapon weapon)
        {
            var definition = weapon.Definition;
            if (_camera != null)
                _camera.AddRecoil(definition.recoilPitch, UnityEngine.Random.Range(-definition.recoilYaw, definition.recoilYaw));
            WeaponFired?.Invoke(weapon);
        }

        private void OnReloadStarted(Weapon weapon) => ReloadStarted?.Invoke(weapon);
    }
}
