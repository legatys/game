using System.Collections.Generic;
using UnityEngine;

namespace Game.Animations
{
    /// <summary>Слоты анимаций персонажа.</summary>
    public enum AnimationSlot
    {
        Idle,
        WalkForward, WalkBackward, WalkLeft, WalkRight,
        RunForward, RunBackward, RunLeft, RunRight,
        Sprint,
        CrouchIdle, CrouchForward, CrouchBackward, CrouchLeft, CrouchRight,
        Slide, JumpStart, Fall, Death,
        AimIdle, Fire, Reload, HitReact
    }

    /// <summary>
    /// Набор анимаций персонажа (ScriptableObject). Сюда перетаскиваются клипы из Mixamo
    /// (Rig = Humanoid), затем кнопка "Rebuild Animator Controller" в инспекторе собирает контроллер.
    /// Пустые слоты заменяются ближайшим запасным клипом (см. Fallbacks), поэтому персонаж
    /// работает даже с одними Idle и Run. Ссылка на собранный контроллер хранится в поле controller,
    /// и <see cref="CharacterAnimator"/> подставляет его в Animator при старте.
    /// </summary>
    [CreateAssetMenu(fileName = "AnimationSet", menuName = "Game/Animation Set")]
    public sealed class AnimationSet : ScriptableObject
    {
        /// <summary>Сгенерированный Animator Controller; CharacterAnimator подставляет его в Animator при старте.</summary>
        [Header("Сгенерированный контроллер (заполняется автоматически)")]
        public RuntimeAnimatorController controller;

        /// <summary>Скорость ходьбы (м/с) — точка клипов Walk в blend tree.</summary>
        [Header("Пороги blend tree (м/с) — должны совпадать с MovementConfig")]
        public float walkSpeed = 2f;
        /// <summary>Скорость бега (м/с) — точка клипов Run в blend tree.</summary>
        public float runSpeed = 4.5f;
        /// <summary>Скорость спринта (м/с) — точка клипа Sprint в blend tree.</summary>
        public float sprintSpeed = 7f;
        /// <summary>Скорость шага в приседе (м/с) — точка клипов Crouch в blend tree.</summary>
        public float crouchSpeed = 2f;

        /// <summary>Стойка на месте.</summary>
        [Header("Локомоция (Mixamo: Idle / Walking / Jogging / Strafe)")]
        public AnimationClip idle;
        /// <summary>Шаг вперёд.</summary>
        public AnimationClip walkForward;
        /// <summary>Шаг назад.</summary>
        public AnimationClip walkBackward;
        /// <summary>Шаг влево (стрейф).</summary>
        public AnimationClip walkLeft;
        /// <summary>Шаг вправо (стрейф).</summary>
        public AnimationClip walkRight;
        /// <summary>Бег вперёд.</summary>
        public AnimationClip runForward;
        /// <summary>Бег назад.</summary>
        public AnimationClip runBackward;
        /// <summary>Бег влево.</summary>
        public AnimationClip runLeft;
        /// <summary>Бег вправо.</summary>
        public AnimationClip runRight;
        [Tooltip("Mixamo: Running / Fast Run")]
        public AnimationClip sprint;

        /// <summary>Стойка в приседе.</summary>
        [Header("Присед и подкат (Mixamo: Crouch Idle / Crouched Walking / Running Slide)")]
        public AnimationClip crouchIdle;
        /// <summary>Гусиный шаг вперёд.</summary>
        public AnimationClip crouchForward;
        /// <summary>Гусиный шаг назад.</summary>
        public AnimationClip crouchBackward;
        /// <summary>Гусиный шаг влево.</summary>
        public AnimationClip crouchLeft;
        /// <summary>Гусиный шаг вправо.</summary>
        public AnimationClip crouchRight;
        /// <summary>Подкат.</summary>
        public AnimationClip slide;

        /// <summary>Начало прыжка.</summary>
        [Header("Воздух и смерть (Mixamo: Jump / Falling Idle / Dying)")]
        public AnimationClip jumpStart;
        /// <summary>Падение / полёт (цикл).</summary>
        public AnimationClip fall;
        /// <summary>Смерть (не цикл).</summary>
        public AnimationClip death;

        /// <summary>Оружие у плеча, прицеливание (слой UpperBody).</summary>
        [Header("Верх тела — слой UpperBody (Mixamo: Rifle Aiming Idle / Firing Rifle / Reloading / Hit Reaction)")]
        public AnimationClip aimIdle;
        /// <summary>Выстрел (слой UpperBody).</summary>
        public AnimationClip fire;
        /// <summary>Перезарядка (слой UpperBody).</summary>
        public AnimationClip reload;
        /// <summary>Реакция на попадание (слой UpperBody).</summary>
        public AnimationClip hitReact;

