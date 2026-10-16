using Game.AI;
using Game.Animations;
using Game.Character;
using Game.Combat;
using Game.Combat.Weapons;
using Game.Inputs;
using Game.Interaction;
using UnityEditor;
using UnityEngine;

namespace Game.EditorTools
{
    /// <summary>Префабы: игрок (манекен на скелете Mixamo), мишень, турель, аптечка, патроны, батут.</summary>
    internal static class PrefabContentBuilder
    {
        public sealed class Result
        {
            public GameObject Player, Dummy, Turret, HealthPickup, AmmoPickup, JumpPad;
        }

        public static Result Build(MovementConfig movement, AnimationSet animationSet, WeaponContentBuilder.Result weapons, bool overwrite)
        {
            return new Result
            {
                Player = WeaponContentBuilder.Prefab(SkeletonPaths.PlayerPrefab, overwrite, () => BuildPlayer(movement, animationSet, weapons.PlayerLoadout)),
                Dummy = WeaponContentBuilder.Prefab($"{SkeletonPaths.Level}/TargetDummy.prefab", overwrite, BuildDummy),
                Turret = WeaponContentBuilder.Prefab($"{SkeletonPaths.Level}/EnemyTurret.prefab", overwrite, () => BuildTurret(weapons.TurretRifle)),
                HealthPickup = WeaponContentBuilder.Prefab($"{SkeletonPaths.Level}/HealthPickup.prefab", overwrite,
                    () => BuildPickup<HealthPickup>("HealthPickup", MaterialLibrary.Emissive("HealthPickup", new Color(0.2f, 0.9f, 0.3f), 1.5f))),
                AmmoPickup = WeaponContentBuilder.Prefab($"{SkeletonPaths.Level}/AmmoPickup.prefab", overwrite,
                    () => BuildPickup<AmmoPickup>("AmmoPickup", MaterialLibrary.Emissive("AmmoPickup", new Color(1f, 0.8f, 0.2f), 1.5f))),
                JumpPad = WeaponContentBuilder.Prefab($"{SkeletonPaths.Level}/JumpPad.prefab", overwrite, BuildJumpPad),
            };
        }

        // ---------- Игрок ----------

        private static GameObject BuildPlayer(MovementConfig movement, AnimationSet animationSet, WeaponDefinition[] loadout)
        {
            var root = new GameObject("Player") { tag = "Player" };

            var controller = root.AddComponent<CharacterController>();
            controller.height = movement.standHeight;
            controller.radius = 0.3f;
            controller.center = new Vector3(0f, movement.standHeight * 0.5f, 0f);
            controller.stepOffset = 0.35f;
            controller.slopeLimit = 50f;
            controller.skinWidth = 0.05f;
            controller.minMoveDistance = 0f;

            // Порядок важен: RequireComponent у PlayerCombat/CharacterAnimator иначе добавит дубликаты.
            root.AddComponent<CharacterMotor>();
            var health = root.AddComponent<Health>();
            root.AddComponent<PlayerInputReader>();
            var inventory = root.AddComponent<WeaponInventory>();
            var player = root.AddComponent<PlayerCharacter>();
            root.AddComponent<PlayerCombat>();
            var characterAnimator = root.AddComponent<CharacterAnimator>();

            Serialized.Of(health).Set("maxHealth", 100f).Set("regenDelay", 4f).Set("regenPerSecond", 12f).Apply();
            Serialized.Of(player).Set("config", movement).Apply();

            // Модель: манекен из примитивов на скелете Mixamo + Humanoid Avatar.
            var mannequin = new MannequinBuilder().WithMaterials(
                MaterialLibrary.Lit("PlayerBody", new Color(0.25f, 0.45f, 0.85f)),
                MaterialLibrary.Lit("PlayerJoints", new Color(0.12f, 0.13f, 0.16f)),
                MaterialLibrary.Lit("PlayerAccent", new Color(1f, 0.75f, 0.2f)));
            var visual = mannequin.Build("Visual");
            visual.transform.SetParent(root.transform, false);

            var avatar = AssetDatabase.LoadAssetAtPath<Avatar>(SkeletonPaths.MannequinAvatar);
            if (avatar == null) avatar = MannequinBuilder.CreateAvatar(visual, SkeletonPaths.MannequinAvatar);

            var animator = visual.AddComponent<Animator>();
            animator.avatar = avatar;
            animator.runtimeAnimatorController = animationSet.controller;
            animator.applyRootMotion = false;
            animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            visual.AddComponent<AimIKHandler>();

            // Сокет оружия на груди справа — следует за анимацией корпуса.
            var socket = EditorAssetUtility.Child("WeaponSocket", mannequin.GetBone("Spine2"), new Vector3(0.2f, 0.05f, 0.25f));
            socket.gameObject.AddComponent<WeaponSocket>();

            Serialized.Of(inventory).Set("startingWeapons", loadout).Set("weaponSocket", socket).Apply();
            Serialized.Of(characterAnimator).Set("animator", animator).Set("animationSet", animationSet).Apply();
            return root;
        }

