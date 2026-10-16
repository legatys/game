using UnityEditor;
using UnityEngine;

namespace Game.EditorTools
{
    /// <summary>Создаёт простые материалы (URP Lit/Unlit, иначе Standard) и текстуру-"шахматку" для пола.</summary>
    internal static class MaterialLibrary
    {
        public static Material Lit(string name, Color color, float smoothness = 0.25f)
        {
            return LoadOrCreate(name, FindShader("Universal Render Pipeline/Lit", "Standard"), material =>
            {
                SetColor(material, color);
                if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness", smoothness);
                if (material.HasProperty("_Glossiness")) material.SetFloat("_Glossiness", smoothness);
            });
        }

        public static Material Emissive(string name, Color color, float intensity = 2f)
        {
            return LoadOrCreate(name, FindShader("Universal Render Pipeline/Lit", "Standard"), material =>
            {
                SetColor(material, color);
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", color * intensity);
                material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
            });
        }

        public static Material Unlit(string name, Color color)
        {
            return LoadOrCreate(name, FindShader("Universal Render Pipeline/Unlit", "Unlit/Color"), material => SetColor(material, color));
        }

        public static Material Checker(string name, Color a, Color b, float tiling)
        {
            return LoadOrCreate(name, FindShader("Universal Render Pipeline/Lit", "Standard"), material =>
            {
                var texture = CheckerTexture(a, b);
                if (material.HasProperty("_BaseMap"))
                {
                    material.SetTexture("_BaseMap", texture);
                    material.SetTextureScale("_BaseMap", Vector2.one * tiling);
                }

                material.mainTexture = texture;
                material.mainTextureScale = Vector2.one * tiling;
                SetColor(material, Color.white);
            });
        }

        private static Texture2D CheckerTexture(Color a, Color b)
        {
            var path = $"{SkeletonPaths.Materials}/Checker.asset";
            var existing = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if (existing != null) return existing;

            const int size = 64;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, true) { name = "Checker", wrapMode = TextureWrapMode.Repeat, anisoLevel = 4 };
            for (var y = 0; y < size; y++)
            for (var x = 0; x < size; x++)
            {
                var cell = (x / (size / 2) + y / (size / 2)) % 2 == 0;
                var line = x % (size / 2) == 0 || y % (size / 2) == 0;
                texture.SetPixel(x, y, line ? Color.Lerp(a, Color.black, 0.25f) : cell ? a : b);
            }

            texture.Apply();
            EditorAssetUtility.EnsureFolder(SkeletonPaths.Materials);
            AssetDatabase.CreateAsset(texture, path);
            return texture;
        }

        private static Material LoadOrCreate(string name, Shader shader, System.Action<Material> setup)
        {
            var path = $"{SkeletonPaths.Materials}/{name}.mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material != null) return material;

            EditorAssetUtility.EnsureFolder(SkeletonPaths.Materials);
            material = new Material(shader) { name = name };
            setup(material);
            AssetDatabase.CreateAsset(material, path);
            return material;
        }

        private static void SetColor(Material material, Color color)
        {
            if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);
            if (material.HasProperty("_Color")) material.SetColor("_Color", color);
        }

        private static Shader FindShader(string preferred, string fallback) => Shader.Find(preferred) ?? Shader.Find(fallback);
    }
}
