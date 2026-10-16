using System.Collections;
using Game.Cameras;
using Game.Character;
using Game.Combat;
using Game.Core;
using UnityEngine;

namespace Game.Gameplay
{
    /// <summary>
    /// Появление и возрождение игрока. Если игрок уже стоит в сцене — использует его,
    /// иначе создаёт из префаба. Сообщает о появлении/смерти через EventBus.
    /// </summary>
    public sealed class PlayerSpawner : MonoBehaviour
    {
        [SerializeField] private PlayerCharacter playerPrefab;
        [SerializeField] private Transform spawnPoint;
        [SerializeField] private float respawnDelay = 3f;

        private PlayerCharacter _player;

        public PlayerCharacter Player => _player;

        /// <summary>Настройка из кода. Вызывать до активации объекта.</summary>
        public void Configure(PlayerCharacter prefab, Transform spawn)
        {
            playerPrefab = prefab;
            spawnPoint = spawn;
        }

        private void Start()
        {
            if (spawnPoint == null) spawnPoint = transform;

            _player = FindFirstObjectByType<PlayerCharacter>();
            if (_player == null && playerPrefab != null)
                _player = Instantiate(playerPrefab, spawnPoint.position, spawnPoint.rotation);

            if (_player == null)
            {
                Debug.LogError("[PlayerSpawner] Нет игрока в сцене и не задан playerPrefab.", this);
                return;
            }

            _player.Health.Died += OnPlayerDied;
            if (ServiceLocator.TryGet(out ICameraRig rig)) rig.SetTarget(_player);
            EventBus.Publish(new PlayerSpawnedEvent(_player));
        }

        private void OnDestroy()
        {
            if (_player != null) _player.Health.Died -= OnPlayerDied;
        }

        private void OnPlayerDied(DamageInfo info)
        {
            EventBus.Publish(new PlayerDiedEvent(_player, respawnDelay));
            StartCoroutine(RespawnRoutine());
        }

        private IEnumerator RespawnRoutine()
        {
            yield return new WaitForSeconds(respawnDelay);
            _player.Respawn(spawnPoint.position, spawnPoint.rotation);
            EventBus.Publish(new PlayerSpawnedEvent(_player));
        }
    }
}
