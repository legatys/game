using System;
using Game.Demo;
using UnityEditor;
using UnityEngine;

namespace Game.EditorTools
{
    /// <summary>
    /// <see cref="IMaterialProvider"/> для генератора: материалы сохраняются ассетами в Generated/Materials
    /// (иначе префабы потеряли бы ссылки на них). Существующие .mat не перезаписываются — их можно править.
    /// </summary>
    internal sealed class AssetMaterialProvider : IMaterialProvider
    {
        public static readonly AssetMaterialProvider Instance = new AssetMaterialProvider();

        public Material Lit(string name, Color color, float smoothness = 0.25f) =>
            LoadOrCreate(name, () => MaterialFactory.CreateLit(name, color, smoothness));

        public Material Emissive(string name, Color color, float intensity = 2f) =>
            LoadOrCreate(name, () => MaterialFactory.CreateEmissive(name, color, intensity));

        public Material Unlit(string name, Color color) =>
            LoadOrCreate(name, () => MaterialFactory.CreateUnlit(name, color));

        public Material Checker(string name, Color a, Color b, float tiling) =>
            LoadOrCreate(name, () => MaterialFactory.CreateChecker(name, CheckerTexture(a, b), tiling));

        private static Texture2D CheckerTexture(Color a, Color b)
        {
            var path = $"{SkeletonPaths.Materials}/Checker.asset";
            var existing = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if (existing != null) return existing;

            var texture = MaterialFactory.CreateCheckerTexture(a, b);
            EditorAssetUtility.EnsureFolder(SkeletonPaths.Materials);
            AssetDatabase.CreateAsset(texture, path);
            return texture;
        }

        private static Material LoadOrCreate(string name, Func<Material> create)
        {
            var path = $"{SkeletonPaths.Materials}/{name}.mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material != null) return material;

            EditorAssetUtility.EnsureFolder(SkeletonPaths.Materials);
            material = create();
            AssetDatabase.CreateAsset(material, path);
            return material;
        }
    }
}
