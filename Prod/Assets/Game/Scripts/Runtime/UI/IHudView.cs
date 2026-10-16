namespace Game.UI
{
    /// <summary>
    /// View в паттерне MVP: только отображение, никакой логики. Сейчас реализован на IMGUI
    /// (<see cref="ImGuiHudView"/>), можно сделать реализацию на uGUI/TextMeshPro/UI Toolkit,
    /// не трогая <see cref="HudPresenter"/>.
    /// </summary>
    public interface IHudView
    {
        /// <summary>Показать здоровье.</summary>
        void SetHealth(float current, float max);
        /// <summary>weaponName == null — оружия нет.</summary>
        void SetWeapon(string weaponName, int magazine, int magazineSize, int reserve);
        /// <summary>Показать/скрыть полосу перезарядки.</summary>
        void SetReload(bool reloading, float progress01);
        /// <summary>Размер прицела (радиус разброса в пикселях) и режим прицеливания.</summary>
        void SetCrosshair(float spreadPixels, bool aiming);
        /// <summary>Хит-маркер попадания (красный — убийство).</summary>
        void ShowHitMarker(bool killed, bool critical);
        /// <summary>Красная вспышка экрана при получении урона.</summary>
        void ShowDamageFlash(float strength01);
        /// <summary>Отсчёт до возрождения (0 — скрыть).</summary>
        void SetRespawnCountdown(float seconds);
        /// <summary>Отладочный текст в углу экрана.</summary>
        void SetDebugText(string text);
    }
}
