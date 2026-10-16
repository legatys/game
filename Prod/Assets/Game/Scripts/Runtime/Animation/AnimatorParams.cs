using UnityEngine;

namespace Game.Animations
{
    /// <summary>
    /// Имена параметров, состояний и слоёв Animator Controller. Генератор контроллера и
    /// <see cref="CharacterAnimator"/> берут имена отсюда — переименовывать только здесь.
    /// </summary>
    public static class AnimatorParams
    {
        // Float
        /// <summary>Float: скорость вбок в локальных осях персонажа, м/с.</summary>
        public const string MoveX = "MoveX";
        /// <summary>Float: скорость вперёд/назад в локальных осях, м/с.</summary>
        public const string MoveZ = "MoveZ";
        /// <summary>Float: горизонтальная скорость, м/с.</summary>
        public const string Speed = "Speed";
        /// <summary>Float: вертикальная скорость, м/с.</summary>
        public const string VerticalSpeed = "VerticalSpeed";
        // Bool
        /// <summary>Bool: персонаж на земле.</summary>
        public const string IsGrounded = "IsGrounded";
        /// <summary>Bool: присед или подкат.</summary>
        public const string IsCrouching = "IsCrouching";
        /// <summary>Bool: подкат.</summary>
        public const string IsSliding = "IsSliding";
        /// <summary>Bool: оружие поднято (прицел или стрельба).</summary>
        public const string IsAiming = "IsAiming";
        /// <summary>Bool: персонаж мёртв.</summary>
        public const string IsDead = "IsDead";
        // Trigger
        /// <summary>Trigger: прыжок.</summary>
        public const string Jump = "Jump";
        /// <summary>Trigger: выстрел.</summary>
        public const string Fire = "Fire";
        /// <summary>Trigger: начало перезарядки.</summary>
        public const string Reload = "Reload";
        /// <summary>Trigger: получен урон.</summary>
        public const string Hit = "Hit";

        /// <summary>Имя слоя верхней части тела.</summary>
        public const string UpperBodyLayer = "UpperBody";

        /// <summary>Хэш параметра MoveX.</summary>
        public static readonly int MoveXHash = Animator.StringToHash(MoveX);
        /// <summary>Хэш параметра MoveZ.</summary>
        public static readonly int MoveZHash = Animator.StringToHash(MoveZ);
        /// <summary>Хэш параметра Speed.</summary>
        public static readonly int SpeedHash = Animator.StringToHash(Speed);
        /// <summary>Хэш параметра VerticalSpeed.</summary>
        public static readonly int VerticalSpeedHash = Animator.StringToHash(VerticalSpeed);
        /// <summary>Хэш параметра IsGrounded.</summary>
        public static readonly int IsGroundedHash = Animator.StringToHash(IsGrounded);
        /// <summary>Хэш параметра IsCrouching.</summary>
        public static readonly int IsCrouchingHash = Animator.StringToHash(IsCrouching);
        /// <summary>Хэш параметра IsSliding.</summary>
        public static readonly int IsSlidingHash = Animator.StringToHash(IsSliding);
        /// <summary>Хэш параметра IsAiming.</summary>
        public static readonly int IsAimingHash = Animator.StringToHash(IsAiming);
        /// <summary>Хэш параметра IsDead.</summary>
        public static readonly int IsDeadHash = Animator.StringToHash(IsDead);
        /// <summary>Хэш триггера Jump.</summary>
        public static readonly int JumpHash = Animator.StringToHash(Jump);
        /// <summary>Хэш триггера Fire.</summary>
        public static readonly int FireHash = Animator.StringToHash(Fire);
        /// <summary>Хэш триггера Reload.</summary>
        public static readonly int ReloadHash = Animator.StringToHash(Reload);
        /// <summary>Хэш триггера Hit.</summary>
        public static readonly int HitHash = Animator.StringToHash(Hit);
    }
}
