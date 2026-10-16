using System.Collections.Generic;
using Game.Character;
using Game.Combat;
using Game.Combat.Weapons;
using UnityEngine;

namespace Game.Animations
{
    /// <summary>
    /// Паттерн Adapter: переводит состояние геймплея (скорость, присед, прицел, события выстрела/урона)
    /// в параметры Animator. Геймплей ничего не знает об Animator — анимации можно менять независимо.
    /// Animator ищется в дочерних объектах (модель персонажа), поэтому модель легко заменить на любую
    /// Humanoid-модель Mixamo, оставив этот компонент на корне.
    /// </summary>
    [DefaultExecutionOrder(50)]
    [RequireComponent(typeof(PlayerCharacter))]
    public sealed class CharacterAnimator : MonoBehaviour
    {
        /// <summary>Animator модели. Пусто — ищется в дочерних объектах.</summary>
        [SerializeField] private Animator animator;
        /// <summary>Набор анимаций; его контроллер подставляется в Animator.</summary>
        [SerializeField] private AnimationSet animationSet;
        /// <summary>Сглаживание параметров движения, сек.</summary>
        [SerializeField] private float locomotionDamping = 0.1f;
        /// <summary>Скорость изменения веса слоя UpperBody в секунду.</summary>
        [SerializeField] private float upperBodyBlendSpeed = 8f;
        [Tooltip("Сколько держать слой верха тела после выстрела/попадания")]
        [SerializeField] private float actionHoldTime = 0.35f;
        [Tooltip("Короткие отрывы от земли (ступеньки) не считаются падением")]
        [SerializeField] private float groundedGraceTime = 0.15f;
        [Header("Запасные процедурные анимации (если клипа нет в AnimationSet)")]
        [Tooltip("Нет клипа приседа — сплющивать модель по высоте капсулы, чтобы присед был виден")]
        [SerializeField] private bool squashVisualWithoutCrouchClip = true;
        [Tooltip("Нет клипа смерти — модель падает на спину")]
        [SerializeField] private bool tipOverWithoutDeathClip = true;

        private readonly HashSet<int> _parameters = new HashSet<int>();
        private PlayerCharacter _player;
        private PlayerCombat _combat;
        private Health _health;
        private int _upperBodyLayer = -1;
        private float _upperBodyWeight;
        private float _actionTimer;
        private Vector3 _visualBaseScale = Vector3.one;
        private Quaternion _visualBaseRotation = Quaternion.identity;

        /// <summary>Используемый Animator.</summary>
        public Animator Animator => animator;

        /// <summary>Настройка из кода. Вызывать до активации объекта (до Awake).</summary>
        public void Configure(Animator targetAnimator, AnimationSet set)
        {
            animator = targetAnimator;
            animationSet = set;
        }

        private void Awake()
        {
            // Берём компоненты напрямую: порядок Awake между компонентами не гарантирован.
            _player = GetComponent<PlayerCharacter>();
            _combat = GetComponent<PlayerCombat>();
            _health = GetComponent<Health>();
            if (animator == null) animator = GetComponentInChildren<Animator>();
            if (animator == null) return;

            if (animationSet != null && animationSet.controller != null)
                animator.runtimeAnimatorController = animationSet.controller;

            animator.applyRootMotion = false;
            _visualBaseScale = animator.transform.localScale;
            _visualBaseRotation = animator.transform.localRotation;
            CacheParameters();
        }

        private void OnEnable()
        {
            _player.Jumped += OnJumped;
            _player.Respawned += OnRespawned;
            _health.Damaged += OnDamaged;
            if (_combat != null)
            {
                _combat.WeaponFired += OnWeaponFired;
                _combat.ReloadStarted += OnReloadStarted;
            }
        }

        private void OnDisable()
        {
            _player.Jumped -= OnJumped;
            _player.Respawned -= OnRespawned;
            _health.Damaged -= OnDamaged;
            if (_combat != null)
            {
                _combat.WeaponFired -= OnWeaponFired;
                _combat.ReloadStarted -= OnReloadStarted;
            }
        }

