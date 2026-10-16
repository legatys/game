namespace Game.Inputs
{
    /// <summary>Имена карты и действий из InputSystem_Actions.inputactions — единственное место, где они записаны строками.</summary>
    public static class InputActionNames
    {
        public const string PlayerMap = "Player";

        public const string Move = "Move";
        public const string Look = "Look";
        public const string Jump = "Jump";
        public const string Sprint = "Sprint";
        public const string Crouch = "Crouch";
        public const string Attack = "Attack";
        public const string Aim = "Aim";
        public const string Reload = "Reload";
        public const string Interact = "Interact";
        public const string NextWeapon = "Next";
        public const string PreviousWeapon = "Previous";
        public const string SwitchShoulder = "SwitchShoulder";
        public const string Pause = "Pause";

        public static readonly string[] WeaponSlots = { "Weapon1", "Weapon2", "Weapon3", "Weapon4" };
    }
}
