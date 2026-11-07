using System;
using NUnit.Framework;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    
    private CharacterController controller;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] private float speed = 5f;
    [SerializeField] private float validate_vec = 0.005f;
    [SerializeField] private float rotation_speed =  10f;

    [SerializeField] private float go_step = 1f;
    [SerializeField] private float gravity = -9.8f;
    [SerializeField] private float jump_height = 1f;    
    private float ver_vel ; 
    void Awake()
    {
        controller = GetComponent<CharacterController>();
    }
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if(Keyboard.current == null)
            return;

        float x = 0f;
        if(Keyboard.current.dKey.isPressed)
            x+= go_step;
        if(Keyboard.current.aKey.isPressed)
            x-=go_step;
        
        float z = 0f;
        if(Keyboard.current.wKey.isPressed)
            z+=go_step;
        if(Keyboard.current.sKey.isPressed)
            z-=go_step;
        
        
        UnityEngine.Vector3 dir = new UnityEngine.Vector3(x, 0f,z).normalized;
        
        bool is_ground = controller.isGrounded; 
        
        if ( is_ground && Keyboard.current.spaceKey.wasPressedThisFrame)
            ver_vel = (float)Math.Sqrt(jump_height * -2f* gravity);

        ver_vel += gravity* Time.deltaTime;

        UnityEngine.Vector3 vel = dir * speed;
        vel.y = ver_vel;

        controller.Move( vel * Time.deltaTime);
        if (dir.sqrMagnitude > validate_vec)
        {
            UnityEngine.Quaternion rotation_data = UnityEngine.Quaternion.LookRotation(dir);
            transform.rotation = UnityEngine.Quaternion.Slerp(transform.rotation, rotation_data, rotation_speed* Time.deltaTime);
        }

        
    }
}
