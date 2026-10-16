using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Core
{
    /// <summary>
    /// Паттерн Service Locator: единая точка регистрации и поиска глобальных сервисов
    /// (пул объектов, камера, игрок и т.д.), чтобы компоненты не искали друг друга через FindObjectOfType.
    /// Сервис регистрируется по типу ключа (обычно интерфейс).
    /// </summary>
    public static class ServiceLocator
    {
        private static readonly Dictionary<Type, object> Services = new Dictionary<Type, object>();

        public static void Register<T>(T service) where T : class
        {
            if (service == null) throw new ArgumentNullException(nameof(service));
            Services[typeof(T)] = service;
        }

        /// <summary>Снимает регистрацию, только если зарегистрирован именно этот экземпляр.</summary>
        public static void Unregister<T>(T service) where T : class
        {
            if (Services.TryGetValue(typeof(T), out var current) && ReferenceEquals(current, service))
                Services.Remove(typeof(T));
        }

        public static bool TryGet<T>(out T service) where T : class
        {
            if (Services.TryGetValue(typeof(T), out var raw) && raw is T typed && IsAlive(typed))
            {
                service = typed;
                return true;
            }

            service = null;
            return false;
        }

        public static T Get<T>() where T : class
        {
            if (TryGet(out T service)) return service;
            throw new InvalidOperationException($"[ServiceLocator] Сервис {typeof(T).Name} не зарегистрирован.");
        }

        public static void Clear() => Services.Clear();

        // Unity-объекты могут быть уничтожены, а ссылка в словаре останется — проверяем это.
        private static bool IsAlive(object service) => !(service is UnityEngine.Object unityObject) || unityObject != null;

        // Сброс статики при входе в Play Mode с отключённой перезагрузкой домена.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => Clear();
    }
}
