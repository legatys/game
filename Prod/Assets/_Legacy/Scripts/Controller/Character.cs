using UnityEngine;

public abstract class Character : MonoBehaviour
{
    [SerializeField] protected GameSettings _settings; 
    [SerializeField] private Transform _cameraTransform;

    [Header ("Ground Check")]
    [SerializeField] private Transform _GroundCheck;
    [SerializeField] private float _GroundDistance = 0.2f;
    [SerializeField] private LayerMask _GroundMask;
    protected bool _IsGrounded; 

    protected Rigidbody _rb;
    protected float _speed;
    protected IBaseMovement _currentStrategy = new IdleMove();

    private MouseLook _mouseLook;
    protected JumpState _jumpState;
    private IBaseMovement _jumpMove;
    protected Animator _animator; 
    protected virtual void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _speed = _settings.acceleration ;
        _mouseLook = new MouseLook(_settings);

        _jumpState = new JumpState { maxJumps = _settings.max_jump };
        _jumpState.ResetJumps();
        _jumpMove = new JumpMove(_jumpState, _settings.jumpForce);

        Cursor.lockState = CursorLockMode.Locked;
        _animator = GetComponentInChildren<Animator>(); 
    }

    private void OnCollisionEnter(Collision collision)
    {
        JumpableSurface surface = collision.gameObject.GetComponent<JumpableSurface>();
        if (surface != null)
        {
            _jumpState.maxJumps = surface.jumpsGranted;
            _jumpState.ResetJumps();
        }
    }

    protected virtual void Update()
    {
        _IsGrounded = Physics.CheckSphere(_GroundCheck.position, _GroundDistance,_GroundMask);
        ReadInput();
        _mouseLook.Look(transform, _cameraTransform);
    }

    void FixedUpdate()
    {
        _currentStrategy.Execute(_rb, _speed);
    }

    protected void SetStrategy(IBaseMovement strategy)
    {
        _currentStrategy = strategy;
    }

    protected void TryJump()
    {
        _jumpMove.Execute(_rb, 0f); 
    }

    protected abstract void ReadInput();
}