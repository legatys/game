using Game.Animations;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace Game.EditorTools
{
    /// <summary>
    /// Паттерн Builder: собирает Animator Controller из <see cref="AnimationSet"/>.
    /// Base Layer — локомоция (2D blend tree), присед, подкат, прыжок/падение, смерть (+ IK Pass для взгляда).
    /// UpperBody — прицел, выстрел, перезарядка, реакция на попадание (маска верхней части тела).
    /// Меняете клипы в AnimationSet → жмёте Rebuild → контроллер пересобирается, код не трогается.
    /// </summary>
    internal sealed class AnimatorControllerBuilder
    {
        private readonly AnimationSet _set;
        private AnimatorController _controller;

        public AnimatorControllerBuilder(AnimationSet set) => _set = set;

        public AnimatorController Build(string path, AvatarMask upperBodyMask)
        {
            // Пересоздаём с нуля, а ссылку храним в AnimationSet.controller — она обновляется здесь же.
            if (EditorAssetUtility.Exists(path)) AssetDatabase.DeleteAsset(path);
            EditorAssetUtility.EnsureFolder(System.IO.Path.GetDirectoryName(path)?.Replace('\\', '/'));
            _controller = AnimatorController.CreateAnimatorControllerAtPath(path);

            AddParameters();
            BuildBaseLayer();
            BuildUpperBodyLayer(upperBodyMask);

            _set.controller = _controller;
            EditorUtility.SetDirty(_set);
            EditorUtility.SetDirty(_controller);
            AssetDatabase.SaveAssets();
            return _controller;
        }

        /// <summary>Маска "верх тела" для слоя UpperBody.</summary>
        public static AvatarMask LoadOrCreateUpperBodyMask(string path)
        {
            var mask = AssetDatabase.LoadAssetAtPath<AvatarMask>(path);
            if (mask != null) return mask;

            mask = new AvatarMask { name = "UpperBody" };
            for (var part = AvatarMaskBodyPart.Root; part < AvatarMaskBodyPart.LastBodyPart; part++)
                mask.SetHumanoidBodyPartActive(part, false);

            mask.SetHumanoidBodyPartActive(AvatarMaskBodyPart.Body, true);
            mask.SetHumanoidBodyPartActive(AvatarMaskBodyPart.Head, true);
            mask.SetHumanoidBodyPartActive(AvatarMaskBodyPart.LeftArm, true);
            mask.SetHumanoidBodyPartActive(AvatarMaskBodyPart.RightArm, true);
            mask.SetHumanoidBodyPartActive(AvatarMaskBodyPart.LeftFingers, true);
            mask.SetHumanoidBodyPartActive(AvatarMaskBodyPart.RightFingers, true);
            mask.SetHumanoidBodyPartActive(AvatarMaskBodyPart.LeftHandIK, true);
            mask.SetHumanoidBodyPartActive(AvatarMaskBodyPart.RightHandIK, true);

            EditorAssetUtility.EnsureFolder(System.IO.Path.GetDirectoryName(path)?.Replace('\\', '/'));
            AssetDatabase.CreateAsset(mask, path);
            return mask;
        }

        private void AddParameters()
        {
            foreach (var name in new[] { AnimatorParams.MoveX, AnimatorParams.MoveZ, AnimatorParams.Speed, AnimatorParams.VerticalSpeed })
                _controller.AddParameter(name, AnimatorControllerParameterType.Float);

            foreach (var name in new[] { AnimatorParams.IsGrounded, AnimatorParams.IsCrouching, AnimatorParams.IsSliding, AnimatorParams.IsAiming, AnimatorParams.IsDead })
                _controller.AddParameter(name, AnimatorControllerParameterType.Bool);

            foreach (var name in new[] { AnimatorParams.Jump, AnimatorParams.Fire, AnimatorParams.Reload, AnimatorParams.Hit })
                _controller.AddParameter(name, AnimatorControllerParameterType.Trigger);

            // IsGrounded по умолчанию true, чтобы персонаж не "падал" в первом кадре.
            var parameters = _controller.parameters;
            foreach (var parameter in parameters)
                if (parameter.name == AnimatorParams.IsGrounded)
                    parameter.defaultBool = true;
            _controller.parameters = parameters;
        }

        // ---------- Base Layer ----------

        private void BuildBaseLayer()
        {
            var layers = _controller.layers;
            layers[0].iKPass = true;
            _controller.layers = layers;

            var machine = _controller.layers[0].stateMachine;
            machine.entryPosition = new Vector3(-250f, 0f);
            machine.anyStatePosition = new Vector3(-250f, 200f);

            var locomotion = CreateLocomotionTree("Locomotion", new[]
            {
                (AnimationSlot.Idle, new Vector2(0f, 0f)),
                (AnimationSlot.WalkForward, new Vector2(0f, _set.walkSpeed)),
                (AnimationSlot.WalkBackward, new Vector2(0f, -_set.walkSpeed)),
                (AnimationSlot.WalkLeft, new Vector2(-_set.walkSpeed, 0f)),
                (AnimationSlot.WalkRight, new Vector2(_set.walkSpeed, 0f)),
                (AnimationSlot.RunForward, new Vector2(0f, _set.runSpeed)),
                (AnimationSlot.RunBackward, new Vector2(0f, -_set.runSpeed)),
                (AnimationSlot.RunLeft, new Vector2(-_set.runSpeed, 0f)),
                (AnimationSlot.RunRight, new Vector2(_set.runSpeed, 0f)),
                (AnimationSlot.Sprint, new Vector2(0f, _set.sprintSpeed)),
            });
            var crouch = CreateLocomotionTree("CrouchLocomotion", new[]
            {
                (AnimationSlot.CrouchIdle, new Vector2(0f, 0f)),
                (AnimationSlot.CrouchForward, new Vector2(0f, _set.crouchSpeed)),
                (AnimationSlot.CrouchBackward, new Vector2(0f, -_set.crouchSpeed)),
                (AnimationSlot.CrouchLeft, new Vector2(-_set.crouchSpeed, 0f)),
                (AnimationSlot.CrouchRight, new Vector2(_set.crouchSpeed, 0f)),
            });

            var slide = AddState(machine, "Slide", AnimationSlot.Slide, new Vector3(250f, 150f));
            var jump = AddState(machine, "Jump", AnimationSlot.JumpStart, new Vector3(500f, -100f));
            var fall = AddState(machine, "Fall", AnimationSlot.Fall, new Vector3(500f, 50f));
            var death = AddState(machine, "Death", AnimationSlot.Death, new Vector3(0f, 300f));

            Place(machine, locomotion, new Vector3(0f, 0f));
            Place(machine, crouch, new Vector3(0f, 150f));
            machine.defaultState = locomotion;

            const AnimatorConditionMode on = AnimatorConditionMode.If;
            const AnimatorConditionMode off = AnimatorConditionMode.IfNot;

            Link(locomotion, crouch, 0.15f, (on, AnimatorParams.IsCrouching));
            Link(crouch, locomotion, 0.15f, (off, AnimatorParams.IsCrouching));
            Link(locomotion, slide, 0.1f, (on, AnimatorParams.IsSliding));
            Link(crouch, slide, 0.1f, (on, AnimatorParams.IsSliding));
            Link(slide, crouch, 0.2f, (off, AnimatorParams.IsSliding), (on, AnimatorParams.IsCrouching));
            Link(slide, locomotion, 0.2f, (off, AnimatorParams.IsSliding), (off, AnimatorParams.IsCrouching));

            Link(locomotion, fall, 0.2f, (off, AnimatorParams.IsGrounded));
            Link(crouch, fall, 0.2f, (off, AnimatorParams.IsGrounded));
            Link(slide, fall, 0.2f, (off, AnimatorParams.IsGrounded));
            Link(jump, locomotion, 0.15f, (on, AnimatorParams.IsGrounded));
            LinkOnExit(jump, fall, 0.8f, 0.2f);
            Link(fall, locomotion, 0.15f, (on, AnimatorParams.IsGrounded));

            var toJump = machine.AddAnyStateTransition(jump);
            Configure(toJump, 0.1f, (AnimatorConditionMode.If, AnimatorParams.Jump), (off, AnimatorParams.IsDead));

            var toDeath = machine.AddAnyStateTransition(death);
            Configure(toDeath, 0.2f, (on, AnimatorParams.IsDead));
            Link(death, locomotion, 0.1f, (off, AnimatorParams.IsDead));
        }

        // ---------- UpperBody Layer ----------

        private void BuildUpperBodyLayer(AvatarMask mask)
        {
            _controller.AddLayer(AnimatorParams.UpperBodyLayer);
            var layers = _controller.layers;
            var index = layers.Length - 1;
            layers[index].avatarMask = mask;
            layers[index].defaultWeight = 0f; // вес выставляет CharacterAnimator
            layers[index].blendingMode = AnimatorLayerBlendingMode.Override;
            _controller.layers = layers;

            var machine = _controller.layers[index].stateMachine;
            var empty = machine.AddState("Empty", new Vector3(0f, 0f));
            var aim = AddState(machine, "Aim", AnimationSlot.AimIdle, new Vector3(250f, 0f));
            var fire = AddState(machine, "Fire", AnimationSlot.Fire, new Vector3(250f, 150f));
            var reload = AddState(machine, "Reload", AnimationSlot.Reload, new Vector3(500f, 0f));
            var hit = AddState(machine, "Hit", AnimationSlot.HitReact, new Vector3(500f, 150f));
            machine.defaultState = empty;

            Link(empty, aim, 0.15f, (AnimatorConditionMode.If, AnimatorParams.IsAiming));
            Link(aim, empty, 0.15f, (AnimatorConditionMode.IfNot, AnimatorParams.IsAiming));

            Configure(machine.AddAnyStateTransition(fire), 0.05f, (AnimatorConditionMode.If, AnimatorParams.Fire));
            Configure(machine.AddAnyStateTransition(reload), 0.1f, (AnimatorConditionMode.If, AnimatorParams.Reload));
            Configure(machine.AddAnyStateTransition(hit), 0.05f, (AnimatorConditionMode.If, AnimatorParams.Hit));

            LinkOnExit(fire, aim, 0.9f, 0.1f);
            LinkOnExit(reload, empty, 0.95f, 0.15f);
            LinkOnExit(hit, empty, 0.9f, 0.1f);
        }

        // ---------- Помощники ----------

        private AnimatorState CreateLocomotionTree(string name, (AnimationSlot slot, Vector2 position)[] points)
        {
            var state = _controller.CreateBlendTreeInController(name, out var tree, 0);
            tree.blendType = BlendTreeType.FreeformDirectional2D;
            tree.blendParameter = AnimatorParams.MoveX;
            tree.blendParameterY = AnimatorParams.MoveZ;

            foreach (var (slot, position) in points)
            {
                var clip = _set.Resolve(slot, out var resolvedFrom);
                if (clip == null) continue;

                tree.AddChild(clip, position);
                var children = tree.children;
                children[children.Length - 1].timeScale = FallbackTimeScale(slot, resolvedFrom);
                tree.children = children;
            }

            return state;
        }

        /// <summary>Если вместо ходьбы подставлен бег (или наоборот) — подгоняем скорость клипа под скорость движения.</summary>
        private float FallbackTimeScale(AnimationSlot wanted, AnimationSlot actual)
        {
            var wantedSpeed = NominalSpeed(wanted);
            var actualSpeed = NominalSpeed(actual);
            if (wanted == actual || wantedSpeed <= 0f || actualSpeed <= 0f) return 1f;
            return Mathf.Clamp(wantedSpeed / actualSpeed, 0.5f, 2f);
        }

        private float NominalSpeed(AnimationSlot slot)
        {
            switch (slot)
            {
                case AnimationSlot.WalkForward:
                case AnimationSlot.WalkBackward:
                case AnimationSlot.WalkLeft:
                case AnimationSlot.WalkRight: return _set.walkSpeed;
                case AnimationSlot.RunForward:
                case AnimationSlot.RunBackward:
                case AnimationSlot.RunLeft:
                case AnimationSlot.RunRight: return _set.runSpeed;
                case AnimationSlot.Sprint: return _set.sprintSpeed;
                case AnimationSlot.CrouchForward:
                case AnimationSlot.CrouchBackward:
                case AnimationSlot.CrouchLeft:
                case AnimationSlot.CrouchRight: return _set.crouchSpeed;
                default: return 0f;
            }
        }

        private AnimatorState AddState(AnimatorStateMachine machine, string name, AnimationSlot slot, Vector3 position)
        {
            var state = machine.AddState(name, position);
            state.motion = _set.Resolve(slot, out _);
            return state;
        }

        private static void Place(AnimatorStateMachine machine, AnimatorState state, Vector3 position)
        {
            var states = machine.states;
            for (var i = 0; i < states.Length; i++)
                if (states[i].state == state)
                    states[i].position = position;
            machine.states = states;
        }

        private static void Link(AnimatorState from, AnimatorState to, float duration, params (AnimatorConditionMode mode, string parameter)[] conditions)
        {
            Configure(from.AddTransition(to), duration, conditions);
        }

        private static void LinkOnExit(AnimatorState from, AnimatorState to, float exitTime, float duration)
        {
            var transition = from.AddTransition(to);
            transition.hasExitTime = true;
            transition.exitTime = exitTime;
            transition.duration = duration;
            transition.hasFixedDuration = true;
        }

        private static void Configure(AnimatorStateTransition transition, float duration, params (AnimatorConditionMode mode, string parameter)[] conditions)
        {
            transition.hasExitTime = false;
            transition.duration = duration;
            transition.hasFixedDuration = true;
            transition.canTransitionToSelf = false;
            foreach (var (mode, parameter) in conditions)
                transition.AddCondition(mode, 0f, parameter);
        }
    }
}
