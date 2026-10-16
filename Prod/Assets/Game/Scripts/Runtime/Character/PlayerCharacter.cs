using System;
using Game.Cameras;
using Game.Character.States;
using Game.Combat;
using Game.Combat.Weapons;
using Game.Core;
using Game.Inputs;
using UnityEngine;

namespace Game.Character
{
    /// <summary>
    /// Главный компонент игрока. Паттерн Facade: собирает мотор, здоровье, бой и ввод в один
    /// понятный API для других систем (камера, HUD, подбираемые предметы, спавнер).
    /// Также владеет конечным автоматом состояний движения — все переходы описаны в BuildStateMachine().
    /// </summary>
    [DefaultExecutionOrder(0)]
    [RequireComponent(typeof(CharacterMotor), typeof(Health))]
    public sealed class PlayerCharacter : MonoBehaviour, ICameraTarget
    {
        [SerializeField] private MovementConfig config;
        [Tooltip("Высота точки вращения камеры в долях роста капсулы")]
        [SerializeField] private float cameraPivotHeight = 0.9f;
        [Tooltip("Множитель скорости поворота к прицелу (быстрее, чем к направлению бега)")]
        [SerializeField] private float combatTurnMultiplier = 2f;

        private StateMachine _stateMachine;
        private GroundedState _grounded;
        private CrouchState _crouch;
        private SlideState _slide;
        private AirborneState _airborne;
        private DeadState _dead;

        private float _jumpBuffer;
        private float _slideCooldown;
        private ICameraRig _cameraRig;

        public MovementConfig Config => config;
        public CharacterMotor Motor { get; private set; }
        public Health Health { get; private set; }
        public PlayerCombat Combat { get; private set; }
        public WeaponInventory Inventory { get; private set; }
        public IPlayerInput Input { get; private set; }

        public IState CurrentState => _stateMachine?.Current;
        public string CurrentStateName => CurrentState?.Name ?? "-";
        public bool IsSprinting { get; private set; }
        public bool IsSliding => CurrentState == _slide;
        public bool IsCrouching => CurrentState == _crouch || IsSliding;
        public bool IsDead => Health != null && !Health.IsAlive;
        public bool IsAiming => Combat != null && Combat.IsAiming;
        public bool WantsCombatFacing => Combat != null && Combat.WantsCombatFacing;
        public int AirJumpsRemaining { get; set; }
        public bool HasJumpRequest => _jumpBuffer > 0f;

        // ICameraTarget
        public Transform Root => transform;
        public Vector3 PivotPosition => transform.position + Vector3.up * (Motor.Height * cameraPivotHeight);

        public event Action Jumped;
        public event Action<float> Landed;
        public event Action Respawned;
        /// <summary>(предыдущее, новое) состояние движения.</summary>
        public event Action<IState, IState> StateChanged;

        private void Awake()
        {
            Motor = GetComponent<CharacterMotor>();
            Health = GetComponent<Health>();
            Combat = GetComponent<PlayerCombat>();
            Inventory = GetComponent<WeaponInventory>();
            Input = GetComponent<IPlayerInput>();

            if (config == null)
            {
                Debug.LogWarning("[PlayerCharacter] MovementConfig не назначен — используются значения по умолчанию.", this);
                config = ScriptableObject.CreateInstance<MovementConfig>();
            }

            BuildStateMachine();
        }

        private void OnEnable()
        {
            ServiceLocator.Register(this);
            ServiceLocator.Register<ICameraTarget>(this);
            Motor.Landed += OnMotorLanded;
        }

        private void OnDisable()
        {
            ServiceLocator.Unregister(this);
            ServiceLocator.Unregister<ICameraTarget>(this);
            Motor.Landed -= OnMotorLanded;
        }

        private void Update()
        {
            if (Input == null)
            {
                if (!ServiceLocator.TryGet(out IPlayerInput input)) return;
                Input = input;
            }

            var deltaTime = Time.deltaTime;
            if (Input.JumpPressed) _jumpBuffer = config.jumpBufferTime;
            else _jumpBuffer -= deltaTime;
            _slideCooldown -= deltaTime;

            IsSprinting = ComputeSprinting();
            _stateMachine.Tick(deltaTime);
        }

        // ---------- Конечный автомат ----------

