using System.Collections.Generic;
using UnityEngine;

namespace Game.Core
{
    /// <summary>Компонент на пулируемом объекте получает уведомления о выдаче/возврате в пул.</summary>
    public interface IPoolable
    {
        void OnSpawned();
        void OnDespawned();
    }

    /// <summary>
    /// Паттерн Object Pool: пул экземпляров одного префаба. Вместо Instantiate/Destroy
    /// для пуль, трассеров и эффектов объекты переиспользуются — меньше мусора и лагов.
    /// </summary>
    public sealed class GameObjectPool
    {
        private readonly GameObject _prefab;
        private readonly Transform _root;
        private readonly Stack<PooledObject> _inactive = new Stack<PooledObject>();

        public GameObjectPool(GameObject prefab, Transform root)
        {
            _prefab = prefab;
            _root = root;
        }

        public PooledObject Get(Vector3 position, Quaternion rotation)
        {
            PooledObject item = null;
            while (_inactive.Count > 0 && item == null)
                item = _inactive.Pop(); // объект мог быть уничтожен извне

            if (item == null) item = CreateInstance();

            item.transform.SetPositionAndRotation(position, rotation);
            item.gameObject.SetActive(true);
            item.NotifySpawned();
            return item;
        }

        public void Release(PooledObject item)
        {
            if (item == null || !item.gameObject.activeSelf) return;

            item.NotifyDespawned();
            item.gameObject.SetActive(false);
            item.transform.SetParent(_root, false);
            _inactive.Push(item);
        }

        public void Prewarm(int count)
        {
            for (var i = 0; i < count; i++)
            {
                var item = CreateInstance();
                item.gameObject.SetActive(false);
                _inactive.Push(item);
            }
        }

        private PooledObject CreateInstance()
        {
            var instance = Object.Instantiate(_prefab, _root);
            var pooled = instance.GetComponent<PooledObject>();
            if (pooled == null) pooled = instance.AddComponent<PooledObject>();
            pooled.Pool = this;
            return pooled;
        }
    }

    /// <summary>
    /// Сервис пулов: хранит по пулу на каждый префаб. Регистрируется в ServiceLocator при старте игры
    /// (<see cref="CoreServices"/>) и живёт между сценами.
    /// </summary>
    public sealed class PoolService
    {
        private readonly Dictionary<GameObject, GameObjectPool> _pools = new Dictionary<GameObject, GameObjectPool>();
        private readonly Transform _root;

        public PoolService(Transform root) => _root = root;

        public GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation)
        {
            if (prefab == null) return null;
            return GetPool(prefab).Get(position, rotation).gameObject;
        }

        public T Spawn<T>(T prefab, Vector3 position, Quaternion rotation) where T : Component
        {
            if (prefab == null) return null;
            var instance = Spawn(prefab.gameObject, position, rotation);
            return instance.GetComponent<T>();
        }

        public void Prewarm(GameObject prefab, int count)
        {
            if (prefab != null) GetPool(prefab).Prewarm(count);
        }

        public static void Release(GameObject instance)
        {
            if (instance == null) return;

            var pooled = instance.GetComponent<PooledObject>();
            if (pooled != null) pooled.Release();
            else Object.Destroy(instance);
        }

        private GameObjectPool GetPool(GameObject prefab)
        {
            if (!_pools.TryGetValue(prefab, out var pool))
            {
                pool = new GameObjectPool(prefab, _root);
                _pools[prefab] = pool;
            }

            return pool;
        }
    }
}