        private void Update()
        {
            if (animator == null || animator.runtimeAnimatorController == null) return;

            var deltaTime = Time.deltaTime;
            var motor = _player.Motor;
            var local = transform.InverseTransformDirection(motor.ActualVelocity);

            SetFloat(AnimatorParams.MoveXHash, local.x, locomotionDamping, deltaTime);
            SetFloat(AnimatorParams.MoveZHash, local.z, locomotionDamping, deltaTime);
            SetFloat(AnimatorParams.SpeedHash, new Vector2(local.x, local.z).magnitude, locomotionDamping, deltaTime);
            SetFloat(AnimatorParams.VerticalSpeedHash, motor.Velocity.y, 0f, deltaTime);

            // Короткий отрыв при спуске (ступенька) — ещё "на земле"; после прыжка (vy > 0) — уже нет.
            var grounded = motor.IsGrounded || (motor.AirTime < groundedGraceTime && motor.Velocity.y <= 0f);
            SetBool(AnimatorParams.IsGroundedHash, grounded);
            SetBool(AnimatorParams.IsCrouchingHash, _player.IsCrouching);
            SetBool(AnimatorParams.IsSlidingHash, _player.IsSliding);
            SetBool(AnimatorParams.IsAimingHash, _player.WantsCombatFacing); // оружие поднято: прицел или стрельба от бедра
            SetBool(AnimatorParams.IsDeadHash, _player.IsDead);

            UpdateUpperBodyLayer(deltaTime);
            UpdateCrouchFallback();
            UpdateDeathFallback(deltaTime);
        }

        private void UpdateUpperBodyLayer(float deltaTime)
        {
            if (_upperBodyLayer < 0) return;

            _actionTimer -= deltaTime;
            var weapon = _combat != null ? _combat.CurrentWeapon : null;
            var reloading = weapon != null && weapon.IsReloading;

            var wanted = !_player.IsDead && (
                (_player.WantsCombatFacing && Has(AnimationSlot.AimIdle)) ||
                (reloading && Has(AnimationSlot.Reload)) ||
                _actionTimer > 0f);

            _upperBodyWeight = Mathf.MoveTowards(_upperBodyWeight, wanted ? 1f : 0f, upperBodyBlendSpeed * deltaTime);
            animator.SetLayerWeight(_upperBodyLayer, _upperBodyWeight);
        }

        private void UpdateCrouchFallback()
        {
            if (!squashVisualWithoutCrouchClip || animationSet == null || Has(AnimationSlot.CrouchIdle)) return;

            var ratio = _player.Motor.Height / Mathf.Max(0.01f, _player.Config.standHeight);
            animator.transform.localScale = new Vector3(_visualBaseScale.x, _visualBaseScale.y * ratio, _visualBaseScale.z);
        }

        private void UpdateDeathFallback(float deltaTime)
        {
            if (!tipOverWithoutDeathClip || animationSet == null || Has(AnimationSlot.Death)) return;

            var target = _player.IsDead ? _visualBaseRotation * Quaternion.Euler(-85f, 0f, 0f) : _visualBaseRotation;
            animator.transform.localRotation = Quaternion.RotateTowards(animator.transform.localRotation, target, 240f * deltaTime);
        }

        // ---------- События геймплея → триггеры ----------

        private void OnJumped() => SetTrigger(AnimatorParams.JumpHash);

        private void OnWeaponFired(Weapon weapon)
        {
            SetTrigger(AnimatorParams.FireHash);
            if (Has(AnimationSlot.Fire)) _actionTimer = actionHoldTime;
        }

        private void OnReloadStarted(Weapon weapon) => SetTrigger(AnimatorParams.ReloadHash);

        private void OnDamaged(DamageInfo info)
        {
            if (!_health.IsAlive) return;
            SetTrigger(AnimatorParams.HitHash);
            if (Has(AnimationSlot.HitReact)) _actionTimer = actionHoldTime;
        }

        private void OnRespawned()
        {
            if (animator == null) return;
            animator.transform.localRotation = _visualBaseRotation;
            animator.Rebind();
            animator.Update(0f);
        }

        // ---------- Безопасные обёртки: нет параметра в контроллере — нет ошибки ----------

        private void CacheParameters()
        {
            _parameters.Clear();
            if (animator.runtimeAnimatorController == null) return;

            foreach (var parameter in animator.parameters) _parameters.Add(parameter.nameHash);
            _upperBodyLayer = animator.GetLayerIndex(AnimatorParams.UpperBodyLayer);
        }

        // Свой контроллер без AnimationSet — считаем, что все клипы есть.
        private bool Has(AnimationSlot slot) => animationSet == null || animationSet.HasClip(slot);

        private void SetFloat(int hash, float value, float damping, float deltaTime)
        {
            if (_parameters.Contains(hash)) animator.SetFloat(hash, value, damping, deltaTime);
        }

        private void SetBool(int hash, bool value)
        {
            if (_parameters.Contains(hash)) animator.SetBool(hash, value);
        }

        private void SetTrigger(int hash)
        {
            if (animator != null && _parameters.Contains(hash)) animator.SetTrigger(hash);
        }
    }
}
