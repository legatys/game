using UnityEngine;

namespace Game.Combat.Weapons
{
    /// <summary>Куда и как стрелять в этом кадре. Заполняется владельцем (игрок/ИИ) и передаётся в оружие.</summary>
    public readonly struct AimData
    {
        /// <summary>Точка в мире, куда целится стрелок.</summary>
        public readonly Vector3 AimPoint;
        /// <summary>Стрелок (его коллайдеры игнорируются выстрелом).</summary>
        public readonly GameObject Owner;
        /// <summary>Прицеливание (меньший разброс).</summary>
        public readonly bool IsAiming;
        /// <summary>Множитель разброса от движения, прыжка, приседа.</summary>
        public readonly float SpreadMultiplier;

        /// <summary>Создаёт данные прицеливания.</summary>
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
        /// <summary>Определение оружия.</summary>
        public readonly WeaponDefinition Definition;
        /// <summary>Откуда летит выстрел (дуло).</summary>
        public readonly Vector3 Origin;
        /// <summary>Куда целились.</summary>
        public readonly Vector3 AimPoint;
        /// <summary>Стрелок.</summary>
        public readonly GameObject Owner;
        /// <summary>Итоговый разброс в градусах.</summary>
        public readonly float SpreadDegrees;

        /// <summary>Создаёт контекст выстрела.</summary>
        public ShotContext(WeaponDefinition definition, Vector3 origin, Vector3 aimPoint, GameObject owner, float spreadDegrees)
        {
            Definition = definition;
            Origin = origin;
            AimPoint = aimPoint;
            Owner = owner;
            SpreadDegrees = spreadDegrees;
        }

        /// <summary>Направление от дула к точке прицела без разброса.</summary>
        public Vector3 BaseDirection
        {
            get
            {
                var direction = AimPoint - Origin;
                return direction.sqrMagnitude > 0.0001f ? direction.normalized : Vector3.forward;
            }
        }

        /// <summary>Transform стрелка (или null).</summary>
        public Transform OwnerRoot => Owner != null ? Owner.transform : null;
    }
}
