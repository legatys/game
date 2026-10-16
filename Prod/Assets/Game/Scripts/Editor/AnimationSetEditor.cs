using System;
using System.IO;
using Game.Animations;
using UnityEditor;
using UnityEngine;

namespace Game.EditorTools
{
    /// <summary>Инспектор AnimationSet: подсказки по Mixamo, список подменённых слотов и кнопка пересборки контроллера.</summary>
    [CustomEditor(typeof(AnimationSet))]
    internal sealed class AnimationSetEditor : UnityEditor.Editor
    {
        /// <summary>Рисует инспектор: подсказку по Mixamo, поля набора, отчёт о заменах и кнопку пересборки.</summary>
        public override void OnInspectorGUI()
        {
            EditorGUILayout.HelpBox(
                "Как заменить анимацию:\n" +
                "1. Скачайте клип с mixamo.com (FBX for Unity, Without Skin, для движения — галочка In Place).\n" +
                "2. В импорте FBX: Rig → Animation Type = Humanoid; Animation → Loop Time для циклов.\n" +
                "3. Перетащите клип в нужный слот ниже и нажмите «Rebuild Animator Controller».",
                MessageType.Info);

            DrawDefaultInspector();

            var set = (AnimationSet)target;
            EditorGUILayout.Space();
            DrawFallbackReport(set);

            EditorGUILayout.Space();
            if (GUILayout.Button("Rebuild Animator Controller", GUILayout.Height(32))) Rebuild(set);
        }

        private static void DrawFallbackReport(AnimationSet set)
        {
            var report = string.Empty;
            foreach (AnimationSlot slot in Enum.GetValues(typeof(AnimationSlot)))
            {
                if (set.HasClip(slot)) continue;

                var clip = set.Resolve(slot, out var from);
                report += clip != null ? $"{slot} ← {from}\n" : $"{slot} — пусто\n";
            }

            if (report.Length > 0)
                EditorGUILayout.HelpBox("Пустые слоты и их замены:\n" + report.TrimEnd(), MessageType.None);
        }

        private static void Rebuild(AnimationSet set)
        {
            var setPath = AssetDatabase.GetAssetPath(set);
            if (setPath == SkeletonPaths.AnimationSet)
            {
                CoreAssetsBuilder.BuildAnimator(set); // заодно обновит ссылку в префабе игрока
                return;
            }

            // Для своих наборов контроллер кладём рядом: MySet.asset → MySet_Animator.controller.
            var directory = Path.GetDirectoryName(setPath)?.Replace('\\', '/');
            var controllerPath = set.controller != null
                ? AssetDatabase.GetAssetPath(set.controller)
                : $"{directory}/{Path.GetFileNameWithoutExtension(setPath)}_Animator.controller";

            var mask = AnimatorControllerBuilder.LoadOrCreateUpperBodyMask(SkeletonPaths.UpperBodyMask);
            new AnimatorControllerBuilder(set).Build(controllerPath, mask);
        }
    }
}
