namespace Game.Character.States
{
    /// <summary>Присед: капсула ниже, скорость меньше. Включается/выключается нажатием Crouch.</summary>
    public sealed class CrouchState : CharacterState
    {
        public CrouchState(PlayerCharacter player) : base(player) { }

        public override string Name => "Crouch";

        public override void Enter()
        {
            Player.RefillAirJumps();
            Motor.SetTargetHeight(Config.crouchHeight, Config.heightChangeSpeed);
        }

        protected override void OnTick(float deltaTime)
        {
            var wish = Player.GetMoveDirection();
            var acceleration = wish.sqrMagnitude > 0.0001f ? Config.acceleration : Config.deceleration;

            Motor.AccelerateTowards(wish * Config.crouchSpeed, acceleration, deltaTime);
            Player.UpdateFacing(wish, deltaTime);
        }
    }
}
