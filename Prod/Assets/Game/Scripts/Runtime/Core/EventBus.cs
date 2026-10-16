using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Core
{
    /// <summary>Маркер для событий, которые можно публиковать через <see cref="EventBus"/>.</summary>
    public interface IGameEvent { }

    /// <summary>
    /// Паттерн Observer (Publish/Subscribe): глобальная типизированная шина событий.
    /// Издатель не знает подписчиков: Health публикует DamageDealtEvent, а HUD показывает хит-маркер.
    /// Подписывайтесь в OnEnable и отписывайтесь в OnDisable.
    /// </summary>
    public static class EventBus
    {
        private static readonly Dictionary<Type, Delegate> Handlers = new Dictionary<Type, Delegate>();

        public static void Subscribe<T>(Action<T> handler) where T : struct, IGameEvent
        {
            Handlers.TryGetValue(typeof(T), out var existing);
            Handlers[typeof(T)] = Delegate.Combine(existing, handler);
        }

        public static void Unsubscribe<T>(Action<T> handler) where T : struct, IGameEvent
        {
            if (!Handlers.TryGetValue(typeof(T), out var existing)) return;

            var result = Delegate.Remove(existing, handler);
            if (result == null) Handlers.Remove(typeof(T));
            else Handlers[typeof(T)] = result;
        }

        public static void Publish<T>(T gameEvent) where T : struct, IGameEvent
        {
            if (Handlers.TryGetValue(typeof(T), out var handler))
                ((Action<T>)handler).Invoke(gameEvent);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => Handlers.Clear();
    }
}
