using System;
using Game.Combat;
using Game.Combat.Weapons;
using UnityEditor;
using UnityEngine;

namespace Game.EditorTools
{
    /// <summary>Генерирует эффекты (трассер, искры, взрыв, граната), модели оружия из примитивов и WeaponDefinition-ы.</summary>
    internal static class WeaponContentBuilder
    {
        public sealed class Result
        {
            public WeaponDefinition Pistol, Rifle, Shotgun, Launcher, TurretRifle;
            public WeaponDefinition[] PlayerLoadout => new[] { Pistol, Rifle, Shotgun, Launcher };
        }

        public static Result Build(bool overwritePrefabs)
        {
            var tracer = Prefab($"{SkeletonPaths.Effects}/Tracer.prefab", overwritePrefabs, BuildTracer);
            var impact = Prefab($"{SkeletonPaths.Effects}/Impact.prefab", overwritePrefabs, BuildImpact);
            var explosion = Prefab($"{SkeletonPaths.Effects}/Explosion.prefab", overwritePrefabs, BuildExplosion);
            var grenade = Prefab($"{SkeletonPaths.Effects}/Grenade.prefab", overwritePrefabs, () => BuildGrenade(explosion));

            var pistolView = View("Pistol", overwritePrefabs, new Vector3(0.05f, 0.11f, 0.2f), 0.025f, 0.08f);
            var rifleView = View("Rifle", overwritePrefabs, new Vector3(0.07f, 0.12f, 0.6f), 0.03f, 0.25f);
            var shotgunView = View("Shotgun", overwritePrefabs, new Vector3(0.08f, 0.1f, 0.5f), 0.05f, 0.35f);
            var launcherView = View("Launcher", overwritePrefabs, new Vector3(0.12f, 0.14f, 0.45f), 0.1f, 0.3f);

            var result = new Result
            {
                Pistol = Definition("Pistol", d =>
                {
                    d.displayName = "Pistol"; d.fireMode = FireModeType.SemiAuto; d.shotType = ShotType.Hitscan;
                    d.damage = 25f; d.roundsPerMinute = 420f; d.range = 120f;
                    d.magazineSize = 12; d.startingReserve = 60; d.maxReserve = 120; d.reloadTime = 1.2f;
                    d.hipSpread = 1.2f; d.aimSpread = 0.25f; d.spreadPerShot = 0.9f; d.maxBloom = 3.5f; d.spreadRecovery = 9f;
                    d.recoilPitch = 1.4f; d.recoilYaw = 0.4f;
                }),
                Rifle = Definition("Rifle", d =>
                {
                    d.displayName = "Assault Rifle"; d.fireMode = FireModeType.FullAuto; d.shotType = ShotType.Hitscan;
                    d.damage = 16f; d.roundsPerMinute = 650f; d.range = 150f;
                    d.magazineSize = 30; d.startingReserve = 120; d.maxReserve = 240; d.reloadTime = 2f;
                    d.hipSpread = 2f; d.aimSpread = 0.4f; d.spreadPerShot = 0.35f; d.maxBloom = 4f; d.spreadRecovery = 10f;
                    d.recoilPitch = 0.7f; d.recoilYaw = 0.35f;
                }),
                Shotgun = Definition("Shotgun", d =>
                {
                    d.displayName = "Shotgun"; d.fireMode = FireModeType.SemiAuto; d.shotType = ShotType.Spread;
                    d.damage = 11f; d.pelletCount = 9; d.pelletSpread = 4.5f; d.roundsPerMinute = 75f; d.range = 45f;
                    d.magazineSize = 6; d.startingReserve = 24; d.maxReserve = 48; d.reloadTime = 2.4f;
                    d.hipSpread = 2.5f; d.aimSpread = 1.5f; d.spreadPerShot = 1.5f; d.maxBloom = 3f; d.spreadRecovery = 6f;
                    d.recoilPitch = 3.5f; d.recoilYaw = 1f; d.impactForce = 2f;
                }),
                Launcher = Definition("Launcher", d =>
                {
                    d.displayName = "Grenade Launcher"; d.fireMode = FireModeType.SemiAuto; d.shotType = ShotType.Projectile;
                    d.damage = 100f; d.explosionRadius = 4f; d.projectileSpeed = 28f; d.projectileGravity = 9.81f;
                    d.roundsPerMinute = 60f; d.magazineSize = 4; d.startingReserve = 8; d.maxReserve = 16; d.reloadTime = 2.6f;
                    d.hipSpread = 1f; d.aimSpread = 0.3f; d.spreadPerShot = 0f; d.recoilPitch = 4f; d.recoilYaw = 0.5f;
                }),
                TurretRifle = Definition("TurretRifle", d =>
                {
                    d.displayName = "Turret Rifle"; d.fireMode = FireModeType.Burst; d.burstCount = 4; d.burstCooldown = 0.35f;
                    d.shotType = ShotType.Hitscan; d.damage = 6f; d.roundsPerMinute = 600f; d.range = 60f;
                    d.magazineSize = 30; d.reloadTime = 1.5f; d.hipSpread = 1.5f; d.aimSpread = 1.5f;
                    d.spreadPerShot = 0.2f; d.recoilPitch = 0f; d.recoilYaw = 0f;
                }),
            };

            // Ссылки на префабы обновляем всегда (могли пересоздаться).
            Link(result.Pistol, pistolView, tracer, impact);
            Link(result.Rifle, rifleView, tracer, impact);
            Link(result.Shotgun, shotgunView, tracer, impact);
            Link(result.Launcher, launcherView, null, impact);
            result.Launcher.projectilePrefab = grenade.GetComponent<Projectile>();
            EditorUtility.SetDirty(result.Launcher);
            Link(result.TurretRifle, rifleView, tracer, impact);
            return result;
        }

