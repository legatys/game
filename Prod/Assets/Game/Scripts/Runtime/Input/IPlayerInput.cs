using UnityEngine;

namespace Game.Inputs
{
    /// <summary>
    /// Абстракция ввода. Персонаж, камера и оружие зависят только от этого интерфейса,
    /// а не от Input System — поэтому управление можно подменить (ИИ, реплей, тесты, сеть).
    /// "Pressed" = нажато в этом кадре, "Held" = удерживается.
    /// </summary>
    public interface IPlayerInput
    {
        /// <summary>Движение: x — вбок, y — вперёд (-1..1).</summary>
        Vector2 Move { get; }
        /// <summary>Поворот камеры: дельта мыши или отклонение стика.</summary>
        Vector2 Look { get; }
        /// <summary>true — Look это дельта мыши (пиксели за кадр), false — стик (умножать на deltaTime).</summary>
        bool LookIsDelta { get; }

        /// <summary>Прыжок нажат в этом кадре.</summary>
        bool JumpPressed { get; }
        /// <summary>Бег удерживается.</summary>
        bool SprintHeld { get; }
        /// <summary>Присед нажат в этом кадре.</summary>
        bool CrouchPressed { get; }
        /// <summary>Присед удерживается.</summary>
        bool CrouchHeld { get; }

        /// <summary>Огонь нажат в этом кадре.</summary>
        bool FirePressed { get; }
        /// <summary>Огонь удерживается.</summary>
        bool FireHeld { get; }
        /// <summary>Прицел удерживается.</summary>
        bool AimHeld { get; }
        /// <summary>Перезарядка нажата в этом кадре.</summary>
        bool ReloadPressed { get; }
        /// <summary>Взаимодействие нажато в этом кадре.</summary>
        bool InteractPressed { get; }

        /// <summary>Следующее оружие.</summary>
        bool NextWeaponPressed { get; }
        /// <summary>Предыдущее оружие.</summary>
        bool PreviousWeaponPressed { get; }
        /// <summary>Индекс слота оружия (0..3), нажатого в этом кадре, или -1.</summary>
        int WeaponSlotPressed { get; }

        /// <summary>Сменить плечо камеры.</summary>
        bool SwitchShoulderPressed { get; }
        /// <summary>Пауза (работает и при выключенном геймплейном вводе).</summary>
        bool PausePressed { get; }

        /// <summary>Выключает геймплейный ввод (пауза, смерть, катсцена). PausePressed продолжает работать.</summary>
        bool GameplayEnabled { get; set; }
    }
}
