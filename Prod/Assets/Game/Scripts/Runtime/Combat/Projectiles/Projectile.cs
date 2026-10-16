using Game.Combat.Weapons;
using Game.Core;
using UnityEngine;

namespace Game.Combat
{
    /// <summary>
    /// Снаряд из пула (граната/ракета). Двигается сам (без Rigidbody), каждый шаг проверяет
    /// попадание лучом — быстрые снаряды не пролетают сквозь стены. При попадании взрывается.
    /// </summary>
    public sealed class Projectile : MonoBehaviour, IPoolable
    {
        [SerializeField] private float lifetime = 5f;
        [SerializeField] private GameObject explosionEffectPrefab;
        [SerializeField] private float explosionForce = 12f;

        private TrailRenderer _trail;
        private Vector3 _velocity;
        private WeaponDefinition _definition;
        private GameObject _owner;
        private float _age;
        private bool _active;

        private void Awake() => _trail = GetComponentInChildren<TrailRenderer>();

        public void Launch(Vector3 velocity, WeaponDefinition definition, GameObject owner)
        {
            if (_trail != null) _trail.Clear(); // иначе след из пула тянется с прошлой позиции
            _velocity = velocity;
            _definition = definition;
            _owner = owner;
            _age = 0f;
            _active = true;
        }

        public void OnSpawned() => _active = false;
        public void OnDespawned() => _active = false;

        private void Update()
        {
            if (!_active) return;

            var deltaTime = Time.deltaTime;
            _age += deltaTime;
            if (_age >= lifetime)
            {
                Explode(transform.position);
                return;
            }

            _velocity += Vector3.down * (_definition.projectileGravity * deltaTime);
            var step = _velocity * deltaTime;
            var ray = new Ray(transform.position, step.normalized);
            var ownerRoot = _owner != null ? _owner.transform : null;

            if (PhysicsQuery.Raycast(ray, step.magnitude, _definition.hitMask, ownerRoot, out var hit))
            {
                var direct = hit.collider.GetComponentInParent<IDamageable>();
                direct?.ApplyDamage(new DamageInfo(_definition.damage * 0.5f, hit.point, step.normalized, _owner));
                Explode(hit.point + hit.normal * 0.05f);
                return;
            }

            transform.position += step;
            if (step.sqrMagnitude > 0f) transform.rotation = Quaternion.LookRotation(step);
        }

        private void Explode(Vector3 position)
        {
            _active = false;
            Explosion.Detonate(position, _definition.explosionRadius, _definition.damage, explosionForce, _owner);
            CoreServices.Spawn(explosionEffectPrefab, position, Quaternion.identity);
            PoolService.Release(gameObject);
        }
    }
}
