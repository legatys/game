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
        [SerializeField] private float maxHealth = 100f;
        [SerializeField] private bool invulnerable;

        [Header("Регенерация (0 — выключена)")]
        [SerializeField] private float regenDelay = 4f;
        [SerializeField] private float regenPerSecond;

        private float _timeSinceDamage;

        public float Current { get; private set; }
        public float Max => maxHealth;
        public float Normalized => maxHealth > 0f ? Current / maxHealth : 0f;
        public bool IsAlive => Current > 0f;
        public Transform DamageRoot => transform;

        public event Action<DamageInfo> Damaged;
        public event Action<DamageInfo> Died;
        /// <summary>(текущее, максимальное)</summary>
        public event Action<float, float> Changed;
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
