using Game.Core;
using Game.Inputs;

namespace Game.Character.States
{
    /// <summary>
    /// Базовое состояние персонажа. Паттерн Template Method: Tick() фиксирует порядок
    /// "логика состояния → физика мотора", а наследники переопределяют только OnTick().
    /// </summary>
    public abstract class CharacterState : IState
    {
        protected readonly PlayerCharacter Player;

        protected CharacterState(PlayerCharacter player) => Player = player;

        public abstract string Name { get; }

        protected CharacterMotor Motor => Player.Motor;
        protected MovementConfig Config => Player.Config;
        protected IPlayerInput Input => Player.Input;

        public virtual void Enter() { }
        public virtual void Exit() { }

        public void Tick(float deltaTime)
        {
            OnTick(deltaTime);
            Motor.Simulate(Config.gravity, deltaTime);
        }

        protected abstract void OnTick(float deltaTime);
    }
}
