using System.Collections.Generic;
using UnityEngine;

namespace Game.Core
{
    /// <summary>Компонент на пулируемом объекте получает уведомления о выдаче/возврате в пул.</summary>
    public interface IPoolable
    {
        /// <summary>Объект выдан из пула (сброс состояния).</summary>
        void OnSpawned();
        /// <summary>Объект возвращён в пул.</summary>
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

        /// <summary>Создаёт пул префаба <paramref name="prefab"/>; экземпляры хранятся внутри <paramref name="root"/>.</summary>
        public GameObjectPool(GameObject prefab, Transform root)
        {
            _prefab = prefab;
            _root = root;
        }

        /// <summary>Выдать объект (новый или из запаса) в позиции и повороте.</summary>
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

        /// <summary>Вернуть объект в пул.</summary>
        public void Release(PooledObject item)
        {
            if (item == null || !item.gameObject.activeSelf) return;

            item.NotifyDespawned();
            item.gameObject.SetActive(false);
            item.transform.SetParent(_root, false);
            _inactive.Push(item);
        }

        /// <summary>Заранее создать <paramref name="count"/> неактивных экземпляров.</summary>
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

        /// <summary>Создаёт сервис; пулы хранят объекты внутри <paramref name="root"/>.</summary>
        public PoolService(Transform root) => _root = root;

        /// <summary>Выдать экземпляр префаба из его пула.</summary>
        public GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation)
        {
            if (prefab == null) return null;
            return GetPool(prefab).Get(position, rotation).gameObject;
        }

        /// <summary>Выдать экземпляр и вернуть его компонент T.</summary>
        public T Spawn<T>(T prefab, Vector3 position, Quaternion rotation) where T : Component
        {
            if (prefab == null) return null;
            var instance = Spawn(prefab.gameObject, position, rotation);
            return instance.GetComponent<T>();
        }

        /// <summary>Заранее создать экземпляры префаба.</summary>
        public void Prewarm(GameObject prefab, int count)
        {
            if (prefab != null) GetPool(prefab).Prewarm(count);
        }

        /// <summary>Вернуть экземпляр в пул (или уничтожить, если он не из пула).</summary>
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
