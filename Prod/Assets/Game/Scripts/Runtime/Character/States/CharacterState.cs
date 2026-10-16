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
        /// <summary>Игрок-владелец.</summary>
        protected readonly PlayerCharacter Player;

        /// <summary>Создаёт состояние для игрока <paramref name="player"/>.</summary>
        protected CharacterState(PlayerCharacter player) => Player = player;

        /// <inheritdoc />
        public abstract string Name { get; }

        /// <summary>Мотор игрока.</summary>
        protected CharacterMotor Motor => Player.Motor;
        /// <summary>Настройки передвижения.</summary>
        protected MovementConfig Config => Player.Config;
        /// <summary>Ввод игрока.</summary>
        protected IPlayerInput Input => Player.Input;

        /// <inheritdoc />
        public virtual void Enter() { }
        /// <inheritdoc />
        public virtual void Exit() { }

        /// <summary>Шаблонный метод: OnTick() состояния, затем Motor.Simulate().</summary>
        public void Tick(float deltaTime)
        {
            OnTick(deltaTime);
            Motor.Simulate(Config.gravity, deltaTime);
        }

        /// <summary>Логика состояния в кадре (до физики мотора).</summary>
        protected abstract void OnTick(float deltaTime);
    }
}
