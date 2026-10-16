using UnityEngine;

namespace Game.Core
{
    /// <summary>Ссылка экземпляра на свой пул. Вызовите Release(), чтобы вернуть объект.</summary>
    public sealed class PooledObject : MonoBehaviour
    {
        private IPoolable[] _listeners;
        private float _releaseTimer = -1f;

        /// <summary>Пул-владелец (null — объект создан не пулом).</summary>
        internal GameObjectPool Pool { get; set; }

        /// <summary>Вернуть объект в пул (или уничтожить, если пула нет).</summary>
        public void Release()
        {
            _releaseTimer = -1f;
            if (Pool != null) Pool.Release(this);
            else Destroy(gameObject);
        }

        /// <summary>Вернуть в пул через seconds секунд.</summary>
        public void ReleaseAfter(float seconds) => _releaseTimer = Mathf.Max(0f, seconds);

        /// <summary>Вызывает IPoolable.OnSpawned у всех компонентов.</summary>
        internal void NotifySpawned()
        {
            _listeners ??= GetComponentsInChildren<IPoolable>(true);
            foreach (var listener in _listeners) listener.OnSpawned();
        }

        /// <summary>Вызывает IPoolable.OnDespawned у всех компонентов.</summary>
        internal void NotifyDespawned()
        {
            _listeners ??= GetComponentsInChildren<IPoolable>(true);
            foreach (var listener in _listeners) listener.OnDespawned();
        }

        private void Update()
        {
            if (_releaseTimer < 0f) return;

            _releaseTimer -= Time.deltaTime;
            if (_releaseTimer <= 0f) Release();
        }
    }
}
