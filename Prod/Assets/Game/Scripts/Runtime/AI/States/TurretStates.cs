using Game.Core;
using UnityEngine;

namespace Game.AI.States
{
    /// <summary>Нет цели: башня медленно вращается.</summary>
    public sealed class TurretIdleState : IState
    {
        private readonly EnemyTurret _turret;
        /// <summary>Создаёт состояние для турели <paramref name="turret"/>.</summary>
        public TurretIdleState(EnemyTurret turret) => _turret = turret;

        /// <inheritdoc />
        public string Name => "Idle";
        /// <inheritdoc />
        public void Enter() { }
        /// <inheritdoc />
        public void Exit() { }

        /// <summary>Вращает башню и держит курок отпущенным.</summary>
        public void Tick(float deltaTime)
        {
            _turret.Scan(deltaTime);
            _turret.TickWeapon(false, deltaTime, 180f);
        }
    }

    /// <summary>Цель видна: доворачиваемся и стреляем очередями с паузами.</summary>
    public sealed class TurretAttackState : IState
    {
        private const float BurstDuration = 0.8f;
        private const float PauseDuration = 0.7f;

        private readonly EnemyTurret _turret;
        private float _timer;

        /// <summary>Создаёт состояние для турели <paramref name="turret"/>.</summary>
        public TurretAttackState(EnemyTurret turret) => _turret = turret;

        /// <inheritdoc />
        public string Name => "Attack";
        /// <summary>Сбрасывает таймер: перед первой очередью есть время реакции.</summary>
        public void Enter() => _timer = -0.4f; // реакция перед первой очередью
        /// <inheritdoc />
        public void Exit() { }

        /// <summary>Наводится на цель и стреляет очередями с паузами.</summary>
        public void Tick(float deltaTime)
        {
            _timer += deltaTime;
            if (_timer > BurstDuration + PauseDuration) _timer = 0f;

            var error = _turret.AimAt(_turret.TargetPoint, deltaTime);
            var firing = _timer >= 0f && _timer < BurstDuration;
            _turret.TickWeapon(firing, deltaTime, error);
        }
    }

    /// <summary>Уничтожена: скрыта, через RespawnDelay восстанавливается.</summary>
    public sealed class TurretDeadState : IState
    {
        private readonly EnemyTurret _turret;
        private float _timer;

        /// <summary>Создаёт состояние для турели <paramref name="turret"/>.</summary>
        public TurretDeadState(EnemyTurret turret) => _turret = turret;

        /// <inheritdoc />
        public string Name => "Dead";

        /// <summary>Скрывает турель и запускает таймер восстановления.</summary>
        public void Enter()
        {
            _timer = _turret.RespawnDelay;
            _turret.SetVisible(false);
        }

        /// <summary>Показывает турель снова.</summary>
        public void Exit() => _turret.SetVisible(true);

        /// <summary>Отсчитывает время и восстанавливает здоровье.</summary>
        public void Tick(float deltaTime)
        {
            _timer -= deltaTime;
            if (_timer <= 0f) _turret.Health.Revive();
        }
    }
}
