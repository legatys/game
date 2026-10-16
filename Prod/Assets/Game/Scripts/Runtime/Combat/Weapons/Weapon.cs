using System;
using UnityEngine;

namespace Game.Combat.Weapons
{
    /// <summary>
    /// Экземпляр оружия в руках (модель): патроны, перезарядка, кулдаун, разброс.
    /// Обычный C#-класс, не MonoBehaviour — его тикает владелец (WeaponInventory или турель).
    /// Поведение делегируется стратегиям <see cref="IFireMode"/> и <see cref="IShotStrategy"/>.
    /// </summary>
    public sealed class Weapon
    {
        private readonly IFireMode _fireMode;
        private readonly IShotStrategy _shot;

        private float _cooldown;
        private float _bloom;
        private float _reloadTimer;
        private bool _isAiming;

        public Weapon(WeaponDefinition definition, WeaponView view, IFireMode fireMode, IShotStrategy shot)
        {
            Definition = definition;
            View = view;
            _fireMode = fireMode;
            _shot = shot;
            AmmoInMagazine = definition.magazineSize;
            ReserveAmmo = definition.startingReserve;
        }

        public WeaponDefinition Definition { get; }
        public WeaponView View { get; }

        public int AmmoInMagazine { get; private set; }
        public int ReserveAmmo { get; private set; }
        /// <summary>Бесконечный запас (для ИИ).</summary>
        public bool InfiniteReserve { get; set; }
        public bool IsReloading { get; private set; }
        public float ReloadProgress => IsReloading ? 1f - _reloadTimer / Definition.reloadTime : 0f;

        public bool IsEmpty => AmmoInMagazine <= 0 && !IsReloading;
        public bool IsReadyToFire => !IsReloading && _cooldown <= 0f && AmmoInMagazine > 0;
        /// <summary>Текущий разброс в градусах (база от прицеливания + "раскрытие" от стрельбы).</summary>
        public float CurrentSpread => (_isAiming ? Definition.aimSpread : Definition.hipSpread) + _bloom;

        public event Action<Weapon> Fired;
        public event Action<Weapon> DryFired;
        public event Action<Weapon> AmmoChanged;
        public event Action<Weapon> ReloadStarted;
        public event Action<Weapon> ReloadFinished;

        /// <summary>Вызывается владельцем каждый кадр, пока оружие в руках.</summary>
        public void Tick(float deltaTime, bool triggerPressed, bool triggerHeld, in AimData aim)
        {
            _isAiming = aim.IsAiming;
            _cooldown = Mathf.Max(0f, _cooldown - deltaTime);
            _bloom = Mathf.MoveTowards(_bloom, 0f, Definition.spreadRecovery * deltaTime);

            UpdateReload(deltaTime);
            _fireMode.Tick(this, triggerPressed, triggerHeld, aim, deltaTime);
        }

        /// <summary>Попытка одного выстрела. Вызывается режимом огня.</summary>
        public bool TryFire(in AimData aim)
        {
            if (IsReloading || _cooldown > 0f) return false;

            if (AmmoInMagazine <= 0)
            {
                _cooldown = 0.25f;
                if (View != null) View.PlaySound(Definition.emptySound);
                DryFired?.Invoke(this);
                if (Definition.autoReloadWhenEmpty) TryReload();
                return false;
            }

            AmmoInMagazine--;
            _cooldown = Definition.FireInterval;

            var origin = View != null ? View.Muzzle.position : aim.AimPoint;
            var spread = CurrentSpread * aim.SpreadMultiplier;
            _shot.Fire(new ShotContext(Definition, origin, aim.AimPoint, aim.Owner, spread));
            _bloom = Mathf.Min(_bloom + Definition.spreadPerShot, Definition.maxBloom);

            if (View != null) View.PlayFireEffects(Definition.fireSound);
            Fired?.Invoke(this);
            AmmoChanged?.Invoke(this);
            return true;
        }

        public bool TryReload()
        {
            if (IsReloading || AmmoInMagazine >= Definition.magazineSize) return false;
            if (ReserveAmmo <= 0 && !InfiniteReserve) return false;

            IsReloading = true;
            _reloadTimer = Definition.reloadTime;
            if (View != null) View.PlaySound(Definition.reloadSound);
            ReloadStarted?.Invoke(this);
            return true;
        }

        public void CancelReload()
        {
            IsReloading = false;
            _reloadTimer = 0f;
        }

        /// <summary>Убрать оружие: сброс режима огня и перезарядки.</summary>
        public void Holster()
        {
            CancelReload();
            _fireMode.Reset();
            if (View != null) View.SetVisible(false);
        }

        public void Draw()
        {
            if (View != null) View.SetVisible(true);
        }

        /// <summary>Добавить патроны в запас. Возвращает, сколько реально добавлено.</summary>
        public int AddReserveAmmo(int amount)
        {
            var before = ReserveAmmo;
            ReserveAmmo = Mathf.Min(Definition.maxReserve, ReserveAmmo + Mathf.Max(0, amount));
            if (ReserveAmmo != before) AmmoChanged?.Invoke(this);
            return ReserveAmmo - before;
        }

        private void UpdateReload(float deltaTime)
        {
            if (!IsReloading) return;

            _reloadTimer -= deltaTime;
            if (_reloadTimer > 0f) return;

            var needed = Definition.magazineSize - AmmoInMagazine;
            var taken = InfiniteReserve ? needed : Mathf.Min(needed, ReserveAmmo);
            if (!InfiniteReserve) ReserveAmmo -= taken;
            AmmoInMagazine += taken;
            IsReloading = false;

            ReloadFinished?.Invoke(this);
            AmmoChanged?.Invoke(this);
        }
    }
}
