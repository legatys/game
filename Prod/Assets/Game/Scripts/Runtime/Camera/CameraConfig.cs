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
        /// <summary>Множитель чувствительности при прицеливании.</summary>
        [Range(0.1f, 1f)] public float aimSensitivityMultiplier = 0.6f;
        /// <summary>Инвертировать вертикальную ось.</summary>
        public bool invertY;
        /// <summary>Минимальный наклон камеры (взгляд вверх), градусы.</summary>
        public float minPitch = -60f;
        /// <summary>Максимальный наклон камеры (взгляд вниз), градусы.</summary>
        public float maxPitch = 75f;

        /// <summary>Дистанция до персонажа, м.</summary>
        [Header("Позиция (обычный режим)")]
        public float distance = 3.6f;
        [Tooltip("Смещение вбок (через плечо). Знак меняется кнопкой SwitchShoulder")]
        public float shoulderOffset = 0.6f;
        /// <summary>Смещение камеры вверх, м.</summary>
        public float heightOffset = 0.15f;
        /// <summary>Угол обзора, градусы.</summary>
        public float fieldOfView = 60f;

        /// <summary>Дистанция при прицеливании, м.</summary>
        [Header("Прицеливание")]
        public float aimDistance = 1.6f;
        /// <summary>Смещение через плечо при прицеливании, м.</summary>
        public float aimShoulderOffset = 0.7f;
        /// <summary>Угол обзора при прицеливании, градусы.</summary>
        public float aimFieldOfView = 42f;
        /// <summary>Скорость перехода в режим прицела.</summary>
        public float aimTransitionSpeed = 6f;
        /// <summary>Скорость смены плеча.</summary>
        public float shoulderSwitchSpeed = 6f;

        /// <summary>Сглаживание следования за персонажем, сек.</summary>
        [Header("Сглаживание и коллизии")]
        public float followSmoothTime = 0.04f;
        /// <summary>Радиус сферы для отталкивания от стен, м.</summary>
        public float collisionRadius = 0.2f;
        /// <summary>Слои, от которых отталкивается камера.</summary>
        public LayerMask collisionMask = ~0;
        [Tooltip("Скорость возврата камеры на дистанцию после столкновения со стеной, м/с")]
        public float distanceRecoverSpeed = 6f;

        [Header("Отдача")]
        [Tooltip("Скорость возврата прицела после отдачи, градусов в секунду")]
        public float recoilRecovery = 12f;
        /// <summary>Скорость "подброса" отдачи, градусов в секунду.</summary>
        public float recoilKickSpeed = 40f;
    }
}
