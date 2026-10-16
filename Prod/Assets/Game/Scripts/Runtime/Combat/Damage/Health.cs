using System;
using Game.Core;
using UnityEngine;

namespace Game.Combat
{
    /// <summary>
    /// Здоровье с событиями (паттерн Observer): Damaged, Died, Changed, Revived.
    /// HUD, аниматор, спавнер и ИИ подписываются на события вместо опроса каждый кадр.
    /// Дополнительно публикует глобальные события в <see cref="EventBus"/>.
    /// </summary>
    public sealed class Health : MonoBehaviour, IDamageable
    {
        /// <summary>Максимальное здоровье.</summary>
        [SerializeField] private float maxHealth = 100f;
        /// <summary>Не получать урон (для отладки).</summary>
        [SerializeField] private bool invulnerable;

        /// <summary>Через сколько секунд после урона начинается регенерация.</summary>
        [Header("Регенерация (0 — выключена)")]
        [SerializeField] private float regenDelay = 4f;
        /// <summary>Восстановление здоровья в секунду (0 — без регенерации).</summary>
        [SerializeField] private float regenPerSecond;

        private float _timeSinceDamage;

        /// <summary>Текущее здоровье.</summary>
        public float Current { get; private set; }
        /// <summary>Максимальное здоровье.</summary>
        public float Max => maxHealth;
        /// <summary>Здоровье в долях от максимума (0..1).</summary>
        public float Normalized => maxHealth > 0f ? Current / maxHealth : 0f;
        /// <inheritdoc />
        public bool IsAlive => Current > 0f;
        /// <inheritdoc />
        public Transform DamageRoot => transform;

        /// <summary>Получен урон (вызывается и на смертельном попадании).</summary>
        public event Action<DamageInfo> Damaged;
        /// <summary>Здоровье дошло до нуля.</summary>
        public event Action<DamageInfo> Died;
        /// <summary>(текущее, максимальное)</summary>
        public event Action<float, float> Changed;
        /// <summary>Здоровье восстановлено через Revive().</summary>
        public event Action Revived;

        private void Awake() => Current = maxHealth;

        /// <summary>Настройка из кода (генератор, рантайм-сборка уровня). Вызывать до активации объекта.</summary>
        public void Configure(float max, float regenDelaySeconds = 4f, float regenPerSecondAmount = 0f)
        {
            maxHealth = max;
            regenDelay = regenDelaySeconds;
            regenPerSecond = regenPerSecondAmount;
            Current = maxHealth;
        }

        /// <summary>Уменьшает здоровье, вызывает события и публикует DamageDealtEvent.</summary>
        public void ApplyDamage(in DamageInfo info)
        {
            if (!IsAlive || invulnerable || info.Amount <= 0f) return;

            Current = Mathf.Max(0f, Current - info.Amount);
            _timeSinceDamage = 0f;

            Damaged?.Invoke(info);
            Changed?.Invoke(Current, maxHealth);

            var killed = !IsAlive;
            EventBus.Publish(new DamageDealtEvent(info, this, killed));
            if (killed) Died?.Invoke(info);
        }

        /// <summary>Лечит и возвращает, сколько реально восстановлено.</summary>
        public float Heal(float amount)
        {
            if (!IsAlive || amount <= 0f) return 0f;

            var before = Current;
            Current = Mathf.Min(maxHealth, Current + amount);
            if (Current > before) Changed?.Invoke(Current, maxHealth);
            return Current - before;
        }

        /// <summary>Полное восстановление (респаун).</summary>
        public void Revive()
        {
            Current = maxHealth;
            _timeSinceDamage = regenDelay;
            Changed?.Invoke(Current, maxHealth);
            Revived?.Invoke();
        }

        private void Update()
        {
            if (regenPerSecond <= 0f || !IsAlive || Current >= maxHealth) return;

            _timeSinceDamage += Time.deltaTime;
            if (_timeSinceDamage >= regenDelay) Heal(regenPerSecond * Time.deltaTime);
        }
    }
}
