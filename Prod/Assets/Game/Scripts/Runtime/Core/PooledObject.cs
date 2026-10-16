using UnityEngine;

namespace Game.Core
{
    /// <summary>Ссылка экземпляра на свой пул. Вызовите Release(), чтобы вернуть объект.</summary>
    public sealed class PooledObject : MonoBehaviour
    {
        private IPoolable[] _listeners;
        private float _releaseTimer = -1f;

        internal GameObjectPool Pool { get; set; }

        public void Release()
        {
            _releaseTimer = -1f;
            if (Pool != null) Pool.Release(this);
            else Destroy(gameObject);
        }

        /// <summary>Вернуть в пул через seconds секунд.</summary>
        public void ReleaseAfter(float seconds) => _releaseTimer = Mathf.Max(0f, seconds);

        internal void NotifySpawned()
        {
            _listeners ??= GetComponentsInChildren<IPoolable>(true);
            foreach (var listener in _listeners) listener.OnSpawned();
        }

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
