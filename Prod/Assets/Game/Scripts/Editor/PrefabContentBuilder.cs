using Game.Animations;
using Game.Character;
using Game.Demo;
using UnityEditor;
using UnityEngine;

namespace Game.EditorTools
{
    /// <summary>
    /// Сохраняет в префабы объекты из <see cref="DemoContentFactory"/>: игрок (манекен на скелете Mixamo),
    /// мишень, турель, аптечка, патроны, батут. Avatar манекена сохраняется отдельным ассетом.
    /// </summary>
    internal static class PrefabContentBuilder
    {
        /// <summary>Загружает или создаёт префабы демо (при <paramref name="overwrite"/> — пересоздаёт).</summary>
        public static DemoPrefabs Build(MovementConfig movement, AnimationSet animationSet, DemoWeapons weapons, bool overwrite)
        {
            var materials = AssetMaterialProvider.Instance;
            return new DemoPrefabs
            {
                Player = WeaponContentBuilder.Prefab(SkeletonPaths.PlayerPrefab, overwrite,
                    () => DemoContentFactory.BuildPlayer(movement, animationSet, weapons.PlayerLoadout, materials, LoadOrCreateAvatar)),
                Dummy = WeaponContentBuilder.Prefab($"{SkeletonPaths.Level}/TargetDummy.prefab", overwrite,
                    () => DemoContentFactory.BuildDummy(materials)),
                Turret = WeaponContentBuilder.Prefab($"{SkeletonPaths.Level}/EnemyTurret.prefab", overwrite,
                    () => DemoContentFactory.BuildTurret(materials, weapons.TurretRifle)),
                HealthPickup = WeaponContentBuilder.Prefab($"{SkeletonPaths.Level}/HealthPickup.prefab", overwrite,
                    () => DemoContentFactory.BuildHealthPickup(materials)),
                AmmoPickup = WeaponContentBuilder.Prefab($"{SkeletonPaths.Level}/AmmoPickup.prefab", overwrite,
                    () => DemoContentFactory.BuildAmmoPickup(materials)),
                JumpPad = WeaponContentBuilder.Prefab($"{SkeletonPaths.Level}/JumpPad.prefab", overwrite,
                    () => DemoContentFactory.BuildJumpPad(materials)),
            };
        }

        /// <summary>Avatar манекена: один ассет на все пересборки (скелет не меняется).</summary>
        private static Avatar LoadOrCreateAvatar(GameObject visual)
        {
            var avatar = AssetDatabase.LoadAssetAtPath<Avatar>(SkeletonPaths.MannequinAvatar);
            if (avatar != null) return avatar;

            avatar = MannequinBuilder.CreateAvatar(visual);
            EditorAssetUtility.EnsureFolder(SkeletonPaths.Animation);
            AssetDatabase.CreateAsset(avatar, SkeletonPaths.MannequinAvatar);
            return avatar;
        }
    }
}
