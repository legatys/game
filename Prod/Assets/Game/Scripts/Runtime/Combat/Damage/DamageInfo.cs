using UnityEngine;

namespace Game.Combat
{
    /// <summary>Неизменяемое описание одного попадания: сколько, куда, откуда и кто стрелял.</summary>
    public readonly struct DamageInfo
    {
        public readonly float Amount;
        public readonly Vector3 Point;
        public readonly Vector3 Direction;
        public readonly GameObject Instigator;
        /// <summary>Критическое попадание (например, в голову).</summary>
        public readonly bool IsCritical;

        public DamageInfo(float amount, Vector3 point, Vector3 direction, GameObject instigator, bool isCritical = false)
        {
            Amount = amount;
            Point = point;
            Direction = direction;
            Instigator = instigator;
            IsCritical = isCritical;
        }

        public DamageInfo Scaled(float multiplier, bool critical) =>
            new DamageInfo(Amount * multiplier, Point, Direction, Instigator, IsCritical || critical);
    }

    /// <summary>Всё, что можно повредить: здоровье, хитбоксы, разрушаемые объекты.</summary>
    public interface IDamageable
    {
        bool IsAlive { get; }
        /// <summary>Корневой объект "жертвы" — чтобы взрыв не задел один объект дважды через разные хитбоксы.</summary>
        Transform DamageRoot { get; }
        void ApplyDamage(in DamageInfo info);
    }
}
