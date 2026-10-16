using UnityEngine;

namespace Game.Cameras
{
    /// <summary>То, за чем следует камера (игрок). Камера не знает конкретного класса персонажа.</summary>
    public interface ICameraTarget
    {
        /// <summary>Корень цели (её коллайдеры игнорируются камерой).</summary>
        Transform Root { get; }
        /// <summary>Точка, вокруг которой вращается камера (обычно уровень плеч).</summary>
        Vector3 PivotPosition { get; }
        /// <summary>Цель целится — камера приближается.</summary>
        bool IsAiming { get; }
    }

    /// <summary>Сервис камеры для геймплея: направление взгляда, точка прицеливания, отдача.</summary>
    public interface ICameraRig
    {
        /// <summary>Unity-камера.</summary>
        UnityEngine.Camera Camera { get; }
        /// <summary>Поворот камеры только по горизонтали — для движения относительно камеры.</summary>
        Quaternion YawRotation { get; }
        /// <summary>Направление взгляда камеры.</summary>
        Vector3 Forward { get; }
        /// <summary>Точка в мире под прицелом (центр экрана), игнорируя коллайдеры ignoreRoot.</summary>
        Vector3 GetAimPoint(float maxDistance, Transform ignoreRoot);
        /// <summary>Добавить отдачу: вверх на <paramref name="pitchDegrees"/>, вбок на <paramref name="yawDegrees"/>.</summary>
        void AddRecoil(float pitchDegrees, float yawDegrees);
        /// <summary>Следовать за целью.</summary>
        void SetTarget(ICameraTarget target);
    }
}