        // ---------- Мишень ----------

        private static GameObject BuildDummy()
        {
            var root = new GameObject("TargetDummy");
            var body = MaterialLibrary.Lit("Dummy", new Color(0.85f, 0.25f, 0.2f));
            var head = MaterialLibrary.Lit("DummyHead", new Color(0.95f, 0.85f, 0.75f));

            EditorAssetUtility.Visual(PrimitiveType.Cylinder, "Base", root.transform, new Vector3(0f, 0.05f, 0f), new Vector3(0.7f, 0.05f, 0.7f),
                MaterialLibrary.Lit("Wall", new Color(0.3f, 0.32f, 0.36f)));
            EditorAssetUtility.Solid(PrimitiveType.Capsule, "Body", root.transform, new Vector3(0f, 0.95f, 0f), new Vector3(0.6f, 0.75f, 0.6f), body);
            var headGo = EditorAssetUtility.Solid(PrimitiveType.Sphere, "Head", root.transform, new Vector3(0f, 1.9f, 0f), Vector3.one * 0.38f, head);

            var health = root.AddComponent<Health>();
            Serialized.Of(health).Set("maxHealth", 100f).Apply();
            root.AddComponent<TargetDummy>();

            var hitbox = headGo.AddComponent<Hitbox>();
            Serialized.Of(hitbox).Set("damageMultiplier", 2f).Set("isCritical", true).Apply();
            return root;
        }

        // ---------- Турель ----------

        private static GameObject BuildTurret(WeaponDefinition weapon)
        {
            var root = new GameObject("EnemyTurret");
            var metal = MaterialLibrary.Lit("Turret", new Color(0.25f, 0.27f, 0.3f), 0.5f);

            EditorAssetUtility.Solid(PrimitiveType.Cylinder, "Base", root.transform, new Vector3(0f, 0.5f, 0f), new Vector3(0.8f, 0.5f, 0.8f), metal);
            var yaw = EditorAssetUtility.Child("YawPivot", root.transform, new Vector3(0f, 1.25f, 0f));
            EditorAssetUtility.Solid(PrimitiveType.Cube, "Head", yaw, Vector3.zero, new Vector3(0.75f, 0.45f, 0.75f), metal);
            var pitch = EditorAssetUtility.Child("PitchPivot", yaw, new Vector3(0f, 0.05f, 0.2f));
            EditorAssetUtility.Visual(PrimitiveType.Cube, "Eye", pitch, new Vector3(0f, 0.08f, 0.18f), new Vector3(0.3f, 0.08f, 0.04f),
                MaterialLibrary.Emissive("TurretEye", new Color(1f, 0.1f, 0.1f), 3f));
            var socket = EditorAssetUtility.Child("WeaponSocket", pitch, new Vector3(0f, -0.05f, 0.2f));

            var health = root.AddComponent<Health>();
            Serialized.Of(health).Set("maxHealth", 150f).Apply();
            var turret = root.AddComponent<EnemyTurret>();
            Serialized.Of(turret).Set("yawPivot", yaw).Set("pitchPivot", pitch).Set("weaponSocket", socket).Set("weapon", weapon).Apply();
            return root;
        }

        // ---------- Подбираемое и батут ----------

        private static GameObject BuildPickup<T>(string name, Material material) where T : Pickup
        {
            var root = new GameObject(name);
            var trigger = root.AddComponent<BoxCollider>();
            trigger.isTrigger = true;
            trigger.size = new Vector3(1f, 1.2f, 1f);
            trigger.center = new Vector3(0f, 0.6f, 0f);

            var visual = EditorAssetUtility.Child("Visual", root.transform, new Vector3(0f, 0.6f, 0f));
            EditorAssetUtility.Visual(PrimitiveType.Cube, "Box", visual, Vector3.zero, Vector3.one * 0.4f, material);

            var pickup = root.AddComponent<T>();
            Serialized.Of(pickup).Set("visual", visual).Apply();
            return root;
        }

        private static GameObject BuildJumpPad()
        {
            var root = new GameObject("JumpPad");
            EditorAssetUtility.Visual(PrimitiveType.Cylinder, "Pad", root.transform, new Vector3(0f, 0.05f, 0f), new Vector3(1.8f, 0.05f, 1.8f),
                MaterialLibrary.Emissive("JumpPad", new Color(0.2f, 0.9f, 1f), 1.5f));

            var trigger = root.AddComponent<BoxCollider>();
            trigger.isTrigger = true;
            trigger.size = new Vector3(1.6f, 0.6f, 1.6f);
            trigger.center = new Vector3(0f, 0.35f, 0f);

            var pad = root.AddComponent<JumpPad>();
            Serialized.Of(pad).Set("launchVelocity", new Vector3(0f, 14f, 5f)).Apply();
            return root;
        }
    }
}
