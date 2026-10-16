using Game.Core;
using UnityEngine;

namespace Game.Combat
{
    /// <summary>
    /// Простой эффект из пула (искры попадания, взрыв): живёт lifetime секунд, меняет масштаб
    /// от startScale до endScale и гасит свет, затем возвращается в пул. Замените на ParticleSystem при желании.
    /// </summary>
    public sealed class PooledEffect : MonoBehaviour, IPoolable
    {
        [SerializeField] private float lifetime = 0.3f;
        [SerializeField] private float startScale = 1f;
        [SerializeField] private float endScale = 0f;
        [SerializeField] private Light effectLight;

        private float _age;
        private float _lightIntensity;
        private Vector3 _baseScale = Vector3.one;

        private void Awake()
        {
            _baseScale = transform.localScale;
            if (effectLight != null) _lightIntensity = effectLight.intensity;
        }

        public void OnSpawned()
        {
            _age = 0f;
            Apply(0f);
        }

        public void OnDespawned() { }

        private void Update()
        {
            _age += Time.deltaTime;
            var t = Mathf.Clamp01(_age / lifetime);
            Apply(t);

            if (t >= 1f) PoolService.Release(gameObject);
        }

        private void Apply(float t)
        {
            transform.localScale = _baseScale * Mathf.Lerp(startScale, endScale, t);
            if (effectLight != null) effectLight.intensity = _lightIntensity * (1f - t);
        }
    }
}
