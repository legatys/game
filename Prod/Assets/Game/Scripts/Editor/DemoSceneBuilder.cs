using System.Linq;
using Game.Cameras;
using Game.Character;
using Game.Demo;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.EditorTools
{
    /// <summary>
    /// Собирает сцену Demo из сгенерированных префабов. Раскладку уровня строит тот же
    /// <see cref="DemoLevelBuilder"/>, что и сцена Playground, — отличие только в том, что здесь
    /// ставятся экземпляры префабов, которые можно редактировать.
    /// </summary>
    internal static class DemoSceneBuilder
    {
        public static Scene Build(DemoPrefabs prefabs, CameraConfig cameraConfig)
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            var light = Object.FindFirstObjectByType<Light>();
            if (light != null)
            {
                light.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
                light.shadows = LightShadows.Soft;
            }

            var level = new DemoLevelBuilder(AssetMaterialProvider.Instance, prefabs, PlacePrefab);
            level.Build(null);
            var spawn = level.SpawnPoint;

            var player = PlacePrefab(prefabs.Player, spawn.position, spawn.rotation, null);
            DemoLevelBuilder.SetupCamera(cameraConfig, player.transform.position);
            DemoLevelBuilder.BuildSystems(prefabs.Player.GetComponent<PlayerCharacter>(), spawn);

            EditorAssetUtility.EnsureFolder(SkeletonPaths.Scenes);
            EditorSceneManager.SaveScene(scene, SkeletonPaths.DemoScene);
            AddToBuildSettings(SkeletonPaths.DemoScene);
            return scene;
        }

        private static GameObject PlacePrefab(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent)
        {
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            if (parent != null) instance.transform.SetParent(parent, true);
            instance.transform.SetPositionAndRotation(position, rotation);
            return instance;
        }

        private static void AddToBuildSettings(string path)
        {
            var scenes = EditorBuildSettings.scenes.Where(s => s.path != path).ToList();
            scenes.Insert(0, new EditorBuildSettingsScene(path, true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }
    }
}
