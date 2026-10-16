using UnityEngine;

namespace Game.Cameras
{
    /// <summary>То, за чем следует камера (игрок). Камера не знает конкретного класса персонажа.</summary>
    public interface ICameraTarget
    {
        Transform Root { get; }
        /// <summary>Точка, вокруг которой вращается камера (обычно уровень плеч).</summary>
        Vector3 PivotPosition { get; }
        bool IsAiming { get; }
    }

    /// <summary>Сервис камеры для геймплея: направление взгляда, точка прицеливания, отдача.</summary>
    public interface ICameraRig
    {
        UnityEngine.Camera Camera { get; }
        /// <summary>Поворот камеры только по горизонтали — для движения относительно камеры.</summary>
        Quaternion YawRotation { get; }
        Vector3 Forward { get; }
        /// <summary>Точка в мире под прицелом (центр экрана), игнорируя коллайдеры ignoreRoot.</summary>
        Vector3 GetAimPoint(float maxDistance, Transform ignoreRoot);
        void AddRecoil(float pitchDegrees, float yawDegrees);
        void SetTarget(ICameraTarget target);
    }
}
