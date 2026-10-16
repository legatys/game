using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Game.EditorTools
{
    /// <summary>
    /// Паттерн Facade: одно меню Tools → TPS Skeleton скрывает всю цепочку генерации
    /// (конфиги → анимации → оружие → префабы → сцена).
    /// Существующие ScriptableObject-ы (настройки, оружие, AnimationSet) НЕ перезаписываются никогда.
    /// </summary>
    internal static class TpsSkeletonMenu
    {
        private const string Menu = "Tools/TPS Skeleton/";

        [MenuItem(Menu + "1. Build Demo Scene", priority = 0)]
        private static void BuildDemo()
        {
            var rebuildScene = true;
            if (EditorAssetUtility.Exists(SkeletonPaths.DemoScene))
                rebuildScene = EditorUtility.DisplayDialog("TPS Skeleton",
                    "Сцена Demo уже существует. Пересоздать её (ваши правки в сцене пропадут)?\n" +
                    "«Нет» — только догенерировать недостающие ассеты и открыть сцену.", "Пересоздать", "Нет");

            Generate(overwritePrefabs: false, rebuildScene);
        }

        [MenuItem(Menu + "2. Rebuild All Prefabs And Scene", priority = 1)]
        private static void RebuildAll()
        {
            if (!EditorUtility.DisplayDialog("TPS Skeleton",
                    "Пересоздать все сгенерированные префабы и сцену Demo? Настройки и оружие (ScriptableObject) сохранятся.",
                    "Пересоздать", "Отмена"))
                return;

            Generate(overwritePrefabs: true, rebuildScene: true);
        }

        [MenuItem(Menu + "3. Rebuild Animator Controller", priority = 20)]
        private static void RebuildAnimator()
        {
            var set = CoreAssetsBuilder.LoadAnimationSet(CoreAssetsBuilder.LoadMovementConfig());
            CoreAssetsBuilder.BuildAnimator(set);
            Selection.activeObject = set.controller;
            Debug.Log("[TPS Skeleton] Animator Controller пересобран из " + SkeletonPaths.AnimationSet);
        }

        [MenuItem(Menu + "Select Animation Set", priority = 40)]
        private static void SelectAnimationSet() => Ping(SkeletonPaths.AnimationSet);

        [MenuItem(Menu + "Select Movement Config", priority = 41)]
        private static void SelectMovementConfig() => Ping(SkeletonPaths.MovementConfig);

        [MenuItem(Menu + "Select Player Prefab", priority = 42)]
        private static void SelectPlayer() => Ping(SkeletonPaths.PlayerPrefab);

        private static void Generate(bool overwritePrefabs, bool rebuildScene)
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

            try
            {
                EditorUtility.DisplayProgressBar("TPS Skeleton", "Конфиги и анимации", 0.1f);
                var movement = CoreAssetsBuilder.LoadMovementConfig();
                var camera = CoreAssetsBuilder.LoadCameraConfig();
                var animationSet = CoreAssetsBuilder.LoadAnimationSet(movement);
                if (animationSet.controller == null || overwritePrefabs) CoreAssetsBuilder.BuildAnimator(animationSet);

                EditorUtility.DisplayProgressBar("TPS Skeleton", "Оружие и эффекты", 0.35f);
                var weapons = WeaponContentBuilder.Build(overwritePrefabs);

                EditorUtility.DisplayProgressBar("TPS Skeleton", "Префабы", 0.6f);
                var prefabs = PrefabContentBuilder.Build(movement, animationSet, weapons, overwritePrefabs);

                AssetDatabase.SaveAssets();

                EditorUtility.DisplayProgressBar("TPS Skeleton", "Сцена", 0.85f);
                if (rebuildScene) DemoSceneBuilder.Build(prefabs, camera);
                else EditorSceneManager.OpenScene(SkeletonPaths.DemoScene);

                AssetDatabase.SaveAssets();
                Debug.Log("[TPS Skeleton] Готово. Нажмите Play. Анимации: " + SkeletonPaths.AnimationSet);
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }

        private static void Ping(string path)
        {
            var asset = AssetDatabase.LoadMainAssetAtPath(path);
            if (asset == null)
            {
                Debug.LogWarning($"[TPS Skeleton] {path} ещё не создан — сначала выполните «1. Build Demo Scene».");
                return;
            }

            Selection.activeObject = asset;
            EditorGUIUtility.PingObject(asset);
        }
    }
}
