namespace Game.UI
{
    /// <summary>
    /// View в паттерне MVP: только отображение, никакой логики. Сейчас реализован на IMGUI
    /// (<see cref="ImGuiHudView"/>), можно сделать реализацию на uGUI/TextMeshPro/UI Toolkit,
    /// не трогая <see cref="HudPresenter"/>.
    /// </summary>
    public interface IHudView
    {
        void SetHealth(float current, float max);
        /// <summary>weaponName == null — оружия нет.</summary>
        void SetWeapon(string weaponName, int magazine, int magazineSize, int reserve);
        void SetReload(bool reloading, float progress01);
        void SetCrosshair(float spreadPixels, bool aiming);
        void ShowHitMarker(bool killed, bool critical);
        void ShowDamageFlash(float strength01);
        void SetRespawnCountdown(float seconds);
        void SetDebugText(string text);
    }
}
