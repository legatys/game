using UnityEngine;

namespace Game.Combat.Weapons
{
    /// <summary>
    /// Паттерн Factory: собирает рабочее оружие из данных <see cref="WeaponDefinition"/> —
    /// создаёт визуал и подбирает стратегии режима огня и выстрела по enum-ам.
    /// </summary>
    public static class WeaponFactory
    {
        // Стратегии выстрела без состояния — один экземпляр на всех (Flyweight).
        private static readonly IShotStrategy Hitscan = new HitscanShot();
        private static readonly IShotStrategy Spread = new SpreadShot();
        private static readonly IShotStrategy Projectile = new ProjectileShot();

        /// <summary>Создаёт модель оружия внутри <paramref name="parent"/> и оружие с подходящими стратегиями.</summary>
        public static Weapon Create(WeaponDefinition definition, Transform parent)
        {
            var view = definition.viewPrefab != null
                ? Object.Instantiate(definition.viewPrefab, parent)
                : WeaponView.CreatePlaceholder(parent, definition.displayName);

            view.transform.localPosition = Vector3.zero;
            view.transform.localRotation = Quaternion.identity;

            return new Weapon(definition, view, CreateFireMode(definition), CreateShot(definition));
        }

        /// <summary>Стратегия режима огня по полю fireMode.</summary>
        public static IFireMode CreateFireMode(WeaponDefinition definition)
        {
            switch (definition.fireMode)
            {
                case FireModeType.FullAuto: return new FullAutoFireMode();
                case FireModeType.Burst: return new BurstFireMode(definition.burstCount, definition.burstCooldown);
                default: return new SemiAutoFireMode();
            }
        }

        /// <summary>Стратегия выстрела по полю shotType (общие экземпляры).</summary>
        public static IShotStrategy CreateShot(WeaponDefinition definition)
        {
            switch (definition.shotType)
            {
                case ShotType.Spread: return Spread;
                case ShotType.Projectile: return Projectile;
                default: return Hitscan;
            }
        }
    }
}
