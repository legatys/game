using UnityEngine;
using UnityEngine.InputSystem;

public class MouseLook
{
    private readonly GameSettings _settings;
    private float _pitch;

    public MouseLook(GameSettings settings)
    {
        _settings = settings;
    }

    public void Look(Transform bodyTransform, Transform cameraTransform)
    {
        if (Mouse.current == null) return;

        Vector2 delta = Mouse.current.delta.ReadValue() * _settings.mouseSensitivity;


        bodyTransform.Rotate(0f, delta.x, 0f);


        _pitch -= delta.y;
        _pitch = Mathf.Clamp(_pitch, _settings.minPitch, _settings.maxPitch);
        cameraTransform.localRotation = Quaternion.Euler(_pitch, 0f, 0f);
    }
}