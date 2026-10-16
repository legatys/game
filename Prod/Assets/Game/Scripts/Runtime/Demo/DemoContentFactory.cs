using System;
using Game.AI;
using Game.Animations;
using Game.Character;
using Game.Combat;
using Game.Combat.Weapons;
using Game.Inputs;
using Game.Interaction;
using UnityEngine;

namespace Game.Demo
{
    /// <summary>Шаблоны (префабы или неактивные объекты-образцы) для расстановки по уровню.</summary>
    public sealed class DemoPrefabs
    {
        public GameObject Player, Dummy, Turret, HealthPickup, AmmoPickup, JumpPad;
    }

    /// <summary>
    /// Паттерн Factory: собирает игровые объекты демо из примитивов — игрока на скелете Mixamo, мишень,
    /// турель, предметы, модели оружия и эффекты. Один код для двух потребителей:
    /// генератор в редакторе сохраняет результат в префабы, сцена Playground создаёт их прямо в игре.
    /// Объекты создаются внутри parent; если parent неактивен — Awake компонентов ждёт активации,
    /// поэтому все Configure() успевают выполниться до него.
    /// </summary>
    public static class DemoContentFactory
    {
        // ---------- Эффекты ----------

        public static GameObject BuildTracer(IMaterialProvider materials, Transform parent = null)
        {
            var root = Root("Tracer", parent);
            var line = root.AddComponent<LineRenderer>();
            line.sharedMaterial = materials.Unlit("Tracer", new Color(1f, 0.85f, 0.4f));
            line.positionCount = 2;
            line.useWorldSpace = true;
            line.numCapVertices = 2;
            line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            line.receiveShadows = false;
            root.AddComponent<Tracer>();
            return root;
        }

        public static GameObject BuildImpact(IMaterialProvider materials, Transform parent = null)
        {
            var root = Root("Impact", parent);
            Primitives.Visual(PrimitiveType.Cube, "Spark", root.transform, Vector3.zero, Vector3.one * 0.08f,
                materials.Emissive("Impact", new Color(1f, 0.6f, 0.2f), 3f));
            root.AddComponent<PooledEffect>().Configure(0.35f, 1f, 0f);
            return root;
        }

        public static GameObject BuildExplosion(IMaterialProvider materials, Transform parent = null)
        {
            var root = Root("Explosion", parent);
            Primitives.Visual(PrimitiveType.Sphere, "Fireball", root.transform, Vector3.zero, Vector3.one,
                materials.Emissive("Explosion", new Color(1f, 0.45f, 0.1f), 3f));
            var light = root.AddComponent<Light>();
            light.type = LightType.Point;
            light.range = 10f;
            light.intensity = 6f;
            light.color = new Color(1f, 0.6f, 0.3f);
            root.AddComponent<PooledEffect>().Configure(0.3f, 0.5f, 6f, light);
            return root;
        }

        public static GameObject BuildGrenade(IMaterialProvider materials, GameObject explosion, Transform parent = null)
        {
            var root = Root("Grenade", parent);
            Primitives.Visual(PrimitiveType.Sphere, "Body", root.transform, Vector3.zero, Vector3.one * 0.15f,
                materials.Lit("Grenade", new Color(0.2f, 0.3f, 0.15f)));

            var trail = root.AddComponent<TrailRenderer>();
            trail.sharedMaterial = materials.Unlit("Tracer", new Color(1f, 0.85f, 0.4f));
            trail.time = 0.25f;
            trail.startWidth = 0.08f;
            trail.endWidth = 0f;
            trail.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

            root.AddComponent<Projectile>().Configure(explosion);
            return root;
        }

        // ---------- Оружие ----------

        /// <summary>Модель оружия из примитивов: корпус, рукоять, ствол, дуло со вспышкой и светом.</summary>
        public static GameObject BuildWeaponView(string weapon, IMaterialProvider materials, Transform parent = null)
        {
            var (bodySize, barrelRadius, barrelLength) = DemoWeaponCatalog.ViewShape(weapon);
            var root = Root($"WeaponView_{weapon}", parent);
            var metal = materials.Lit("Weapon", new Color(0.18f, 0.19f, 0.21f), 0.6f);

            Primitives.Visual(PrimitiveType.Cube, "Body", root.transform, new Vector3(0f, 0f, bodySize.z * 0.5f - 0.05f), bodySize, metal);
            Primitives.Visual(PrimitiveType.Cube, "Grip", root.transform, new Vector3(0f, -bodySize.y * 0.8f, 0.02f),
                new Vector3(bodySize.x * 0.8f, bodySize.y, 0.05f), metal).transform.localRotation = Quaternion.Euler(15f, 0f, 0f);

            var barrelStart = bodySize.z - 0.05f;
            var barrel = Primitives.Visual(PrimitiveType.Cylinder, "Barrel", root.transform,
                new Vector3(0f, bodySize.y * 0.15f, barrelStart + barrelLength * 0.5f),
                new Vector3(barrelRadius * 2f, barrelLength * 0.5f, barrelRadius * 2f), metal);
            barrel.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);

