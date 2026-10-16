using Game.Core;
using UnityEngine;

namespace Game.AI.States
{
    /// <summary>Нет цели: башня медленно вращается.</summary>
    public sealed class TurretIdleState : IState
    {
        private readonly EnemyTurret _turret;
        public TurretIdleState(EnemyTurret turret) => _turret = turret;

        public string Name => "Idle";
        public void Enter() { }
        public void Exit() { }

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

        public TurretAttackState(EnemyTurret turret) => _turret = turret;

        public string Name => "Attack";
        public void Enter() => _timer = -0.4f; // реакция перед первой очередью
        public void Exit() { }

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

        public TurretDeadState(EnemyTurret turret) => _turret = turret;

        public string Name => "Dead";

        public void Enter()
        {
            _timer = _turret.RespawnDelay;
            _turret.SetVisible(false);
        }

        public void Exit() => _turret.SetVisible(true);

        public void Tick(float deltaTime)
        {
            _timer -= deltaTime;
            if (_timer <= 0f) _turret.Health.Revive();
        }
    }
}