        // ---------- Помощники ----------

        public static GameObject Prefab(string path, bool overwrite, Func<GameObject> build)
        {
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            return existing != null && !overwrite ? existing : EditorAssetUtility.SavePrefab(build(), path);
        }

        private static WeaponDefinition Definition(string name, Action<WeaponDefinition> setup) =>
            EditorAssetUtility.LoadOrCreate($"{SkeletonPaths.Weapons}/{name}.asset", setup);

        private static void Link(WeaponDefinition definition, GameObject view, GameObject tracer, GameObject impact)
        {
            definition.viewPrefab = view.GetComponent<WeaponView>();
            definition.tracerPrefab = tracer;
            definition.impactPrefab = impact;
            EditorUtility.SetDirty(definition);
        }

        private static GameObject View(string name, bool overwrite, Vector3 bodySize, float barrelRadius, float barrelLength)
        {
            return Prefab($"{SkeletonPaths.WeaponViews}/WeaponView_{name}.prefab", overwrite, () =>
            {
                var root = new GameObject($"WeaponView_{name}");
                var metal = MaterialLibrary.Lit("Weapon", new Color(0.18f, 0.19f, 0.21f), 0.6f);

                EditorAssetUtility.Visual(PrimitiveType.Cube, "Body", root.transform, new Vector3(0f, 0f, bodySize.z * 0.5f - 0.05f), bodySize, metal);
                EditorAssetUtility.Visual(PrimitiveType.Cube, "Grip", root.transform, new Vector3(0f, -bodySize.y * 0.8f, 0.02f),
                    new Vector3(bodySize.x * 0.8f, bodySize.y, 0.05f), metal).transform.localRotation = Quaternion.Euler(15f, 0f, 0f);

                var barrelStart = bodySize.z - 0.05f;
                var barrel = EditorAssetUtility.Visual(PrimitiveType.Cylinder, "Barrel", root.transform,
                    new Vector3(0f, bodySize.y * 0.15f, barrelStart + barrelLength * 0.5f),
                    new Vector3(barrelRadius * 2f, barrelLength * 0.5f, barrelRadius * 2f), metal);
                barrel.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);

                var muzzle = EditorAssetUtility.Child("Muzzle", root.transform, new Vector3(0f, bodySize.y * 0.15f, barrelStart + barrelLength + 0.02f));
                var flash = EditorAssetUtility.Visual(PrimitiveType.Sphere, "MuzzleFlash", muzzle, Vector3.zero, Vector3.one * 0.12f,
                    MaterialLibrary.Emissive("MuzzleFlash", new Color(1f, 0.8f, 0.3f), 4f));
                var light = flash.AddComponent<Light>();
                light.type = LightType.Point;
                light.range = 4f;
                light.intensity = 3f;
                light.color = new Color(1f, 0.75f, 0.4f);
                flash.SetActive(false);

                var view = root.AddComponent<WeaponView>();
                Serialized.Of(view).Set("muzzle", muzzle).Set("muzzleFlash", flash).Apply();
                return root;
            });
        }

        private static GameObject BuildTracer()
        {
            var root = new GameObject("Tracer");
            var line = root.AddComponent<LineRenderer>();
            line.sharedMaterial = MaterialLibrary.Unlit("Tracer", new Color(1f, 0.85f, 0.4f));
            line.positionCount = 2;
            line.useWorldSpace = true;
            line.widthMultiplier = 1f;
            line.numCapVertices = 2;
            line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            line.receiveShadows = false;
            root.AddComponent<Tracer>();
            return root;
        }

        private static GameObject BuildImpact()
        {
            var root = new GameObject("Impact");
            EditorAssetUtility.Visual(PrimitiveType.Cube, "Spark", root.transform, Vector3.zero, Vector3.one * 0.08f,
                MaterialLibrary.Emissive("Impact", new Color(1f, 0.6f, 0.2f), 3f));
            var effect = root.AddComponent<PooledEffect>();
            Serialized.Of(effect).Set("lifetime", 0.35f).Set("startScale", 1f).Set("endScale", 0f).Apply();
            return root;
        }

        private static GameObject BuildExplosion()
        {
            var root = new GameObject("Explosion");
            EditorAssetUtility.Visual(PrimitiveType.Sphere, "Fireball", root.transform, Vector3.zero, Vector3.one,
                MaterialLibrary.Emissive("Explosion", new Color(1f, 0.45f, 0.1f), 3f));
            var light = root.AddComponent<Light>();
            light.type = LightType.Point;
            light.range = 10f;
            light.intensity = 6f;
            light.color = new Color(1f, 0.6f, 0.3f);

            var effect = root.AddComponent<PooledEffect>();
            Serialized.Of(effect).Set("lifetime", 0.3f).Set("startScale", 0.5f).Set("endScale", 6f).Set("effectLight", light).Apply();
            return root;
        }

        private static GameObject BuildGrenade(GameObject explosion)
        {
            var root = new GameObject("Grenade");
            EditorAssetUtility.Visual(PrimitiveType.Sphere, "Body", root.transform, Vector3.zero, Vector3.one * 0.15f,
                MaterialLibrary.Lit("Grenade", new Color(0.2f, 0.3f, 0.15f)));

            var trail = root.AddComponent<TrailRenderer>();
            trail.sharedMaterial = MaterialLibrary.Unlit("Tracer", new Color(1f, 0.85f, 0.4f));
            trail.time = 0.25f;
            trail.startWidth = 0.08f;
            trail.endWidth = 0f;
            trail.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

            var projectile = root.AddComponent<Projectile>();
            Serialized.Of(projectile).Set("explosionEffectPrefab", explosion).Apply();
            return root;
        }
    }
}
