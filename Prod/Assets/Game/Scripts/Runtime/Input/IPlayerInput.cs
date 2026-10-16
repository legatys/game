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
        Vector2 Move { get; }
        Vector2 Look { get; }
        /// <summary>true — Look это дельта мыши (пиксели за кадр), false — стик (умножать на deltaTime).</summary>
        bool LookIsDelta { get; }

        bool JumpPressed { get; }
        bool SprintHeld { get; }
        bool CrouchPressed { get; }
        bool CrouchHeld { get; }

        bool FirePressed { get; }
        bool FireHeld { get; }
        bool AimHeld { get; }
        bool ReloadPressed { get; }
        bool InteractPressed { get; }

        bool NextWeaponPressed { get; }
        bool PreviousWeaponPressed { get; }
        /// <summary>Индекс слота оружия (0..3), нажатого в этом кадре, или -1.</summary>
        int WeaponSlotPressed { get; }

        bool SwitchShoulderPressed { get; }
        bool PausePressed { get; }

        /// <summary>Выключает геймплейный ввод (пауза, смерть, катсцена). PausePressed продолжает работать.</summary>
        bool GameplayEnabled { get; set; }
    }
}
