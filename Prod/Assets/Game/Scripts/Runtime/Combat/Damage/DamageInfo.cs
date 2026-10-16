using UnityEngine;

namespace Game.Combat
{
    /// <summary>Неизменяемое описание одного попадания: сколько, куда, откуда и кто стрелял.</summary>
    public readonly struct DamageInfo
    {
        /// <summary>Количество урона.</summary>
        public readonly float Amount;
        /// <summary>Точка попадания в мире.</summary>
        public readonly Vector3 Point;
        /// <summary>Направление удара.</summary>
        public readonly Vector3 Direction;
        /// <summary>Кто нанёс урон (стрелок).</summary>
        public readonly GameObject Instigator;
        /// <summary>Критическое попадание (например, в голову).</summary>
        public readonly bool IsCritical;

        /// <summary>Создаёт описание попадания.</summary>
        public DamageInfo(float amount, Vector3 point, Vector3 direction, GameObject instigator, bool isCritical = false)
        {
            Amount = amount;
            Point = point;
            Direction = direction;
            Instigator = instigator;
            IsCritical = isCritical;
        }

        /// <summary>Копия с уроном, умноженным на <paramref name="multiplier"/>; при <paramref name="critical"/> помечается как критическая.</summary>
        public DamageInfo Scaled(float multiplier, bool critical) =>
            new DamageInfo(Amount * multiplier, Point, Direction, Instigator, IsCritical || critical);
    }

    /// <summary>Всё, что можно повредить: здоровье, хитбоксы, разрушаемые объекты.</summary>
    public interface IDamageable
    {
        /// <summary>Жив ли объект.</summary>
        bool IsAlive { get; }
        /// <summary>Корневой объект "жертвы" — чтобы взрыв не задел один объект дважды через разные хитбоксы.</summary>
        Transform DamageRoot { get; }
        /// <summary>Нанести урон.</summary>
        void ApplyDamage(in DamageInfo info);
    }
}
