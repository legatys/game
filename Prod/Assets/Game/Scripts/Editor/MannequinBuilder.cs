using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Game.EditorTools
{
    /// <summary>
    /// Паттерн Builder: строит "манекен" из примитивов на скелете с именами костей Mixamo
    /// (mixamorig:Hips, mixamorig:Spine...) и создаёт для него Humanoid Avatar.
    /// Благодаря Humanoid-ретаргетингу на манекене проигрывается ЛЮБАЯ Humanoid-анимация Mixamo,
    /// а сам манекен можно заменить на настоящую модель с тем же Animator Controller.
    /// </summary>
    internal sealed class MannequinBuilder
    {
        public const string BonePrefix = "mixamorig:";

        private readonly struct Bone
        {
            public readonly string Name;
            public readonly string Parent;
            public readonly Vector3 Position;
            public readonly string HumanName;

            public Bone(string name, string parent, Vector3 position, string humanName = null)
            {
                Name = name;
                Parent = parent;
                Position = position;
                HumanName = humanName;
            }
        }

        // T-поза, персонаж смотрит в +Z, левая сторона персонажа — это -X. Рост ≈ 1.8 м.
        private static readonly Bone[] Skeleton =
        {
            new Bone("Hips", null, new Vector3(0f, 1.0f, 0f), "Hips"),
            new Bone("Spine", "Hips", new Vector3(0f, 0.10f, 0f), "Spine"),
            new Bone("Spine1", "Spine", new Vector3(0f, 0.12f, 0f), "Chest"),
            new Bone("Spine2", "Spine1", new Vector3(0f, 0.12f, 0f), "UpperChest"),
            new Bone("Neck", "Spine2", new Vector3(0f, 0.16f, 0f), "Neck"),
            new Bone("Head", "Neck", new Vector3(0f, 0.08f, 0f), "Head"),
            new Bone("HeadTop_End", "Head", new Vector3(0f, 0.20f, 0f)),

            new Bone("LeftShoulder", "Spine2", new Vector3(-0.05f, 0.12f, 0f), "LeftShoulder"),
            new Bone("LeftArm", "LeftShoulder", new Vector3(-0.13f, 0f, 0f), "LeftUpperArm"),
            new Bone("LeftForeArm", "LeftArm", new Vector3(-0.27f, 0f, 0f), "LeftLowerArm"),
            new Bone("LeftHand", "LeftForeArm", new Vector3(-0.25f, 0f, 0f), "LeftHand"),
            new Bone("LeftHand_End", "LeftHand", new Vector3(-0.12f, 0f, 0f)),

            new Bone("RightShoulder", "Spine2", new Vector3(0.05f, 0.12f, 0f), "RightShoulder"),
            new Bone("RightArm", "RightShoulder", new Vector3(0.13f, 0f, 0f), "RightUpperArm"),
            new Bone("RightForeArm", "RightArm", new Vector3(0.27f, 0f, 0f), "RightLowerArm"),
            new Bone("RightHand", "RightForeArm", new Vector3(0.25f, 0f, 0f), "RightHand"),
            new Bone("RightHand_End", "RightHand", new Vector3(0.12f, 0f, 0f)),

            new Bone("LeftUpLeg", "Hips", new Vector3(-0.09f, -0.05f, 0f), "LeftUpperLeg"),
            new Bone("LeftLeg", "LeftUpLeg", new Vector3(0f, -0.43f, 0f), "LeftLowerLeg"),
            new Bone("LeftFoot", "LeftLeg", new Vector3(0f, -0.42f, 0f), "LeftFoot"),
            new Bone("LeftToeBase", "LeftFoot", new Vector3(0f, -0.07f, 0.12f), "LeftToes"),
            new Bone("LeftToe_End", "LeftToeBase", new Vector3(0f, 0f, 0.07f)),

            new Bone("RightUpLeg", "Hips", new Vector3(0.09f, -0.05f, 0f), "RightUpperLeg"),
            new Bone("RightLeg", "RightUpLeg", new Vector3(0f, -0.43f, 0f), "RightLowerLeg"),
            new Bone("RightFoot", "RightLeg", new Vector3(0f, -0.42f, 0f), "RightFoot"),
            new Bone("RightToeBase", "RightFoot", new Vector3(0f, -0.07f, 0.12f), "RightToes"),
            new Bone("RightToe_End", "RightToeBase", new Vector3(0f, 0f, 0.07f)),
        };

        private Material _body;
        private Material _joints;
        private Material _accent;
        private Dictionary<string, Transform> _bones;

        public MannequinBuilder WithMaterials(Material body, Material joints, Material accent)
        {
            _body = body;
            _joints = joints;
            _accent = accent;
            return this;
        }

        /// <summary>Создаёт объект-модель (корень + скелет + меши). Animator вешается снаружи.</summary>
        public GameObject Build(string name)
        {
            var root = new GameObject(name);
            _bones = new Dictionary<string, Transform>();

            foreach (var bone in Skeleton)
            {
                var parent = bone.Parent == null ? root.transform : _bones[bone.Parent];
                var transform = new GameObject(BonePrefix + bone.Name).transform;
                transform.SetParent(parent, false);
                transform.localPosition = bone.Position;
                _bones[bone.Name] = transform;
            }

            BuildGeometry();
            return root;
        }

        public Transform GetBone(string shortName) => _bones != null && _bones.TryGetValue(shortName, out var bone) ? bone : null;

        /// <summary>Humanoid Avatar по скелету манекена. Сохраняется как ассет.</summary>
        public static Avatar CreateAvatar(GameObject root, string assetPath)
        {
            var skeleton = root.GetComponentsInChildren<Transform>(true)
                .Select(t => new SkeletonBone
                {
                    name = t.name,
                    position = t.localPosition,
                    rotation = t.localRotation,
                    scale = t.localScale
                })
                .ToArray();

            var human = Skeleton
                .Where(b => b.HumanName != null)
                .Select(b => new HumanBone
                {
                    boneName = BonePrefix + b.Name,
                    humanName = b.HumanName,
                    limit = new HumanLimit { useDefaultValues = true }
                })
                .ToArray();

            var description = new HumanDescription
            {
                human = human,
                skeleton = skeleton,
                upperArmTwist = 0.5f,
                lowerArmTwist = 0.5f,
                upperLegTwist = 0.5f,
                lowerLegTwist = 0.5f,
                armStretch = 0.05f,
                legStretch = 0.05f,
                feetSpacing = 0f,
                hasTranslationDoF = false
            };

            var avatar = AvatarBuilder.BuildHumanAvatar(root, description);
            avatar.name = "MannequinAvatar";
            if (!avatar.isValid || !avatar.isHuman)
                Debug.LogError("[TPS Skeleton] Не удалось построить Humanoid Avatar для манекена.");

            EditorAssetUtility.EnsureFolder(System.IO.Path.GetDirectoryName(assetPath)?.Replace('\\', '/'));
            AssetDatabase.CreateAsset(avatar, assetPath);
            return avatar;
        }

        // ---------- Геометрия из примитивов ----------

        private void BuildGeometry()
        {
            Box("Hips", new Vector3(0f, 0f, 0f), new Vector3(0.30f, 0.16f, 0.19f), _body);
            Box("Spine", new Vector3(0f, 0.06f, 0f), new Vector3(0.28f, 0.16f, 0.17f), _body);
            Box("Spine1", new Vector3(0f, 0.10f, 0f), new Vector3(0.38f, 0.30f, 0.22f), _body);
            Box("Spine2", new Vector3(0f, 0.02f, -0.14f), new Vector3(0.26f, 0.30f, 0.10f), _accent); // рюкзак — видно, где спина
            Limb("Neck", "Head", 0.08f, _joints);
            Sphere("Head", new Vector3(0f, 0.10f, 0f), 0.24f, _body);
            Box("Head", new Vector3(0f, 0.11f, 0.10f), new Vector3(0.17f, 0.06f, 0.06f), _accent); // визор — видно, где лицо

            foreach (var side in new[] { "Left", "Right" })
            {
                var sign = side == "Left" ? -1f : 1f;
                Sphere(side + "Arm", Vector3.zero, 0.11f, _joints);
                Limb(side + "Arm", side + "ForeArm", 0.10f, _body);
                Limb(side + "ForeArm", side + "Hand", 0.085f, _body);
                Sphere(side + "Hand", new Vector3(0.05f * sign, 0f, 0f), 0.10f, _joints);

                Limb(side + "UpLeg", side + "Leg", 0.14f, _body);
                Sphere(side + "Leg", Vector3.zero, 0.12f, _joints);
                Limb(side + "Leg", side + "Foot", 0.115f, _body);
                Box(side + "Foot", new Vector3(0f, -0.05f, 0.06f), new Vector3(0.10f, 0.08f, 0.24f), _accent);
            }
        }

        private void Box(string bone, Vector3 position, Vector3 size, Material material) =>
            EditorAssetUtility.Visual(PrimitiveType.Cube, $"Geo_{bone}_Box", _bones[bone], position, size, material);

        private void Sphere(string bone, Vector3 position, float diameter, Material material) =>
            EditorAssetUtility.Visual(PrimitiveType.Sphere, $"Geo_{bone}_Sphere", _bones[bone], position, Vector3.one * diameter, material);

        /// <summary>Капсула от кости from до дочерней кости to.</summary>
        private void Limb(string from, string to, float thickness, Material material)
        {
            var offset = _bones[to].localPosition;
            var geo = EditorAssetUtility.Visual(PrimitiveType.Capsule, $"Geo_{from}_Limb", _bones[from], offset * 0.5f,
                new Vector3(thickness, offset.magnitude * 0.5f, thickness), material);
            geo.transform.localRotation = Quaternion.FromToRotation(Vector3.up, offset.normalized);
        }
    }
}
