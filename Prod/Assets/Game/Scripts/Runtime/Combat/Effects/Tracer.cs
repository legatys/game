using Game.Core;
using UnityEngine;

namespace Game.Combat
{
    /// <summary>Трассер пули: линия от дула до точки попадания, быстро сужается и возвращается в пул.</summary>
    [RequireComponent(typeof(LineRenderer))]
    public sealed class Tracer : MonoBehaviour, IPoolable
    {
        [SerializeField] private float lifetime = 0.06f;
        [SerializeField] private float width = 0.03f;

        private LineRenderer _line;
        private float _age;

        private void Awake()
        {
            _line = GetComponent<LineRenderer>();
            _line.positionCount = 2;
            _line.useWorldSpace = true;
        }

        public void Show(Vector3 from, Vector3 to)
        {
            _line.SetPosition(0, from);
            _line.SetPosition(1, to);
            _age = 0f;
            SetWidth(width);
        }

        public void OnSpawned() => _age = 0f;
        public void OnDespawned() { }

        private void Update()
        {
            _age += Time.deltaTime;
            SetWidth(Mathf.Lerp(width, 0f, _age / lifetime));
            if (_age >= lifetime) PoolService.Release(gameObject);
        }

        private void SetWidth(float value)
        {
            _line.startWidth = value;
            _line.endWidth = value * 0.5f;
        }
    }
}
