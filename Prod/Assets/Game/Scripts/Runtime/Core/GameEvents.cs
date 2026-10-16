using Game.Character;
using Game.Combat;
using UnityEngine;

namespace Game.Core
{
    // Все глобальные события игры в одном месте — легко увидеть, что вообще можно слушать.

    /// <summary>Кто-то получил урон (публикует <see cref="Health"/>).</summary>
    public readonly struct DamageDealtEvent : IGameEvent
    {
        /// <summary>Описание попадания.</summary>
        public readonly DamageInfo Info;
        /// <summary>Кто получил урон.</summary>
        public readonly Health Target;
        /// <summary>Попадание было смертельным.</summary>
        public readonly bool Killed;

        /// <summary>Создаёт событие.</summary>
        public DamageDealtEvent(DamageInfo info, Health target, bool killed)
        {
            Info = info;
            Target = target;
            Killed = killed;
        }
    }

    /// <summary>Игрок появился или возродился (публикует PlayerSpawner).</summary>
    public readonly struct PlayerSpawnedEvent : IGameEvent
    {
        /// <summary>Появившийся игрок.</summary>
        public readonly PlayerCharacter Player;
        /// <summary>Создаёт событие.</summary>
        public PlayerSpawnedEvent(PlayerCharacter player) => Player = player;
    }

    /// <summary>Игрок погиб, возрождение через RespawnDelay секунд.</summary>
    public readonly struct PlayerDiedEvent : IGameEvent
    {
        /// <summary>Погибший игрок.</summary>
        public readonly PlayerCharacter Player;
        /// <summary>Через сколько секунд возрождение.</summary>
        public readonly float RespawnDelay;

        /// <summary>Создаёт событие.</summary>
        public PlayerDiedEvent(PlayerCharacter player, float respawnDelay)
        {
            Player = player;
            RespawnDelay = respawnDelay;
        }
    }

    /// <summary>Пауза включена/выключена (публикует PauseMenu).</summary>
    public readonly struct GamePausedEvent : IGameEvent
    {
        /// <summary>true — пауза включена.</summary>
        public readonly bool IsPaused;
        /// <summary>Создаёт событие.</summary>
        public GamePausedEvent(bool isPaused) => IsPaused = isPaused;
    }

    /// <summary>Что-то взорвалось (для тряски камеры, звука и т.д.).</summary>
    public readonly struct ExplosionEvent : IGameEvent
    {
        /// <summary>Центр взрыва.</summary>
        public readonly Vector3 Position;
        /// <summary>Радиус взрыва.</summary>
        public readonly float Radius;

        /// <summary>Создаёт событие.</summary>
        public ExplosionEvent(Vector3 position, float radius)
        {
            Position = position;
            Radius = radius;
        }
    }
}
