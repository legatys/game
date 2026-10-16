using UnityEngine;

public class WalkMove : IBaseMovement
{
    private Vector3 _direction;
     private float _maxSpeed = 7f;
    public WalkMove(Vector3 direction)
    {
        _direction = direction.normalized; 
    }

    public void Execute(Rigidbody rb, float speed)
     {
        rb.AddForce(_direction * speed);

  
        Vector3 flatVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

        if (flatVelocity.magnitude > _maxSpeed)
        {
            
            Vector3 limitedVelocity = flatVelocity.normalized * _maxSpeed;

            rb.linearVelocity = new Vector3(limitedVelocity.x, rb.linearVelocity.y, limitedVelocity.z);
        }
    }
}

public class JumpMove : IBaseMovement
{
    private readonly JumpState _jumpState;
    private readonly float _jumpForce;

    public JumpMove(JumpState jumpState, float jumpForce)
    {
        _jumpState = jumpState;
        _jumpForce = jumpForce;
    }

    public void Execute(Rigidbody rb, float speed)
    {
        if (_jumpState.TryConsumeJump())
        {
            rb.AddForce(Vector3.up * _jumpForce, ForceMode.Impulse);
        }
    }
}

public class IdleMove : IBaseMovement
{
    public void Execute(Rigidbody rb, float speed)
    {
        rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
    }
}
public class AirMove : IBaseMovement
{
    public void Execute(Rigidbody rb, float speed)
    {

    }
}
