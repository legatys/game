using Game.Core;
using Game.Inputs;
using UnityEngine;

namespace Game.Cameras
{
    /// <summary>
    /// Камера от третьего лица "через плечо": орбита вокруг цели, приближение при прицеливании,
    /// смена плеча, отдача, отталкивание от стен (SphereCast). Вешается на Main Camera.
    /// Регистрируется в ServiceLocator как <see cref="ICameraRig"/>.
    /// </summary>
    [DefaultExecutionOrder(100)]
    [RequireComponent(typeof(UnityEngine.Camera))]
    public sealed class ThirdPersonCamera : MonoBehaviour, ICameraRig
    {
        [SerializeField] private CameraConfig config;

        private UnityEngine.Camera _camera;
        private ICameraTarget _target;
        private IPlayerInput _input;

        private float _yaw;
        private float _pitch;
        private float _shoulderSide = 1f;
        private float _currentSide = 1f;
        private float _aimBlend;
        private float _currentDistance;
        private Vector3 _smoothedPivot;
        private Vector3 _pivotVelocity;
        private Vector2 _recoil;        // текущее смещение от отдачи (x — pitch, y — yaw)
        private Vector2 _pendingRecoil; // отдача, которую ещё нужно "докинуть"

        public UnityEngine.Camera Camera => _camera;
        public Quaternion YawRotation => Quaternion.Euler(0f, _yaw, 0f);
        public Vector3 Forward => transform.forward;

        private void Awake()
        {
            _camera = GetComponent<UnityEngine.Camera>();
            if (config == null)
            {
                Debug.LogWarning("[ThirdPersonCamera] CameraConfig не назначен — используются значения по умолчанию.", this);
                config = ScriptableObject.CreateInstance<CameraConfig>();
            }

            var euler = transform.eulerAngles;
            _yaw = euler.y;
            _pitch = NormalizeAngle(euler.x);
            _currentDistance = config.distance;
            ServiceLocator.Register<ICameraRig>(this);
        }

        private void OnDestroy() => ServiceLocator.Unregister<ICameraRig>(this);

        public void SetTarget(ICameraTarget target)
        {
            _target = target;
            if (target == null) return;

            _smoothedPivot = target.PivotPosition;
            _pivotVelocity = Vector3.zero;
            _yaw = target.Root.eulerAngles.y;
        }

        public void AddRecoil(float pitchDegrees, float yawDegrees) => _pendingRecoil += new Vector2(pitchDegrees, yawDegrees);

        public Vector3 GetAimPoint(float maxDistance, Transform ignoreRoot)
        {
            var ray = new Ray(transform.position, transform.forward);
            return PhysicsQuery.Raycast(ray, maxDistance, config.collisionMask, ignoreRoot, out var hit)
                ? hit.point
                : ray.GetPoint(maxDistance);
        }

        private void LateUpdate()
        {
            if (_input == null) ServiceLocator.TryGet(out _input);
            if (!IsTargetAlive())
            {
                if (ServiceLocator.TryGet(out ICameraTarget found)) SetTarget(found);
                else return;
            }

            var deltaTime = Time.deltaTime;
            ReadInput(deltaTime);
            UpdateRecoil(deltaTime);

            _aimBlend = Mathf.MoveTowards(_aimBlend, _target.IsAiming ? 1f : 0f, config.aimTransitionSpeed * deltaTime);
            _currentSide = Mathf.MoveTowards(_currentSide, _shoulderSide, config.shoulderSwitchSpeed * deltaTime);
            _smoothedPivot = Vector3.SmoothDamp(_smoothedPivot, _target.PivotPosition, ref _pivotVelocity, config.followSmoothTime);

            var rotation = Quaternion.Euler(_pitch - _recoil.x, _yaw + _recoil.y, 0f);
            var distance = Mathf.Lerp(config.distance, config.aimDistance, _aimBlend);
            var side = Mathf.Lerp(config.shoulderOffset, config.aimShoulderOffset, _aimBlend) * _currentSide;
            var offset = rotation * new Vector3(side, config.heightOffset, -distance);

            transform.SetPositionAndRotation(ResolveCollision(_smoothedPivot, offset, deltaTime), rotation);
            _camera.fieldOfView = Mathf.Lerp(config.fieldOfView, config.aimFieldOfView, _aimBlend);
        }

        private void ReadInput(float deltaTime)
        {
            if (_input == null) return;

            var look = _input.Look;
            var sensitivity = _input.LookIsDelta ? config.mouseSensitivity : config.gamepadSensitivity * deltaTime;
            sensitivity *= Mathf.Lerp(1f, config.aimSensitivityMultiplier, _aimBlend);

            _yaw += look.x * sensitivity;
            _pitch += look.y * sensitivity * (config.invertY ? 1f : -1f);
            _pitch = Mathf.Clamp(_pitch, config.minPitch, config.maxPitch);

            if (_input.SwitchShoulderPressed) _shoulderSide = -_shoulderSide;
        }

        private void UpdateRecoil(float deltaTime)
        {
            // Быстро "докидываем" отдачу, затем плавно возвращаем прицел.
            var kick = Vector2.MoveTowards(Vector2.zero, _pendingRecoil, config.recoilKickSpeed * deltaTime);
            _pendingRecoil -= kick;
            _recoil += kick;
            _recoil = Vector2.MoveTowards(_recoil, Vector2.zero, config.recoilRecovery * deltaTime);
        }

        private Vector3 ResolveCollision(Vector3 pivot, Vector3 offset, float deltaTime)
        {
            var wanted = offset.magnitude;
            var direction = offset / Mathf.Max(wanted, 0.0001f);
            var allowed = wanted;

            if (PhysicsQuery.SphereCast(new Ray(pivot, direction), config.collisionRadius, wanted,
                    config.collisionMask, _target.Root, out var hit))
                allowed = Mathf.Max(0.1f, hit.distance);

            // Приближаемся мгновенно (чтобы не заглянуть в стену), отдаляемся плавно.
            _currentDistance = allowed < _currentDistance
                ? allowed
                : Mathf.MoveTowards(_currentDistance, allowed, config.distanceRecoverSpeed * deltaTime);

            return pivot + direction * _currentDistance;
        }

        // Цель может быть уничтожена (Unity-объект), а ссылка на интерфейс останется.
        private bool IsTargetAlive() => _target != null && !(_target is Object unityObject && unityObject == null);

        private static float NormalizeAngle(float angle) => angle > 180f ? angle - 360f : angle;
    }
}