        /// <summary>Цепочки замен для пустых слотов.</summary>
        public static readonly IReadOnlyDictionary<AnimationSlot, AnimationSlot[]> Fallbacks =
            new Dictionary<AnimationSlot, AnimationSlot[]>
            {
                { AnimationSlot.WalkForward, new[] { AnimationSlot.RunForward, AnimationSlot.Idle } },
                { AnimationSlot.WalkBackward, new[] { AnimationSlot.WalkForward, AnimationSlot.RunForward } },
                { AnimationSlot.WalkLeft, new[] { AnimationSlot.WalkForward, AnimationSlot.RunForward } },
                { AnimationSlot.WalkRight, new[] { AnimationSlot.WalkForward, AnimationSlot.RunForward } },
                { AnimationSlot.RunForward, new[] { AnimationSlot.WalkForward, AnimationSlot.Idle } },
                { AnimationSlot.RunBackward, new[] { AnimationSlot.WalkBackward, AnimationSlot.RunForward } },
                { AnimationSlot.RunLeft, new[] { AnimationSlot.WalkLeft, AnimationSlot.RunForward } },
                { AnimationSlot.RunRight, new[] { AnimationSlot.WalkRight, AnimationSlot.RunForward } },
                { AnimationSlot.Sprint, new[] { AnimationSlot.RunForward } },
                { AnimationSlot.CrouchIdle, new[] { AnimationSlot.Idle } },
                { AnimationSlot.CrouchForward, new[] { AnimationSlot.WalkForward } },
                { AnimationSlot.CrouchBackward, new[] { AnimationSlot.CrouchForward, AnimationSlot.WalkBackward } },
                { AnimationSlot.CrouchLeft, new[] { AnimationSlot.CrouchForward, AnimationSlot.WalkLeft } },
                { AnimationSlot.CrouchRight, new[] { AnimationSlot.CrouchForward, AnimationSlot.WalkRight } },
                { AnimationSlot.Slide, new[] { AnimationSlot.CrouchIdle } },
                { AnimationSlot.JumpStart, new[] { AnimationSlot.Fall } },
                { AnimationSlot.Fall, new[] { AnimationSlot.JumpStart, AnimationSlot.Idle } },
                // Без клипа смерти CharacterAnimator просто "роняет" модель (см. procedural fallback).
                { AnimationSlot.Death, new[] { AnimationSlot.Idle } },
            };

        /// <summary>Клип, назначенный в слот (без замен), или null.</summary>
        public AnimationClip GetClip(AnimationSlot slot)
        {
            switch (slot)
            {
                case AnimationSlot.Idle: return idle;
                case AnimationSlot.WalkForward: return walkForward;
                case AnimationSlot.WalkBackward: return walkBackward;
                case AnimationSlot.WalkLeft: return walkLeft;
                case AnimationSlot.WalkRight: return walkRight;
                case AnimationSlot.RunForward: return runForward;
                case AnimationSlot.RunBackward: return runBackward;
                case AnimationSlot.RunLeft: return runLeft;
                case AnimationSlot.RunRight: return runRight;
                case AnimationSlot.Sprint: return sprint;
                case AnimationSlot.CrouchIdle: return crouchIdle;
                case AnimationSlot.CrouchForward: return crouchForward;
                case AnimationSlot.CrouchBackward: return crouchBackward;
                case AnimationSlot.CrouchLeft: return crouchLeft;
                case AnimationSlot.CrouchRight: return crouchRight;
                case AnimationSlot.Slide: return slide;
                case AnimationSlot.JumpStart: return jumpStart;
                case AnimationSlot.Fall: return fall;
                case AnimationSlot.Death: return death;
                case AnimationSlot.AimIdle: return aimIdle;
                case AnimationSlot.Fire: return fire;
                case AnimationSlot.Reload: return reload;
                case AnimationSlot.HitReact: return hitReact;
                default: return null;
            }
        }

        /// <summary>Назначен ли клип в слот (без учёта замен).</summary>
        public bool HasClip(AnimationSlot slot) => GetClip(slot) != null;

        /// <summary>Клип слота или первый доступный запасной. resolvedFrom — откуда реально взят клип.</summary>
        public AnimationClip Resolve(AnimationSlot slot, out AnimationSlot resolvedFrom)
        {
            resolvedFrom = slot;
            var visited = new HashSet<AnimationSlot>();
            return ResolveRecursive(slot, visited, ref resolvedFrom);
        }

        private AnimationClip ResolveRecursive(AnimationSlot slot, HashSet<AnimationSlot> visited, ref AnimationSlot resolvedFrom)
        {
            if (!visited.Add(slot)) return null;

            var clip = GetClip(slot);
            if (clip != null)
            {
                resolvedFrom = slot;
                return clip;
            }

            if (!Fallbacks.TryGetValue(slot, out var chain)) return null;
            foreach (var fallback in chain)
            {
                clip = ResolveRecursive(fallback, visited, ref resolvedFrom);
                if (clip != null) return clip;
            }

            return null;
        }
    }
}
