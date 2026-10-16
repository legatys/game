namespace Game.Character.States
{
    /// <summary>На земле: стоим, идём (при прицеливании), бежим или спринтуем.</summary>
    public sealed class GroundedState : CharacterState
    {
        public GroundedState(PlayerCharacter player) : base(player) { }

        public override string Name => Player.IsSprinting ? "Sprint" : "Grounded";

        public override void Enter()
        {
            Player.RefillAirJumps();
            Motor.SetTargetHeight(Config.standHeight, Config.heightChangeSpeed);
        }

        protected override void OnTick(float deltaTime)
        {
            var wish = Player.GetMoveDirection();
            var hasInput = wish.sqrMagnitude > 0.0001f;
            var acceleration = hasInput ? Config.acceleration : Config.deceleration;

            Motor.AccelerateTowards(wish * Player.GetGroundSpeed(), acceleration, deltaTime);
            Player.UpdateFacing(wish, deltaTime);
        }
    }
}
