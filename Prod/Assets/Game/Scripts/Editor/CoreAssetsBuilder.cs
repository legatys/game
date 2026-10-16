using System.Linq;
using Game.Animations;
using Game.Cameras;
using Game.Character;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace Game.EditorTools
{
    /// <summary>Конфиги, AnimationSet (с клипами Mixamo из Assets/Models), Avatar манекена и Animator Controller.</summary>
    internal static class CoreAssetsBuilder
    {
        public static MovementConfig LoadMovementConfig() => EditorAssetUtility.LoadOrCreate<MovementConfig>(SkeletonPaths.MovementConfig);
        public static CameraConfig LoadCameraConfig() => EditorAssetUtility.LoadOrCreate<CameraConfig>(SkeletonPaths.CameraConfig);

        /// <summary>Создаёт AnimationSet. При первом создании подставляет Idle и Jogging из исходного проекта.</summary>
        public static AnimationSet LoadAnimationSet(MovementConfig movement)
        {
            return EditorAssetUtility.LoadOrCreate<AnimationSet>(SkeletonPaths.AnimationSet, set =>
            {
                set.walkSpeed = movement.walkSpeed;
                set.runSpeed = movement.runSpeed;
                set.sprintSpeed = movement.sprintSpeed;
                set.crouchSpeed = movement.crouchSpeed;
                set.idle = FindClip("Idle_") ?? FindClip("Idle");
                set.runForward = FindClip("Jogging");
            });
        }

        public static AnimatorController BuildAnimator(AnimationSet set)
        {
            var mask = AnimatorControllerBuilder.LoadOrCreateUpperBodyMask(SkeletonPaths.UpperBodyMask);
            var controller = new AnimatorControllerBuilder(set).Build(SkeletonPaths.AnimatorController, mask);
            RelinkPlayerPrefab(controller);
            return controller;
        }

        /// <summary>После пересборки контроллера обновляем ссылку в префабе игрока (для превью в редакторе).</summary>
        private static void RelinkPlayerPrefab(RuntimeAnimatorController controller)
        {
            if (!EditorAssetUtility.Exists(SkeletonPaths.PlayerPrefab)) return;

            var root = PrefabUtility.LoadPrefabContents(SkeletonPaths.PlayerPrefab);
            foreach (var animator in root.GetComponentsInChildren<Animator>(true))
                animator.runtimeAnimatorController = controller;
            PrefabUtility.SaveAsPrefabAsset(root, SkeletonPaths.PlayerPrefab);
            PrefabUtility.UnloadPrefabContents(root);
        }

        /// <summary>Ищет AnimationClip внутри FBX в Assets/Models (например, Jogging.fbx).</summary>
        private static AnimationClip FindClip(string fbxName)
        {
            var path = $"{SkeletonPaths.MixamoModels}/{fbxName}.fbx";
            return AssetDatabase.LoadAllAssetsAtPath(path)
                .OfType<AnimationClip>()
                .FirstOrDefault(clip => !clip.name.StartsWith("__preview__"));
        }
    }
}
