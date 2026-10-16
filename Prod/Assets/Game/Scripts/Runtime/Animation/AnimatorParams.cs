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
        public const string MoveX = "MoveX";
        public const string MoveZ = "MoveZ";
        public const string Speed = "Speed";
        public const string VerticalSpeed = "VerticalSpeed";
        // Bool
        public const string IsGrounded = "IsGrounded";
        public const string IsCrouching = "IsCrouching";
        public const string IsSliding = "IsSliding";
        public const string IsAiming = "IsAiming";
        public const string IsDead = "IsDead";
        // Trigger
        public const string Jump = "Jump";
        public const string Fire = "Fire";
        public const string Reload = "Reload";
        public const string Hit = "Hit";

        public const string UpperBodyLayer = "UpperBody";

        public static readonly int MoveXHash = Animator.StringToHash(MoveX);
        public static readonly int MoveZHash = Animator.StringToHash(MoveZ);
        public static readonly int SpeedHash = Animator.StringToHash(Speed);
        public static readonly int VerticalSpeedHash = Animator.StringToHash(VerticalSpeed);
        public static readonly int IsGroundedHash = Animator.StringToHash(IsGrounded);
        public static readonly int IsCrouchingHash = Animator.StringToHash(IsCrouching);
        public static readonly int IsSlidingHash = Animator.StringToHash(IsSliding);
        public static readonly int IsAimingHash = Animator.StringToHash(IsAiming);
        public static readonly int IsDeadHash = Animator.StringToHash(IsDead);
        public static readonly int JumpHash = Animator.StringToHash(Jump);
        public static readonly int FireHash = Animator.StringToHash(Fire);
        public static readonly int ReloadHash = Animator.StringToHash(Reload);
        public static readonly int HitHash = Animator.StringToHash(Hit);
    }
}
