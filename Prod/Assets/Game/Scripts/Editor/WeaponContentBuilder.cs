using System;
using Game.Combat.Weapons;
using Game.Demo;
using UnityEditor;
using UnityEngine;

namespace Game.EditorTools
{
    /// <summary>
    /// Сохраняет в ассеты то, что строит <see cref="DemoContentFactory"/>: эффекты, модели оружия,
    /// WeaponDefinition-ы (цифры — из <see cref="DemoWeaponCatalog"/>).
    /// </summary>
    internal static class WeaponContentBuilder
    {
        /// <summary>Создаёт эффекты, модели и определения оружия; возвращает набор оружия демо.</summary>
        public static DemoWeapons Build(bool overwritePrefabs)
        {
            var materials = AssetMaterialProvider.Instance;
            var tracer = Prefab($"{SkeletonPaths.Effects}/Tracer.prefab", overwritePrefabs, () => DemoContentFactory.BuildTracer(materials));
            var impact = Prefab($"{SkeletonPaths.Effects}/Impact.prefab", overwritePrefabs, () => DemoContentFactory.BuildImpact(materials));
            var explosion = Prefab($"{SkeletonPaths.Effects}/Explosion.prefab", overwritePrefabs, () => DemoContentFactory.BuildExplosion(materials));
            var grenade = Prefab($"{SkeletonPaths.Effects}/Grenade.prefab", overwritePrefabs, () => DemoContentFactory.BuildGrenade(materials, explosion));

            var weapons = new DemoWeapons
            {
                Pistol = Definition(DemoWeaponCatalog.Pistol),
                Rifle = Definition(DemoWeaponCatalog.Rifle),
                Shotgun = Definition(DemoWeaponCatalog.Shotgun),
                Launcher = Definition(DemoWeaponCatalog.Launcher),
                TurretRifle = Definition(DemoWeaponCatalog.TurretRifle),
            };

            // Ссылки на префабы обновляем всегда (префабы могли пересоздаться).
            DemoContentFactory.LinkWeapons(weapons, name => View(name, overwritePrefabs), tracer, impact, grenade);
            foreach (var definition in weapons.All) EditorUtility.SetDirty(definition);
            return weapons;
        }

        /// <summary>Загрузить префаб или (если нет / overwrite) построить и сохранить.</summary>
        public static GameObject Prefab(string path, bool overwrite, Func<GameObject> build)
        {
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            return existing != null && !overwrite ? existing : EditorAssetUtility.SavePrefab(build(), path);
        }

        private static WeaponDefinition Definition(string name) =>
            EditorAssetUtility.LoadOrCreate($"{SkeletonPaths.Weapons}/{name}.asset", DemoWeaponCatalog.Setup(name));

        private static WeaponView View(string name, bool overwrite) =>
            Prefab($"{SkeletonPaths.WeaponViews}/WeaponView_{name}.prefab", overwrite,
                () => DemoContentFactory.BuildWeaponView(name, AssetMaterialProvider.Instance)).GetComponent<WeaponView>();
    }
}
