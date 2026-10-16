using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Game.EditorTools
{
    /// <summary>Общие операции генератора: папки, ScriptableObject-ы, префабы.</summary>
    internal static class EditorAssetUtility
    {
        public static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;

            var parent = Path.GetDirectoryName(path)?.Replace('\\', '/');
            if (!string.IsNullOrEmpty(parent)) EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, Path.GetFileName(path));
        }

        /// <summary>Загружает ассет, а если его нет — создаёт и вызывает setup. Существующие ассеты не перезаписываются.</summary>
        public static T LoadOrCreate<T>(string path, Action<T> setup = null) where T : ScriptableObject
        {
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset != null) return asset;

            EnsureFolder(Path.GetDirectoryName(path)?.Replace('\\', '/'));
            asset = ScriptableObject.CreateInstance<T>();
            setup?.Invoke(asset);
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }

        public static bool Exists(string path) => AssetDatabase.LoadMainAssetAtPath(path) != null;

        /// <summary>Сохраняет объект сцены как префаб (перезаписывая, GUID сохраняется) и удаляет временный объект.</summary>
        public static GameObject SavePrefab(GameObject instance, string path)
        {
            EnsureFolder(Path.GetDirectoryName(path)?.Replace('\\', '/'));
            var prefab = PrefabUtility.SaveAsPrefabAsset(instance, path);
            Object.DestroyImmediate(instance);
            return prefab;
        }
    }
}
