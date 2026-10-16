using Game.Core;
using Game.Inputs;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.UI
{
    /// <summary>
    /// Пауза по кнопке Pause (Esc): останавливает время, отключает геймплейный ввод,
    /// освобождает курсор и публикует <see cref="GamePausedEvent"/>. Также управляет захватом курсора.
    /// </summary>
    public sealed class PauseMenu : MonoBehaviour
    {
        private IPlayerInput _input;
        private bool _paused;

        /// <summary>Включена ли пауза.</summary>
        public bool IsPaused => _paused;

        private void Start() => SetCursorLocked(true);

        private void OnDestroy()
        {
            if (_paused) Time.timeScale = 1f;
        }

        private void Update()
        {
            if (_input == null && !ServiceLocator.TryGet(out _input)) return;

            if (_input.PausePressed) SetPaused(!_paused);

            // Курсор мог освободиться (Alt+Tab, клик вне окна) — захватываем обратно по клику.
            if (!_paused && Cursor.lockState != CursorLockMode.Locked && _input.FirePressed) SetCursorLocked(true);
        }

        /// <summary>Включить/выключить паузу.</summary>
        public void SetPaused(bool paused)
        {
            _paused = paused;
            Time.timeScale = paused ? 0f : 1f;
            if (_input != null) _input.GameplayEnabled = !paused;
            SetCursorLocked(!paused);
            EventBus.Publish(new GamePausedEvent(paused));
        }

        private void OnGUI()
        {
            if (!_paused) return;

            var rect = new Rect(Screen.width * 0.5f - 110, Screen.height * 0.5f - 80, 220, 160);
            GUI.Box(rect, "Пауза");
            GUILayout.BeginArea(new Rect(rect.x + 10, rect.y + 30, rect.width - 20, rect.height - 40));
            if (GUILayout.Button("Продолжить", GUILayout.Height(30))) SetPaused(false);
            if (GUILayout.Button("Перезапустить", GUILayout.Height(30)))
            {
                SetPaused(false);
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }
            if (GUILayout.Button("Выход", GUILayout.Height(30))) Quit();
            GUILayout.EndArea();
        }

        private static void SetCursorLocked(bool locked)
        {
            Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !locked;
        }

        private static void Quit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