            var muzzle = Primitives.Child("Muzzle", root.transform, new Vector3(0f, bodySize.y * 0.15f, barrelStart + barrelLength + 0.02f));
            var flash = Primitives.Visual(PrimitiveType.Sphere, "MuzzleFlash", muzzle, Vector3.zero, Vector3.one * 0.12f,
                materials.Emissive("MuzzleFlash", new Color(1f, 0.8f, 0.3f), 4f));
            var light = flash.AddComponent<Light>();
            light.type = LightType.Point;
            light.range = 4f;
            light.intensity = 3f;
            light.color = new Color(1f, 0.75f, 0.4f);
            flash.SetActive(false);

            root.AddComponent<WeaponView>().Configure(muzzle, flash);
            return root;
        }

        /// <summary>Связать определения оружия с моделями и эффектами.</summary>
        public static void LinkWeapons(DemoWeapons weapons, Func<string, WeaponView> view, GameObject tracer, GameObject impact, GameObject grenade)
        {
            void Link(WeaponDefinition definition, string viewName, GameObject tracerPrefab)
            {
                definition.viewPrefab = view(viewName);
                definition.tracerPrefab = tracerPrefab;
                definition.impactPrefab = impact;
            }

            Link(weapons.Pistol, DemoWeaponCatalog.Pistol, tracer);
            Link(weapons.Rifle, DemoWeaponCatalog.Rifle, tracer);
            Link(weapons.Shotgun, DemoWeaponCatalog.Shotgun, tracer);
            Link(weapons.Launcher, DemoWeaponCatalog.Launcher, null);
            Link(weapons.TurretRifle, DemoWeaponCatalog.Rifle, tracer);
            weapons.Launcher.projectilePrefab = grenade.GetComponent<Projectile>();
        }

        // ---------- Игрок ----------

        /// <param name="avatarProvider">Как получить Avatar для модели (в редакторе — ассет, в игре — построить на лету).</param>
        public static GameObject BuildPlayer(MovementConfig movement, AnimationSet animationSet, WeaponDefinition[] loadout,
            IMaterialProvider materials, Func<GameObject, Avatar> avatarProvider, Transform parent = null)
        {
            var root = Root("Player", parent);
            root.tag = "Player";

            var standHeight = movement != null ? movement.standHeight : 1.8f;
            var controller = root.AddComponent<CharacterController>();
            controller.height = standHeight;
            controller.radius = 0.3f;
            controller.center = new Vector3(0f, standHeight * 0.5f, 0f);
            controller.stepOffset = 0.35f;
            controller.slopeLimit = 50f;
            controller.skinWidth = 0.05f;
            controller.minMoveDistance = 0f;

            // Порядок важен: RequireComponent у PlayerCombat/CharacterAnimator иначе добавит дубликаты.
            root.AddComponent<CharacterMotor>();
            root.AddComponent<Health>().Configure(100f, 4f, 12f);
            root.AddComponent<PlayerInputReader>();
            var inventory = root.AddComponent<WeaponInventory>();
            root.AddComponent<PlayerCharacter>().Configure(movement);
            root.AddComponent<PlayerCombat>();
            var characterAnimator = root.AddComponent<CharacterAnimator>();

            // Модель: манекен из примитивов на скелете Mixamo + Humanoid Avatar.
            var mannequin = new MannequinBuilder().WithMaterials(
                materials.Lit("PlayerBody", new Color(0.25f, 0.45f, 0.85f)),
                materials.Lit("PlayerJoints", new Color(0.12f, 0.13f, 0.16f)),
                materials.Lit("PlayerAccent", new Color(1f, 0.75f, 0.2f)));
            var visual = mannequin.Build("Visual");
            visual.transform.SetParent(root.transform, false);

            var animator = visual.AddComponent<Animator>();
            animator.avatar = avatarProvider(visual);
            animator.runtimeAnimatorController = animationSet != null ? animationSet.controller : null;
            animator.applyRootMotion = false;
            animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            visual.AddComponent<AimIKHandler>();

            // Сокет оружия на груди справа — следует за анимацией корпуса.
            var socket = Primitives.Child("WeaponSocket", mannequin.GetBone("Spine2"), new Vector3(0.2f, 0.05f, 0.25f));
            socket.gameObject.AddComponent<WeaponSocket>();

            inventory.Configure(loadout, socket);
            characterAnimator.Configure(animator, animationSet);
            return root;
        }

        // ---------- Мишень и турель ----------

