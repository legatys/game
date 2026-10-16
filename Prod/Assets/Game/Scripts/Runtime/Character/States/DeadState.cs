using UnityEngine;

namespace Game.Character.States
{
    /// <summary>Смерть: ввод игнорируется, персонаж останавливается. Выход — через PlayerCharacter.Respawn().</summary>
    public sealed class DeadState : CharacterState
    {
        public DeadState(PlayerCharacter player) : base(player) { }

        public override string Name => "Dead";

        protected override void OnTick(float deltaTime)
        {
            Motor.AccelerateTowards(Vector3.zero, Config.deceleration, deltaTime);
        }
    }
}
