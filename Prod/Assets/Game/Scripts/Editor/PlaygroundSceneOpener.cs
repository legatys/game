using UnityEditor;
using UnityEditor.SceneManagement;

namespace Game.EditorTools
{
    /// <summary>
    /// При запуске редактора открывает сцену Playground, если открыта пустая безымянная сцена
    /// (так Unity делает при первом открытии проекта). Один раз за сессию, ваши сцены не трогает.
    /// </summary>
    [InitializeOnLoad]
    internal static class PlaygroundSceneOpener
    {
        /// <summary>Путь к сцене Playground.</summary>
        public const string ScenePath = "Assets/Game/Scenes/Playground.unity";
        private const string SessionKey = "Game.PlaygroundSceneOpener.Done";

        static PlaygroundSceneOpener() => EditorApplication.delayCall += OpenIfUntitled;

        /// <summary>Пункт меню Tools → TPS Skeleton → Open Playground Scene.</summary>
        [MenuItem("Tools/TPS Skeleton/Open Playground Scene", priority = -10)]
        public static void Open()
        {
            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                EditorSceneManager.OpenScene(ScenePath);
        }

        private static void OpenIfUntitled()
        {
            if (SessionState.GetBool(SessionKey, false)) return;
            SessionState.SetBool(SessionKey, true);

            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            var active = EditorSceneManager.GetActiveScene();
            if (!string.IsNullOrEmpty(active.path) || active.isDirty) return;
            if (AssetDatabase.LoadMainAssetAtPath(ScenePath) == null) return;

            EditorSceneManager.OpenScene(ScenePath);
        }
    }
}
