using UnityEngine;

namespace Game.Cameras
{
    /// <summary>Настройки камеры от третьего лица (ScriptableObject).</summary>
    [CreateAssetMenu(fileName = "CameraConfig", menuName = "Game/Camera Config")]
    public sealed class CameraConfig : ScriptableObject
    {
        [Header("Чувствительность")]
        [Tooltip("Градусов на пиксель движения мыши")]
        public float mouseSensitivity = 0.12f;
        [Tooltip("Градусов в секунду при полном отклонении стика")]
        public float gamepadSensitivity = 180f;
        [Range(0.1f, 1f)] public float aimSensitivityMultiplier = 0.6f;
        public bool invertY;
        public float minPitch = -60f;
        public float maxPitch = 75f;

        [Header("Позиция (обычный режим)")]
        public float distance = 3.6f;
        [Tooltip("Смещение вбок (через плечо). Знак меняется кнопкой SwitchShoulder")]
        public float shoulderOffset = 0.6f;
        public float heightOffset = 0.15f;
        public float fieldOfView = 60f;

        [Header("Прицеливание")]
        public float aimDistance = 1.6f;
        public float aimShoulderOffset = 0.7f;
        public float aimFieldOfView = 42f;
        public float aimTransitionSpeed = 6f;
        public float shoulderSwitchSpeed = 6f;

        [Header("Сглаживание и коллизии")]
        public float followSmoothTime = 0.04f;
        public float collisionRadius = 0.2f;
        public LayerMask collisionMask = ~0;
        [Tooltip("Скорость возврата камеры на дистанцию после столкновения со стеной, м/с")]
        public float distanceRecoverSpeed = 6f;

        [Header("Отдача")]
        [Tooltip("Скорость возврата прицела после отдачи, градусов в секунду")]
        public float recoilRecovery = 12f;
        public float recoilKickSpeed = 40f;
    }
}
