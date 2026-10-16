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
        [Header("Сгенерированный контроллер (заполняется автоматически)")]
        public RuntimeAnimatorController controller;

        [Header("Пороги blend tree (м/с) — должны совпадать с MovementConfig")]
        public float walkSpeed = 2f;
        public float runSpeed = 4.5f;
        public float sprintSpeed = 7f;
        public float crouchSpeed = 2f;

        [Header("Локомоция (Mixamo: Idle / Walking / Jogging / Strafe)")]
        public AnimationClip idle;
        public AnimationClip walkForward;
        public AnimationClip walkBackward;
        public AnimationClip walkLeft;
        public AnimationClip walkRight;
        public AnimationClip runForward;
        public AnimationClip runBackward;
        public AnimationClip runLeft;
        public AnimationClip runRight;
        [Tooltip("Mixamo: Running / Fast Run")]
        public AnimationClip sprint;

        [Header("Присед и подкат (Mixamo: Crouch Idle / Crouched Walking / Running Slide)")]
        public AnimationClip crouchIdle;
        public AnimationClip crouchForward;
        public AnimationClip crouchBackward;
        public AnimationClip crouchLeft;
        public AnimationClip crouchRight;
        public AnimationClip slide;

        [Header("Воздух и смерть (Mixamo: Jump / Falling Idle / Dying)")]
        public AnimationClip jumpStart;
        public AnimationClip fall;
        public AnimationClip death;

        [Header("Верх тела — слой UpperBody (Mixamo: Rifle Aiming Idle / Firing Rifle / Reloading / Hit Reaction)")]
        public AnimationClip aimIdle;
        public AnimationClip fire;
        public AnimationClip reload;
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
