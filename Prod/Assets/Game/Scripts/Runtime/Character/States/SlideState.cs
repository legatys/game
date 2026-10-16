using UnityEngine;

namespace Game.Character.States
{
    /// <summary>
    /// Подкат: рывок в направлении бега с низкой капсулой и постепенным торможением.
    /// Заканчивается по времени или когда скорость упала до скорости приседа.
    /// </summary>
    public sealed class SlideState : CharacterState
    {
        private Vector3 _direction;
        private float _speed;
        private float _timer;

        public SlideState(PlayerCharacter player) : base(player) { }

        public override string Name => "Slide";

        public bool IsFinished => _timer >= Config.slideMaxDuration || _speed <= Config.crouchSpeed;

        public override void Enter()
        {
            var planar = Motor.PlanarVelocity;
            _direction = planar.sqrMagnitude > 0.01f ? planar.normalized : Player.transform.forward;
            _speed = Mathf.Max(Config.slideStartSpeed, planar.magnitude);
            _timer = 0f;
            Motor.SetTargetHeight(Config.crouchHeight, Config.heightChangeSpeed * 2f);
        }

        public override void Exit() => Player.StartSlideCooldown();

        protected override void OnTick(float deltaTime)
        {
            _timer += deltaTime;
            _speed = Mathf.MoveTowards(_speed, 0f, Config.slideFriction * deltaTime);

            var wish = Player.GetMoveDirection();
            if (wish.sqrMagnitude > 0.01f)
                _direction = Vector3.Slerp(_direction, wish.normalized, Config.slideSteering * deltaTime * 10f).normalized;

            Motor.SetPlanarVelocity(_direction * _speed);
            Motor.RotateTowards(_direction, Config.rotationSpeed, deltaTime);
        }
    }
}
