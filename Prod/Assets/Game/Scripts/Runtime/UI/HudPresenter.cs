using Game.Cameras;
using Game.Character;
using Game.Combat;
using Game.Combat.Weapons;
using Game.Core;
using UnityEngine;

namespace Game.UI
{
    /// <summary>
    /// Presenter в паттерне MVP: слушает модель (Health, Weapon, EventBus) и обновляет <see cref="IHudView"/>.
    /// Дискретные вещи (урон, патроны, хит-маркер) — через события, непрерывные (разброс, перезарядка) — в Update.
    /// </summary>
    public sealed class HudPresenter : MonoBehaviour
    {
        [Tooltip("Урон, при котором красная вспышка экрана максимальна")]
        [SerializeField] private float fullFlashDamage = 40f;
        /// <summary>Показывать состояние и скорость игрока.</summary>
        [SerializeField] private bool showDebugInfo = true;

        private IHudView _view;
        private PlayerCharacter _player;
        private Weapon _weapon;
        private float _respawnTimer;

        private void Awake()
        {
            _view = GetComponent<IHudView>();
            if (_view == null)
            {
                Debug.LogError("[HudPresenter] На объекте нет компонента, реализующего IHudView.", this);
                enabled = false;
            }
        }

        private void OnEnable()
        {
            EventBus.Subscribe<PlayerSpawnedEvent>(OnPlayerSpawned);
            EventBus.Subscribe<PlayerDiedEvent>(OnPlayerDied);
            EventBus.Subscribe<DamageDealtEvent>(OnDamageDealt);
            if (ServiceLocator.TryGet(out PlayerCharacter player)) Bind(player);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<PlayerSpawnedEvent>(OnPlayerSpawned);
            EventBus.Unsubscribe<PlayerDiedEvent>(OnPlayerDied);
            EventBus.Unsubscribe<DamageDealtEvent>(OnDamageDealt);
            Unbind();
        }

        private void Update()
        {
            if (_respawnTimer > 0f)
            {
                _respawnTimer -= Time.deltaTime;
                _view.SetRespawnCountdown(_respawnTimer);
            }

            if (_player == null) return;

            _view.SetReload(_weapon != null && _weapon.IsReloading, _weapon?.ReloadProgress ?? 0f);
            _view.SetCrosshair(SpreadToPixels(_weapon?.CurrentSpread ?? 0f), _player.IsAiming);

            if (showDebugInfo)
            {
                var speed = _player.Motor.PlanarVelocity.magnitude;
                _view.SetDebugText($"State: {_player.CurrentStateName}\nSpeed: {speed:0.0} m/s\nAir jumps: {_player.AirJumpsRemaining}");
            }
        }

        // ---------- Привязка к игроку ----------

        private void Bind(PlayerCharacter player)
        {
            if (player == _player) return;
            Unbind();

            _player = player;
            _player.Health.Changed += OnHealthChanged;
            _player.Health.Damaged += OnPlayerDamaged;
            if (_player.Inventory != null) _player.Inventory.WeaponEquipped += OnWeaponEquipped;

            OnHealthChanged(_player.Health.Current, _player.Health.Max);
            OnWeaponEquipped(_player.Inventory != null ? _player.Inventory.Current : null);
        }

        private void Unbind()
        {
            BindWeapon(null);
            if (_player == null) return;

            _player.Health.Changed -= OnHealthChanged;
            _player.Health.Damaged -= OnPlayerDamaged;
            if (_player.Inventory != null) _player.Inventory.WeaponEquipped -= OnWeaponEquipped;
            _player = null;
        }

        private void BindWeapon(Weapon weapon)
        {
            if (_weapon != null) _weapon.AmmoChanged -= OnAmmoChanged;
            _weapon = weapon;
            if (_weapon != null) _weapon.AmmoChanged += OnAmmoChanged;
            OnAmmoChanged(_weapon);
        }

        // ---------- Обработчики ----------

        private void OnPlayerSpawned(PlayerSpawnedEvent e)
        {
            Bind(e.Player);
            _respawnTimer = 0f;
            _view.SetRespawnCountdown(0f);
            OnHealthChanged(e.Player.Health.Current, e.Player.Health.Max);
        }

        private void OnPlayerDied(PlayerDiedEvent e) => _respawnTimer = e.RespawnDelay;

        private void OnDamageDealt(DamageDealtEvent e)
        {
            if (_player == null || e.Target == _player.Health) return;
            if (e.Info.Instigator == _player.gameObject) _view.ShowHitMarker(e.Killed, e.Info.IsCritical);
        }

        private void OnHealthChanged(float current, float max) => _view.SetHealth(current, max);

        private void OnPlayerDamaged(DamageInfo info) => _view.ShowDamageFlash(Mathf.Clamp01(info.Amount / fullFlashDamage));

        private void OnWeaponEquipped(Weapon weapon) => BindWeapon(weapon);

        private void OnAmmoChanged(Weapon weapon)
        {
            if (weapon == null) _view.SetWeapon(null, 0, 0, 0);
            else _view.SetWeapon(weapon.Definition.displayName, weapon.AmmoInMagazine, weapon.Definition.magazineSize, weapon.ReserveAmmo);
        }

        /// <summary>Угол разброса → радиус прицела в пикселях для текущего FOV.</summary>
        private static float SpreadToPixels(float spreadDegrees)
        {
            var fov = ServiceLocator.TryGet(out ICameraRig rig) && rig.Camera != null ? rig.Camera.fieldOfView : 60f;
            var halfFov = Mathf.Tan(fov * 0.5f * Mathf.Deg2Rad);
            return Mathf.Tan(spreadDegrees * Mathf.Deg2Rad) / halfFov * (Screen.height * 0.5f);
        }
    }
}
