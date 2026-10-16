namespace Game.Combat.Weapons
{
    /// <summary>
    /// Паттерн Strategy: КОГДА оружие стреляет (полуавтомат, автомат, очередь).
    /// Получает состояние спускового крючка и просит оружие выстрелить через Weapon.TryFire().
    /// </summary>
    public interface IFireMode
    {
        void Tick(Weapon weapon, bool triggerPressed, bool triggerHeld, in AimData aim, float deltaTime);
        void Reset();
    }

    /// <summary>Один выстрел на одно нажатие. Нажатие во время кулдауна запоминается ненадолго.</summary>
    public sealed class SemiAutoFireMode : IFireMode
    {
        private const float QueueTime = 0.15f;
        private float _queued;

        public void Tick(Weapon weapon, bool triggerPressed, bool triggerHeld, in AimData aim, float deltaTime)
        {
            if (triggerPressed) _queued = QueueTime;
            if (_queued <= 0f) return;

            _queued -= deltaTime;
            if (weapon.IsReadyToFire || weapon.IsEmpty)
            {
                weapon.TryFire(aim);
                _queued = 0f;
            }
        }

        public void Reset() => _queued = 0f;
    }

    /// <summary>Стреляет, пока зажат курок.</summary>
    public sealed class FullAutoFireMode : IFireMode
    {
        private bool _dryFired;

        public void Tick(Weapon weapon, bool triggerPressed, bool triggerHeld, in AimData aim, float deltaTime)
        {
            if (!triggerHeld)
            {
                _dryFired = false;
                return;
            }

            // Щелчок пустого магазина — один раз на нажатие, а не каждый кадр.
            if (weapon.IsEmpty && _dryFired) return;
            _dryFired = weapon.IsEmpty;

            if (weapon.IsReadyToFire || weapon.IsEmpty) weapon.TryFire(aim);
        }

        public void Reset() => _dryFired = false;
    }

    /// <summary>Очередь из N выстрелов на одно нажатие, затем пауза.</summary>
    public sealed class BurstFireMode : IFireMode
    {
        private readonly int _burstCount;
        private readonly float _burstCooldown;
        private int _shotsLeft;
        private float _cooldown;

        public BurstFireMode(int burstCount, float burstCooldown)
        {
            _burstCount = burstCount < 1 ? 1 : burstCount;
            _burstCooldown = burstCooldown;
        }

        public void Tick(Weapon weapon, bool triggerPressed, bool triggerHeld, in AimData aim, float deltaTime)
        {
            _cooldown -= deltaTime;

            if (triggerPressed && _shotsLeft == 0 && _cooldown <= 0f) _shotsLeft = _burstCount;
            if (_shotsLeft == 0 || !weapon.IsReadyToFire && !weapon.IsEmpty) return;

            if (!weapon.TryFire(aim) || --_shotsLeft == 0)
            {
                _shotsLeft = 0;
                _cooldown = _burstCooldown;
            }
        }

        public void Reset()
        {
            _shotsLeft = 0;
            _cooldown = 0f;
        }
    }
}