        private void BuildStateMachine()
        {
            _grounded = new GroundedState(this);
            _crouch = new CrouchState(this);
            _slide = new SlideState(this);
            _airborne = new AirborneState(this);
            _dead = new DeadState(this);

            _stateMachine = new StateMachine();
            _stateMachine.StateChanged += (from, to) => StateChanged?.Invoke(from, to);

            bool CanStand() => Motor.CanStandUp(config.standHeight);
            bool Fell() => !Motor.IsGrounded && Motor.AirTime > config.coyoteTime;

            _stateMachine.AddAnyTransition(_dead, () => IsDead);

            _stateMachine.AddTransition(_grounded, _airborne, () => HasJumpRequest || Fell());
            _stateMachine.AddTransition(_grounded, _slide, () => Input.CrouchPressed && IsSprinting
                && Motor.PlanarVelocity.magnitude >= config.slideMinStartSpeed && _slideCooldown <= 0f);
            _stateMachine.AddTransition(_grounded, _crouch, () => Input.CrouchPressed);

            _stateMachine.AddTransition(_crouch, _airborne, () => (HasJumpRequest && CanStand()) || Fell());
            _stateMachine.AddTransition(_crouch, _grounded, () => (Input.CrouchPressed || Input.SprintHeld) && CanStand());

            _stateMachine.AddTransition(_slide, _airborne, () => (HasJumpRequest && CanStand()) || Fell());
            _stateMachine.AddTransition(_slide, _grounded, () => _slide.IsFinished && CanStand() && !Input.CrouchHeld);
            _stateMachine.AddTransition(_slide, _crouch, () => _slide.IsFinished);

            _stateMachine.AddTransition(_airborne, _grounded, () => Motor.IsGrounded && Motor.Velocity.y <= 0f);

            _stateMachine.SetState(_grounded);
        }

        // ---------- API для состояний ----------

        /// <summary>Направление ввода в мире относительно камеры (длина 0..1).</summary>
        public Vector3 GetMoveDirection()
        {
            var move = Input?.Move ?? Vector2.zero;
            var yaw = ResolveCameraRig() != null
                ? _cameraRig.YawRotation
                : Quaternion.Euler(0f, transform.eulerAngles.y, 0f);
            return Vector3.ClampMagnitude(yaw * new Vector3(move.x, 0f, move.y), 1f);
        }

        public float GetGroundSpeed()
        {
            if (IsAiming) return config.walkSpeed;
            return IsSprinting ? config.sprintSpeed : config.runSpeed;
        }

        /// <summary>В бою — лицом к камере (стрейф), иначе — по направлению движения.</summary>
        public void UpdateFacing(Vector3 moveDirection, float deltaTime)
        {
            if (WantsCombatFacing && ResolveCameraRig() != null)
                Motor.RotateTowards(_cameraRig.Forward, config.rotationSpeed * combatTurnMultiplier, deltaTime);
            else if (moveDirection.sqrMagnitude > 0.0001f)
                Motor.RotateTowards(moveDirection, config.rotationSpeed, deltaTime);
        }

        public bool TryConsumeJumpRequest()
        {
            if (!HasJumpRequest) return false;
            _jumpBuffer = 0f;
            return true;
        }

        public void PerformJump()
        {
            Motor.Jump(config.jumpHeight, config.gravity);
            Jumped?.Invoke();
        }

        public void RefillAirJumps() => AirJumpsRemaining = config.maxAirJumps;
        public void StartSlideCooldown() => _slideCooldown = config.slideCooldown;

        // ---------- Публичный API для других систем ----------

        /// <summary>Подбросить персонажа (батут, взрыв).</summary>
        public void Launch(Vector3 velocity, bool refillAirJumps = true)
        {
            Motor.Launch(velocity);
            if (refillAirJumps) RefillAirJumps();
        }

        public void Respawn(Vector3 position, Quaternion rotation)
        {
            Motor.Teleport(position, rotation);
            Health.Revive();
            _stateMachine.SetState(_grounded);
            Respawned?.Invoke();
        }

        private bool ComputeSprinting()
        {
            var canSprintInState = CurrentState == _grounded;
            return canSprintInState && Input.SprintHeld && Input.Move.sqrMagnitude > 0.01f
                   && !Input.AimHeld && !Input.FireHeld;
        }

        private ICameraRig ResolveCameraRig()
        {
            if (_cameraRig == null || _cameraRig is UnityEngine.Object unityObject && unityObject == null)
                ServiceLocator.TryGet(out _cameraRig);
            return _cameraRig;
        }

        private void OnMotorLanded(float verticalSpeed) => Landed?.Invoke(verticalSpeed);
    }
}
