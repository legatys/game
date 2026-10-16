using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class ConCharapter : Character
{
    private List<(Key key, IBaseMovement strategy)> _keyBindings;
    private IBaseMovement _idle = new IdleMove();
    private IBaseMovement _airMove = new AirMove();

     protected override void ReadInput()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        if (keyboard[Key.Space].wasPressedThisFrame)
        {
            TryJump();
        }

        if (!_IsGrounded)
        {
            SetStrategy(_airMove);
            if (_animator != null) _animator.SetBool("IsRunning", false);
            return;
        }

        Vector3 moveDir = Vector3.zero; 

        if (keyboard[Key.W].isPressed) moveDir += transform.forward;
        if (keyboard[Key.S].isPressed) moveDir -= transform.forward;
        if (keyboard[Key.D].isPressed) moveDir += transform.right;
        if (keyboard[Key.A].isPressed) moveDir -= transform.right;

        if (moveDir.magnitude > 0)
        {
            SetStrategy(new WalkMove(moveDir));
            
            if (_animator != null) _animator.SetBool("IsRunning", true);
        }
        else
        {
            SetStrategy(_idle);
            
            if (_animator != null) _animator.SetBool("IsRunning", false);
        }
    }
}