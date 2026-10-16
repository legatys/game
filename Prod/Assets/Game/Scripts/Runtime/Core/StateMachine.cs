using System;
using System.Collections.Generic;

namespace Game.Core
{
    /// <summary>Состояние для <see cref="StateMachine"/> (паттерн State).</summary>
    public interface IState
    {
        /// <summary>Имя для отладки и HUD.</summary>
        string Name { get; }
        /// <summary>Вход в состояние.</summary>
        void Enter();
        /// <summary>Кадр в состоянии.</summary>
        void Tick(float deltaTime);
        /// <summary>Выход из состояния.</summary>
        void Exit();
    }

    /// <summary>
    /// Паттерн State: универсальный конечный автомат с таблицей переходов.
    /// Переходы описываются условиями (Func&lt;bool&gt;) в одном месте, а состояния
    /// содержат только своё поведение. Используется и игроком, и ИИ (турелью).
    /// Чтобы добавить новое состояние: создайте класс IState и добавьте ему переходы.
    /// </summary>
    public sealed class StateMachine
    {
        private sealed class Transition
        {
            public readonly IState To;
            public readonly Func<bool> Condition;

            public Transition(IState to, Func<bool> condition)
            {
                To = to;
                Condition = condition;
            }
        }

        private static readonly List<Transition> NoTransitions = new List<Transition>(0);

        private readonly Dictionary<IState, List<Transition>> _transitions = new Dictionary<IState, List<Transition>>();
        private readonly List<Transition> _anyTransitions = new List<Transition>();
        private List<Transition> _currentTransitions = NoTransitions;

        /// <summary>Текущее состояние.</summary>
        public IState Current { get; private set; }
        /// <summary>Предыдущее состояние.</summary>
        public IState Previous { get; private set; }

        /// <summary>(предыдущее, новое) — вызывается после смены состояния.</summary>
        public event Action<IState, IState> StateChanged;

        /// <summary>Переход from → to, когда condition() == true. Проверяются в порядке добавления.</summary>
        public void AddTransition(IState from, IState to, Func<bool> condition)
        {
            if (!_transitions.TryGetValue(from, out var list))
            {
                list = new List<Transition>();
                _transitions[from] = list;
            }

            list.Add(new Transition(to, condition));
            if (from == Current) _currentTransitions = list;
        }

        /// <summary>Переход из любого состояния (имеет приоритет над обычными).</summary>
        public void AddAnyTransition(IState to, Func<bool> condition)
        {
            _anyTransitions.Add(new Transition(to, condition));
        }

        /// <summary>Немедленно перейти в состояние (Exit старого → Enter нового).</summary>
        public void SetState(IState state)
        {
            if (state == null || state == Current) return;

            Previous = Current;
            Current?.Exit();
            Current = state;
            _currentTransitions = _transitions.TryGetValue(state, out var list) ? list : NoTransitions;
            Current.Enter();
            StateChanged?.Invoke(Previous, Current);
        }

        /// <summary>Сначала проверяет переходы, затем тикает текущее состояние.</summary>
        public void Tick(float deltaTime)
        {
            var next = FindTransition();
            if (next != null) SetState(next);
            Current?.Tick(deltaTime);
        }

        private IState FindTransition()
        {
            foreach (var transition in _anyTransitions)
                if (transition.To != Current && transition.Condition())
                    return transition.To;

            foreach (var transition in _currentTransitions)
                if (transition.Condition())
                    return transition.To;

            return null;
        }
    }
}
