using UnityEngine;

namespace Game.Character.States
{
    /// <summary>
    /// В воздухе: прыжок (если вошли по нажатию), падение, прыжки в воздухе (двойной прыжок)
    /// и слабое управление в полёте с сохранением набранной скорости.
    /// </summary>
    public sealed class AirborneState : CharacterState
    {
        private float _maxAirSpeed;

        /// <summary>Создаёт состояние для игрока <paramref name="player"/>.</summary>
        public AirborneState(PlayerCharacter player) : base(player) { }

        /// <inheritdoc />
        public override string Name => Motor.Velocity.y > 0f ? "Jump" : "Fall";

        /// <summary>Выполняет прыжок, если вход был по нажатию, и запоминает скорость для управления в воздухе.</summary>
        public override void Enter()
        {
            if (Player.TryConsumeJumpRequest()) Player.PerformJump();
            _maxAirSpeed = Mathf.Max(Config.runSpeed, Motor.PlanarVelocity.magnitude);
        }

        /// <summary>Прыжки в воздухе и слабое управление направлением.</summary>
        protected override void OnTick(float deltaTime)
        {
            if (Player.HasJumpRequest && Player.AirJumpsRemaining > 0)
            {
                Player.TryConsumeJumpRequest();
                Player.AirJumpsRemaining--;
                Player.PerformJump();
            }

            var wish = Player.GetMoveDirection();
            if (wish.sqrMagnitude > 0.0001f)
                Motor.AccelerateTowards(wish * _maxAirSpeed, Config.airAcceleration, deltaTime);

            Player.UpdateFacing(wish, deltaTime);
        }
    }
}
