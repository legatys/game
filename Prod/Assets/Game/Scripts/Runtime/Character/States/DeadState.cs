using UnityEngine;

namespace Game.Character.States
{
    /// <summary>Смерть: ввод игнорируется, персонаж останавливается. Выход — через PlayerCharacter.Respawn().</summary>
    public sealed class DeadState : CharacterState
    {
        /// <summary>Создаёт состояние для игрока <paramref name="player"/>.</summary>
        public DeadState(PlayerCharacter player) : base(player) { }

        /// <inheritdoc />
        public override string Name => "Dead";

        /// <summary>Плавная остановка.</summary>
        protected override void OnTick(float deltaTime)
        {
            Motor.AccelerateTowards(Vector3.zero, Config.deceleration, deltaTime);
        }
    }
}
