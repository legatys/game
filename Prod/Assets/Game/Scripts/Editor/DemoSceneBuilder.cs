using System.Linq;
using Game.Cameras;
using Game.Character;
using Game.Gameplay;
using Game.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.EditorTools
{
    /// <summary>
    /// Собирает тестовую сцену-полигон из примитивов: тир с мишенями, укрытия, тоннель для приседа
    /// и подката, лестница, рампа, паркур-блоки, батут, турель, аптечки, физические ящики.
    /// </summary>
    internal static class DemoSceneBuilder
    {
        private static Transform _level;
        private static Material _wall, _cover, _props;

        public static Scene Build(PrefabContentBuilder.Result prefabs, CameraConfig cameraConfig)
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            _wall = MaterialLibrary.Lit("Wall", new Color(0.3f, 0.32f, 0.36f));
            _cover = MaterialLibrary.Lit("Cover", new Color(0.85f, 0.55f, 0.25f));
            _props = MaterialLibrary.Lit("Crate", new Color(0.55f, 0.4f, 0.25f));
            _level = new GameObject("Level").transform;

            SetupLight();
            BuildGround();
            BuildShootingRange(prefabs);
            BuildMovementCourse(prefabs);
            BuildCombatArea(prefabs);

            var spawn = new GameObject("SpawnPoint").transform;
            spawn.SetPositionAndRotation(new Vector3(0f, 0.05f, -20f), Quaternion.identity);

            var player = (GameObject)PrefabUtility.InstantiatePrefab(prefabs.Player);
            player.transform.SetPositionAndRotation(spawn.position, spawn.rotation);

            SetupCamera(cameraConfig, player.transform);
            SetupSystems(prefabs, spawn);

            EditorAssetUtility.EnsureFolder(SkeletonPaths.Scenes);
            EditorSceneManager.SaveScene(scene, SkeletonPaths.DemoScene);
            AddToBuildSettings(SkeletonPaths.DemoScene);
            return scene;
        }

        // ---------- Окружение ----------

        private static void SetupLight()
        {
            var light = Object.FindFirstObjectByType<Light>();
            if (light == null) return;

            light.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
            light.shadows = LightShadows.Soft;
        }

        private static void SetupCamera(CameraConfig config, Transform player)
        {
            var camera = Camera.main;
            if (camera == null) camera = new GameObject("Main Camera") { tag = "MainCamera" }.AddComponent<Camera>();

            camera.nearClipPlane = 0.05f;
            camera.transform.SetPositionAndRotation(player.position + new Vector3(0.6f, 2f, -3.6f), Quaternion.Euler(10f, 0f, 0f));
            var rig = camera.gameObject.AddComponent<ThirdPersonCamera>();
            Serialized.Of(rig).Set("config", config).Apply();
        }

        private static void SetupSystems(PrefabContentBuilder.Result prefabs, Transform spawn)
        {
            var systems = new GameObject("Systems");

            var spawner = new GameObject("PlayerSpawner").AddComponent<PlayerSpawner>();
            spawner.transform.SetParent(systems.transform);
            Serialized.Of(spawner).Set("playerPrefab", prefabs.Player.GetComponent<PlayerCharacter>()).Set("spawnPoint", spawn).Apply();

            var hud = new GameObject("HUD");
            hud.transform.SetParent(systems.transform);
            hud.AddComponent<ImGuiHudView>();
            hud.AddComponent<HudPresenter>();

            var pause = new GameObject("PauseMenu");
            pause.transform.SetParent(systems.transform);
            pause.AddComponent<PauseMenu>();
        }

        private static void BuildGround()
        {
            var ground = EditorAssetUtility.Solid(PrimitiveType.Cube, "Ground", _level, new Vector3(0f, -0.5f, 0f), new Vector3(80f, 1f, 80f),
                MaterialLibrary.Checker("Ground", new Color(0.55f, 0.57f, 0.6f), new Color(0.47f, 0.49f, 0.52f), 40f));
            ground.isStatic = true;

            Block("Wall_N", new Vector3(0f, 2f, 40f), new Vector3(80f, 4f, 1f), _wall);
            Block("Wall_S", new Vector3(0f, 2f, -40f), new Vector3(80f, 4f, 1f), _wall);
            Block("Wall_E", new Vector3(40f, 2f, 0f), new Vector3(1f, 4f, 80f), _wall);
            Block("Wall_W", new Vector3(-40f, 2f, 0f), new Vector3(1f, 4f, 80f), _wall);
        }

        // ---------- Тир ----------

        private static void BuildShootingRange(PrefabContentBuilder.Result prefabs)
        {
            Sign("ТИР: ЛКМ огонь, ПКМ прицел, R перезарядка, 1-4 оружие. Голова = x2 урона", new Vector3(0f, 3f, 8f));

            Block("LowCover_L", new Vector3(-6f, 0.5f, 5f), new Vector3(3f, 1f, 0.6f), _cover);
            Block("LowCover_C", new Vector3(0f, 0.5f, 6f), new Vector3(3f, 1f, 0.6f), _cover);
            Block("LowCover_R", new Vector3(6f, 0.5f, 5f), new Vector3(3f, 1f, 0.6f), _cover);
            Block("TallWall_L", new Vector3(-10f, 1.5f, 0f), new Vector3(0.6f, 3f, 6f), _wall);
            Block("TallWall_R", new Vector3(10f, 1.5f, 0f), new Vector3(0.6f, 3f, 6f), _wall);

            var dummies = new[]
            {
                new Vector3(-6f, 0f, 12f), new Vector3(-2f, 0f, 15f), new Vector3(2f, 0f, 18f),
                new Vector3(6f, 0f, 22f), new Vector3(0f, 0f, 28f)
            };
            foreach (var position in dummies) Place(prefabs.Dummy, position, Quaternion.Euler(0f, 180f, 0f));

            // Физические ящики — их можно толкать выстрелами и взрывами.
            for (var x = 0; x < 3; x++)
            for (var y = 0; y < 3 - x; y++)
            {
                var crate = Block($"Crate_{x}_{y}", new Vector3(13f + x * 0.85f + y * 0.42f, 0.4f + y * 0.8f, 12f), Vector3.one * 0.8f, _props);
                crate.isStatic = false;
                crate.AddComponent<Rigidbody>().mass = 5f;
            }
        }

        // ---------- Полоса движения ----------

        private static void BuildMovementCourse(PrefabContentBuilder.Result prefabs)
        {
            // Тоннель: стоя не пройти (потолок 1.25 м), только присед (C) или подкат (Shift + C).
            Sign("ТОННЕЛЬ: присядь (C) или подкат на бегу (Shift + C)", new Vector3(-18f, 2.6f, -14.5f));
            Block("Tunnel_WallL", new Vector3(-19.4f, 0.8f, -10f), new Vector3(0.4f, 1.6f, 8f), _wall);
            Block("Tunnel_WallR", new Vector3(-16.6f, 0.8f, -10f), new Vector3(0.4f, 1.6f, 8f), _wall);
            Block("Tunnel_Roof", new Vector3(-18f, 1.4f, -10f), new Vector3(3.2f, 0.3f, 8f), _wall);

            // Лестница и площадка.
            Sign("ЛЕСТНИЦА", new Vector3(14f, 3f, -13f));
            for (var i = 0; i < 8; i++)
            {
                var height = 0.25f * (i + 1);
                Block($"Step_{i}", new Vector3(14f, height * 0.5f, -12f + 0.5f * i), new Vector3(3f, height, 0.5f), _cover);
            }
            Block("Stairs_Top", new Vector3(14f, 1f, -6.75f), new Vector3(3f, 2f, 3f), _cover);

            // Рампа.
            Sign("РАМПА", new Vector3(20f, 3.5f, -14f));
            var ramp = Block("Ramp", new Vector3(20f, 1.3f, -10f), new Vector3(3f, 0.3f, 8f), _cover);
            ramp.transform.rotation = Quaternion.Euler(-20f, 0f, 0f);
            Block("Ramp_Top", new Vector3(20f, 1.3f, -4.3f), new Vector3(4f, 2.6f, 4f), _cover);

            // Паркур: блоки под одиночный и двойной прыжок.
            Sign("ПРЫЖКИ: Space, в воздухе ещё раз — двойной прыжок", new Vector3(-15f, 4.2f, 15f));
            Block("Jump_1", new Vector3(-12f, 0.6f, 18f), new Vector3(2f, 1.2f, 2f), _cover);
            Block("Jump_2", new Vector3(-15f, 1f, 21f), new Vector3(2f, 2f, 2f), _cover);
            Block("Jump_3", new Vector3(-18f, 1.4f, 24f), new Vector3(2f, 2.8f, 2f), _cover);

            // Батут на высокую площадку.
            Sign("БАТУТ", new Vector3(8f, 2.5f, -14f));
            Place(prefabs.JumpPad, new Vector3(8f, 0f, -12f), Quaternion.identity);
            Block("HighPlatform", new Vector3(8f, 3.8f, -6f), new Vector3(4f, 0.4f, 4f), _cover);
            Block("HighPlatform_Pillar", new Vector3(8f, 1.8f, -6f), new Vector3(1f, 3.6f, 1f), _wall);
        }

        // ---------- Враг и подбираемое ----------

        private static void BuildCombatArea(PrefabContentBuilder.Result prefabs)
        {
            Sign("ТУРЕЛЬ: стреляет в ответ. Укрывайся за блоками", new Vector3(-12f, 4f, 30f));
            Place(prefabs.Turret, new Vector3(-12f, 0f, 32f), Quaternion.Euler(0f, 180f, 0f));
            Block("TurretCover_1", new Vector3(-14f, 0.6f, 24f), new Vector3(2.5f, 1.2f, 0.6f), _cover);
            Block("TurretCover_2", new Vector3(-9f, 0.6f, 26f), new Vector3(2.5f, 1.2f, 0.6f), _cover);

            Place(prefabs.HealthPickup, new Vector3(-3f, 0f, -14f), Quaternion.identity);
            Place(prefabs.AmmoPickup, new Vector3(3f, 0f, -14f), Quaternion.identity);
        }

        // ---------- Помощники ----------

        private static GameObject Block(string name, Vector3 position, Vector3 size, Material material)
        {
            var block = EditorAssetUtility.Solid(PrimitiveType.Cube, name, _level, position, size, material);
            block.isStatic = true;
            return block;
        }

        private static void Place(GameObject prefab, Vector3 position, Quaternion rotation)
        {
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            instance.transform.SetParent(_level, true);
            instance.transform.SetPositionAndRotation(position, rotation);
        }

        /// <summary>3D-подпись над объектом (TextMesh, не требует TextMeshPro).</summary>
        private static void Sign(string text, Vector3 position)
        {
            var go = new GameObject($"Sign: {text.Split(':')[0]}");
            go.transform.SetParent(_level, false);
            go.transform.SetPositionAndRotation(position, Quaternion.identity);

            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            var mesh = go.AddComponent<TextMesh>();
            mesh.text = text;
            mesh.font = font;
            mesh.fontSize = 48;
            mesh.characterSize = 0.06f;
            mesh.anchor = TextAnchor.MiddleCenter;
            mesh.alignment = TextAlignment.Center;
            mesh.color = Color.white;
            if (font != null) go.GetComponent<MeshRenderer>().sharedMaterial = font.material;
        }

        private static void AddToBuildSettings(string path)
        {
            var scenes = EditorBuildSettings.scenes.Where(s => s.path != path).ToList();
            scenes.Insert(0, new EditorBuildSettingsScene(path, true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }
    }
}
