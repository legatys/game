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
        /// <summary>Множитель урона (голова — 2).</summary>
        [SerializeField] private float damageMultiplier = 1f;
        /// <summary>Отмечать попадание как критическое (для хит-маркера).</summary>
        [SerializeField] private bool isCritical;

        private Health _health;

        /// <inheritdoc />
        public bool IsAlive => _health != null && _health.IsAlive;
        /// <inheritdoc />
        public Transform DamageRoot => _health != null ? _health.transform : transform;

        private void Awake() => _health = GetComponentInParent<Health>();

        /// <summary>Задаёт множитель урона и критичность. Вызывать до активации объекта.</summary>
        public void Configure(float multiplier, bool critical)
        {
            damageMultiplier = multiplier;
            isCritical = critical;
        }

        /// <summary>Умножает урон и передаёт его в Health родителя.</summary>
        public void ApplyDamage(in DamageInfo info)
        {
            if (_health != null) _health.ApplyDamage(info.Scaled(damageMultiplier, isCritical));
        }
    }
}
