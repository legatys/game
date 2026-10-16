using Game.Animations;
using Game.Cameras;
using Game.Character;
using Game.Combat.Weapons;
using UnityEngine;

namespace Game.Demo
{
    /// <summary>
    /// Сцена Playground "из коробки": собирает весь полигон прямо в игре через <see cref="DemoContentFactory"/>
    /// и <see cref="DemoLevelBuilder"/> — без генерации ассетов в редакторе. Composition Root сцены.
    /// В режиме редактирования строит несохраняемое превью уровня, чтобы сцена не выглядела пустой.
    /// Для настоящих префабов и своей сцены используйте Tools → TPS Skeleton → Build Demo Scene.
    /// </summary>
    [ExecuteAlways]
    [DefaultExecutionOrder(-500)]
    public sealed class PlaygroundBootstrap : MonoBehaviour
    {
        private const string PreviewName = "[Preview] Полигон (не сохраняется, в игре создаётся заново)";

        [Tooltip("Клипы Mixamo и Animator Controller игрока")]
        [SerializeField] private AnimationSet animationSet;
        [Tooltip("Пусто — значения по умолчанию")]
        [SerializeField] private MovementConfig movementConfig;
        [Tooltip("Пусто — значения по умолчанию")]
        [SerializeField] private CameraConfig cameraConfig;
        [SerializeField] private bool showPreviewInEditor = true;

        private GameObject _preview;
        private RuntimeMaterialProvider _previewMaterials;

        private void Awake()
        {
            if (!Application.isPlaying) return;

            DestroyPreview();
            BuildPlayable();
        }

        private void OnEnable()
        {
            if (!Application.isPlaying && showPreviewInEditor) BuildPreview();
        }

        private void OnDisable()
        {
            if (!Application.isPlaying) DestroyPreview();
        }

        private void OnValidate()
        {
            // Переключили галочку в инспекторе — пересобрать превью на следующем тике редактора.
            if (Application.isPlaying || !isActiveAndEnabled) return;
#if UNITY_EDITOR
            UnityEditor.EditorApplication.delayCall += () =>
            {
                if (this == null || !isActiveAndEnabled || Application.isPlaying) return;
                if (showPreviewInEditor == (_preview != null)) return; // уже в нужном состоянии
                DestroyPreview();
                if (showPreviewInEditor) BuildPreview();
            };
#endif
        }

        // ---------- Игра ----------

        private void BuildPlayable()
        {
            var materials = new RuntimeMaterialProvider();
            var movement = movementConfig != null ? movementConfig : ScriptableObject.CreateInstance<MovementConfig>();
            var camera = cameraConfig != null ? cameraConfig : ScriptableObject.CreateInstance<CameraConfig>();

            // Шаблоны лежат под неактивным объектом: их компоненты не запускаются, только копируются.
            var templates = Primitives.Inactive("[Templates]", transform).transform;
            var prefabs = BuildTemplates(materials, movement, templates, HideFlags.None);

            var level = new DemoLevelBuilder(materials, prefabs, Spawn);
            level.Build(null);

            var spawn = level.SpawnPoint;
            var player = Spawn(prefabs.Player, spawn.position, spawn.rotation, null);
            player.name = "Player";

            var rig = DemoLevelBuilder.SetupCamera(camera, spawn.position);
            rig.SetTarget(player.GetComponent<PlayerCharacter>());
            DemoLevelBuilder.BuildSystems(prefabs.Player.GetComponent<PlayerCharacter>(), spawn);
        }

        private DemoPrefabs BuildTemplates(IMaterialProvider materials, MovementConfig movement, Transform templates, HideFlags assetFlags)
        {
            var tracer = DemoContentFactory.BuildTracer(materials, templates);
            var impact = DemoContentFactory.BuildImpact(materials, templates);
            var explosion = DemoContentFactory.BuildExplosion(materials, templates);
            var grenade = DemoContentFactory.BuildGrenade(materials, explosion, templates);

            var weapons = DemoWeaponCatalog.CreateInMemory();
            foreach (var definition in weapons.All) definition.hideFlags = assetFlags;

            Avatar CreateAvatar(GameObject visual)
            {
                var avatar = MannequinBuilder.CreateAvatar(visual);
                avatar.hideFlags = assetFlags;
                return avatar;
            }

            DemoContentFactory.LinkWeapons(weapons,
                name => DemoContentFactory.BuildWeaponView(name, materials, templates).GetComponent<WeaponView>(),
                tracer, impact, grenade);

            return new DemoPrefabs
            {
                Player = DemoContentFactory.BuildPlayer(movement, animationSet, weapons.PlayerLoadout, materials, CreateAvatar, templates),
                Dummy = DemoContentFactory.BuildDummy(materials, templates),
                Turret = DemoContentFactory.BuildTurret(materials, weapons.TurretRifle, templates),
                HealthPickup = DemoContentFactory.BuildHealthPickup(materials, templates),
                AmmoPickup = DemoContentFactory.BuildAmmoPickup(materials, templates),
                JumpPad = DemoContentFactory.BuildJumpPad(materials, templates),
            };
        }

        private static GameObject Spawn(GameObject template, Vector3 position, Quaternion rotation, Transform parent)
        {
            var instance = Instantiate(template, position, rotation, parent);
            instance.name = template.name;
            instance.SetActive(true);
            return instance;
        }

        // ---------- Превью в редакторе ----------

        private void BuildPreview()
        {
            if (_preview != null || !gameObject.scene.IsValid()) return;

            _previewMaterials = new RuntimeMaterialProvider(HideFlags.DontSave);
            _preview = new GameObject(PreviewName);
            _preview.transform.SetParent(transform, false);

            // В превью только визуал: компоненты не работают в режиме редактирования.
            var templates = Primitives.Inactive("[Templates]", _preview.transform).transform;
            var prefabs = BuildTemplates(_previewMaterials, movementConfig, templates, HideFlags.DontSave);
            var level = new DemoLevelBuilder(_previewMaterials, prefabs, Spawn);
            level.Build(_preview.transform);
            Spawn(prefabs.Player, level.SpawnPoint.position, level.SpawnPoint.rotation, _preview.transform);

            foreach (var t in _preview.GetComponentsInChildren<Transform>(true))
                t.gameObject.hideFlags = HideFlags.DontSave | HideFlags.NotEditable;
        }

        private void DestroyPreview()
        {
            if (_preview == null)
            {
                // После перезагрузки скриптов ссылка теряется — ищем превью по имени.
                var stale = transform.Find(PreviewName);
                if (stale != null) _preview = stale.gameObject;
            }

            if (_preview != null) DestroyImmediate(_preview);
            _preview = null;

            _previewMaterials?.DestroyAll();
            _previewMaterials = null;
        }
    }
}
