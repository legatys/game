using System.Collections.Generic;
using UnityEngine;

namespace Game.Demo
{
    /// <summary>
    /// Откуда брать материалы для примитивов (паттерн Strategy): в редакторе — ассеты .mat
    /// (AssetMaterialProvider в Game.Editor), в рантайме — материалы в памяти (<see cref="RuntimeMaterialProvider"/>).
    /// Фабрики контента работают с любым из них одинаково.
    /// </summary>
    public interface IMaterialProvider
    {
        /// <summary>Материал с освещением.</summary>
        Material Lit(string name, Color color, float smoothness = 0.25f);
        /// <summary>Светящийся материал.</summary>
        Material Emissive(string name, Color color, float intensity = 2f);
        /// <summary>Материал без освещения.</summary>
        Material Unlit(string name, Color color);
        /// <summary>Материал с текстурой-шахматкой.</summary>
        Material Checker(string name, Color a, Color b, float tiling);
    }

    /// <summary>Создание материалов URP (Lit/Unlit) с запасным вариантом на встроенные шейдеры.</summary>
    public static class MaterialFactory
    {
        /// <summary>Новый материал URP Lit (или Standard).</summary>
        public static Material CreateLit(string name, Color color, float smoothness)
        {
            var material = new Material(FindShader("Universal Render Pipeline/Lit", "Standard")) { name = name };
            SetColor(material, color);
            if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness", smoothness);
            if (material.HasProperty("_Glossiness")) material.SetFloat("_Glossiness", smoothness);
            return material;
        }

        /// <summary>Новый светящийся Lit-материал.</summary>
        public static Material CreateEmissive(string name, Color color, float intensity)
        {
            var material = CreateLit(name, color, 0.3f);
            material.EnableKeyword("_EMISSION");
            material.SetColor("_EmissionColor", color * intensity);
            material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
            return material;
        }

        /// <summary>Новый материал URP Unlit (или Unlit/Color).</summary>
        public static Material CreateUnlit(string name, Color color)
        {
            var material = new Material(FindShader("Universal Render Pipeline/Unlit", "Unlit/Color")) { name = name };
            SetColor(material, color);
            return material;
        }

        /// <summary>Новый Lit-материал с текстурой и тайлингом.</summary>
        public static Material CreateChecker(string name, Texture2D texture, float tiling)
        {
            var material = CreateLit(name, Color.white, 0.1f);
            if (material.HasProperty("_BaseMap"))
            {
                material.SetTexture("_BaseMap", texture);
                material.SetTextureScale("_BaseMap", Vector2.one * tiling);
            }

            material.mainTexture = texture;
            material.mainTextureScale = Vector2.one * tiling;
            return material;
        }

        /// <summary>Текстура-"шахматка" с тонкой сеткой: на ней хорошо видно скорость движения.</summary>
        public static Texture2D CreateCheckerTexture(Color a, Color b)
        {
            const int size = 64;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, true)
            {
                name = "Checker",
                wrapMode = TextureWrapMode.Repeat,
                anisoLevel = 4
            };

            for (var y = 0; y < size; y++)
            for (var x = 0; x < size; x++)
            {
                var cell = (x / (size / 2) + y / (size / 2)) % 2 == 0;
                var line = x % (size / 2) == 0 || y % (size / 2) == 0;
                texture.SetPixel(x, y, line ? Color.Lerp(a, Color.black, 0.25f) : cell ? a : b);
            }

            texture.Apply();
            return texture;
        }

        /// <summary>Задаёт цвет и для URP (_BaseColor), и для встроенных шейдеров (_Color).</summary>
        public static void SetColor(Material material, Color color)
        {
            if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);
            if (material.HasProperty("_Color")) material.SetColor("_Color", color);
        }

        private static Shader FindShader(string preferred, string fallback)
        {
            var shader = Shader.Find(preferred);
            return shader != null ? shader : Shader.Find(fallback);
        }
    }

    /// <summary>Материалы в памяти с кэшем по имени. Используется сценой Playground (без генерации ассетов).</summary>
    public sealed class RuntimeMaterialProvider : IMaterialProvider
    {
        private readonly Dictionary<string, Material> _cache = new Dictionary<string, Material>();
        private readonly HideFlags _hideFlags;

        /// <param name="hideFlags">Для превью в редакторе — DontSave, чтобы материалы не попали в сцену.</param>
        /// <summary>Создаёт провайдер; <paramref name="hideFlags"/> назначаются всем созданным материалам.</summary>
        public RuntimeMaterialProvider(HideFlags hideFlags = HideFlags.None) => _hideFlags = hideFlags;

        /// <inheritdoc />
        public Material Lit(string name, Color color, float smoothness = 0.25f) =>
            Cached(name, () => MaterialFactory.CreateLit(name, color, smoothness));

        /// <inheritdoc />
        public Material Emissive(string name, Color color, float intensity = 2f) =>
            Cached(name, () => MaterialFactory.CreateEmissive(name, color, intensity));

        /// <inheritdoc />
        public Material Unlit(string name, Color color) =>
            Cached(name, () => MaterialFactory.CreateUnlit(name, color));

        /// <inheritdoc />
        public Material Checker(string name, Color a, Color b, float tiling) =>
            Cached(name, () =>
            {
                var texture = MaterialFactory.CreateCheckerTexture(a, b);
                texture.hideFlags = _hideFlags;
                return MaterialFactory.CreateChecker(name, texture, tiling);
            });

        /// <summary>Уничтожить созданные материалы (при пересборке превью).</summary>
        public void DestroyAll()
        {
            foreach (var material in _cache.Values)
            {
                if (material == null) continue;
                if (material.mainTexture != null) Object.DestroyImmediate(material.mainTexture);
                Object.DestroyImmediate(material);
            }

            _cache.Clear();
        }

        private Material Cached(string name, System.Func<Material> create)
        {
            if (_cache.TryGetValue(name, out var material) && material != null) return material;

            material = create();
            material.hideFlags = _hideFlags;
            _cache[name] = material;
            return material;
        }
    }
}
