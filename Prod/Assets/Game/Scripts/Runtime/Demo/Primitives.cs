using UnityEngine;

namespace Game.Demo
{
    /// <summary>Создание примитивов (куб, сфера, капсула) для визуала и геометрии уровня. Работает и в редакторе, и в игре.</summary>
    public static class Primitives
    {
        /// <summary>Примитив без коллайдера (визуал).</summary>
        public static GameObject Visual(PrimitiveType type, string name, Transform parent, Vector3 localPosition, Vector3 localScale, Material material)
        {
            var go = Solid(type, name, parent, localPosition, localScale, material);
            // DestroyImmediate, а не Destroy: коллайдер не должен прожить ещё кадр и толкнуть персонажа.
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

        /// <summary>Неактивный объект: компоненты можно добавить и настроить через Configure() до их Awake.</summary>
        public static GameObject Inactive(string name, Transform parent = null)
        {
            var go = new GameObject(name);
            go.SetActive(false);
            if (parent != null) go.transform.SetParent(parent, false);
            return go;
        }
    }
}
