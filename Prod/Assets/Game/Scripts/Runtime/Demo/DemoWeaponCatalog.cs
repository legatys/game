using System;
using Game.Combat.Weapons;
using UnityEngine;

namespace Game.Demo
{
    /// <summary>Набор оружия демо: стартовое оружие игрока и оружие турели.</summary>
    public sealed class DemoWeapons
    {
        public WeaponDefinition Pistol, Rifle, Shotgun, Launcher, TurretRifle;

        public WeaponDefinition[] PlayerLoadout => new[] { Pistol, Rifle, Shotgun, Launcher };
        public WeaponDefinition[] All => new[] { Pistol, Rifle, Shotgun, Launcher, TurretRifle };
    }

    /// <summary>
    /// Каталог параметров демо-оружия — одно место с цифрами баланса для генератора ассетов (редактор)
    /// и для сцены Playground (рантайм). Модели оружия — из примитивов, размеры тоже здесь.
    /// </summary>
    public static class DemoWeaponCatalog
    {
        public const string Pistol = "Pistol";
        public const string Rifle = "Rifle";
        public const string Shotgun = "Shotgun";
        public const string Launcher = "Launcher";
        public const string TurretRifle = "TurretRifle";

        /// <summary>Размеры модели: корпус, радиус и длина ствола.</summary>
        public static (Vector3 body, float barrelRadius, float barrelLength) ViewShape(string weapon)
        {
            switch (weapon)
            {
                case Pistol: return (new Vector3(0.05f, 0.11f, 0.2f), 0.025f, 0.08f);
                case Shotgun: return (new Vector3(0.08f, 0.1f, 0.5f), 0.05f, 0.35f);
                case Launcher: return (new Vector3(0.12f, 0.14f, 0.45f), 0.1f, 0.3f);
                default: return (new Vector3(0.07f, 0.12f, 0.6f), 0.03f, 0.25f);
            }
        }

        public static Action<WeaponDefinition> Setup(string weapon)
        {
            switch (weapon)
            {
                case Pistol: return ConfigurePistol;
                case Rifle: return ConfigureRifle;
                case Shotgun: return ConfigureShotgun;
                case Launcher: return ConfigureLauncher;
                case TurretRifle: return ConfigureTurretRifle;
                default: throw new ArgumentException($"Неизвестное оружие {weapon}");
            }
        }

        /// <summary>Создать определения в памяти (для Playground). Генератор создаёт их ассетами через Setup().</summary>
        public static DemoWeapons CreateInMemory()
        {
            WeaponDefinition Make(string name)
            {
                var definition = ScriptableObject.CreateInstance<WeaponDefinition>();
                definition.name = name;
                Setup(name)(definition);
                return definition;
            }

            return new DemoWeapons
            {
                Pistol = Make(Pistol),
                Rifle = Make(Rifle),
                Shotgun = Make(Shotgun),
                Launcher = Make(Launcher),
                TurretRifle = Make(TurretRifle),
            };
        }

        public static void ConfigurePistol(WeaponDefinition d)
        {
            d.displayName = "Pistol"; d.fireMode = FireModeType.SemiAuto; d.shotType = ShotType.Hitscan;
            d.damage = 25f; d.roundsPerMinute = 420f; d.range = 120f;
            d.magazineSize = 12; d.startingReserve = 60; d.maxReserve = 120; d.reloadTime = 1.2f;
            d.hipSpread = 1.2f; d.aimSpread = 0.25f; d.spreadPerShot = 0.9f; d.maxBloom = 3.5f; d.spreadRecovery = 9f;
            d.recoilPitch = 1.4f; d.recoilYaw = 0.4f;
        }

        public static void ConfigureRifle(WeaponDefinition d)
        {
            d.displayName = "Assault Rifle"; d.fireMode = FireModeType.FullAuto; d.shotType = ShotType.Hitscan;
            d.damage = 16f; d.roundsPerMinute = 650f; d.range = 150f;
            d.magazineSize = 30; d.startingReserve = 120; d.maxReserve = 240; d.reloadTime = 2f;
            d.hipSpread = 2f; d.aimSpread = 0.4f; d.spreadPerShot = 0.35f; d.maxBloom = 4f; d.spreadRecovery = 10f;
            d.recoilPitch = 0.7f; d.recoilYaw = 0.35f;
        }

        public static void ConfigureShotgun(WeaponDefinition d)
        {
            d.displayName = "Shotgun"; d.fireMode = FireModeType.SemiAuto; d.shotType = ShotType.Spread;
            d.damage = 11f; d.pelletCount = 9; d.pelletSpread = 4.5f; d.roundsPerMinute = 75f; d.range = 45f;
            d.magazineSize = 6; d.startingReserve = 24; d.maxReserve = 48; d.reloadTime = 2.4f;
            d.hipSpread = 2.5f; d.aimSpread = 1.5f; d.spreadPerShot = 1.5f; d.maxBloom = 3f; d.spreadRecovery = 6f;
            d.recoilPitch = 3.5f; d.recoilYaw = 1f; d.impactForce = 2f;
        }

        public static void ConfigureLauncher(WeaponDefinition d)
        {
            d.displayName = "Grenade Launcher"; d.fireMode = FireModeType.SemiAuto; d.shotType = ShotType.Projectile;
            d.damage = 100f; d.explosionRadius = 4f; d.projectileSpeed = 28f; d.projectileGravity = 9.81f;
            d.roundsPerMinute = 60f; d.magazineSize = 4; d.startingReserve = 8; d.maxReserve = 16; d.reloadTime = 2.6f;
            d.hipSpread = 1f; d.aimSpread = 0.3f; d.spreadPerShot = 0f; d.recoilPitch = 4f; d.recoilYaw = 0.5f;
        }

        public static void ConfigureTurretRifle(WeaponDefinition d)
        {
            d.displayName = "Turret Rifle"; d.fireMode = FireModeType.Burst; d.burstCount = 4; d.burstCooldown = 0.35f;
            d.shotType = ShotType.Hitscan; d.damage = 6f; d.roundsPerMinute = 600f; d.range = 60f;
            d.magazineSize = 30; d.reloadTime = 1.5f; d.hipSpread = 1.5f; d.aimSpread = 1.5f;
            d.spreadPerShot = 0.2f; d.recoilPitch = 0f; d.recoilYaw = 0f;
        }
    }
}
