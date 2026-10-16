using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Game.EditorTools
{
    /// <summary>Общие операции генератора: папки, ScriptableObject-ы, префабы, примитивы.</summary>
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

        /// <summary>Примитив без коллайдера (для визуала).</summary>
        public static GameObject Visual(PrimitiveType type, string name, Transform parent, Vector3 localPosition, Vector3 localScale, Material material)
        {
            var go = Solid(type, name, parent, localPosition, localScale, material);
            Object.DestroyImmediate(go.GetComponent<Collider>());
            return go;
        }

        /// <summary>Примитив с коллайдером.</summary>
        public static GameObject Solid(PrimitiveType type, string name, Transform parent, Vector3 localPosition, Vector3 localScale, Material material)
        {
            var go = GameObject.CreatePrimitive(type);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPosition;
            go.transform.localScale = localScale;
            if (material != null) go.GetComponent<Renderer>().sharedMaterial = material;
            return go;
        }

        public static Transform Child(string name, Transform parent, Vector3 localPosition)
        {
            var child = new GameObject(name).transform;
            child.SetParent(parent, false);
            child.localPosition = localPosition;
            return child;
        }
    }

    /// <summary>
    /// Builder для приватных [SerializeField]-полей через SerializedObject:
    /// <c>Serialized.Of(component).Set("field", value).Apply();</c>
    /// Ошибка в имени поля выводится в консоль, а не теряется молча.
    /// </summary>
    internal sealed class Serialized
    {
        private readonly SerializedObject _serializedObject;

        private Serialized(Object target) => _serializedObject = new SerializedObject(target);

        public static Serialized Of(Object target) => new Serialized(target);

        public Serialized Set(string field, object value)
        {
            var property = _serializedObject.FindProperty(field);
            if (property == null)
            {
                Debug.LogError($"[TPS Skeleton] Поле '{field}' не найдено в {_serializedObject.targetObject.GetType().Name}");
                return this;
            }

            switch (value)
            {
                case null: property.objectReferenceValue = null; break;
                case Object unityObject: property.objectReferenceValue = unityObject; break;
                case float f: property.floatValue = f; break;
                case int i: property.intValue = i; break;
                case bool b: property.boolValue = b; break;
                case string s: property.stringValue = s; break;
                case Vector3 v: property.vector3Value = v; break;
                case Color c: property.colorValue = c; break;
                case Object[] array:
                    property.arraySize = array.Length;
                    for (var index = 0; index < array.Length; index++)
                        property.GetArrayElementAtIndex(index).objectReferenceValue = array[index];
                    break;
                default:
                    Debug.LogError($"[TPS Skeleton] Тип {value.GetType().Name} не поддерживается для поля '{field}'");
                    break;
            }

            return this;
        }

        public void Apply() => _serializedObject.ApplyModifiedPropertiesWithoutUndo();
    }
}