        public static GameObject BuildDummy(IMaterialProvider materials, Transform parent = null)
        {
            var root = Root("TargetDummy", parent);
            Primitives.Visual(PrimitiveType.Cylinder, "Base", root.transform, new Vector3(0f, 0.05f, 0f), new Vector3(0.7f, 0.05f, 0.7f),
                materials.Lit("Wall", new Color(0.3f, 0.32f, 0.36f)));
            Primitives.Solid(PrimitiveType.Capsule, "Body", root.transform, new Vector3(0f, 0.95f, 0f), new Vector3(0.6f, 0.75f, 0.6f),
                materials.Lit("Dummy", new Color(0.85f, 0.25f, 0.2f)));
            var head = Primitives.Solid(PrimitiveType.Sphere, "Head", root.transform, new Vector3(0f, 1.9f, 0f), Vector3.one * 0.38f,
                materials.Lit("DummyHead", new Color(0.95f, 0.85f, 0.75f)));

            root.AddComponent<Health>().Configure(100f);
            root.AddComponent<TargetDummy>();
            head.AddComponent<Hitbox>().Configure(2f, true);
            return root;
        }

        public static GameObject BuildTurret(IMaterialProvider materials, WeaponDefinition weapon, Transform parent = null)
        {
            var root = Root("EnemyTurret", parent);
            var metal = materials.Lit("Turret", new Color(0.25f, 0.27f, 0.3f), 0.5f);

            Primitives.Solid(PrimitiveType.Cylinder, "Base", root.transform, new Vector3(0f, 0.5f, 0f), new Vector3(0.8f, 0.5f, 0.8f), metal);
            var yaw = Primitives.Child("YawPivot", root.transform, new Vector3(0f, 1.25f, 0f));
            Primitives.Solid(PrimitiveType.Cube, "Head", yaw, Vector3.zero, new Vector3(0.75f, 0.45f, 0.75f), metal);
            var pitch = Primitives.Child("PitchPivot", yaw, new Vector3(0f, 0.05f, 0.2f));
            Primitives.Visual(PrimitiveType.Cube, "Eye", pitch, new Vector3(0f, 0.08f, 0.18f), new Vector3(0.3f, 0.08f, 0.04f),
                materials.Emissive("TurretEye", new Color(1f, 0.1f, 0.1f), 3f));
            var socket = Primitives.Child("WeaponSocket", pitch, new Vector3(0f, -0.05f, 0.2f));

            root.AddComponent<Health>().Configure(150f);
            root.AddComponent<EnemyTurret>().Configure(yaw, pitch, socket, weapon);
            return root;
        }

        // ---------- Предметы и батут ----------

        public static GameObject BuildHealthPickup(IMaterialProvider materials, Transform parent = null) =>
            BuildPickup<HealthPickup>("HealthPickup", materials.Emissive("HealthPickup", new Color(0.2f, 0.9f, 0.3f), 1.5f), parent);

        public static GameObject BuildAmmoPickup(IMaterialProvider materials, Transform parent = null) =>
            BuildPickup<AmmoPickup>("AmmoPickup", materials.Emissive("AmmoPickup", new Color(1f, 0.8f, 0.2f), 1.5f), parent);

        public static GameObject BuildJumpPad(IMaterialProvider materials, Transform parent = null)
        {
            var root = Root("JumpPad", parent);
            Primitives.Visual(PrimitiveType.Cylinder, "Pad", root.transform, new Vector3(0f, 0.05f, 0f), new Vector3(1.8f, 0.05f, 1.8f),
                materials.Emissive("JumpPad", new Color(0.2f, 0.9f, 1f), 1.5f));

            var trigger = root.AddComponent<BoxCollider>();
            trigger.isTrigger = true;
            trigger.size = new Vector3(1.6f, 0.6f, 1.6f);
            trigger.center = new Vector3(0f, 0.35f, 0f);

            root.AddComponent<JumpPad>().Configure(new Vector3(0f, 14f, 5f));
            return root;
        }

        private static GameObject BuildPickup<T>(string name, Material material, Transform parent) where T : Pickup
        {
            var root = Root(name, parent);
            var trigger = root.AddComponent<BoxCollider>();
            trigger.isTrigger = true;
            trigger.size = new Vector3(1f, 1.2f, 1f);
            trigger.center = new Vector3(0f, 0.6f, 0f);

            var visual = Primitives.Child("Visual", root.transform, new Vector3(0f, 0.6f, 0f));
            Primitives.Visual(PrimitiveType.Cube, "Box", visual, Vector3.zero, Vector3.one * 0.4f, material);

            root.AddComponent<T>().Configure(visual);
            return root;
        }

        private static GameObject Root(string name, Transform parent)
        {
            var root = new GameObject(name);
            if (parent != null) root.transform.SetParent(parent, false);
            return root;
        }
    }
}
