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
        [Header("Скорости (м/с) — используются и в blend tree аниматора")]
        public float walkSpeed = 2f;
        public float runSpeed = 4.5f;
        public float sprintSpeed = 7f;
        public float crouchSpeed = 2f;

        [Header("Разгон / торможение (м/с²)")]
        public float acceleration = 25f;
        public float deceleration = 30f;
        public float airAcceleration = 6f;

        [Header("Поворот")]
        [Tooltip("Скорость поворота персонажа, градусов в секунду")]
        public float rotationSpeed = 720f;

        [Header("Прыжок и гравитация")]
        public float gravity = -20f;
        public float jumpHeight = 1.3f;
        [Tooltip("Сколько дополнительных прыжков в воздухе (1 = двойной прыжок)")]
        public int maxAirJumps = 1;
        [Tooltip("Время после схода с края, когда прыжок ещё засчитывается как с земли")]
        public float coyoteTime = 0.12f;
        [Tooltip("Нажатие прыжка чуть раньше приземления запоминается на это время")]
        public float jumpBufferTime = 0.15f;

        [Header("Рост капсулы")]
        public float standHeight = 1.8f;
        public float crouchHeight = 1.1f;
        [Tooltip("Скорость изменения роста капсулы, м/с")]
        public float heightChangeSpeed = 6f;

        [Header("Подкат (Shift + C во время бега)")]
        public float slideStartSpeed = 9f;
        public float slideMinStartSpeed = 5f;
        public float slideFriction = 7f;
        public float slideMaxDuration = 1f;
        public float slideCooldown = 0.4f;
        [Range(0f, 1f)] public float slideSteering = 0.15f;
    }
}
