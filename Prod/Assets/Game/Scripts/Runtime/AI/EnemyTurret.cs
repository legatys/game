using Game.AI.States;
using Game.Character;
using Game.Combat;
using Game.Combat.Weapons;
using Game.Core;
using UnityEngine;

namespace Game.AI
{
    /// <summary>
    /// Простой враг-турель: ищет игрока, поворачивается к нему и стреляет очередями.
    /// Показывает переиспользование систем: тот же <see cref="StateMachine"/>, что у игрока,
    /// то же оружие через <see cref="WeaponFactory"/>, тот же <see cref="Health"/>.
    /// </summary>
    [RequireComponent(typeof(Health))]
    public sealed class EnemyTurret : MonoBehaviour
    {
        /// <summary>Часть, поворачивающаяся по горизонтали.</summary>
        [Header("Части модели")]
        [SerializeField] private Transform yawPivot;
        /// <summary>Часть, наклоняющаяся по вертикали (внутри yawPivot).</summary>
        [SerializeField] private Transform pitchPivot;
        /// <summary>Куда крепится модель оружия.</summary>
        [SerializeField] private Transform weaponSocket;

        /// <summary>Оружие турели.</summary>
        [Header("Оружие")]
        [SerializeField] private WeaponDefinition weapon;
        [Tooltip("Дополнительный разброс ИИ, чтобы он не был снайпером")]
        [SerializeField] private float extraSpread = 1.5f;

        /// <summary>Дальность обнаружения игрока, м.</summary>
        [Header("Поведение")]
        [SerializeField] private float detectionRange = 30f;
        /// <summary>Скорость поворота башни, градусов в секунду.</summary>
        [SerializeField] private float turnSpeed = 90f;
        /// <summary>Стрелять, только если ствол отклонён от цели меньше чем на столько градусов.</summary>
        [SerializeField] private float fireAngleTolerance = 4f;
        /// <summary>Скорость вращения при поиске цели, градусов в секунду.</summary>
        [SerializeField] private float idleScanSpeed = 25f;
        /// <summary>Через сколько секунд уничтоженная турель восстанавливается.</summary>
        [SerializeField] private float respawnDelay = 6f;
        /// <summary>Слои, которые перекрывают обзор.</summary>
        [SerializeField] private LayerMask sightMask = ~0;

        private StateMachine _stateMachine;
        private Health _health;
        private Weapon _weapon;
        private PlayerCharacter _target;

        /// <summary>Здоровье турели.</summary>
        public Health Health => _health;
        /// <summary>Экземпляр оружия турели (бесконечные патроны).</summary>
        public Weapon Weapon => _weapon;
        /// <summary>Скорость вращения при поиске цели.</summary>
        public float IdleScanSpeed => idleScanSpeed;
        /// <summary>Задержка восстановления после уничтожения.</summary>
        public float RespawnDelay => respawnDelay;
        /// <summary>Имя текущего состояния ИИ (для отладки).</summary>
        public string CurrentStateName => _stateMachine?.Current?.Name ?? "-";

        private void Awake()
        {
            _health = GetComponent<Health>();
            if (yawPivot == null) yawPivot = transform;
            if (pitchPivot == null) pitchPivot = yawPivot;
            if (weaponSocket == null) weaponSocket = pitchPivot;

            if (weapon != null)
            {
                _weapon = WeaponFactory.Create(weapon, weaponSocket);
                _weapon.InfiniteReserve = true;
                _weapon.Draw();
            }

            BuildStateMachine();
        }

        /// <summary>Настройка из кода. Вызывать до активации объекта (до Awake).</summary>
        public void Configure(Transform yaw, Transform pitch, Transform socket, WeaponDefinition weaponDefinition)
        {
            yawPivot = yaw;
            pitchPivot = pitch;
            weaponSocket = socket;
            weapon = weaponDefinition;
        }

        private void BuildStateMachine()
        {
            var idle = new TurretIdleState(this);
            var attack = new TurretAttackState(this);
            var dead = new TurretDeadState(this);

            _stateMachine = new StateMachine();
            _stateMachine.AddAnyTransition(dead, () => !_health.IsAlive);
            _stateMachine.AddTransition(idle, attack, CanSeeTarget);
            _stateMachine.AddTransition(attack, idle, () => !CanSeeTarget());
            _stateMachine.AddTransition(dead, idle, () => _health.IsAlive);
            _stateMachine.SetState(idle);
        }

        private void Update() => _stateMachine.Tick(Time.deltaTime);

        // ---------- API для состояний ----------

        /// <summary>Игрок жив, в радиусе и не закрыт препятствием.</summary>
        public bool CanSeeTarget()
        {
            if (!TryGetTarget(out var target)) return false;

            var origin = pitchPivot.position;
            var toTarget = target.PivotPosition - origin;
            if (toTarget.magnitude > detectionRange) return false;

            // Видим, если первым на линии оказался сам игрок.
            return PhysicsQuery.Raycast(new Ray(origin, toTarget.normalized), detectionRange, sightMask, transform, out var hit)
                   && hit.transform.IsChildOf(target.Root);
        }

        /// <summary>Точка, в которую целиться (уровень плеч игрока).</summary>
        public Vector3 TargetPoint => TryGetTarget(out var target) ? target.PivotPosition : pitchPivot.position + pitchPivot.forward;

        /// <summary>Повернуть башню к точке. Возвращает угол между стволом и целью.</summary>
        public float AimAt(Vector3 point, float deltaTime)
        {
            var direction = point - pitchPivot.position;
            var flat = Vector3.ProjectOnPlane(direction, Vector3.up);
            if (flat.sqrMagnitude > 0.0001f)
                yawPivot.rotation = Quaternion.RotateTowards(yawPivot.rotation, Quaternion.LookRotation(flat), turnSpeed * deltaTime);

            if (pitchPivot != yawPivot && direction.sqrMagnitude > 0.0001f)
            {
                var local = yawPivot.InverseTransformDirection(direction);
                var pitch = -Mathf.Atan2(local.y, new Vector2(local.x, local.z).magnitude) * Mathf.Rad2Deg;
                var targetRotation = Quaternion.Euler(pitch, 0f, 0f);
                pitchPivot.localRotation = Quaternion.RotateTowards(pitchPivot.localRotation, targetRotation, turnSpeed * deltaTime);
            }

            return Vector3.Angle(pitchPivot.forward, direction);
        }

        /// <summary>Медленно вращает башню в поиске цели.</summary>
        public void Scan(float deltaTime) => yawPivot.Rotate(0f, idleScanSpeed * deltaTime, 0f, Space.World);

        /// <summary>Тик оружия: trigger — держать ли курок.</summary>
        public void TickWeapon(bool trigger, float deltaTime, float aimError)
        {
            if (_weapon == null) return;

            var shouldFire = trigger && aimError <= fireAngleTolerance;
            var aim = new AimData(TargetPoint, gameObject, false, 1f + extraSpread);
            _weapon.Tick(deltaTime, shouldFire, shouldFire, aim);
        }

        /// <summary>Показать/скрыть турель вместе с коллайдерами (мёртвую нельзя "добивать").</summary>
        public void SetVisible(bool visible)
        {
            foreach (var renderer in GetComponentsInChildren<Renderer>(true)) renderer.enabled = visible;
            foreach (var collider in GetComponentsInChildren<Collider>(true)) collider.enabled = visible;
        }

        private bool TryGetTarget(out PlayerCharacter target)
        {
            if (_target == null) ServiceLocator.TryGet(out _target);

            target = _target;
            return target != null && !target.IsDead;
        }
    }
}
