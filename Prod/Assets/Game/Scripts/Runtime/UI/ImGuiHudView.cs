using UnityEngine;

namespace Game.UI
{
    /// <summary>
    /// Реализация HUD на IMGUI (OnGUI): не требует Canvas и префабов, удобно для прототипа.
    /// Для релиза замените на uGUI/TextMeshPro-реализацию <see cref="IHudView"/>.
    /// </summary>
    public sealed class ImGuiHudView : MonoBehaviour, IHudView
    {
        [SerializeField] private bool showControlsHelp = true;
        [SerializeField] private Color crosshairColor = new Color(1f, 1f, 1f, 0.9f);
        [SerializeField] private Color hitColor = Color.white;
        [SerializeField] private Color killColor = new Color(1f, 0.25f, 0.2f);

        private const string ControlsHelp =
            "WASD — движение    Shift — бег    Space — прыжок (x2 в воздухе)\n" +
            "C / Ctrl — присесть, в беге — подкат\n" +
            "ЛКМ — огонь    ПКМ — прицел    R — перезарядка\n" +
            "1-4 / колесо — оружие    V — сменить плечо    Esc — пауза";

        private float _health = 1f, _maxHealth = 1f;
        private string _weaponName;
        private int _magazine, _magazineSize, _reserve;
        private bool _reloading;
        private float _reloadProgress;
        private float _spreadPixels;
        private bool _aiming;
        private float _hitTimer;
        private bool _hitKill, _hitCritical;
        private float _damageFlash;
        private float _respawnSeconds;
        private string _debugText = string.Empty;

        private GUIStyle _bigLabel, _smallLabel, _centerLabel;

        public void SetHealth(float current, float max)
        {
            _health = current;
            _maxHealth = Mathf.Max(1f, max);
        }

        public void SetWeapon(string weaponName, int magazine, int magazineSize, int reserve)
        {
            _weaponName = weaponName;
            _magazine = magazine;
            _magazineSize = magazineSize;
            _reserve = reserve;
        }

        public void SetReload(bool reloading, float progress01)
        {
            _reloading = reloading;
            _reloadProgress = progress01;
        }

        public void SetCrosshair(float spreadPixels, bool aiming)
        {
            _spreadPixels = spreadPixels;
            _aiming = aiming;
        }

        public void ShowHitMarker(bool killed, bool critical)
        {
            _hitTimer = killed ? 0.35f : 0.15f;
            _hitKill = killed;
            _hitCritical = critical;
        }

        public void ShowDamageFlash(float strength01) => _damageFlash = Mathf.Max(_damageFlash, strength01);
        public void SetRespawnCountdown(float seconds) => _respawnSeconds = seconds;
        public void SetDebugText(string text) => _debugText = text ?? string.Empty;

        private void Update()
        {
            _hitTimer -= Time.unscaledDeltaTime;
            _damageFlash = Mathf.MoveTowards(_damageFlash, 0f, Time.unscaledDeltaTime * 1.5f);
        }

        private void OnGUI()
        {
            EnsureStyles();
            DrawDamageFlash();
            DrawCrosshair();
            DrawHitMarker();
            DrawHealth();
            DrawAmmo();
            DrawTexts();
        }

        private void EnsureStyles()
        {
            if (_bigLabel != null) return;

            _bigLabel = new GUIStyle(GUI.skin.label) { fontSize = 28, fontStyle = FontStyle.Bold, alignment = TextAnchor.LowerRight };
            _smallLabel = new GUIStyle(GUI.skin.label) { fontSize = 13 };
            _centerLabel = new GUIStyle(GUI.skin.label) { fontSize = 26, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
        }

        private void DrawDamageFlash()
        {
            if (_damageFlash <= 0f) return;
            Fill(new Rect(0, 0, Screen.width, Screen.height), new Color(0.8f, 0f, 0f, _damageFlash * 0.35f));
        }

        private void DrawCrosshair()
        {
            if (_respawnSeconds > 0f) return;

            var center = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
            var gap = 4f + _spreadPixels;
            const float length = 8f, thickness = 2f;

            Fill(new Rect(center.x - thickness / 2, center.y - gap - length, thickness, length), crosshairColor);
            Fill(new Rect(center.x - thickness / 2, center.y + gap, thickness, length), crosshairColor);
            Fill(new Rect(center.x - gap - length, center.y - thickness / 2, length, thickness), crosshairColor);
            Fill(new Rect(center.x + gap, center.y - thickness / 2, length, thickness), crosshairColor);
            if (_aiming) Fill(new Rect(center.x - 1, center.y - 1, 2, 2), crosshairColor);
        }

        private void DrawHitMarker()
        {
            if (_hitTimer <= 0f) return;

            var center = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
            var color = _hitKill ? killColor : hitColor;
            var size = _hitCritical || _hitKill ? 12f : 9f;

            for (var i = 0; i < 4; i++)
            {
                var matrix = GUI.matrix;
                GUIUtility.RotateAroundPivot(45f + 90f * i, center);
                Fill(new Rect(center.x + 6f, center.y - 1f, size, 2f), color);
                GUI.matrix = matrix;
            }
        }

        private void DrawHealth()
        {
            var rect = new Rect(20, Screen.height - 40, 260, 18);
            Fill(rect, new Color(0f, 0f, 0f, 0.5f));
            var ratio = Mathf.Clamp01(_health / _maxHealth);
            Fill(new Rect(rect.x + 2, rect.y + 2, (rect.width - 4) * ratio, rect.height - 4), Color.Lerp(Color.red, Color.green, ratio));
            GUI.Label(new Rect(rect.x, rect.y - 20, 200, 20), $"HP {Mathf.CeilToInt(_health)}", _smallLabel);
        }

        private void DrawAmmo()
        {
            if (_weaponName == null) return;

            GUI.Label(new Rect(Screen.width - 320, Screen.height - 80, 300, 40), $"{_magazine} / {_reserve}", _bigLabel);
            GUI.Label(new Rect(Screen.width - 220, Screen.height - 36, 200, 20), _weaponName, _smallLabel);

            if (!_reloading) return;
            var rect = new Rect(Screen.width * 0.5f - 60, Screen.height * 0.5f + 40, 120, 6);
            Fill(rect, new Color(0f, 0f, 0f, 0.5f));
            Fill(new Rect(rect.x, rect.y, rect.width * _reloadProgress, rect.height), Color.white);
        }

        private void DrawTexts()
        {
            if (showControlsHelp) GUI.Label(new Rect(20, 20, 520, 80), ControlsHelp, _smallLabel);
            if (!string.IsNullOrEmpty(_debugText)) GUI.Label(new Rect(Screen.width - 200, 20, 190, 70), _debugText, _smallLabel);
            if (_respawnSeconds > 0f)
                GUI.Label(new Rect(0, 0, Screen.width, Screen.height), $"Вы погибли\nВозрождение через {Mathf.CeilToInt(_respawnSeconds)}", _centerLabel);
        }

        private static void Fill(Rect rect, Color color)
        {
            var previous = GUI.color;
            GUI.color = color;
            GUI.DrawTexture(rect, Texture2D.whiteTexture);
            GUI.color = previous;
        }
    }
}
