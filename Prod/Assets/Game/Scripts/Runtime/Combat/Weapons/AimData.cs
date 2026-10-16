using UnityEngine;

namespace Game.Combat.Weapons
{
    /// <summary>Куда и как стрелять в этом кадре. Заполняется владельцем (игрок/ИИ) и передаётся в оружие.</summary>
    public readonly struct AimData
    {
        public readonly Vector3 AimPoint;
        public readonly GameObject Owner;
        public readonly bool IsAiming;
        /// <summary>Множитель разброса от движения, прыжка, приседа.</summary>
        public readonly float SpreadMultiplier;

        public AimData(Vector3 aimPoint, GameObject owner, bool isAiming, float spreadMultiplier = 1f)
        {
            AimPoint = aimPoint;
            Owner = owner;
            IsAiming = isAiming;
            SpreadMultiplier = spreadMultiplier;
        }
    }

    /// <summary>Контекст одного выстрела для <see cref="IShotStrategy"/>.</summary>
    public readonly struct ShotContext
    {
        public readonly WeaponDefinition Definition;
        public readonly Vector3 Origin;
        public readonly Vector3 AimPoint;
        public readonly GameObject Owner;
        public readonly float SpreadDegrees;

        public ShotContext(WeaponDefinition definition, Vector3 origin, Vector3 aimPoint, GameObject owner, float spreadDegrees)
        {
            Definition = definition;
            Origin = origin;
            AimPoint = aimPoint;
            Owner = owner;
            SpreadDegrees = spreadDegrees;
        }

        public Vector3 BaseDirection
        {
            get
            {
                var direction = AimPoint - Origin;
                return direction.sqrMagnitude > 0.0001f ? direction.normalized : Vector3.forward;
            }
        }

        public Transform OwnerRoot => Owner != null ? Owner.transform : null;
    }
}
