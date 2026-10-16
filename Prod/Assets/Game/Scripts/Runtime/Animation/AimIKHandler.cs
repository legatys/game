using Game.Combat;
using Game.Combat.Weapons;
using UnityEngine;

namespace Game.Animations
{
    /// <summary>
    /// Humanoid IK взгляда: голова и корпус поворачиваются к точке прицела.
    /// Должен висеть на том же объекте, что и Animator (OnAnimatorIK вызывается только там),
    /// а в контроллере у базового слоя должен быть включён IK Pass (генератор делает это сам).
    /// </summary>
    [RequireComponent(typeof(Animator))]
    public sealed class AimIKHandler : MonoBehaviour
    {
        /// <summary>Вес взгляда в прицел, когда персонаж целится или стреляет.</summary>
        [SerializeField, Range(0f, 1f)] private float combatWeight = 1f;
        /// <summary>Вес взгляда вне боя (голова следит за камерой).</summary>
        [SerializeField, Range(0f, 1f)] private float idleWeight = 0.4f;
        /// <summary>Насколько поворачивается корпус.</summary>
        [SerializeField, Range(0f, 1f)] private float bodyWeight = 0.35f;
        /// <summary>Насколько поворачивается голова.</summary>
        [SerializeField, Range(0f, 1f)] private float headWeight = 0.9f;
        /// <summary>Ограничение угла поворота (0 — без ограничений, 1 — не поворачивать).</summary>
        [SerializeField, Range(0f, 1f)] private float clampWeight = 0.5f;
        /// <summary>Скорость изменения веса IK в секунду.</summary>
        [SerializeField] private float blendSpeed = 4f;

        private Animator _animator;
        private IAimProvider _aim;
        private Health _health;
        private float _weight;

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            _aim = GetComponentInParent<IAimProvider>();
            _health = GetComponentInParent<Health>();
        }

        private void OnAnimatorIK(int layerIndex)
        {
            if (layerIndex != 0 || _aim == null) return;

            var alive = _health == null || _health.IsAlive;
            var target = !alive ? 0f : _aim.IsCombatReady ? combatWeight : idleWeight;
            _weight = Mathf.MoveTowards(_weight, target, blendSpeed * Time.deltaTime);

            _animator.SetLookAtWeight(_weight, bodyWeight, headWeight, 0f, clampWeight);
            _animator.SetLookAtPosition(_aim.AimPoint);
        }
    }
}
