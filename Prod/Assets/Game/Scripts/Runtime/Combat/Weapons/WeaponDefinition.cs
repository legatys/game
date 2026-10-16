using UnityEngine;

namespace Game.Combat.Weapons
{
    public enum FireModeType { SemiAuto, FullAuto, Burst }
    public enum ShotType { Hitscan, Spread, Projectile }

    /// <summary>
    /// Описание оружия (ScriptableObject, паттерн Flyweight): общие неизменяемые данные,
    /// которые разделяют все экземпляры оружия. Новое оружие = новый ассет, без кода.
    /// Поведение выбирается полями fireMode / shotType (их превращает в стратегии <see cref="WeaponFactory"/>).
    /// Create → Game/Weapon Definition.
    /// </summary>
    [CreateAssetMenu(fileName = "Weapon", menuName = "Game/Weapon Definition")]
    public sealed class WeaponDefinition : ScriptableObject
    {
        [Header("Общее")]
        public string displayName = "Weapon";
        public FireModeType fireMode = FireModeType.SemiAuto;
        public ShotType shotType = ShotType.Hitscan;
        [Tooltip("Префаб модели оружия с компонентом WeaponView. Пусто — сгенерируется кубик")]
        public WeaponView viewPrefab;

        [Header("Урон и темп")]
        public float damage = 20f;
        [Tooltip("Выстрелов в минуту")]
        public float roundsPerMinute = 450f;
        public float range = 120f;
        public float impactForce = 4f;
        public LayerMask hitMask = ~0;

        [Header("Очередь (для Burst)")]
        public int burstCount = 3;
        public float burstCooldown = 0.3f;

        [Header("Патроны")]
        public int magazineSize = 12;
        public int startingReserve = 48;
        public int maxReserve = 120;
        public float reloadTime = 1.5f;
        public bool autoReloadWhenEmpty = true;

        [Header("Разброс (градусы)")]
        public float hipSpread = 1.5f;
        public float aimSpread = 0.3f;
        public float spreadPerShot = 0.6f;
        public float maxBloom = 4f;
        [Tooltip("Скорость уменьшения разброса, градусов в секунду")]
        public float spreadRecovery = 8f;

        [Header("Дробь (для Spread)")]
        public int pelletCount = 8;
        public float pelletSpread = 5f;

        [Header("Снаряд (для Projectile)")]
        public Projectile projectilePrefab;
        public float projectileSpeed = 30f;
        public float projectileGravity = 9.81f;
        public float explosionRadius = 4f;

        [Header("Отдача камеры (градусы)")]
        public float recoilPitch = 1f;
        public float recoilYaw = 0.3f;

        [Header("Эффекты и звук (можно оставить пустыми)")]
        public GameObject tracerPrefab;
        public GameObject impactPrefab;
        public AudioClip fireSound;
        public AudioClip reloadSound;
        public AudioClip emptySound;

        public float FireInterval => 60f / Mathf.Max(1f, roundsPerMinute);
    }
}
