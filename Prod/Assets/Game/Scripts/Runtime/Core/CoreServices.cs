using UnityEngine;

namespace Game.Core
{
    /// <summary>
    /// Composition Root для глобальных сервисов: создаётся автоматически до загрузки первой сцены,
    /// регистрирует сервисы в <see cref="ServiceLocator"/> и живёт всю игру (DontDestroyOnLoad).
    /// Новый глобальный сервис добавляйте сюда.
    /// </summary>
    public static class CoreServices
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Install()
        {
            var root = new GameObject("[Core Services]");
            Object.DontDestroyOnLoad(root);

            ServiceLocator.Register(new PoolService(root.transform));
        }

        /// <summary>Заспавнить из пула, а если сервиса нет (например, в тестовой сцене) — обычный Instantiate.</summary>
        public static GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation)
        {
            if (prefab == null) return null;
            if (ServiceLocator.TryGet(out PoolService pool)) return pool.Spawn(prefab, position, rotation);
            return Object.Instantiate(prefab, position, rotation);
        }
    }
}
