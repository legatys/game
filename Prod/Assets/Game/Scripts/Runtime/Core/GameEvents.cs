using Game.Character;
using Game.Combat;
using UnityEngine;

namespace Game.Core
{
    // Все глобальные события игры в одном месте — легко увидеть, что вообще можно слушать.

    /// <summary>Кто-то получил урон (публикует <see cref="Health"/>).</summary>
    public readonly struct DamageDealtEvent : IGameEvent
    {
        public readonly DamageInfo Info;
        public readonly Health Target;
        public readonly bool Killed;

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
        public readonly PlayerCharacter Player;
        public PlayerSpawnedEvent(PlayerCharacter player) => Player = player;
    }

    /// <summary>Игрок погиб, возрождение через RespawnDelay секунд.</summary>
    public readonly struct PlayerDiedEvent : IGameEvent
    {
        public readonly PlayerCharacter Player;
        public readonly float RespawnDelay;

        public PlayerDiedEvent(PlayerCharacter player, float respawnDelay)
        {
            Player = player;
            RespawnDelay = respawnDelay;
        }
    }

    /// <summary>Пауза включена/выключена (публикует PauseMenu).</summary>
    public readonly struct GamePausedEvent : IGameEvent
    {
        public readonly bool IsPaused;
        public GamePausedEvent(bool isPaused) => IsPaused = isPaused;
    }

    /// <summary>Что-то взорвалось (для тряски камеры, звука и т.д.).</summary>
    public readonly struct ExplosionEvent : IGameEvent
    {
        public readonly Vector3 Position;
        public readonly float Radius;

        public ExplosionEvent(Vector3 position, float radius)
        {
            Position = position;
            Radius = radius;
        }
    }
}
