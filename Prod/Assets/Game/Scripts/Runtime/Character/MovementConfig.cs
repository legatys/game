using UnityEngine;

namespace Game.Character
{
    /// <summary>
    /// Data-driven настройки передвижения (ScriptableObject). Разным персонажам — разные ассеты,
    /// код при этом один. Создать: ПКМ в Project → Create → Game/Movement Config.
    /// </summary>
    [CreateAssetMenu(fileName = "MovementConfig", menuName = "Game/Movement Config")]
    public sealed class MovementConfig : ScriptableObject
    {
        /// <summary>Скорость шага (при прицеливании), м/с.</summary>
        [Header("Скорости (м/с) — используются и в blend tree аниматора")]
        public float walkSpeed = 2f;
        /// <summary>Скорость бега по умолчанию, м/с.</summary>
        public float runSpeed = 4.5f;
        /// <summary>Скорость спринта, м/с.</summary>
        public float sprintSpeed = 7f;
        /// <summary>Скорость в приседе, м/с.</summary>
        public float crouchSpeed = 2f;

        /// <summary>Разгон на земле, м/с².</summary>
        [Header("Разгон / торможение (м/с²)")]
        public float acceleration = 25f;
        /// <summary>Торможение на земле без ввода, м/с².</summary>
        public float deceleration = 30f;
        /// <summary>Управление в воздухе, м/с².</summary>
        public float airAcceleration = 6f;

        [Header("Поворот")]
        [Tooltip("Скорость поворота персонажа, градусов в секунду")]
        public float rotationSpeed = 720f;

        /// <summary>Гравитация (отрицательная), м/с².</summary>
        [Header("Прыжок и гравитация")]
        public float gravity = -20f;
        /// <summary>Высота прыжка, м.</summary>
        public float jumpHeight = 1.3f;
        [Tooltip("Сколько дополнительных прыжков в воздухе (1 = двойной прыжок)")]
        public int maxAirJumps = 1;
        [Tooltip("Время после схода с края, когда прыжок ещё засчитывается как с земли")]
        public float coyoteTime = 0.12f;
        [Tooltip("Нажатие прыжка чуть раньше приземления запоминается на это время")]
        public float jumpBufferTime = 0.15f;

        /// <summary>Рост капсулы стоя, м.</summary>
        [Header("Рост капсулы")]
        public float standHeight = 1.8f;
        /// <summary>Рост капсулы в приседе и подкате, м.</summary>
        public float crouchHeight = 1.1f;
        [Tooltip("Скорость изменения роста капсулы, м/с")]
        public float heightChangeSpeed = 6f;

        /// <summary>Скорость в начале подката, м/с.</summary>
        [Header("Подкат (Shift + C во время бега)")]
        public float slideStartSpeed = 9f;
        /// <summary>Минимальная скорость бега для начала подката, м/с.</summary>
        public float slideMinStartSpeed = 5f;
        /// <summary>Торможение во время подката, м/с².</summary>
        public float slideFriction = 7f;
        /// <summary>Максимальная длительность подката, сек.</summary>
        public float slideMaxDuration = 1f;
        /// <summary>Пауза между подкатами, сек.</summary>
        public float slideCooldown = 0.4f;
        /// <summary>Насколько можно поворачивать во время подката (0..1).</summary>
        [Range(0f, 1f)] public float slideSteering = 0.15f;
    }
}
