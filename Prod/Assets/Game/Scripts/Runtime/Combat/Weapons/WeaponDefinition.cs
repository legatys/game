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
        /// <summary>Название для HUD.</summary>
        [Header("Общее")]
        public string displayName = "Weapon";
        /// <summary>Режим огня → IFireMode (см. WeaponFactory).</summary>
        public FireModeType fireMode = FireModeType.SemiAuto;
        /// <summary>Тип выстрела → IShotStrategy (см. WeaponFactory).</summary>
        public ShotType shotType = ShotType.Hitscan;
        [Tooltip("Префаб модели оружия с компонентом WeaponView. Пусто — сгенерируется кубик")]
        public WeaponView viewPrefab;

        /// <summary>Урон за попадание (у дробовика — за дробинку).</summary>
        [Header("Урон и темп")]
        public float damage = 20f;
        [Tooltip("Выстрелов в минуту")]
        public float roundsPerMinute = 450f;
        /// <summary>Дальность луча, м.</summary>
        public float range = 120f;
        /// <summary>Импульс, передаваемый физическим объектам при попадании.</summary>
        public float impactForce = 4f;
        /// <summary>Слои, в которые можно попасть.</summary>
        public LayerMask hitMask = ~0;

        /// <summary>Выстрелов в очереди (режим Burst).</summary>
        [Header("Очередь (для Burst)")]
        public int burstCount = 3;
        /// <summary>Пауза после очереди, сек.</summary>
        public float burstCooldown = 0.3f;

        /// <summary>Ёмкость магазина.</summary>
        [Header("Патроны")]
        public int magazineSize = 12;
        /// <summary>Запас патронов при получении оружия.</summary>
        public int startingReserve = 48;
        /// <summary>Максимальный запас патронов.</summary>
        public int maxReserve = 120;
        /// <summary>Время перезарядки, сек.</summary>
        public float reloadTime = 1.5f;
        /// <summary>Перезаряжать автоматически при попытке выстрела с пустым магазином.</summary>
        public bool autoReloadWhenEmpty = true;

        /// <summary>Базовый разброс от бедра, градусы.</summary>
        [Header("Разброс (градусы)")]
        public float hipSpread = 1.5f;
        /// <summary>Базовый разброс при прицеливании, градусы.</summary>
        public float aimSpread = 0.3f;
        /// <summary>Насколько раскрывается разброс за выстрел, градусы.</summary>
        public float spreadPerShot = 0.6f;
        /// <summary>Максимальное раскрытие разброса, градусы.</summary>
        public float maxBloom = 4f;
        [Tooltip("Скорость уменьшения разброса, градусов в секунду")]
        public float spreadRecovery = 8f;

        /// <summary>Дробинок за выстрел (тип Spread).</summary>
        [Header("Дробь (для Spread)")]
        public int pelletCount = 8;
        /// <summary>Разлёт дробинок, градусы.</summary>
        public float pelletSpread = 5f;

        /// <summary>Префаб снаряда (тип Projectile).</summary>
        [Header("Снаряд (для Projectile)")]
        public Projectile projectilePrefab;
        /// <summary>Начальная скорость снаряда, м/с.</summary>
        public float projectileSpeed = 30f;
        /// <summary>Гравитация для снаряда, м/с².</summary>
        public float projectileGravity = 9.81f;
        /// <summary>Радиус взрыва снаряда, м.</summary>
        public float explosionRadius = 4f;

        /// <summary>Подброс камеры вверх за выстрел, градусы.</summary>
        [Header("Отдача камеры (градусы)")]
        public float recoilPitch = 1f;
        /// <summary>Максимальный случайный увод камеры вбок, градусы.</summary>
        public float recoilYaw = 0.3f;

        /// <summary>Трассер (необязательно).</summary>
        [Header("Эффекты и звук (можно оставить пустыми)")]
        public GameObject tracerPrefab;
        /// <summary>Эффект попадания (необязательно).</summary>
        public GameObject impactPrefab;
        /// <summary>Звук выстрела.</summary>
        public AudioClip fireSound;
        /// <summary>Звук перезарядки.</summary>
        public AudioClip reloadSound;
        /// <summary>Щелчок пустого магазина.</summary>
        public AudioClip emptySound;

        /// <summary>Интервал между выстрелами, сек (из roundsPerMinute).</summary>
        public float FireInterval => 60f / Mathf.Max(1f, roundsPerMinute);
    }
}
