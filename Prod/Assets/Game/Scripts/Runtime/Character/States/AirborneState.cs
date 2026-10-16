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

        public AirborneState(PlayerCharacter player) : base(player) { }

        public override string Name => Motor.Velocity.y > 0f ? "Jump" : "Fall";

        public override void Enter()
        {
            if (Player.TryConsumeJumpRequest()) Player.PerformJump();
            _maxAirSpeed = Mathf.Max(Config.runSpeed, Motor.PlanarVelocity.magnitude);
        }

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
