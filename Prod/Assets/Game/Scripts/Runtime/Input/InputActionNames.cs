namespace Game.Inputs
{
    /// <summary>Имена карты и действий из InputSystem_Actions.inputactions — единственное место, где они записаны строками.</summary>
    public static class InputActionNames
    {
        /// <summary>Карта действий игрока.</summary>
        public const string PlayerMap = "Player";

        /// <summary>Действие движения.</summary>
        public const string Move = "Move";
        /// <summary>Действие обзора.</summary>
        public const string Look = "Look";
        /// <summary>Действие прыжка.</summary>
        public const string Jump = "Jump";
        /// <summary>Действие бега.</summary>
        public const string Sprint = "Sprint";
        /// <summary>Действие приседа/подката.</summary>
        public const string Crouch = "Crouch";
        /// <summary>Действие огня.</summary>
        public const string Attack = "Attack";
        /// <summary>Действие прицеливания.</summary>
        public const string Aim = "Aim";
        /// <summary>Действие перезарядки.</summary>
        public const string Reload = "Reload";
        /// <summary>Действие взаимодействия.</summary>
        public const string Interact = "Interact";
        /// <summary>Действие "следующее оружие".</summary>
        public const string NextWeapon = "Next";
        /// <summary>Действие "предыдущее оружие".</summary>
        public const string PreviousWeapon = "Previous";
        /// <summary>Действие смены плеча.</summary>
        public const string SwitchShoulder = "SwitchShoulder";
        /// <summary>Действие паузы.</summary>
        public const string Pause = "Pause";

        /// <summary>Действия выбора слотов оружия 1–4.</summary>
        public static readonly string[] WeaponSlots = { "Weapon1", "Weapon2", "Weapon3", "Weapon4" };
    }
}
