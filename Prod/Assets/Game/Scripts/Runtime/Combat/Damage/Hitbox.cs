using UnityEngine;

namespace Game.Combat
{
    /// <summary>
    /// Хитбокс на отдельном коллайдере (голова, корпус): умножает урон и передаёт его в
    /// <see cref="Health"/> родителя (паттерн Proxy). Пример: голова ×2 и IsCritical.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public sealed class Hitbox : MonoBehaviour, IDamageable
    {
        [SerializeField] private float damageMultiplier = 1f;
        [SerializeField] private bool isCritical;

        private Health _health;

        public bool IsAlive => _health != null && _health.IsAlive;
        public Transform DamageRoot => _health != null ? _health.transform : transform;

        private void Awake() => _health = GetComponentInParent<Health>();

        public void ApplyDamage(in DamageInfo info)
        {
            if (_health != null) _health.ApplyDamage(info.Scaled(damageMultiplier, isCritical));
        }
    }
}
